-- ============================================================
-- Migration: NIS Project running number + Ticket code
-- Feature: NIS-Backend — ProjectNo (NisProject) + TicketCode (NisTicket)
-- Date: 2026-07-06
-- Author: Claude (backend-agent)
-- ============================================================
-- Run this script on GoAlongDatabase, AFTER add_nis_project_tables.sql.
-- Safe to re-run: all ALTER/UPDATE blocks are guarded.
-- Rollback section is at the bottom of this file.
--
-- What this does:
--   1. Adds NisProject.ProjectNo (INT NULL) — sequential running number per CmpId.
--   2. Backfills ProjectNo for any existing rows (ordered by CreatedDate per CmpId).
--   3. Adds a UNIQUE index on (CmpId, ProjectNo) — safe only after backfill (no NULLs left).
--   4. Adds NisTicket.TicketCode (NVARCHAR(50) NULL) — e.g. "TK-BK-0007-01".
--      TicketCode is NOT backfilled for existing tickets (historical tickets keep
--      TicketCode = NULL; only newly created tickets get a code going forward).
--
-- NOTE: GO batch separators are required here — ALTER TABLE ADD COLUMN runs inside
-- an IF block, and SQL Server binds/compiles a whole batch before executing it, so a
-- later statement in the SAME batch can't reference a column added earlier in that
-- same batch. Each step below runs in its own batch.
--
-- No explicit BEGIN/COMMIT TRANSACTION wrapper: every step is independently guarded
-- (IF EXISTS / IF NOT EXISTS) and safe to re-run, so a mid-script failure just means
-- re-running the whole file picks up from where it left off — no partial-transaction
-- rollback surprises across GO batches.
-- ============================================================

-- ─── NisProject.ProjectNo ────────────────────────────────────────────────────
-- Self-healing: if the column doesn't exist, add it as INT. If it already exists
-- but was created with the wrong type (e.g. a manual/partial fix left it as
-- varchar), drop the dependent unique index and convert the column to INT.
-- Any pre-existing values are NOT real data (this column didn't exist as a
-- business concept before this feature), so they're wiped rather than preserved —
-- the backfill step below assigns clean, guaranteed-unique numbers from scratch.

DECLARE @ProjectNoType nvarchar(128);
SELECT @ProjectNoType = t.name
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.NisProject') AND c.name = 'ProjectNo';

IF @ProjectNoType IS NULL
BEGIN
    ALTER TABLE [dbo].[NisProject] ADD [ProjectNo] INT NULL;
    PRINT 'Added column NisProject.ProjectNo (int)';
END
ELSE IF @ProjectNoType <> 'int'
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'UX_NisProject_CmpId_ProjectNo' AND object_id = OBJECT_ID('dbo.NisProject')
    )
    BEGIN
        DROP INDEX [UX_NisProject_CmpId_ProjectNo] ON [dbo].[NisProject];
        PRINT 'Dropped UX_NisProject_CmpId_ProjectNo before converting column type';
    END

    UPDATE [dbo].[NisProject] SET [ProjectNo] = NULL;

    ALTER TABLE [dbo].[NisProject] ALTER COLUMN [ProjectNo] INT NULL;
    PRINT 'Converted NisProject.ProjectNo from ' + @ProjectNoType + ' to int (existing values reset)';
END
ELSE
    PRINT 'Column NisProject.ProjectNo already exists as int — skipped';
GO

-- Safety net: clear duplicate (CmpId, ProjectNo) pairs left over from any previous
-- broken attempt — keep the earliest-created row's number, null the rest so the
-- backfill step below can re-assign them.
;WITH Ranked AS (
    SELECT
        ProjectId,
        ROW_NUMBER() OVER (PARTITION BY CmpId, ProjectNo ORDER BY CreatedDate, ProjectId) AS DupRank
    FROM [dbo].[NisProject]
    WHERE ProjectNo IS NOT NULL
)
UPDATE p
SET p.ProjectNo = NULL
FROM [dbo].[NisProject] p
INNER JOIN Ranked r ON r.ProjectId = p.ProjectId
WHERE r.DupRank > 1;

PRINT 'Cleared duplicate NisProject.ProjectNo values (if any)';
GO

-- Backfill: assign sequential ProjectNo per CmpId for any row missing it, continuing
-- from that CmpId's current max (so it's safe even if some rows already have a
-- legitimate number and only newer rows are NULL).
;WITH ExistingMax AS (
    SELECT CmpId, MAX(ProjectNo) AS MaxNo
    FROM [dbo].[NisProject]
    WHERE ProjectNo IS NOT NULL
    GROUP BY CmpId
),
Numbered AS (
    SELECT
        p.ProjectId,
        p.CmpId,
        ROW_NUMBER() OVER (PARTITION BY p.CmpId ORDER BY p.CreatedDate, p.ProjectId) AS RowNo
    FROM [dbo].[NisProject] p
    WHERE p.ProjectNo IS NULL
)
UPDATE p
SET p.ProjectNo = n.RowNo + ISNULL(m.MaxNo, 0)
FROM [dbo].[NisProject] p
INNER JOIN Numbered n ON n.ProjectId = p.ProjectId
LEFT JOIN ExistingMax m ON m.CmpId = n.CmpId;

PRINT 'Backfilled NisProject.ProjectNo for existing rows (if any)';
GO

-- Unique index — only add once every row has a ProjectNo (guaranteed by the backfill above).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_NisProject_CmpId_ProjectNo' AND object_id = OBJECT_ID('dbo.NisProject')
)
BEGIN
    CREATE UNIQUE INDEX [UX_NisProject_CmpId_ProjectNo] ON [dbo].[NisProject] ([CmpId], [ProjectNo]);
    PRINT 'Created unique index UX_NisProject_CmpId_ProjectNo';
END
ELSE
    PRINT 'Index UX_NisProject_CmpId_ProjectNo already exists — skipped';
GO

-- ─── NisTicket.TicketCode ────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.NisTicket') AND name = 'TicketCode'
)
BEGIN
    ALTER TABLE [dbo].[NisTicket] ADD [TicketCode] NVARCHAR(50) NULL;
    PRINT 'Added column NisTicket.TicketCode';
END
ELSE
    PRINT 'Column NisTicket.TicketCode already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_NisTicket_TicketCode' AND object_id = OBJECT_ID('dbo.NisTicket')
)
BEGIN
    CREATE INDEX [IX_NisTicket_TicketCode] ON [dbo].[NisTicket] ([TicketCode]);
    PRINT 'Created index IX_NisTicket_TicketCode';
END
ELSE
    PRINT 'Index IX_NisTicket_TicketCode already exists — skipped';
GO

PRINT '=== Migration add_nis_ticket_code complete ===';
GO

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NisTicket_TicketCode' AND object_id = OBJECT_ID('dbo.NisTicket'))
    DROP INDEX [IX_NisTicket_TicketCode] ON [dbo].[NisTicket];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NisTicket') AND name = 'TicketCode')
    ALTER TABLE [dbo].[NisTicket] DROP COLUMN [TicketCode];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_NisProject_CmpId_ProjectNo' AND object_id = OBJECT_ID('dbo.NisProject'))
    DROP INDEX [UX_NisProject_CmpId_ProjectNo] ON [dbo].[NisProject];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NisProject') AND name = 'ProjectNo')
    ALTER TABLE [dbo].[NisProject] DROP COLUMN [ProjectNo];
GO

PRINT 'Rolled back add_nis_ticket_code';
GO
*/
