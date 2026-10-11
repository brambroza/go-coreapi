using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisServiceCase ──────────────────────────────────────────────────────────
// Archive ของเคสงานบริการสำหรับโมดูล NIS Service Report (dbo.NisServiceCase)
// คัดลอกจาก SP getProblem* (ชุดเดียวกับ ProblemReceiveController.gettaskservice) ทุก 15 นาที
// โดย NisArchiveSyncService แล้ว normalize ผ่าน NisCaseAdapter — รายงานที่ส่งลูกค้าอ่านจากตารางนี้
// ไม่อ่านจาก Problem ตรง เพื่อให้ dataAsOf / isTestData / resolutionHours มีความหมายคงที่
//
// 🔴 RawJson เก็บ STProblem ทั้งก้อนเสมอ — ถ้าภายหลังพบว่าตีความ field ผิด ย้อนคำนวณใหม่ได้จากตรงนี้
// 🔴 ไม่มีการลบแถว — เคสที่ต้นทางถอนออกจะถูกนับเป็น missing ใน NisSyncLog เท่านั้น

public class NisServiceCase
{
    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// ProblemId จากต้นทาง — คีย์ upsert ร่วมกับ CmpId
    [MaxLength(50)]
    public string SourceId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CaseNo { get; set; } = string.Empty;

    /// TaskId (GUID) — คีย์สำรองของต้นทาง เก็บไว้ตรวจสอบย้อนหลัง
    [MaxLength(100)]
    public string ExternalGuid { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(300)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? BranchName { get; set; }

    [MaxLength(200)]
    public string? Province { get; set; }

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? RequestBy { get; set; }

    [MaxLength(100)]
    public string? RequestPhone { get; set; }

    [MaxLength(200)]
    public string? RequestEmail { get; set; }

    [MaxLength(200)]
    public string? RequestPosition { get; set; }

    public string Subject { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Category { get; set; } = string.Empty;

    /// critical | major | medium | minor
    [MaxLength(20)]
    public string Priority { get; set; } = "minor";

    /// new | inprogress | finish
    [MaxLength(20)]
    public string Status { get; set; } = "new";

    public string? Remark { get; set; }

    /// แจ้งเข้า — ใช้กับตัวเลขชุด "งานที่แจ้งเข้าเดือนนี้"
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    /// ปิดได้ — ใช้กับตัวเลขชุด "งานที่ปิดได้เดือนนี้"
    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    /// เวลานาฬิกา (แจ้ง→ปิด) เป็นชั่วโมง ไม่ใช่เวลาลงมือทำ — ห้ามเอาไปคิดราคาต่อชั่วโมง
    public double? ResolutionHours { get; set; }

    /// JSON array ของ userId
    public string AssigneeIdsJson { get; set; } = "[]";

    /// JSON array ของชื่อ
    public string AssigneeNamesJson { get; set; } = "[]";

    [MaxLength(200)]
    public string? PrimaryAssignee { get; set; }

    public string? ActionDetails { get; set; }

    /// JSON array ของชื่อผู้ลงมือทำ (action.actionBy[].userFullName)
    public string ActionByNamesJson { get; set; } = "[]";

    /// action.serviceType 0/1/2/3 — ความหมายยังไม่ยืนยันจากต้นทาง
    public int? ServiceTypeCode { get; set; }

    /// 1–5 เท่านั้น · null = ยังไม่ให้คะแนน (ต้นทางส่ง 9 หรือ 0)
    public int? SatisfactionScore { get; set; }

    public string? SatisfactionComment { get; set; }

    public bool ViaLine { get; set; }

    [MaxLength(100)]
    public string? AnydeskId { get; set; }

    public int AttachmentCount { get; set; }

    /// กรองที่ adapter ชั้นเดียว — เก็บแถวไว้แต่ติดธงไม่ให้เข้าสถิติ
    public bool IsTestData { get; set; }

    /// STProblem ทั้งก้อน serialize เป็น JSON
    public string RawJson { get; set; } = "{}";

    public DateTimeOffset SyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
