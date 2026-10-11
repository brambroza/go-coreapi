using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using goalongapi.Dtos.Nis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace goalongapi.Helpers;

/// <summary>
/// ที่เก็บโลโก้ลูกค้า / QR LINE ของโมดูล NIS Service Report — port จาก apps/api/src/reports/logo.service.ts
/// (ตาม pattern <see cref="NisReportPdfStorage"/>)
///
/// 🔴 ตรวจชนิดไฟล์จากไบต์จริง ไม่เชื่อ MIME ที่เบราว์เซอร์แจ้ง (docs/06 ข้อ 4.1) — ไฟล์นี้จะถูกเรนเดอร์ด้วย
///    Chromium ตอนออก PDF · SVG ที่มีสคริปต์/handler ถูกปฏิเสธตั้งแต่ชั้นรับเข้า ไม่หวังพึ่ง CSP
/// 🔴 ชื่อไฟล์มาจากเนื้อหา (sha256) ไม่ใช่ชื่อที่ผู้ใช้ส่งมา — กัน path traversal · Read รับเฉพาะชื่อที่ระบบตั้งเอง
///
/// Configuration: <c>NisReport:AssetDir</c> (ว่าง = {ContentRoot}/App_Data/nis-report-assets)
/// ลงทะเบียน AddScoped ใน Program.cs (ชื่อไม่ลงท้าย Service · Autofac ไม่ auto-register)
/// </summary>
public sealed class NisReportAssetStorage
{
    /// <summary>เพดานขนาดไฟล์ 512 KB (ข้อ 4.1)</summary>
    public const long MaxBytes = 512 * 1024;

    /// <summary>prefix ของ url ที่ controller เสิร์ฟ — docMeta.customerLogoPath / company.lineQrPath เก็บค่านี้</summary>
    public const string UrlPrefix = "/api/nis/report/assets/";

    private static readonly Regex SafeNameRegex = new(@"^[A-Za-z0-9_-]{1,80}\.(png|jpg|webp|svg)$", RegexOptions.Compiled);
    private static readonly Regex SvgHeadRegex = new(@"^\s*(<\?xml[\s\S]{0,200}?)?<svg[\s>]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    /// <summary>SVG รันสคริปต์ได้ — ปฏิเสธ script · event handler · javascript: · foreignObject · ENTITY</summary>
    private static readonly Regex SvgForbiddenRegex = new(@"<script|\son\w+\s*=|javascript:|<foreignObject|<!ENTITY", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

    private readonly string _baseDir;

    public NisReportAssetStorage(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["NisReport:AssetDir"];
        _baseDir = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "nis-report-assets")
            : configured;
    }

    /// <summary>โฟลเดอร์จริงที่เก็บไฟล์</summary>
    public string BaseDir => _baseDir;

    /// <summary>ตรวจไบต์ต้นไฟล์ — คืน (ext, mime) หรือ null ถ้าไม่ใช่ PNG · JPG · WebP · SVG</summary>
    public static (string Ext, string Mime)? Sniff(byte[] buf)
    {
        if (buf.Length >= 8 && buf.AsSpan(0, 8).SequenceEqual(PngMagic)) return ("png", "image/png");
        if (buf.Length >= 3 && buf[0] == 0xff && buf[1] == 0xd8 && buf[2] == 0xff) return ("jpg", "image/jpeg");
        if (buf.Length >= 12 && Encoding.ASCII.GetString(buf, 0, 4) == "RIFF" && Encoding.ASCII.GetString(buf, 8, 4) == "WEBP") return ("webp", "image/webp");
        var head = Encoding.UTF8.GetString(buf, 0, Math.Min(512, buf.Length));
        if (SvgHeadRegex.IsMatch(head)) return ("svg", "image/svg+xml");
        return null;
    }

    /// <summary>MIME ตามนามสกุล (เฉพาะชุดที่รองรับ)</summary>
    public static string MimeOf(string ext) => ext switch
    {
        "png" => "image/png",
        "jpg" => "image/jpeg",
        "webp" => "image/webp",
        "svg" => "image/svg+xml",
        _ => "application/octet-stream",
    };

    /// <summary>
    /// บันทึกไฟล์ — ชื่อ <c>{kind}-{ownerId}-{sha256[:16]}.{ext}</c> · ไฟล์เดิมเนื้อหาเดิมได้ชื่อเดิม (ไม่กองซ้ำ)
    /// </summary>
    /// <param name="kind">customer-logo · company-line · contact-qr</param>
    /// <param name="ownerId">รหัสลูกค้าหรือตัวบอกเจ้าของ (ถูกล้างให้เหลือ A-Z a-z 0-9 _ -)</param>
    /// <param name="bytes">เนื้อไฟล์ดิบ</param>
    /// <param name="fileNameHint">ชื่อไฟล์ที่ผู้ใช้ส่งมา — ใช้ในข้อความ error เท่านั้น</param>
    /// <exception cref="NisReportBadRequestException">ไฟล์ว่าง · เกิน 512 KB · ชนิดไม่รองรับ · SVG มีสคริปต์</exception>
    public async Task<NisAssetUploadResultDto> SaveAsync(string kind, string ownerId, byte[] bytes, string fileNameHint, CancellationToken ct = default)
    {
        if (bytes.Length == 0) throw new NisReportBadRequestException("ไฟล์ว่าง");
        if (bytes.Length > MaxBytes)
            throw new NisReportBadRequestException($"ไฟล์ใหญ่ {(bytes.Length / 1024d).ToString("0")} KB — ข้อ 4.1 จำกัดไว้ที่ 512 KB ต่อไฟล์");
        var sig = Sniff(bytes);
        if (sig == null)
            throw new NisReportBadRequestException(
                $"ไฟล์ \"{fileNameHint}\" ไม่ใช่ PNG · JPG · SVG · WebP เมื่อดูจากไบต์จริง — " +
                "ระบบไม่เชื่อชนิดไฟล์ที่เบราว์เซอร์แจ้งมา (ข้อ 4.1)");
        if (sig.Value.Ext == "svg" && SvgForbiddenRegex.IsMatch(Encoding.UTF8.GetString(bytes)))
            throw new NisReportBadRequestException(
                "SVG มีสคริปต์หรือ event handler อยู่ข้างใน — ไฟล์นี้จะถูกเรนเดอร์ด้วย Chromium ตอนออก PDF จึงรับไม่ได้");

        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()[..16];
        var safeKind = Segment(kind, 20);
        var safeOwner = Segment(ownerId, 40);
        var name = $"{safeKind}-{safeOwner}-{hash}.{sig.Value.Ext}";
        if (!SafeNameRegex.IsMatch(name))
            throw new NisReportBadRequestException("ตั้งชื่อไฟล์ไม่ได้ — รหัสเจ้าของไฟล์ไม่ถูกต้อง");

        Directory.CreateDirectory(_baseDir);
        await File.WriteAllBytesAsync(Path.Combine(_baseDir, name), bytes, ct);
        return new NisAssetUploadResultDto { UrlPath = UrlPrefix + name, Bytes = bytes.Length, Mime = sig.Value.Mime };
    }

    /// <summary>อ่านไฟล์ — ยอมเฉพาะชื่อที่ระบบเป็นคนตั้งเอง (ไม่มีทางให้ .. หรือ / เล็ดลอด) · null = ชื่อผิดรูปหรือไม่มีไฟล์</summary>
    public (byte[] Bytes, string Mime)? Read(string name)
    {
        if (string.IsNullOrEmpty(name) || !SafeNameRegex.IsMatch(name)) return null;
        var full = Path.Combine(_baseDir, name);
        if (!File.Exists(full)) return null;
        var ext = name[(name.LastIndexOf('.') + 1)..];
        return (File.ReadAllBytes(full), MimeOf(ext));
    }

    private static string Segment(string? value, int max)
    {
        var s = string.IsNullOrWhiteSpace(value) ? "unknown" : Regex.Replace(value, "[^A-Za-z0-9_-]", "_");
        return s.Length > max ? s[..max] : s;
    }
}
