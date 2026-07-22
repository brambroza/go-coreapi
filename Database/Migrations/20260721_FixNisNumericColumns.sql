SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.NisProject WHERE TRY_CONVERT(int, Progress) IS NULL)
    THROW 50002, 'dbo.NisProject.Progress contains a value that cannot be converted to int.', 1;

IF EXISTS (SELECT 1 FROM dbo.NisTicket WHERE TRY_CONVERT(int, Pct) IS NULL)
    THROW 50003, 'dbo.NisTicket.Pct contains a value that cannot be converted to int.', 1;

UPDATE dbo.NisProject SET Progress = '0' WHERE Progress IS NULL;
UPDATE dbo.NisTicket SET Pct = '0' WHERE Pct IS NULL;

IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisProject')
      AND name = N'Progress'
      AND system_type_id <> 56
)
    ALTER TABLE dbo.NisProject ALTER COLUMN Progress int NOT NULL;

IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NisTicket')
      AND name = N'Pct'
      AND system_type_id <> 56
)
    ALTER TABLE dbo.NisTicket ALTER COLUMN Pct int NOT NULL;

COMMIT TRANSACTION;
