using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisReport ───────────────────────────────────────────────────────────────
// รายงานบริการหนึ่งฉบับที่ออกให้ลูกค้า (dbo.NisReport)
//
// 🔴 LayoutSnapshotJson + DataSnapshotJson คือเหตุผลที่ตารางนี้มีอยู่ — รายงานที่ส่งลูกค้าไปแล้ว
//    ต้องเปิดดูซ้ำได้เลขเดิมเสมอ แม้ข้อมูลใน archive ถูกแก้ย้อนหลัง
// 🔴 DataSnapshotJson เก็บเฉพาะสิ่งที่รายงานฉบับนั้นแสดงจริง (assertSnapshotWithinProjection ก่อนลงฐาน)
// 🔴 PdfBlob เก็บไฟล์ที่เรนเดอร์แล้วในฐาน ไม่ใช่แค่ path — backup ของฐานครบในตัวมันเอง
// PrintKeyHash/PrintKeyExpiresAt: one-time key ให้ Playwright เปิดหน้าพิมพ์บน CRM โดยไม่ต้องมี JWT

public class NisReport
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public Guid TemplateId { get; set; }

    public NisReportTemplate? Template { get; set; }

    /// เวอร์ชันของ template ณ ตอนออกรายงาน
    public int TemplateVersion { get; set; }

    [MaxLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(300)]
    public string CustomerName { get; set; } = string.Empty;

    /// ฉบับที่ x — ลูกค้าใช้ตรวจว่าได้รับครบตามสัญญาไหม
    public int? IssueNo { get; set; }

    public DateTimeOffset PeriodStart { get; set; }

    public DateTimeOffset PeriodEnd { get; set; }

    public string LayoutSnapshotJson { get; set; } = "{}";

    public string DataSnapshotJson { get; set; } = "{}";

    /// draft | generating | ready | failed
    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    public byte[]? PdfBlob { get; set; }

    [MaxLength(64)]
    public string? PdfSha256 { get; set; }

    [MaxLength(500)]
    public string? PdfPath { get; set; }

    [MaxLength(64)]
    public string? PrintKeyHash { get; set; }

    public DateTimeOffset? PrintKeyExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<NisReportJob> Jobs { get; set; } = new List<NisReportJob>();
}
