using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisSystemConfig ───────────────────────────────────────────────────────────
// Stores per-company system configuration as pipe-delimited lists.
// One row per CmpId — upsert pattern (GET returns default if not found).

public class NisSystemConfig
{
    [Key]
    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// Pipe-delimited job types, e.g. "Runrate|Implement|MA-Device"
    [MaxLength(2000)]
    public string JobTypesRaw { get; set; } = "Runrate|Implement|MA-Device|MA-Fortigate|MA-Software|MA-Network";

    /// Pipe-delimited tags
    [MaxLength(4000)]
    public string TagsRaw { get; set; } = "Firewall|Network|WiFi|Server|CCTV|Access Control|PC&Notebook|Peripheral|Software|Cable|Windows Server|VMware|HyperV";

    /// Pipe-delimited implement checklist items
    [MaxLength(8000)]
    public string ImplementChecklistRaw { get; set; } = string.Empty;

    /// Pipe-delimited MA checklist items
    [MaxLength(8000)]
    public string MaChecklistRaw { get; set; } = string.Empty;

    /// Pipe-delimited PM checklist items
    [MaxLength(8000)]
    public string PmChecklistRaw { get; set; } = string.Empty;

    /// JSON object ของ checklist มาตรฐานตามประเภท ticket
    /// เช่น {"Install":["..."],"PM":["..."],"MA Onsite":[...],"Backup":[...],"Report":[...],"Delivery":[...]}
    public string? ChecklistByTicketTypeJson { get; set; }

    /// JSON object ของ checklist เฉพาะลูกค้า — customerCode → (ticketType → items)
    /// เช่น {"CUST001":{"Install":["..."],"PM":["..."]}}. ว่าง/ไม่มี = ใช้ ChecklistByTicketType
    public string? ChecklistByCustomerJson { get; set; }

    /// JSON array ของ email template — [{"id":"close-job","name":"...","subject":"...","body":"...","enabled":true}]
    public string? EmailTemplatesJson { get; set; }

    /// JSON object ของลายเซ็นอีเมล (ชื่อ/ตำแหน่ง/มือถือ ปล่อยว่างได้ — ใช้ของผู้ล็อกอินตอนส่ง)
    public string? EmailSignatureJson { get; set; }

    /// JSON object ของตัวเลือกเงื่อนไขงาน (serviceYears/onsitePerYear/pmPerYear/delivery/defaults ฯลฯ)
    /// สำหรับ wizard สร้างโครงการ — null = ใช้ default จากโค้ด
    public string? ServiceConditionOptionsJson { get; set; }

    /// Pipe-delimited SLA options, e.g. "8x5xNBD|8x5|24x7x4|24x7xNBD"
    [MaxLength(500)]
    public string SlaOptionsRaw { get; set; } = "8x5xNBD|8x5|24x7x4|24x7xNBD";

    public int WarningDaysService { get; set; } = 60;
    public int WarningDaysProduct { get; set; } = 30;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
