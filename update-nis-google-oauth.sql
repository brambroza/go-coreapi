-- Replace REPLACE_WITH_CMP_ID before running.
-- The client secret below is AES-encrypted with the current EmailCrypto:KeyBase64.
DECLARE @CmpId NVARCHAR(100) = N'REPLACE_WITH_CMP_ID';
DECLARE @SettingName NVARCHAR(100) = N'nis';

UPDATE dbo.EmailSmtpSettings
SET GoogleOAuthClientId = N'381112939518-mqmasrsvpmd8dh122vccvf8k79ild9m8.apps.googleusercontent.com',
    GoogleOAuthClientSecretEnc = 0x4AE88C89E75B6ED3F8CFF225B09ABD27A1228C17DB5AC589F3B12E79DC820C29CE1191B7DBE2C6B4D5480E491E5D92B0,
    GoogleOAuthClientSecretIv = 0x3D187878123A79F393D8D9A1552F997A,
    UpdatedAt = SYSUTCDATETIME()
WHERE CmpId = @CmpId
  AND SettingName = @SettingName;

IF @@ROWCOUNT <> 1
    THROW 50000, 'Expected exactly one EmailSmtpSettings row.', 1;
