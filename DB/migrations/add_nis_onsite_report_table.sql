-- ============================================================
-- Migration: Add NIS onsite service report table
-- Feature: Onsite Form (NisTicket-backed) submit / request-close
-- Endpoints: POST /api/nis/onsite/submit, POST /api/nis/onsite/{id}/request-close
-- Date: 2026-07-09
-- ============================================================
-- Run this script on GoAlongDatabase.
-- Safe to re-run: CREATE guarded with IF NOT EXISTS.
-- Rollback section is at the bottom of this file.
-- ============================================================

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisOnsiteReport' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[NisOnsiteReport] (
        [ReportId]              NVARCHAR(50)    NOT NULL,
        [NisTicketId]           NVARCHAR(50)    NOT NULL,
        [TicketCode]            NVARCHAR(50)    NULL,
        [SrNumber]              NVARCHAR(50)    NULL,
        [CmpId]                 NVARCHAR(50)    NOT NULL,
        [Engineer]              NVARCHAR(200)   NULL,
        [CheckInTime]           NVARCHAR(100)   NULL,
        [CheckOutTime]          NVARCHAR(100)   NULL,
        [CheckInLatitude]       DECIMAL(10,6)   NULL,
        [CheckInLongitude]      DECIMAL(10,6)   NULL,
        [CheckOutLatitude]      DECIMAL(10,6)   NULL,
        [CheckOutLongitude]     DECIMAL(10,6)   NULL,
        [WorkDetail]            NVARCHAR(MAX)   NULL,
        [IssueDetail]           NVARCHAR(MAX)   NULL,
        [ChecklistJson]         NVARCHAR(MAX)   NULL,
        [PmItemsJson]           NVARCHAR(MAX)   NULL,
        [DamagedProductJson]    NVARCHAR(MAX)   NULL,
        [SupportCasesJson]      NVARCHAR(MAX)   NULL,
        [PhotosJson]            NVARCHAR(MAX)   NULL,
        [SignatureImageBase64]  NVARCHAR(MAX)   NULL,
        [SkipSignature]         BIT             NOT NULL CONSTRAINT [DF_NisOnsiteReport_SkipSignature] DEFAULT (0),
        [Status]                NVARCHAR(30)    NOT NULL CONSTRAINT [DF_NisOnsiteReport_Status] DEFAULT ('submitted'),
        [CreatedDate]           DATETIME        NOT NULL CONSTRAINT [DF_NisOnsiteReport_CreatedDate] DEFAULT (GETDATE()),
        CONSTRAINT [PK_NisOnsiteReport] PRIMARY KEY CLUSTERED ([ReportId])
    );

    -- NisTicketId → report lookup; (CmpId, SrNumber) → SR-number counter query.
    CREATE INDEX [IX_NisOnsiteReport_NisTicketId]   ON [dbo].[NisOnsiteReport] ([NisTicketId]);
    CREATE INDEX [IX_NisOnsiteReport_CmpId_SrNumber] ON [dbo].[NisOnsiteReport] ([CmpId], [SrNumber]);

    PRINT 'Created table NisOnsiteReport';
END
ELSE
    PRINT 'Table NisOnsiteReport already exists — skipped';

COMMIT TRANSACTION;

PRINT '=== Migration add_nis_onsite_report_table complete ===';

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisOnsiteReport' AND schema_id = SCHEMA_ID('dbo'))
    DROP TABLE [dbo].[NisOnsiteReport];
COMMIT TRANSACTION;
*/
