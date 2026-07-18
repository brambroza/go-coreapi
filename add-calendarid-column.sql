-- Run manually against the SQL Server DB used by coreapi-26v1.
-- Adds the column that stores which Google Calendar ID to sync per mail setting.
ALTER TABLE dbo.EmailSmtpSettings ADD CalendarId NVARCHAR(255) NULL;
