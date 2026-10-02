-- ============================================================
-- Migration: NIS Equipment change request (dbo.NisEquipmentChangeRequest)
--
-- คำขอแก้ทะเบียนอุปกรณ์ในตู้ Rack — ช่างยื่นจากหน้าตรวจ PM (CRM onsite form / แอปช่าง)
-- เมื่อพบว่าทะเบียนไม่ตรงหน้างาน SM อนุมัติใน Service Board จึงจะแก้ dbo.NisEquipment จริง
--   Reason: missing_in_registry (เพิ่ม) | not_in_rack (ลบ) | incorrect_info (แก้ข้อมูล)
--   Status: Pending | Approved | Rejected
-- ก่อนอนุมัติทะเบียนไม่เปลี่ยน ช่างตรวจตามทะเบียนเดิมต่อได้
--
-- โครงสร้างตารางตรงกับ EF entity goalongapi.Models.Nis.NisEquipmentChangeRequest
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisEquipmentChangeRequest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisEquipmentChangeRequest
    (
        RequestId    nvarchar(50)   NOT NULL CONSTRAINT PK_NisEquipmentChangeRequest PRIMARY KEY,
        CmpId        nvarchar(50)   NOT NULL,
        CustomerCode nvarchar(50)   NOT NULL,
        TicketId     nvarchar(50)   NULL,
        RackName     nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_RackName DEFAULT (''),
        Reason       nvarchar(50)   NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_Reason DEFAULT (''),
        EquipmentId  nvarchar(50)   NULL,
        DeviceName   nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_DeviceName DEFAULT (''),
        Brand        nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_Brand DEFAULT (''),
        Model        nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_Model DEFAULT (''),
        SerialNo     nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_SerialNo DEFAULT (''),
        UPosition    int            NULL,
        Note         nvarchar(1000) NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_Note DEFAULT (''),
        RequestedBy  nvarchar(200)  NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_RequestedBy DEFAULT (''),
        Status       nvarchar(20)   NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_Status DEFAULT (N'Pending'),
        ApprovedBy   nvarchar(100)  NULL,
        RejectedBy   nvarchar(100)  NULL,
        RejectReason nvarchar(500)  NULL,
        CreatedDate  datetime       NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate  datetime       NOT NULL CONSTRAINT DF_NisEquipmentChangeRequest_UpdatedDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_NisEquipmentChangeRequest_Cmp_Status ON dbo.NisEquipmentChangeRequest (CmpId, Status);
    CREATE INDEX IX_NisEquipmentChangeRequest_Ticket ON dbo.NisEquipmentChangeRequest (TicketId);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอนทั้งตารางที่เพิ่งสร้าง):
--   DROP TABLE IF EXISTS dbo.NisEquipmentChangeRequest;
-- ============================================================
