-- ============================================================
-- Migration: NIS System Config — คลังที่ใช้เบิกของหน้างาน (onsite stock)
--
--   StockWarehousesJson nvarchar(max) NULL
--       — JSON object: { "saleWhId": 1, "spareWhId": 2, "stockWhId": 2600002 }
--         (mWareHouse.WareHouseId ของคลังสินค้าขาย / Spare ลูกค้า / ของใน Stock)
--       NULL = tenant นั้นใช้ default จากโค้ด (ไม่ต้อง backfill)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisSystemConfig
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisSystemConfig', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisSystemConfig')
          AND name = N'StockWarehousesJson'
    )
    BEGIN
        ALTER TABLE dbo.NisSystemConfig ADD StockWarehousesJson nvarchar(max) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN StockWarehousesJson;
-- ============================================================
