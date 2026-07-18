-- ============================================================
-- Migration: Onsite check-in/check-out GPS coordinates
-- Feature: NIS-Backend — ServiceTicketSubTaskAction.CheckInLatitude/CheckInLongitude/
--          CheckOutLatitude/CheckOutLongitude
-- Date: 2026-07-07
-- Author: Claude (backend-agent)
-- ============================================================
-- Run this script on GoAlongDatabase, AFTER add_serviceticket_master_tables.sql
-- and add_serviceticket_onsite_fields.sql.
-- Safe to re-run: every ALTER is guarded by an IF NOT EXISTS column check.
-- Rollback section is at the bottom of this file.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckInLatitude'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [CheckInLatitude] DECIMAL(18,10) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.CheckInLatitude';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.CheckInLatitude already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckInLongitude'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [CheckInLongitude] DECIMAL(18,10) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.CheckInLongitude';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.CheckInLongitude already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckOutLatitude'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [CheckOutLatitude] DECIMAL(18,10) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.CheckOutLatitude';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.CheckOutLatitude already exists — skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckOutLongitude'
)
BEGIN
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] ADD [CheckOutLongitude] DECIMAL(18,10) NULL;
    PRINT 'Added column ServiceTicketSubTaskAction.CheckOutLongitude';
END
ELSE
    PRINT 'Column ServiceTicketSubTaskAction.CheckOutLongitude already exists — skipped';
GO

PRINT '=== Migration add_serviceticketsubtaskaction_gps complete ===';
GO

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckOutLongitude')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [CheckOutLongitude];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckOutLatitude')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [CheckOutLatitude];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckInLongitude')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [CheckInLongitude];
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ServiceTicketSubTaskAction') AND name = 'CheckInLatitude')
    ALTER TABLE [dbo].[ServiceTicketSubTaskAction] DROP COLUMN [CheckInLatitude];
GO

PRINT 'Rolled back add_serviceticketsubtaskaction_gps';
GO
*/
