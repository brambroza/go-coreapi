-- ============================================================
-- Migration: NIS System Config — ตัวเลือกเงื่อนไขงานของ wizard สร้างโครงการ
--
--   ServiceConditionOptionsJson nvarchar(max) NULL
--       — JSON object: { "serviceYears": ["1","2","3"],
--                        "onsitePerYearImplement": [...], "onsitePerYearMa": [...],
--                        "pmPerYearImplement": [...], "pmPerYearMa": [...],
--                        "remoteBackupImplement": [...], "remoteBackupMa": [...],
--                        "monthlyReport": [...],
--                        "serviceReplacement"/"deliveryType"/"deliveryBy": [{value,label}],
--                        "defaults": {...} }
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
          AND name = N'ServiceConditionOptionsJson'
    )
    BEGIN
        ALTER TABLE dbo.NisSystemConfig ADD ServiceConditionOptionsJson nvarchar(max) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN ServiceConditionOptionsJson;
-- ============================================================
