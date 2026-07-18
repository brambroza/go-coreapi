-- ============================================================
-- Migration: Add extended fields to ServiceTicketSubTaskAction
-- Date: 2026-06-24
-- Description: Adds WorkDetail, IssueDetail, SignatureFilePath,
--              ChecklistItemsJson, RackPhotosJson, DamagedProductJson,
--              OthersItemsJson for field service report capture.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'WorkDetail'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD WorkDetail nvarchar(4000) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'IssueDetail'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD IssueDetail nvarchar(4000) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'SignatureFilePath'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD SignatureFilePath nvarchar(500) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'ChecklistItemsJson'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD ChecklistItemsJson nvarchar(max) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'RackPhotosJson'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD RackPhotosJson nvarchar(max) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'DamagedProductJson'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD DamagedProductJson nvarchar(max) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction')
      AND name = 'OthersItemsJson'
)
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD OthersItemsJson nvarchar(max) NULL;

PRINT 'Migration add_action_extended_fields completed successfully.';
