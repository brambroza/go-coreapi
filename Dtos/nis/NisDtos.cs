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
    /// อีเมลเซลผู้ดูแล — แสดงตามไปทุกหน้า ticket/project (CRM + RN)
    public string? Email { get; set; }
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
    /// วันเวลาที่ปิดงาน (yyyy-MM-dd HH:mm) — ค่าว่าง = ยังไม่ปิด · ใช้เรียงงานที่ปิดแล้วล่าสุดก่อน
    public string ClosedDate { get; set; } = string.Empty;
    /// วันเวลาที่ช่าง Check-in (yyyy-MM-dd HH:mm) — ค่าว่าง = ยังไม่เช็คอิน
    public string CheckInTime { get; set; } = string.Empty;
    public double? CheckInLat { get; set; }
    public double? CheckInLng { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckInBy { get; set; }
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

/// ช่าง Check-in (PUT tickets/{id}/checkin) — เวลาเช็คอินใช้เวลา server เสมอ (client ส่งแค่พิกัด/สถานที่)
public class NisTicketCheckInDto
{
    public string? CmpId { get; set; }
    /// ผู้เช็คอิน (username หรือ FullName ของช่าง)
    public string? CheckInBy { get; set; }
    /// null = เช็คอินโดยไม่มีพิกัด GPS
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    /// ชื่อสถานที่ที่ช่างยืนเช็คอิน (reverse geocode)
    public string? Location { get; set; }
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
    /// วันที่สร้างโครงการ (yyyy-MM-dd HH:mm)
    public string CreatedDate { get; set; } = string.Empty;
    /// เงื่อนไขบริการ (สัญญา) ที่เลือกตอนสร้างโครงการ — null สำหรับโครงการเก่า
    public NisServiceConditionsDto? ServiceConditions { get; set; }
    /// NisEquipment.Id ที่เลือกไว้ตอนสร้างโครงการ (PM) — ว่าง = ไม่ได้เลือก/โครงการเก่า
    public List<string> EquipmentIds { get; set; } = new();
    public List<NisTicketResponseDto> Tickets { get; set; } = new();
    public List<NisAttachmentDto> Attachments { get; set; } = new();
}

/// เงื่อนไขบริการของโครงการ (สัญญา) — เก็บเป็น JSON บน NisProject.ServiceConditionsJson
public class NisServiceConditionsDto
{
    public string ServiceYears { get; set; } = string.Empty;
    public string OnsitePerYear { get; set; } = string.Empty;
    public string PmPerYear { get; set; } = string.Empty;
    public string Sla { get; set; } = string.Empty;
    public string ServiceReplacement { get; set; } = string.Empty;
    public string RemoteBackup { get; set; } = string.Empty;
    public string MonthlyReport { get; set; } = string.Empty;
    public string MonthlyReportDay { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public string DeliveryBy { get; set; } = string.Empty;
    /// เคส Accident — ตั๋ว onsite สร้างครบโควตาแต่ไม่กำหนดรอบ/วันล่วงหน้า
    public bool OnsiteAccident { get; set; } = false;
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
    /// เงื่อนไขบริการ (สัญญา) — client เก่าไม่ส่งได้ (null = ไม่บันทึก)
    public NisServiceConditionsDto? ServiceConditions { get; set; }
    /// NisEquipment.Id ที่เลือกไว้ตอนสร้างโครงการ (PM) — client เก่าไม่ส่งได้
    public List<string> EquipmentIds { get; set; } = new();
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

/// ตัวเลือกแบบ value/label สำหรับ select (deliveryType ฯลฯ)
public class NisOptionItemDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// ค่า default ของฟอร์มเงื่อนไขในหน้า New Project
public class NisServiceCondDefaultsDto
{
    public string ServiceYears { get; set; } = "1";
    public string OnsitePerYear { get; set; } = "4";
    public string PmPerYear { get; set; } = "4";
    public string Sla { get; set; } = "8x5xNBD";
    public string ServiceReplacement { get; set; } = "company";
    public string RemoteBackup { get; set; } = "4";
    public string MonthlyReport { get; set; } = "4";
    public string MonthlyReportDay { get; set; } = "5";
    public string DeliveryType { get; set; } = "onsite_install";
    public string DeliveryBy { get; set; } = "nis_team";
    /// ค่าเริ่มต้นของ checkbox "เคส Accident" ใน wizard สร้างโครงการ
    public bool OnsiteAccident { get; set; } = false;
}

/// ตัวเลือกเงื่อนไขงานของ wizard สร้างโครงการ — JSON บน NisSystemConfig.ServiceConditionOptionsJson
public class NisServiceConditionOptionsDto
{
    public List<string> ServiceYears { get; set; } = new();
    public List<string> OnsitePerYearImplement { get; set; } = new();
    public List<string> OnsitePerYearMa { get; set; } = new();
    public List<string> PmPerYearImplement { get; set; } = new();
    public List<string> PmPerYearMa { get; set; } = new();
    public List<string> RemoteBackupImplement { get; set; } = new();
    public List<string> RemoteBackupMa { get; set; } = new();
    public List<string> MonthlyReport { get; set; } = new();
    public List<NisOptionItemDto> ServiceReplacement { get; set; } = new();
    public List<NisOptionItemDto> DeliveryType { get; set; } = new();
    public List<NisOptionItemDto> DeliveryBy { get; set; } = new();
    public NisServiceCondDefaultsDto Defaults { get; set; } = new();
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
    /// ตัวเลือกเงื่อนไขงานของ wizard สร้างโครงการ — GET เติม default เสมอ
    public NisServiceConditionOptionsDto ServiceConditionOptions { get; set; } = new();
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
    /// nullable โดยตั้งใจ — client เก่าที่ PUT โดยไม่มี field นี้ต้องไม่ลบค่าที่บันทึกไว้ (null-preserve)
    public NisServiceConditionOptionsDto? ServiceConditionOptions { get; set; }
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
    /// null = ไม่แตะผู้ติดต่อ (dbo.Contact) เลย — ใช้ตอนบันทึกเฉพาะสถานที่
    /// ส่ง list มา = แทนที่ผู้ติดต่อทั้งชุดตามเดิม
    public List<NisCustomerContactDto>? Contacts { get; set; }
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

// ── Equipment master DTOs (dbo.NisEquipment) — อุปกรณ์ใน Rack ต่อลูกค้า ──────────
// Matches frontend INisEquipment. ผูกกับ customerCode อย่างเดียว ไม่แยกตาม location.

public class NisEquipmentDto
{
    /// null/ว่าง เมื่อเป็นแถวใหม่ที่ยังไม่บันทึก
    public string? Id { get; set; }
    public string RackName { get; set; } = string.Empty;
    /// ที่ตั้งของตู้ — ซ้ำกันทุกแถวของตู้เดียวกัน
    public string RackLocation { get; set; } = string.Empty;
    /// ตำแหน่ง U ในตู้ — null = ไม่ระบุ
    public int? UPosition { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

/// Save payload สำหรับหน้า master — replace-all รายการอุปกรณ์ทั้งชุดของลูกค้ารายนี้
public class NisEquipmentSaveDto
{
    public string? Cmpid { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public List<NisEquipmentDto> Items { get; set; } = new();
}

/// ผลลัพธ์ import Excel — เพิ่มเข้าไปในของเดิม (append) ไม่ลบของเก่า
public class NisEquipmentImportResultDto
{
    public int Imported { get; set; }
    public List<NisEquipmentDto> Items { get; set; } = new();
}

/// สรุปจำนวนอุปกรณ์ต่อลูกค้า — ใช้แสดงหน้าแรกของ Equipment Master ว่าลูกค้าเจ้าไหนบันทึกไว้แล้วบ้าง
public class NisEquipmentCustomerSummaryDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Count { get; set; }
}

// ── Equipment change request DTOs (dbo.NisEquipmentChangeRequest) ──────────────
// ช่างแจ้งแก้ทะเบียนตู้ Rack จากหน้า onsite (PM) → SM อนุมัติใน Service Board
// ทะเบียน NisEquipment ไม่เปลี่ยนจนกว่าจะอนุมัติ

/// แถวคำขอแก้ทะเบียน + ชื่อลูกค้า/รหัสตั๋ว (join ตอนอ่าน)
public class NisEquipmentChangeRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? TicketId { get; set; }
    public string? TicketCode { get; set; }
    public string RackName { get; set; } = string.Empty;
    /// missing_in_registry | not_in_rack | incorrect_info
    public string Reason { get; set; } = string.Empty;
    /// NisEquipment.Id เป้าหมาย — บังคับเมื่อ Reason ≠ missing_in_registry
    public string? EquipmentId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public int? UPosition { get; set; }
    public string Note { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    /// Pending | Approved | Rejected
    public string Status { get; set; } = "Pending";
    public string? ApprovedBy { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// Payload POST api/nis/equipment-change-requests
public class NisEquipmentChangeRequestCreateDto
{
    public string? CmpId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string? TicketId { get; set; }
    public string RackName { get; set; } = string.Empty;
    /// missing_in_registry | not_in_rack | incorrect_info
    public string Reason { get; set; } = string.Empty;
    public string? EquipmentId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public int? UPosition { get; set; }
    public string Note { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
}

/// Payload PUT api/nis/equipment-change-requests/{id}/approve
public class NisEquipmentChangeRequestApproveDto
{
    public string? CmpId { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
}

/// Payload PUT api/nis/equipment-change-requests/{id}/reject
public class NisEquipmentChangeRequestRejectDto
{
    public string? CmpId { get; set; }
    public string RejectedBy { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
}
