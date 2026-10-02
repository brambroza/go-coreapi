-- ============================================================
-- Migration: NIS Equipment — ตำแหน่ง U + ที่ตั้งตู้ (dbo.NisEquipment)
--
--   UPosition    int NULL
--       — ตำแหน่ง U ของอุปกรณ์ในตู้ Rack (NULL = ไม่ระบุ) ใช้เรียงแถวอุปกรณ์
--       ในหน้าตรวจ PM (CRM onsite form + แอปช่าง) และตอน import Excel
--   RackLocation nvarchar(200) NOT NULL DEFAULT ''
--       — ที่ตั้งของตู้ (เช่น "ห้อง IT ชั้น 2") ซ้ำกันทุกแถวของตู้เดียวกัน
--       (ตู้ = group by RackName เหมือนเดิม ไม่มีตาราง Rack แยก)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisEquipment
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisEquipment จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisEquipment', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.NisEquipment', 'UPosition') IS NULL
    BEGIN
        ALTER TABLE dbo.NisEquipment ADD UPosition int NULL;
    END;

    IF COL_LENGTH('dbo.NisEquipment', 'RackLocation') IS NULL
    BEGIN
        ALTER TABLE dbo.NisEquipment
            ADD RackLocation nvarchar(200) NOT NULL CONSTRAINT DF_NisEquipment_RackLocation DEFAULT ('');
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisEquipment DROP CONSTRAINT DF_NisEquipment_RackLocation;
--   ALTER TABLE dbo.NisEquipment DROP COLUMN RackLocation;
--   ALTER TABLE dbo.NisEquipment DROP COLUMN UPosition;
-- ============================================================
