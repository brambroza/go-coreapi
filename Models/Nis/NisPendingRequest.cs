using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisPendingRequest ───────────────────────────────────────────────────────────
// A "self open ticket" request raised by a field engineer in the Staff Portal.
// It lands in the Service Board → Staff Requests tab where a manager either
// approves it (→ creates a real NisTicket assigned to an engineer) or rejects it.

public class NisPendingRequest
{
    [Key]
    [MaxLength(50)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// Engineer (FullName) who raised the request.
    [MaxLength(200)]
    public string RequestedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// Install | MA | PM | Support | Backup | Report | งานภายใน
    [MaxLength(50)]
    public string TicketType { get; set; } = string.Empty;

    /// Onsite | Remote | Telephone
    [MaxLength(50)]
    public string? SupportMethod { get; set; }

    /// Project the resulting ticket should be created under (optional at request
    /// time, but required before a manager can approve).
    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(2000)]
    public string? Detail { get; set; }

    public DateTime? Due { get; set; }

    public bool NoOnsite { get; set; } = false;
    public bool SkipSignature { get; set; } = false;
    public bool RequireCloseApproval { get; set; } = false;

    /// Set when the request is opened against an existing ticket (sub-request).
    [MaxLength(50)]
    public string? ParentTicketId { get; set; }

    /// Pending | Approved | Rejected
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// TicketId created when the request is approved (audit link).
    [MaxLength(50)]
    public string? CreatedTicketId { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    [MaxLength(100)]
    public string? RejectedBy { get; set; }

    /// เหตุผลที่ SM ปฏิเสธ — แสดงให้ผู้ขอใน Staff Portal
    [MaxLength(500)]
    public string? RejectReason { get; set; }

    // ── คำขอที่เปิดจากเคส helpdesk (ปุ่ม "เปิดเคส Onsite" ใน /productservice/servicerequest) ──
    // คำขอชนิดนี้สร้าง NisTicket 'MA Onsite' สถานะ 'Waiting Approval' ทันที (CreatedTicketId ชี้ตั๋วนั้น)
    // SM อนุมัติ → อัปเดตตั๋วเดิม · ปฏิเสธ → ลบตั๋ว (คืนโควตา) แถวนี้คงไว้เป็นประวัติ

    /// เลขเคส helpdesk ต้นทาง (STProblem.ProblemId) · null = คำขอจาก Staff Portal / RN แบบเดิม
    [MaxLength(50)]
    public string? SourceProblemId { get; set; }

    [MaxLength(50)]
    public string? CustomerCode { get; set; }

    [MaxLength(300)]
    public string? CustomerName { get; set; }

    /// LINE userId ของลูกค้าที่แจ้งเคส (สำเนาไปที่ NisTicket.CustomerLineUserId)
    [MaxLength(100)]
    public string? CustomerLineUserId { get; set; }

    [MaxLength(100)]
    public string? CustomerOaLineId { get; set; }

    /// ผู้ขอยืนยันเปิดทั้งที่โควตา Onsite Accident ของโครงการหมดแล้ว
    public bool AllowOverQuota { get; set; } = false;

    /// High | Medium | Low — priority ที่ขอ (default = ของโครงการ)
    [MaxLength(20)]
    public string? Priority { get; set; }

    /// ไฟล์แนบที่ staff แนบตอนขอเปิดตั๋ว (คำขอแบบเดิมจาก Staff Portal ที่ยังไม่มีตั๋ว) —
    /// JSON ของ List&lt;NisAttachmentDto&gt; (FileName/FilePath/Seq/FileSize) · ตัวไฟล์ upload ผ่าน /uploadallfile + /movefile แล้ว
    /// SM อนุมัติ → คัดลอกเป็น NisTicketFile ของตั๋วที่สร้าง · คำขอจาก helpdesk ไม่ใช้ช่องนี้ (ไฟล์อยู่ที่ตั๋ว 'Waiting Approval' ตั้งแต่ขอ)
    public string? AttachmentsJson { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
