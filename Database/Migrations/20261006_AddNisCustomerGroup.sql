-- ============================================================
-- Migration: NIS Service Report — กลุ่มลูกค้า (ลูกค้าหลายรหัส = รายงานฉบับเดียว)
--   dbo.NisCustomerGroup  MergeIntoCustomerId = รหัสหลักที่รวมเข้า · Backdated = ธงบันทึกย้อนหลังระดับกลุ่ม
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisCustomerGroup
-- ไม่มี FK ไป NisServiceCase (รหัสอาจถูกตั้งค่าไว้ก่อนมีเคสจริง)
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisCustomerGroup', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisCustomerGroup
    (
        CmpId               nvarchar(50)  NOT NULL,
        CustomerId          nvarchar(50)  NOT NULL,
        MergeIntoCustomerId nvarchar(50)  NULL,
        Backdated           bit           NOT NULL CONSTRAINT DF_NisCustomerGroup_Backdated DEFAULT (0),
        CreatedAt           datetimeoffset(3) NOT NULL CONSTRAINT DF_NisCustomerGroup_CreatedAt DEFAULT (SYSDATETIMEOFFSET()),
        UpdatedAt           datetimeoffset(3) NOT NULL CONSTRAINT DF_NisCustomerGroup_UpdatedAt DEFAULT (SYSDATETIMEOFFSET()),
        UpdatedBy           nvarchar(100) NOT NULL CONSTRAINT DF_NisCustomerGroup_UpdatedBy DEFAULT (''),
        CONSTRAINT PK_NisCustomerGroup PRIMARY KEY (CmpId, CustomerId)
    );

    CREATE INDEX IX_NisCustomerGroup_Cmp_MergeInto ON dbo.NisCustomerGroup (CmpId, MergeIntoCustomerId);
END;

COMMIT TRANSACTION;

-- Rollback (manual): DROP TABLE IF EXISTS dbo.NisCustomerGroup;
