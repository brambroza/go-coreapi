using System.Data;
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
IF EXISTS (SELECT 1 FROM dbo.EmailSmtpSettings WHERE CmpId = @CmpId AND SettingName = @SettingName)
BEGIN
    UPDATE dbo.EmailSmtpSettings
    SET FromEmail=@FromEmail, FromName=@FromName,
        SmtpHost=@SmtpHost, SmtpPort=@SmtpPort, EnableSsl=@EnableSsl,
        Username=@Username,
        PasswordEnc=@PasswordEnc, PasswordIv=@PasswordIv,
        IsActive=@IsActive,
        UpdatedAt=SYSUTCDATETIME()
    WHERE CmpId=@CmpId AND SettingName=@SettingName;
END
ELSE
BEGIN
    INSERT INTO dbo.EmailSmtpSettings
    (CmpId, SettingName, FromEmail, FromName, SmtpHost, SmtpPort, EnableSsl, Username, PasswordEnc, PasswordIv, IsActive)
    VALUES
    (@CmpId, @SettingName, @FromEmail, @FromName, @SmtpHost, @SmtpPort, @EnableSsl, @Username, @PasswordEnc, @PasswordIv, @IsActive);
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

        cmd.Parameters.AddWithValue("@IsActive", setting.IsActive);

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
            Id = (int)rd["Id"],
            CmpId = rd["CmpId"] as string,
            SettingName = (string)rd["SettingName"],
            FromEmail = (string)rd["FromEmail"],
            FromName = rd["FromName"] as string,
            SmtpHost = (string)rd["SmtpHost"],
            SmtpPort = (int)rd["SmtpPort"],
            EnableSsl = (bool)rd["EnableSsl"],
            Username = (string)rd["Username"],
            PasswordEnc = (byte[])rd["PasswordEnc"],
            PasswordIv = (byte[])rd["PasswordIv"],
            IsActive = (bool)rd["IsActive"],
            UpdatedAt = (DateTime)rd["UpdatedAt"],
            CreatedAt = (DateTime)rd["CreatedAt"],
        };
    }
}
