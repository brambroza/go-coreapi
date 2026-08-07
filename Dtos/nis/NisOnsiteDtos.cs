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

public class NisOnsitePmItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string? BeforePhoto { get; set; }
    public string? AfterPhoto { get; set; }
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
    public List<NisOnsiteSupportCaseDto> SupportCases { get; set; } = new();
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

    /// Client-generated Service Report PDF, base64 (no data-URI prefix). Optional: older
    /// clients omit it and the email is sent without an attachment (backward-compatible).
    public string? ReportPdfBase64 { get; set; }

    /// Suggested attachment file name; backend falls back to "Service-Report-{SrNumber}.pdf".
    public string? ReportPdfFileName { get; set; }
}

public class NisOnsiteRequestCloseDto : NisOnsiteReportBaseDto
{
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
    /// 'YYYY-MM-DD'
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "Closed";
}
