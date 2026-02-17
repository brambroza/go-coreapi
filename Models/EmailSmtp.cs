

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
}
