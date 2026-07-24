-- ============================================================
-- NIS Project — เพิ่มคอลัมน์ CustomerCode
-- เก็บรหัสลูกค้า (msb.mCustomer.CustomerCode) ไว้บน project เพื่อให้หน้าแก้ไข
-- สถานที่ (EditLocationDialog) resolve สถานที่ที่บันทึกไว้ของลูกค้า
-- (msb.mCustomerLocations) และ upsert เข้าลูกค้าได้ถูกต้อง
-- Nullable — โครงการเก่าจะเป็น NULL แล้ว backfill แบบ best-effort จากชื่อลูกค้า
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- 1) เพิ่มคอลัมน์ (idempotent)
IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisProject')
      AND name = N'CustomerCode'
)
BEGIN
    ALTER TABLE dbo.NisProject ADD CustomerCode nvarchar(50) NULL;
END;

COMMIT TRANSACTION;
GO

-- 2) Backfill best-effort: จับคู่ project.Customer (ชื่อ) กับ master customer ที่ CmpId เดียวกัน
--    เฉพาะกรณีชื่อไม่ซ้ำ (มี master ตรงเพียงรายเดียว) เพื่อกันจับคู่ผิด
UPDATE p
SET p.CustomerCode = c.CustomerCode
FROM dbo.NisProject p
CROSS APPLY
(
    SELECT TOP (2) m.CustomerCode
    FROM msb.mCustomer m
    WHERE m.CmpId = p.CmpId
      AND m.CustomerName = p.Customer
) c
WHERE p.CustomerCode IS NULL
  AND c.CustomerCode IS NOT NULL
  AND (
        SELECT COUNT(*)
        FROM msb.mCustomer m2
        WHERE m2.CmpId = p.CmpId
          AND m2.CustomerName = p.Customer
      ) = 1;
GO

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   ALTER TABLE dbo.NisProject DROP COLUMN CustomerCode;
-- ============================================================
