-- ============================================================
-- Migration: NisEquipmentChangeRequest — เพิ่มช่องให้ครบเท่า Equipment Master
--
-- ฟอร์ม "แจ้งแก้ทะเบียนตู้ Rack" (CRM onsite form / แอปช่าง) คีย์ข้อมูลเหมือนหน้า master อุปกรณ์:
--   RackLocation  = ตำแหน่งตู้ (ห้อง/ชั้น) → NisEquipment.RackLocation (ทุกแถวของตู้)
--   EquipmentNote = หมายเหตุของอุปกรณ์ → NisEquipment.Note
--     NULL = คำขอรุ่นก่อน (ตอนนั้น Note ถูกใช้เป็นหมายเหตุอุปกรณ์ด้วย) · Note ยังเป็นเหตุผลถึงผู้อนุมัติ
--
-- ต้องรันหลัง 20261002_AddNisEquipmentChangeRequest.sql
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisEquipmentChangeRequest', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.NisEquipmentChangeRequest', N'RackLocation') IS NULL
BEGIN
    ALTER TABLE dbo.NisEquipmentChangeRequest
        ADD RackLocation nvarchar(200) NOT NULL
            CONSTRAINT DF_NisEquipmentChangeRequest_RackLocation DEFAULT ('');
END;

IF OBJECT_ID(N'dbo.NisEquipmentChangeRequest', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.NisEquipmentChangeRequest', N'EquipmentNote') IS NULL
BEGIN
    ALTER TABLE dbo.NisEquipmentChangeRequest
        ADD EquipmentNote nvarchar(1000) NULL;
END;

COMMIT TRANSACTION;
