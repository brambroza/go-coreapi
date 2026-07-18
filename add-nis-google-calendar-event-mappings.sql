IF OBJECT_ID('dbo.NisGoogleCalendarEventMappings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisGoogleCalendarEventMappings
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CmpId NVARCHAR(100) NULL,
        SettingName NVARCHAR(100) NOT NULL,
        TicketId NVARCHAR(100) NOT NULL,
        GoogleEventId NVARCHAR(255) NOT NULL,
        CalendarId NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NisGoogleCalendarEventMappings_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_NisGoogleCalendarEventMappings_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_NisGoogleCalendarEventMappings_Ticket
        ON dbo.NisGoogleCalendarEventMappings (CmpId, SettingName, TicketId);
END
