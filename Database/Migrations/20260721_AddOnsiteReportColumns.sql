SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.ServiceTicketSubTaskAction', N'SrNumber') IS NULL
BEGIN
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD SrNumber nvarchar(50) NULL;
END;

IF COL_LENGTH(N'dbo.ServiceTicketSubTaskAction', N'SignatureImageBase64') IS NULL
BEGIN
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD SignatureImageBase64 nvarchar(max) NULL;
END;

IF COL_LENGTH(N'dbo.ServiceTicketSubTaskAction', N'WorkPhotosJson') IS NULL
BEGIN
    ALTER TABLE dbo.ServiceTicketSubTaskAction
        ADD WorkPhotosJson nvarchar(max) NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ServiceTicketSubTaskAction')
      AND name = N'IX_ServiceTicketSubTaskAction_CmpId_SrNumber'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE INDEX IX_ServiceTicketSubTaskAction_CmpId_SrNumber
            ON dbo.ServiceTicketSubTaskAction (CmpId, SrNumber)
            WHERE SrNumber IS NOT NULL;';
END;

COMMIT TRANSACTION;
