-- ============================================================
-- Migration: NIS Onsite Form support fields
-- Feature: NIS-Backend — SkipSignature/RequireCloseApproval (ServiceTicket)
--          + SrNumber/SignatureImageBase64/WorkPhotosJson (ServiceTicketSubTaskAction)
-- Date: 2026-07-06
-- Author: Claude (backend-agent)
-- ============================================================
-- Run this script on GoAlongDatabase, AFTER add_serviceticket_master_tables.sql.
-- Safe to re-run: every ALTER is guarded by an IF NOT EXISTS column check.
-- No explicit BEGIN/COMMIT TRANSACTION wrapper — each step is independently
-- idempotent, so a mid-script failure just means re-running the file.
-- Rollback section is at the bottom of this file.
-- ============================================================

-- ─── ServiceTicket.SkipSignature ─────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicket') AND name = 'SkipSignature'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicket] ADD [SkipSignature] BIT NOT NULL CONSTRAINT DF_ServiceTicket_SkipSignature DEFAULT (0);
    PRINT 'Added column ServiceTicket.SkipSignature';
END
ELSE
    PRINT 'Column ServiceTicket.SkipSignature already exists — skipped';
GO

-- ─── ServiceTicket.RequireCloseApproval ──────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicket') AND name = 'RequireCloseApproval'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicket] ADD [RequireCloseApproval] BIT NOT NULL CONSTRAINT DF_ServiceTicket_RequireCloseApproval DEFAULT (0);
    PRINT 'Added column ServiceTicket.RequireCloseApproval';
END
ELSE
    PRINT 'Column ServiceTicket.RequireCloseApproval already exists — skipped';
GO

-- ─── ServiceTicketSubTaskAction.SrNumber ─────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'SrNumber'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [SrNumber] NVARCHAR(50) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.SrNumber';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.SrNumber already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ServiceTicketSubTaskAction_SrNumber' AND object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
)
BEGIN
    CREATE INDEX [IX_ServiceTicketSubTaskAction_SrNumber] ON [dbo].[ServiceTicketSubTaskAction] ([SrNumber]);
    PRINT 'Created index IX_ServiceTicketSubTaskAction_SrNumber';
END
ELSE
    PRINT 'Index IX_ServiceTicketSubTaskAction_SrNumber already exists — skipped';
GO

-- ─── ServiceTicketSubTaskAction.SignatureImageBase64 ─────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'SignatureImageBase64'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [SignatureImageBase64] NVARCHAR(MAX) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.SignatureImageBase64';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.SignatureImageBase64 already exists — skipped';
GO

-- ─── ServiceTicketSubTaskAction.WorkPhotosJson ───────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'WorkPhotosJson'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [WorkPhotosJson] NVARCHAR(MAX) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.WorkPhotosJson';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.WorkPhotosJson already exists — skipped';
GO

PRINT '=== Migration add_serviceticket_onsite_fields complete ===';
GO

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'WorkPhotosJson')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [WorkPhotosJson];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'SignatureImageBase64')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [SignatureImageBase64];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ServiceTicketSubTaskAction_SrNumber' AND object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction'))
    DROP INDEX [IX_ServiceTicketSubTaskAction_SrNumber] ON [dbo].[ServiceTicketSubTaskAction];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'SrNumber')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [SrNumber];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicket') AND name = 'RequireCloseApproval')
    ALTER TABLE [dbo].[ServiceTicket] DROP COLUMN [RequireCloseApproval];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicket') AND name = 'SkipSignature')
    ALTER TABLE [dbo].[ServiceTicket] DROP COLUMN [SkipSignature];
GO

PRINT 'Rolled back add_serviceticket_onsite_fields';
GO
*/
