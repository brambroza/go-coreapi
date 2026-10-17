-- ============================================================
-- Migration: NIS Ticket — สถานะ Check-out ของช่าง
--
--   CheckOutTime     datetime       NULL — เวลาเช็คเอาท์ครั้งแรก (เวลาไทย ตาม server)
--   CheckOutLat      float          NULL — พิกัดตอนเช็คเอาท์ (null = ไม่มี GPS)
--   CheckOutLng      float          NULL
--   CheckOutLocation nvarchar(500)  NULL — ชื่อสถานที่ที่เช็คเอาท์
--   CheckOutBy       nvarchar(200)  NULL — ผู้เช็คเอาท์
--
--   เขียนผ่าน PUT api/nis/tickets/{id}/checkout (RN + CRM) — คู่กับ 20261002_AddNisTicketCheckIn
--   เดิม check-out อยู่แค่ใน draft ของเครื่อง/ของ user → ช่างเช็คเอาท์จาก RN (Pct 90) แล้ว
--   CRM เปิดฟอร์มดู autosave snapshot ที่ไม่มี checkOutTime ทับ Pct กลับเป็น 25
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisTicket
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisTicket จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisTicket', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckOutTime')
        ALTER TABLE dbo.NisTicket ADD CheckOutTime datetime NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckOutLat')
        ALTER TABLE dbo.NisTicket ADD CheckOutLat float NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckOutLng')
        ALTER TABLE dbo.NisTicket ADD CheckOutLng float NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckOutLocation')
        ALTER TABLE dbo.NisTicket ADD CheckOutLocation nvarchar(500) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckOutBy')
        ALTER TABLE dbo.NisTicket ADD CheckOutBy nvarchar(200) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisTicket DROP COLUMN CheckOutTime, CheckOutLat, CheckOutLng, CheckOutLocation, CheckOutBy;
-- ============================================================
