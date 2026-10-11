-- ============================================================
-- Migration: NIS Service Report — วันหยุดที่หักจากเวลาทำการ + ประวัติการแก้
--   dbo.NisHoliday           วันหยุดต่อบริษัท · Observed=1 เท่านั้นที่ถูกหักจากเวลาทำการ
--   dbo.NisHolidayChangeLog  ใครแก้อะไรจากอะไรเป็นอะไรเมื่อไร
--
-- โครงสร้างตรงกับ EF entity goalongapi.Models.Nis.NisHoliday / NisHolidayChangeLog
-- Date เก็บเป็น char 'YYYY-MM-DD' ตั้งใจ — ไม่ให้ timezone เลื่อนวัน
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisHoliday', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisHoliday
    (
        CmpId    nvarchar(50)  NOT NULL,
        [Date]   nvarchar(10)  NOT NULL,
        [Year]   int           NOT NULL,
        NameTh   nvarchar(200) NOT NULL CONSTRAINT DF_NisHoliday_NameTh DEFAULT (''),
        Kind     nvarchar(20)  NOT NULL CONSTRAINT DF_NisHoliday_Kind DEFAULT ('government'),
        Verified bit           NOT NULL CONSTRAINT DF_NisHoliday_Verified DEFAULT (0),
        Observed bit           NOT NULL CONSTRAINT DF_NisHoliday_Observed DEFAULT (0),
        Note     nvarchar(500) NULL,
        CONSTRAINT PK_NisHoliday PRIMARY KEY (CmpId, [Date])
    );

    CREATE INDEX IX_NisHoliday_Cmp_Year     ON dbo.NisHoliday (CmpId, [Year]);
    CREATE INDEX IX_NisHoliday_Cmp_Observed ON dbo.NisHoliday (CmpId, Observed);
END;

IF OBJECT_ID(N'dbo.NisHolidayChangeLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisHolidayChangeLog
    (
        Id        uniqueidentifier NOT NULL CONSTRAINT PK_NisHolidayChangeLog PRIMARY KEY DEFAULT NEWID(),
        CmpId     nvarchar(50)  NOT NULL,
        [Date]    nvarchar(10)  NOT NULL,
        Field     nvarchar(30)  NOT NULL,
        OldValue  nvarchar(500) NULL,
        NewValue  nvarchar(500) NULL,
        ChangedBy nvarchar(100) NOT NULL CONSTRAINT DF_NisHolidayChangeLog_ChangedBy DEFAULT (''),
        ChangedAt datetimeoffset(3) NOT NULL CONSTRAINT DF_NisHolidayChangeLog_ChangedAt DEFAULT (SYSDATETIMEOFFSET())
    );

    CREATE INDEX IX_NisHolidayChangeLog_Cmp_Date      ON dbo.NisHolidayChangeLog (CmpId, [Date]);
    CREATE INDEX IX_NisHolidayChangeLog_Cmp_ChangedAt ON dbo.NisHolidayChangeLog (CmpId, ChangedAt);
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual):
--   DROP TABLE IF EXISTS dbo.NisHolidayChangeLog;
--   DROP TABLE IF EXISTS dbo.NisHoliday;
-- ============================================================
