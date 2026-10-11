-- ============================================================
-- Migration: NIS Service Report — ข้อมูลบริษัทผู้ออกรายงาน + เซลติดต่อ (JSON ก้อนเดียวต่อบริษัท)
--   dbo.NisReportCompany  InfoJson = ส่วนเสริมของ CompanyInfo (lineQrPath + salesContacts) — ชื่อ/ที่อยู่/โทร/อีเมล
--                         อ่านจาก dbo.getcmpinfo ของระบบหลักตอนใช้งาน ไม่เก็บซ้ำที่นี่ (snapshot ของรายงาน freeze ก้อนที่รวมแล้ว)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisReportCompany
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisReportCompany', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisReportCompany
    (
        CmpId     nvarchar(50)  NOT NULL CONSTRAINT PK_NisReportCompany PRIMARY KEY,
        InfoJson  nvarchar(max) NOT NULL CONSTRAINT DF_NisReportCompany_InfoJson DEFAULT ('{}'),
        UpdatedAt datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReportCompany_UpdatedAt DEFAULT (SYSDATETIMEOFFSET()),
        UpdatedBy nvarchar(100) NOT NULL CONSTRAINT DF_NisReportCompany_UpdatedBy DEFAULT (''),
        CONSTRAINT CK_NisReportCompany_InfoJson CHECK (ISJSON(InfoJson) = 1)
    );
END;

COMMIT TRANSACTION;

-- Rollback (manual): DROP TABLE IF EXISTS dbo.NisReportCompany;
