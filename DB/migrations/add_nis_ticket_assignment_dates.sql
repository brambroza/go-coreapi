-- ============================================================
-- Migration: Add assignment date range to NIS tickets
-- Feature: NIS Service Board quick assign start/end date
-- Date: 2026-07-08
-- ============================================================
-- Run this script on GoAlongDatabase.
-- Safe to re-run: every ALTER is guarded by sys.columns checks.
-- Rollback section is at the bottom of this file.
-- ============================================================

BEGIN TRANSACTION;

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
ELSE
    PRINT 'Table NisTicket not found — skipped';

COMMIT TRANSACTION;

PRINT '=== Migration add_nis_ticket_assignment_dates complete ===';

-- ============================================================
-- ROLLBACK SCRIPT (run manually if needed — do NOT execute here)
-- ============================================================
/*
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('[dbo].[NisTicket]') AND name = 'EndDate'
)
    ALTER TABLE [dbo].[NisTicket] DROP COLUMN [EndDate];

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('[dbo].[NisTicket]') AND name = 'StartDate'
)
    ALTER TABLE [dbo].[NisTicket] DROP COLUMN [StartDate];

COMMIT TRANSACTION;
*/
