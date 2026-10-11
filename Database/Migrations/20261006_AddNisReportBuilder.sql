-- ============================================================
-- Migration: NIS Service Report — Report Builder
--   dbo.NisReportTemplate  Master Template ต่อลูกค้า (LayoutJson = TemplateSchema ทั้งฉบับ) · ไม่มี DELETE ใช้ ArchivedAt
--   dbo.NisReport          รายงานหนึ่งฉบับ · LayoutSnapshotJson + DataSnapshotJson + PdfBlob (เปิดซ้ำได้เลขเดิม)
--   dbo.NisReportJob       คิวเรนเดอร์ PDF ในฐาน (lease + stage + เหตุผลที่ล้มอ่านออก)
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisReportTemplate / NisReport / NisReportJob
-- FK ทั้งสองเป็น NO ACTION — ห้ามลบ template/report ที่มีของอ้างถึง
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisReportTemplate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisReportTemplate
    (
        Id         uniqueidentifier NOT NULL CONSTRAINT PK_NisReportTemplate PRIMARY KEY DEFAULT NEWID(),
        CmpId      nvarchar(50)  NOT NULL,
        CustomerId nvarchar(50)  NOT NULL,
        Name       nvarchar(120) NOT NULL,
        DocKind    nvarchar(20)  NOT NULL CONSTRAINT DF_NisReportTemplate_DocKind DEFAULT ('book'),
        LayoutJson nvarchar(max) NOT NULL CONSTRAINT DF_NisReportTemplate_LayoutJson DEFAULT ('{}'),
        Version    int           NOT NULL CONSTRAINT DF_NisReportTemplate_Version DEFAULT (1),
        IsDefault  bit           NOT NULL CONSTRAINT DF_NisReportTemplate_IsDefault DEFAULT (0),
        ArchivedAt datetimeoffset(3) NULL,
        CreatedAt  datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReportTemplate_CreatedAt DEFAULT (SYSDATETIMEOFFSET()),
        UpdatedAt  datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReportTemplate_UpdatedAt DEFAULT (SYSDATETIMEOFFSET()),
        CreatedBy  nvarchar(100) NOT NULL CONSTRAINT DF_NisReportTemplate_CreatedBy DEFAULT (''),
        UpdatedBy  nvarchar(100) NOT NULL CONSTRAINT DF_NisReportTemplate_UpdatedBy DEFAULT (''),
        CONSTRAINT CK_NisReportTemplate_LayoutJson CHECK (ISJSON(LayoutJson) = 1)
    );

    CREATE UNIQUE INDEX UX_NisReportTemplate_Cmp_Customer_Name ON dbo.NisReportTemplate (CmpId, CustomerId, Name);
    CREATE INDEX IX_NisReportTemplate_Cmp_Customer   ON dbo.NisReportTemplate (CmpId, CustomerId);
    CREATE INDEX IX_NisReportTemplate_Cmp_ArchivedAt ON dbo.NisReportTemplate (CmpId, ArchivedAt);
END;

IF OBJECT_ID(N'dbo.NisReport', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisReport
    (
        Id                 uniqueidentifier NOT NULL CONSTRAINT PK_NisReport PRIMARY KEY DEFAULT NEWID(),
        CmpId              nvarchar(50)  NOT NULL,
        TemplateId         uniqueidentifier NOT NULL,
        TemplateVersion    int           NOT NULL,
        CustomerId         nvarchar(50)  NOT NULL,
        CustomerName       nvarchar(300) NOT NULL CONSTRAINT DF_NisReport_CustomerName DEFAULT (''),
        IssueNo            int           NULL,
        PeriodStart        datetimeoffset(3) NOT NULL,
        PeriodEnd          datetimeoffset(3) NOT NULL,
        LayoutSnapshotJson nvarchar(max) NOT NULL CONSTRAINT DF_NisReport_LayoutSnapshotJson DEFAULT ('{}'),
        DataSnapshotJson   nvarchar(max) NOT NULL CONSTRAINT DF_NisReport_DataSnapshotJson DEFAULT ('{}'),
        Status             nvarchar(20)  NOT NULL CONSTRAINT DF_NisReport_Status DEFAULT ('draft'),
        PdfBlob            varbinary(max) NULL,
        PdfSha256          nvarchar(64)  NULL,
        PdfPath            nvarchar(500) NULL,
        PrintKeyHash       nvarchar(64)  NULL,
        PrintKeyExpiresAt  datetimeoffset(3) NULL,
        CreatedAt          datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReport_CreatedAt DEFAULT (SYSDATETIMEOFFSET()),
        UpdatedAt          datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReport_UpdatedAt DEFAULT (SYSDATETIMEOFFSET()),
        CreatedBy          nvarchar(100) NOT NULL CONSTRAINT DF_NisReport_CreatedBy DEFAULT (''),
        CONSTRAINT FK_NisReport_Template FOREIGN KEY (TemplateId) REFERENCES dbo.NisReportTemplate (Id) ON DELETE NO ACTION,
        CONSTRAINT CK_NisReport_LayoutSnapshotJson CHECK (ISJSON(LayoutSnapshotJson) = 1),
        CONSTRAINT CK_NisReport_DataSnapshotJson   CHECK (ISJSON(DataSnapshotJson) = 1)
    );

    CREATE INDEX IX_NisReport_Cmp_Customer_PeriodStart ON dbo.NisReport (CmpId, CustomerId, PeriodStart);
    CREATE INDEX IX_NisReport_Cmp_Template ON dbo.NisReport (CmpId, TemplateId);
    CREATE INDEX IX_NisReport_Cmp_Status   ON dbo.NisReport (CmpId, Status);
END;

IF OBJECT_ID(N'dbo.NisReportJob', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisReportJob
    (
        Id         uniqueidentifier NOT NULL CONSTRAINT PK_NisReportJob PRIMARY KEY DEFAULT NEWID(),
        CmpId      nvarchar(50)  NOT NULL,
        ReportId   uniqueidentifier NOT NULL,
        Status     nvarchar(20)  NOT NULL CONSTRAINT DF_NisReportJob_Status DEFAULT ('queued'),
        Stage      nvarchar(20)  NULL,
        Attempts   int           NOT NULL CONSTRAINT DF_NisReportJob_Attempts DEFAULT (0),
        LeaseUntil datetimeoffset(3) NULL,
        WorkerId   nvarchar(100) NULL,
        Error      nvarchar(max) NULL,
        PdfPath    nvarchar(500) NULL,
        PdfBytes   int           NULL,
        RenderMs   int           NULL,
        CreatedAt  datetimeoffset(3) NOT NULL CONSTRAINT DF_NisReportJob_CreatedAt DEFAULT (SYSDATETIMEOFFSET()),
        StartedAt  datetimeoffset(3) NULL,
        FinishedAt datetimeoffset(3) NULL,
        CONSTRAINT FK_NisReportJob_Report FOREIGN KEY (ReportId) REFERENCES dbo.NisReport (Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_NisReportJob_Status_CreatedAt ON dbo.NisReportJob (Status, CreatedAt);
    CREATE INDEX IX_NisReportJob_Cmp_Report ON dbo.NisReportJob (CmpId, ReportId);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ลำดับตาม FK):
--   DROP TABLE IF EXISTS dbo.NisReportJob;
--   DROP TABLE IF EXISTS dbo.NisReport;
--   DROP TABLE IF EXISTS dbo.NisReportTemplate;
-- ============================================================
