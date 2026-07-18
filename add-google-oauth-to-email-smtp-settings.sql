-- Run once before deploying the API change.
-- Google OAuth client secrets are stored encrypted by AesCrypto, never as plaintext.
IF COL_LENGTH('dbo.EmailSmtpSettings', 'GoogleOAuthClientId') IS NULL
    ALTER TABLE dbo.EmailSmtpSettings ADD GoogleOAuthClientId NVARCHAR(255) NULL;

IF COL_LENGTH('dbo.EmailSmtpSettings', 'GoogleOAuthClientSecretEnc') IS NULL
    ALTER TABLE dbo.EmailSmtpSettings ADD GoogleOAuthClientSecretEnc VARBINARY(MAX) NULL;

IF COL_LENGTH('dbo.EmailSmtpSettings', 'GoogleOAuthClientSecretIv') IS NULL
    ALTER TABLE dbo.EmailSmtpSettings ADD GoogleOAuthClientSecretIv VARBINARY(32) NULL;

IF COL_LENGTH('dbo.EmailSmtpSettings', 'GoogleOAuthRefreshTokenEnc') IS NULL
    ALTER TABLE dbo.EmailSmtpSettings ADD GoogleOAuthRefreshTokenEnc VARBINARY(MAX) NULL;

IF COL_LENGTH('dbo.EmailSmtpSettings', 'GoogleOAuthRefreshTokenIv') IS NULL
    ALTER TABLE dbo.EmailSmtpSettings ADD GoogleOAuthRefreshTokenIv VARBINARY(32) NULL;
