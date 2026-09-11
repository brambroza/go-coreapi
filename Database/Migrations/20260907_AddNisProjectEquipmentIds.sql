-- ============================================================
-- Migration: NisProject.EquipmentIdsJson
--
-- เก็บรายการ NisEquipment.Id (JSON array ของ string) ที่เลือกไว้ตอนสร้างโครงการ PM
-- ("Preventive Maintenance (ครั้ง/ปี)" > 0) — null = ไม่ได้เลือก/โครงการเก่าก่อนมี feature นี้
--
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisProject')
      AND name = N'EquipmentIdsJson'
)
BEGIN
    ALTER TABLE dbo.NisProject ADD EquipmentIdsJson nvarchar(max) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisProject DROP COLUMN EquipmentIdsJson;
-- ============================================================
