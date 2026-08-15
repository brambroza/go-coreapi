-- ============================================================
-- Migration: NIS email template — เปลี่ยนหัวเรื่องอีเมลปิดงาน (close-job)
--
--   เดิม: "Service Report [TK_NUMBER] - [COMPANY]"
--   ใหม่: "[PROJECT] - [COMPANY]"   → ได้หัวเรื่องเป็นชื่องาน เช่น "Onsite MA รอบที่ 2 - บริษัท ก"
--
-- ค่า default ใน NisController เปลี่ยนแล้ว แต่ tenant ที่เคยกด "บันทึก" ในหน้า System Config
-- จะมี JSON เก็บไว้ใน dbo.NisSystemConfig.EmailTemplatesJson — script นี้อัปเดตของเดิมให้ตรงกัน
--
-- แตะเฉพาะแถวที่หัวเรื่องยังเป็นค่าเดิมเป๊ะ ๆ (tenant ที่แก้หัวเรื่องเองจะไม่ถูกเขียนทับ)
-- Idempotent: รันซ้ำได้ (รอบสองจะไม่เจอ pattern เดิมแล้ว)
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisSystemConfig', N'U') IS NULL
BEGIN
    RAISERROR (N'ไม่พบตาราง dbo.NisSystemConfig — รัน 20260814_AddNisSystemConfigEmailTemplate.sql ก่อน', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @oldSubject nvarchar(200) = N'"Subject":"Service Report [TK_NUMBER] - [COMPANY]"';
DECLARE @newSubject nvarchar(200) = N'"Subject":"[PROJECT] - [COMPANY]"';

-- ใช้ CHARINDEX ไม่ใช่ LIKE เพราะ [TK_NUMBER] จะถูกตีความเป็น character class ใน LIKE
UPDATE dbo.NisSystemConfig
SET EmailTemplatesJson = REPLACE(EmailTemplatesJson, @oldSubject, @newSubject)
WHERE EmailTemplatesJson IS NOT NULL
  AND CHARINDEX(@oldSubject, EmailTemplatesJson) > 0;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   UPDATE dbo.NisSystemConfig
--   SET EmailTemplatesJson = REPLACE(EmailTemplatesJson,
--       N'"Subject":"[PROJECT] - [COMPANY]"',
--       N'"Subject":"Service Report [TK_NUMBER] - [COMPANY]"')
--   WHERE EmailTemplatesJson IS NOT NULL
--     AND CHARINDEX(N'"Subject":"[PROJECT] - [COMPANY]"', EmailTemplatesJson) > 0;
-- ============================================================
