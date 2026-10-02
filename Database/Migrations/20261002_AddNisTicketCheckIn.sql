-- ============================================================
-- Migration: NIS Ticket — สถานะ Check-in ของช่าง
--
--   CheckInTime     datetime       NULL — เวลาเช็คอินครั้งแรก (เวลาไทย ตาม server)
--   CheckInLat      float          NULL — พิกัดตอนเช็คอิน (null = ไม่มี GPS)
--   CheckInLng      float          NULL
--   CheckInLocation nvarchar(500)  NULL — ชื่อสถานที่ที่เช็คอิน
--   CheckInBy       nvarchar(200)  NULL — ผู้เช็คอิน
--
--   เขียนผ่าน PUT api/nis/tickets/{id}/checkin (RN + CRM)
--   เดิม check-in อยู่แค่ใน draft ของเครื่อง → ช่างเช็คอินจาก RN แล้ว CRM ไม่เห็นสถานะ
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisTicket
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisTicket จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisTicket', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckInTime')
        ALTER TABLE dbo.NisTicket ADD CheckInTime datetime NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckInLat')
        ALTER TABLE dbo.NisTicket ADD CheckInLat float NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckInLng')
        ALTER TABLE dbo.NisTicket ADD CheckInLng float NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckInLocation')
        ALTER TABLE dbo.NisTicket ADD CheckInLocation nvarchar(500) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CheckInBy')
        ALTER TABLE dbo.NisTicket ADD CheckInBy nvarchar(200) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisTicket DROP COLUMN CheckInTime, CheckInLat, CheckInLng, CheckInLocation, CheckInBy;
-- ============================================================
