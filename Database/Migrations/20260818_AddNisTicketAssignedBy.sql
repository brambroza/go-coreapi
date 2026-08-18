-- ============================================================
-- Migration: เพิ่มคอลัมน์ผู้มอบหมายงาน ให้ dbo.NisTicket
--   AssignedBy nvarchar(100) NULL — Accounts.Username ของ SM ที่กดมอบหมายตั๋วนี้ให้ช่าง
--   (ต่างจาก Assignee ที่เก็บ Accounts.FullName ของช่างผู้รับงาน)
--   ใช้ตอนช่างกดรับงาน เพื่อแจ้งเตือนเฉพาะคนที่มอบหมาย ไม่ใช่ SM ทุกคน
-- Idempotent: รันซ้ำได้ (IF NOT EXISTS ต่อคอลัมน์)
-- ============================================================

BEGIN TRANSACTION;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
      AND name = N'AssignedBy'
)
BEGIN
    ALTER TABLE dbo.NisTicket ADD AssignedBy nvarchar(100) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   ALTER TABLE dbo.NisTicket DROP COLUMN AssignedBy;
-- ============================================================
