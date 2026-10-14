-- ============================================================
-- Migration: NIS — ไฟล์แนบ/ภาพบนคำขอเปิดตั๋วจาก Staff Portal
--
-- คำขอแบบเดิม (ไม่มี SourceProblemId) ยังไม่มีตั๋วตอนขอ จึงไม่มีที่เก็บ NisTicketFile —
-- เก็บ metadata ไฟล์เป็น JSON บนแถวคำขอ แล้วคัดลอกเป็น NisTicketFile ของตั๋วที่สร้างตอน SM อนุมัติ
--
-- dbo.NisPendingRequest
--   AttachmentsJson nvarchar(max) NULL — JSON List<NisAttachmentDto> {FileName, FilePath, Seq, FileSize}
--     ตัวไฟล์อยู่ที่ wwwroot/{cmpid}/nis/{projectNo}/tickets/ (ระบุโครงการ) หรือ wwwroot/{cmpid}/nis/requests/ (ไม่ระบุ)
--     ผ่าน /uploadallfile + /movefile · คำขอจาก helpdesk ไม่ใช้ช่องนี้ (ไฟล์อยู่ที่ตั๋ว 'Waiting Approval')
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisPendingRequest
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisPendingRequest', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'AttachmentsJson')
        ALTER TABLE dbo.NisPendingRequest ADD AttachmentsJson nvarchar(max) NULL;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   ALTER TABLE dbo.NisPendingRequest DROP COLUMN AttachmentsJson;
-- ============================================================
