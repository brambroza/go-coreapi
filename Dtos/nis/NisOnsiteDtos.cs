namespace goalongapi.Dtos.Nis;

// ── NIS Onsite Form — matches frontend src/types/nis/onsite-form.ts ──────────
// Backed by the existing ServiceTicket / ServiceTicketSubTask / ServiceTicketSubTaskAction
// tables (same data the Staff Portal "My Tasks" board uses) — not a separate table set.

public class NisOnsiteTicketResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    /// Customer master code — needed by the frontend to auto-create a linked Helpdesk case (IServiceProblem.customerCode).
    public string? CustomerCode { get; set; }
    public string Location { get; set; } = string.Empty;
    /// Install | MA | PM
    public string TicketType { get; set; } = "Install";
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? SalesName { get; set; }
    public string? SalesNickname { get; set; }
    public string? SalesPhone { get; set; }
    public string? SalesEmail { get; set; }
    public string? SalesRole { get; set; }
    public string EngineerName { get; set; } = "-";
    public string EngineerNick { get; set; } = string.Empty;
    public string? EngineerPhone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public bool SkipSignature { get; set; }
    public bool RequireCloseApproval { get; set; }
    public bool Accepted { get; set; } = true;
    public bool? NoOnsite { get; set; }
    public int? MaRoundCurrent { get; set; }
    public int? MaRoundTotal { get; set; }

    /// สถานะ Check-in ที่ server รู้ (NisTicket) — ค่าว่าง = ยังไม่เช็คอิน
    /// CRM ใช้เติมขั้น Check-in เมื่อช่างเช็คอินจากแอป RN (ไม่มี draft บนเครื่อง CRM)
    public string? CheckInTime { get; set; }
    public double? CheckInLat { get; set; }
    public double? CheckInLng { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckInBy { get; set; }

    /// สถานะ Check-out ที่ server รู้ (NisTicket) — ค่าว่าง = ยังไม่เช็คเอาท์
    /// CRM ใช้เติมขั้น Check-out เมื่อช่างเช็คเอาท์จากแอป RN (draft ของ user CRM ไม่มี)
    public string? CheckOutTime { get; set; }
    public double? CheckOutLat { get; set; }
    public double? CheckOutLng { get; set; }
    public string? CheckOutLocation { get; set; }
    public string? CheckOutBy { get; set; }

    /// ทะเบียนอุปกรณ์ในตู้ Rack ของลูกค้า (flat) — เติมเฉพาะตั๋ว PM; client จัดกลุ่มตาม RackName เอง
    /// null = ไม่ใช่ PM หรือไม่มีทะเบียน
    public List<NisOnsiteEquipmentDto>? Equipment { get; set; }
}

/// อุปกรณ์ 1 ชิ้นจากทะเบียน NisEquipment สำหรับหน้า onsite (= NisEquipmentDto + UPosition/RackLocation)
public class NisOnsiteEquipmentDto
{
    public string Id { get; set; } = string.Empty;
    public string RackName { get; set; } = string.Empty;
    public string RackLocation { get; set; } = string.Empty;
    public int? UPosition { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class NisOnsiteSrNumberRequestDto
{
    public string? CmpId { get; set; }
    public string? User { get; set; }
}

public class NisOnsiteChecklistItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Checked { get; set; }
}

/// 1 item = 1 ตู้ Rack — Name/Location/Remark/BeforePhoto/AfterPhoto = ชื่อตู้/ที่ตั้ง/หมายเหตุ/รูปตู้ (ความหมายเดิม)
public class NisOnsitePmItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string? BeforePhoto { get; set; }
    public string? AfterPhoto { get; set; }

    /// = NisEquipment.RackName — ใช้ merge/dedupe กับทะเบียน (null = item แบบเก่า)
    public string? RackKey { get; set; }

    /// ผลตรวจอุปกรณ์รายชิ้นในตู้ — null = item แบบเก่า (legacy, ไม่มีการตรวจรายชิ้น)
    public List<NisOnsitePmDeviceDto>? Devices { get; set; }
}

/// ผลตรวจอุปกรณ์ 1 ชิ้นในตู้ Rack (งาน PM)
public class NisOnsitePmDeviceDto
{
    /// NisEquipment.Id — null เมื่อช่างกรอกเอง (Source = manual)
    public string? EquipmentId { get; set; }
    public int? UPosition { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    /// master | manual
    public string Source { get; set; } = "master";
    /// normal | abnormal | null (ยังไม่ตรวจ)
    public string? Status { get; set; }
    /// อาการที่พบ — บังคับเมื่อ Status = abnormal
    public string Symptom { get; set; } = string.Empty;
}

public class NisOnsiteDamagedProductDto
{
    public bool Checked { get; set; }
    /// on | off
    public string Warranty { get; set; } = "on";
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = new();

    /// ของเสียครบทุกรายการ (อาการ/ความรุนแรง/รูป/เลขเคลม) แบบ opaque JSON ของ client —
    /// เก็บซ้อนใน DamagedProductJson (ไม่ต้องเพิ่มคอลัมน์) เพื่อให้ใบรายงานย้อนหลังแสดงครบเหมือนตอนส่ง
    public List<System.Text.Json.JsonElement>? Reports { get; set; }
}

public class NisOnsiteSupportCaseDto
{
    public long Id { get; set; }
    public string Reporter { get; set; } = string.Empty;
    /// Hardware | Software | Network | Cabling
    public string Category { get; set; } = "Hardware";
    public string Problem { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    /// ผลลัพธ์หน้างาน: resolved (ปิดเคส Finish) | followup (เคสค้าง Inprogress) — null = ของเดิมก่อนมีช่องนี้
    public string? Result { get; set; }
    /// LINE userId ของผู้แจ้ง เมื่อเลือกจาก LINE contact (ว่าง = พิมพ์ชื่อเอง)
    public string? ReporterLineUserId { get; set; }
}

/// Shared base for submit + request-close (mirrors Omit<INisOnsiteSubmitPayload, 'recipientEmail'|'emailSubject'> on the frontend).
public class NisOnsiteReportBaseDto
{
    public string SrNumber { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public double? CheckInLat { get; set; }
    public double? CheckInLng { get; set; }
    public double? CheckOutLat { get; set; }
    public double? CheckOutLng { get; set; }
    public List<NisOnsiteChecklistItemDto> Checklist { get; set; } = new();
    public string? WorkDetail { get; set; }
    public string? IssueDetail { get; set; }
    public List<string> Photos { get; set; } = new();
    public List<NisOnsitePmItemDto> PmItems { get; set; } = new();
    public NisOnsiteDamagedProductDto? DamagedProduct { get; set; }
    /// ของเสียครบทุกรายการ (shape DamageReport ของ RN / INisOnsiteDamageReport ของ CRM) — optional,
    /// client เก่าไม่ส่ง · ถูกเก็บลง DamagedProductJson.Reports
    public List<System.Text.Json.JsonElement>? DamageReports { get; set; }
    public List<NisOnsiteSupportCaseDto> SupportCases { get; set; } = new();

    // รูปเดี่ยวระดับ report (ก่อน/ระหว่าง/หลังงาน) — คนละความหมายกับ NisOnsitePmItemDto.BeforePhoto/
    // AfterPhoto (รูปต่อ PM item)
    public string? BeforePhoto { get; set; }
    public string? DuringPhoto { get; set; }
    public string? AfterPhoto { get; set; }

    public string? SignatureImg { get; set; }
    public bool SkipSignature { get; set; }
    public string? CmpId { get; set; }
    public string? User { get; set; }
}

public class NisOnsiteSubmitDto : NisOnsiteReportBaseDto
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string EmailSubject { get; set; } = string.Empty;
    /// Optional message entered by the engineer in the close-job email modal.
    public string EmailMessage { get; set; } = string.Empty;

    /// ชื่อผู้ติดต่อฝั่งลูกค้า — เติมตัวแปร [CONTACT] ใน email template (ว่าง = ใช้ชื่อลูกค้าแทน)
    public string? ContactName { get; set; }

    /// ชื่อ/ตำแหน่ง/มือถือ ของผู้ล็อกอินที่ปิดงาน — ใช้เติมลายเซ็นอีเมลเมื่อ UseLoginName = true
    /// (ว่าง = backend หา FullName จาก Accounts เอง แล้ว fallback ค่าที่ตั้งไว้ในหน้า System Config)
    public string? SenderName { get; set; }
    public string? SenderPosition { get; set; }
    public string? SenderMobile { get; set; }

    /// Client-generated Service Report PDF, base64 (no data-URI prefix). Optional: older
    /// clients omit it and the email is sent without an attachment (backward-compatible).
    public string? ReportPdfBase64 { get; set; }

    /// Suggested attachment file name; backend falls back to "Service-Report-{SrNumber}.pdf".
    public string? ReportPdfFileName { get; set; }
}

/// request-close — ฟิลด์อีเมล/PDF ใช้ชื่อเดียวกับ NisOnsiteSubmitDto (recipientEmail, emailSubject, emailMessage,
/// contactName, senderName/Position/Mobile, reportPdfBase64, reportPdfFileName) แต่เป็น nullable ทั้งหมด
/// (client เก่าที่ไม่ส่งยังใช้ได้) — เก็บลง report แล้วส่งอีเมลจริงตอน SM อนุมัติ (close-approve)
/// SrNumber ที่ client ส่งมาจะถูกละเว้นถ้ามีแถว reserved จาก POST onsite/{id}/reserve-sr อยู่แล้ว
public class NisOnsiteRequestCloseDto : NisOnsiteReportBaseDto
{
    public string? RecipientEmail { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailMessage { get; set; }
    public string? ContactName { get; set; }
    public string? SenderName { get; set; }
    public string? SenderPosition { get; set; }
    public string? SenderMobile { get; set; }

    /// PDF ใบรายงาน (base64 ไม่มี data-URI prefix) ที่ render ด้วยเลข SR จาก reserve-sr — ≤ 7.5 MB หลัง decode
    public string? ReportPdfBase64 { get; set; }
    public string? ReportPdfFileName { get; set; }
}

/// ข้อมูลอีเมลปิดงานที่เก็บใน NisOnsiteReport.CloseEmailJson (ตอน request-close) เพื่อส่งตอน close-approve
public class NisOnsiteStoredCloseEmailDto
{
    public string? RecipientEmail { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailMessage { get; set; }
    public string? ContactName { get; set; }
    public string? SenderName { get; set; }
    public string? SenderPosition { get; set; }
    public string? SenderMobile { get; set; }
    public string? ReportPdfFileName { get; set; }
}

/// POST api/nis/onsite/{id}/reserve-sr — body { cmpId, user? }
public class NisOnsiteReserveSrDto
{
    public string? CmpId { get; set; }
    /// ผู้จอง (userlogin) — เก็บเป็น Engineer ของแถว reserved (ว่าง = ใช้ Assignee ของตั๋ว)
    public string? User { get; set; }
}

/// ผลอัปโหลดรูป 1 ไฟล์ — POST api/nis/tickets/{ticketId}/photos
public class NisTicketPhotoUploadItemDto
{
    /// URL เต็ม ("{serverUrl}/{cmpId}/nis/{projectNo}/tickets/{ticketId}/photos/{group}/{guid}.jpg")
    public string Url { get; set; } = string.Empty;
    /// ชื่อไฟล์เดิม (ใช้แสดงผลเท่านั้น — ชื่อไฟล์จริงบน server เป็น guid)
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class NisTicketPhotoUploadResponseDto
{
    public List<NisTicketPhotoUploadItemDto> Files { get; set; } = new();
}

// ── Service Report (list) — GET api/nis/onsite/reports · map ตรง RN ServiceReport ──
public class NisServiceReportChecklistDto
{
    public string Label { get; set; } = string.Empty;
    public bool Done { get; set; }
}

public class NisServiceReportDto
{
    /// = SrNumber (RN ServiceReport.id ใช้เลข SR)
    public string Id { get; set; } = string.Empty;
    public string SrNumber { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string? TicketCode { get; set; }
    public string TicketTitle { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Engineer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string? WorkNote { get; set; }
    public string? WorkDetail { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<NisServiceReportChecklistDto> Checklist { get; set; } = new();
    public string? SignatureImg { get; set; }
    public bool SkipSignature { get; set; }

    public List<string> Photos { get; set; } = new();
    public List<NisOnsitePmItemDto> PmItems { get; set; } = new();
    public NisOnsiteDamagedProductDto? DamagedProduct { get; set; }
    public string? BeforePhoto { get; set; }
    public string? DuringPhoto { get; set; }
    public string? AfterPhoto { get; set; }

    /// 'YYYY-MM-DD'
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "Closed";

    /// NisOnsiteReport.ReportId — ใช้เปิดไฟล์ PDF ที่ส่งลูกค้าจริง (GET onsite/reports/{reportId}/pdf)
    public string ReportId { get; set; } = string.Empty;

    /// มีไฟล์ PDF ฉบับที่ส่งลูกค้าเก็บไว้หรือไม่ (false = ใบเก่า/client ไม่แนบ → client สร้างเอกสารจากข้อมูลแทน)
    public bool HasPdf { get; set; }

    /// ของเสียครบทุกรายการตามที่ส่งตอนปิดงาน (DamagedProductJson.Reports) — ใบเก่าว่าง
    public List<System.Text.Json.JsonElement> DamageReports { get; set; } = new();

    /// เคส support ที่บันทึกตอนปิดงาน (SupportCasesJson)
    public List<NisOnsiteSupportCaseDto> SupportCases { get; set; } = new();

    /// ผู้รับมอบงาน ณ ตอนปิดงาน (CloseEmailJson.ContactName) — ไม่มี = ผู้ติดต่อของโครงการปัจจุบัน
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
}
