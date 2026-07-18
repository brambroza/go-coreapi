-- ============================================================
-- Migration: NIS Service Project Portal Tables
-- Feature: NIS-Backend — NIS Project, Ticket, SalesOrder
-- Date: 2026-06-26
-- Author: Claude (backend-agent)
-- ============================================================
-- Run this script on GoAlongDatabase.
-- Safe to re-run: all CREATE TABLE blocks are guarded with IF NOT EXISTS.
-- Rollback section is at the bottom of this file.
-- ============================================================

BEGIN TRANSACTION;

-- ─── NisProject ──────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisProject' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[NisProject] (
        [ProjectId]        NVARCHAR(50)    NOT NULL PRIMARY KEY,
        [Name]             NVARCHAR(200)   NOT NULL,
        [Customer]         NVARCHAR(200)   NOT NULL DEFAULT '',
        -- Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
        [Type]             NVARCHAR(50)    NOT NULL DEFAULT 'Implement',
        -- High | Medium | Low
        [Priority]         NVARCHAR(20)    NOT NULL DEFAULT 'Medium',
        [Progress]         INT             NOT NULL DEFAULT 0,
        [Status]           NVARCHAR(50)    NOT NULL DEFAULT 'Active',
        [StartDate]        DATETIME        NULL,
        [EndDate]          DATETIME        NULL,
        [Staff]            NVARCHAR(200)   NOT NULL DEFAULT '',
        [SoRef]            NVARCHAR(100)   NOT NULL DEFAULT '',
        -- Pipe-delimited tag list e.g. "Firewall|Network|WiFi"
        [TagsRaw]          NVARCHAR(1000)  NOT NULL DEFAULT '',
        -- Contact fields (flat)
        [ContactName]      NVARCHAR(200)   NULL,
        [ContactPhone]     NVARCHAR(50)    NULL,
        [ContactEmail]     NVARCHAR(200)   NULL,
        -- SalesPM fields (flat)
        [SalesPMName]      NVARCHAR(200)   NULL,
        [SalesPMNickname]  NVARCHAR(100)   NULL,
        [SalesPMPhone]     NVARCHAR(50)    NULL,
        [SalesPMRole]      NVARCHAR(100)   NULL,
        -- Engineer fields (flat)
        [EngineerName]     NVARCHAR(200)   NULL,
        [EngineerNickname] NVARCHAR(100)   NULL,
        [EngineerPhone]    NVARCHAR(50)    NULL,
        -- Location & tenant
        [Location]         NVARCHAR(500)   NULL,
        [CmpId]            NVARCHAR(50)    NOT NULL DEFAULT '',
        [CreatedBy]        NVARCHAR(100)   NOT NULL DEFAULT '',
        [UpdatedBy]        NVARCHAR(100)   NULL,
        [CreatedDate]      DATETIME        NOT NULL DEFAULT GETDATE(),
        [UpdatedDate]      DATETIME        NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX [IX_NisProject_CmpId]        ON [dbo].[NisProject] ([CmpId]);
    CREATE INDEX [IX_NisProject_CmpId_Status] ON [dbo].[NisProject] ([CmpId], [Status]);

    PRINT 'Created table NisProject';
END
ELSE
    PRINT 'Table NisProject already exists — skipped';

-- ─── NisTicket ────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisTicket' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[NisTicket] (
        [TicketId]    NVARCHAR(50)    NOT NULL PRIMARY KEY,
        [ProjectId]   NVARCHAR(50)    NOT NULL,
        [Title]       NVARCHAR(500)   NOT NULL,
        -- Open | In Progress | Pending | Done | Closed | Scheduled
        [Status]      NVARCHAR(50)    NOT NULL DEFAULT 'Open',
        [Assignee]    NVARCHAR(200)   NOT NULL DEFAULT '-',
        [StartDate]   DATETIME        NULL,
        [EndDate]     DATETIME        NULL,
        [Due]         DATETIME        NULL,
        [Pct]         INT             NOT NULL DEFAULT 0,
        -- Install | PM | MA Onsite | Support | Backup | Report | Delivery | MA
        [Type]        NVARCHAR(50)    NULL,
        -- High | Medium | Low
        [Priority]    NVARCHAR(20)    NULL,
        -- Pipe-delimited tag list
        [TagsRaw]     NVARCHAR(500)   NULL,
        [CmpId]       NVARCHAR(50)    NOT NULL DEFAULT '',
        [CreatedBy]   NVARCHAR(100)   NOT NULL DEFAULT '',
        [CreatedDate] DATETIME        NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT [FK_NisTicket_NisProject]
            FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[NisProject] ([ProjectId])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_NisTicket_ProjectId]       ON [dbo].[NisTicket] ([ProjectId]);
    CREATE INDEX [IX_NisTicket_CmpId_Status]    ON [dbo].[NisTicket] ([CmpId], [Status]);

    PRINT 'Created table NisTicket';
END
ELSE
    PRINT 'Table NisTicket already exists — skipped';

-- Existing installations may already have NisTicket without assignment date range.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisTicket' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('[dbo].[NisTicket]') AND name = 'StartDate'
    )
    BEGIN
        ALTER TABLE [dbo].[NisTicket] ADD [StartDate] DATETIME NULL;
        PRINT 'Added column NisTicket.StartDate';
    END
    ELSE
        PRINT 'Column NisTicket.StartDate already exists — skipped';

    IF NOT EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('[dbo].[NisTicket]') AND name = 'EndDate'
    )
    BEGIN
        ALTER TABLE [dbo].[NisTicket] ADD [EndDate] DATETIME NULL;
        PRINT 'Added column NisTicket.EndDate';
    END
    ELSE
        PRINT 'Column NisTicket.EndDate already exists — skipped';
END

-- ─── NisSalesOrder ───────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisSalesOrder' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[NisSalesOrder] (
        [SoId]        NVARCHAR(50)    NOT NULL PRIMARY KEY,
        [QuoteRef]    NVARCHAR(100)   NOT NULL,
        [Customer]    NVARCHAR(200)   NOT NULL DEFAULT '',
        [Date]        DATETIME        NULL,
        -- Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
        [Type]        NVARCHAR(50)    NOT NULL DEFAULT 'Implement',
        [Value]       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [Status]      NVARCHAR(50)    NOT NULL DEFAULT 'Active',
        [Project]     NVARCHAR(200)   NULL,
        [PoNumber]    NVARCHAR(100)   NULL,
        [PoDate]      DATETIME        NULL,
        [SalesName]   NVARCHAR(200)   NULL,
        [CmpId]       NVARCHAR(50)    NOT NULL DEFAULT '',
        [CreatedBy]   NVARCHAR(100)   NOT NULL DEFAULT '',
        [CreatedDate] DATETIME        NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME        NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX [IX_NisSalesOrder_CmpId]   ON [dbo].[NisSalesOrder] ([CmpId]);
    CREATE INDEX [IX_NisSalesOrder_Status]  ON [dbo].[NisSalesOrder] ([Status]);

    PRINT 'Created table NisSalesOrder';
END
ELSE
    PRINT 'Table NisSalesOrder already exists — skipped';

COMMIT TRANSACTION;

PRINT '=== Migration add_nis_project_tables complete ===';

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisTicket' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[NisTicket];
        PRINT 'Dropped NisTicket';
    END

    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisProject' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[NisProject];
        PRINT 'Dropped NisProject';
    END

    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisSalesOrder' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP TABLE [dbo].[NisSalesOrder];
        PRINT 'Dropped NisSalesOrder';
    END

COMMIT TRANSACTION;
*/
ฟ
