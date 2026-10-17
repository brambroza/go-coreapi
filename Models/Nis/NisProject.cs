using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models.Nis;

// ── NisProject ────────────────────────────────────────────────────────────────
// Represents a NIS service project (Implement / MA / Runrate)

public class NisProject
{
    [Key]
    [MaxLength(50)]
    public string ProjectId { get; set; } = Guid.NewGuid().ToString();

    /// Yearly sequential number per CmpId, stored as varchar (e.g. "NIS-2600001").
    /// Format: NIS-YYXXXXX, where XXXXX resets to 00001 each Bangkok calendar year.
    [MaxLength(50)]
    public string? ProjectNo { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Customer { get; set; } = string.Empty;

    /// Master customer code (msb.mCustomer.CustomerCode). Kept so the project can
    /// resolve the customer's saved locations (msb.mCustomerLocations) later.
    [MaxLength(50)]
    public string? CustomerCode { get; set; }

    /// Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
    [MaxLength(50)]
    public string Type { get; set; } = "Implement";

    /// High | Medium | Low
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    public int Progress { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// Primary staff name (display only)
    [MaxLength(200)]
    public string Staff { get; set; } = string.Empty;

    /// Sales Order reference number
    [MaxLength(100)]
    public string SoRef { get; set; } = string.Empty;

    /// Pipe-delimited tag list, e.g. "Firewall|Network|WiFi"
    [MaxLength(1000)]
    public string TagsRaw { get; set; } = string.Empty;

    // ── Contact ───────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    // ── SalesPM ───────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? SalesPMName { get; set; }

    [MaxLength(100)]
    public string? SalesPMNickname { get; set; }

    [MaxLength(50)]
    public string? SalesPMPhone { get; set; }

    [MaxLength(200)]
    public string? SalesPMEmail { get; set; }

    [MaxLength(100)]
    public string? SalesPMRole { get; set; }

    // ── Engineer ──────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? EngineerName { get; set; }

    [MaxLength(100)]
    public string? EngineerNickname { get; set; }

    [MaxLength(50)]
    public string? EngineerPhone { get; set; }

    /// JSON object เงื่อนไขบริการ (สัญญา) ที่เลือกตอนสร้างโครงการ
    /// (sla/onsitePerYear/pmPerYear/onsiteAccident ฯลฯ) — null = โครงการเก่า
    public string? ServiceConditionsJson { get; set; }

    /// JSON array ของ NisEquipment.Id ที่เลือกตอนสร้างโครงการ (PM) — null = ไม่ได้เลือก/โครงการเก่า
    public string? EquipmentIdsJson { get; set; }

    // ── Location / Tenant ─────────────────────────────────────────────────────

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual ICollection<NisTicket> Tickets { get; set; } = new List<NisTicket>();

    /// Documents attached when the project was created (PDF/Excel/Word/Visio/Image).
    public virtual ICollection<NisProjectFile> Files { get; set; } = new List<NisProjectFile>();
}

// ── NisProjectFile ────────────────────────────────────────────────────────────
// One attached document for a NIS project. Binary is stored on disk via the shared
// upload endpoints (/uploadallfile + /movefile); this row only keeps the metadata.

public class NisProjectFile
{
    [Key]
    [MaxLength(50)]
    public string FileId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    /// Original file name shown to the user, e.g. "Network-Diagram.pdf"
    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    /// Full URL/path where the file was moved, e.g. "{serverUrl}/{cmpId}/nis/{projectNo}/{fileName}"
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    /// Display ordering (1-based) in the attachment list.
    public int Seq { get; set; } = 1;

    public long FileSize { get; set; } = 0;

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual NisProject? Project { get; set; }
}

// ── NisTicket ─────────────────────────────────────────────────────────────────

public class NisTicket
{
    [Key]
    [MaxLength(50)]
    public string TicketId { get; set; } = Guid.NewGuid().ToString();

    /// Human-readable code, e.g. TK-BK-0007-01 (TK-{TypePrefix}-{ProjectNo}-{RunNo}).
    /// Assigned by the backend on create — RunNo resets per (ProjectId, Type).
    [MaxLength(50)]
    public string? TicketCode { get; set; }

    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// Open | In Progress | Pending | Done | Closed | Scheduled
    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [MaxLength(200)]
    public string Assignee { get; set; } = "-";

    /// Accounts.Username ของ SM ที่กดมอบหมายตั๋วนี้ (ไม่ใช่ FullName — ต่างจาก Assignee)
    /// ใช้เป็นผู้รับแจ้งเตือนตอนช่างกดรับงาน · null = ตั๋วเก่าก่อนมี column นี้ หรือถูก unassign แล้ว
    [MaxLength(100)]
    public string? AssignedBy { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime? Due { get; set; }

    public int Pct { get; set; } = 0;

    /// Install | PM | MA Onsite | Support | Backup | Report | Delivery | MA
    [MaxLength(50)]
    public string? Type { get; set; }

    /// High | Medium | Low
    [MaxLength(20)]
    public string? Priority { get; set; }

    /// Pipe-delimited
    [MaxLength(500)]
    public string? TagsRaw { get; set; }

    /// รายละเอียดงานที่ระบุก่อนมอบหมาย (ดู/แก้จากการ์ดบนบอร์ด) — nvarchar(max)
    public string? WorkDetail { get; set; }

    /// Checklist ก่อนมอบหมายงาน เก็บเป็น JSON array ของ { id, text, done } — nvarchar(max)
    public string? ChecklistJson { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    /// เวลาที่ตั๋วเข้าสถานะ Closed / Done (เวลาไทย) · null = ยังไม่ปิด หรือถูกเปิดกลับ
    /// ใช้เรียงงานที่ปิดแล้ว — UpdatedDate ใช้แทนไม่ได้เพราะเปลี่ยนทุกครั้งที่ตั๋วถูกแก้
    public DateTime? ClosedDate { get; set; }

    /// เลขเคส helpdesk (Problem.ProblemId) ที่เปิดตั๋วนี้ — ตั๋ว MA Onsite ของโครงการเคส Accident
    /// ที่ตัดโควตาจากฟอร์ม Action ของ /productservice/servicerequest · null = ตั๋วที่เปิดจาก NIS เอง
    [MaxLength(50)]
    public string? SourceProblemId { get; set; }

    /// true = เปิดตั๋ว MA Onsite ทั้งที่โควตา Onsite Accident ของโครงการหมดแล้ว (ผู้ใช้ยืนยัน)
    public bool OverQuota { get; set; } = false;

    /// เวลาที่ช่าง Check-in ครั้งแรก (เวลาไทย ตามเวลา server) · null = ยังไม่เช็คอิน
    /// เขียนผ่าน PUT tickets/{id}/checkin ทั้งจาก RN และ CRM — ให้ทุก client เห็นสถานะเดียวกัน
    /// (เดิมอยู่แค่ใน draft ของเครื่อง/ของ user จนกว่าจะปิดงาน)
    public DateTime? CheckInTime { get; set; }

    /// พิกัด GPS ตอนเช็คอิน · null = เช็คอินโดยไม่มีพิกัด
    public double? CheckInLat { get; set; }
    public double? CheckInLng { get; set; }

    /// ชื่อสถานที่ที่ช่างยืนเช็คอิน (reverse geocode ฝั่ง client) · null = ไม่ทราบ
    [MaxLength(500)]
    public string? CheckInLocation { get; set; }

    /// ผู้เช็คอิน (username/fullName ที่ client ส่งมา)
    [MaxLength(200)]
    public string? CheckInBy { get; set; }

    /// เวลาที่ช่าง Check-out ครั้งแรก (เวลาไทย ตามเวลา server) · null = ยังไม่เช็คเอาท์
    /// เขียนผ่าน PUT tickets/{id}/checkout ทั้งจาก RN และ CRM — เป็น milestone 90% ที่ snapshot
    /// ของ user อื่น (CRM autosave) ห้ามลดลง (เดิมอยู่แค่ใน draft → เปิดดูใน CRM แล้ว Pct ถอยเป็น 25)
    public DateTime? CheckOutTime { get; set; }

    /// พิกัด GPS ตอนเช็คเอาท์ · null = เช็คเอาท์โดยไม่มีพิกัด
    public double? CheckOutLat { get; set; }
    public double? CheckOutLng { get; set; }

    /// ชื่อสถานที่ที่ช่างยืนเช็คเอาท์ (reverse geocode ฝั่ง client) · null = ไม่ทราบ
    [MaxLength(500)]
    public string? CheckOutLocation { get; set; }

    /// ผู้เช็คเอาท์ (username/fullName ที่ client ส่งมา)
    [MaxLength(200)]
    public string? CheckOutBy { get; set; }

    /// LINE userId ของลูกค้าที่แจ้งเคส (สำเนา STProblem.UserLineId ตอนเปิดเคส Onsite จาก helpdesk)
    /// null = ตั๋วที่ไม่ได้มาจากเคส LINE · เก็บไว้ใช้ต่อ (เช่น ส่งผลปิดงานทาง LINE) ยังไม่ส่งอัตโนมัติ
    [MaxLength(100)]
    public string? CustomerLineUserId { get; set; }

    /// LINE OA id ที่ลูกค้าคุยด้วย (สำเนา STProblem.OALineId)
    [MaxLength(100)]
    public string? CustomerOaLineId { get; set; }

    public virtual NisProject? Project { get; set; }

    /// ไฟล์แนบระดับตั๋ว (ภาพ/เอกสารที่ staff แนบตอนขอเปิดเคส Onsite จาก helpdesk)
    public virtual ICollection<NisTicketFile> Files { get; set; } = new List<NisTicketFile>();
}

// ── NisTicketFile ─────────────────────────────────────────────────────────────
// ไฟล์แนบระดับตั๋ว — คนละชุดกับ NisProjectFile (เอกสารโครงการ)
// ตัวไฟล์ upload ผ่าน /uploadallfile + /movefile ไป wwwroot/{cmpId}/nis/{projectNo}/tickets/
// เก็บเฉพาะ metadata ที่นี่ (pattern เดียวกับ NisProjectFile)

public class NisTicketFile
{
    [Key]
    [MaxLength(50)]
    public string FileId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string TicketId { get; set; } = string.Empty;

    /// โครงการของตั๋ว (denormalize ไว้ให้ query ต่อโครงการได้โดยไม่ join)
    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    /// ชื่อไฟล์เดิมที่แสดงให้ผู้ใช้ เช่น "ภาพหน้างาน.jpg"
    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    /// URL เต็มของไฟล์ เช่น "{serverUrl}/{cmpId}/nis/{projectNo}/tickets/{fileName}"
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    public int Seq { get; set; } = 1;

    public long FileSize { get; set; } = 0;

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual NisTicket? Ticket { get; set; }
}

// ── NisSalesOrder ─────────────────────────────────────────────────────────────

public class NisSalesOrder
{
    [Key]
    [MaxLength(50)]
    public string SoId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(100)]
    public string QuoteRef { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Customer { get; set; } = string.Empty;

    public DateTime? Date { get; set; }

    /// Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
    [MaxLength(50)]
    public string Type { get; set; } = "Implement";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    [MaxLength(200)]
    public string? Project { get; set; }

    [MaxLength(100)]
    public string? PoNumber { get; set; }

    public DateTime? PoDate { get; set; }

    [MaxLength(200)]
    public string? SalesName { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
