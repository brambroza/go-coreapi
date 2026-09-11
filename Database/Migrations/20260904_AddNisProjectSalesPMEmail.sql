-- ============================================================
-- Migration: NIS Project — อีเมลเซลผู้ดูแล (SalesPMEmail)
--
--   SalesPMEmail nvarchar(200) NULL
--       — อีเมลของเซล/PM ผู้ดูแลโครงการ คู่กับ SalesPMName / SalesPMPhone ที่มีอยู่แล้ว
--       NULL = โครงการเก่าที่สร้างก่อนมี field นี้
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisProject
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisProject', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisProject')
          AND name = N'SalesPMEmail'
    )
    BEGIN
        ALTER TABLE dbo.NisProject ADD SalesPMEmail nvarchar(200) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisProject DROP COLUMN SalesPMEmail;
-- ============================================================
