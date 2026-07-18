-- Adds persisted Service Report PDF reference columns to dbo.NisOnsiteReport.
-- The PDF blob itself is written to disk (NisOnsite:ReportPdfDir); only a reference is kept
-- in-row so the customer's signed report can be re-sent/audited without bloating the table.
-- Idempotent: safe to run more than once. Apply with the team's normal migration process.

IF COL_LENGTH('dbo.NisOnsiteReport', 'ReportPdfPath') IS NULL
BEGIN
    ALTER TABLE dbo.NisOnsiteReport ADD ReportPdfPath NVARCHAR(400) NULL;
END;

IF COL_LENGTH('dbo.NisOnsiteReport', 'ReportPdfSize') IS NULL
BEGIN
    ALTER TABLE dbo.NisOnsiteReport ADD ReportPdfSize BIGINT NULL;
END;

IF COL_LENGTH('dbo.NisOnsiteReport', 'ReportPdfSha256') IS NULL
BEGIN
    ALTER TABLE dbo.NisOnsiteReport ADD ReportPdfSha256 NVARCHAR(64) NULL;
END;
