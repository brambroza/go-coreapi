-- Outgoing email audit log. NIS Onsite initially records failed attempts only.
-- Idempotent: safe to run more than once. Apply with the team's normal migration process.

IF OBJECT_ID('dbo.EmailSendLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailSendLog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_EmailSendLog PRIMARY KEY,
        Source NVARCHAR(100) NOT NULL,
        CmpId NVARCHAR(100) NOT NULL,
        RecipientEmail NVARCHAR(320) NOT NULL,
        Subject NVARCHAR(500) NOT NULL,
        Provider NVARCHAR(30) NOT NULL,
        IsSuccess BIT NOT NULL,
        ErrorMessage NVARCHAR(4000) NULL,
        ErrorDetail NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_EmailSendLog_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_EmailSendLog_CmpId_CreatedAt
        ON dbo.EmailSendLog (CmpId, CreatedAt);
END;
