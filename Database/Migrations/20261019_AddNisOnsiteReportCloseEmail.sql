-- ============================================================
-- Migration: NIS Onsite Report — จองเลข SR ตอนขอปิดงาน + เก็บอีเมล/PDF ไว้ส่งตอน SM อนุมัติ
--
--   CloseEmailJson nvarchar(max) NULL — ข้อมูลอีเมลปิดงานที่ช่างกรอกตอน POST onsite/{id}/request-close
--                                       (recipientEmail, emailSubject, emailMessage, contactName,
--                                        senderName, senderPosition, senderMobile, reportPdfFileName)
--                                       ใช้ตอน PUT tickets/{id}/close-approve ส่งอีเมลลูกค้าพร้อม PDF ที่เก็บไว้
--   RejectReason   nvarchar(500) NULL — เหตุผลที่ SM ตีกลับคำขอปิดงาน (PUT tickets/{id}/close-reject)
--
-- สถานะ (Status nvarchar(30) เดิม) มีค่าใหม่: reserved (POST onsite/{id}/reserve-sr) และ rejected
-- รายการ Service Report ยังกรองเฉพาะ submitted เหมือนเดิม
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisOnsiteReport
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisOnsiteReport จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisOnsiteReport', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteReport') AND name = N'CloseEmailJson')
        ALTER TABLE dbo.NisOnsiteReport ADD CloseEmailJson nvarchar(max) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteReport') AND name = N'RejectReason')
        ALTER TABLE dbo.NisOnsiteReport ADD RejectReason nvarchar(500) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisOnsiteReport DROP COLUMN CloseEmailJson, RejectReason;
-- ============================================================
