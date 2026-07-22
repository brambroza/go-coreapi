using System.Data;
using System.Globalization;
using goalongapi.Models;
using Microsoft.Data.SqlClient;

public class EmailSettingRepository
{
    private readonly string _connStr;

    public EmailSettingRepository(string connStr)
    {
        _connStr = connStr;
    }

    public async Task UpsertAsync(EmailSmtpSetting setting, bool updatePassword)
    {
        const string sql = @"
IF EXISTS (
    SELECT 1 FROM dbo.EmailSmtpSettings
    WHERE SettingName = @SettingName
      AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId)
)
BEGIN
    UPDATE dbo.EmailSmtpSettings
    SET FromEmail=@FromEmail, FromName=@FromName,
        SmtpHost=@SmtpHost, SmtpPort=@SmtpPort, EnableSsl=@EnableSsl,
        Username=@Username,
        PasswordEnc=CASE WHEN @UpdatePassword = 1 THEN @PasswordEnc ELSE PasswordEnc END,
        PasswordIv=CASE WHEN @UpdatePassword = 1 THEN @PasswordIv ELSE PasswordIv END,
        IsActive=@IsActive,
        CalendarId=@CalendarId,
        GoogleOAuthClientId=@GoogleOAuthClientId,
        GoogleOAuthClientSecretEnc=CASE WHEN @UpdateGoogleOAuthSecret = 1 THEN @GoogleOAuthClientSecretEnc ELSE GoogleOAuthClientSecretEnc END,
        GoogleOAuthClientSecretIv=CASE WHEN @UpdateGoogleOAuthSecret = 1 THEN @GoogleOAuthClientSecretIv ELSE GoogleOAuthClientSecretIv END,
        UpdatedAt=SYSUTCDATETIME()
    WHERE SettingName=@SettingName
      AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId);
END
ELSE
BEGIN
    INSERT INTO dbo.EmailSmtpSettings
    (CmpId, SettingName, FromEmail, FromName, SmtpHost, SmtpPort, EnableSsl, Username, PasswordEnc, PasswordIv, IsActive, CalendarId, GoogleOAuthClientId, GoogleOAuthClientSecretEnc, GoogleOAuthClientSecretIv)
    VALUES
    (@CmpId, @SettingName, @FromEmail, @FromName, @SmtpHost, @SmtpPort, @EnableSsl, @Username, @PasswordEnc, @PasswordIv, @IsActive, @CalendarId, @GoogleOAuthClientId, @GoogleOAuthClientSecretEnc, @GoogleOAuthClientSecretIv);
END
";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CmpId", (object?)setting.CmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", setting.SettingName);
        cmd.Parameters.AddWithValue("@FromEmail", setting.FromEmail);
        cmd.Parameters.AddWithValue("@FromName", (object?)setting.FromName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SmtpHost", setting.SmtpHost);
        cmd.Parameters.AddWithValue("@SmtpPort", setting.SmtpPort);
        cmd.Parameters.AddWithValue("@EnableSsl", setting.EnableSsl);
        cmd.Parameters.AddWithValue("@Username", setting.Username);

        cmd.Parameters.Add("@PasswordEnc", SqlDbType.VarBinary, -1).Value = setting.PasswordEnc;
        cmd.Parameters.Add("@PasswordIv", SqlDbType.VarBinary, 32).Value = setting.PasswordIv;
        cmd.Parameters.AddWithValue("@UpdatePassword", updatePassword);

        cmd.Parameters.AddWithValue("@IsActive", setting.IsActive);
        cmd.Parameters.AddWithValue("@CalendarId", (object?)setting.CalendarId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoogleOAuthClientId", (object?)setting.GoogleOAuthClientId ?? DBNull.Value);
        cmd.Parameters.Add("@GoogleOAuthClientSecretEnc", SqlDbType.VarBinary, -1).Value = setting.GoogleOAuthClientSecretEnc;
        cmd.Parameters.Add("@GoogleOAuthClientSecretIv", SqlDbType.VarBinary, 32).Value = setting.GoogleOAuthClientSecretIv;
        cmd.Parameters.AddWithValue("@UpdateGoogleOAuthSecret", setting.GoogleOAuthClientSecretEnc.Length > 0);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<EmailSmtpSetting?> GetActiveAsync(string? cmpId, string settingName = "default")
    {
        const string sql = @"
SELECT TOP 1 *
FROM dbo.EmailSmtpSettings
WHERE IsActive = 1
  AND SettingName = @SettingName
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR (CmpId = @CmpId))
ORDER BY UpdatedAt DESC;
";
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CmpId", (object?)cmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", settingName);

        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;

        return new EmailSmtpSetting
        {
            Id = Convert.ToInt32(rd["Id"], CultureInfo.InvariantCulture),
            CmpId = rd["CmpId"] as string,
            SettingName = (string)rd["SettingName"],
            FromEmail = (string)rd["FromEmail"],
            FromName = rd["FromName"] as string,
            SmtpHost = (string)rd["SmtpHost"],
            SmtpPort = Convert.ToInt32(rd["SmtpPort"], CultureInfo.InvariantCulture),
            EnableSsl = (bool)rd["EnableSsl"],
            Username = (string)rd["Username"],
            PasswordEnc = (byte[])rd["PasswordEnc"],
            PasswordIv = (byte[])rd["PasswordIv"],
            IsActive = (bool)rd["IsActive"],
            UpdatedAt = (DateTime)rd["UpdatedAt"],
            CreatedAt = (DateTime)rd["CreatedAt"],
            CalendarId = rd["CalendarId"] as string,
            GoogleOAuthClientId = rd["GoogleOAuthClientId"] as string,
            GoogleOAuthClientSecretEnc = rd["GoogleOAuthClientSecretEnc"] as byte[] ?? Array.Empty<byte>(),
            GoogleOAuthClientSecretIv = rd["GoogleOAuthClientSecretIv"] as byte[] ?? Array.Empty<byte>(),
            GoogleOAuthRefreshTokenEnc = rd["GoogleOAuthRefreshTokenEnc"] as byte[] ?? Array.Empty<byte>(),
            GoogleOAuthRefreshTokenIv = rd["GoogleOAuthRefreshTokenIv"] as byte[] ?? Array.Empty<byte>(),
        };
    }

    public async Task<bool> UpdateGoogleOAuthAsync(
        string? cmpId,
        string settingName,
        string clientId,
        byte[] clientSecretEnc,
        byte[] clientSecretIv)
    {
        const string sql = @"
UPDATE dbo.EmailSmtpSettings
SET GoogleOAuthClientId = @GoogleOAuthClientId,
    GoogleOAuthClientSecretEnc = @GoogleOAuthClientSecretEnc,
    GoogleOAuthClientSecretIv = @GoogleOAuthClientSecretIv,
    UpdatedAt = SYSUTCDATETIME()
WHERE SettingName = @SettingName
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId);";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CmpId", (object?)cmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", settingName);
        cmd.Parameters.AddWithValue("@GoogleOAuthClientId", clientId);
        cmd.Parameters.Add("@GoogleOAuthClientSecretEnc", SqlDbType.VarBinary, -1).Value = clientSecretEnc;
        cmd.Parameters.Add("@GoogleOAuthClientSecretIv", SqlDbType.VarBinary, 32).Value = clientSecretIv;

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateGoogleOAuthRefreshTokenAsync(string? cmpId, string settingName, byte[] refreshTokenEnc, byte[] refreshTokenIv)
    {
        const string sql = @"
UPDATE dbo.EmailSmtpSettings
SET GoogleOAuthRefreshTokenEnc = @GoogleOAuthRefreshTokenEnc,
    GoogleOAuthRefreshTokenIv = @GoogleOAuthRefreshTokenIv,
    UpdatedAt = SYSUTCDATETIME()
WHERE SettingName = @SettingName
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId);";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CmpId", (object?)cmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", settingName);
        cmd.Parameters.Add("@GoogleOAuthRefreshTokenEnc", SqlDbType.VarBinary, -1).Value = refreshTokenEnc;
        cmd.Parameters.Add("@GoogleOAuthRefreshTokenIv", SqlDbType.VarBinary, 32).Value = refreshTokenIv;
        return await cmd.ExecuteNonQueryAsync() == 1;
    }
}
