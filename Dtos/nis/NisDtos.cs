namespace goalongapi.Dtos.Nis;

// ── Shared nested DTOs (match frontend INisContact / INisSalesPM / INisEngineer) ──

public class NisContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class NisSalesPMDto
{
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
}

public class NisEngineerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
}

// ── Attachment DTOs (match frontend INisAttachment) ──────────────────────────
// File binary is uploaded separately via the shared /uploadallfile + /movefile
// endpoints; only this metadata is persisted against the project.

public class NisAttachmentDto
{
    public string? Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int Seq { get; set; } = 1;
    public long FileSize { get; set; } = 0;
}

// ── Ticket DTOs ──────────────────────────────────────────────────────────────

public class NisTicketResponseDto
{
    public string Id { get; set; } = string.Empty;
    /// Human-readable code, e.g. TK-BK-0007-01
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Assignee { get; set; } = "-";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Due { get; set; } = string.Empty;
    public int Pct { get; set; }
    public string? Type { get; set; }
    public string? TicketType { get; set; }
    public string? Priority { get; set; }
    public List<string>? Tags { get; set; }
    /// รายละเอียดงานที่ระบุก่อนมอบหมาย
    public string? WorkDetail { get; set; }
    /// Checklist ก่อนมอบหมายงาน
    public List<NisChecklistItemDto> Checklist { get; set; } = new();
    /// วันเวลาที่สร้าง ticket (yyyy-MM-dd HH:mm) — ใช้ทำ badge "มาใหม่" บนบอร์ด
    public string CreatedDate { get; set; } = string.Empty;
    /// วันเวลาที่แก้ไขล่าสุด (yyyy-MM-dd HH:mm) — การมอบหมายงานเขียนค่านี้ทุกครั้ง
    /// แอปช่างใช้เป็นเวลา "เพิ่งได้รับมอบหมาย" สำหรับ badge งานใหม่ (ตั๋วที่ยังไม่กดรับ)
    public string UpdatedDate { get; set; } = string.Empty;
}

/// รายการ checklist หนึ่งข้อ (ก่อนมอบหมายงาน)
public class NisChecklistItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Done { get; set; }
}

/// อัปเดตรายละเอียดงาน + checklist ของ ticket (ก่อนมอบหมาย)
public class NisTicketTaskUpdateDto
{
    public string? WorkDetail { get; set; }
    public List<NisChecklistItemDto> Checklist { get; set; } = new();
    public string? CmpId { get; set; }
    public string? UpdatedBy { get; set; }
}

public class NisTicketCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Assignee { get; set; } = "-";
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Due { get; set; }
    public int Pct { get; set; } = 0;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public List<string>? Tags { get; set; }
    /// Checklist เตรียมไว้ก่อนมอบหมาย (resolve ตาม ticket type + customer ฝั่ง frontend)
    public List<NisChecklistItemDto> Checklist { get; set; } = new();
    public string? CmpId { get; set; }
}

public class NisTicketAssignDto
{
    public string Assignee { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? CmpId { get; set; }

    /// Accounts.Username ของผู้กดมอบหมาย (CRM ส่ง userlogin · RN ส่ง username จาก session)
    /// เก็บลง NisTicket.AssignedBy → ใช้เป็นผู้รับแจ้งเตือนตอนช่างกดรับงาน
    public string? UpdatedBy { get; set; }
}

public class NisTicketStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? CmpId { get; set; }
    public string? UpdatedBy { get; set; }
}

/// % ความคืบหน้าที่แอปช่างคำนวณเอง (milestone: รับงาน 10 · เช็คอิน 25 · checklist 25→85 · เช็คเอาท์ 90)
public class NisTicketProgressDto
{
    public int Pct { get; set; }
    public string? CmpId { get; set; }
    public string? UpdatedBy { get; set; }
}

/// ช่างกดรับงาน (accept) จากแอปหน้างาน — Scheduled → In Progress + แจ้งเตือน SM
public class NisTicketAcceptDto
{
    /// FullName ของช่างที่กดรับ (ใช้ยืนยันว่าเป็นผู้รับผิดชอบตั๋ว + แสดงในข้อความแจ้งเตือน)
    public string? AcceptedBy { get; set; }
    public string? CmpId { get; set; }
}

/// Manager decision on a ticket's close-approval request (from the onsite form).
public class NisTicketCloseDto
{
    public bool Approved { get; set; }
    public string? Reason { get; set; }
    public string? CmpId { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectedBy { get; set; }
}

// ── Project DTOs ─────────────────────────────────────────────────────────────

public class NisProjectResponseDto
{
    public string Id { get; set; } = string.Empty;
    /// Yearly running number per company, e.g. "NIS-2600001"
    public string? ProjectNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Staff { get; set; } = string.Empty;
    public string SoRef { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public NisContactDto? Contact { get; set; }
    public NisSalesPMDto? SalesPM { get; set; }
    public NisEngineerDto? Engineer { get; set; }
    public string? Location { get; set; }
    public List<NisTicketResponseDto> Tickets { get; set; } = new();
    public List<NisAttachmentDto> Attachments { get; set; } = new();
}

public class NisProjectCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string Type { get; set; } = "Implement";
    public string Priority { get; set; } = "Medium";
    public int Progress { get; set; } = 0;
    public string Status { get; set; } = "Active";
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Staff { get; set; } = string.Empty;
    public string SoRef { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public NisContactDto? Contact { get; set; }
    public NisSalesPMDto? SalesPM { get; set; }
    public NisEngineerDto? Engineer { get; set; }
    public string? Location { get; set; }
    public List<NisTicketCreateDto> Tickets { get; set; } = new();
    public string? CmpId { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Partial update for an existing NIS project. Only non-null fields are applied,
/// so the client can PUT just the changed field (e.g. Location) without resending
/// the whole project. Matches frontend updateNisProjectLocation.
/// </summary>
public class NisProjectUpdateDto
{
    public string? Location { get; set; }
    public string? CmpId { get; set; }
    public string? UpdatedBy { get; set; }
}

// ── System Config DTOs ───────────────────────────────────────────────────────

public class NisWarningDaysDto
{
    public int Service { get; set; } = 60;
    public int Product { get; set; } = 30;
}

/// Template อีเมลของระบบ NIS — Id เป็นคีย์คงที่ที่ฝั่ง client ใช้ค้นหา (เช่น "close-job")
public class NisEmailTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    /// HTML body — ใส่ตัวแปรรูปแบบ [TK_NUMBER] ได้
    public string Body { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}

/// ลายเซ็นอีเมล — ชื่อ/ตำแหน่ง/มือถือ ปล่อยว่างได้เมื่อ UseLoginName = true (client เติมจากผู้ล็อกอิน)
public class NisEmailSignatureDto
{
    public bool Enabled { get; set; } = true;
    public bool UseLoginName { get; set; } = true;
    public string SenderName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string CompanyNameTh { get; set; } = string.Empty;
    public string CompanyNameEn { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
}

public class NisSystemConfigResponseDto
{
    public List<string> JobTypes { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> ImplementChecklist { get; set; } = new();
    public List<string> MaChecklist { get; set; } = new();
    public List<string> PmChecklist { get; set; } = new();
    /// checklist มาตรฐานตามประเภท ticket — ticketType → items
    public Dictionary<string, List<string>> ChecklistByTicketType { get; set; } = new();
    /// checklist เฉพาะลูกค้า — customerCode → (ticketType → items)
    public Dictionary<string, Dictionary<string, List<string>>> ChecklistByCustomer { get; set; } = new();
    public List<string> SlaOptions { get; set; } = new();
    public NisWarningDaysDto WarningDays { get; set; } = new();
    /// template อีเมลของระบบ (ปิดงาน / ใบเสนอราคา / ต่ออายุ MA / รับงาน)
    public List<NisEmailTemplateDto> EmailTemplates { get; set; } = new();
    /// ลายเซ็นที่ต่อท้าย body ของทุก template
    public NisEmailSignatureDto EmailSignature { get; set; } = new();
}

public class NisSystemConfigSaveDto
{
    public List<string> JobTypes { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> ImplementChecklist { get; set; } = new();
    public List<string> MaChecklist { get; set; } = new();
    public List<string> PmChecklist { get; set; } = new();
    /// checklist มาตรฐานตามประเภท ticket — ticketType → items
    public Dictionary<string, List<string>> ChecklistByTicketType { get; set; } = new();
    /// checklist เฉพาะลูกค้า — customerCode → (ticketType → items)
    public Dictionary<string, Dictionary<string, List<string>>> ChecklistByCustomer { get; set; } = new();
    public List<string> SlaOptions { get; set; } = new();
    public NisWarningDaysDto WarningDays { get; set; } = new();
    /// template อีเมลของระบบ (ปิดงาน / ใบเสนอราคา / ต่ออายุ MA / รับงาน)
    public List<NisEmailTemplateDto> EmailTemplates { get; set; } = new();
    /// ลายเซ็นที่ต่อท้าย body ของทุก template
    public NisEmailSignatureDto EmailSignature { get; set; } = new();
    public string CmpId { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

// ── Sales Order DTOs ─────────────────────────────────────────────────────────

public class NisSalesOrderResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string QuoteRef { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Project { get; set; }
    public string? PoNumber { get; set; }
    public string? PoDate { get; set; }
    public string? SalesName { get; set; }
}

// ── Pending Request DTOs (Staff "open ticket" request → manager approve/reject) ──

public class NisPendingRequestResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string Due { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Detail { get; set; }
    public bool NoOnsite { get; set; }
    public bool SkipSignature { get; set; }
    public bool RequireCloseApproval { get; set; }
    public string? RequestTime { get; set; }
    public string? SupportMethod { get; set; }
    public string? ParentTicketId { get; set; }
    /// Pending | Approved | Rejected
    public string? Status { get; set; }
}

public class NisPendingRequestCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string SupportMethod { get; set; } = string.Empty;
    public string? ProjectId { get; set; }
    public string? Location { get; set; }
    public string Due { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? CmpId { get; set; }
    public string? RequestedBy { get; set; }
}

public class NisApprovePendingDto
{
    public string SupportMethod { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool NoOnsite { get; set; }
    public bool SkipSignature { get; set; }
    /// Engineer the created ticket is assigned to. "-" leaves it unassigned.
    public string Assignee { get; set; } = "-";
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? CmpId { get; set; }
    public string? ApprovedBy { get; set; }
}

public class NisRejectPendingDto
{
    public string? CmpId { get; set; }
    public string? RejectedBy { get; set; }
}

// ── Customer directory DTOs (Service Board → Customer tab) ───────────────────
// Matches frontend INisBoardCustomer / INisCustomerContact / INisCustomerLocation.

public class NisCustomerContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class NisCustomerLocationDto
{
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string> AssignedStaff { get; set; } = new();
    /// "lat,lon"
    public string? Coordinates { get; set; }
    /// Google Maps link (mCustomerLocations.LocationURL)
    public string? LocationUrl { get; set; }
}

/// Save payload for the Customer tab — writes contacts (dbo.Contact) + locations
/// (msb.mCustomerLocations) for an existing master customer. Does NOT touch
/// msb.mCustomer or mCustomerAssignEmp.
public class NisCustomerSaveDto
{
    /// CustomerCode of the (already existing) master customer.
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public List<NisCustomerContactDto> Contacts { get; set; } = new();
    public List<NisCustomerLocationDto> Locations { get; set; } = new();
    public string? Cmpid { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class NisBoardCustomerDto
{
    /// CustomerCode
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    /// Customer-level caretakers from msb.mCustomerAssignEmp (→ Account.FullName).
    public List<string> AssignedStaff { get; set; } = new();
    public List<NisCustomerContactDto> Contacts { get; set; } = new();
    public List<NisCustomerLocationDto> Locations { get; set; } = new();
}
