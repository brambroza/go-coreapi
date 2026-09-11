-- ============================================================
-- Migration: NIS Equipment master (dbo.NisEquipment)
--
-- อุปกรณ์ในตู้ Rack ของลูกค้าแต่ละราย (ชื่อตู้ rack, ชื่ออุปกรณ์, SN, รุ่น, ยี่ห้อ, หมายเหตุ)
-- ผูกกับ CustomerCode อย่างเดียว (ไม่แยกตาม location/สาขา) — เลือกใช้ตอนสร้าง NIS Project
-- ที่มีเงื่อนไข Preventive Maintenance (pmPerYear > 0)
--
-- โครงสร้างตารางตรงกับ EF entity goalongapi.Models.Nis.NisEquipment
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisEquipment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisEquipment
    (
        Id           uniqueidentifier NOT NULL CONSTRAINT PK_NisEquipment PRIMARY KEY DEFAULT NEWID(),
        CmpId        nvarchar(50)   NOT NULL,
        CustomerCode nvarchar(50)   NOT NULL,
        RackName     nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipment_RackName DEFAULT (''),
        DeviceName   nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipment_DeviceName DEFAULT (''),
        SerialNo     nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipment_SerialNo DEFAULT (''),
        Model        nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipment_Model DEFAULT (''),
        Brand        nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipment_Brand DEFAULT (''),
        Note         nvarchar(1000) NOT NULL CONSTRAINT DF_NisEquipment_Note DEFAULT (''),
        CreatedBy    nvarchar(100)  NOT NULL CONSTRAINT DF_NisEquipment_CreatedBy DEFAULT (''),
        CreatedDate  datetime       NOT NULL CONSTRAINT DF_NisEquipment_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy    nvarchar(100)  NOT NULL CONSTRAINT DF_NisEquipment_UpdatedBy DEFAULT (''),
        UpdatedDate  datetime       NOT NULL CONSTRAINT DF_NisEquipment_UpdatedDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_NisEquipment_Cmp_Customer ON dbo.NisEquipment (CmpId, CustomerCode);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอนทั้งตารางที่เพิ่งสร้าง):
--   DROP TABLE IF EXISTS dbo.NisEquipment;
-- ============================================================
