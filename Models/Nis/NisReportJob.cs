using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisReportJob ────────────────────────────────────────────────────────────
// งานเรนเดอร์ PDF ที่รอคิวอยู่ (dbo.NisReportJob) — คิวเป็นตารางในฐาน ไม่ใช่ Redis/Hangfire
// (9–15 ฉบับ/เดือน ไม่มีอะไรให้ block และได้ transaction ร่วมกับข้อมูลจริงฟรี)
//
// 🔴 ต้องยึดงานค้างคืนได้ — งาน running ตอน container รีสตาร์ตจะค้างตลอดกาลถ้าไม่มีใครกวาด
//    LeaseUntil เลยเวลา = คนที่ถืออยู่ตายแล้ว ใครก็หยิบต่อได้
// Stage บอกขั้นที่งานผ่านจริง (rendering → writing) ให้หน้าจอแสดง ไม่ใช่เปอร์เซ็นต์เดาจากเวลา
// Error เป็นข้อความอ่านออก ไม่ใช่แค่สถานะ failed

public class NisReportJob
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public Guid ReportId { get; set; }

    public NisReport? Report { get; set; }

    /// queued | running | done | failed
    [MaxLength(20)]
    public string Status { get; set; } = "queued";

    /// rendering | writing
    [MaxLength(20)]
    public string? Stage { get; set; }

    public int Attempts { get; set; }

    public DateTimeOffset? LeaseUntil { get; set; }

    [MaxLength(100)]
    public string? WorkerId { get; set; }

    public string? Error { get; set; }

    [MaxLength(500)]
    public string? PdfPath { get; set; }

    public int? PdfBytes { get; set; }

    public int? RenderMs { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }
}
