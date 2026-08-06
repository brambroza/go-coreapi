-- ============================================================
-- Migration: NIS System Config — checklist master แบบผูกกับ ticket type + customer override
--
-- กรณี A) ตาราง dbo.NisSystemConfig ยังไม่มี  → สร้างใหม่พร้อมคอลัมน์ครบ (รวม 2 คอลัมน์ใหม่)
-- กรณี B) ตารางมีอยู่แล้ว                     → เพิ่มเฉพาะ 2 คอลัมน์ที่ยังไม่มี
--
--   ChecklistByTicketTypeJson nvarchar(max) NULL
--       — JSON object: { "Install": ["..."], "PM": ["..."], ... }
--   ChecklistByCustomerJson   nvarchar(max) NULL
--       — JSON object: { "<customerCode>": { "Install": ["..."], ... }, ... }
--       ว่าง/ไม่มี customer นั้น = fallback ไปใช้ ChecklistByTicketType (standard)
--
-- โครงสร้างตารางตรงกับ EF entity goalongapi.Models.Nis.NisSystemConfig
-- (คอลัมน์ >4000 ตัวอักษร ใช้ nvarchar(max) แทน nvarchar(8000) ที่ SQL Server ไม่รองรับ)
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisSystemConfig', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisSystemConfig
    (
        CmpId nvarchar(50) NOT NULL CONSTRAINT PK_NisSystemConfig PRIMARY KEY,
        JobTypesRaw nvarchar(2000) NOT NULL
            CONSTRAINT DF_NisSystemConfig_JobTypesRaw
            DEFAULT ('Runrate|Implement|MA-Device|MA-Fortigate|MA-Software|MA-Network'),
        TagsRaw nvarchar(4000) NOT NULL
            CONSTRAINT DF_NisSystemConfig_TagsRaw
            DEFAULT ('Firewall|Network|WiFi|Server|CCTV|Access Control|PC&Notebook|Peripheral|Software|Cable|Windows Server|VMware|HyperV'),
        ImplementChecklistRaw nvarchar(max) NOT NULL
            CONSTRAINT DF_NisSystemConfig_ImplementChecklistRaw DEFAULT (''),
        MaChecklistRaw nvarchar(max) NOT NULL
            CONSTRAINT DF_NisSystemConfig_MaChecklistRaw DEFAULT (''),
        PmChecklistRaw nvarchar(max) NOT NULL
            CONSTRAINT DF_NisSystemConfig_PmChecklistRaw DEFAULT (''),
        ChecklistByTicketTypeJson nvarchar(max) NULL,
        ChecklistByCustomerJson nvarchar(max) NULL,
        SlaOptionsRaw nvarchar(500) NOT NULL
            CONSTRAINT DF_NisSystemConfig_SlaOptionsRaw
            DEFAULT ('8x5xNBD|8x5|24x7x4|24x7xNBD'),
        WarningDaysService int NOT NULL
            CONSTRAINT DF_NisSystemConfig_WarningDaysService DEFAULT (60),
        WarningDaysProduct int NOT NULL
            CONSTRAINT DF_NisSystemConfig_WarningDaysProduct DEFAULT (30),
        UpdatedBy nvarchar(100) NOT NULL
            CONSTRAINT DF_NisSystemConfig_UpdatedBy DEFAULT (''),
        UpdatedDate datetime NOT NULL
            CONSTRAINT DF_NisSystemConfig_UpdatedDate DEFAULT (GETDATE())
    );
END
ELSE
BEGIN
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisSystemConfig')
          AND name = N'ChecklistByTicketTypeJson'
    )
    BEGIN
        ALTER TABLE dbo.NisSystemConfig ADD ChecklistByTicketTypeJson nvarchar(max) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.NisSystemConfig')
          AND name = N'ChecklistByCustomerJson'
    )
    BEGIN
        ALTER TABLE dbo.NisSystemConfig ADD ChecklistByCustomerJson nvarchar(max) NULL;
    END;
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน เฉพาะ 2 คอลัมน์ใหม่):
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN ChecklistByCustomerJson;
--   ALTER TABLE dbo.NisSystemConfig DROP COLUMN ChecklistByTicketTypeJson;
-- (ถ้าต้องถอนทั้งตารางที่เพิ่งสร้าง: DROP TABLE IF EXISTS dbo.NisSystemConfig;)
-- ============================================================
