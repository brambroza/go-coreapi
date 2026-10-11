-- ============================================================
-- Migration: NIS Service Report — archive ของเคสงานบริการ + log รอบ sync
--   dbo.NisServiceCase  คัดลอกจาก SP getProblem* โดย NisArchiveSyncService ทุก 15 นาที
--   dbo.NisSyncLog      บันทึกรอบ sync — dataAsOf ของรายงานคำนวณจากตารางนี้
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisServiceCase / NisSyncLog
-- คอลัมน์เวลาเป็น datetimeoffset(3) ทั้งหมด (เวลาไทย +07:00 เก็บ offset จริง ไม่เลื่อนวัน)
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisServiceCase', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisServiceCase
    (
        CmpId               nvarchar(50)   NOT NULL,
        SourceId            nvarchar(50)   NOT NULL,
        CaseNo              nvarchar(50)   NOT NULL CONSTRAINT DF_NisServiceCase_CaseNo DEFAULT (''),
        ExternalGuid        nvarchar(100)  NOT NULL CONSTRAINT DF_NisServiceCase_ExternalGuid DEFAULT (''),
        CustomerId          nvarchar(50)   NOT NULL CONSTRAINT DF_NisServiceCase_CustomerId DEFAULT (''),
        CustomerName        nvarchar(300)  NOT NULL CONSTRAINT DF_NisServiceCase_CustomerName DEFAULT (''),
        BranchName          nvarchar(300)  NULL,
        Province            nvarchar(200)  NULL,
        ContactName         nvarchar(200)  NULL,
        RequestBy           nvarchar(200)  NULL,
        RequestPhone        nvarchar(100)  NULL,
        RequestEmail        nvarchar(200)  NULL,
        RequestPosition     nvarchar(200)  NULL,
        Subject             nvarchar(max)  NOT NULL CONSTRAINT DF_NisServiceCase_Subject DEFAULT (''),
        Category            nvarchar(200)  NOT NULL CONSTRAINT DF_NisServiceCase_Category DEFAULT (''),
        Priority            nvarchar(20)   NOT NULL CONSTRAINT DF_NisServiceCase_Priority DEFAULT ('minor'),
        Status              nvarchar(20)   NOT NULL CONSTRAINT DF_NisServiceCase_Status DEFAULT ('new'),
        Remark              nvarchar(max)  NULL,
        CreatedAt           datetimeoffset(3) NOT NULL,
        StartedAt           datetimeoffset(3) NULL,
        ResolvedAt          datetimeoffset(3) NULL,
        ModifiedAt          datetimeoffset(3) NULL,
        ResolutionHours     float          NULL,
        AssigneeIdsJson     nvarchar(max)  NOT NULL CONSTRAINT DF_NisServiceCase_AssigneeIdsJson DEFAULT ('[]'),
        AssigneeNamesJson   nvarchar(max)  NOT NULL CONSTRAINT DF_NisServiceCase_AssigneeNamesJson DEFAULT ('[]'),
        PrimaryAssignee     nvarchar(200)  NULL,
        ActionDetails       nvarchar(max)  NULL,
        ActionByNamesJson   nvarchar(max)  NOT NULL CONSTRAINT DF_NisServiceCase_ActionByNamesJson DEFAULT ('[]'),
        ServiceTypeCode     int            NULL,
        SatisfactionScore   int            NULL,
        SatisfactionComment nvarchar(max)  NULL,
        ViaLine             bit            NOT NULL CONSTRAINT DF_NisServiceCase_ViaLine DEFAULT (0),
        AnydeskId           nvarchar(100)  NULL,
        AttachmentCount     int            NOT NULL CONSTRAINT DF_NisServiceCase_AttachmentCount DEFAULT (0),
        IsTestData          bit            NOT NULL CONSTRAINT DF_NisServiceCase_IsTestData DEFAULT (0),
        RawJson             nvarchar(max)  NOT NULL CONSTRAINT DF_NisServiceCase_RawJson DEFAULT ('{}'),
        SyncedAt            datetimeoffset(3) NOT NULL CONSTRAINT DF_NisServiceCase_SyncedAt DEFAULT (SYSDATETIMEOFFSET()),
        CONSTRAINT PK_NisServiceCase PRIMARY KEY (CmpId, SourceId),
        CONSTRAINT CK_NisServiceCase_RawJson CHECK (ISJSON(RawJson) = 1)
    );

    CREATE UNIQUE INDEX UX_NisServiceCase_Cmp_ExternalGuid ON dbo.NisServiceCase (CmpId, ExternalGuid);
    CREATE INDEX IX_NisServiceCase_Cmp_Customer_CreatedAt ON dbo.NisServiceCase (CmpId, CustomerId, CreatedAt);
    CREATE INDEX IX_NisServiceCase_Cmp_CreatedAt  ON dbo.NisServiceCase (CmpId, CreatedAt);
    CREATE INDEX IX_NisServiceCase_Cmp_ResolvedAt ON dbo.NisServiceCase (CmpId, ResolvedAt);
    CREATE INDEX IX_NisServiceCase_Cmp_Status     ON dbo.NisServiceCase (CmpId, Status);
    CREATE INDEX IX_NisServiceCase_Cmp_Priority   ON dbo.NisServiceCase (CmpId, Priority);
    CREATE INDEX IX_NisServiceCase_Cmp_IsTestData ON dbo.NisServiceCase (CmpId, IsTestData);
END;

IF OBJECT_ID(N'dbo.NisSyncLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisSyncLog
    (
        Id             uniqueidentifier NOT NULL CONSTRAINT PK_NisSyncLog PRIMARY KEY DEFAULT NEWID(),
        CmpId          nvarchar(50)  NOT NULL,
        StartedAt      datetimeoffset(3) NOT NULL,
        FinishedAt     datetimeoffset(3) NULL,
        Status         nvarchar(20)  NOT NULL CONSTRAINT DF_NisSyncLog_Status DEFAULT ('running'),
        [Trigger]      nvarchar(20)  NOT NULL CONSTRAINT DF_NisSyncLog_Trigger DEFAULT ('cron'),
        FetchedCount   int NULL,
        ParsedCount    int NULL,
        InvalidCount   int NULL,
        InsertedCount  int NULL,
        UpdatedCount   int NULL,
        UnchangedCount int NULL,
        MissingCount   int NULL,
        DurationMs     int NULL,
        Error          nvarchar(max) NULL
    );

    CREATE INDEX IX_NisSyncLog_Cmp_StartedAt ON dbo.NisSyncLog (CmpId, StartedAt);
    CREATE INDEX IX_NisSyncLog_Cmp_Status_StartedAt ON dbo.NisSyncLog (CmpId, Status, StartedAt);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   DROP TABLE IF EXISTS dbo.NisSyncLog;
--   DROP TABLE IF EXISTS dbo.NisServiceCase;
-- ============================================================
