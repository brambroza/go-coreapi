using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisReportTemplate ───────────────────────────────────────────────────────
// Master Template ของลูกค้าหนึ่งราย สำหรับรายงานบริการรายเดือน (dbo.NisReportTemplate)
// LayoutJson เก็บ TemplateSchema ทั้งฉบับ (docMeta + blocks + page + period) — ตรวจรูปก่อนเขียนทุกครั้ง
//
// 🔴 ไม่มีการลบ template จริง — "ลบ" บนหน้าจอคือตั้ง ArchivedAt (รายงานที่เคยออกยังเปิดดูได้)
// Version เพิ่มเฉพาะเมื่อ LayoutJson เปลี่ยนจริง — คนละตัวกับ schemaVersion ที่อยู่ใน layout
// ชื่อ entity ตั้งใจไม่ชน Entities/ReportTemplate.cs (ของ ReportTemplatesController ซึ่งไม่มี CmpId)

public class NisReportTemplate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// book | brief
    [MaxLength(20)]
    public string DocKind { get; set; } = "book";

    public string LayoutJson { get; set; } = "{}";

    public int Version { get; set; } = 1;

    public bool IsDefault { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    public ICollection<NisReport> Reports { get; set; } = new List<NisReport>();
}
