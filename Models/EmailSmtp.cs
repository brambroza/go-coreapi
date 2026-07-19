

namespace goalongapi.Models;

public class EmailSmtpSetting
{
    public int Id { get; set; }
    public string? CmpId { get; set; }
    public string SettingName { get; set; } = "default";

    public string FromEmail { get; set; } = "";
    public string? FromName { get; set; }

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = "";

    public byte[] PasswordEnc { get; set; } = Array.Empty<byte>();
    public byte[] PasswordIv { get; set; } = Array.Empty<byte>();

    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Google Calendar ID (usually the calendar owner's email) synced via
    // GoogleCalendarApiKeyClient. Requires the calendar's sharing setting to
    // be public, since only an API key (no per-user OAuth) is used to read it.
    public string? CalendarId { get; set; }

    // OAuth client settings used to obtain a Gmail access/refresh token.  The
    // client secret is encrypted before it is persisted.
    public string? GoogleOAuthClientId { get; set; }
    public byte[] GoogleOAuthClientSecretEnc { get; set; } = Array.Empty<byte>();
    public byte[] GoogleOAuthClientSecretIv { get; set; } = Array.Empty<byte>();
    public byte[] GoogleOAuthRefreshTokenEnc { get; set; } = Array.Empty<byte>();
    public byte[] GoogleOAuthRefreshTokenIv { get; set; } = Array.Empty<byte>();
}

public class EmailSmtpSettingUpsertDto
{
    public string? CmpId { get; set; }
    public string SettingName { get; set; } = "default";
    public string FromEmail { get; set; } = "";
    public string? FromName { get; set; }
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string AppPasswordPlain { get; set; } = ""; // รับจาก UI แล้วเข้ารหัสก่อนเก็บ
    public string? CalendarId { get; set; }

    // These are optional because normal SMTP/app-password settings continue to
    // work without Google OAuth being configured.
    public string? GoogleOAuthClientId { get; set; }
    public string? GoogleOAuthClientSecret { get; set; }
}


public class EmailSmtpSettingViewDto
{
    public string? CmpId { get; set; }
    public string SettingName { get; set; } = "default";

    public string FromEmail { get; set; } = "";
    public string? FromName { get; set; }

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = "";

    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CalendarId { get; set; }
    public string? GoogleOAuthClientId { get; set; }
}
