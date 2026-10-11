-- ============================================================
-- Migration: NIS Ticket — โควตา Onsite Accident
--
--   SourceProblemId nvarchar(50) NULL          — เลขเคส helpdesk (Problem.ProblemId) ที่เปิดตั๋ว MA Onsite นี้
--                                                 จากฟอร์ม Action ของ /productservice/servicerequest
--                                                 null = ตั๋วที่เปิดจาก NIS เอง
--   OverQuota       bit NOT NULL DEFAULT 0     — เปิดตั๋วทั้งที่โควตา Onsite Accident ของโครงการหมดแล้ว (ผู้ใช้ยืนยัน)
--
--   POST api/nis/projects/{id}/tickets:
--     - SourceProblemId ซ้ำในโครงการเดิม → คืนตั๋วเดิม (idempotent)
--     - Type = 'MA Onsite' บนโครงการ onsiteAccident และ remaining <= 0 → 409 เว้นแต่ส่ง allowOverQuota
--   GET api/nis/projects: เพิ่ม onsiteQuota { quota, used, remaining } (คำนวณ ไม่มี column)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisTicket
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query NisTicket จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisTicket', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'SourceProblemId')
        ALTER TABLE dbo.NisTicket ADD SourceProblemId nvarchar(50) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'OverQuota')
        ALTER TABLE dbo.NisTicket ADD OverQuota bit NOT NULL CONSTRAINT DF_NisTicket_OverQuota DEFAULT (0);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'IX_NisTicket_ProjectId_SourceProblemId')
        CREATE INDEX IX_NisTicket_ProjectId_SourceProblemId ON dbo.NisTicket (ProjectId, SourceProblemId);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   DROP INDEX IX_NisTicket_ProjectId_SourceProblemId ON dbo.NisTicket;
--   ALTER TABLE dbo.NisTicket DROP CONSTRAINT DF_NisTicket_OverQuota;
--   ALTER TABLE dbo.NisTicket DROP COLUMN SourceProblemId, OverQuota;
-- ============================================================
