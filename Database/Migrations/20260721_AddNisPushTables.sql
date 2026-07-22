SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisPushToken', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisPushToken
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NisPushToken PRIMARY KEY,
        CmpId nvarchar(100) NOT NULL,
        StaffName nvarchar(200) NOT NULL,
        UserId nvarchar(100) NULL,
        ExpoPushToken nvarchar(255) NOT NULL,
        DeviceId nvarchar(255) NOT NULL,
        Platform nvarchar(20) NULL,
        AppVersion nvarchar(50) NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.NisPushToken')
      AND name = N'UX_NisPushToken_CmpId_StaffName_DeviceId'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE UNIQUE INDEX UX_NisPushToken_CmpId_StaffName_DeviceId
            ON dbo.NisPushToken (CmpId, StaffName, DeviceId);';
END;

IF OBJECT_ID(N'dbo.NisPushLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisPushLog
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NisPushLog PRIMARY KEY,
        EventKey nvarchar(255) NOT NULL,
        CmpId nvarchar(100) NULL,
        TicketId nvarchar(100) NULL,
        StaffName nvarchar(200) NULL,
        Title nvarchar(255) NULL,
        Body nvarchar(500) NULL,
        SentAt datetime2 NOT NULL
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.NisPushLog')
      AND name = N'UX_NisPushLog_EventKey'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE UNIQUE INDEX UX_NisPushLog_EventKey
            ON dbo.NisPushLog (EventKey);';
END;

COMMIT TRANSACTION;
