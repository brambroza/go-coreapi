using System.Text;
using System.Text.RegularExpressions;
using goalongapi.Dtos.Nis;

namespace goalongapi.Helpers;

/// <summary>
/// ประกอบ subject/body อีเมลของระบบ NIS จาก email template + ลายเซ็นที่ตั้งไว้ในหน้า System Config.
/// พอร์ตตรงจาก <c>go-crm-24v4/src/utils/nis-email-template.ts</c> เพื่อให้อีเมลที่ส่งจาก backend
/// (flow ปิดงาน onsite ของ RN และ CRM) หน้าตาเหมือนกับที่ CRM ประกอบเองในหน้า Service Board.
/// ตัวแปรอยู่ในรูปแบบ [KEY] ตรงกับที่แสดงในหน้า config.
/// </summary>
public static class NisEmailTemplateRenderer
{
    /// id ของ template ที่ใช้ตอนส่งปิดงาน — ต้องตรงกับฝั่ง CRM (NIS_EMAIL_TEMPLATE_CLOSE_JOB)
    public const string CloseJobTemplateId = "close-job";

    private static readonly Regex VariablePattern = new(@"\[([A-Z0-9_]+)\]", RegexOptions.Compiled);

    /// <summary>ข้อมูลผู้ส่ง (ผู้ล็อกอินตอนปิดงาน) ที่ใช้เติมลายเซ็นเมื่อ UseLoginName = true</summary>
    /// <param name="FullName">ชื่อเต็มของผู้ล็อกอิน (Account.FullName)</param>
    /// <param name="Position">ตำแหน่ง / role ของผู้ล็อกอิน</param>
    /// <param name="Mobile">เบอร์มือถือของผู้ล็อกอิน</param>
    public sealed record NisEmailSender(string? FullName, string? Position, string? Mobile);

    /// <summary>escape ค่าที่ผู้ใช้/ลูกค้าป้อน ก่อนยัดลง HTML body (กัน HTML injection ในเมล)</summary>
    /// <param name="value">ข้อความดิบ</param>
    /// <returns>ข้อความที่ escape แล้ว</returns>
    public static string EscapeHtml(string? value) =>
        (value ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");

    /// <summary>
    /// แทนที่ตัวแปร [KEY] ในข้อความ template
    /// </summary>
    /// <param name="text">ข้อความ template (subject หรือ body)</param>
    /// <param name="vars">ค่าตัวแปร (key ไม่มีวงเล็บ) — key ที่ไม่ได้ส่งมาจะคงรูป [KEY] ไว้, ค่าว่างแทนด้วย '-'</param>
    /// <param name="html">true = escape ค่าก่อนแทน (ใช้กับ body ที่เป็น HTML)</param>
    /// <returns>ข้อความที่แทนค่าตัวแปรแล้ว</returns>
    public static string Render(string? text, IReadOnlyDictionary<string, string?> vars, bool html = false)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        return VariablePattern.Replace(text, match =>
        {
            var key = match.Groups[1].Value;
            if (!vars.TryGetValue(key, out var raw)) return match.Value;
            var value = (raw ?? string.Empty).Trim();
            if (value.Length == 0) value = "-";
            return html ? EscapeHtml(value) : value;
        });
    }

    /// <summary>ค้นหา template ตาม id — คืน null เมื่อไม่มีหรือถูกปิดใช้งาน</summary>
    /// <param name="templates">รายการ template จาก System Config</param>
    /// <param name="id">id ของ template เช่น "close-job"</param>
    /// <returns>template ที่เปิดใช้งานอยู่ หรือ null</returns>
    public static NisEmailTemplateDto? FindTemplate(IEnumerable<NisEmailTemplateDto>? templates, string id)
    {
        var found = templates?.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
        return found?.Enabled == true ? found : null;
    }

    /// <summary>
    /// สร้าง HTML ลายเซ็นอีเมล — ชื่อ/ตำแหน่ง/มือถือ ยึดผู้ล็อกอินเมื่อ UseLoginName = true
    /// (ค่าที่ตั้งไว้ในหน้า config เป็น fallback), ข้อมูลบริษัทมาจาก config อย่างเดียว
    /// </summary>
    /// <param name="sig">ค่าลายเซ็นจาก System Config (null = ไม่ใส่ลายเซ็น)</param>
    /// <param name="sender">ข้อมูลผู้ล็อกอินที่ปิดงาน</param>
    /// <returns>HTML ของลายเซ็น หรือ string ว่างเมื่อปิดใช้งาน</returns>
    public static string BuildSignatureHtml(NisEmailSignatureDto? sig, NisEmailSender sender)
    {
        if (sig == null || !sig.Enabled) return string.Empty;

        var name = FirstNonEmpty(sig.UseLoginName ? sender.FullName : null, sig.SenderName, sender.FullName);
        var position = FirstNonEmpty(sig.UseLoginName ? sender.Position : null, sig.Position);
        var mobile = FirstNonEmpty(sig.UseLoginName ? sender.Mobile : null, sig.Mobile);

        var companyTh = sig.CompanyNameTh ?? string.Empty;
        var companyEn = sig.CompanyNameEn ?? string.Empty;
        var address = sig.Address ?? string.Empty;
        var phone = sig.Phone ?? string.Empty;
        var website = sig.Website ?? string.Empty;
        var logoUrl = sig.LogoUrl ?? string.Empty;
        var qrUrl = sig.QrUrl ?? string.Empty;

        var websiteHref = website.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? website : $"https://{website}";

        var sb = new StringBuilder();
        sb.Append("<div style=\"font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#111;line-height:1.5;\">");
        sb.Append("<hr style=\"border:none;border-top:2px solid #111;margin:16px 0 12px;\" />");
        sb.Append("<div style=\"font-weight:700;\">Best regards,</div>");
        AppendLine(sb, name.Length > 0 ? $"<span style=\"font-weight:700;color:#1a56db;\">{EscapeHtml(name)}</span>" : "");
        AppendLine(sb, position.Length > 0 ? $"<span style=\"color:#1a56db;\">{EscapeHtml(position)}</span>" : "");
        AppendLine(sb, mobile.Length > 0 ? $"<span style=\"color:#1a56db;\">Mobile: {EscapeHtml(mobile)}</span>" : "");
        sb.Append(logoUrl.Length > 0
            ? $"<div style=\"margin:10px 0;\"><img src=\"{EscapeHtml(logoUrl)}\" alt=\"logo\" style=\"max-height:64px;\" /></div>"
            : "<div style=\"height:8px;\"></div>");

        var companyHeadline = companyEn.Length > 0 ? companyEn : companyTh;
        AppendLine(sb, companyHeadline.Length > 0
            ? $"<span style=\"font-weight:700;color:#e07b00;text-decoration:underline;\">{EscapeHtml(companyHeadline)}</span>"
            : "");
        AppendLine(sb, companyEn.Length > 0 && companyTh.Length > 0 ? EscapeHtml(companyTh) : "");
        AppendLine(sb, address.Length > 0 ? EscapeHtml(address) : "");
        AppendLine(sb, phone.Length > 0 ? $"Tel&amp;Fax: {EscapeHtml(phone)}" : "");
        if (website.Length > 0)
            sb.Append($"<div><a href=\"{EscapeHtml(websiteHref)}\" style=\"font-weight:700;color:#e07b00;\">{EscapeHtml(website)}</a></div>");
        if (qrUrl.Length > 0)
            sb.Append($"<div style=\"margin-top:10px;\"><img src=\"{EscapeHtml(qrUrl)}\" alt=\"line qr\" style=\"max-height:96px;\" /></div>");
        sb.Append("</div>");

        return sb.ToString();
    }

    /// <summary>แปลงข้อความ plain text ของช่าง (ขึ้นบรรทัดใหม่ด้วย \n) เป็น HTML ที่ escape แล้ว</summary>
    /// <param name="text">ข้อความดิบจากผู้ใช้</param>
    /// <returns>HTML ที่พร้อมฝังใน body (คงการขึ้นบรรทัดใหม่)</returns>
    public static string PlainTextToHtml(string? text) =>
        EscapeHtml(text)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", "<br />");

    private static void AppendLine(StringBuilder sb, string content)
    {
        if (content.Length > 0) sb.Append($"<div>{content}</div>");
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;
}
