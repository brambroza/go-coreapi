using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace goalongapi.Helpers;

/// Persists client-generated Service Report PDFs to disk (or a configured directory) and
/// returns a small reference (path/size/sha256) to store on the report row. Keeping the blob
/// out of the database row lets the customer's signed report be re-sent/audited without
/// bloating the table. Configuration:
///   NisOnsite:ReportPdfDir       — base directory (default: {ContentRoot}/App_Data/nis-reports)
///   NisOnsite:MaxReportPdfBytes  — max decoded size (default: 8 MB)
public sealed class NisReportPdfStorage
{
    private readonly string _baseDir;

    public NisReportPdfStorage(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["NisOnsite:ReportPdfDir"];
        _baseDir = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "nis-reports")
            : configured;
        MaxBytes = config.GetValue<long?>("NisOnsite:MaxReportPdfBytes") ?? 8L * 1024 * 1024;
    }

    /// Maximum allowed decoded PDF size in bytes.
    public long MaxBytes { get; }

    public sealed record StoredPdf(string RelativePath, long Size, string Sha256);

    /// Decode a base64 payload and enforce the size limit before allocating the full buffer.
    /// Returns false (with a client-safe message) on malformed base64 or oversize.
    public bool TryDecode(string? base64, out byte[] bytes, out string error)
    {
        bytes = Array.Empty<byte>();
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(base64)) { error = "empty report PDF"; return false; }

        // Reject oversize by encoded length first (base64 ≈ 4/3 of decoded) — avoids the decode allocation.
        var approxDecoded = (long)base64.Length * 3 / 4;
        if (approxDecoded > MaxBytes)
        {
            error = $"report PDF too large (max {MaxBytes / (1024 * 1024)} MB)";
            return false;
        }

        try { bytes = Convert.FromBase64String(base64); }
        catch (FormatException) { error = "report PDF is not valid base64"; return false; }

        if (bytes.Length == 0) { error = "empty report PDF"; return false; }
        if (bytes.Length > MaxBytes)
        {
            bytes = Array.Empty<byte>();
            error = $"report PDF too large (max {MaxBytes / (1024 * 1024)} MB)";
            return false;
        }
        return true;
    }

    /// Write the decoded PDF under {base}/nis-onsite/{cmpId}/{ticketId}/{srNumber}.pdf and
    /// return its relative path, size and SHA-256 (hex). Overwrites deterministically so a
    /// resubmit for the same SR replaces the previous file rather than piling up.
    public async Task<StoredPdf> SaveAsync(string cmpId, string ticketId, string srNumber, byte[] content)
    {
        var relDir = Path.Combine("nis-onsite", Segment(cmpId), Segment(ticketId));
        var fileName = $"{Segment(string.IsNullOrWhiteSpace(srNumber) ? "draft" : srNumber)}.pdf";
        var absDir = Path.Combine(_baseDir, relDir);
        Directory.CreateDirectory(absDir);
        var absPath = Path.Combine(absDir, fileName);
        await File.WriteAllBytesAsync(absPath, content);

        var sha256 = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        var relPath = Path.Combine(relDir, fileName).Replace('\\', '/');
        return new StoredPdf(relPath, content.Length, sha256);
    }

    /// <summary>
    /// อ่าน PDF ที่เก็บไว้ (RelativePath จาก SaveAsync) กลับมาเป็น byte[] — ใช้ตอน SM อนุมัติปิดงาน
    /// แล้วต้องแนบ PDF ที่ช่างส่งมาตอน request-close ไปกับอีเมลลูกค้า
    /// กัน path traversal: path ที่ resolve แล้วต้องอยู่ใต้ base dir เท่านั้น · ไม่พบ/ผิดรูป = null
    /// </summary>
    /// <param name="relativePath">NisOnsiteReport.ReportPdfPath</param>
    /// <returns>เนื้อไฟล์ หรือ null เมื่อไม่พบ</returns>
    public async Task<byte[]?> TryReadAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        var baseFull = Path.GetFullPath(_baseDir);
        var absPath = Path.GetFullPath(Path.Combine(baseFull, relativePath));
        var basePrefix = baseFull.EndsWith(Path.DirectorySeparatorChar) ? baseFull : baseFull + Path.DirectorySeparatorChar;
        if (!absPath.StartsWith(basePrefix, StringComparison.Ordinal)) return null;
        if (!File.Exists(absPath)) return null;

        var bytes = await File.ReadAllBytesAsync(absPath);
        return bytes.Length == 0 ? null : bytes;
    }

    private static string Segment(string? value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : Regex.Replace(value, "[^A-Za-z0-9_-]", "_");
}
