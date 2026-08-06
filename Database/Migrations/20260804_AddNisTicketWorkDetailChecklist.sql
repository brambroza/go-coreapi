-- ============================================================
-- Migration: เพิ่มรายละเอียดงาน + checklist ก่อนมอบหมาย ให้ dbo.NisTicket
--   WorkDetail    nvarchar(max) NULL — รายละเอียดงานที่ระบุก่อน assign
--   ChecklistJson nvarchar(max) NULL — JSON array ของ { id, text, done }
-- Idempotent: รันซ้ำได้ (IF NOT EXISTS ต่อคอลัมน์)
-- ============================================================

BEGIN TRANSACTION;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
      AND name = N'WorkDetail'
)
BEGIN
    ALTER TABLE dbo.NisTicket ADD WorkDetail nvarchar(max) NULL;
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
      AND name = N'ChecklistJson'
)
BEGIN
    ALTER TABLE dbo.NisTicket ADD ChecklistJson nvarchar(max) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   ALTER TABLE dbo.NisTicket DROP COLUMN ChecklistJson;
--   ALTER TABLE dbo.NisTicket DROP COLUMN WorkDetail;
-- ============================================================
