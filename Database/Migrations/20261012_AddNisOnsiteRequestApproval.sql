-- ============================================================
-- Migration: NIS — เปิดเคส Onsite จาก helpdesk → ตั๋วรอ SM อนุมัติ
--
-- สถานะตั๋วใหม่: 'Waiting Approval' (ไม่มี column ใหม่ — ค่าใน NisTicket.Status)
--   staff helpdesk กด "เปิดเคส Onsite" → สร้าง NisTicket type 'MA Onsite' สถานะนี้ทันที (ตัดโควตาทันที)
--   คู่กับแถว NisPendingRequest (Status = Pending, CreatedTicketId = ตั๋วนั้น)
--   SM อนุมัติ → ตั๋วเดิมเปลี่ยนเป็น Scheduled/Open · ปฏิเสธ → ลบตั๋ว (คืนโควตา) เก็บแถวคำขอเป็นประวัติ
--   ตั๋วสถานะนี้ห้าม assign / เปลี่ยนสถานะ / accept / check-in (API คืน 409 TICKET_WAITING_APPROVAL)
--
-- dbo.NisTicket
--   CustomerLineUserId nvarchar(100) NULL   — LINE userId ของลูกค้าที่แจ้งเคส (STProblem.UserLineId)
--   CustomerOaLineId   nvarchar(100) NULL   — LINE OA id ที่ลูกค้าคุยด้วย (STProblem.OALineId)
--
-- dbo.NisPendingRequest
--   SourceProblemId    nvarchar(50)  NULL   — เลขเคส helpdesk ต้นทาง (null = คำขอจาก Staff Portal/RN เดิม)
--   CustomerCode / CustomerName            — ลูกค้าของเคส (แสดงในแท็บอนุมัติโดยไม่ต้อง join)
--   CustomerLineUserId / CustomerOaLineId  — สำเนาจากเคส ใช้ตอนสร้างตั๋ว
--   AllowOverQuota     bit NOT NULL DEFAULT 0 — ผู้ขอยืนยันเปิดเกินโควตา Onsite Accident
--   Priority           nvarchar(20)  NULL   — priority ที่ขอ (default = ของโครงการ)
--   RejectReason       nvarchar(500) NULL   — เหตุผลที่ SM ปฏิเสธ (แสดงให้ผู้ขอ)
--
-- dbo.NisTicketFile (ใหม่) — ไฟล์แนบระดับตั๋ว (ภาพ/เอกสารที่ staff แนบตอนขอเปิดเคส)
--   ตัวไฟล์อยู่ที่ wwwroot/{cmpid}/nis/{projectNo}/tickets/{fileName} ผ่าน /uploadallfile + /movefile
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisTicket / NisPendingRequest / NisTicketFile
-- Idempotent: รันซ้ำได้
-- ต้องรันก่อน deploy โค้ดที่อ่านคอลัมน์นี้ — ไม่งั้น query จะ error "Invalid column name"
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisTicket', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CustomerLineUserId')
        ALTER TABLE dbo.NisTicket ADD CustomerLineUserId nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisTicket') AND name = N'CustomerOaLineId')
        ALTER TABLE dbo.NisTicket ADD CustomerOaLineId nvarchar(100) NULL;
END;

IF OBJECT_ID(N'dbo.NisPendingRequest', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'SourceProblemId')
        ALTER TABLE dbo.NisPendingRequest ADD SourceProblemId nvarchar(50) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'CustomerCode')
        ALTER TABLE dbo.NisPendingRequest ADD CustomerCode nvarchar(50) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'CustomerName')
        ALTER TABLE dbo.NisPendingRequest ADD CustomerName nvarchar(300) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'CustomerLineUserId')
        ALTER TABLE dbo.NisPendingRequest ADD CustomerLineUserId nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'CustomerOaLineId')
        ALTER TABLE dbo.NisPendingRequest ADD CustomerOaLineId nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'AllowOverQuota')
        ALTER TABLE dbo.NisPendingRequest ADD AllowOverQuota bit NOT NULL CONSTRAINT DF_NisPendingRequest_AllowOverQuota DEFAULT (0);

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'Priority')
        ALTER TABLE dbo.NisPendingRequest ADD Priority nvarchar(20) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'RejectReason')
        ALTER TABLE dbo.NisPendingRequest ADD RejectReason nvarchar(500) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.NisPendingRequest') AND name = N'IX_NisPendingRequest_Cmp_SourceProblem')
        CREATE INDEX IX_NisPendingRequest_Cmp_SourceProblem ON dbo.NisPendingRequest (CmpId, SourceProblemId);
END;

IF OBJECT_ID(N'dbo.NisTicketFile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisTicketFile
    (
        FileId      nvarchar(50)   NOT NULL CONSTRAINT PK_NisTicketFile PRIMARY KEY,
        TicketId    nvarchar(50)   NOT NULL,
        ProjectId   nvarchar(50)   NOT NULL CONSTRAINT DF_NisTicketFile_ProjectId DEFAULT (''),
        FileName    nvarchar(300)  NOT NULL CONSTRAINT DF_NisTicketFile_FileName DEFAULT (''),
        FilePath    nvarchar(1000) NOT NULL CONSTRAINT DF_NisTicketFile_FilePath DEFAULT (''),
        Seq         int            NOT NULL CONSTRAINT DF_NisTicketFile_Seq DEFAULT (1),
        FileSize    bigint         NOT NULL CONSTRAINT DF_NisTicketFile_FileSize DEFAULT (0),
        CmpId       nvarchar(50)   NOT NULL CONSTRAINT DF_NisTicketFile_CmpId DEFAULT (''),
        CreatedBy   nvarchar(100)  NOT NULL CONSTRAINT DF_NisTicketFile_CreatedBy DEFAULT (''),
        CreatedDate datetime       NOT NULL CONSTRAINT DF_NisTicketFile_CreatedDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_NisTicketFile_Ticket ON dbo.NisTicketFile (TicketId);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   DROP TABLE IF EXISTS dbo.NisTicketFile;
--   DROP INDEX IX_NisPendingRequest_Cmp_SourceProblem ON dbo.NisPendingRequest;
--   ALTER TABLE dbo.NisPendingRequest DROP CONSTRAINT DF_NisPendingRequest_AllowOverQuota;
--   ALTER TABLE dbo.NisPendingRequest DROP COLUMN SourceProblemId, CustomerCode, CustomerName,
--       CustomerLineUserId, CustomerOaLineId, AllowOverQuota, Priority, RejectReason;
--   ALTER TABLE dbo.NisTicket DROP COLUMN CustomerLineUserId, CustomerOaLineId;
--   UPDATE dbo.NisTicket SET Status = 'Open' WHERE Status = 'Waiting Approval';
-- ============================================================
