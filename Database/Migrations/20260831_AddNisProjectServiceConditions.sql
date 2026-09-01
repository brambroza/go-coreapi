-- ============================================================
-- Migration: NIS Project — เงื่อนไขบริการ (สัญญา) ที่เลือกตอนสร้างโครงการ
--
--   ServiceConditionsJson nvarchar(max) NULL
--       — JSON object: { "serviceYears": "1", "onsitePerYear": "4", "pmPerYear": "4",
--                        "sla": "8x5xNBD", "serviceReplacement": "company",
--                        "remoteBackup": "4", "monthlyReport": "4", "monthlyReportDay": "5",
--                        "deliveryType": "onsite_install", "deliveryBy": "nis_team",
--                        "onsiteAccident": false }
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
          AND name = N'ServiceConditionsJson'
    )
    BEGIN
        ALTER TABLE dbo.NisProject ADD ServiceConditionsJson nvarchar(max) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisProject DROP COLUMN ServiceConditionsJson;
-- ============================================================
