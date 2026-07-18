-- ============================================================
-- Migration: Add NIS pending-request table
-- Feature: Staff Portal "open ticket" request → Service Board approve/reject
-- Endpoints: GET/POST /api/nis/pending-requests,
--            POST /api/nis/pending-requests/{id}/approve,
--            DELETE /api/nis/pending-requests/{id}
-- Date: 2026-07-09
-- ============================================================
-- Run this script on GoAlongDatabase.
-- Safe to re-run: CREATE guarded with IF NOT EXISTS.
-- Rollback section is at the bottom of this file.
-- ============================================================

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisPendingRequest' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[NisPendingRequest] (
        [RequestId]            NVARCHAR(50)   NOT NULL,
        [RequestedBy]          NVARCHAR(200)  NOT NULL,
        [Title]                NVARCHAR(500)  NOT NULL,
        [TicketType]           NVARCHAR(50)   NULL,
        [SupportMethod]        NVARCHAR(50)   NULL,
        [ProjectId]            NVARCHAR(50)   NULL,
        [Location]             NVARCHAR(500)  NULL,
        [Detail]               NVARCHAR(2000) NULL,
        [Due]                  DATETIME       NULL,
        [NoOnsite]             BIT            NOT NULL CONSTRAINT [DF_NisPendingRequest_NoOnsite] DEFAULT (0),
        [SkipSignature]        BIT            NOT NULL CONSTRAINT [DF_NisPendingRequest_SkipSignature] DEFAULT (0),
        [RequireCloseApproval] BIT            NOT NULL CONSTRAINT [DF_NisPendingRequest_RequireCloseApproval] DEFAULT (0),
        [ParentTicketId]       NVARCHAR(50)   NULL,
        [Status]               NVARCHAR(20)   NOT NULL CONSTRAINT [DF_NisPendingRequest_Status] DEFAULT ('Pending'),
        [CreatedTicketId]      NVARCHAR(50)   NULL,
        [CmpId]                NVARCHAR(50)   NOT NULL,
        [ApprovedBy]           NVARCHAR(100)  NULL,
        [RejectedBy]           NVARCHAR(100)  NULL,
        [CreatedDate]          DATETIME       NOT NULL CONSTRAINT [DF_NisPendingRequest_CreatedDate] DEFAULT (GETDATE()),
        [UpdatedDate]          DATETIME       NOT NULL CONSTRAINT [DF_NisPendingRequest_UpdatedDate] DEFAULT (GETDATE()),
        CONSTRAINT [PK_NisPendingRequest] PRIMARY KEY CLUSTERED ([RequestId])
    );

    -- Filter/sort indexes match the query paths in NisController.GetPendingRequests:
    --   manager view → (CmpId, Status);  staff history → (CmpId, RequestedBy)
    CREATE INDEX [IX_NisPendingRequest_CmpId_Status]      ON [dbo].[NisPendingRequest] ([CmpId], [Status]);
    CREATE INDEX [IX_NisPendingRequest_CmpId_RequestedBy] ON [dbo].[NisPendingRequest] ([CmpId], [RequestedBy]);

    PRINT 'Created table NisPendingRequest';
END
ELSE
    PRINT 'Table NisPendingRequest already exists — skipped';

COMMIT TRANSACTION;

PRINT '=== Migration add_nis_pending_request_table complete ===';

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisPendingRequest' AND schema_id = SCHEMA_ID('dbo'))
    DROP TABLE [dbo].[NisPendingRequest];
COMMIT TRANSACTION;
*/
