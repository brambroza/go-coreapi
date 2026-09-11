-- ============================================================
-- Migration: NIS Onsite Report — รูปเดี่ยวระดับ report (ก่อน/ระหว่าง/หลังงาน)
--
--   BeforePhoto nvarchar(max) NULL — รูปก่อนเริ่มงาน (URL หรือ base64)
--   DuringPhoto nvarchar(max) NULL — รูประหว่างทำงาน
--   AfterPhoto  nvarchar(max) NULL — รูปหลังทำงานเสร็จ
--
-- ⚠️ คนละความหมายกับ PmItemsJson (ใน dbo.NisOnsiteReport เดียวกัน) ที่แต่ละ PM item ก็มี
-- BeforePhoto/AfterPhoto ของตัวเอง (รูปก่อน-หลังต่อตู้/จุดตรวจ PM) — สามคอลัมน์นี้คือรูปเดี่ยว
-- ระดับใบรายงานทั้งใบ ไม่ผูกกับ PM item ใด
--
-- เดิม RN ExecuteScreen เก็บรูปสามช่องนี้เป็น state ชั่วคราวใช้แค่ render PDF preview ในเครื่อง
-- ไม่เคยส่งขึ้น backend เลย — ทำให้เปิดใบรายงานเก่าย้อนหลังแล้วไม่มีรูป เพิ่ม column ให้ submit/
-- request-close ส่งขึ้นมาเก็บจริง แล้ว GET /api/nis/onsite/reports คืนกลับให้ client แสดงย้อนหลังได้
--
-- หมายเหตุ: ตาราง dbo.NisOnsiteReport ไม่มี CREATE TABLE script อยู่ใน repo นี้ (สร้างนอกรอบ
-- เหมือน NisChatMessages) — migration นี้จึงเช็ค OBJECT_ID ก่อนเสมอ ถ้ายังไม่มีตารางให้ no-op เงียบ ๆ
--
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisOnsiteReport', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteReport')
          AND name = N'BeforePhoto'
    )
    BEGIN
        ALTER TABLE dbo.NisOnsiteReport ADD BeforePhoto nvarchar(max) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteReport')
          AND name = N'DuringPhoto'
    )
    BEGIN
        ALTER TABLE dbo.NisOnsiteReport ADD DuringPhoto nvarchar(max) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteReport')
          AND name = N'AfterPhoto'
    )
    BEGIN
        ALTER TABLE dbo.NisOnsiteReport ADD AfterPhoto nvarchar(max) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisOnsiteReport DROP COLUMN BeforePhoto;
--   ALTER TABLE dbo.NisOnsiteReport DROP COLUMN DuringPhoto;
--   ALTER TABLE dbo.NisOnsiteReport DROP COLUMN AfterPhoto;
-- ============================================================
