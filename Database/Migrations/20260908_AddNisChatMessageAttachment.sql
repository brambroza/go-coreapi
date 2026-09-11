-- ============================================================
-- Migration: NIS Chat Message — แนบไฟล์ (Attachment)
--
--   AttachmentUrl  nvarchar(500) NULL — URL/path ของไฟล์แนบ (เช่น PDF) ที่อัปโหลดแล้ว
--   AttachmentName nvarchar(255) NULL — ชื่อไฟล์เดิมที่ผู้ใช้แนบ (แสดงในแชท)
--   AttachmentType nvarchar(50)  NULL — MIME/ประเภทไฟล์ (เช่น "application/pdf") ใช้เลือก preview UI
--
-- go-chat-api (Node.js) เพิ่ม feature แนบ + preview PDF ในแชท Staff ของ NIS ticket แล้ว —
-- แต่ schema ownership ของตาราง NisChatMessages อยู่ที่ coreapi-new ตาม CLAUDE.md ของทั้งสอง repo
-- (go-chat-api เรียกแค่ parameterized SQL ไม่ได้เป็นเจ้าของ migration) จึงเพิ่ม column ที่นี่
--
-- หมายเหตุ: ตาราง dbo.NisChatMessages ไม่มี CREATE TABLE script อยู่ใน repo นี้เลย (สร้างตรงโดย DBA
-- นอกรอบตามที่ comment ฝั่ง go-chat-api ระบุ) — migration นี้จึงเช็ค OBJECT_ID ก่อนเสมอ ถ้ายังไม่มีตาราง
-- ให้ no-op เงียบ ๆ (ไม่ throw) แทนที่จะ fail กันกรณี environment ที่ยังไม่ได้สร้างตารางนี้
--
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisChatMessages', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisChatMessages')
          AND name = N'AttachmentUrl'
    )
    BEGIN
        ALTER TABLE dbo.NisChatMessages ADD AttachmentUrl nvarchar(500) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisChatMessages')
          AND name = N'AttachmentName'
    )
    BEGIN
        ALTER TABLE dbo.NisChatMessages ADD AttachmentName nvarchar(255) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisChatMessages')
          AND name = N'AttachmentType'
    )
    BEGIN
        ALTER TABLE dbo.NisChatMessages ADD AttachmentType nvarchar(50) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisChatMessages DROP COLUMN AttachmentUrl;
--   ALTER TABLE dbo.NisChatMessages DROP COLUMN AttachmentName;
--   ALTER TABLE dbo.NisChatMessages DROP COLUMN AttachmentType;
-- ============================================================
