-- ============================================================
-- Migration: เพิ่มคอลัมน์ผู้แก้ไขล่าสุด ให้ dbo.NisTicket
--   UpdatedBy nvarchar(100) NULL — user ที่แก้ไข ticket ล่าสุด (คู่กับ UpdatedDate)
-- Idempotent: รันซ้ำได้ (IF NOT EXISTS ต่อคอลัมน์)
-- ============================================================

BEGIN TRANSACTION;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
      AND name = N'UpdatedBy'
)
BEGIN
    ALTER TABLE dbo.NisTicket ADD UpdatedBy nvarchar(100) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   ALTER TABLE dbo.NisTicket DROP COLUMN UpdatedBy;
-- ============================================================
