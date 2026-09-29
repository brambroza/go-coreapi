-- ============================================================
-- Migration: NIS Ticket — วันเวลาที่ปิดงาน (ClosedDate)
--
--   ClosedDate datetime NULL
--       — เวลาที่ตั๋วเข้าสถานะ Closed / Done (เวลาไทย)
--       NULL = ตั๋วที่ยังไม่ปิด หรือถูกเปิดกลับ (reopen)
--       ใช้เรียงรายการ "งานที่ปิดแล้ว" ล่าสุดก่อน ทั้งฝั่ง CRM และแอปช่าง
--       (UpdatedDate ใช้แทนไม่ได้ เพราะเปลี่ยนทุกครั้งที่ตั๋วถูกแก้)
--
--   Backfill: ตั๋วที่ปิดไปแล้วก่อนมีคอลัมน์นี้ ใช้ UpdatedDate เป็นค่าประมาณ
--       (ไม่มีตาราง history ของสถานะให้ย้อนดูเวลาปิดจริง)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisTicket
-- Idempotent: รันซ้ำได้ (backfill แตะเฉพาะแถวที่ ClosedDate ยัง NULL)
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisTicket จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisTicket', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
          AND name = N'ClosedDate'
    )
    BEGIN
        ALTER TABLE dbo.NisTicket ADD ClosedDate datetime NULL;
    END;

    -- dynamic SQL: คอลัมน์เพิ่งถูกเพิ่มใน batch เดียวกัน ถ้าเขียน UPDATE ตรง ๆ จะ compile ไม่ผ่าน
    EXEC sys.sp_executesql N'
        UPDATE dbo.NisTicket
        SET ClosedDate = UpdatedDate
        WHERE Status IN (N''Closed'', N''Done'')
          AND ClosedDate IS NULL;';
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisTicket DROP COLUMN ClosedDate;
-- ============================================================
