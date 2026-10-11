using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisSyncLog ──────────────────────────────────────────────────────────────
// บันทึกรอบ sync ของ NisArchiveSyncService ต่อบริษัท (dbo.NisSyncLog)
// dataAsOf ของรายงานคำนวณจากรอบล่าสุดที่ Status='ok' และ InvalidCount=0 และ missing% ต่ำกว่าเกณฑ์
// — sync ที่ "สำเร็จแบบไม่ครบ" ห้ามเลื่อนตราเวลาความสด

public class NisSyncLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    /// ok | failed | skipped | running | interrupted (ถูกตัดกลางคัน — แอปเริ่มใหม่)
    [MaxLength(20)]
    public string Status { get; set; } = "running";

    /// cron | manual
    [MaxLength(20)]
    public string Trigger { get; set; } = "cron";

    public int? FetchedCount { get; set; }
    public int? ParsedCount { get; set; }
    public int? InvalidCount { get; set; }
    public int? InsertedCount { get; set; }
    public int? UpdatedCount { get; set; }
    public int? UnchangedCount { get; set; }

    /// แถวที่เคยมีใน archive แต่รอบนี้ต้นทางไม่ส่งมา — ไม่ลบ แค่บันทึก
    public int? MissingCount { get; set; }

    public int? DurationMs { get; set; }

    public string? Error { get; set; }
}
