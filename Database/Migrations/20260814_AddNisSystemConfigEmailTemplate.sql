-- ============================================================
-- Migration: NIS System Config — email template + ลายเซ็นอีเมล
--
-- เพิ่ม 2 คอลัมน์บน dbo.NisSystemConfig (ต้องรันหลัง 20260804_AddNisSystemConfigChecklistByType.sql)
--
--   EmailTemplatesJson nvarchar(max) NULL
--       — JSON array: [{"id":"close-job","name":"...","subject":"...","body":"...","enabled":true}]
--         id ที่โค้ดอ้างถึง: close-job (ใช้ตอนส่งปิดงานให้ลูกค้า), quotation, ma-renewal, customer-accept
--   EmailSignatureJson nvarchar(max) NULL
--       — JSON object: {"enabled":true,"useLoginName":true,"senderName":"","position":"",...}
--         useLoginName = true → ชื่อ/ตำแหน่ง/มือถือ ใช้ของผู้ล็อกอินตอนส่ง (client เติมให้)
--
-- ค่า NULL = ใช้ค่า default ที่ hardcode ใน NisController (ไม่ต้อง seed)
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisSystemConfig', N'U') IS NULL
BEGIN
    RAISERROR (N'ไม่พบตาราง dbo.NisSystemConfig — รัน 20260804_AddNisSystemConfigChecklistByType.sql ก่อน', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisSystemConfig')
      AND name = N'EmailTemplatesJson'
)
BEGIN
    ALTER TABLE dbo.NisSystemConfig ADD EmailTemplatesJson nvarchar(max) NULL;
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisSystemConfig')
      AND name = N'EmailSignatureJson'
)
BEGIN
    ALTER TABLE dbo.NisSystemConfig ADD EmailSignatureJson nvarchar(max) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN EmailSignatureJson;
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN EmailTemplatesJson;
-- ============================================================
