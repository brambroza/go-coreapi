-- ============================================================
-- Migration: NIS project items (dbo.NisProjectItem)
--
-- รายการสินค้าขายของโครงการ — ดึงจาก Sale Order ใน NisProject.SoRef (SP getSaleOrderItem_All)
-- งาน Implement: ช่างกด "นำของออก" จากคลัง 1 สินค้าสำหรับขาย ได้ไม่เกิน Qty
--   QtyIssued = ยอดที่สร้างใบเบิกแล้ว (backend ตรวจไม่ให้เกิน Qty)
--
-- โครงสร้างตารางตรงกับ EF entity goalongapi.Models.Nis.NisProjectItem
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisProjectItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisProjectItem
    (
        Id          nvarchar(50)   NOT NULL CONSTRAINT PK_NisProjectItem PRIMARY KEY,
        ProjectId   nvarchar(50)   NOT NULL,
        CmpId       nvarchar(50)   NOT NULL,
        SoNo        nvarchar(50)   NOT NULL CONSTRAINT DF_NisProjectItem_SoNo DEFAULT (''),
        ProdCode    nvarchar(100)  NOT NULL CONSTRAINT DF_NisProjectItem_ProdCode DEFAULT (''),
        ProdName    nvarchar(500)  NOT NULL CONSTRAINT DF_NisProjectItem_ProdName DEFAULT (''),
        UnitCode    nvarchar(50)   NOT NULL CONSTRAINT DF_NisProjectItem_UnitCode DEFAULT (''),
        Qty         decimal(18, 4) NOT NULL CONSTRAINT DF_NisProjectItem_Qty DEFAULT (0),
        QtyIssued   decimal(18, 4) NOT NULL CONSTRAINT DF_NisProjectItem_QtyIssued DEFAULT (0),
        CreatedDate datetime       NOT NULL CONSTRAINT DF_NisProjectItem_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate datetime       NOT NULL CONSTRAINT DF_NisProjectItem_UpdatedDate DEFAULT (GETDATE())
    );

    CREATE INDEX IX_NisProjectItem_Project ON dbo.NisProjectItem (ProjectId);
    CREATE UNIQUE INDEX UX_NisProjectItem_Project_So_Prod ON dbo.NisProjectItem (ProjectId, SoNo, ProdCode);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอนทั้งตารางที่เพิ่งสร้าง):
--   DROP TABLE IF EXISTS dbo.NisProjectItem;
-- ============================================================
