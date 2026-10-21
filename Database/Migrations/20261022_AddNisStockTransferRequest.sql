-- ============================================================
-- Migration: NIS stock transfer request (dbo.NisStockTransferRequest)
--
-- คำขอโยกย้ายคลัง — ช่างยื่นจากหน้าเบิกของ (CRM onsite form / แอปช่าง) เมื่อคลัง 2 Spare ลูกค้า
-- หรือคลัง 3 ของใน Stock ไม่มีของ SM โยกย้ายจริงผ่าน WMS โอนย้ายคลัง แล้วกด "โยกแล้ว"
--   Status: Pending | Done | Rejected
-- ตารางนี้ไม่ขยับสต็อกเอง (สต็อกเปลี่ยนตามเอกสาร transferwh ของ WMS)
--
-- โครงสร้างตารางตรงกับ EF entity goalongapi.Models.Nis.NisStockTransferRequest
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisStockTransferRequest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisStockTransferRequest
    (
        RequestId     nvarchar(50)   NOT NULL CONSTRAINT PK_NisStockTransferRequest PRIMARY KEY,
        CmpId         nvarchar(50)   NOT NULL,
        TicketId      nvarchar(50)   NULL,
        ProdCode      nvarchar(100)  NOT NULL CONSTRAINT DF_NisStockTransferRequest_ProdCode DEFAULT (''),
        ProdName      nvarchar(500)  NOT NULL CONSTRAINT DF_NisStockTransferRequest_ProdName DEFAULT (''),
        UnitCode      nvarchar(50)   NOT NULL CONSTRAINT DF_NisStockTransferRequest_UnitCode DEFAULT (''),
        Qty           decimal(18, 4) NOT NULL CONSTRAINT DF_NisStockTransferRequest_Qty DEFAULT (0),
        ToWhId        int            NOT NULL,
        Note          nvarchar(1000) NOT NULL CONSTRAINT DF_NisStockTransferRequest_Note DEFAULT (''),
        RequestedBy   nvarchar(200)  NOT NULL CONSTRAINT DF_NisStockTransferRequest_RequestedBy DEFAULT (''),
        Status        nvarchar(20)   NOT NULL CONSTRAINT DF_NisStockTransferRequest_Status DEFAULT (N'Pending'),
        HandledBy     nvarchar(200)  NULL,
        TransferDocNo nvarchar(50)   NULL,
        RejectReason  nvarchar(500)  NULL,
        CreatedDate   datetime       NOT NULL CONSTRAINT DF_NisStockTransferRequest_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate   datetime       NOT NULL CONSTRAINT DF_NisStockTransferRequest_UpdatedDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_NisStockTransferRequest_Cmp_Status ON dbo.NisStockTransferRequest (CmpId, Status);
    CREATE INDEX IX_NisStockTransferRequest_Ticket ON dbo.NisStockTransferRequest (TicketId);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอนทั้งตารางที่เพิ่งสร้าง):
--   DROP TABLE IF EXISTS dbo.NisStockTransferRequest;
-- ============================================================
