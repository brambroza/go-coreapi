using System.Text.Json;
using System.Globalization;
using ClosedXML.Excel;

using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models;
using goalongapi.Models.Nis;
using goalongapi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Service Project Portal — project/ticket-generation endpoints, plus the NIS Onsite
/// Form endpoints (which read/write the existing ServiceTicket / ServiceTicketSubTask /
/// ServiceTicketSubTaskAction tables — the same data ServiceTicketsController and the Staff
/// Portal "My Tasks" board use — rather than a separate table set).
/// Route prefix: api/nis
/// Auth: No [Authorize] — consistent with ServiceTicketsController pattern in this project
/// </summary>
[ApiController]
[Route("api/nis")]
public class NisController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly EmailSettingRepository _emailRepo;
    private readonly AesCrypto _crypto;
    private readonly GoogleOAuthMailService _googleOAuthMail;
    private readonly GoogleOAuthCalendarService _googleOAuthCalendar;
    private readonly goalongapi.Services.ExpoPushService _push;
    private readonly goalongapi.Services.NisRealtimeNotifyService _nisRealtimeNotify;
    private readonly goalongapi.Services.NisCrmNotifyService _crmNotify;
    private readonly NisReportPdfStorage _pdfStorage;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<NisController> _logger;

    /// Feature flag (NisOnsite:AttachReportPdf, default true) — lets ops disable PDF attachment
    /// instantly if the shared mail path misbehaves, without a redeploy.
    private readonly bool _attachReportPdf;

    /// base URL สาธารณะของ API (NisOnsite:PublicBaseUrl) — ใช้ประกอบ URL โลโก้บริษัทในลายเซ็นอีเมล
    /// เว้นว่างได้ (จะ fallback ไป host ของ request) แต่ตั้งไว้จะแน่นอนกว่าเมื่ออยู่หลัง reverse proxy
    private readonly string? _publicBaseUrl;

    public NisController(
        DatabaseContext context,
        EmailSettingRepository emailRepo,
        AesCrypto crypto,
        GoogleOAuthMailService googleOAuthMail,
        GoogleOAuthCalendarService googleOAuthCalendar,
        goalongapi.Services.ExpoPushService push,
        goalongapi.Services.NisRealtimeNotifyService nisRealtimeNotify,
        goalongapi.Services.NisCrmNotifyService crmNotify,
        NisReportPdfStorage pdfStorage,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        ILogger<NisController> logger)
    {
        _context = context;
        _emailRepo = emailRepo;
        _crypto = crypto;
        _googleOAuthMail = googleOAuthMail;
        _googleOAuthCalendar = googleOAuthCalendar;
        _push = push;
        _nisRealtimeNotify = nisRealtimeNotify;
        _crmNotify = crmNotify;
        _pdfStorage = pdfStorage;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _attachReportPdf = configuration.GetValue<bool?>("NisOnsite:AttachReportPdf") ?? true;
        _publicBaseUrl = configuration.GetValue<string?>("NisOnsite:PublicBaseUrl");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DateTime BangkokNow() =>
        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;

    private static List<string> SplitTags(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? new List<string>()
            : raw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

    private static string JoinTags(IEnumerable<string>? tags) =>
        tags == null ? string.Empty : string.Join('|', tags.Where(t => !string.IsNullOrWhiteSpace(t)));

    private static string FormatDate(DateTime? d) =>
        d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatDateTime(DateTime? d) =>
        d?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatProjectNo(string? no) =>
        int.TryParse(no, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value.ToString("D4", CultureInfo.InvariantCulture)
            : no?.Trim() ?? string.Empty;

    /// Short code prefix per ticket Type, used to build TicketCode (TK-{Prefix}-{ProjectNo}-{RunNo}).
    private static readonly Dictionary<string, string> TicketTypePrefixes = new()
    {
        ["Install"] = "INS",
        ["PM"] = "PM",
        ["MA Onsite"] = "ONS",
        ["Support"] = "SUP",
        ["Backup"] = "BK",
        ["Report"] = "RPT",
        ["Delivery"] = "DLV",
        ["MA"] = "MA",
    };

    // ── ความคืบหน้าตั๋ว NIS (NisTicket.Pct) — milestone ชุดเดียวใช้ทุก client ────────────
    // 0 ยังไม่รับงาน · 10 ช่างกดรับงาน · 25 เช็คอินถึงหน้างาน
    // · 25→85 ติ๊ก checklist ตามสัดส่วน · 90 เช็คเอาท์/รออนุมัติปิด · 100 ปิดงาน
    // ค่าเดียวกันนี้ mirror อยู่ฝั่ง RN ที่ NIS-OnsiteService/src/utils/progress.ts — แก้ต้องแก้คู่กัน
    private const int NIS_PCT_NOT_ACCEPTED = 0;
    private const int NIS_PCT_ACCEPTED = 10;
    private const int NIS_PCT_CHECKED_IN = 25;
    private const int NIS_PCT_CHECKLIST_DONE = 85;
    private const int NIS_PCT_CHECKED_OUT = 90;
    private const int NIS_PCT_CLOSED = 100;

    private static string TicketPrefixFor(string? type) =>
        !string.IsNullOrWhiteSpace(type) && TicketTypePrefixes.TryGetValue(type, out var prefix)
            ? prefix
            : "TK";

    private static string BuildTicketCode(string? type, string? projectNo, int runNo) =>
        $"TK-{TicketPrefixFor(type)}-{FormatProjectNo(projectNo)}-{runNo:D2}";

    private static NisTicketResponseDto MapTicket(NisTicket t) => new()
    {
        Id = t.TicketId,
        Code = t.TicketCode,
        Title = t.Title,
        Status = t.Status,
        Assignee = t.Assignee,
        StartDate = FormatDate(t.StartDate),
        EndDate = FormatDate(t.EndDate),
        Due = FormatDate(t.Due),
        Pct = t.Pct,
        Type = t.Type,
        TicketType = t.Type,
        Priority = t.Priority,
        Tags = SplitTags(t.TagsRaw),
        WorkDetail = t.WorkDetail,
        Checklist = ParseChecklist(t.ChecklistJson),
        CreatedDate = FormatDateTime(t.CreatedDate),
        UpdatedDate = FormatDateTime(t.UpdatedDate),
        ClosedDate = FormatDateTime(t.ClosedDate),
        CheckInTime = FormatDateTime(t.CheckInTime),
        CheckInLat = t.CheckInLat,
        CheckInLng = t.CheckInLng,
        CheckInLocation = t.CheckInLocation,
        CheckInBy = t.CheckInBy,
        CheckOutTime = FormatDateTime(t.CheckOutTime),
        CheckOutLat = t.CheckOutLat,
        CheckOutLng = t.CheckOutLng,
        CheckOutLocation = t.CheckOutLocation,
        CheckOutBy = t.CheckOutBy,
        SourceProblemId = t.SourceProblemId,
        OverQuota = t.OverQuota,
        CustomerLineUserId = t.CustomerLineUserId,
        CustomerOaLineId = t.CustomerOaLineId,
        Files = (t.Files ?? Array.Empty<NisTicketFile>()).OrderBy(f => f.Seq).Select(MapTicketFile).ToList(),
    };

    private static NisAttachmentDto MapTicketFile(NisTicketFile f) => new()
    {
        Id = f.FileId,
        FileName = f.FileName,
        FilePath = f.FilePath,
        Seq = f.Seq,
        FileSize = f.FileSize,
    };

    /// ประเภทตั๋วที่นับเข้าโควตา Onsite Incident case — ตรงกับ NisTicketType ฝั่ง frontend
    private const string OnsiteTicketType = "MA Onsite";

    /// สถานะตั๋วที่เปิดจากเคส helpdesk (ปุ่ม "เปิดเคส Onsite") และยังรอ SM อนุมัติ
    /// ตั๋วสถานะนี้ตัดโควตา Onsite Incident case แล้ว แต่ห้าม assign / เปลี่ยนสถานะ / accept / check-in
    /// จนกว่าจะอนุมัติผ่าน POST pending-requests/{id}/approve — ปฏิเสธ = ลบตั๋ว (คืนโควตา)
    private const string WaitingApprovalStatus = "Waiting Approval";

    /// รหัส error 409 เมื่อ client พยายามทำงานกับตั๋วที่ยังรออนุมัติ
    private const string TicketWaitingApprovalCode = "TICKET_WAITING_APPROVAL";

    private static bool IsWaitingApproval(NisTicket t) => t.Status == WaitingApprovalStatus;

    private static ObjectResult WaitingApprovalConflict(NisTicket t) => new ConflictObjectResult(new
    {
        code = TicketWaitingApprovalCode,
        message = $"ตั๋ว {t.TicketCode ?? t.TicketId} ยังรอ Service Manager อนุมัติคำขอเปิดเคส — ต้องอนุมัติก่อนจึงทำรายการนี้ได้",
    });

    /// <summary>
    /// แปลงค่า quota จาก select ('0', '4', ...) เป็นจำนวน — ตรงกับ parseTicketCount ฝั่ง frontend
    /// ค่าว่าง/อ่านไม่ออก/ติดลบ → fallback
    /// </summary>
    private static int ParseTicketCount(string? value, int fallback = 0)
    {
        if (int.TryParse((value ?? string.Empty).Trim(), out var n) && n >= 0) return n;
        return fallback;
    }

    /// <summary>
    /// โควตา Onsite ของโครงการIncident case — นับทั้งสัญญาตามระยะโครงการ (ไม่รีเซ็ตรายปี)
    /// quota = onsitePerYear × serviceYears (serviceYears อ่านไม่ออก = 1) · used = ตั๋ว MA Onsite ที่มีอยู่
    /// </summary>
    /// <returns>null เมื่อไม่ใช่โครงการIncident case</returns>
    private static NisOnsiteQuotaDto? ComputeOnsiteQuota(
        NisServiceConditionsDto? cond,
        int onsiteTicketCount,
        int waitingApprovalCount = 0)
    {
        if (cond == null || !cond.OnsiteAccident) return null;
        var quota = ParseTicketCount(cond.OnsitePerYear) * ParseTicketCount(cond.ServiceYears, 1);
        return new NisOnsiteQuotaDto
        {
            Quota = quota,
            Used = onsiteTicketCount,
            Remaining = quota - onsiteTicketCount,
            WaitingApproval = waitingApprovalCount,
        };
    }

    /// โควตา Onsite Incident case ของโครงการจากรายการตั๋วที่โหลดมาแล้ว (ใช้ร่วม MapProject / pending-requests)
    private static NisOnsiteQuotaDto? ComputeOnsiteQuota(NisProject project, IEnumerable<NisTicket> tickets)
    {
        var onsite = tickets.Where(t => t.Type == OnsiteTicketType).ToList();
        return ComputeOnsiteQuota(
            ParseJson<NisServiceConditionsDto?>(project.ServiceConditionsJson, null),
            onsite.Count,
            onsite.Count(IsWaitingApproval));
    }

    /// <summary>
    /// sync ClosedDate ให้ตรงกับสถานะปัจจุบันของตั๋ว — เรียกทุกครั้งหลังเขียน Status
    /// Closed/Done → ตั้งเวลาไทยตอนนี้ถ้ายังว่าง (ปิดซ้ำไม่ทับเวลาปิดเดิม) · สถานะอื่น → ล้างทิ้ง (reopen)
    /// </summary>
    private static void ApplyClosedDate(NisTicket ticket)
    {
        if (ticket.Status is "Closed" or "Done")
            ticket.ClosedDate ??= BangkokNow();
        else
            ticket.ClosedDate = null;
    }

    /// ล้างสถานะ Check-in/Check-out — ตั๋วกลับไปยังไม่รับงาน (Open/Scheduled) = งานรอบใหม่ ช่างต้องเช็คอินใหม่
    private static void ResetCheckInIfNotStarted(NisTicket ticket)
    {
        if (ticket.Status is not ("Open" or "Scheduled")) return;
        ticket.CheckInTime = null;
        ticket.CheckInLat = null;
        ticket.CheckInLng = null;
        ticket.CheckInLocation = null;
        ticket.CheckInBy = null;
        ticket.CheckOutTime = null;
        ticket.CheckOutLat = null;
        ticket.CheckOutLng = null;
        ticket.CheckOutLocation = null;
        ticket.CheckOutBy = null;
    }

    /// milestone ขั้นต่ำของ % จากสถานะหน้างานที่ server รู้จริง (คอลัมน์บน NisTicket)
    /// เช็คเอาท์แล้ว → ไม่ต่ำกว่า 90 · เช็คอินแล้ว → ไม่ต่ำกว่า 25 · อื่น ๆ → 0
    /// ใช้กัน snapshot/autosave จาก client อื่น (เช่น SM เปิดฟอร์มดูใน CRM ซึ่ง draft ของตัวเองไม่มี
    /// checkOutTime) เขียนทับ % ที่ช่างทำไว้จริงให้ถอยหลัง — checklist ยังลดได้ตามปกติเหนือ floor นี้
    private static int TicketPctFloor(NisTicket ticket)
    {
        if (ticket.CheckOutTime != null) return NIS_PCT_CHECKED_OUT;
        if (ticket.CheckInTime != null) return NIS_PCT_CHECKED_IN;
        return NIS_PCT_NOT_ACCEPTED;
    }

    /// normalize + serialize checklist ที่ client ส่งมา — ตัดข้อว่าง, การันตี Id · ว่าง = null (ตรงกับ UpdateTicketTask)
    private static string? SerializeChecklist(IEnumerable<NisChecklistItemDto>? items)
    {
        var list = (items ?? Array.Empty<NisChecklistItemDto>())
            .Where(c => !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => new NisChecklistItemDto
            {
                Id = string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString() : c.Id,
                Text = c.Text.Trim(),
                Done = c.Done,
            })
            .ToList();
        return list.Count == 0 ? null : JsonSerializer.Serialize(list);
    }

    /// แปลง ChecklistJson (nvarchar) → รายการ checklist; ค่าว่าง/พังคืน list ว่าง (ไม่ throw)
    private static List<NisChecklistItemDto> ParseChecklist(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new();
        try
        {
            return JsonSerializer.Deserialize<List<NisChecklistItemDto>>(json) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    private static NisProjectResponseDto MapProject(NisProject p) => new()
    {
        Id = p.ProjectId,
        ProjectNo = FormatProjectNo(p.ProjectNo),
        Name = p.Name,
        Customer = p.Customer,
        CustomerCode = p.CustomerCode,
        Type = p.Type,
        Priority = p.Priority,
        Progress = p.Progress,
        Status = p.Status,
        StartDate = FormatDate(p.StartDate),
        EndDate = FormatDate(p.EndDate),
        Staff = p.Staff,
        SoRef = p.SoRef,
        Tags = SplitTags(p.TagsRaw),
        Location = p.Location,
        CreatedDate = FormatDateTime(p.CreatedDate),
        ServiceConditions = ParseJson<NisServiceConditionsDto?>(p.ServiceConditionsJson, null),
        OnsiteQuota = ComputeOnsiteQuota(p, p.Tickets),
        EquipmentIds = ParseJson<List<string>?>(p.EquipmentIdsJson, null) ?? new(),
        Contact = string.IsNullOrEmpty(p.ContactName) ? null : new NisContactDto
        {
            Name = p.ContactName ?? string.Empty,
            Phone = p.ContactPhone ?? string.Empty,
            Email = p.ContactEmail ?? string.Empty,
        },
        SalesPM = string.IsNullOrEmpty(p.SalesPMName) ? null : new NisSalesPMDto
        {
            Name = p.SalesPMName ?? string.Empty,
            Nickname = p.SalesPMNickname,
            Phone = p.SalesPMPhone,
            Email = p.SalesPMEmail,
            Role = p.SalesPMRole,
        },
        Engineer = string.IsNullOrEmpty(p.EngineerName) ? null : new NisEngineerDto
        {
            Name = p.EngineerName ?? string.Empty,
            Nickname = p.EngineerNickname,
            Phone = p.EngineerPhone,
        },
        Tickets = p.Tickets.Select(MapTicket).ToList(),
        Attachments = p.Files
            .OrderBy(f => f.Seq)
            .Select(MapAttachment)
            .ToList(),
    };

    private static NisAttachmentDto MapAttachment(NisProjectFile f) => new()
    {
        Id = f.FileId,
        FileName = f.FileName,
        FilePath = f.FilePath,
        Seq = f.Seq,
        FileSize = f.FileSize,
    };

    // ── GET api/nis/projects ─────────────────────────────────────────────────

    /// <summary>Returns all NIS projects for a company. Matches frontend fetchNisProjectList.</summary>
    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<NisProjectResponseDto>>> GetProjects(
        [FromQuery] string? cmpid,
        [FromQuery] string? username)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var projects = await _context.NisProjects
            .AsNoTracking()
            .Include(p => p.Tickets).ThenInclude(t => t.Files)
            .Include(p => p.Files)
            .Where(p => p.CmpId == cmpid)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return Ok(projects.Select(MapProject));
    }

    // ── GET api/nis/projects/{id} ────────────────────────────────────────────

    /// <summary>Returns a single NIS project by ID. Matches frontend fetchNisProjectById.</summary>
    [HttpGet("projects/{id}")]
    public async Task<ActionResult<NisProjectResponseDto>> GetProject(
        string id,
        [FromQuery] string? cmpid)
    {
        var project = await _context.NisProjects
            .AsNoTracking()
            .Include(p => p.Tickets).ThenInclude(t => t.Files)
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
            return NotFound(new { message = $"Project {id} not found" });

        if (!string.IsNullOrWhiteSpace(cmpid) && project.CmpId != cmpid)
            return Forbid();

        return Ok(MapProject(project));
    }

    // ── POST api/nis/projects ────────────────────────────────────────────────

    /// <summary>Creates a new NIS project. Matches frontend createNisProject.</summary>
    [HttpPost("projects")]
    public async Task<ActionResult<NisProjectResponseDto>> CreateProject([FromBody] NisProjectCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "name is required" });

        var cmpId = dto.CmpId ?? string.Empty;

        // Transaction keeps the customer QR-flag update and the project/ticket insert atomic.
        using var transaction = await _context.Database.BeginTransactionAsync();

        // เลขโครงการรันจาก SEQUENCE dbo.projectTaskNew (ไม่ใช่ MAX+1 อีกแล้ว) — ลบโครงการล่าสุดแล้วสร้างใหม่
        // จะไม่ได้เลขเดิมซ้ำ · sequence ไม่รีเซ็ตรายปีและรวมทุก CmpId · ค่าที่ดึงแล้วไม่ถูก rollback (เลขข้ามได้)
        var bangkokNow = BangkokNow();
        var projectPrefix = $"NIS-{bangkokNow.ToString("yy", CultureInfo.InvariantCulture)}";
        var projectSeq = await _context.Database
            .SqlQueryRaw<long>("SELECT CAST(NEXT VALUE FOR dbo.projectTaskNew AS bigint) AS [Value]")
            .SingleAsync();
        var nextProjectNo = $"{projectPrefix}{projectSeq:D5}";

        // กันกรณี sequence ตั้ง START WITH ต่ำกว่าเลขที่มีอยู่แล้ว → ไม่ปล่อยให้เลขโครงการ/ตั๋วชนของเดิม
        if (await _context.NisProjects.AnyAsync(p => p.ProjectNo == nextProjectNo))
            return Conflict(new { message = $"ProjectNo {nextProjectNo} already exists — check SEQUENCE dbo.projectTaskNew start value" });

        var entity = new NisProject
        {
            ProjectId = Guid.NewGuid().ToString(),
            ProjectNo = nextProjectNo,
            Name = dto.Name,
            Customer = dto.Customer,
            CustomerCode = dto.CustomerCode,
            Type = dto.Type,
            Priority = dto.Priority,
            Progress = dto.Progress,
            Status = dto.Status,
            StartDate = ParseDate(dto.StartDate),
            EndDate = ParseDate(dto.EndDate),
            Staff = dto.Staff,
            SoRef = dto.SoRef,
            TagsRaw = JoinTags(dto.Tags),
            Location = dto.Location,
            ServiceConditionsJson = dto.ServiceConditions == null ? null : SerializeJson(dto.ServiceConditions),
            EquipmentIdsJson = dto.EquipmentIds.Count == 0 ? null : SerializeJson(dto.EquipmentIds),
            ContactName = dto.Contact?.Name,
            ContactPhone = dto.Contact?.Phone,
            ContactEmail = dto.Contact?.Email,
            SalesPMName = dto.SalesPM?.Name,
            SalesPMNickname = dto.SalesPM?.Nickname,
            SalesPMPhone = dto.SalesPM?.Phone,
            SalesPMEmail = dto.SalesPM?.Email,
            SalesPMRole = dto.SalesPM?.Role,
            EngineerName = dto.Engineer?.Name,
            EngineerNickname = dto.Engineer?.Nickname,
            EngineerPhone = dto.Engineer?.Phone,
            CmpId = dto.CmpId ?? string.Empty,
            CreatedBy = dto.CreatedBy ?? string.Empty,
            CreatedDate = bangkokNow,
            UpdatedDate = bangkokNow,
        };

        // Add initial tickets (e.g. auto-generated tickets from wizard step 3)
        // RunNo resets per ticket Type within this new project, starting at 1.
        var typeRunNo = new Dictionary<string, int>();
        foreach (var t in dto.Tickets)
        {
            var typeKey = t.Type ?? string.Empty;
            typeRunNo.TryGetValue(typeKey, out var prevRunNo);
            var runNo = prevRunNo + 1;
            typeRunNo[typeKey] = runNo;

            entity.Tickets.Add(new NisTicket
            {
                TicketId = Guid.NewGuid().ToString(),
                TicketCode = BuildTicketCode(t.Type, nextProjectNo, runNo),
                Title = t.Title,
                Status = t.Status,
                Assignee = t.Assignee,
                StartDate = ParseDate(t.StartDate),
                EndDate = ParseDate(t.EndDate),
                Due = ParseDate(t.Due),
                Pct = t.Pct,
                Type = t.Type,
                Priority = t.Priority,
                TagsRaw = JoinTags(t.Tags),
                ChecklistJson = t.Checklist.Count == 0 ? null : JsonSerializer.Serialize(t.Checklist),
                CmpId = cmpId,
                CreatedBy = dto.CreatedBy ?? string.Empty,
                CreatedDate = bangkokNow,
                UpdatedDate = bangkokNow,
                ClosedDate = t.Status is "Closed" or "Done" ? bangkokNow : null,
            });
        }

        var updatedCustomers = await _context.customers
            .Where(c => c.CmpId == cmpId && c.CustomerCode == dto.CustomerCode)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.StateGenQRCode, 1));

        if (updatedCustomers == 0)
            return BadRequest(new { message = $"Customer '{dto.Customer}' not found in master" });

        _context.NisProjects.Add(entity);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return CreatedAtAction(nameof(GetProject), new { id = entity.ProjectId }, MapProject(entity));
    }

    // ── PUT api/nis/projects/{id} ────────────────────────────────────────────

    /// <summary>
    /// Partial update of an existing project. Only non-null fields in the body are
    /// applied. Currently supports editing the project Location from the project
    /// list view. Matches frontend updateNisProjectLocation.
    /// </summary>
    [HttpPut("projects/{id}")]
    public async Task<ActionResult<NisProjectResponseDto>> UpdateProject(
        string id,
        [FromBody] NisProjectUpdateDto dto)
    {
        var project = await _context.NisProjects
            .Include(p => p.Tickets)
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
            return NotFound(new { message = $"Project {id} not found" });

        if (!string.IsNullOrWhiteSpace(dto.CmpId) && project.CmpId != dto.CmpId)
            return Forbid();

        // Only apply fields the client actually sent (partial update).
        if (dto.Location != null)
            project.Location = dto.Location;

        project.UpdatedBy = dto.UpdatedBy ?? project.UpdatedBy;
        project.UpdatedDate = BangkokNow();

        await _context.SaveChangesAsync();

        return Ok(MapProject(project));
    }

    // ── POST api/nis/projects/{id}/attachments ───────────────────────────────

    /// <summary>
    /// Saves attachment metadata for a project. The binary is already uploaded by the
    /// client via the shared /uploadallfile + /movefile endpoints; this only persists
    /// FileName/FilePath so the project list can show and link to the documents.
    /// Matches frontend attachNisProjectFiles (called right after createNisProject).
    /// </summary>
    [HttpPost("projects/{id}/attachments")]
    public async Task<ActionResult<IEnumerable<NisAttachmentDto>>> AttachFiles(
        string id,
        [FromBody] List<NisAttachmentDto> attachments)
    {
        var project = await _context.NisProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
            return NotFound(new { message = $"Project {id} not found" });

        if (attachments == null || attachments.Count == 0)
            return Ok(Array.Empty<NisAttachmentDto>());

        // Seq continues from whatever attachments the project already has.
        var existingCount = await _context.NisProjectFiles.CountAsync(f => f.ProjectId == id);
        var now = BangkokNow();

        var entities = attachments
            .Where(a => !string.IsNullOrWhiteSpace(a.FileName) && !string.IsNullOrWhiteSpace(a.FilePath))
            .Select((a, i) => new NisProjectFile
            {
                FileId = Guid.NewGuid().ToString(),
                ProjectId = id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                Seq = existingCount + i + 1,
                FileSize = a.FileSize,
                CmpId = project.CmpId,
                CreatedBy = project.CreatedBy,
                CreatedDate = now,
            })
            .ToList();

        if (entities.Count == 0)
            return Ok(Array.Empty<NisAttachmentDto>());

        _context.NisProjectFiles.AddRange(entities);
        await _context.SaveChangesAsync();

        return Ok(entities.Select(MapAttachment));
    }

    // ── DELETE api/nis/projects/{id}/attachments/{fileId} ────────────────────

    /// <summary>
    /// Removes an attachment's metadata from a project. Matches frontend
    /// deleteNisProjectFile. Returns 204 whether or not the row existed so the
    /// client can treat delete as idempotent.
    /// </summary>
    [HttpDelete("projects/{id}/attachments/{fileId}")]
    public async Task<IActionResult> DeleteAttachment(string id, string fileId)
    {
        var file = await _context.NisProjectFiles
            .FirstOrDefaultAsync(f => f.ProjectId == id && f.FileId == fileId);

        if (file != null)
        {
            _context.NisProjectFiles.Remove(file);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    // ── POST api/nis/projects/{id}/tickets ───────────────────────────────────

    /// <summary>Adds a ticket to a project. Matches frontend addNisTicket.</summary>
    [HttpPost("projects/{id}/tickets")]
    public async Task<ActionResult<NisTicketResponseDto>> AddTicket(
        string id,
        [FromBody] NisTicketCreateDto dto)
    {
        var project = await _context.NisProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
            return NotFound(new { message = $"Project {id} not found" });

        // ตั๋วที่เปิดจากเคส helpdesk เดิม (SourceProblemId) ในโครงการเดิม → คืนตั๋วเดิม กันกดบันทึก Action ซ้ำแล้วตัดโควตาสองครั้ง
        if (!string.IsNullOrWhiteSpace(dto.SourceProblemId))
        {
            var existing = await _context.NisTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProjectId == id && t.SourceProblemId == dto.SourceProblemId);
            if (existing != null) return Ok(MapTicket(existing));
        }

        // RunNo continues from however many tickets of this Type already exist in the project.
        var existingCount = await _context.NisTickets
            .CountAsync(t => t.ProjectId == id && t.Type == dto.Type);
        var runNo = existingCount + 1;

        // โควตา Onsite Incident case (business rule อยู่ที่นี่ — frontend แค่เตือน): หมดแล้วต้องยืนยัน AllowOverQuota
        var overQuota = false;
        if (dto.Type == OnsiteTicketType)
        {
            var quota = ComputeOnsiteQuota(
                ParseJson<NisServiceConditionsDto?>(project.ServiceConditionsJson, null),
                existingCount);
            if (quota != null && quota.Remaining <= 0)
            {
                if (!dto.AllowOverQuota)
                    return Conflict(new
                    {
                        code = "ONSITE_QUOTA_EXCEEDED",
                        message = $"โควตา Onsite Incident case ของโครงการหมดแล้ว (ใช้ไป {quota.Used}/{quota.Quota})",
                        quota = quota.Quota,
                        used = quota.Used,
                    });
                overQuota = true;
            }
        }

        var ticket = new NisTicket
        {
            TicketId = Guid.NewGuid().ToString(),
            TicketCode = BuildTicketCode(dto.Type, project.ProjectNo, runNo),
            ProjectId = id,
            Title = dto.Title,
            Status = dto.Status,
            Assignee = dto.Assignee,
            StartDate = ParseDate(dto.StartDate),
            EndDate = ParseDate(dto.EndDate),
            Due = string.IsNullOrEmpty(dto.Due) ? null : DateTime.Parse(dto.Due),
            Pct = dto.Pct,
            Type = dto.Type,
            Priority = dto.Priority,
            TagsRaw = JoinTags(dto.Tags),
            CmpId = dto.CmpId ?? string.Empty,
            // checklist ที่ frontend resolve ตาม ticket type + ลูกค้า (เดิมรับมาแล้วไม่ได้บันทึก)
            ChecklistJson = SerializeChecklist(dto.Checklist),
            CreatedBy = dto.CreatedBy ?? string.Empty,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
            SourceProblemId = string.IsNullOrWhiteSpace(dto.SourceProblemId) ? null : dto.SourceProblemId.Trim(),
            OverQuota = overQuota,
        };
        ApplyClosedDate(ticket);

        _context.NisTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(MapTicket(ticket));
    }

    // ── PUT api/nis/tickets/{id}/status ─────────────────────────────────────

    /// <summary>Updates ticket status (used by Kanban drag-and-drop). Matches frontend updateNisTicketStatus.</summary>
    [HttpPut("tickets/{id}/status")]
    public async Task<IActionResult> UpdateTicketStatus(
        string id,
        [FromBody] NisTicketStatusDto dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        // ตั๋วที่ยังรอ SM อนุมัติคำขอเปิดเคส — เปลี่ยนสถานะได้ทางอนุมัติ/ปฏิเสธคำขอเท่านั้น
        if (IsWaitingApproval(ticket))
            return WaitingApprovalConflict(ticket);

        ticket.Status = dto.Status;
        // ลาก Kanban ข้ามคอลัมน์ → ให้ % ตรงกับ milestone ของสถานะปลายทาง
        // (Open/Scheduled = ยังไม่รับงาน · Done/Closed = ปิดแล้ว)
        // "In Progress" ตั้งใจ "ไม่" bump Pct ขึ้นเป็น NIS_PCT_ACCEPTED ที่นี่ — การลาก Kanban
        // เฉย ๆ ไม่ได้แปลว่าช่างรับงานจริง (ไม่มี check-in/checklist) ปล่อยให้ Pct มาจาก action
        // จริงของช่างเท่านั้น: PUT tickets/{id}/accept (กดรับงาน) หรือ tickets/{id}/progress
        // (checklist ระหว่างทำงาน) — ดู PR ที่แก้บั๊ก "ลาก Kanban แล้วโชว์ 10% ทั้งที่ยังไม่รับงาน"
        ticket.Pct = dto.Status switch
        {
            "Open" or "Scheduled" => NIS_PCT_NOT_ACCEPTED,
            "Done" or "Closed" => NIS_PCT_CLOSED,
            "In Progress" => ticket.Pct,
            _ => ticket.Pct,
        };
        ApplyClosedDate(ticket);
        ResetCheckInIfNotStarted(ticket);
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Status updated" });
    }

    // ── PUT api/nis/tickets/{id}/progress ───────────────────────────────────

    /// <summary>
    /// อัปเดต % ความคืบหน้าของตั๋วจากแอปช่าง (NIS-OnsiteService) — แอปคำนวณ milestone เอง
    /// ด้วยสูตรเดียวกับ ComputeOnsiteProgressPct (utils/progress.ts) แล้วส่งค่ามาที่นี่
    ///
    /// แยกจาก POST onsite/{id}/progress (ที่ CRM ใช้) เพราะอันนั้นเขียนทั้ง snapshot draft —
    /// ถ้าแอปช่างยิง snapshot บาง ๆ ทับ จะทำให้ draft ของ CRM (รูป/ลายเซ็น) หาย
    /// </summary>
    [HttpPut("tickets/{id}/progress")]
    public async Task<IActionResult> UpdateTicketProgress(
        string id,
        [FromBody] NisTicketProgressDto dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        // ตั๋วที่พ้นมือช่างแล้วห้ามให้แอปเขียนทับ (กัน autosave ค้างลด % ของงานที่ปิดไปแล้ว)
        if (ticket.Status is "Waiting Close Approval" or "Done" or "Closed")
            return Ok(new { message = "Ticket already closed — progress unchanged", pct = ticket.Pct });

        // 100 ออกได้เฉพาะ flow ปิดงานจริงเท่านั้น — จากแอปตัดที่ 90
        // floor จากสถานะหน้างานบนตั๋ว (เช็คอิน 25 / เช็คเอาท์ 90) — autosave ค่าเก่าจากอีกเครื่องห้ามดึง % ถอย
        ticket.Pct = Math.Max(Math.Clamp(dto.Pct, NIS_PCT_NOT_ACCEPTED, NIS_PCT_CHECKED_OUT), TicketPctFloor(ticket));
        ticket.UpdatedBy = dto.UpdatedBy ?? ticket.UpdatedBy;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        await NotifyTicketChangedAsync(ticket, "pct", dto.UpdatedBy);
        return Ok(new { message = "Progress updated", pct = ticket.Pct });
    }

    // ── PUT api/nis/tickets/{id}/checkin ────────────────────────────────────

    /// <summary>
    /// ช่าง Check-in (RN หรือ CRM) — บันทึกเวลา/พิกัดลงตั๋วให้ทุก client เห็นสถานะเดียวกัน
    /// เดิม check-in อยู่แค่ใน draft ของเครื่อง (RN = AsyncStorage · CRM = progress snapshot ต่อ user)
    /// จึงเช็คอินจาก RN แล้วหน้า CRM ไม่เห็นจนกว่าจะปิดงาน
    ///
    /// - {id} = TicketId หรือ TicketCode
    /// - เวลาใช้เวลา server (เวลาไทย) — client แต่ละตัว format เวลาไม่เหมือนกัน
    /// - idempotent: เช็คอินแล้วไม่ทับเวลาเดิม (retry / กดซ้ำ / เครื่องที่สอง) แต่เติมพิกัดถ้ารอบแรกไม่มี
    /// - ต้องรับงานก่อน (Open/Scheduled → 409) · ตั๋วที่พ้นมือช่างแล้วไม่แตะ
    /// </summary>
    [HttpPut("tickets/{id}/checkin")]
    public async Task<IActionResult> CheckInTicket(string id, [FromBody] NisTicketCheckInDto? dto)
    {
        var ticket = await ResolveNisOnsiteTicketAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (!string.IsNullOrWhiteSpace(dto?.CmpId) && !string.IsNullOrWhiteSpace(ticket.CmpId) && dto!.CmpId != ticket.CmpId)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (IsWaitingApproval(ticket))
            return WaitingApprovalConflict(ticket);

        if (ticket.Status is "Open" or "Scheduled")
            return Conflict(new { message = "Ticket must be accepted before check-in" });

        if (ticket.Status is "Waiting Close Approval" or "Done" or "Closed")
            return Ok(new { message = "Ticket already closed — check-in unchanged", checkInTime = FormatDateTime(ticket.CheckInTime) });

        // พิกัดต้องอยู่ในช่วงจริง — นอกช่วง = ถือว่าไม่มีพิกัด (ไม่เก็บค่าขยะ)
        double? lat = dto?.Lat is >= -90 and <= 90 ? dto.Lat : null;
        double? lng = dto?.Lng is >= -180 and <= 180 ? dto.Lng : null;
        if (lat == null || lng == null) { lat = null; lng = null; }

        var location = dto?.Location?.Trim();
        if (location?.Length > 500) location = location[..500];
        var checkInBy = dto?.CheckInBy?.Trim();
        if (checkInBy?.Length > 200) checkInBy = checkInBy[..200];

        if (ticket.CheckInTime == null)
        {
            ticket.CheckInTime = BangkokNow();
            ticket.CheckInLat = lat;
            ticket.CheckInLng = lng;
            ticket.CheckInLocation = string.IsNullOrEmpty(location) ? null : location;
            ticket.CheckInBy = string.IsNullOrEmpty(checkInBy) ? null : checkInBy;
        }
        else if (ticket.CheckInLat == null && lat != null)
        {
            ticket.CheckInLat = lat;
            ticket.CheckInLng = lng;
            if (!string.IsNullOrEmpty(location)) ticket.CheckInLocation = location;
        }

        // milestone "เช็คอินแล้ว" — ไม่ลดค่าที่สูงกว่า
        ticket.Pct = Math.Max(ticket.Pct, NIS_PCT_CHECKED_IN);
        if (ticket.Status == "Pending") ticket.Status = "In Progress";
        ticket.UpdatedBy = string.IsNullOrEmpty(checkInBy) ? ticket.UpdatedBy : checkInBy;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        await NotifyTicketChangedAsync(ticket, "checkin", checkInBy);
        return Ok(new
        {
            message = "Checked in",
            checkInTime = FormatDateTime(ticket.CheckInTime),
            checkInLat = ticket.CheckInLat,
            checkInLng = ticket.CheckInLng,
            checkInLocation = ticket.CheckInLocation,
            pct = ticket.Pct,
        });
    }

    // ── PUT api/nis/tickets/{id}/checkout ───────────────────────────────────

    /// <summary>
    /// ช่าง Check-out (RN หรือ CRM) — บันทึกเวลา/พิกัดลงตั๋ว คู่กับ CheckInTicket
    /// เดิม check-out อยู่แค่ใน draft ของเครื่อง/ของ user → RN เช็คเอาท์ (Pct 90) แล้ว SM เปิดฟอร์มใน CRM
    /// autosave snapshot ที่ไม่มี checkOutTime ทับ Pct กลับเป็น 25 — เก็บบนตั๋วให้ทุก client เห็นค่าเดียวกัน
    ///
    /// - {id} = TicketId หรือ TicketCode
    /// - เวลาใช้เวลา server (เวลาไทย)
    /// - idempotent: เช็คเอาท์แล้วไม่ทับเวลาเดิม (retry / กดซ้ำ / เครื่องที่สอง) แต่เติมพิกัดถ้ารอบแรกไม่มี
    /// - ต้องรับงานก่อน (Open/Scheduled → 409) · ต้องเช็คอินก่อน (409) · ตั๋วที่พ้นมือช่างแล้วไม่แตะ
    /// </summary>
    [HttpPut("tickets/{id}/checkout")]
    public async Task<IActionResult> CheckOutTicket(string id, [FromBody] NisTicketCheckOutDto? dto)
    {
        var ticket = await ResolveNisOnsiteTicketAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (!string.IsNullOrWhiteSpace(dto?.CmpId) && !string.IsNullOrWhiteSpace(ticket.CmpId) && dto!.CmpId != ticket.CmpId)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (IsWaitingApproval(ticket))
            return WaitingApprovalConflict(ticket);

        if (ticket.Status is "Open" or "Scheduled")
            return Conflict(new { message = "Ticket must be accepted before check-out" });

        if (ticket.Status is "Waiting Close Approval" or "Done" or "Closed")
            return Ok(new { message = "Ticket already closed — check-out unchanged", checkOutTime = FormatDateTime(ticket.CheckOutTime) });

        // business gate ตรวจซ้ำที่ backend: ต้องเช็คอินก่อนเสมอ (ทุกประเภทงาน รวม noOnsite)
        if (ticket.CheckInTime == null)
            return Conflict(new { message = "Ticket must be checked in before check-out" });

        // พิกัดต้องอยู่ในช่วงจริง — นอกช่วง = ถือว่าไม่มีพิกัด (ไม่เก็บค่าขยะ)
        double? lat = dto?.Lat is >= -90 and <= 90 ? dto.Lat : null;
        double? lng = dto?.Lng is >= -180 and <= 180 ? dto.Lng : null;
        if (lat == null || lng == null) { lat = null; lng = null; }

        var location = dto?.Location?.Trim();
        if (location?.Length > 500) location = location[..500];
        var checkOutBy = dto?.CheckOutBy?.Trim();
        if (checkOutBy?.Length > 200) checkOutBy = checkOutBy[..200];

        if (ticket.CheckOutTime == null)
        {
            ticket.CheckOutTime = BangkokNow();
            ticket.CheckOutLat = lat;
            ticket.CheckOutLng = lng;
            ticket.CheckOutLocation = string.IsNullOrEmpty(location) ? null : location;
            ticket.CheckOutBy = string.IsNullOrEmpty(checkOutBy) ? null : checkOutBy;
        }
        else if (ticket.CheckOutLat == null && lat != null)
        {
            ticket.CheckOutLat = lat;
            ticket.CheckOutLng = lng;
            if (!string.IsNullOrEmpty(location)) ticket.CheckOutLocation = location;
        }

        // milestone "เช็คเอาท์แล้ว" — ไม่ลดค่าที่สูงกว่า (เพดาน 90 · 100 เฉพาะ flow ปิดงาน)
        ticket.Pct = Math.Max(ticket.Pct, NIS_PCT_CHECKED_OUT);
        ticket.UpdatedBy = string.IsNullOrEmpty(checkOutBy) ? ticket.UpdatedBy : checkOutBy;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        await NotifyTicketChangedAsync(ticket, "checkout", checkOutBy);
        return Ok(new
        {
            message = "Checked out",
            checkOutTime = FormatDateTime(ticket.CheckOutTime),
            checkOutLat = ticket.CheckOutLat,
            checkOutLng = ticket.CheckOutLng,
            checkOutLocation = ticket.CheckOutLocation,
            pct = ticket.Pct,
        });
    }

    // ── POST api/nis/tickets/{ticketId}/photos ──────────────────────────────

    /// กลุ่มรูปที่อนุญาต (โฟลเดอร์ย่อยใต้ photos/) — ค่าอื่น = 400
    private static readonly HashSet<string> TicketPhotoGroups = new(StringComparer.Ordinal)
    {
        "work", "rack", "damage", "other",
    };

    /// จำนวนไฟล์สูงสุดต่อ request
    private const int TicketPhotoMaxFiles = 10;

    /// ขนาดสูงสุดต่อไฟล์ (8 MB) — client ต้องย่อเหลือด้านยาว ≤ 1600px + JPEG ~0.8 ก่อนส่งอยู่แล้ว
    private const long TicketPhotoMaxBytes = 8L * 1024 * 1024;

    /// ตัด segment ของ path ให้เหลือเฉพาะอักขระปลอดภัย — กัน path traversal ("..", "/", "\")
    private static string SafePathSegment(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? "unknown"
            : System.Text.RegularExpressions.Regex.Replace(value.Trim(), "[^A-Za-z0-9_-]", "_");

    /// ตรวจ magic bytes ของไฟล์ — คืน ".jpg" / ".png" หรือ null เมื่อไม่ใช่ JPEG/PNG จริง
    private static string? DetectImageExtension(ReadOnlySpan<byte> head)
    {
        if (head.Length >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF) return ".jpg";
        if (head.Length >= 8
            && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47
            && head[4] == 0x0D && head[5] == 0x0A && head[6] == 0x1A && head[7] == 0x0A) return ".png";
        return null;
    }

    /// <summary>
    /// อัปโหลดรูปหน้างานของตั๋ว (แทน base64 / file:// ใน payload onsite) — multipart/form-data
    /// field "files" (1..10 ไฟล์, image/jpeg | image/png, ≤ 8 MB/ไฟล์ ตรวจ magic bytes ด้วย)
    /// + field "group" = work | rack | damage | other (default work)
    ///
    /// - cmpId / projectNo มาจากตั๋วใน DB เท่านั้น (ไม่เชื่อ client) · ชื่อไฟล์ = guid ใหม่ (ไม่ใช้ชื่อเดิม)
    /// - เก็บที่ wwwroot/{cmpId}/nis/{projectNo}/tickets/{ticketId}/photos/{group}/{guid}.jpg
    ///   (โครงเดียวกับ NisTicketFile) · url = "{serverUrl}/{cmpId}/nis/..." แบบเดียวกับ NisTicketFile.FilePath
    /// - static file ได้ CORS header จาก UseCors ที่อยู่ก่อน UseStaticFiles ใน Program.cs (CRM แปลง URL → dataURI ทำ PDF ได้)
    /// - {ticketId} = TicketId หรือ TicketCode
    /// - [Authorize] เฉพาะ endpoint นี้ (ต่างจากส่วนอื่นของ controller) — เขียนไฟล์ลง disk จึงต้องมี JWT
    ///   (RN photoSync + CRM uploadNisTicketPhotos ส่ง Bearer token อยู่แล้ว) · ไม่มี/หมดอายุ → 401
    /// </summary>
    [Authorize]
    [HttpPost("tickets/{ticketId}/photos")]
    [RequestSizeLimit(TicketPhotoMaxFiles * TicketPhotoMaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = TicketPhotoMaxFiles * TicketPhotoMaxBytes + 1024 * 1024)]
    public async Task<ActionResult<NisTicketPhotoUploadResponseDto>> UploadTicketPhotos(
        string ticketId,
        [FromForm] List<IFormFile>? files,
        [FromForm] string? group)
    {
        var ticket = await ResolveNisOnsiteTicketAsync(ticketId);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {ticketId} not found" });

        var photoGroup = string.IsNullOrWhiteSpace(group) ? "work" : group.Trim().ToLowerInvariant();
        if (!TicketPhotoGroups.Contains(photoGroup))
            return BadRequest(new { message = "group ต้องเป็น work | rack | damage | other" });

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "กรุณาแนบรูปอย่างน้อย 1 ไฟล์ (field: files)" });
        if (files.Count > TicketPhotoMaxFiles)
            return BadRequest(new { message = $"อัปโหลดได้สูงสุด {TicketPhotoMaxFiles} ไฟล์ต่อครั้ง" });

        // ตรวจทุกไฟล์ก่อนเขียนลง disk (ไม่เขียนครึ่ง ๆ กลาง ๆ เมื่อมีไฟล์เสีย)
        var accepted = new List<(IFormFile File, string Ext)>();
        foreach (var file in files)
        {
            var displayName = Path.GetFileName(file.FileName ?? string.Empty);
            if (file.Length <= 0)
                return BadRequest(new { message = $"ไฟล์ว่าง: {displayName}" });
            if (file.Length > TicketPhotoMaxBytes)
                return BadRequest(new { message = $"ไฟล์ใหญ่เกิน 8 MB: {displayName}" });

            var contentType = (file.ContentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();
            if (contentType != "image/jpeg" && contentType != "image/png")
                return BadRequest(new { message = $"รองรับเฉพาะรูป JPEG / PNG: {displayName}" });

            var head = new byte[8];
            int read;
            await using (var probe = file.OpenReadStream())
                read = await probe.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false);

            var ext = DetectImageExtension(head.AsSpan(0, read));
            if (ext == null)
                return BadRequest(new { message = $"ไฟล์ไม่ใช่รูป JPEG / PNG จริง: {displayName}" });

            accepted.Add((file, ext));
        }

        // projectNo จาก DB (convention เดียวกับ CRM: projectNo ?? projectId)
        var project = await _context.NisProjects
            .AsNoTracking()
            .Where(p => p.ProjectId == ticket.ProjectId)
            .Select(p => new { p.ProjectNo, p.ProjectId })
            .FirstOrDefaultAsync();

        var cmpSeg = SafePathSegment(ticket.CmpId);
        var projectSeg = SafePathSegment(string.IsNullOrWhiteSpace(project?.ProjectNo) ? (project?.ProjectId ?? ticket.ProjectId) : project!.ProjectNo);
        var ticketSeg = SafePathSegment(ticket.TicketId);
        var relDir = $"{cmpSeg}/nis/{projectSeg}/tickets/{ticketSeg}/photos/{photoGroup}";

        var webRoot = Path.GetFullPath(_webHostEnvironment.WebRootPath
            ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot"));
        var absDir = Path.GetFullPath(Path.Combine(webRoot, relDir));
        var rootPrefix = webRoot.EndsWith(Path.DirectorySeparatorChar) ? webRoot : webRoot + Path.DirectorySeparatorChar;
        if (!absDir.StartsWith(rootPrefix, StringComparison.Ordinal))
            return BadRequest(new { message = "invalid path" });

        Directory.CreateDirectory(absDir);

        var baseUrl = PublicBaseUrl();
        var result = new NisTicketPhotoUploadResponseDto();
        foreach (var (file, ext) in accepted)
        {
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(absDir, storedName);
            await using (var target = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write))
                await file.CopyToAsync(target);

            result.Files.Add(new NisTicketPhotoUploadItemDto
            {
                Url = $"{baseUrl}/{relDir}/{storedName}",
                FileName = TruncateLogValue(Path.GetFileName(file.FileName ?? string.Empty), 255),
                Size = file.Length,
            });
        }

        return Ok(result);
    }

    // ── รูปที่ยังไม่ได้อัปโหลด (local-only reference) ─────────────────────────

    /// prefix ของ reference ที่อยู่แค่ในเครื่อง client — server เปิดไม่ได้ ห้ามบันทึกลง report
    private static readonly string[] LocalPhotoPrefixes =
    {
        "file://", "content://", "ph://", "assets-library://", "blob:",
    };

    /// true เมื่อค่ารูปเป็น reference ในเครื่อง (file:// ฯลฯ) — ต้องอัปโหลดผ่าน POST tickets/{id}/photos ก่อน
    private static bool IsLocalPhotoRef(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && LocalPhotoPrefixes.Any(p => value.TrimStart().StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// รูปถือว่า "ไม่มี" เมื่อว่าง หรือเป็น reference ในเครื่องที่ server เปิดไม่ได้ (ใช้กับ gate PM)
    private static bool IsMissingPhoto(string? value) =>
        string.IsNullOrWhiteSpace(value) || IsLocalPhotoRef(value);

    /// <summary>
    /// หา field แรกใน payload ปิดงานที่ยังเป็นรูป local-only (file:// content:// ph:// assets-library:// blob:)
    /// คืนชื่อ field (เช่น "photos[2]", "pmItems[0].beforePhoto") หรือ null เมื่อผ่าน
    /// </summary>
    private static string? FindLocalPhotoField(NisOnsiteReportBaseDto dto)
    {
        for (var i = 0; i < (dto.Photos?.Count ?? 0); i++)
            if (IsLocalPhotoRef(dto.Photos![i])) return $"photos[{i}]";

        if (IsLocalPhotoRef(dto.BeforePhoto)) return "beforePhoto";
        if (IsLocalPhotoRef(dto.DuringPhoto)) return "duringPhoto";
        if (IsLocalPhotoRef(dto.AfterPhoto)) return "afterPhoto";
        if (IsLocalPhotoRef(dto.SignatureImg)) return "signatureImg";

        for (var i = 0; i < (dto.PmItems?.Count ?? 0); i++)
        {
            var item = dto.PmItems![i];
            if (IsLocalPhotoRef(item.BeforePhoto)) return $"pmItems[{i}].beforePhoto";
            if (IsLocalPhotoRef(item.AfterPhoto)) return $"pmItems[{i}].afterPhoto";
        }

        var damagePhotos = dto.DamagedProduct?.Photos;
        for (var i = 0; i < (damagePhotos?.Count ?? 0); i++)
            if (IsLocalPhotoRef(damagePhotos![i])) return $"damagedProduct.photos[{i}]";

        return null;
    }

    // ── PUT api/nis/tickets/{id}/accept ─────────────────────────────────────

    /// <summary>
    /// ช่างกดรับงานจากแอปหน้างาน — Scheduled → In Progress แล้วแจ้งเตือน Service Manager
    /// ทุกช่องทาง (Expo push บนแอป SM · socket.io ของ RN · กระดิ่ง CRM)
    ///
    /// แยก endpoint จาก PUT tickets/{id}/status เพราะ status ถูกใช้จากการลาก Kanban ด้วย —
    /// ไม่ควรทำให้การลากบอร์ดยิงแจ้งเตือน "ช่างรับงาน" ตามไปด้วย
    /// </summary>
    [HttpPut("tickets/{id}/accept")]
    public async Task<IActionResult> AcceptTicket(string id, [FromBody] NisTicketAcceptDto? dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (IsWaitingApproval(ticket))
            return WaitingApprovalConflict(ticket);

        var acceptedBy = string.IsNullOrWhiteSpace(dto?.AcceptedBy) ? ticket.Assignee : dto!.AcceptedBy!;

        // กดรับซ้ำ (retry ตอนเน็ตกระตุก / ปุ่มโดนกดสองครั้ง) → ไม่ถือเป็น error แต่ไม่ต้องแจ้งซ้ำ
        // (ตัวกัน dedupe จริงอยู่ที่ EventKey ของ push อีกชั้น)
        var alreadyAccepted = ticket.Status != "Scheduled";

        if (!alreadyAccepted)
        {
            ticket.Status = "In Progress";
            // milestone "รับงานแล้ว" — ไม่ลดค่าที่สูงกว่านี้อยู่แล้ว (กดรับซ้ำหลัง check-in)
            ticket.Pct = Math.Max(ticket.Pct, NIS_PCT_ACCEPTED);
            ticket.UpdatedBy = acceptedBy;
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        if (!alreadyAccepted)
        {
            await NotifyTicketAcceptedAsync(ticket, acceptedBy);
            // contract "ticket changed" — CRM (SignalR) + RN (ticket_update) refetch ตั๋ว
            await NotifyTicketChangedAsync(ticket, "accept", acceptedBy);
        }

        return Ok(new { message = "Ticket accepted", status = ticket.Status });
    }

    /// <summary>
    /// แจ้งว่าช่างกดรับงานแล้ว — best-effort ทั้งหมด (push ไม่สำเร็จห้ามทำให้การกดรับงานล้ม)
    ///
    /// ผู้รับ = SM ที่เป็นคนมอบหมายตั๋วใบนี้เท่านั้น (NisTicket.AssignedBy)
    /// ตั๋วเก่าก่อนมีคอลัมน์ AssignedBy → fallback ไปที่ SM จริงของบริษัท
    /// (SystemRole.StateManager = 1 — แหล่งเดียวกับ claim role ที่ CRM/RN ใช้)
    /// </summary>
    private async Task NotifyTicketAcceptedAsync(NisTicket ticket, string acceptedBy)
    {
        try
        {
            var managers = await ResolveAcceptNotifyTargetsAsync(ticket);

            if (managers.Count == 0) return;

            var title = "✅ ช่างรับงานแล้ว";
            var body = $"{acceptedBy} รับงาน {ticket.TicketCode} · {ticket.Title}";
            // dedupe ระดับนาที — กันยิงซ้ำจากกดรัวๆ/retry แต่ถ้าตั๋วถูก assign ใหม่แล้วรับอีกรอบยังแจ้งได้
            var stamp = DateTime.Now.ToString("yyyyMMddHHmm");

            // 1) Expo push — SM ที่ใช้แอปมือถือ
            foreach (var m in managers.Where(m => !string.IsNullOrWhiteSpace(m.FullName)))
            {
                await _push.SendToStaffAsync(
                    ticket.CmpId,
                    m.FullName,
                    eventKey: $"accept:{ticket.TicketId}:{m.FullName}:{stamp}",
                    title: title,
                    body: body,
                    ticketId: ticket.TicketId,
                    data: new Dictionary<string, string> { ["type"] = "accept" });
            }

            // 2) socket.io /nis — SM ที่เปิดแอปค้างอยู่ (refresh ทันที ไม่ต้องรอ poll)
            var managerUsernames = managers
                .Select(m => m.Username)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList();

            if (managerUsernames.Count > 0)
            {
                await _nisRealtimeNotify.NotifyAsync(
                    ticket.CmpId,
                    users: managerUsernames,
                    type: "accept",
                    ticketId: ticket.TicketId,
                    title: title,
                    body: body);
            }

            // 3) กระดิ่ง CRM — SM ที่นั่งหน้าเว็บบอร์ด
            var acceptedByUsername = await _context.Accounts
                .Where(a => a.CmpId == ticket.CmpId && a.FullName == acceptedBy)
                .Select(a => a.Username)
                .FirstOrDefaultAsync() ?? acceptedBy;

            foreach (var username in managerUsernames)
            {
                await _crmNotify.NotifyAsync(
                    ticket.CmpId,
                    toUsername: username,
                    fromUsername: acceptedByUsername,
                    title: body,
                    // ตรงกับ paths.gocrm.nis.ticketDetail ฝั่ง CRM (/productservice/service-protal/tickets/{id})
                    linkTo: $"/productservice/service-protal/tickets/{ticket.TicketId}",
                    moduleFormName: "nis/serviceboard",
                    docNo: ticket.TicketCode ?? ticket.TicketId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS accept: แจ้งเตือน SM ไม่สำเร็จ (ticket {TicketId})", ticket.TicketId);
        }
    }

    /// ผู้รับแจ้งเตือน "ช่างรับงาน" — Username ใช้กับ socket room/กระดิ่ง CRM · FullName ใช้กับ Expo push
    private sealed record NisNotifyTarget(string Username, string FullName);

    /// <summary>
    /// หาผู้รับแจ้งเตือนตอนช่างกดรับงาน: คนที่มอบหมายตั๋วใบนี้ (NisTicket.AssignedBy) เป็นหลัก
    /// ถ้าตั๋วไม่มีค่านั้น (ตั๋วเก่าก่อน migration / assign มาจาก client รุ่นที่ยังไม่ส่ง updatedBy)
    /// จึง fallback ไปที่ SM ทุกคนของบริษัท
    /// </summary>
    private async Task<List<NisNotifyTarget>> ResolveAcceptNotifyTargetsAsync(NisTicket ticket)
    {
        if (!string.IsNullOrWhiteSpace(ticket.AssignedBy))
        {
            var assigner = await _context.Accounts
                .Where(a => a.CmpId == ticket.CmpId && a.Username == ticket.AssignedBy)
                .Select(a => new NisNotifyTarget(a.Username, a.FullName ?? string.Empty))
                .FirstOrDefaultAsync();

            if (assigner != null) return [assigner];

            // AssignedBy ค้างชื่อ account ที่ถูกลบไปแล้ว → ตกไป fallback ด้านล่าง ดีกว่าเงียบ
            _logger.LogWarning(
                "NIS accept: ไม่พบ account ผู้มอบหมาย (ticket {TicketId}) — fallback ไป SM ทั้งบริษัท",
                ticket.TicketId);
        }

        return await GetNisManagersAsync(ticket.CmpId);
    }

    /// <summary>
    /// SM ของบริษัทตามนิยามเดียวกับ claim role ที่ CRM/RN ใช้ — SystemRole.StateManager = 1
    /// ผ่าน SystemPermission (มิเรอร์ AccountService.GetNisRoleAsync) ไม่ใช่ Accounts.Role.Name
    /// ซึ่งเป็น role เดิมของ CRM และกว้างเกินไป (ทุก account ที่ชื่อ role = admin)
    /// </summary>
    private async Task<List<NisNotifyTarget>> GetNisManagersAsync(string cmpId)
    {
        const string sql = @"
SELECT DISTINCT a.Username, ISNULL(a.FullName, '') AS FullName
FROM Accounts AS a
JOIN SystemPermission AS per ON per.AccountID = a.AccountID
JOIN SystemRole AS role      ON role.RoleId   = per.RoleId
WHERE a.CmpId = @CmpId
  AND ISNULL(role.StateManager, 0) = 1;";

        var result = new List<NisNotifyTarget>();
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@CmpId";
            parameter.Value = cmpId ?? string.Empty;
            command.Parameters.Add(parameter);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var username = reader.GetString(0);
                if (string.IsNullOrWhiteSpace(username)) continue;
                result.Add(new NisNotifyTarget(username, reader.GetString(1)));
            }
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }

        return result;
    }

    // ── Realtime "ticket changed" (contract: scratchpad nis-onsite-contract.md §2) ──────────

    /// ข้อความสั้นต่อ action สำหรับ body ของ nis:notify (RN แสดง/ใช้ refetch เท่านั้น)
    private static string TicketChangedLabel(string action) => action switch
    {
        "accept" => "ช่างรับงานแล้ว",
        "assign" => "มอบหมายงาน",
        "checkin" => "Check-in แล้ว",
        "checkout" => "Check-out แล้ว",
        "pct" => "อัปเดตความคืบหน้า",
        "progress" => "บันทึกฉบับร่างงาน",
        "request_close" => "ขออนุมัติปิดงาน",
        "closed" => "ปิดงานแล้ว",
        "close_approved" => "อนุมัติปิดงานแล้ว",
        "close_rejected" => "คำขอปิดงานถูกตีกลับ",
        _ => action,
    };

    /// <summary>
    /// แจ้งว่าตั๋วเปลี่ยน หลัง save สำเร็จ — ยิง 2 ช่องทาง best-effort (ห้าม throw ออกไป):
    /// 1) SignalR NisTicketChanged{cmpId} → CRM ที่เปิดตั๋วอยู่ refetch
    /// 2) socket.io /nis nis:notify type "ticket_update" (+action/status) → RN ของช่างผู้รับผิดชอบ
    ///    + SM ทุกคนของบริษัท (SystemRole.StateManager เหมือน AcceptTicket) + ผู้ทำ action เอง
    ///    (คนเดียวกันอาจเปิดทั้ง CRM และ iPad)
    /// </summary>
    /// <param name="ticket">ตั๋วหลัง save (Status = ค่าหลังเปลี่ยน)</param>
    /// <param name="action">ค่าตาม contract §2.3 เช่น checkin / close_rejected</param>
    /// <param name="actor">ผู้ทำ action — Username หรือ FullName (resolve เป็น Username ให้)</param>
    /// <param name="body">ข้อความเพิ่มเติม (เช่น เหตุผลตีกลับ) — ว่าง = ใช้ label ตาม action</param>
    /// <param name="extraAssignee">ช่างอีกคนที่ควรได้รับด้วย (เช่น ช่างคนเดิมตอนเปลี่ยนมือ) — FullName/Username</param>
    private async Task NotifyTicketChangedAsync(
        NisTicket ticket,
        string action,
        string? actor,
        string? body = null,
        string? extraAssignee = null)
    {
        try
        {
            var cmpId = ticket.CmpId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cmpId)) return;

            var actorUsername = await ResolveUsernameAsync(cmpId, actor);

            // 1) CRM web — SignalR
            await _crmNotify.BroadcastTicketChangedAsync(
                cmpId,
                ticket.TicketId,
                action,
                ticket.Status,
                actorUsername ?? actor,
                ticket.TicketCode);

            // 2) RN — socket.io /nis (room key = Accounts.Username)
            var names = new[] { ticket.Assignee, extraAssignee }
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "-")
                .Select(n => n!)
                .Distinct()
                .ToList();

            var users = new List<string>();
            if (names.Count > 0)
            {
                users.AddRange(await _context.Accounts
                    .Where(a => a.CmpId == cmpId && (names.Contains(a.FullName!) || names.Contains(a.Username)))
                    .Select(a => a.Username)
                    .ToListAsync());
            }
            users.AddRange((await GetNisManagersAsync(cmpId)).Select(m => m.Username));
            if (!string.IsNullOrWhiteSpace(actorUsername)) users.Add(actorUsername);

            var recipients = users
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (recipients.Count == 0) return;

            await _nisRealtimeNotify.NotifyAsync(
                cmpId,
                users: recipients,
                type: "ticket_update",
                ticketId: ticket.TicketId,
                title: $"{ticket.TicketCode ?? ticket.TicketId} · {ticket.Title}",
                body: string.IsNullOrWhiteSpace(body) ? TicketChangedLabel(action) : body,
                action: action,
                status: ticket.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS ticket changed notify ไม่สำเร็จ (ticket {TicketId}, action {Action})", ticket.TicketId, action);
        }
    }

    // ── PUT api/nis/tickets/{id}/close-approve ───────────────────────────────

    /// <summary>Manager approves a "Waiting Close Approval" ticket → Closed. Matches frontend approveNisCloseTicket.</summary>
    [HttpPut("tickets/{id}/close-approve")]
    public async Task<IActionResult> ApproveCloseTicket(string id, [FromBody] NisTicketCloseDto? dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        var cmpId = dto?.CmpId ?? ticket.CmpId ?? string.Empty;

        // การอนุมัติปิดงาน = จุดที่ต้อง "ออกเลข + persist Service Report" ให้จบ.
        // ตอนช่างขออนุมัติ (request-close) เราเก็บ report ไว้แล้วสถานะ pending_approval แต่ SrNumber ว่าง
        // (เลข SR ออกตอนอนุมัติ). เดิม endpoint นี้ตั้งแค่สถานะตั๋ว → report ไม่เคยได้เลข = หายตอน sync.
        // แก้: หา report ล่าสุดของตั๋วนี้ (รวม submitted เพื่อรองรับ retry แบบ idempotent)
        // แล้ว gen เลข SR ฝั่ง server + finalize เป็น submitted.
        // + reserved: เลข SR ที่จองไว้ตอน reserve-sr (request-close ไม่ถึง server) — ใช้เลขเดิม ไม่ออกเลขใหม่
        var report = await _context.NisOnsiteReports
            .Where(r => r.NisTicketId == ticket.TicketId
                && (r.Status == "pending_approval" || r.Status == "submitted" || r.Status == "reserved"))
            .OrderByDescending(r => r.Status == "pending_approval")
            .ThenByDescending(r => r.CreatedDate)
            .FirstOrDefaultAsync();

        // edge: ไม่พบ report รออนุมัติ (เช่น request-close ไม่เคยถึง server ตอน offline) →
        // สร้าง report ขั้นต่ำจากตั๋ว เพื่อให้ SR ยัง persist + โผล่ในลิสต์ (ข้อมูลหน้างานอาจไม่ครบ)
        if (report == null)
        {
            report = new NisOnsiteReport
            {
                ReportId = Guid.NewGuid().ToString(),
                NisTicketId = ticket.TicketId,
                TicketCode = ticket.TicketCode,
                CmpId = cmpId,
                Engineer = ticket.Assignee,
                Status = "pending_approval",
                CreatedDate = DateTime.Now,
            };
            _context.NisOnsiteReports.Add(report);
        }

        // gen เลขเฉพาะเมื่อยังไม่มี (idempotent ต่อการกดอนุมัติซ้ำ — ไม่ consume เลขใหม่)
        if (string.IsNullOrWhiteSpace(report.SrNumber))
            report.SrNumber = await NextSrNumberAsync(cmpId);
        // อนุมัติซ้ำ (retry) → ไม่ส่งอีเมลลูกค้าซ้ำ
        var alreadySubmitted = report.Status == "submitted" && ticket.Status == "Closed";
        report.Status = "submitted";

        ticket.Status = "Closed";
        ticket.Pct = NIS_PCT_CLOSED;
        ApplyClosedDate(ticket);
        ticket.UpdatedDate = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto?.ApprovedBy)) ticket.UpdatedBy = dto!.ApprovedBy;

        await _context.SaveChangesAsync();

        // ส่งอีเมลลูกค้าพร้อม PDF ที่ช่างส่งมาตอน request-close — code path เดียวกับ onsite/submit
        // best-effort: อีเมล/อ่าน PDF ล้มต้องไม่ทำให้การอนุมัติล้ม (ปิดตั๋วไปแล้วข้างบน)
        var emailSent = false;
        string? emailError = null;
        var pdfAttached = false;
        if (!alreadySubmitted)
        {
            try
            {
                var stored = ParseJson<NisOnsiteStoredCloseEmailDto?>(report.CloseEmailJson, null);
                if (stored != null && !string.IsNullOrWhiteSpace(stored.RecipientEmail))
                {
                    var emailDto = new NisOnsiteSubmitDto
                    {
                        SrNumber = report.SrNumber ?? string.Empty,
                        TicketId = ticket.TicketId,
                        CheckInTime = report.CheckInTime,
                        CheckOutTime = report.CheckOutTime,
                        WorkDetail = report.WorkDetail,
                        IssueDetail = report.IssueDetail,
                        SignatureImg = report.SignatureImageBase64,
                        SkipSignature = report.SkipSignature,
                        CmpId = cmpId,
                        User = report.Engineer,
                        RecipientEmail = stored.RecipientEmail!,
                        EmailSubject = stored.EmailSubject ?? string.Empty,
                        EmailMessage = stored.EmailMessage ?? string.Empty,
                        ContactName = stored.ContactName,
                        SenderName = stored.SenderName,
                        SenderPosition = stored.SenderPosition,
                        SenderMobile = stored.SenderMobile,
                        ReportPdfFileName = stored.ReportPdfFileName,
                    };

                    EmailAttachment? pdfAttachment = null;
                    var pdfBytes = await _pdfStorage.TryReadAsync(report.ReportPdfPath);
                    if (pdfBytes != null)
                        pdfAttachment = new EmailAttachment(ReportPdfFileName(emailDto), pdfBytes, "application/pdf");

                    (emailSent, emailError, pdfAttached) = await SendNisOnsiteCloseEmailAsync(ticket, cmpId, emailDto, pdfAttachment);
                }
            }
            catch (Exception ex)
            {
                emailError = ex.Message;
                _logger.LogWarning(ex, "NIS close-approve: ส่งอีเมลปิดงานไม่สำเร็จ (ticket {TicketId})", ticket.TicketId);
            }
        }

        await NotifyTicketChangedAsync(ticket, "close_approved", dto?.ApprovedBy);

        return Ok(new
        {
            message = "Ticket closed",
            srNumber = report.SrNumber,
            emailSent,
            emailError,
            pdfAttached,
        });
    }

    // ── PUT api/nis/tickets/{id}/close-reject ────────────────────────────────

    /// <summary>Manager rejects the close request → back to In Progress. Matches frontend rejectNisCloseTicket.</summary>
    [HttpPut("tickets/{id}/close-reject")]
    public async Task<IActionResult> RejectCloseTicket(string id, [FromBody] NisTicketCloseDto? dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        ticket.Status = "In Progress";
        ApplyClosedDate(ticket);
        ticket.UpdatedDate = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto?.RejectedBy)) ticket.UpdatedBy = dto!.RejectedBy;

        // report ที่รออนุมัติ → rejected (คงเลข SR ไว้ให้ request-close รอบถัดไปใช้ต่อ · ไม่โผล่ในลิสต์ submitted)
        var rejectReason = TrimOrNull(dto?.Reason, 500);
        var pendingReports = await _context.NisOnsiteReports
            .Where(r => r.NisTicketId == ticket.TicketId && r.Status == "pending_approval")
            .ToListAsync();
        foreach (var r in pendingReports)
        {
            r.Status = "rejected";
            r.RejectReason = rejectReason;
        }

        await _context.SaveChangesAsync();

        // NIS push event 2/3 — SM ตีกลับการปิดงาน (ช่างมักคิดว่าจบแล้ว ต้องรู้ทันที · best-effort)
        // dedupe ระดับนาที — ตีกลับซ้ำคนละรอบยังแจ้งได้
        var reason = dto?.Reason;
        await _push.SendToStaffAsync(
            ticket.CmpId,
            ticket.Assignee,
            eventKey: $"reject-close:{ticket.TicketId}:{DateTime.Now:yyyyMMddHHmm}",
            title: "↩️ งานถูกตีกลับ — ต้องแก้ไข",
            body: $"{ticket.TicketCode} · {ticket.Title}"
                  + (string.IsNullOrWhiteSpace(reason) ? "" : $" — เหตุผล: {reason}"),
            ticketId: ticket.TicketId,
            data: new Dictionary<string, string> { ["type"] = "reject-close" });

        // socket.io /nis — reject_close (type เดิม) ไปหาช่างพร้อมเหตุผล ให้แอปที่เปิดค้างเด้งทันที
        try
        {
            var assigneeUsername = await ResolveUsernameAsync(ticket.CmpId, ticket.Assignee);
            if (!string.IsNullOrWhiteSpace(assigneeUsername))
            {
                await _nisRealtimeNotify.NotifyAsync(
                    ticket.CmpId,
                    users: [assigneeUsername],
                    type: "reject_close",
                    ticketId: ticket.TicketId,
                    title: "↩️ งานถูกตีกลับ — ต้องแก้ไข",
                    body: string.IsNullOrWhiteSpace(reason)
                        ? $"{ticket.TicketCode} · {ticket.Title}"
                        : $"{ticket.TicketCode} · {ticket.Title} — เหตุผล: {reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS close-reject: socket notify ไม่สำเร็จ (ticket {TicketId})", ticket.TicketId);
        }

        await NotifyTicketChangedAsync(
            ticket,
            "close_rejected",
            dto?.RejectedBy,
            body: string.IsNullOrWhiteSpace(reason) ? null : $"คำขอปิดงานถูกตีกลับ — เหตุผล: {reason}");

        return Ok(new { message = "Close request rejected" });
    }

    // ── PUT api/nis/tickets/{id}/assign ─────────────────────────────────────

    /// <summary>Assigns a staff member to a ticket. Matches frontend assignNisTicket.</summary>
    [HttpPut("tickets/{id}/assign")]
    public async Task<IActionResult> AssignTicket(
        string id,
        [FromBody] NisTicketAssignDto dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        // ตั๋วรออนุมัติคำขอเปิดเคส — มอบหมายผ่าน POST pending-requests/{id}/approve เท่านั้น
        if (IsWaitingApproval(ticket))
            return WaitingApprovalConflict(ticket);

        var previousAssignee = ticket.Assignee;

        ticket.Assignee = dto.Assignee;
        ticket.StartDate = ParseDate(dto.StartDate);
        ticket.EndDate = ParseDate(dto.EndDate);
        ticket.Due = ticket.EndDate ?? ticket.Due;

        if (dto.Assignee == "-")
        {
            // Unassigned → back to the Open / Assigned backlog.
            ticket.Status = "Open";
            ticket.Pct = NIS_PCT_NOT_ACCEPTED;
            // การมอบหมายรอบเก่าจบแล้ว — ล้างผู้มอบหมายทิ้ง กันแจ้งเตือนไปหาคนที่ไม่ได้มอบหมายรอบใหม่
            ticket.AssignedBy = null;
        }
        else if (previousAssignee != dto.Assignee
                 || ticket.Status == "Open"
                 || ticket.Status == "Scheduled"
                 || string.IsNullOrWhiteSpace(ticket.Status))
        {
            // ยังไม่มีใครรับงาน (หรือเปลี่ยนมือช่าง) → ความคืบหน้าเริ่มนับใหม่ที่ 0
            ticket.Pct = NIS_PCT_NOT_ACCEPTED;
            // Freshly assigned (or handed to a different engineer) → awaits the
            // assignee's acceptance in the Staff Portal, which surfaces the
            // "accept task" notification. Accepting moves it to "In Progress"
            // via PUT tickets/{id}/status.
            ticket.Status = "Scheduled";
        }
        // else: same assignee already working (In Progress / Pending / …) — a
        // date-only edit must not reset their progress, so keep the status.

        // ผู้มอบหมาย = ผู้รับแจ้งเตือนตอนช่างกดรับงาน (ไม่ทับด้วย null จาก client รุ่นเก่าที่ยังไม่ส่งค่า)
        if (dto.Assignee != "-" && !string.IsNullOrWhiteSpace(dto.UpdatedBy))
            ticket.AssignedBy = dto.UpdatedBy;

        // ตั๋วที่ปิดแล้วถูก unassign / เปลี่ยนมือช่าง = เปิดกลับ → ล้างเวลาปิด (แก้วันอย่างเดียวคงค่าเดิม)
        ApplyClosedDate(ticket);
        // กลับไปรอรับงาน (unassign / เปลี่ยนช่าง) → check-in รอบเก่าใช้ไม่ได้แล้ว
        ResetCheckInIfNotStarted(ticket);

        ticket.UpdatedBy = dto.UpdatedBy ?? ticket.UpdatedBy;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        // NIS push event 1/3 — งานใหม่รอตอบรับ (เฉพาะ assign ใหม่/เปลี่ยนคน · best-effort ไม่กระทบ response)
        // dedupe วันละครั้งต่อ (ตั๋ว, ช่าง) กันยิงซ้ำจาก double-save
        if (ticket.Status == "Scheduled" && dto.Assignee != "-")
        {
            await _push.SendToStaffAsync(
                ticket.CmpId,
                dto.Assignee,
                eventKey: $"assign:{ticket.TicketId}:{dto.Assignee}:{DateTime.Today:yyyyMMdd}",
                title: "🔔 งานใหม่รอตอบรับ",
                body: $"{ticket.TicketCode} · {ticket.Title}",
                ticketId: ticket.TicketId,
                data: new Dictionary<string, string> { ["type"] = "assign" });

            // NIS realtime notify (go-chat-api socket.io bridge) — refresh instantly for any
            // RN client currently connected (foreground), on top of the Expo push above.
            // Socket room keys off Username (Accounts.Username), not the FullName stored on
            // the ticket, so resolve it the same way the Google Calendar sync below does.
            var assigneeUsername = await _context.Accounts
                .Where(a => a.CmpId == ticket.CmpId && a.FullName == dto.Assignee)
                .Select(a => a.Username)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(assigneeUsername))
            {
                await _nisRealtimeNotify.NotifyAsync(
                    ticket.CmpId,
                    users: [assigneeUsername],
                    type: "assign",
                    ticketId: ticket.TicketId,
                    title: "🔔 งานใหม่รอตอบรับ",
                    body: $"{ticket.TicketCode} · {ticket.Title}");
            }
        }

        // NIS Google Calendar sync — สร้าง/อัปเดต event ที่ผูกกับ ticket ตอนมอบหมาย
        // (upsert per-ticket ผ่าน mapping → assign/แก้วันซ้ำก็ patch event เดิม ไม่สร้างซ้ำ).
        // best-effort: Google พลาด (ยังไม่ต่อ OAuth ฯลฯ) ต้องไม่ทำให้การ assign ล้ม — log ไว้เฉยๆ
        if (dto.Assignee != "-" && ticket.StartDate.HasValue && ticket.EndDate.HasValue)
        {
            try
            {
                // Assignee is stored as Account.FullName; Accounts.Username holds the
                // account's email address, which is what gets tagged on the event.
                var assigneeEmail = await _context.Accounts
                    .Where(a => a.CmpId == ticket.CmpId && a.FullName == dto.Assignee)
                    .Select(a => a.Username)
                    .FirstOrDefaultAsync();

                var appt = await _googleOAuthCalendar.CreateOrUpdateEventAsync(new GoogleCalendarAppointmentCreateDto
                {
                    CmpId = ticket.CmpId,
                    SettingName = "nis",
                    TicketId = ticket.TicketId,
                    Title = $"{ticket.TicketCode} {ticket.Title}".Trim(),
                    Description = $"ผู้รับผิดชอบ: {dto.Assignee}",
                    Location = "",
                    Start = ticket.StartDate.Value,
                    End = ticket.EndDate.Value,
                    AllDay = true,
                    AttendeeEmails = assigneeEmail == null ? [] : [assigneeEmail],
                });
                _logger.LogInformation(
                    "NIS assign: Google Calendar synced ticket {TicketId} (cmp {CmpId}) → event {EventId} on calendar {CalendarId} [{Start:yyyy-MM-dd}..{End:yyyy-MM-dd}]",
                    ticket.TicketId, ticket.CmpId, appt.GoogleEventId, appt.CalendarId, appt.Start, appt.End);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NIS assign: Google Calendar sync failed for ticket {TicketId}", ticket.TicketId);
            }
        }

        // contract "ticket changed" — รวมช่างคนเดิมด้วย (ถูกเปลี่ยนมือ/unassign ต้องเห็นว่าตั๋วหลุดจากมือ)
        await NotifyTicketChangedAsync(ticket, "assign", dto.UpdatedBy, extraAssignee: previousAssignee);

        return Ok(new { message = "Assigned successfully" });
    }

    // ── PUT api/nis/tickets/{id}/task ────────────────────────────────────────

    /// <summary>
    /// อัปเดตรายละเอียดงาน + checklist ของ ticket (ก่อนมอบหมาย). ไม่แตะ assignee/สถานะ —
    /// เป็นการเตรียมงาน (scope + checklist) ให้ช่างก่อนกดมอบหมาย. Matches frontend updateNisTicketTask.
    /// </summary>
    [HttpPut("tickets/{id}/task")]
    public async Task<IActionResult> UpdateTicketTask(
        string id,
        [FromBody] NisTicketTaskUpdateDto dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);

        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        ticket.WorkDetail = string.IsNullOrWhiteSpace(dto.WorkDetail) ? null : dto.WorkDetail;

        // Normalize: ตัดข้อที่ text ว่าง, การันตี Id ทุกข้อ (กันข้อว่างค้างจากฟอร์ม)
        var items = (dto.Checklist ?? new())
            .Where(c => !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => new NisChecklistItemDto
            {
                Id = string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString() : c.Id,
                Text = c.Text.Trim(),
                Done = c.Done,
            })
            .ToList();

        ticket.ChecklistJson = items.Count == 0 ? null : JsonSerializer.Serialize(items);

        if (!string.IsNullOrWhiteSpace(dto.UpdatedBy))
            ticket.UpdatedBy = dto.UpdatedBy;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Task updated", checklist = items, workDetail = ticket.WorkDetail });
    }

    // ── Pending Requests (Staff "open ticket" request → manager approve/reject) ──

    private static NisPendingRequestResponseDto MapPendingRequest(NisPendingRequest r) => new()
    {
        Id = r.RequestId,
        RequestedBy = r.RequestedBy,
        Title = r.Title,
        TicketType = r.TicketType,
        Due = FormatDate(r.Due),
        ProjectId = r.ProjectId,
        Location = r.Location,
        Detail = r.Detail,
        NoOnsite = r.NoOnsite,
        SkipSignature = r.SkipSignature,
        RequireCloseApproval = r.RequireCloseApproval,
        RequestTime = FormatDateTime(r.CreatedDate),
        SupportMethod = r.SupportMethod,
        ParentTicketId = r.ParentTicketId,
        Status = r.Status,
        SourceProblemId = r.SourceProblemId,
        CustomerCode = r.CustomerCode,
        CustomerName = r.CustomerName,
        CustomerLineUserId = r.CustomerLineUserId,
        CustomerOaLineId = r.CustomerOaLineId,
        AllowOverQuota = r.AllowOverQuota,
        Priority = r.Priority,
        CreatedTicketId = r.CreatedTicketId,
        ApprovedBy = r.ApprovedBy,
        RejectedBy = r.RejectedBy,
        RejectReason = r.RejectReason,
        // ไฟล์แนบของคำขอเอง (Staff Portal) — BuildPendingRequestDtosAsync จะทับด้วยไฟล์ของตั๋วเมื่อมี CreatedTicketId
        Attachments = ParseJson<List<NisAttachmentDto>>(r.AttachmentsJson, new()),
    };

    /// <summary>
    /// ไฟล์แนบที่ส่งมากับคำขอ → JSON เก็บใน NisPendingRequest.AttachmentsJson (ตัด rows ที่ไม่มีชื่อ/path · Seq เรียงใหม่ 1..n)
    /// </summary>
    private static string? SerializeAttachments(IEnumerable<NisAttachmentDto>? items)
    {
        var list = (items ?? Array.Empty<NisAttachmentDto>())
            .Where(a => !string.IsNullOrWhiteSpace(a.FileName) && !string.IsNullOrWhiteSpace(a.FilePath))
            .Select((a, i) => new NisAttachmentDto
            {
                FileName = a.FileName.Length > 300 ? a.FileName[..300] : a.FileName,
                FilePath = a.FilePath.Length > 1000 ? a.FilePath[..1000] : a.FilePath,
                Seq = i + 1,
                FileSize = a.FileSize,
            })
            .ToList();
        return list.Count == 0 ? null : JsonSerializer.Serialize(list);
    }

    /// <summary>
    /// map คำขอพร้อมข้อมูลตั๋วที่ผูก (TicketCode, ไฟล์แนบ, overQuota) และโควตาของโครงการ —
    /// คำขอจาก helpdesk มีตั๋ว 'Waiting Approval' ตั้งแต่สร้าง แท็บอนุมัติ/Staff Portal ต้องเห็นเลขตั๋ว+ไฟล์
    /// </summary>
    private async Task<List<NisPendingRequestResponseDto>> BuildPendingRequestDtosAsync(List<NisPendingRequest> requests)
    {
        var ticketIds = requests
            .Select(r => r.CreatedTicketId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();
        var ticketsById = ticketIds.Count == 0
            ? new Dictionary<string, NisTicket>()
            : await _context.NisTickets
                .AsNoTracking()
                .Include(t => t.Files)
                .Where(t => ticketIds.Contains(t.TicketId))
                .ToDictionaryAsync(t => t.TicketId);

        // โควตาเฉพาะคำขอที่มาจาก helpdesk (มี SourceProblemId) — โหลดโครงการ + ตั๋วทีเดียว
        var projectIds = requests
            .Where(r => !string.IsNullOrWhiteSpace(r.SourceProblemId) && !string.IsNullOrWhiteSpace(r.ProjectId))
            .Select(r => r.ProjectId)
            .Distinct()
            .ToList();
        var projectsById = projectIds.Count == 0
            ? new Dictionary<string, NisProject>()
            : await _context.NisProjects
                .AsNoTracking()
                .Include(p => p.Tickets)
                .Where(p => projectIds.Contains(p.ProjectId))
                .ToDictionaryAsync(p => p.ProjectId);

        // ชื่อเต็มผู้ขอ — RequestedBy เก็บ Username (email) หรือ FullName แล้วแต่ client
        var cmpIdForNames = requests.Select(r => r.CmpId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        var accountInfos = string.IsNullOrWhiteSpace(cmpIdForNames)
            ? new Dictionary<string, AccountInfoLite>()
            : await ResolveAccountInfosAsync(cmpIdForNames, requests.Select(r => r.RequestedBy));

        return requests.Select(r =>
        {
            var dto = MapPendingRequest(r);
            if (!string.IsNullOrWhiteSpace(r.RequestedBy) && accountInfos.TryGetValue(r.RequestedBy.Trim(), out var requester))
            {
                dto.RequestedByName = requester.FullName;
                dto.RequestedByImgPath = requester.ImgPath;
            }
            if (!string.IsNullOrWhiteSpace(r.CreatedTicketId) && ticketsById.TryGetValue(r.CreatedTicketId, out var ticket))
            {
                dto.CreatedTicketCode = ticket.TicketCode;
                dto.OverQuota = ticket.OverQuota;
                dto.Attachments = ticket.Files.OrderBy(f => f.Seq).Select(MapTicketFile).ToList();
            }
            if (projectsById.TryGetValue(r.ProjectId, out var project))
                dto.OnsiteQuota = ComputeOnsiteQuota(project, project.Tickets);
            return dto;
        }).ToList();
    }

    // ── GET api/nis/pending-requests ─────────────────────────────────────────

    /// <summary>
    /// Returns pending requests. Manager view (no requestedBy) → only Status="Pending".
    /// Staff view (requestedBy set) → that engineer's requests in every status (history).
    /// Matches frontend fetchNisPendingRequests / fetchMyNisPendingRequests.
    /// </summary>
    /// <param name="sourceProblemId">เคส helpdesk — คืนคำขอทุกสถานะของเคสนั้น (เช็คซ้ำ/แสดงป้าย)</param>
    /// <param name="hasSource">true = เฉพาะคำขอที่เปิดจาก helpdesk ทุกสถานะ (หน้ารายการแจ้งปัญหาใช้ทำป้ายต่อแถว)</param>
    [HttpGet("pending-requests")]
    public async Task<ActionResult<IEnumerable<NisPendingRequestResponseDto>>> GetPendingRequests(
        [FromQuery] string? cmpid,
        [FromQuery] string? requestedBy,
        [FromQuery] string? sourceProblemId,
        [FromQuery] bool hasSource = false)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var query = _context.NisPendingRequests
            .AsNoTracking()
            .Where(r => r.CmpId == cmpid);

        if (!string.IsNullOrWhiteSpace(sourceProblemId))
            query = query.Where(r => r.SourceProblemId == sourceProblemId);
        else if (hasSource)
            query = query.Where(r => r.SourceProblemId != null && r.SourceProblemId != "");
        else if (!string.IsNullOrWhiteSpace(requestedBy))
            query = query.Where(r => r.RequestedBy == requestedBy);
        else
            query = query.Where(r => r.Status == "Pending");

        var list = await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return Ok(await BuildPendingRequestDtosAsync(list));
    }

    // ── POST api/nis/pending-requests ────────────────────────────────────────

    /// <summary>
    /// Staff raises an "open ticket" request. Matches frontend createNisPendingRequest.
    ///
    /// โหมด helpdesk (ส่ง <c>sourceProblemId</c>): ปุ่ม "เปิดเคส Onsite" ใน /productservice/servicerequest —
    /// สร้าง NisTicket 'MA Onsite' สถานะ 'Waiting Approval' ทันทีใต้โครงการที่เลือก (ตัดโควตาทันที)
    /// พร้อมไฟล์แนบ + LINE ลูกค้า แล้วแจ้ง SM ทุกคน · 409 เมื่อเคสนั้นมีคำขอ Pending อยู่แล้ว
    /// (REQUEST_ALREADY_PENDING) หรือโควตาหมดและไม่ส่ง allowOverQuota (ONSITE_QUOTA_EXCEEDED)
    /// </summary>
    [HttpPost("pending-requests")]
    public async Task<ActionResult<NisPendingRequestResponseDto>> CreatePendingRequest(
        [FromBody] NisPendingRequestCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "title is required" });

        var sourceProblemId = dto.SourceProblemId?.Trim();
        var fromHelpdesk = !string.IsNullOrWhiteSpace(sourceProblemId);

        var request = new NisPendingRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RequestedBy = dto.RequestedBy ?? string.Empty,
            Title = dto.Title.Trim(),
            TicketType = fromHelpdesk ? OnsiteTicketType : dto.TicketType,
            SupportMethod = fromHelpdesk && string.IsNullOrWhiteSpace(dto.SupportMethod) ? "Onsite" : dto.SupportMethod,
            ProjectId = dto.ProjectId ?? string.Empty,
            Location = dto.Location,
            Detail = dto.Detail,
            Due = ParseDate(dto.Due),
            Status = "Pending",
            CmpId = dto.CmpId,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };

        if (!fromHelpdesk)
        {
            // ไฟล์แนบ/ภาพจาก Staff Portal — ยังไม่มีตั๋ว จึงเก็บเป็น JSON บนคำขอ แล้วคัดลอกเป็น NisTicketFile ตอน SM อนุมัติ
            request.AttachmentsJson = SerializeAttachments(dto.Attachments);
            _context.NisPendingRequests.Add(request);
            await _context.SaveChangesAsync();
            return Ok(MapPendingRequest(request));
        }

        // ── โหมด helpdesk: validate + สร้างตั๋ว Waiting Approval คู่กับคำขอ ──
        if (string.IsNullOrWhiteSpace(dto.ProjectId))
            return BadRequest(new { message = "projectId is required when sourceProblemId is set" });

        var project = await _context.NisProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == dto.ProjectId && p.CmpId == dto.CmpId);
        if (project == null)
            return NotFound(new { message = $"Project {dto.ProjectId} not found" });

        var duplicate = await _context.NisPendingRequests
            .AsNoTracking()
            .Where(r => r.CmpId == dto.CmpId && r.SourceProblemId == sourceProblemId && r.Status == "Pending")
            .Select(r => new { r.RequestId, r.CreatedTicketId })
            .FirstOrDefaultAsync();
        if (duplicate != null)
            return Conflict(new
            {
                code = "REQUEST_ALREADY_PENDING",
                message = $"เคส {sourceProblemId} มีคำขอเปิดเคส Onsite ที่รออนุมัติอยู่แล้ว",
                requestId = duplicate.RequestId,
                ticketId = duplicate.CreatedTicketId,
            });

        // โควตา Onsite Incident case — business rule เดียวกับ AddTicket (ตั๋ว Waiting Approval นับเป็น used ทันที)
        var existingCount = await _context.NisTickets
            .CountAsync(t => t.ProjectId == project.ProjectId && t.Type == OnsiteTicketType);
        var overQuota = false;
        var quota = ComputeOnsiteQuota(
            ParseJson<NisServiceConditionsDto?>(project.ServiceConditionsJson, null),
            existingCount);
        if (quota != null && quota.Remaining <= 0)
        {
            if (!dto.AllowOverQuota)
                return Conflict(new
                {
                    code = "ONSITE_QUOTA_EXCEEDED",
                    message = $"โควตา Onsite Incident case ของโครงการหมดแล้ว (ใช้ไป {quota.Used}/{quota.Quota})",
                    quota = quota.Quota,
                    used = quota.Used,
                });
            overQuota = true;
        }

        var priority = string.IsNullOrWhiteSpace(dto.Priority) ? project.Priority : dto.Priority!.Trim();
        var now = DateTime.Now;
        var ticket = new NisTicket
        {
            TicketId = Guid.NewGuid().ToString(),
            TicketCode = BuildTicketCode(OnsiteTicketType, project.ProjectNo, existingCount + 1),
            ProjectId = project.ProjectId,
            Title = request.Title,
            Status = WaitingApprovalStatus,
            Assignee = "-",
            Due = request.Due,
            Pct = 0,
            Type = OnsiteTicketType,
            Priority = priority,
            TagsRaw = JoinTags(new List<string> { OnsiteTicketType, "Incident case", "Helpdesk" }),
            WorkDetail = string.IsNullOrWhiteSpace(dto.Detail) ? null : dto.Detail,
            ChecklistJson = SerializeChecklist(dto.Checklist),
            CmpId = dto.CmpId,
            CreatedBy = request.RequestedBy,
            CreatedDate = now,
            UpdatedDate = now,
            SourceProblemId = sourceProblemId,
            OverQuota = overQuota,
            CustomerLineUserId = TrimOrNull(dto.CustomerLineUserId, 100),
            CustomerOaLineId = TrimOrNull(dto.CustomerOaLineId, 100),
        };

        var files = (dto.Attachments ?? new())
            .Where(a => !string.IsNullOrWhiteSpace(a.FileName) && !string.IsNullOrWhiteSpace(a.FilePath))
            .Select((a, i) => new NisTicketFile
            {
                FileId = Guid.NewGuid().ToString(),
                TicketId = ticket.TicketId,
                ProjectId = project.ProjectId,
                FileName = a.FileName.Length > 300 ? a.FileName[..300] : a.FileName,
                FilePath = a.FilePath.Length > 1000 ? a.FilePath[..1000] : a.FilePath,
                Seq = i + 1,
                FileSize = a.FileSize,
                CmpId = dto.CmpId,
                CreatedBy = request.RequestedBy,
                CreatedDate = now,
            })
            .ToList();

        request.SourceProblemId = sourceProblemId;
        request.CustomerCode = TrimOrNull(dto.CustomerCode ?? project.CustomerCode, 50);
        request.CustomerName = TrimOrNull(dto.CustomerName ?? project.Customer, 300);
        request.CustomerLineUserId = ticket.CustomerLineUserId;
        request.CustomerOaLineId = ticket.CustomerOaLineId;
        request.AllowOverQuota = dto.AllowOverQuota;
        request.Priority = priority;
        request.Location = string.IsNullOrWhiteSpace(dto.Location) ? project.Location : dto.Location;
        request.CreatedTicketId = ticket.TicketId;

        _context.NisTickets.Add(ticket);
        if (files.Count > 0) _context.NisTicketFiles.AddRange(files);
        _context.NisPendingRequests.Add(request);
        await _context.SaveChangesAsync();

        await NotifyPendingRequestCreatedAsync(request, ticket, project);

        ticket.Files = files;
        var result = MapPendingRequest(request);
        result.CreatedTicketCode = ticket.TicketCode;
        result.OverQuota = ticket.OverQuota;
        result.Attachments = files.Select(MapTicketFile).ToList();
        result.OnsiteQuota = quota == null
            ? null
            : ComputeOnsiteQuota(
                ParseJson<NisServiceConditionsDto?>(project.ServiceConditionsJson, null),
                existingCount + 1,
                // รออนุมัติ = ตั๋ว Waiting Approval เดิมในโครงการ + ใบนี้
                await _context.NisTickets.CountAsync(t =>
                    t.ProjectId == project.ProjectId && t.Type == OnsiteTicketType && t.Status == WaitingApprovalStatus));
        return Ok(result);
    }

    /// ตัดช่องว่าง + จำกัดความยาว · ว่าง = null
    private static string? TrimOrNull(string? value, int max)
    {
        var v = value?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        return v.Length > max ? v[..max] : v;
    }

    // ── POST api/nis/pending-requests/{id}/approve ───────────────────────────

    /// <summary>
    /// Manager approves a request → creates a NisTicket (assigned to dto.Assignee)
    /// in the request's project, then marks the request Approved.
    /// Matches frontend approveNisPendingRequest.
    /// </summary>
    [HttpPost("pending-requests/{id}/approve")]
    public async Task<ActionResult<NisTicketResponseDto>> ApprovePendingRequest(
        string id,
        [FromBody] NisApprovePendingDto dto)
    {
        var request = await _context.NisPendingRequests.FindAsync(id);

        if (request == null)
            return NotFound(new { message = $"Pending request {id} not found" });
        if (request.Status != "Pending")
            return BadRequest(new { message = $"Request already {request.Status}" });

        var cmpId = dto.CmpId ?? request.CmpId;

        // ── SM แก้รายละเอียดคำขอตอนอนุมัติ (หัวข้อ/รายละเอียด/วันครบกำหนด) — ไม่ส่งมา = คงเดิม ──
        if (!string.IsNullOrWhiteSpace(dto.Title)) request.Title = dto.Title.Trim();
        if (dto.Detail != null) request.Detail = string.IsNullOrWhiteSpace(dto.Detail) ? null : dto.Detail.Trim();
        var dueOverride = ParseDate(dto.Due);
        if (dueOverride != null) request.Due = dueOverride;

        // ── คำขอจาก helpdesk ("เปิดเคส Onsite"): ตั๋ว 'Waiting Approval' ถูกสร้างไว้แล้วตอนขอ ──
        // อนุมัติ = เปลี่ยนตั๋วเดิมเป็น Scheduled (มอบหมายแล้ว) / Open (ยังไม่มอบหมาย) ไม่สร้างใบใหม่
        // (เลขตั๋ว/โควตา/ไฟล์แนบ/LINE ลูกค้า คงเดิม) — logic ด้านล่างสำหรับคำขอแบบเดิมไม่เปลี่ยน
        if (!string.IsNullOrWhiteSpace(request.CreatedTicketId))
        {
            var preCreated = await _context.NisTickets
                .Include(t => t.Files)
                .FirstOrDefaultAsync(t => t.TicketId == request.CreatedTicketId);

            if (preCreated != null && IsWaitingApproval(preCreated))
            {
                var assignedNow = !string.IsNullOrWhiteSpace(dto.Assignee) && dto.Assignee != "-";
                var approvedAt = DateTime.Now;

                preCreated.Title = request.Title;
                if (dto.Detail != null) preCreated.WorkDetail = request.Detail;
                if (dueOverride != null) preCreated.Due = dueOverride;
                preCreated.Status = assignedNow ? "Scheduled" : "Open";
                preCreated.Assignee = assignedNow ? dto.Assignee : "-";
                preCreated.AssignedBy = assignedNow ? dto.ApprovedBy : null;
                preCreated.StartDate = ParseDate(dto.StartDate);
                preCreated.EndDate = ParseDate(dto.EndDate);
                preCreated.Due = preCreated.EndDate ?? preCreated.Due;
                AppendApproveAttachments(preCreated, dto, approvedAt);
                preCreated.Pct = NIS_PCT_NOT_ACCEPTED;
                preCreated.UpdatedBy = dto.ApprovedBy ?? preCreated.UpdatedBy;
                preCreated.UpdatedDate = approvedAt;
                ApplyClosedDate(preCreated);

                request.Status = "Approved";
                request.ApprovedBy = dto.ApprovedBy;
                request.NoOnsite = dto.NoOnsite;
                request.SkipSignature = dto.SkipSignature;
                if (!string.IsNullOrWhiteSpace(dto.Location)) request.Location = dto.Location;
                if (!string.IsNullOrWhiteSpace(dto.SupportMethod)) request.SupportMethod = dto.SupportMethod;
                request.UpdatedDate = approvedAt;

                await _context.SaveChangesAsync();

                // แจ้งช่างที่ได้รับมอบหมาย (push/socket 'assign' เหมือน AssignTicket) + แจ้งผู้ขอว่าอนุมัติแล้ว
                if (assignedNow) await NotifyAssignedOnApproveAsync(preCreated);
                await NotifyRequestDecisionAsync(request, preCreated, approved: true);

                return Ok(MapTicket(preCreated));
            }
        }

        var project = await _context.NisProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

        // คำขอที่ยิงมาจากแอปหน้างาน (RN) พก projectId แบบ mock/offline (เช่น "PRJ-GENERAL")
        // ที่ไม่มีอยู่ใน DB จริง — แทนที่จะบล็อกการอนุมัติ ให้ fallback ไปผูกกับโปรเจกต์
        // "งานทั่วไป" ต่อบริษัท (get-or-create) เพื่อให้ ticket ที่สร้างยังโผล่บนบอร์ด
        project ??= await GetOrCreateGeneralProjectAsync(cmpId, dto.ApprovedBy);

        // RunNo continues from however many tickets of this Type already exist in the project.
        var ticketType = request.TicketType;
        var existingCount = await _context.NisTickets
            .CountAsync(t => t.ProjectId == project.ProjectId && t.Type == ticketType);
        var runNo = existingCount + 1;

        var isAssigned = !string.IsNullOrWhiteSpace(dto.Assignee) && dto.Assignee != "-";

        var ticket = new NisTicket
        {
            TicketId = Guid.NewGuid().ToString(),
            TicketCode = BuildTicketCode(ticketType, project.ProjectNo, runNo),
            ProjectId = project.ProjectId,
            Title = request.Title,
            // Assigned → "Scheduled" (awaits engineer acceptance in the Staff Portal);
            // unassigned → "Open". Mirrors AssignTicket / the accept-notification flow.
            Status = isAssigned ? "Scheduled" : "Open",
            Assignee = isAssigned ? dto.Assignee : "-",
            StartDate = ParseDate(dto.StartDate),
            EndDate = ParseDate(dto.EndDate),
            Due = request.Due,
            Pct = 0,
            Type = ticketType,
            WorkDetail = request.Detail,
            CmpId = cmpId,
            CreatedBy = dto.ApprovedBy ?? string.Empty,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };
        // ไฟล์ที่ staff แนบตอนขอ (AttachmentsJson) → NisTicketFile ของตั๋วใหม่ก่อน แล้วค่อยต่อท้ายไฟล์ที่ SM แนบเพิ่ม
        ticket.Files = ParseJson<List<NisAttachmentDto>>(request.AttachmentsJson, new())
            .Where(a => !string.IsNullOrWhiteSpace(a.FileName) && !string.IsNullOrWhiteSpace(a.FilePath))
            .Select((a, i) => new NisTicketFile
            {
                FileId = Guid.NewGuid().ToString(),
                TicketId = ticket.TicketId,
                ProjectId = ticket.ProjectId,
                FileName = a.FileName.Length > 300 ? a.FileName[..300] : a.FileName,
                FilePath = a.FilePath.Length > 1000 ? a.FilePath[..1000] : a.FilePath,
                Seq = i + 1,
                FileSize = a.FileSize,
                CmpId = cmpId,
                CreatedBy = request.RequestedBy,
                CreatedDate = ticket.CreatedDate,
            })
            .ToList();
        AppendApproveAttachments(ticket, dto, ticket.CreatedDate);

        request.Status = "Approved";
        request.ApprovedBy = dto.ApprovedBy;
        request.CreatedTicketId = ticket.TicketId;
        request.UpdatedDate = DateTime.Now;

        _context.NisTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(MapTicket(ticket));
    }

    /// <summary>
    /// ไฟล์แนบที่ SM เพิ่มตอนอนุมัติคำขอ — ต่อท้าย Seq จากไฟล์เดิมของตั๋ว (ticket.Files ต้อง Include มาก่อน
    /// สำหรับตั๋วที่มีอยู่แล้ว; ตั๋วใหม่ยังไม่มีไฟล์) · เพิ่มเข้า context ให้ caller SaveChanges ทีเดียว
    /// </summary>
    private void AppendApproveAttachments(NisTicket ticket, NisApprovePendingDto dto, DateTime now)
    {
        var incoming = (dto.Attachments ?? new())
            .Where(a => !string.IsNullOrWhiteSpace(a.FileName) && !string.IsNullOrWhiteSpace(a.FilePath))
            .ToList();
        if (incoming.Count == 0) return;

        var nextSeq = (ticket.Files?.Count > 0 ? ticket.Files.Max(f => f.Seq) : 0) + 1;
        var files = incoming.Select((a, i) => new NisTicketFile
        {
            FileId = Guid.NewGuid().ToString(),
            TicketId = ticket.TicketId,
            ProjectId = ticket.ProjectId,
            FileName = a.FileName.Length > 300 ? a.FileName[..300] : a.FileName,
            FilePath = a.FilePath.Length > 1000 ? a.FilePath[..1000] : a.FilePath,
            Seq = nextSeq + i,
            FileSize = a.FileSize,
            CmpId = ticket.CmpId,
            CreatedBy = dto.ApprovedBy ?? string.Empty,
            CreatedDate = now,
        }).ToList();

        _context.NisTicketFiles.AddRange(files);
    }

    // ── General (catch-all) project ──────────────────────────────────────────
    // โปรเจกต์ "งานทั่วไป" ต่อบริษัท ใช้รองรับตั๋วที่คำขอไม่ได้ระบุโปรเจกต์จริง
    // (เช่น คำขอจากแอปหน้างานที่ projectId เป็นค่า mock) — ผูก ticket ไว้ที่นี่
    // เพื่อให้ยังแสดงบนบอร์ด. Get-or-create ครั้งเดียวต่อ CmpId ด้วย id ที่คาดเดาได้.
    private const string GeneralProjectNo = "GENERAL";

    private async Task<NisProject> GetOrCreateGeneralProjectAsync(string cmpId, string? createdBy)
    {
        var generalId = $"NIS-GENERAL-{cmpId}";

        var existing = await _context.NisProjects
            .FirstOrDefaultAsync(p => p.ProjectId == generalId);
        if (existing != null)
            return existing;

        var general = new NisProject
        {
            ProjectId = generalId,
            ProjectNo = GeneralProjectNo,
            Name = "งานทั่วไป (ไม่ระบุโปรเจกต์)",
            Type = "Runrate",
            Status = "Active",
            CmpId = cmpId,
            CreatedBy = createdBy ?? string.Empty,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };

        // เพิ่มเข้า context — จะถูก persist พร้อม ticket ใน SaveChanges เดียวของ caller
        _context.NisProjects.Add(general);
        return general;
    }

    // ── DELETE api/nis/pending-requests/{id} ─────────────────────────────────

    /// <summary>
    /// Manager rejects a request. Soft-delete: the row is kept as Status="Rejected"
    /// so the requester still sees it in their history. Matches frontend rejectNisPendingRequest.
    /// </summary>
    [HttpDelete("pending-requests/{id}")]
    public async Task<IActionResult> RejectPendingRequest(
        string id,
        [FromBody] NisRejectPendingDto? dto)
    {
        var request = await _context.NisPendingRequests.FindAsync(id);

        if (request == null)
            return NotFound(new { message = $"Pending request {id} not found" });
        if (request.Status != "Pending")
            return BadRequest(new { message = $"Request already {request.Status}" });

        request.Status = "Rejected";
        request.RejectedBy = dto?.RejectedBy;
        request.RejectReason = TrimOrNull(dto?.Reason, 500);
        request.UpdatedDate = DateTime.Now;

        // คำขอจาก helpdesk: ตั๋ว 'Waiting Approval' ที่สร้างไว้ตอนขอ → ลบทิ้งเพื่อคืนโควตา Onsite Incident case
        // (ยังไม่มีช่างทำงาน/รายงานใด ๆ ผูกกับตั๋ว) — แถวคำขอคงไว้เป็นประวัติพร้อมเหตุผล
        NisTicket? removedTicket = null;
        if (!string.IsNullOrWhiteSpace(request.CreatedTicketId))
        {
            removedTicket = await _context.NisTickets
                .Include(t => t.Files)
                .FirstOrDefaultAsync(t => t.TicketId == request.CreatedTicketId);
            if (removedTicket != null && IsWaitingApproval(removedTicket))
            {
                _context.NisTicketFiles.RemoveRange(removedTicket.Files);
                _context.NisTickets.Remove(removedTicket);
            }
            else
            {
                removedTicket = null;
            }
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.SourceProblemId))
            await NotifyRequestDecisionAsync(request, removedTicket, approved: false);

        return Ok(new { message = "Request rejected", ticketRemoved = removedTicket != null });
    }

    // ── แจ้งเตือนคำขอเปิดเคส Onsite (helpdesk) — best-effort ทั้งหมด ─────────────

    /// <summary>
    /// คำขอใหม่จาก helpdesk → แจ้ง SM ทุกคนของบริษัท 3 ช่องทาง (pattern เดียวกับ NotifyTicketAcceptedAsync):
    /// Expo push (แอป) · socket.io nis:notify type "pending_request" (RN เปิดค้าง) · กระดิ่ง CRM (Service Board)
    /// </summary>
    private async Task NotifyPendingRequestCreatedAsync(NisPendingRequest request, NisTicket ticket, NisProject project)
    {
        try
        {
            var managers = await GetNisManagersAsync(request.CmpId);
            if (managers.Count == 0) return;

            var title = "📥 คำขอเปิดเคส Onsite รออนุมัติ";
            var customer = string.IsNullOrWhiteSpace(request.CustomerName) ? project.Customer : request.CustomerName;
            var body = $"{ticket.TicketCode} · {customer} · {request.Title}";
            if (ticket.OverQuota) body += " (เกินโควตา)";

            foreach (var m in managers.Where(m => !string.IsNullOrWhiteSpace(m.FullName)))
            {
                await _push.SendToStaffAsync(
                    request.CmpId,
                    m.FullName,
                    eventKey: $"pending:{request.RequestId}:{m.FullName}",
                    title: title,
                    body: body,
                    ticketId: ticket.TicketId,
                    data: new Dictionary<string, string> { ["type"] = "pending_request", ["requestId"] = request.RequestId });
            }

            var managerUsernames = managers.Select(m => m.Username).Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
            if (managerUsernames.Count > 0)
            {
                await _nisRealtimeNotify.NotifyAsync(
                    request.CmpId,
                    users: managerUsernames,
                    type: "pending_request",
                    ticketId: ticket.TicketId,
                    title: title,
                    body: body);
            }

            var requesterUsername = await ResolveUsernameAsync(request.CmpId, request.RequestedBy);
            foreach (var username in managerUsernames)
            {
                await _crmNotify.NotifyAsync(
                    request.CmpId,
                    toUsername: username,
                    fromUsername: requesterUsername ?? request.RequestedBy,
                    title: body,
                    // แท็บอนุมัติของ Service Board — moduleFormName นี้ทำให้บอร์ด (use-nis-board-realtime) รีเฟรชทันที
                    linkTo: "/productservice/service-board",
                    moduleFormName: "nis/serviceboard",
                    docNo: ticket.TicketCode ?? ticket.TicketId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS pending request: แจ้งเตือน SM ไม่สำเร็จ (request {RequestId})", request.RequestId);
        }
    }

    /// <summary>
    /// ผล อนุมัติ/ปฏิเสธ คำขอจาก helpdesk → แจ้งผู้ขอ (staff helpdesk) ทางกระดิ่ง CRM + push/socket ถ้ามีแอป
    /// </summary>
    private async Task NotifyRequestDecisionAsync(NisPendingRequest request, NisTicket? ticket, bool approved)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RequestedBy)) return;

            var account = await _context.Accounts
                .Where(a => a.CmpId == request.CmpId && (a.Username == request.RequestedBy || a.FullName == request.RequestedBy))
                .Select(a => new NisNotifyTarget(a.Username, a.FullName ?? string.Empty))
                .FirstOrDefaultAsync();
            var username = account?.Username ?? request.RequestedBy;

            var code = ticket?.TicketCode ?? request.CreatedTicketId ?? string.Empty;
            var title = approved ? "✅ อนุมัติคำขอเปิดเคส Onsite แล้ว" : "⛔ คำขอเปิดเคส Onsite ถูกปฏิเสธ";
            var body = approved
                ? $"{code} · {request.Title}" + (ticket != null && ticket.Assignee != "-" ? $" · มอบหมาย {ticket.Assignee}" : string.Empty)
                : $"{code} · {request.Title}" + (string.IsNullOrWhiteSpace(request.RejectReason) ? string.Empty : $" · เหตุผล: {request.RejectReason}");
            var actor = (approved ? request.ApprovedBy : request.RejectedBy) ?? string.Empty;
            var type = approved ? "request_approved" : "request_rejected";

            if (!string.IsNullOrWhiteSpace(account?.FullName))
            {
                await _push.SendToStaffAsync(
                    request.CmpId,
                    account.FullName,
                    eventKey: $"{type}:{request.RequestId}",
                    title: title,
                    body: body,
                    ticketId: approved ? ticket?.TicketId : null,
                    data: new Dictionary<string, string> { ["type"] = type, ["requestId"] = request.RequestId });
            }

            await _nisRealtimeNotify.NotifyAsync(
                request.CmpId,
                users: [username],
                type: type,
                ticketId: approved ? ticket?.TicketId : null,
                title: title,
                body: body);

            await _crmNotify.NotifyAsync(
                request.CmpId,
                toUsername: username,
                fromUsername: actor,
                title: $"{title} — {body}",
                // อนุมัติ → หน้ารายละเอียดตั๋ว · ปฏิเสธ → Staff Portal (แท็บคำขอของฉัน)
                linkTo: approved && ticket != null
                    ? $"/productservice/service-protal/tickets/{ticket.TicketId}"
                    : "/productservice/service-protal/staff-portal",
                moduleFormName: "nis/staffportal",
                docNo: code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS pending request: แจ้งผลให้ผู้ขอไม่สำเร็จ (request {RequestId})", request.RequestId);
        }
    }

    /// <summary>
    /// อนุมัติคำขอ helpdesk พร้อมมอบหมายช่าง → แจ้งช่างเหมือน PUT tickets/{id}/assign (Expo + socket 'assign')
    /// </summary>
    private async Task NotifyAssignedOnApproveAsync(NisTicket ticket)
    {
        try
        {
            if (ticket.Status != "Scheduled" || ticket.Assignee == "-") return;

            await _push.SendToStaffAsync(
                ticket.CmpId,
                ticket.Assignee,
                eventKey: $"assign:{ticket.TicketId}:{ticket.Assignee}:{DateTime.Today:yyyyMMdd}",
                title: "🔔 งานใหม่รอตอบรับ",
                body: $"{ticket.TicketCode} · {ticket.Title}",
                ticketId: ticket.TicketId,
                data: new Dictionary<string, string> { ["type"] = "assign" });

            var assigneeUsername = await _context.Accounts
                .Where(a => a.CmpId == ticket.CmpId && a.FullName == ticket.Assignee)
                .Select(a => a.Username)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(assigneeUsername))
            {
                await _nisRealtimeNotify.NotifyAsync(
                    ticket.CmpId,
                    users: [assigneeUsername],
                    type: "assign",
                    ticketId: ticket.TicketId,
                    title: "🔔 งานใหม่รอตอบรับ",
                    body: $"{ticket.TicketCode} · {ticket.Title}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS pending request: แจ้งช่างหลังอนุมัติไม่สำเร็จ (ticket {TicketId})", ticket.TicketId);
        }
    }

    /// Accounts.Username จากค่าที่อาจเป็น Username หรือ FullName (RequestedBy รับได้ทั้งสองแบบจาก CRM/RN)
    private async Task<string?> ResolveUsernameAsync(string cmpId, string? usernameOrFullName)
    {
        if (string.IsNullOrWhiteSpace(usernameOrFullName)) return null;
        return await _context.Accounts
            .Where(a => a.CmpId == cmpId && (a.Username == usernameOrFullName || a.FullName == usernameOrFullName))
            .Select(a => a.Username)
            .FirstOrDefaultAsync();
    }

    /// ข้อมูลผู้ใช้ย่อสำหรับแสดงในรายการ (ชื่อเต็ม + รูป)
    private sealed record AccountInfoLite(string FullName, string? ImgPath);

    /// Map (Username หรือ FullName) → Accounts (FullName + imgPath) ทีเดียวหลายค่า — ใช้แสดงชื่อ/avatar ผู้ขอแทน email ใน CRM
    private async Task<Dictionary<string, AccountInfoLite>> ResolveAccountInfosAsync(string cmpId, IEnumerable<string?> usernamesOrFullNames)
    {
        var keys = usernamesOrFullNames
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .Distinct()
            .ToList();
        if (keys.Count == 0) return new Dictionary<string, AccountInfoLite>();

        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.CmpId == cmpId && (keys.Contains(a.Username) || keys.Contains(a.FullName)))
            .Select(a => new { a.Username, a.FullName, a.imgPath })
            .ToListAsync();

        var map = new Dictionary<string, AccountInfoLite>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in accounts)
        {
            if (string.IsNullOrWhiteSpace(a.FullName)) continue;
            var info = new AccountInfoLite(a.FullName, string.IsNullOrWhiteSpace(a.imgPath) ? null : a.imgPath);
            if (!string.IsNullOrWhiteSpace(a.Username)) map.TryAdd(a.Username, info);
            map.TryAdd(a.FullName, info);
        }
        return map;
    }

    // ── GET api/nis/customers ────────────────────────────────────────────────

    /// <summary>
    /// Customer directory for the Service Board Customer tab. Joins msb.mCustomer +
    /// mCustomerLocations + mCustomerAssignEmp (→ Account.FullName) + dbo.Contact
    /// (DocType='customer'). Matches frontend fetchNisCustomers → INisBoardCustomer[].
    /// </summary>
    [HttpGet("customers")]
    public async Task<ActionResult<IEnumerable<NisBoardCustomerDto>>> GetCustomers([FromQuery] string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        // Project + COALESCE only the columns we need. The full Customer entity has
        // many non-nullable string properties that are actually NULL in the DB, which
        // would throw SqlNullValueException when materialized.
        var customers = await _context.customers
            .AsNoTracking()
            .Where(c => c.CmpId == cmpid && c.StateGenQRCode == 1)
            .OrderBy(c => c.CustomerName)
            .Select(c => new
            {
                Code = c.CustomerCode,
                Name = c.CustomerName ?? string.Empty,
                Tax = c.CustomerTaxNo ?? string.Empty,
            })
            .ToListAsync();

        var codes = customers.Select(c => c.Code).ToList();

        var locations = await _context.NisCustomerLocations
            .AsNoTracking()
            .Where(l => l.CmpId == cmpid && codes.Contains(l.CustomerCode))
            .OrderBy(l => l.Seq)
            .ToListAsync();

        var assigns = await _context.NisCustomerAssignEmps
            .AsNoTracking()
            .Where(a => a.CmpId == cmpid && codes.Contains(a.CustomerCode))
            .OrderBy(a => a.Priority)
            .ToListAsync();

        var contacts = await _context.NisContacts
            .AsNoTracking()
            .Where(c => c.CmpId == cmpid && c.DocType == "customer" && c.DocNo != null && codes.Contains(c.DocNo))
            .ToListAsync();

        // Resolve assigned AccountIDs → FullName.
        var accountIds = assigns.Select(a => a.AccountID).Distinct().ToList();
        var accountNames = await _context.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.AccountId))
            .Select(a => new { a.AccountId, Name = a.FullName ?? string.Empty })
            .ToDictionaryAsync(a => a.AccountId, a => a.Name);

        var locationsByCustomer = locations.GroupBy(l => l.CustomerCode).ToDictionary(g => g.Key, g => g.ToList());
        var contactsByCustomer = contacts.GroupBy(c => c.DocNo!).ToDictionary(g => g.Key, g => g.ToList());
        var staffByCustomer = assigns
            .GroupBy(a => a.CustomerCode)
            .ToDictionary(
                g => g.Key,
                g => g.Select(a => accountNames.TryGetValue(a.AccountID, out var n) ? n : null)
                      .Where(n => !string.IsNullOrWhiteSpace(n))
                      .Select(n => n!)
                      .ToList());

        var result = customers.Select(c =>
        {
            var custStaff = staffByCustomer.TryGetValue(c.Code, out var s) ? s : new List<string>();

            var locDtos = (locationsByCustomer.TryGetValue(c.Code, out var locs) ? locs : new List<NisCustomerLocation>())
                .Select(l => new NisCustomerLocationDto
                {
                    Label = l.LocationName ?? string.Empty,
                    Address = l.Remark ?? string.Empty,
                    // mCustomerAssignEmp is customer-level, so each site inherits the same caretakers.
                    AssignedStaff = new List<string>(custStaff),
                    Coordinates = l.Lat.HasValue && l.Lon.HasValue ? $"{l.Lat},{l.Lon}" : null,
                    LocationUrl = l.LocationURL,
                })
                .ToList();

            var conDtos = (contactsByCustomer.TryGetValue(c.Code, out var cons) ? cons : new List<NisContactRow>())
                .Select(x => new NisCustomerContactDto
                {
                    Name = x.ContactName ?? string.Empty,
                    Phone = x.ContactPhone ?? string.Empty,
                    Email = x.ContactEmail ?? string.Empty,
                    Role = x.ContactPosition ?? string.Empty,
                })
                .ToList();

            return new NisBoardCustomerDto
            {
                Id = c.Code,
                Name = c.Name,
                TaxId = c.Tax,
                AssignedStaff = custStaff,
                Contacts = conDtos,
                Locations = locDtos,
            };
        }).ToList();

        return Ok(result);
    }

    // Caretaker assign/unassign (msb.mCustomerAssignEmp writes) is handled by the
    // existing CustomerAssignEmp API — not duplicated here.

    // ── POST/PUT api/nis/customers — save contacts + locations ───────────────

    /// <summary>Create — saves the customer directory entry (contacts + locations).</summary>
    [HttpPost("customers")]
    public Task<IActionResult> CreateCustomer([FromBody] NisCustomerSaveDto dto) =>
        SaveNisCustomerAsync(dto.Id, dto);

    /// <summary>Update — saves the customer directory entry (contacts + locations).</summary>
    [HttpPut("customers/{id}")]
    public Task<IActionResult> UpdateCustomer(string id, [FromBody] NisCustomerSaveDto dto) =>
        SaveNisCustomerAsyncNew(id, dto);

    /// <summary>Parse a "lat,lon" string into decimals (InvariantCulture).</summary>
    private static (decimal? Lat, decimal? Lon) ParseLatLon(string? coordinates)
    {
        if (string.IsNullOrWhiteSpace(coordinates)) return (null, null);
        var parts = coordinates.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return (null, null);
        decimal? lat = decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var la) ? la : null;
        decimal? lon = decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var lo) ? lo : null;
        return (lat, lon);
    }

    /// <summary>
    /// Replaces the customer's contacts (dbo.Contact, DocType='customer') and locations
    /// (msb.mCustomerLocations) with the submitted set. Does NOT touch msb.mCustomer
    /// (master data) or mCustomerAssignEmp (managed by the caretakers matrix).
    /// </summary>
    private async Task<IActionResult> SaveNisCustomerAsync(string? code, NisCustomerSaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { message = "customer code (id) is required" });

        var cmpId = dto.Cmpid ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });

        // The customer must already exist in the master table (picked from the list).
        /*       var exists = await _context.customers
                  .AsNoTracking()
                  .AnyAsync(c => c.CmpId == cmpId && c.CustomerCode == code);
              if (!exists)
                  return BadRequest(new { message = $"Customer '{code}' not found in master" }); */


        var customer = await _context.customers
      .FirstOrDefaultAsync(c =>
       c.CmpId == cmpId &&
       c.CustomerCode == code);

        if (customer == null)
            return BadRequest(new
            {
                message = $"Customer '{code}' not found in master"
            });

        var user = dto.UpdatedBy ?? dto.CreatedBy;

        using var tx = await _context.Database.BeginTransactionAsync();

        customer.StateGenQRCode = 1;

        // Replace contacts (delete-then-insert so it survives re-saves cleanly).
        // dto.Contacts == null = caller บันทึกเฉพาะสถานที่ → ห้ามลบผู้ติดต่อเดิม
        if (dto.Contacts != null)
        {
            var oldContacts = await _context.NisContacts
                .Where(c => c.CmpId == cmpId && c.DocType == "customer" && c.DocNo == code)
                .ToListAsync();
            _context.NisContacts.RemoveRange(oldContacts);
        }

        var oldLocations = await _context.NisCustomerLocations
            .Where(l => l.CmpId == cmpId && l.CustomerCode == code)
            .ToListAsync();
        _context.NisCustomerLocations.RemoveRange(oldLocations);

        await _context.SaveChangesAsync();

        foreach (var con in (dto.Contacts ?? new()).Where(c => !string.IsNullOrWhiteSpace(c.Name)))
        {
            _context.NisContacts.Add(new NisContactRow
            {
                ContactId = Guid.NewGuid().ToString(),
                ContactName = con.Name,
                ContactPhone = con.Phone,
                ContactEmail = con.Email,
                ContactPosition = con.Role,
                CmpId = cmpId,
                DocType = "customer",
                DocNo = code,
            });
        }

        var seq = 1;
        foreach (var loc in dto.Locations.Where(l => !string.IsNullOrWhiteSpace(l.Label) || !string.IsNullOrWhiteSpace(l.Address)))
        {
            var (lat, lon) = ParseLatLon(loc.Coordinates);
            _context.NisCustomerLocations.Add(new NisCustomerLocation
            {
                CustomerCode = code,
                CmpId = cmpId,
                Seq = seq++,
                LocationName = loc.Label,
                Remark = loc.Address,
                Lat = lat,
                Lon = lon,
                LocationURL = loc.LocationUrl,
                UpdUser = user,
            });
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        // Echo back the saved customer + its current caretakers.
        var assigns = await _context.NisCustomerAssignEmps
            .AsNoTracking()
            .Where(a => a.CmpId == cmpId && a.CustomerCode == code)
            .OrderBy(a => a.Priority)
            .ToListAsync();
        var accountIds = assigns.Select(a => a.AccountID).Distinct().ToList();
        var accountNames = await _context.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.AccountId))
            .Select(a => new { a.AccountId, Name = a.FullName ?? string.Empty })
            .ToDictionaryAsync(a => a.AccountId, a => a.Name);
        var assignedStaff = assigns
            .Select(a => accountNames.TryGetValue(a.AccountID, out var n) ? n : null)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .ToList();

        return Ok(new NisBoardCustomerDto
        {
            Id = code,
            Name = dto.Name ?? string.Empty,
            TaxId = dto.TaxId ?? string.Empty,
            AssignedStaff = assignedStaff,
            // ไม่ได้ส่ง contacts มา → คืนผู้ติดต่อจริงจาก DB ให้ client เก็บ record ล่าสุด
            Contacts = dto.Contacts ?? await LoadNisCustomerContactsAsync(cmpId, code),
            Locations = dto.Locations,
        });
    }


    /// <summary>ผู้ติดต่อของลูกค้า (dbo.Contact, DocType='customer') ในรูป DTO ของ Customer tab</summary>
    private async Task<List<NisCustomerContactDto>> LoadNisCustomerContactsAsync(string cmpId, string code)
    {
        return await _context.NisContacts
            .AsNoTracking()
            .Where(c => c.CmpId == cmpId && c.DocType == "customer" && c.DocNo == code)
            .Select(x => new NisCustomerContactDto
            {
                Name = x.ContactName ?? string.Empty,
                Phone = x.ContactPhone ?? string.Empty,
                Email = x.ContactEmail ?? string.Empty,
                Role = x.ContactPosition ?? string.Empty,
            })
            .ToListAsync();
    }

    private async Task<IActionResult> SaveNisCustomerAsyncNew(
        string? code,
        NisCustomerSaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { message = "customer code (id) is required" });

        var cmpId = dto.Cmpid ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });

        var customer = await _context.customers
            .FirstOrDefaultAsync(c =>
                c.CmpId == cmpId &&
                c.CustomerCode == code);

        if (customer == null)
        {
            return BadRequest(new
            {
                message = $"Customer '{code}' not found in master"
            });
        }

        var user = dto.UpdatedBy ?? dto.CreatedBy;

        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            // EF Core Tracking จะ update field นี้เมื่อ SaveChangesAsync()
            customer.StateGenQRCode = 1;

            // ถ้ามี column ผู้แก้ไข/วันที่แก้ไข
            // customer.UpdUser = user;
            // customer.UpdDate = DateTime.Now;

            // dto.Contacts == null = caller บันทึกเฉพาะสถานที่ → ห้ามลบผู้ติดต่อเดิม
            if (dto.Contacts != null)
            {
                var oldContacts = await _context.NisContacts
                    .Where(c =>
                        c.CmpId == cmpId &&
                        c.DocType == "customer" &&
                        c.DocNo == code)
                    .ToListAsync();

                _context.NisContacts.RemoveRange(oldContacts);
            }

            var oldLocations = await _context.NisCustomerLocations
                .Where(l =>
                    l.CmpId == cmpId &&
                    l.CustomerCode == code)
                .ToListAsync();

            _context.NisCustomerLocations.RemoveRange(oldLocations);

            foreach (var con in (dto.Contacts ?? new())
                         .Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                _context.NisContacts.Add(new NisContactRow
                {
                    ContactId = Guid.NewGuid().ToString(),
                    ContactName = con.Name,
                    ContactPhone = con.Phone,
                    ContactEmail = con.Email,
                    ContactPosition = con.Role,
                    CmpId = cmpId,
                    DocType = "customer",
                    DocNo = code,
                });
            }

            var seq = 1;

            foreach (var loc in dto.Locations.Where(l =>
                         !string.IsNullOrWhiteSpace(l.Label) ||
                         !string.IsNullOrWhiteSpace(l.Address)))
            {
                var (lat, lon) = ParseLatLon(loc.Coordinates);

                _context.NisCustomerLocations.Add(new NisCustomerLocation
                {
                    CustomerCode = code,
                    CmpId = cmpId,
                    Seq = seq++,
                    LocationName = loc.Label,
                    Remark = loc.Address,
                    Lat = lat,
                    Lon = lon,
                    LocationURL = loc.LocationUrl,
                    UpdUser = user,
                });
            }

            // บันทึกทั้งหมดในครั้งเดียว:
            // 1. UPDATE customer
            // 2. DELETE contacts เดิม
            // 3. DELETE locations เดิม
            // 4. INSERT contacts ใหม่
            // 5. INSERT locations ใหม่
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to save customer",
                detail = ex.Message
            });
        }

        var assigns = await _context.NisCustomerAssignEmps
            .AsNoTracking()
            .Where(a => a.CmpId == cmpId && a.CustomerCode == code)
            .OrderBy(a => a.Priority)
            .ToListAsync();

        var accountIds = assigns
            .Select(a => a.AccountID)
            .Distinct()
            .ToList();

        var accountNames = await _context.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.AccountId))
            .Select(a => new
            {
                a.AccountId,
                Name = a.FullName ?? string.Empty
            })
            .ToDictionaryAsync(a => a.AccountId, a => a.Name);

        var assignedStaff = assigns
            .Select(a =>
                accountNames.TryGetValue(a.AccountID, out var name)
                    ? name
                    : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();

        return Ok(new NisBoardCustomerDto
        {
            Id = code,
            Name = dto.Name ?? string.Empty,
            TaxId = dto.TaxId ?? string.Empty,
            AssignedStaff = assignedStaff,
            // ไม่ได้ส่ง contacts มา → คืนผู้ติดต่อจริงจาก DB ให้ client เก็บ record ล่าสุด
            Contacts = dto.Contacts ?? await LoadNisCustomerContactsAsync(cmpId, code),
            Locations = dto.Locations,
        });
    }

    // ── Equipment master (dbo.NisEquipment) — อุปกรณ์ใน Rack ต่อลูกค้า ──────────

    private static NisEquipmentDto MapEquipment(NisEquipment e) => new()
    {
        Id = e.Id.ToString(),
        RackName = e.RackName,
        RackLocation = e.RackLocation,
        UPosition = e.UPosition,
        DeviceName = e.DeviceName,
        SerialNo = e.SerialNo,
        Model = e.Model,
        Brand = e.Brand,
        Note = e.Note,
    };

    /// <summary>
    /// Returns one row per customer that has at least one saved equipment row, with the count —
    /// used to render the customer list on the Equipment Master landing page. Matches frontend
    /// fetchNisEquipmentCustomers.
    /// </summary>
    [HttpGet("equipment/customers")]
    public async Task<ActionResult<IEnumerable<NisEquipmentCustomerSummaryDto>>> GetNisEquipmentCustomers(
        [FromQuery] string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var counts = await _context.NisEquipments
            .AsNoTracking()
            .Where(e => e.CmpId == cmpid)
            .GroupBy(e => e.CustomerCode)
            .Select(g => new { CustomerCode = g.Key, Count = g.Count() })
            .ToListAsync();

        var codes = counts.Select(c => c.CustomerCode).ToList();
        var names = await _context.customers
            .AsNoTracking()
            .Where(c => c.CmpId == cmpid && codes.Contains(c.CustomerCode))
            .Select(c => new { c.CustomerCode, c.CustomerName })
            .ToDictionaryAsync(c => c.CustomerCode, c => c.CustomerName ?? string.Empty);

        var result = counts
            .Select(c => new NisEquipmentCustomerSummaryDto
            {
                CustomerCode = c.CustomerCode,
                CustomerName = names.TryGetValue(c.CustomerCode, out var n) ? n : c.CustomerCode,
                Count = c.Count,
            })
            .OrderBy(c => c.CustomerName)
            .ToList();

        return Ok(result);
    }

    /// <summary>Returns the equipment list for one customer. Matches frontend fetchNisEquipment.</summary>
    [HttpGet("equipment")]
    public async Task<ActionResult<IEnumerable<NisEquipmentDto>>> GetNisEquipment(
        [FromQuery] string? cmpid,
        [FromQuery] string? customerCode)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(new { message = "customerCode is required" });

        var items = await _context.NisEquipments
            .AsNoTracking()
            .Where(e => e.CmpId == cmpid && e.CustomerCode == customerCode)
            .OrderBy(e => e.RackName)
            .ThenBy(e => e.DeviceName)
            .ToListAsync();

        return Ok(items.Select(MapEquipment));
    }

    /// <summary>Replaces the full equipment list for one customer. Matches frontend saveNisEquipment.</summary>
    [HttpPost("equipment")]
    public async Task<ActionResult<IEnumerable<NisEquipmentDto>>> SaveNisEquipment([FromBody] NisEquipmentSaveDto dto)
    {
        var cmpId = dto.Cmpid ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.CustomerCode))
            return BadRequest(new { message = "customerCode is required" });

        var user = dto.UpdatedBy ?? string.Empty;
        var now = BangkokNow();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var old = await _context.NisEquipments
                .Where(e => e.CmpId == cmpId && e.CustomerCode == dto.CustomerCode)
                .ToListAsync();
            _context.NisEquipments.RemoveRange(old);

            foreach (var item in dto.Items.Where(i =>
                         !string.IsNullOrWhiteSpace(i.RackName) ||
                         !string.IsNullOrWhiteSpace(i.DeviceName) ||
                         !string.IsNullOrWhiteSpace(i.SerialNo)))
            {
                _context.NisEquipments.Add(new NisEquipment
                {
                    Id = Guid.NewGuid(),
                    CmpId = cmpId,
                    CustomerCode = dto.CustomerCode,
                    RackName = item.RackName,
                    RackLocation = item.RackLocation ?? string.Empty,
                    UPosition = item.UPosition,
                    DeviceName = item.DeviceName,
                    SerialNo = item.SerialNo,
                    Model = item.Model,
                    Brand = item.Brand,
                    Note = item.Note,
                    CreatedBy = user,
                    CreatedDate = now,
                    UpdatedBy = user,
                    UpdatedDate = now,
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to save equipment",
                detail = ex.Message,
            });
        }

        // แจ้งหน้า CRM ที่เปิดอยู่ให้ refetch รายการอุปกรณ์ของลูกค้านี้ (SignalR, ไม่บล็อก response)
        await _crmNotify.BroadcastEquipmentChangedAsync(cmpId, dto.CustomerCode);

        var saved = await _context.NisEquipments
            .AsNoTracking()
            .Where(e => e.CmpId == cmpId && e.CustomerCode == dto.CustomerCode)
            .OrderBy(e => e.RackName)
            .ThenBy(e => e.DeviceName)
            .ToListAsync();

        return Ok(saved.Select(MapEquipment));
    }

    /// <summary>
    /// Imports equipment rows from an uploaded Excel file (.xlsx/.xls) — appends to the
    /// customer's existing list (does NOT replace). Header row required; columns matched
    /// case-insensitively by Thai/English name. Matches frontend importNisEquipment.
    /// </summary>
    [HttpPost("equipment/import")]
    public async Task<ActionResult<NisEquipmentImportResultDto>> ImportNisEquipment(
        IFormFile file,
        [FromForm] string cmpid,
        [FromForm] string customerCode,
        [FromForm] string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(new { message = "customerCode is required" });
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "file is required" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest(new { message = "รองรับเฉพาะไฟล์ .xlsx / .xls" });

        List<NisEquipment> parsed;
        try
        {
            parsed = ParseEquipmentExcel(file, cmpid, customerCode, updatedBy ?? string.Empty);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "ไม่สามารถอ่านไฟล์ Excel ได้", detail = ex.Message });
        }

        if (parsed.Count == 0)
            return BadRequest(new { message = "ไม่พบข้อมูลอุปกรณ์ในไฟล์ (ต้องมีแถวหัวตาราง + อย่างน้อย 1 แถวข้อมูล)" });

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.NisEquipments.AddRange(parsed);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to import equipment",
                detail = ex.Message,
            });
        }

        await _crmNotify.BroadcastEquipmentChangedAsync(cmpid, customerCode);

        return Ok(new NisEquipmentImportResultDto
        {
            Imported = parsed.Count,
            Items = parsed.Select(MapEquipment).ToList(),
        });
    }

    /// อ่าน worksheet แรกของไฟล์ Excel — แถวแรกเป็น header, map คอลัมน์แบบ case-insensitive
    /// ตามชื่อไทย/อังกฤษที่ทีมตกลง (rack/ชื่อตู้, device/อุปกรณ์, sn, model/รุ่น, brand/ยี่ห้อ, note/หมายเหตุ)
    /// คอลัมน์เสริม (ไม่บังคับ): U/UPosition/ตำแหน่ง U → UPosition, Location/RackLocation/ตำแหน่งตู้ → RackLocation
    private static List<NisEquipment> ParseEquipmentExcel(IFormFile file, string cmpid, string customerCode, string updatedBy)
    {
        var result = new List<NisEquipment>();
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;

        using var stream = file.OpenReadStream();
        using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
        var ws = workbook.Worksheets.First();
        var firstRow = ws.FirstRowUsed();
        if (firstRow == null) return result;

        var headerRow = firstRow.RowNumber();
        var colIndex = new Dictionary<string, int>();
        foreach (var cell in firstRow.CellsUsed())
        {
            var key = cell.GetString().Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(key)) colIndex[key] = cell.Address.ColumnNumber;
        }

        int? FindCol(params string[] names) =>
            names.Select(n => n.ToLowerInvariant())
                .Where(colIndex.ContainsKey)
                .Select(n => colIndex[n])
                .Cast<int?>()
                .FirstOrDefault();

        var rackCol = FindCol("rackname", "rack", "ชื่อตู้", "ตู้", "ตู้ rack");
        var rackLocationCol = FindCol("racklocation", "location", "ตำแหน่งตู้");
        var uPositionCol = FindCol("uposition", "u", "ตำแหน่ง u");
        var deviceCol = FindCol("devicename", "device", "ชื่ออุปกรณ์", "อุปกรณ์");
        var snCol = FindCol("serialno", "sn", "serial", "หมายเลขเครื่อง");
        var modelCol = FindCol("model", "รุ่น");
        var brandCol = FindCol("brand", "ยี่ห้อ");
        var noteCol = FindCol("note", "remark", "หมายเหตุ");

        string CellText(IXLRow row, int? col) =>
            col == null ? string.Empty : row.Cell(col.Value).GetString().Trim();

        // "U" / "ตำแหน่ง U" — รับทั้งเลขล้วนและรูปแบบ "U12" / "u 12"; อ่านไม่ออก = null
        static int? ParseUPosition(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var digits = raw.Trim().TrimStart('U', 'u').Trim();
            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var u) ? u : null;
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        for (var r = headerRow + 1; r <= lastRow; r++)
        {
            var row = ws.Row(r);
            var rack = CellText(row, rackCol);
            var rackLocation = CellText(row, rackLocationCol);
            var uPosition = ParseUPosition(CellText(row, uPositionCol));
            var device = CellText(row, deviceCol);
            var sn = CellText(row, snCol);
            var model = CellText(row, modelCol);
            var brand = CellText(row, brandCol);
            var note = CellText(row, noteCol);

            if (string.IsNullOrWhiteSpace(rack) && string.IsNullOrWhiteSpace(device) && string.IsNullOrWhiteSpace(sn))
                continue; // แถวว่าง — ข้าม

            result.Add(new NisEquipment
            {
                Id = Guid.NewGuid(),
                CmpId = cmpid,
                CustomerCode = customerCode,
                RackName = rack,
                RackLocation = rackLocation,
                UPosition = uPosition,
                DeviceName = device,
                SerialNo = sn,
                Model = model,
                Brand = brand,
                Note = note,
                CreatedBy = updatedBy,
                CreatedDate = now,
                UpdatedBy = updatedBy,
                UpdatedDate = now,
            });
        }

        return result;
    }

    #region Equipment change requests
    // ── api/nis/equipment-change-requests (dbo.NisEquipmentChangeRequest) ─────
    // ช่างแจ้งแก้ทะเบียนตู้ Rack จากหน้าตรวจ PM → SM อนุมัติ/ปฏิเสธใน Service Board
    // ทะเบียน NisEquipment เปลี่ยนเฉพาะตอน approve (insert / delete / update ตาม Reason)

    private const string EQUIP_CR_REASON_MISSING = "missing_in_registry";
    private const string EQUIP_CR_REASON_NOT_IN_RACK = "not_in_rack";
    private const string EQUIP_CR_REASON_INCORRECT = "incorrect_info";

    private static readonly HashSet<string> EquipmentChangeReasons = new(StringComparer.Ordinal)
    {
        EQUIP_CR_REASON_MISSING, EQUIP_CR_REASON_NOT_IN_RACK, EQUIP_CR_REASON_INCORRECT,
    };

    private static NisEquipmentChangeRequestDto MapEquipmentChangeRequest(
        NisEquipmentChangeRequest r, string? customerName, string? ticketCode) => new()
    {
        RequestId = r.RequestId,
        CmpId = r.CmpId,
        CustomerCode = r.CustomerCode,
        CustomerName = customerName,
        TicketId = r.TicketId,
        TicketCode = ticketCode,
        RackName = r.RackName,
        Reason = r.Reason,
        EquipmentId = r.EquipmentId,
        DeviceName = r.DeviceName,
        Brand = r.Brand,
        Model = r.Model,
        SerialNo = r.SerialNo,
        UPosition = r.UPosition,
        Note = r.Note,
        RequestedBy = r.RequestedBy,
        Status = r.Status,
        ApprovedBy = r.ApprovedBy,
        RejectedBy = r.RejectedBy,
        RejectReason = r.RejectReason,
        CreatedDate = r.CreatedDate,
        UpdatedDate = r.UpdatedDate,
    };

    /// เติม CustomerName (msb customers — แหล่งเดียวกับ GET equipment/customers) + TicketCode (NisTicket)
    private async Task<List<NisEquipmentChangeRequestDto>> MapEquipmentChangeRequestsAsync(
        string cmpId, List<NisEquipmentChangeRequest> rows)
    {
        if (rows.Count == 0) return new List<NisEquipmentChangeRequestDto>();

        var codes = rows.Select(r => r.CustomerCode).Distinct().ToList();
        var names = await _context.customers
            .AsNoTracking()
            .Where(c => c.CmpId == cmpId && codes.Contains(c.CustomerCode))
            .Select(c => new { c.CustomerCode, c.CustomerName })
            .ToDictionaryAsync(c => c.CustomerCode, c => c.CustomerName ?? string.Empty);

        var ticketIds = rows.Where(r => !string.IsNullOrWhiteSpace(r.TicketId)).Select(r => r.TicketId!).Distinct().ToList();
        var ticketCodes = ticketIds.Count == 0
            ? new Dictionary<string, string?>()
            : await _context.NisTickets
                .AsNoTracking()
                .Where(t => ticketIds.Contains(t.TicketId))
                .Select(t => new { t.TicketId, t.TicketCode })
                .ToDictionaryAsync(t => t.TicketId, t => t.TicketCode);

        var accountInfos = await ResolveAccountInfosAsync(cmpId, rows.Select(r => r.RequestedBy));

        return rows.Select(r =>
            {
                var dto = MapEquipmentChangeRequest(
                    r,
                    names.TryGetValue(r.CustomerCode, out var n) ? n : null,
                    r.TicketId != null && ticketCodes.TryGetValue(r.TicketId, out var tc) ? tc : null);
                if (!string.IsNullOrWhiteSpace(r.RequestedBy) && accountInfos.TryGetValue(r.RequestedBy.Trim(), out var requester))
                {
                    dto.RequestedByName = requester.FullName;
                    dto.RequestedByImgPath = requester.ImgPath;
                }
                return dto;
            })
            .ToList();
    }

    /// <summary>
    /// Lists equipment change requests for a company, newest first. Optional filters: status
    /// (Pending|Approved|Rejected), customerCode, ticketId. Used by the Service Board approve tab.
    /// </summary>
    [HttpGet("equipment-change-requests")]
    public async Task<ActionResult<IEnumerable<NisEquipmentChangeRequestDto>>> GetEquipmentChangeRequests(
        [FromQuery] string? cmpid,
        [FromQuery] string? status,
        [FromQuery] string? customerCode,
        [FromQuery] string? ticketId)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var query = _context.NisEquipmentChangeRequests
            .AsNoTracking()
            .Where(r => r.CmpId == cmpid);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(customerCode))
            query = query.Where(r => r.CustomerCode == customerCode);
        if (!string.IsNullOrWhiteSpace(ticketId))
            query = query.Where(r => r.TicketId == ticketId);

        var rows = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
        return Ok(await MapEquipmentChangeRequestsAsync(cmpid, rows));
    }

    /// <summary>
    /// Creates an equipment change request (Status = Pending) from the onsite PM rack inspection.
    /// For not_in_rack / incorrect_info the target equipmentId must exist for that company + customer.
    /// The equipment master is NOT changed until a manager approves.
    /// </summary>
    [HttpPost("equipment-change-requests")]
    public async Task<ActionResult<NisEquipmentChangeRequestDto>> CreateEquipmentChangeRequest(
        [FromBody] NisEquipmentChangeRequestCreateDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.CustomerCode))
            return BadRequest(new { message = "customerCode is required" });
        if (string.IsNullOrWhiteSpace(dto.RackName))
            return BadRequest(new { message = "rackName is required" });
        if (string.IsNullOrWhiteSpace(dto.DeviceName))
            return BadRequest(new { message = "deviceName is required" });
        if (string.IsNullOrWhiteSpace(dto.RequestedBy))
            return BadRequest(new { message = "requestedBy is required" });
        if (!EquipmentChangeReasons.Contains(dto.Reason ?? string.Empty))
            return BadRequest(new { message = "reason must be one of missing_in_registry | not_in_rack | incorrect_info" });

        string? equipmentId = null;
        if (dto.Reason != EQUIP_CR_REASON_MISSING)
        {
            if (string.IsNullOrWhiteSpace(dto.EquipmentId) || !Guid.TryParse(dto.EquipmentId, out var equipGuid))
                return BadRequest(new { message = "equipmentId is required for this reason" });

            var exists = await _context.NisEquipments
                .AsNoTracking()
                .AnyAsync(e => e.Id == equipGuid && e.CmpId == cmpId && e.CustomerCode == dto.CustomerCode);
            if (!exists)
                return BadRequest(new { message = "equipmentId not found for this customer" });

            equipmentId = equipGuid.ToString();
        }

        var now = BangkokNow();
        var row = new NisEquipmentChangeRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            CmpId = cmpId,
            CustomerCode = dto.CustomerCode.Trim(),
            TicketId = string.IsNullOrWhiteSpace(dto.TicketId) ? null : dto.TicketId.Trim(),
            RackName = dto.RackName.Trim(),
            Reason = dto.Reason!,
            EquipmentId = equipmentId,
            DeviceName = dto.DeviceName.Trim(),
            Brand = (dto.Brand ?? string.Empty).Trim(),
            Model = (dto.Model ?? string.Empty).Trim(),
            SerialNo = (dto.SerialNo ?? string.Empty).Trim(),
            UPosition = dto.UPosition,
            Note = (dto.Note ?? string.Empty).Trim(),
            RequestedBy = dto.RequestedBy.Trim(),
            Status = "Pending",
            CreatedDate = now,
            UpdatedDate = now,
        };

        _context.NisEquipmentChangeRequests.Add(row);
        await _context.SaveChangesAsync();

        var mapped = await MapEquipmentChangeRequestsAsync(cmpId, new List<NisEquipmentChangeRequest> { row });
        return Ok(mapped[0]);
    }

    /// <summary>
    /// Approves a pending equipment change request and applies it to the equipment master in one
    /// transaction: missing_in_registry → insert NisEquipment; not_in_rack → delete the target row;
    /// incorrect_info → update only the non-blank fields (and UPosition when provided) on the target row.
    /// </summary>
    [HttpPut("equipment-change-requests/{id}/approve")]
    public async Task<ActionResult<NisEquipmentChangeRequestDto>> ApproveEquipmentChangeRequest(
        string id,
        [FromBody] NisEquipmentChangeRequestApproveDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.ApprovedBy))
            return BadRequest(new { message = "approvedBy is required" });

        var row = await _context.NisEquipmentChangeRequests
            .FirstOrDefaultAsync(r => r.RequestId == id && r.CmpId == cmpId);
        if (row == null)
            return NotFound(new { message = $"Equipment change request '{id}' not found" });
        if (row.Status != "Pending")
            return BadRequest(new { message = $"Request is already {row.Status}" });

        var now = BangkokNow();
        var approver = dto.ApprovedBy.Trim();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            NisEquipment? target = null;
            if (row.Reason != EQUIP_CR_REASON_MISSING && Guid.TryParse(row.EquipmentId, out var equipGuid))
            {
                target = await _context.NisEquipments
                    .FirstOrDefaultAsync(e => e.Id == equipGuid && e.CmpId == cmpId && e.CustomerCode == row.CustomerCode);
            }

            switch (row.Reason)
            {
                case EQUIP_CR_REASON_MISSING:
                    _context.NisEquipments.Add(new NisEquipment
                    {
                        Id = Guid.NewGuid(),
                        CmpId = cmpId,
                        CustomerCode = row.CustomerCode,
                        RackName = row.RackName,
                        DeviceName = row.DeviceName,
                        Brand = row.Brand,
                        Model = row.Model,
                        SerialNo = row.SerialNo,
                        UPosition = row.UPosition,
                        Note = row.Note,
                        CreatedBy = approver,
                        CreatedDate = now,
                        UpdatedBy = approver,
                        UpdatedDate = now,
                    });
                    break;

                case EQUIP_CR_REASON_NOT_IN_RACK:
                    // แถวอาจถูกลบไปก่อนแล้ว (เช่น save replace-all จากหน้า master) — ถือว่าสำเร็จ
                    if (target != null) _context.NisEquipments.Remove(target);
                    break;

                case EQUIP_CR_REASON_INCORRECT:
                    if (target != null)
                    {
                        if (!string.IsNullOrWhiteSpace(row.DeviceName)) target.DeviceName = row.DeviceName;
                        if (!string.IsNullOrWhiteSpace(row.Brand)) target.Brand = row.Brand;
                        if (!string.IsNullOrWhiteSpace(row.Model)) target.Model = row.Model;
                        if (!string.IsNullOrWhiteSpace(row.SerialNo)) target.SerialNo = row.SerialNo;
                        if (row.UPosition.HasValue) target.UPosition = row.UPosition;
                        target.UpdatedBy = approver;
                        target.UpdatedDate = now;
                    }
                    break;
            }

            row.Status = "Approved";
            row.ApprovedBy = approver;
            row.UpdatedDate = now;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unable to approve equipment change request",
                detail = ex.Message,
            });
        }

        // approve มีผลกับ NisEquipment จริง (insert/update/delete) → แจ้งหน้าที่เปิดอยู่
        await _crmNotify.BroadcastEquipmentChangedAsync(cmpId, row.CustomerCode);

        var mapped = await MapEquipmentChangeRequestsAsync(cmpId, new List<NisEquipmentChangeRequest> { row });
        return Ok(mapped[0]);
    }

    /// <summary>Rejects a pending equipment change request — the equipment master is left untouched.</summary>
    [HttpPut("equipment-change-requests/{id}/reject")]
    public async Task<ActionResult<NisEquipmentChangeRequestDto>> RejectEquipmentChangeRequest(
        string id,
        [FromBody] NisEquipmentChangeRequestRejectDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.RejectedBy))
            return BadRequest(new { message = "rejectedBy is required" });

        var row = await _context.NisEquipmentChangeRequests
            .FirstOrDefaultAsync(r => r.RequestId == id && r.CmpId == cmpId);
        if (row == null)
            return NotFound(new { message = $"Equipment change request '{id}' not found" });
        if (row.Status != "Pending")
            return BadRequest(new { message = $"Request is already {row.Status}" });

        row.Status = "Rejected";
        row.RejectedBy = dto.RejectedBy.Trim();
        row.RejectReason = string.IsNullOrWhiteSpace(dto.RejectReason) ? null : dto.RejectReason.Trim();
        row.UpdatedDate = BangkokNow();
        await _context.SaveChangesAsync();

        var mapped = await MapEquipmentChangeRequestsAsync(cmpId, new List<NisEquipmentChangeRequest> { row });
        return Ok(mapped[0]);
    }

    #endregion

    // ── GET api/nis/sales-orders ─────────────────────────────────────────────

    /// <summary>Returns NIS sales orders for SO picker in NewProject wizard. Matches frontend fetchNisSalesOrders.</summary>
    [HttpGet("sales-orders")]
    public async Task<ActionResult<IEnumerable<NisSalesOrderResponseDto>>> GetSalesOrders(
        [FromQuery] string? cmpid,
        [FromQuery] string? username)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var orders = await _context.NisSalesOrders
            .AsNoTracking()
            .Where(s => s.CmpId == cmpid)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();

        return Ok(orders.Select(s => new NisSalesOrderResponseDto
        {
            Id = s.SoId,
            QuoteRef = s.QuoteRef,
            Customer = s.Customer,
            Date = FormatDate(s.Date),
            Type = s.Type,
            Value = s.Value,
            Status = s.Status,
            Project = s.Project,
            PoNumber = s.PoNumber,
            PoDate = FormatDate(s.PoDate),
            SalesName = s.SalesName,
        }));
    }


    // ── GET api/nis/system-config ────────────────────────────────────────────

    /// <summary>Returns system config for a company. Returns defaults if not yet saved.</summary>
    [HttpGet("system-config")]
    public async Task<ActionResult<NisSystemConfigResponseDto>> GetSystemConfig(
        [FromQuery] string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var entity = await _context.NisSystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CmpId == cmpid);

        if (entity == null)
            return Ok(BuildDefaultConfig());

        return Ok(MapSystemConfig(entity));
    }

    // ── PUT api/nis/system-config ────────────────────────────────────────────

    /// <summary>Upserts system config for a company.</summary>
    [HttpPut("system-config")]
    public async Task<ActionResult<NisSystemConfigResponseDto>> SaveSystemConfig(
        [FromBody] NisSystemConfigSaveDto dto,
        [FromQuery] string? cmpid)
    {
        var effectiveCmpId = dto.CmpId ?? cmpid ?? string.Empty;
        if (string.IsNullOrWhiteSpace(effectiveCmpId))
            return BadRequest(new { message = "cmpid is required" });

        var entity = await _context.NisSystemConfigs
            .FirstOrDefaultAsync(c => c.CmpId == effectiveCmpId);

        if (entity == null)
        {
            entity = new NisSystemConfig { CmpId = effectiveCmpId };
            _context.NisSystemConfigs.Add(entity);
        }

        entity.JobTypesRaw = JoinTags(dto.JobTypes);
        entity.TagsRaw = JoinTags(dto.Tags);
        entity.ImplementChecklistRaw = JoinTags(dto.ImplementChecklist);
        entity.MaChecklistRaw = JoinTags(dto.MaChecklist);
        entity.PmChecklistRaw = JoinTags(dto.PmChecklist);
        entity.ChecklistByTicketTypeJson = SerializeChecklistMap(dto.ChecklistByTicketType);
        entity.ChecklistByCustomerJson = SerializeCustomerChecklistMap(dto.ChecklistByCustomer);
        entity.EmailTemplatesJson = SerializeJson(dto.EmailTemplates);
        entity.EmailSignatureJson = SerializeJson(dto.EmailSignature);
        // null-preserve: client เก่าที่ PUT โดยไม่มี field นี้ต้องไม่ลบค่าที่ tenant บันทึกไว้
        if (dto.ServiceConditionOptions != null)
            entity.ServiceConditionOptionsJson = SerializeJson(dto.ServiceConditionOptions);
        entity.SlaOptionsRaw = JoinTags(dto.SlaOptions);
        entity.WarningDaysService = dto.WarningDays.Service;
        entity.WarningDaysProduct = dto.WarningDays.Product;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(MapSystemConfig(entity));
    }

    // ── System Config helpers ────────────────────────────────────────────────
    // Checklist items are pipe-delimited (|). Items themselves should not contain |.

    private static NisSystemConfigResponseDto MapSystemConfig(NisSystemConfig e) => new()
    {
        JobTypes = SplitTags(e.JobTypesRaw),
        Tags = SplitTags(e.TagsRaw),
        ImplementChecklist = SplitTags(e.ImplementChecklistRaw),
        MaChecklist = SplitTags(e.MaChecklistRaw),
        PmChecklist = SplitTags(e.PmChecklistRaw),
        ChecklistByTicketType = ParseChecklistMap(e.ChecklistByTicketTypeJson, DefaultChecklistByTicketType()),
        ChecklistByCustomer = ParseCustomerChecklistMap(e.ChecklistByCustomerJson),
        EmailTemplates = ParseJson(e.EmailTemplatesJson, DefaultEmailTemplates()),
        EmailSignature = ParseJson(e.EmailSignatureJson, new NisEmailSignatureDto()),
        ServiceConditionOptions = ParseJson(e.ServiceConditionOptionsJson, DefaultServiceConditionOptions()),
        SlaOptions = SplitTags(e.SlaOptionsRaw),
        WarningDays = new NisWarningDaysDto
        {
            Service = e.WarningDaysService,
            Product = e.WarningDaysProduct,
        },
    };

    // ── Checklist map (ticket type / customer) helpers ────────────────────────
    // เก็บเป็น JSON บน NisSystemConfig; ค่าว่าง/พังคืน fallback (ไม่ throw)

    private static Dictionary<string, List<string>> ParseChecklistMap(
        string? json, Dictionary<string, List<string>> fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private static Dictionary<string, Dictionary<string, List<string>>> ParseCustomerChecklistMap(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    private static string? SerializeChecklistMap(Dictionary<string, List<string>> map)
        => map.Count == 0 ? null : JsonSerializer.Serialize(map);

    private static string? SerializeCustomerChecklistMap(Dictionary<string, Dictionary<string, List<string>>> map)
        => map.Count == 0 ? null : JsonSerializer.Serialize(map);

    private static NisSystemConfigResponseDto BuildDefaultConfig() => new()
    {
        JobTypes = ["Runrate", "Implement", "MA-Device", "MA-Fortigate", "MA-Software", "MA-Network"],
        Tags = ["Firewall", "Network", "WiFi", "Server", "CCTV", "Access Control", "PC&Notebook", "Peripheral", "Software", "Cable", "Windows Server", "VMware", "HyperV"],
        ImplementChecklist =
        [
            "ตรวจสอบรายการสินค้า / อุปกรณ์ครบถ้วน",
            "ดำเนินการ PreConfig อุปกรณ์ก่อนออกงาน",
            "ติดตั้ง Rack / ขึ้นแร็ค",
            "เดินสาย Fiber / UTP",
            "Config Network Address / VLAN",
            "Config ระบบ Firewall Policy",
            "ทดสอบการเชื่อมต่อ Internet / WAN",
            "ทดสอบ Internal Network",
            "จัดทำ Network Diagram ตาม AS-BUILT",
            "บันทึก IP / User / Password เข้าระบบ",
            "ส่งมอบงานและให้ลูกค้าเซ็นรับ",
        ],
        MaChecklist =
        [
            "ตรวจสอบ Log / Event ย้อนหลัง",
            "ตรวจสอบ CPU / Memory / Disk Usage",
            "Update Firmware / Signature ล่าสุด",
            "ตรวจสอบ HA Cluster / Failover",
            "Remote Backup Config",
            "ทดสอบ Failover System",
            "บันทึกผลการตรวจสอบลง Monthly Report",
        ],
        PmChecklist =
        [
            "ทำความสะอาดอุปกรณ์ใน Rack",
            "ตรวจสอบสถานะ LED / Fan",
            "ตรวจสอบ Cable / Fiber Connection",
            "ตรวจสอบ Power Supply / UPS",
            "ตรวจสอบอุณหภูมิห้อง Server Room",
            "ทดสอบ Backup / Restore",
            "จัดทำ PM Report",
        ],
        ChecklistByTicketType = DefaultChecklistByTicketType(),
        ChecklistByCustomer = new(),
        EmailTemplates = DefaultEmailTemplates(),
        EmailSignature = new NisEmailSignatureDto(),
        ServiceConditionOptions = DefaultServiceConditionOptions(),
        SlaOptions = ["8x5xNBD", "8x5", "24x7x4", "24x7xNBD"],
        WarningDays = new NisWarningDaysDto { Service = 60, Product = 30 },
    };

    /// ตัวเลือกเงื่อนไขงานเริ่มต้นของ wizard สร้างโครงการ — ค่าตรงกับที่เคย hardcode ในหน้า CRM
    private static NisServiceConditionOptionsDto DefaultServiceConditionOptions() => new()
    {
        ServiceYears = ["1", "2", "3"],
        OnsitePerYearImplement = ["0", "2", "4", "6", "12"],
        OnsitePerYearMa = ["0", "2", "3", "4", "12"],
        PmPerYearImplement = ["0", "2", "4", "12"],
        PmPerYearMa = ["0", "2", "4", "12"],
        RemoteBackupImplement = ["0", "2", "4", "12"],
        RemoteBackupMa = ["0", "4", "12"],
        MonthlyReport = ["0", "4", "12"],
        ServiceReplacement =
        [
            new NisOptionItemDto { Value = "company", Label = "จากคลังบริษัทฯ" },
            new NisOptionItemDto { Value = "vendor", Label = "จาก Vendor" },
            new NisOptionItemDto { Value = "both", Label = "ทั้งสองแบบ (เลือกตามรายการสินค้า)" },
        ],
        DeliveryType =
        [
            new NisOptionItemDto { Value = "no_install", Label = "จัดส่ง โดยไม่ต้องติดตั้ง" },
            new NisOptionItemDto { Value = "preconfig", Label = "PreConfig ก่อนจัดส่ง" },
            new NisOptionItemDto { Value = "onsite_install", Label = "PreConfig จัดส่ง พร้อมติดตั้งหน้างาน" },
            new NisOptionItemDto { Value = "remote_config", Label = "จัดส่งสินค้า และ Remote Config" },
            new NisOptionItemDto { Value = "full_install", Label = "จัดส่งสินค้าและติดตั้ง" },
        ],
        DeliveryBy =
        [
            new NisOptionItemDto { Value = "nis_team", Label = "NIS Team (Auto Ticket → Service Manager)" },
            new NisOptionItemDto { Value = "messenger", Label = "Messenger (Auto Ticket → จัดซื้อ)" },
            new NisOptionItemDto { Value = "postal", Label = "ไปรษณีย์ (Auto Ticket → จัดซื้อ)" },
        ],
        Defaults = new NisServiceCondDefaultsDto(),
    };

    // ── Email template helpers ───────────────────────────────────────────────
    // เก็บเป็น JSON บน NisSystemConfig; ค่าว่าง/พังคืน fallback (ไม่ throw)

    private static string SerializeJson<T>(T value)
    {
        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch (NotSupportedException)
        {
            return string.Empty;
        }
    }

    private static T ParseJson<T>(string? json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    /// template อีเมลเริ่มต้น — id ต้องตรงกับที่ฝั่ง CRM อ้างถึง ("close-job" ใช้ตอนส่งปิดงาน)
    private static List<NisEmailTemplateDto> DefaultEmailTemplates() =>
    [
        new NisEmailTemplateDto
        {
            Id = "close-job",
            Name = "ส่งปิดงานให้ลูกค้า (Service Report)",
            // หัวเรื่อง = ชื่องานของตั๋ว + ลูกค้า (เช่น "Onsite MA รอบที่ 2 - บริษัท ก")
            Subject = "[PROJECT] - [COMPANY]",
            Body = "<p>เรียน คุณ[CONTACT]</p>"
                + "<p>บริษัทฯ ขอส่งใบรายงานการให้บริการ (Service Report) สำหรับงานที่ดำเนินการเสร็จสิ้นแล้ว ดังนี้</p>"
                + "<p>โครงการ / งาน: [PROJECT]<br/>"
                + "ลูกค้า: [COMPANY]<br/>วันที่ปฏิบัติงาน: [DATE]<br/>ช่างผู้ปฏิบัติงาน: [ENGINEER]</p>"
                + "<p>รายละเอียดงาน: [SERVICE_DETAIL]</p>"
                + "<p>รบกวนตรวจสอบและยืนยันการปิดงาน หากมีข้อสงสัยเพิ่มเติมติดต่อกลับได้ตามเบอร์ด้านล่างครับ</p>"
                + "<p>ขอบคุณครับ</p>",
            Enabled = true,
        },
        new NisEmailTemplateDto
        {
            Id = "quotation",
            Name = "ส่งใบเสนอราคา",
            Subject = "ใบเสนอราคา [QT_NUMBER] - [COMPANY]",
            Body = "<p>เรียน คุณ[CONTACT]</p>"
                + "<p>บริษัทฯ ขอส่งใบเสนอราคาเลขที่ <strong>[QT_NUMBER]</strong> มาเพื่อพิจารณา</p>"
                + "<p>ขอบคุณครับ</p>",
            Enabled = true,
        },
        new NisEmailTemplateDto
        {
            Id = "ma-renewal",
            Name = "แจ้งเตือนต่ออายุ MA",
            Subject = "แจ้งเตือน: สัญญา MA ใกล้หมดอายุ - [COMPANY]",
            Body = "<p>เรียน คุณ[CONTACT]</p>"
                + "<p>สัญญาบริการ (MA) ของ [COMPANY] จะครบกำหนดในวันที่ [DATE]</p>"
                + "<p>หากประสงค์ต่ออายุ กรุณาแจ้งกลับเพื่อจัดทำใบเสนอราคาครับ</p>",
            Enabled = true,
        },
        new NisEmailTemplateDto
        {
            Id = "customer-accept",
            Name = "ลูกค้าเซ็นรับงาน",
            Subject = "ยืนยันการรับงาน - [PROJECT] - [COMPANY]",
            Body = "<p>เรียน คุณ[CONTACT]</p>"
                + "<p>ขอขอบคุณที่ลงนามรับงาน <strong>[PROJECT]</strong> เมื่อวันที่ [DATE]</p>",
            Enabled = true,
        },
    ];

    /// checklist มาตรฐานเริ่มต้นตามประเภท ticket — ใช้เป็น seed และ fallback
    private static Dictionary<string, List<string>> DefaultChecklistByTicketType() => new()
    {
        ["Install"] =
        [
            "ตรวจสอบรายการสินค้า / อุปกรณ์ครบถ้วน",
            "ดำเนินการ PreConfig อุปกรณ์ก่อนออกงาน",
            "ติดตั้ง Rack / ขึ้นแร็ค",
            "เดินสาย Fiber / UTP",
            "Config Network Address / VLAN",
            "Config ระบบ Firewall Policy",
            "ทดสอบการเชื่อมต่อ Internet / WAN",
            "ทดสอบ Internal Network",
            "จัดทำ Network Diagram ตาม AS-BUILT",
            "บันทึก IP / User / Password เข้าระบบ",
            "ส่งมอบงานและให้ลูกค้าเซ็นรับ",
        ],
        ["PM"] =
        [
            "ทำความสะอาดอุปกรณ์ใน Rack",
            "ตรวจสอบสถานะ LED / Fan",
            "ตรวจสอบ Cable / Fiber Connection",
            "ตรวจสอบ Power Supply / UPS",
            "ตรวจสอบอุณหภูมิห้อง Server Room",
            "ทดสอบ Backup / Restore",
            "จัดทำ PM Report",
        ],
        ["MA Onsite"] =
        [
            "ตรวจสอบ Log / Event ย้อนหลัง",
            "ตรวจสอบ CPU / Memory / Disk Usage",
            "Update Firmware / Signature ล่าสุด",
            "ตรวจสอบ HA Cluster / Failover",
            "Remote Backup Config",
            "ทดสอบ Failover System",
            "บันทึกผลการตรวจสอบลง Monthly Report",
        ],
        ["Backup"] =
        [
            "ตรวจสอบพื้นที่จัดเก็บ Backup",
            "สำรอง Config อุปกรณ์ล่าสุด",
            "ทดสอบ Restore จากไฟล์ Backup",
            "บันทึกผลการ Backup ลงรายงาน",
        ],
        ["Report"] =
        [
            "รวบรวมข้อมูลการให้บริการประจำเดือน",
            "จัดทำ Monthly Report",
            "ส่ง Report ให้ลูกค้าตามกำหนด",
        ],
        ["Delivery"] =
        [
            "ตรวจสอบรายการสินค้าก่อนจัดส่ง",
            "จัดส่งสินค้าตามที่อยู่ลูกค้า",
            "ให้ลูกค้าเซ็นรับสินค้า",
        ],
    };

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        // Parse with InvariantCulture (Gregorian). Without it, a th-TH server culture
        // reads the ISO "yyyy-MM-dd" year as a Buddhist-era year (e.g. 2026 → 1483 CE),
        // which falls below the SQL Server minimum and gets discarded as null.
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) return null;
        if (result < new DateTime(1753, 1, 1)) return null; // SQL Server min
        return result;
    }

    // ── NIS Onsite Form ──────────────────────────────────────────────────────
    // Backed by ServiceTicket / ServiceTicketSubTask / ServiceTicketSubTaskAction —
    // the same tables ServiceTicketsController and the Staff Portal "My Tasks" board use.

    /// Resolves the onsite form's ticket id — which may be a subTaskId (from the "My
    /// Onsite Tasks" quick-pick) or a manually typed TicketNo — back to its ServiceTicket
    /// and (if resolvable) the relevant ServiceTicketSubTask for engineer/approval info.
    private async Task<(ServiceTicket? Ticket, ServiceTicketSubTask? SubTask)> ResolveOnsiteTicketAsync(string idOrSubTaskId)
    {
        var subTask = await _context.ServiceTicketSubTasks
            .Include(x => x.ServiceTicket)
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.SubTaskId == idOrSubTaskId);

        if (subTask?.ServiceTicket != null)
            return (subTask.ServiceTicket, subTask);

        var ticket = await _context.ServiceTickets
            .FirstOrDefaultAsync(t => t.TicketNo == idOrSubTaskId || t.TicketId == idOrSubTaskId);

        if (ticket == null)
            return (null, null);

        var fallbackSubTask = await _context.ServiceTicketSubTasks
            .Include(x => x.Assignments)
            .Where(x => x.TicketId == ticket.TicketId)
            .OrderBy(x => x.Seq)
            .FirstOrDefaultAsync();

        return (ticket, fallbackSubTask);
    }

    /// Install | MA | PM — derived from JobType + the existing MaintenancePMService flag
    /// (there's no separate "PM" JobType in ServiceTicket; PM is a maintenance sub-flag).
    private static string DeriveOnsiteTicketType(ServiceTicket ticket) =>
        string.Equals(ticket.JobType, "implement", StringComparison.OrdinalIgnoreCase)
            ? "Install"
            : ticket.MaintenancePMService ? "PM" : "MA";

    // ── NIS onsite (NisTicket-backed) helpers ────────────────────────────────
    // The onsite picker now shares the Staff Portal data source, so it sends a
    // NisTicket code/id. These resolve + persist against NisTicket / NisOnsiteReport
    // instead of the ServiceTicket tables above.

    private async Task<NisTicket?> ResolveNisOnsiteTicketAsync(string idOrCode)
    {
        if (string.IsNullOrWhiteSpace(idOrCode)) return null;
        return await _context.NisTickets
            .FirstOrDefaultAsync(t => t.TicketId == idOrCode || t.TicketCode == idOrCode);
    }

    /// Install | MA | PM — NisTicket has no "PM" JobType concept beyond Type.
    private static string MapNisOnsiteType(string? type) =>
        type == "Install" ? "Install" : type == "PM" ? "PM" : "MA";

    /// <summary>
    /// Gate ปิดงาน PM — คืนรายการ blocker (ว่าง = ผ่าน). Parity กับ computeRackInspectionBlockers
    /// ใน CRM nis-onsite-flow.ts / RN utils/rackInspection.ts:
    /// - ไม่มี item ไหนมี Devices (client เก่า / ไม่มีทะเบียน) → กฎเดิม ต้องมีอย่างน้อย 1 ตู้
    /// - มี Devices → ทุกตู้ต้องมีรูปก่อน/หลัง, ทุกอุปกรณ์ต้องถูกตรวจ (normal|abnormal),
    ///   abnormal ต้องระบุอาการ
    /// </summary>
    private static List<string> ValidatePmRackInspection(List<NisOnsitePmItemDto>? items)
    {
        var blockers = new List<string>();
        var racks = items ?? new List<NisOnsitePmItemDto>();

        if (!racks.Any(r => r.Devices != null))
        {
            if (racks.Count == 0)
                blockers.Add("งาน PM ต้องบันทึกอย่างน้อย 1 ตู้ Rack (พร้อมรูปก่อน/หลัง)");
            return blockers;
        }

        foreach (var rack in racks.Where(r => r.Devices != null))
        {
            var rackName = string.IsNullOrWhiteSpace(rack.Name) ? (rack.RackKey ?? "ตู้ Rack") : rack.Name;

            // รูป local-only (file:// ฯลฯ) = ยังไม่ได้อัปโหลด → นับเป็นขาดรูป
            if (IsMissingPhoto(rack.BeforePhoto))
                blockers.Add($"{rackName}: ยังขาดรูปก่อนทำ");
            if (IsMissingPhoto(rack.AfterPhoto))
                blockers.Add($"{rackName}: ยังขาดรูปหลังทำ");

            var unchecked_ = rack.Devices!.Count(d => d.Status != "normal" && d.Status != "abnormal");
            if (unchecked_ > 0)
                blockers.Add($"{rackName}: ยังไม่ได้ตรวจ {unchecked_} รายการ");

            foreach (var device in rack.Devices!.Where(d => d.Status == "abnormal" && string.IsNullOrWhiteSpace(d.Symptom)))
            {
                var uLabel = device.UPosition.HasValue ? $"U{device.UPosition.Value}" : "U-";
                blockers.Add($"{rackName} {uLabel} {device.DeviceName}: ยังไม่ระบุอาการ");
            }
        }

        return blockers;
    }

    private static NisOnsiteReport BuildNisOnsiteReport(
        NisTicket ticket, NisOnsiteReportBaseDto dto, string cmpId, string status) => new()
        {
            ReportId = Guid.NewGuid().ToString(),
            NisTicketId = ticket.TicketId,
            TicketCode = ticket.TicketCode,
            SrNumber = dto.SrNumber,
            CmpId = cmpId,
            Engineer = dto.User,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            CheckInLatitude = (decimal?)dto.CheckInLat,
            CheckInLongitude = (decimal?)dto.CheckInLng,
            CheckOutLatitude = (decimal?)dto.CheckOutLat,
            CheckOutLongitude = (decimal?)dto.CheckOutLng,
            WorkDetail = dto.WorkDetail,
            IssueDetail = dto.IssueDetail,
            ChecklistJson = JsonSerializer.Serialize(dto.Checklist),
            PmItemsJson = JsonSerializer.Serialize(dto.PmItems),
            DamagedProductJson = SerializeNisDamagedProduct(dto),
            SupportCasesJson = JsonSerializer.Serialize(dto.SupportCases),
            PhotosJson = JsonSerializer.Serialize(dto.Photos),
            BeforePhoto = dto.BeforePhoto,
            DuringPhoto = dto.DuringPhoto,
            AfterPhoto = dto.AfterPhoto,
            SignatureImageBase64 = dto.SignatureImg,
            SkipSignature = dto.SkipSignature,
            Status = status,
            CreatedDate = DateTime.Now,
        };

    /// DamagedProductJson ของใบรายงาน NIS — ของเสียรายการแรก (shape เดิม) + ครบทุกรายการใน Reports
    /// ไม่มีทั้งคู่ = null (เหมือนเดิม) · มีแต่รายการเต็ม → Checked=false (ไม่กระทบ logic ที่อ่าน Checked)
    private static string? SerializeNisDamagedProduct(NisOnsiteReportBaseDto dto)
    {
        var reports = dto.DamageReports is { Count: > 0 } ? dto.DamageReports : null;
        if (dto.DamagedProduct == null && reports == null) return null;
        var damaged = dto.DamagedProduct ?? new NisOnsiteDamagedProductDto { Checked = false };
        damaged.Reports = reports;
        return JsonSerializer.Serialize(damaged);
    }

    /// PDF ที่แนบมากับ request-close ได้ไม่เกิน 7.5 MB หลัง decode (contract §4)
    private const long RequestClosePdfMaxBytes = 7_864_320;

    /// คัดลอกเนื้อหาใบรายงานจาก src (ที่ build จาก payload ใหม่) ลงแถวเดิม — คง ReportId / SrNumber / CreatedDate ของแถวเดิม
    /// (แถวเดิมไม่มีเลข → ใช้เลขจาก payload) · ใช้ตอน request-close นำแถว reserved/rejected กลับมาใช้
    private static void CopyOnsiteReportContent(NisOnsiteReport target, NisOnsiteReport src)
    {
        if (string.IsNullOrWhiteSpace(target.SrNumber)) target.SrNumber = src.SrNumber;
        target.TicketCode = src.TicketCode;
        target.CmpId = src.CmpId;
        target.Engineer = src.Engineer;
        target.CheckInTime = src.CheckInTime;
        target.CheckOutTime = src.CheckOutTime;
        target.CheckInLatitude = src.CheckInLatitude;
        target.CheckInLongitude = src.CheckInLongitude;
        target.CheckOutLatitude = src.CheckOutLatitude;
        target.CheckOutLongitude = src.CheckOutLongitude;
        target.WorkDetail = src.WorkDetail;
        target.IssueDetail = src.IssueDetail;
        target.ChecklistJson = src.ChecklistJson;
        target.PmItemsJson = src.PmItemsJson;
        target.DamagedProductJson = src.DamagedProductJson;
        target.SupportCasesJson = src.SupportCasesJson;
        target.PhotosJson = src.PhotosJson;
        target.BeforePhoto = src.BeforePhoto;
        target.DuringPhoto = src.DuringPhoto;
        target.AfterPhoto = src.AfterPhoto;
        target.SignatureImageBase64 = src.SignatureImageBase64;
        target.SkipSignature = src.SkipSignature;
        target.Status = src.Status;
    }

    // ── GET api/nis/onsite/reports ───────────────────────────────────────────
    /// <summary>
    /// รายการ Service Report ที่ปิดงานแล้ว (NisOnsiteReport) join NisTicket/NisProject
    /// เพื่อได้ title/customer/type · manager = ทุกช่าง (ไม่ส่ง user) · staff = ส่ง user กรอง Engineer
    /// </summary>
    [HttpGet("onsite/reports")]
    public async Task<ActionResult<IEnumerable<NisServiceReportDto>>> GetOnsiteReports(
        [FromQuery] string? cmpid,
        [FromQuery] string? user)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        // เฉพาะใบที่ปิดจริง (submitted) — ตัดแถว pending_approval ที่ค้างจากรอบ request-close ที่ถูก
        // SM ตีกลับ (close-reject ไม่ลบ/ปรับสถานะแถวเดิม) ไม่งั้นนับซ้ำ/แสดงใบที่ยังไม่ปิดจริง
        var query = _context.NisOnsiteReports.AsNoTracking()
            .Where(r => r.CmpId == cmpid && r.Status == "submitted");
        if (!string.IsNullOrWhiteSpace(user))
            query = query.Where(r => r.Engineer == user);

        var reports = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
        if (reports.Count == 0) return Ok(Array.Empty<NisServiceReportDto>());

        // enrich: NisTicketId → NisTicket (Title/Type/ProjectId) → NisProject (Customer/Location)
        var ticketIds = reports.Select(r => r.NisTicketId).Distinct().ToList();
        var tickets = await _context.NisTickets.AsNoTracking()
            .Where(t => ticketIds.Contains(t.TicketId)).ToListAsync();
        var ticketMap = tickets.ToDictionary(t => t.TicketId);

        var projectIds = tickets.Select(t => t.ProjectId).Distinct().ToList();
        var projects = await _context.NisProjects.AsNoTracking()
            .Where(p => projectIds.Contains(p.ProjectId)).ToListAsync();
        var projectMap = projects.ToDictionary(p => p.ProjectId);

        var result = reports.Select(r =>
        {
            ticketMap.TryGetValue(r.NisTicketId, out var tk);
            NisProject? pj = null;
            if (tk != null) projectMap.TryGetValue(tk.ProjectId, out pj);

            var checklist = new List<NisServiceReportChecklistDto>();
            if (!string.IsNullOrWhiteSpace(r.ChecklistJson))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<NisOnsiteChecklistItemDto>>(r.ChecklistJson!);
                    if (items != null)
                        checklist = items.Select(c => new NisServiceReportChecklistDto { Label = c.Label, Done = c.Checked }).ToList();
                }
                catch { /* checklist ผิดรูป → ปล่อยว่าง */ }
            }

            var photos = new List<string>();
            if (!string.IsNullOrWhiteSpace(r.PhotosJson))
            {
                try { photos = JsonSerializer.Deserialize<List<string>>(r.PhotosJson!) ?? new(); }
                catch { /* photos ผิดรูป → ปล่อยว่าง */ }
            }

            var pmItems = new List<NisOnsitePmItemDto>();
            if (!string.IsNullOrWhiteSpace(r.PmItemsJson))
            {
                try { pmItems = JsonSerializer.Deserialize<List<NisOnsitePmItemDto>>(r.PmItemsJson!) ?? new(); }
                catch { /* pmItems ผิดรูป → ปล่อยว่าง */ }
            }

            NisOnsiteDamagedProductDto? damagedProduct = null;
            if (!string.IsNullOrWhiteSpace(r.DamagedProductJson))
            {
                try { damagedProduct = JsonSerializer.Deserialize<NisOnsiteDamagedProductDto>(r.DamagedProductJson!); }
                catch { /* damagedProduct ผิดรูป → ปล่อยว่าง */ }
            }

            // ของเสียครบทุกรายการซ้อนอยู่ใน DamagedProductJson.Reports — แยกออกเป็น field ของตัวเอง (ไม่ส่งซ้ำ)
            var damageReports = damagedProduct?.Reports ?? new();
            if (damagedProduct != null) damagedProduct.Reports = null;
            // ใบที่มีแต่รายการเต็ม (Checked=false) ไม่ต้องส่ง damagedProduct เปล่า ๆ
            if (damagedProduct is { Checked: false }) damagedProduct = null;

            var supportCases = ParseJson<List<NisOnsiteSupportCaseDto>?>(r.SupportCasesJson, null) ?? new();
            // ผู้รับมอบงาน: ชื่อที่ช่างส่งตอนปิดงาน (freeze) ก่อน · ใบเก่าไม่มี → ผู้ติดต่อของโครงการ
            var closeEmail = ParseJson<NisOnsiteStoredCloseEmailDto?>(r.CloseEmailJson, null);
            var contactName = string.IsNullOrWhiteSpace(closeEmail?.ContactName) ? pj?.ContactName : closeEmail!.ContactName;

            var ticketType = tk?.Type ?? string.Empty;
            return new NisServiceReportDto
            {
                Id = r.SrNumber ?? r.ReportId,
                SrNumber = r.SrNumber ?? string.Empty,
                TicketId = r.NisTicketId,
                TicketCode = r.TicketCode,
                TicketTitle = tk?.Title ?? string.Empty,
                Customer = pj?.Customer ?? string.Empty,
                Location = pj?.Location,
                Engineer = r.Engineer ?? string.Empty,
                Type = ticketType,
                TicketType = ticketType,
                CheckInTime = r.CheckInTime,
                CheckOutTime = r.CheckOutTime,
                WorkNote = r.WorkDetail,
                WorkDetail = r.WorkDetail,
                Summary = r.WorkDetail ?? string.Empty,
                Checklist = checklist,
                SignatureImg = r.SignatureImageBase64,
                SkipSignature = r.SkipSignature,
                Photos = photos,
                PmItems = pmItems,
                DamagedProduct = damagedProduct,
                DamageReports = damageReports,
                BeforePhoto = r.BeforePhoto,
                DuringPhoto = r.DuringPhoto,
                AfterPhoto = r.AfterPhoto,
                Date = r.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Status = "Closed",
                ReportId = r.ReportId,
                HasPdf = !string.IsNullOrWhiteSpace(r.ReportPdfPath),
                SupportCases = supportCases,
                ContactName = contactName,
                ContactPhone = pj?.ContactPhone,
            };
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// ไฟล์ PDF ใบรายงานฉบับที่ส่งลูกค้าจริงตอนปิดงาน (เก็บไว้ตอน onsite/submit หรือ request-close)
    /// ใช้เปิดดูรายงานย้อนหลังให้ตรงกับที่ส่งทุกตัวอักษร · ไม่มีไฟล์ = 404 (client สร้างเอกสารจากข้อมูลแทน)
    /// [Authorize] เหมือน tickets/{id}/photos — เอกสารมีลายเซ็น/ข้อมูลลูกค้า ห้ามเปิดแบบไม่มี JWT
    /// </summary>
    [Authorize]
    [HttpGet("onsite/reports/{reportId}/pdf")]
    public async Task<IActionResult> GetOnsiteReportPdf(string reportId, [FromQuery] string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(reportId) || reportId.Length > 50)
            return BadRequest(new { message = "invalid reportId" });

        var report = await _context.NisOnsiteReports.AsNoTracking()
            .Where(r => r.ReportId == reportId && r.CmpId == cmpid && r.Status == "submitted")
            .Select(r => new { r.ReportPdfPath, r.SrNumber, r.TicketCode })
            .FirstOrDefaultAsync();
        if (report == null)
            return NotFound(new { message = "report not found" });

        var bytes = await _pdfStorage.TryReadAsync(report.ReportPdfPath);
        if (bytes == null)
            return NotFound(new { message = "report pdf not stored" });

        var name = string.IsNullOrWhiteSpace(report.SrNumber) ? report.TicketCode ?? reportId : report.SrNumber;
        var fileName = System.Text.RegularExpressions.Regex.Replace($"Service-Report-{name}.pdf", "[^A-Za-z0-9._-]", "_");
        return File(bytes, "application/pdf", fileName);
    }

    // ── GET api/nis/onsite/tickets/{id} ──────────────────────────────────────

    [HttpGet("onsite/tickets/{id}")]
    public async Task<ActionResult<NisOnsiteTicketResponseDto>> GetOnsiteTicket(
        string id,
        [FromQuery] string? cmpid,
        [FromQuery] string? user)
    {
        // NIS-first: the picker sends a NisTicket code/id (shared Staff Portal data).
        var nisTicket = await ResolveNisOnsiteTicketAsync(id);
        if (nisTicket != null)
        {
            var project = await _context.NisProjects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == nisTicket.ProjectId);

            var nisTicketType = MapNisOnsiteType(nisTicket.Type);

            // เหตุผลที่ SM ตีกลับคำขอปิดงานล่าสุด — แสดงเมื่อตั๋วกลับมาทำต่อ (แถว report ล่าสุดยังเป็น rejected)
            string? nisRejectionReason = null;
            if (nisTicket.Status == "In Progress")
            {
                var lastReport = await _context.NisOnsiteReports
                    .AsNoTracking()
                    .Where(r => r.NisTicketId == nisTicket.TicketId
                        && (r.Status == "reserved" || r.Status == "pending_approval" || r.Status == "rejected"))
                    .OrderByDescending(r => r.CreatedDate)
                    .Select(r => new { r.Status, r.RejectReason })
                    .FirstOrDefaultAsync();
                if (lastReport?.Status == "rejected") nisRejectionReason = lastReport.RejectReason;
            }

            // ทะเบียนอุปกรณ์ในตู้ Rack — เฉพาะงาน PM: โปรเจคเลือก id ไว้ → เอาเฉพาะชุดนั้น
            // ไม่ได้เลือก → ทั้งหมดของลูกค้ารายนี้ (client จัดกลุ่มตาม RackName เอง)
            List<NisOnsiteEquipmentDto>? equipment = null;
            if (nisTicketType == "PM" && project != null && !string.IsNullOrWhiteSpace(project.CustomerCode))
            {
                var equipCmpId = string.IsNullOrWhiteSpace(nisTicket.CmpId) ? project.CmpId : nisTicket.CmpId;
                var selectedIds = (ParseJson<List<string>?>(project.EquipmentIdsJson, null) ?? new())
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                var equipQuery = _context.NisEquipments
                    .AsNoTracking()
                    .Where(e => e.CmpId == equipCmpId && e.CustomerCode == project.CustomerCode);
                if (selectedIds.Count > 0)
                    equipQuery = equipQuery.Where(e => selectedIds.Contains(e.Id));

                var equipRows = await equipQuery.ToListAsync();
                equipment = equipRows
                    .OrderBy(e => e.RackName)
                    .ThenBy(e => e.UPosition == null)          // null U ไปท้ายตู้
                    .ThenBy(e => e.UPosition)
                    .ThenBy(e => e.DeviceName)
                    .Select(e => new NisOnsiteEquipmentDto
                    {
                        Id = e.Id.ToString(),
                        RackName = e.RackName,
                        RackLocation = e.RackLocation,
                        UPosition = e.UPosition,
                        DeviceName = e.DeviceName,
                        SerialNo = e.SerialNo,
                        Model = e.Model,
                        Brand = e.Brand,
                        Note = e.Note,
                    })
                    .ToList();
            }

            return Ok(new NisOnsiteTicketResponseDto
            {
                Id = nisTicket.TicketCode ?? nisTicket.TicketId,
                Title = nisTicket.Title,
                Customer = project?.Customer ?? string.Empty,
                CustomerCode = project?.CustomerCode,
                Location = project?.Location ?? string.Empty,
                TicketType = nisTicketType,
                ContactName = project?.ContactName ?? string.Empty,
                ContactPhone = project?.ContactPhone ?? string.Empty,
                ContactEmail = project?.ContactEmail,
                SalesName = project?.SalesPMName,
                SalesNickname = project?.SalesPMNickname,
                SalesPhone = project?.SalesPMPhone,
                SalesEmail = project?.SalesPMEmail,
                SalesRole = project?.SalesPMRole,
                EngineerName = string.IsNullOrWhiteSpace(project?.EngineerName) ? nisTicket.Assignee : project!.EngineerName,
                EngineerNick = project?.EngineerNickname ?? string.Empty,
                EngineerPhone = project?.EngineerPhone,
                Status = nisTicket.Status,
                RejectionReason = nisRejectionReason,
                SkipSignature = false,
                RequireCloseApproval = false,
                Accepted = true,
                CheckInTime = nisTicket.CheckInTime.HasValue ? FormatDateTime(nisTicket.CheckInTime) : null,
                CheckInLat = nisTicket.CheckInLat,
                CheckInLng = nisTicket.CheckInLng,
                CheckInLocation = nisTicket.CheckInLocation,
                CheckInBy = nisTicket.CheckInBy,
                CheckOutTime = nisTicket.CheckOutTime.HasValue ? FormatDateTime(nisTicket.CheckOutTime) : null,
                CheckOutLat = nisTicket.CheckOutLat,
                CheckOutLng = nisTicket.CheckOutLng,
                CheckOutLocation = nisTicket.CheckOutLocation,
                CheckOutBy = nisTicket.CheckOutBy,
                Equipment = equipment,
            });
        }

        var (ticket, subTask) = await ResolveOnsiteTicketAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Onsite ticket '{id}' not found" });

        if (!string.IsNullOrWhiteSpace(cmpid) && ticket.CmpId != cmpid)
            return Forbid();

        var customer = await _context.customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerCode == ticket.CustomerCode && c.CmpId == ticket.CmpId);

        // Older ServiceTicket rows were created before CustomerCode existed on the table,
        // so it can be blank even though the customer is known by name — fall back to a
        // name-based lookup so the frontend still gets a real customerCode (needed to
        // auto-create the linked MA Helpdesk case / match the LINE contact list).
        if (customer == null && !string.IsNullOrWhiteSpace(ticket.CustomerName))
        {
            customer = await _context.customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerName == ticket.CustomerName && c.CmpId == ticket.CmpId);
        }

        var resolvedCustomerCode = customer?.CustomerCode;

        var activeAssignment = subTask?.Assignments.FirstOrDefault(a => a.IsActive);
        var isWaitingApproval = subTask?.StateSendApprove == "1" && subTask?.StateApprove != "1";
        var isRejected = subTask?.StateApprove == "2";

        var response = new NisOnsiteTicketResponseDto
        {
            Id = ticket.TicketNo ?? ticket.TicketId,
            Title = !string.IsNullOrWhiteSpace(subTask?.Title) ? subTask!.Title : (ticket.AdditionalDetails ?? ticket.TicketNo ?? ticket.TicketId),
            Customer = ticket.CustomerName,
            CustomerCode = resolvedCustomerCode,
            Location = customer?.CustomerAddress ?? string.Empty,
            TicketType = DeriveOnsiteTicketType(ticket),
            ContactName = customer?.ContactName ?? string.Empty,
            ContactPhone = customer?.ContactPhone ?? string.Empty,
            ContactEmail = customer?.ContactEmail,
            EngineerName = activeAssignment?.AssignUserName ?? "-",
            Status = isWaitingApproval ? "Waiting Close Approval" : ticket.Status,
            RejectionReason = isRejected ? subTask?.RejectReason : null,
            SkipSignature = ticket.SkipSignature,
            RequireCloseApproval = ticket.RequireCloseApproval,
            Accepted = true,
        };

        return Ok(response);
    }

    // ── POST api/nis/onsite/sr-number ────────────────────────────────────────

    [HttpPost("onsite/sr-number")]
    public async Task<ActionResult> GenerateOnsiteSrNumber([FromBody] NisOnsiteSrNumberRequestDto dto)
    {
        return Ok(new { srNumber = await NextSrNumberAsync(dto.CmpId ?? string.Empty) });
    }

    /// <summary>
    /// เลข Service Report ถัดไปของบริษัท (prefix SR-yyyyMM-) — นับข้ามทั้งตาราง ServiceTicket
    /// และ NIS report ให้เลขไม่ชนกันไม่ว่ามาจาก flow ปิดงานทางไหน. ยัง count+1 (ไม่ reserve เอง)
    /// ผู้เรียกต้อง persist report ที่ถือเลขนี้ทันทีในทรานแซกชันเดียวกันเพื่อกันเลขซ้ำรอบถัดไป.
    /// </summary>
    private async Task<string> NextSrNumberAsync(string cmpId)
    {
        var prefix = $"SR-{DateTime.Now:yyyyMM}-";

        var svcCount = await _context.ServiceTicketSubTaskActions
            .Where(a => a.CmpId == cmpId && a.SrNumber != null && a.SrNumber.StartsWith(prefix))
            .CountAsync();

        var nisCount = await _context.NisOnsiteReports
            .Where(r => r.CmpId == cmpId && r.SrNumber != null && r.SrNumber.StartsWith(prefix))
            .CountAsync();

        return $"{prefix}{(svcCount + nisCount + 1):D4}";
    }

    // ── POST api/nis/onsite/{id}/reserve-sr ──────────────────────────────────

    /// <summary>
    /// จองเลข SR ตอนช่างกด "ขออนุมัติปิดงาน" — client ต้องได้เลขก่อน render PDF แล้วค่อย POST request-close
    /// - มีแถว NisOnsiteReport ของตั๋วนี้ (reserved / pending_approval / rejected) ที่มีเลขแล้ว → คืนเลขเดิม (idempotent)
    /// - ไม่มี → สร้างแถว Status = reserved พร้อมเลขใหม่ (NextSrNumberAsync) แล้ว save ทันที (กันเลขซ้ำรอบถัดไป)
    /// - แถว reserved/rejected ไม่โผล่ในรายการ Service Report (กรองเฉพาะ submitted)
    /// - {id} = TicketId หรือ TicketCode · cmpId มาจากตั๋วใน DB (body cmpId ไม่ตรง = 404)
    /// </summary>
    [HttpPost("onsite/{id}/reserve-sr")]
    public async Task<IActionResult> ReserveOnsiteSrNumber(string id, [FromBody] NisOnsiteReserveSrDto? dto)
    {
        var ticket = await ResolveNisOnsiteTicketAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (!string.IsNullOrWhiteSpace(dto?.CmpId) && !string.IsNullOrWhiteSpace(ticket.CmpId) && dto!.CmpId != ticket.CmpId)
            return NotFound(new { message = $"Ticket {id} not found" });

        if (ticket.Status is "Done" or "Closed")
            return Conflict(new { message = "Ticket already closed" });

        var existing = await _context.NisOnsiteReports
            .Where(r => r.NisTicketId == ticket.TicketId
                && (r.Status == "reserved" || r.Status == "pending_approval" || r.Status == "rejected")
                && r.SrNumber != null && r.SrNumber != "")
            .OrderByDescending(r => r.CreatedDate)
            .FirstOrDefaultAsync();
        if (existing != null)
            return Ok(new { srNumber = existing.SrNumber });

        var cmpId = string.IsNullOrWhiteSpace(ticket.CmpId) ? (dto?.CmpId ?? string.Empty) : ticket.CmpId;
        var engineer = TrimOrNull(dto?.User, 200) ?? ticket.Assignee;
        var report = new NisOnsiteReport
        {
            ReportId = Guid.NewGuid().ToString(),
            NisTicketId = ticket.TicketId,
            TicketCode = ticket.TicketCode,
            CmpId = cmpId,
            Engineer = engineer,
            SrNumber = await NextSrNumberAsync(cmpId),
            Status = "reserved",
            CreatedDate = DateTime.Now,
        };
        _context.NisOnsiteReports.Add(report);
        await _context.SaveChangesAsync();

        return Ok(new { srNumber = report.SrNumber });
    }

    // ── POST api/nis/onsite/submit ───────────────────────────────────────────
    // Persists the service report, closes the ticket + subtask, auto-creates a
    // replacement/sales ticket for a damaged product and a helpdesk case per support
    // case (mirrors CreateReplacementTicket / CreateHelpdeskCase), then emails the
    // customer best-effort (ticket close always proceeds even if email fails).

    [HttpPost("onsite/submit")]
    public async Task<IActionResult> SubmitOnsiteReport([FromBody] NisOnsiteSubmitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TicketId))
            return BadRequest(new { message = "ticketId is required" });

        // รูปที่ยังเป็น reference ในเครื่อง (file:// ฯลฯ) = ยังอัปโหลดไม่สำเร็จ → ห้ามบันทึก (server เปิดไม่ได้)
        var localPhotoField = FindLocalPhotoField(dto);
        if (localPhotoField != null)
            return BadRequest(new { message = "รูปยังอัปโหลดไม่สำเร็จ", field = localPhotoField });

        // NIS-first: report filed against a NisTicket → persist to NisOnsiteReport
        // and close the NisTicket. Damaged-product / support-case downstream tickets
        // are kept as report JSON (not auto-created for the NIS system).
        var nisTicket = await ResolveNisOnsiteTicketAsync(dto.TicketId);
        if (nisTicket != null)
        {
            var nisCmpId = dto.CmpId ?? nisTicket.CmpId ?? string.Empty;

            // Business rule ตรวจซ้ำที่ backend: งาน PM ต้องตรวจตู้ Rack ครบก่อนปิด (ไม่เชื่อ gate ฝั่ง client)
            if (MapNisOnsiteType(nisTicket.Type) == "PM")
            {
                var pmBlockers = ValidatePmRackInspection(dto.PmItems);
                if (pmBlockers.Count > 0)
                    return BadRequest(new { message = "PM rack inspection incomplete", blockers = pmBlockers });
            }

            var report = BuildNisOnsiteReport(nisTicket, dto, nisCmpId, "submitted");
            // เก็บข้อมูลอีเมลปิดงาน (ผู้รับมอบงาน ฯลฯ) แบบเดียวกับ request-close — ใบรายงานย้อนหลังแสดงชื่อตามที่ส่งจริง
            report.CloseEmailJson = SerializeJson(new NisOnsiteStoredCloseEmailDto
            {
                RecipientEmail = TrimOrNull(dto.RecipientEmail, 320),
                EmailSubject = TrimOrNull(dto.EmailSubject, 500),
                EmailMessage = TrimOrNull(dto.EmailMessage, 4000),
                ContactName = TrimOrNull(dto.ContactName, 200),
                SenderName = TrimOrNull(dto.SenderName, 200),
                SenderPosition = TrimOrNull(dto.SenderPosition, 200),
                SenderMobile = TrimOrNull(dto.SenderMobile, 50),
                ReportPdfFileName = TrimOrNull(dto.ReportPdfFileName, 200),
            });

            // Optional client-generated Service Report PDF: validate → persist blob (for
            // resend/audit) → set the reference columns → prepare the email attachment. A
            // malformed/oversize PDF is rejected *before* closing the ticket (return 400).
            EmailAttachment? pdfAttachment = null;
            if (!string.IsNullOrWhiteSpace(dto.ReportPdfBase64))
            {
                if (!_pdfStorage.TryDecode(dto.ReportPdfBase64, out var pdfBytes, out var pdfError))
                    return BadRequest(new { message = pdfError });

                var stored = await _pdfStorage.SaveAsync(nisCmpId, nisTicket.TicketId, dto.SrNumber, pdfBytes);
                report.ReportPdfPath = stored.RelativePath;
                report.ReportPdfSize = stored.Size;
                report.ReportPdfSha256 = stored.Sha256;
                pdfAttachment = new EmailAttachment(ReportPdfFileName(dto), pdfBytes, "application/pdf");
            }

            _context.NisOnsiteReports.Add(report);
            nisTicket.Status = "Closed";
            nisTicket.Pct = NIS_PCT_CLOSED;
            ApplyClosedDate(nisTicket);
            nisTicket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // contract "ticket changed" — ปิดงานตรง (ไม่ผ่านอนุมัติ)
            await NotifyTicketChangedAsync(nisTicket, "closed", dto.User);

            // อีเมลลูกค้า — helper เดียวกับ close-approve (feature flag _attachReportPdf อยู่ใน helper)
            var (nisEmailSent, nisEmailError, attachPdf) =
                await SendNisOnsiteCloseEmailAsync(nisTicket, nisCmpId, dto, pdfAttachment);

            return Ok(new
            {
                message = "Onsite service report submitted",
                ticketId = nisTicket.TicketId,
                srNumber = dto.SrNumber,
                status = nisTicket.Status,
                emailSent = nisEmailSent,
                emailError = nisEmailError,
                // Surface PDF outcome so the client never silently assumes the customer got the file.
                pdfStored = report.ReportPdfPath != null,
                pdfAttached = attachPdf,
            });
        }

        var (ticket, subTask) = await ResolveOnsiteTicketAsync(dto.TicketId);
        if (ticket == null)
            return NotFound(new { message = $"Onsite ticket '{dto.TicketId}' not found" });

        var cmpId = dto.CmpId ?? ticket.CmpId ?? string.Empty;
        var user = dto.User ?? string.Empty;

        _context.ServiceTicketSubTaskActions.Add(new ServiceTicketSubTaskAction
        {
            TaskActionId = Guid.NewGuid().ToString("N"),
            TicketId = ticket.TicketId,
            SubTaskId = subTask?.SubTaskId ?? string.Empty,
            CmpId = cmpId,
            Seq = 1,
            ActionDate = DateTime.Now,
            ActionDetails = "Onsite service report",
            ActionStatus = "submitted",
            SrNumber = dto.SrNumber,
            WorkDetail = dto.WorkDetail,
            IssueDetail = dto.IssueDetail,
            ChecklistItemsJson = JsonSerializer.Serialize(dto.Checklist),
            RackPhotosJson = JsonSerializer.Serialize(dto.PmItems),
            DamagedProductJson = dto.DamagedProduct != null ? JsonSerializer.Serialize(dto.DamagedProduct) : null,
            OthersItemsJson = JsonSerializer.Serialize(dto.SupportCases),
            WorkPhotosJson = JsonSerializer.Serialize(dto.Photos),
            SignatureImageBase64 = dto.SignatureImg,
            CheckInLatitude = (decimal?)dto.CheckInLat,
            CheckInLongitude = (decimal?)dto.CheckInLng,
            CheckOutLatitude = (decimal?)dto.CheckOutLat,
            CheckOutLongitude = (decimal?)dto.CheckOutLng,
            UpdatedAt = DateTime.Now,
        });

        ticket.Status = "Closed";
        ticket.UpdatedAt = DateTime.Now;

        if (subTask != null)
        {
            subTask.IsDone = true;
            subTask.DoneAt = DateTime.Now;
            subTask.DoneBy = user;
            subTask.Status = "completed";
            subTask.TaskStatus = "completed";
            subTask.ProgressPercent = 100;
            subTask.UpdatedAt = DateTime.Now;
        }

        if (dto.DamagedProduct?.Checked == true)
        {
            var isWarranty = string.Equals(dto.DamagedProduct.Warranty, "on", StringComparison.OrdinalIgnoreCase);
            var label = isWarranty ? "Replacement" : "Sales";
            var title = $"[{label}] {dto.DamagedProduct.Brand} {dto.DamagedProduct.Model} SN:{dto.DamagedProduct.Sn}";

            _context.ServiceTickets.Add(new ServiceTicket
            {
                TicketId = Guid.NewGuid().ToString("N"),
                JobType = isWarranty ? "replacement" : "sales",
                AdditionalDetails = $"{title}\nจากงาน Onsite Ticket {ticket.TicketNo ?? ticket.TicketId}",
                CustomerName = ticket.CustomerName,
                CustomerCode = ticket.CustomerCode,
                CmpId = cmpId,
                UpdUser = user,
                Priority = "minor",
                Status = "draft",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });
        }

        // NOTE: support cases are no longer auto-created here as ServiceTicket(JobType="helpdesk").
        // The onsite form now creates a real IServiceProblem + IActionProblem (serviceType="Onsite")
        // immediately when the engineer adds each case — dto.SupportCases is kept only as the
        // action's audit-trail JSON (OthersItemsJson above), not a creation source.

        await _context.SaveChangesAsync();

        var emailSent = false;
        string? emailError = null;
        if (!string.IsNullOrWhiteSpace(dto.RecipientEmail))
        {
            try
            {
                var (subject, body) = await BuildOnsiteCloseEmailAsync(
                    cmpId,
                    ticket.TicketNo ?? ticket.TicketId,
                    ticket.CustomerName,
                    subTask?.Title ?? ticket.AdditionalDetails,
                    subTask?.DoneBy ?? user,
                    dto,
                    DeriveOnsiteTicketType(ticket));
                emailSent = await SendOnsiteEmailAsync(cmpId, dto.RecipientEmail, subject, body);
            }
            catch (Exception ex)
            {
                emailError = ex.Message;
            }
        }

        return Ok(new
        {
            message = "Onsite service report submitted",
            ticketId = ticket.TicketId,
            srNumber = dto.SrNumber,
            status = ticket.Status,
            emailSent,
            emailError,
        });
    }

    /// <summary>
    /// ส่งอีเมลปิดงานของตั๋ว NIS ให้ลูกค้า — ใช้ร่วมกันระหว่าง POST onsite/submit (ปิดตรง) และ
    /// PUT tickets/{id}/close-approve (ปิดหลัง SM อนุมัติ ด้วยข้อมูล/PDF ที่เก็บไว้ตอน request-close)
    /// แนบ PDF ตาม feature flag NisOnsite:AttachReportPdf · ไม่มี RecipientEmail = ไม่ส่ง
    /// ไม่ throw — error คืนเป็นข้อความให้ response แสดง (ปิดงานต้องสำเร็จเสมอแม้อีเมลล้ม)
    /// </summary>
    /// <param name="nisTicket">ตั๋วที่ปิด</param>
    /// <param name="cmpId">รหัสบริษัท</param>
    /// <param name="dto">payload ปิดงาน (ฟิลด์อีเมล + ข้อมูลใบรายงานที่ใช้ใน template)</param>
    /// <param name="pdfAttachment">PDF ใบรายงาน (null = ไม่มี)</param>
    /// <returns>(ส่งสำเร็จ, ข้อความ error, แนบ PDF หรือไม่)</returns>
    private async Task<(bool Sent, string? Error, bool PdfAttached)> SendNisOnsiteCloseEmailAsync(
        NisTicket nisTicket,
        string cmpId,
        NisOnsiteSubmitDto dto,
        EmailAttachment? pdfAttachment)
    {
        // Attachment is gated by the feature flag so ops can fall back to HTML-only instantly.
        var attachPdf = _attachReportPdf && pdfAttachment != null;
        if (string.IsNullOrWhiteSpace(dto.RecipientEmail))
            return (false, null, attachPdf);

        try
        {
            var customerName = await _context.NisProjects
                .AsNoTracking()
                .Where(p => p.ProjectId == nisTicket.ProjectId)
                .Select(p => p.Customer)
                .FirstOrDefaultAsync();
            var (subject, body) = await BuildOnsiteCloseEmailAsync(
                cmpId,
                nisTicket.TicketCode ?? nisTicket.TicketId,
                customerName ?? string.Empty,
                nisTicket.Title,
                nisTicket.Assignee,
                dto,
                nisTicket.Type);
            var attachments = attachPdf ? new List<EmailAttachment> { pdfAttachment! } : null;
            var sent = await SendOnsiteEmailAsync(cmpId, dto.RecipientEmail, subject, body, attachments);
            return (sent, null, attachPdf);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, attachPdf);
        }
    }

    /// <summary>
    /// Sends the onsite report email using SMTP config from dbo.EmailSmtpSettings
    /// (settingName "nis", falling back to "default") instead of hardcoded relay values.
    /// Password is AES-decrypted via AesCrypto; an empty PasswordEnc means the relay
    /// authenticates by IP allowlist, so no credentials are attached.
    /// </summary>
    private async Task<bool> SendOnsiteEmailAsync(string cmpId, string recipientEmail, string subject, string htmlBody, IReadOnlyList<EmailAttachment>? attachments = null)
    {
        var provider = "Unknown";

        try
        {
            var setting = await _emailRepo.GetActiveAsync(cmpId, "nis")
                ?? await _emailRepo.GetActiveAsync(cmpId, "default");

            if (setting == null)
                throw new InvalidOperationException($"Email SMTP setting not found for company '{cmpId}' (settingName 'nis' or 'default').");

            // Once the Google consent flow has stored a refresh token, prefer Gmail
            // API OAuth. Existing SMTP/app-password settings remain a fallback.
            if (setting.GoogleOAuthRefreshTokenEnc.Length > 0)
            {
                provider = "GoogleOAuth";
                await _googleOAuthMail.SendAsync(setting, recipientEmail, subject, htmlBody, attachments);
                return true;
            }

            provider = "SMTP";
            using var smtpClient = new System.Net.Mail.SmtpClient(setting.SmtpHost, setting.SmtpPort)
            {
                EnableSsl = setting.EnableSsl,
                UseDefaultCredentials = false,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                Timeout = 100000,
            };

            if (setting.PasswordEnc.Length > 0)
            {
                var appPassword = _crypto.Decrypt(setting.PasswordEnc, setting.PasswordIv);
                smtpClient.Credentials = new System.Net.NetworkCredential(setting.Username, appPassword);
            }

            var fromAddress = new System.Net.Mail.MailAddress(setting.FromEmail, setting.FromName ?? "GoAlong Support");
            using var mailMsg = new System.Net.Mail.MailMessage(fromAddress, new System.Net.Mail.MailAddress(recipientEmail))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };

            // MemoryStreams must outlive smtpClient.Send → dispose after sending.
            var attachmentStreams = new List<MemoryStream>();
            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    if (att.Content.Length == 0) continue;
                    var ms = new MemoryStream(att.Content);
                    attachmentStreams.Add(ms);
                    mailMsg.Attachments.Add(new System.Net.Mail.Attachment(ms, att.FileName, att.ContentType));
                }
            }

            try
            {
                smtpClient.Send(mailMsg);
            }
            finally
            {
                foreach (var ms in attachmentStreams) ms.Dispose();
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "NIS onsite email send failed for company {CmpId}, recipient {RecipientEmail}",
                cmpId,
                recipientEmail);

            await SaveEmailFailureLogAsync(cmpId, recipientEmail, subject, provider, ex);
            throw;
        }
    }

    private async Task SaveEmailFailureLogAsync(string cmpId, string recipientEmail, string subject, string provider, Exception sendException)
    {
        try
        {
            _context.EmailSendLogs.Add(new EmailSendLog
            {
                Source = "NisOnsite",
                CmpId = TruncateLogValue(cmpId, 100),
                RecipientEmail = TruncateLogValue(recipientEmail, 320),
                Subject = TruncateLogValue(subject, 500),
                Provider = TruncateLogValue(provider, 30),
                IsSuccess = false,
                ErrorMessage = TruncateLogValue(sendException.Message, 4000),
                ErrorDetail = sendException.ToString(),
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
        }
        catch (Exception logException)
        {
            // Logging must never replace the original email exception returned to the caller.
            _logger.LogError(
                logException,
                "Failed to persist NIS onsite email error in dbo.EmailSendLog for company {CmpId}",
                cmpId);
        }
    }

    private static string TruncateLogValue(string? value, int maxLength)
    {
        var normalized = value ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    /// Attachment file name for the report PDF — client hint, sanitized, with an SR-based fallback.
    private static string ReportPdfFileName(NisOnsiteSubmitDto dto)
    {
        var raw = string.IsNullOrWhiteSpace(dto.ReportPdfFileName)
            ? $"Service-Report-{(string.IsNullOrWhiteSpace(dto.SrNumber) ? "draft" : dto.SrNumber)}"
            : dto.ReportPdfFileName;
        var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, "[^A-Za-z0-9._-]", "_");
        if (!cleaned.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) cleaned += ".pdf";
        return cleaned;
    }

    /// <summary>
    /// หัวเรื่องอีเมลปิดงานเมื่อไม่มี template ที่เปิดใช้งาน — "ชื่องาน - ลูกค้า"
    /// ไม่มีทั้งชื่องานและชื่อลูกค้า → ตกไปใช้เลขใบรายงานเพื่อไม่ให้หัวเรื่องว่าง
    /// </summary>
    /// <param name="projectTitle">ชื่องาน / ชื่อตั๋ว เช่น "Onsite MA รอบที่ 2"</param>
    /// <param name="customerName">ชื่อลูกค้า</param>
    /// <param name="srNumber">เลขใบรายงานบริการ</param>
    /// <returns>หัวเรื่องอีเมล</returns>
    private static string OnsiteCloseSubjectFallback(string? projectTitle, string? customerName, string? srNumber)
    {
        var parts = new[] { projectTitle, customerName }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim())
            .ToList();
        if (parts.Count > 0) return string.Join(" - ", parts);
        return string.IsNullOrWhiteSpace(srNumber) ? "Service Report" : $"Service Report {srNumber}";
    }

    /// <summary>
    /// อ่านข้อมูลบริษัทของ tenant (SP dbo.getcmpinfo) เพื่อใช้เป็น fallback ของบล็อกบริษัทในลายเซ็นอีเมล
    /// เมื่อช่องในหน้า System Config ถูกปล่อยว่าง — ตรงกับพฤติกรรมของ CRM ที่ประกอบลายเซ็นเอง
    /// อ่านไม่ได้ (SP ผิดพลาด / ไม่มีข้อมูล) → คืน null แล้วปล่อยให้ใช้ค่าจาก config อย่างเดียว
    /// </summary>
    /// <param name="cmpId">รหัสบริษัทของ tenant</param>
    /// <returns>ข้อมูลบริษัทสำหรับลายเซ็น หรือ null เมื่อไม่พบ</returns>
    private async Task<NisEmailTemplateRenderer.NisEmailCompany?> LoadEmailSignatureCompanyAsync(string cmpId)
    {
        if (string.IsNullOrWhiteSpace(cmpId)) return null;

        try
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC dbo.getcmpinfo @CmpId = @cmpid";
            var param = cmd.CreateParameter();
            param.ParameterName = "@cmpid";
            param.Value = cmpId;
            cmd.Parameters.Add(param);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            // SP คืนคอลัมน์ต่างชุดกันตามเวอร์ชัน DB → อ่านแบบทนคอลัมน์หาย
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
            string Col(string name)
            {
                if (!columns.Contains(name)) return string.Empty;
                var value = reader[name];
                return value == DBNull.Value ? string.Empty : (value.ToString() ?? string.Empty).Trim();
            }

            var logoFile = Col("CmpImg");
            var logoUrl = string.IsNullOrWhiteSpace(logoFile)
                ? string.Empty
                : logoFile.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? logoFile
                    : $"{PublicBaseUrl()}/images/{logoFile}";

            var phone = Col("Phone");
            if (phone.Length == 0) phone = Col("TelOffice");

            return new NisEmailTemplateRenderer.NisEmailCompany(
                CompanyNameTh: Col("CmpName"),
                CompanyNameEn: Col("CmpNameEN"),
                Address: Col("CmpAddress"),
                Phone: phone,
                Website: Col("WebSite"),
                LogoUrl: logoUrl);
        }
        catch (Exception ex)
        {
            // อีเมลต้องส่งได้เสมอ — ขาด fallback บริษัทไม่ควรทำให้ปิดงานล้ม
            _logger.LogWarning(ex, "NIS email signature: load company info failed (cmpId={CmpId})", cmpId);
            return null;
        }
    }

    /// <summary>
    /// base URL สาธารณะของ API สำหรับประกอบ URL รูปในอีเมล (โลโก้บริษัทอยู่ที่ wwwroot/images)
    /// ตั้งค่าได้ที่ NisOnsite:PublicBaseUrl — ไม่ตั้งไว้จะใช้ host ของ request ปัจจุบัน
    /// </summary>
    /// <returns>base URL ที่ไม่มี / ปิดท้าย</returns>
    private string PublicBaseUrl() =>
        (string.IsNullOrWhiteSpace(_publicBaseUrl)
            ? $"{Request.Scheme}://{Request.Host}"
            : _publicBaseUrl).TrimEnd('/');

    /// <summary>
    /// ประกอบ subject + body ของอีเมลปิดงาน onsite จาก email template + ลายเซ็นในหน้า System Config
    /// (ใช้ร่วมกันทั้ง RN และ CRM เพราะทั้งคู่ปิดงานผ่าน POST api/nis/onsite/submit)
    /// ไม่มี config / template ถูกปิดใช้งาน → fallback เป็น body มาตรฐานเดิม แต่ยังต่อลายเซ็นให้
    /// </summary>
    /// <param name="cmpId">รหัสบริษัท — ใช้หา NisSystemConfig ของ tenant</param>
    /// <param name="ticketNo">เลขที่ตั๋วที่แสดงให้ลูกค้า ([TK_NUMBER])</param>
    /// <param name="customerName">ชื่อลูกค้า ([COMPANY])</param>
    /// <param name="projectTitle">ชื่องาน / โครงการ ([PROJECT])</param>
    /// <param name="assignee">ช่างที่รับผิดชอบตั๋ว ([ENGINEER]) — ว่างได้</param>
    /// <param name="dto">payload ปิดงานจาก client</param>
    /// <param name="ticketType">
    /// ประเภทตั๋ว/งาน (Install/PM/MA Onsite/Support/Backup/Report/Delivery/MA) — ใช้เลือก template
    /// เฉพาะประเภทก่อน (close-job-{ticketType}) แล้ว fallback เป็น close-job เดิมถ้าไม่มี ว่างได้
    /// (จะข้ามไปใช้ default เลย)
    /// </param>
    /// <returns>subject และ body (HTML) ที่พร้อมส่ง</returns>
    private async Task<(string Subject, string Body)> BuildOnsiteCloseEmailAsync(
        string cmpId,
        string ticketNo,
        string customerName,
        string? projectTitle,
        string? assignee,
        NisOnsiteSubmitDto dto,
        string? ticketType = null)
    {
        var config = await _context.NisSystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CmpId == cmpId);

        var templates = ParseJson(config?.EmailTemplatesJson, DefaultEmailTemplates());
        var signature = ParseJson(config?.EmailSignatureJson, new NisEmailSignatureDto());
        var template = NisEmailTemplateRenderer.FindCloseJobTemplateForType(templates, ticketType);

        // ชื่อผู้ส่ง: ค่าที่ client ส่งมา > Accounts.FullName ของ user ที่ปิดงาน > ค่าที่ตั้งไว้ในหน้า config
        var senderName = dto.SenderName;
        if (string.IsNullOrWhiteSpace(senderName) && !string.IsNullOrWhiteSpace(dto.User))
        {
            senderName = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.CmpId == cmpId && a.Username == dto.User)
                .Select(a => a.FullName)
                .FirstOrDefaultAsync();
        }

        var sender = new NisEmailTemplateRenderer.NisEmailSender(senderName, dto.SenderPosition, dto.SenderMobile);
        // ช่องบริษัทที่ปล่อยว่างในหน้า config → ใช้ข้อมูลบริษัทของ tenant (ตรงกับที่ CRM ประกอบเอง)
        var company = await LoadEmailSignatureCompanyAsync(cmpId);
        var signatureHtml = NisEmailTemplateRenderer.BuildSignatureHtml(signature, sender, company);

        if (template == null)
        {
            // ไม่มี template → หัวเรื่อง = ชื่องาน + ลูกค้า (รูปแบบเดียวกับ template "close-job")
            var fallbackSubject = string.IsNullOrWhiteSpace(dto.EmailSubject)
                ? OnsiteCloseSubjectFallback(projectTitle, customerName, dto.SrNumber)
                : dto.EmailSubject;
            return (fallbackSubject, BuildOnsiteReportEmailBody(customerName, dto) + signatureHtml);
        }

        var vars = new Dictionary<string, string?>
        {
            ["TK_NUMBER"] = ticketNo,
            ["SR_NUMBER"] = dto.SrNumber,
            ["PROJECT"] = projectTitle,
            ["COMPANY"] = customerName,
            // ใบรายงานอาจไม่มีชื่อผู้ติดต่อ → ใช้ชื่อลูกค้าแทน (เหมือนหน้า Report tab ของ CRM)
            ["CONTACT"] = string.IsNullOrWhiteSpace(dto.ContactName) ? customerName : dto.ContactName,
            ["ENGINEER"] = string.IsNullOrWhiteSpace(assignee) ? senderName : assignee,
            ["DATE"] = string.IsNullOrWhiteSpace(dto.CheckOutTime) ? BangkokNow().ToString("dd/MM/yyyy") : dto.CheckOutTime,
            ["SERVICE_DETAIL"] = dto.WorkDetail,
            ["SENDER"] = senderName,
            ["SENDER_MOBILE"] = dto.SenderMobile,
        };

        var subject = string.IsNullOrWhiteSpace(dto.EmailSubject)
            ? NisEmailTemplateRenderer.Render(template.Subject, vars)
            : dto.EmailSubject;

        var body = $"""
            <div style="font-family:sans-serif;max-width:640px;margin:0 auto;padding:24px;color:#111;">
              {NisEmailTemplateRenderer.Render(template.Body, vars, html: true)}
              {OnsiteEmailMessageSection(dto)}
              <table style="width:100%;border-collapse:collapse;margin:16px 0;font-size:14px;">
                <tr style="background:#f8fafc;">
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;width:160px;">เลขที่ใบรายงาน</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{NisEmailTemplateRenderer.EscapeHtml(dto.SrNumber)}</td>
                </tr>
                <tr>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Check-in</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{NisEmailTemplateRenderer.EscapeHtml(dto.CheckInTime)}</td>
                </tr>
                <tr style="background:#f8fafc;">
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Check-out</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{NisEmailTemplateRenderer.EscapeHtml(dto.CheckOutTime)}</td>
                </tr>
              </table>
              {OnsiteCustomerSignatureSection(dto)}
              {signatureHtml}
            </div>
            """;

        return (subject, body);
    }

    /// ข้อความเพิ่มเติมที่ช่างพิมพ์ในกล่องส่งเมล — escape แล้วคงการขึ้นบรรทัดใหม่ไว้
    private static string OnsiteEmailMessageSection(NisOnsiteSubmitDto dto) =>
        string.IsNullOrWhiteSpace(dto.EmailMessage)
            ? string.Empty
            : $"<div style=\"margin:16px 0;padding:14px 16px;background:#f8fafc;border-left:4px solid #f59e0b;line-height:1.6;\">{NisEmailTemplateRenderer.PlainTextToHtml(dto.EmailMessage)}</div>";

    /// ลายเซ็นลูกค้าที่เซ็นรับงานหน้างาน (คนละส่วนกับลายเซ็นอีเมลในหน้า config)
    private static string OnsiteCustomerSignatureSection(NisOnsiteSubmitDto dto) =>
        dto.SkipSignature
            ? "<p style=\"color:#64748b;\">* ลูกค้าไม่ได้ลงนาม (skipped)</p>"
            : !string.IsNullOrWhiteSpace(dto.SignatureImg)
                ? $"<p style=\"font-weight:600;\">ลายเซ็นลูกค้า:</p><img src=\"{dto.SignatureImg}\" style=\"max-width:300px;border:1px solid #e2e8f0;border-radius:6px;padding:4px;\" />"
                : string.Empty;

    private static string BuildOnsiteReportEmailBody(string customerName, NisOnsiteSubmitDto dto)
    {
        var emailMessageSection = OnsiteEmailMessageSection(dto);
        var signatureSection = OnsiteCustomerSignatureSection(dto);

        return $"""
            <div style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:24px;">
              <h2 style="color:#312e81;margin-bottom:4px;">NIS Service Report</h2>
              <p style="color:#64748b;margin-top:0;">SR: {dto.SrNumber}</p>
              <hr style="border:none;border-top:1px solid #e2e8f0;margin:16px 0;" />
              {emailMessageSection}
              <table style="width:100%;border-collapse:collapse;margin:16px 0;font-size:14px;">
                <tr style="background:#f8fafc;">
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;width:160px;">Customer</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{customerName}</td>
                </tr>
                <tr>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Check-in</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{dto.CheckInTime}</td>
                </tr>
                <tr style="background:#f8fafc;">
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Check-out</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{dto.CheckOutTime}</td>
                </tr>
              </table>
              <p style="white-space:pre-wrap;">{dto.WorkDetail}</p>
              {signatureSection}
              <hr style="border:none;border-top:1px solid #e2e8f0;margin:16px 0;" />
              <p style="color:#64748b;font-size:12px;">This is an automated email from NIS System. Please do not reply to this email.</p>
            </div>
            """;
    }

    // ── POST api/nis/onsite/{id}/request-close ───────────────────────────────
    // Records the report and marks the subtask as pending approval via the same
    // StateSendApprove/DateSendApprove/SendApproveBy fields the existing
    // subtask/sendapprove endpoint uses — no email is sent (matches the mockup:
    // request-close only notifies internally, unlike the direct submit+email flow).

    [HttpPost("onsite/{id}/request-close")]
    public async Task<IActionResult> RequestOnsiteClose(string id, [FromBody] NisOnsiteRequestCloseDto dto)
    {
        // NIS-first: record the report and mark the NisTicket "Waiting Close Approval"
        // (reviewed via the existing NIS close-approval flow on the Service Board).
        // รูปที่ยังเป็น reference ในเครื่อง (file:// ฯลฯ) = ยังอัปโหลดไม่สำเร็จ → ห้ามบันทึก
        var localPhotoField = FindLocalPhotoField(dto);
        if (localPhotoField != null)
            return BadRequest(new { message = "รูปยังอัปโหลดไม่สำเร็จ", field = localPhotoField });

        var nisTicket = await ResolveNisOnsiteTicketAsync(id);
        if (nisTicket != null)
        {
            var nisCmpId = dto.CmpId ?? nisTicket.CmpId ?? string.Empty;

            // Business rule ตรวจซ้ำที่ backend: งาน PM ต้องตรวจตู้ Rack ครบก่อนขออนุมัติปิด
            if (MapNisOnsiteType(nisTicket.Type) == "PM")
            {
                var pmBlockers = ValidatePmRackInspection(dto.PmItems);
                if (pmBlockers.Count > 0)
                    return BadRequest(new { message = "PM rack inspection incomplete", blockers = pmBlockers });
            }

            // PDF ใบรายงาน (optional) — ตรวจก่อนแตะตั๋ว · จำกัด 7.5 MB หลัง decode (contract §4)
            byte[]? pdfBytes = null;
            if (!string.IsNullOrWhiteSpace(dto.ReportPdfBase64))
            {
                if (!_pdfStorage.TryDecode(dto.ReportPdfBase64, out var decoded, out var pdfError))
                    return BadRequest(new { message = pdfError });
                if (decoded.Length > RequestClosePdfMaxBytes)
                    return BadRequest(new { message = "report PDF too large (max 7.5 MB)" });
                pdfBytes = decoded;
            }

            // ใช้แถวที่จองเลขไว้ (reserve-sr) / แถวที่ถูกตีกลับ / แถวรออนุมัติเดิม (retry) — ไม่สร้างแถวใหม่ซ้อน
            // คงเลข SR ของแถวนั้นไว้ (ละเว้นเลขอื่นที่ client ส่งมา)
            var report = await _context.NisOnsiteReports
                .Where(r => r.NisTicketId == nisTicket.TicketId
                    && (r.Status == "reserved" || r.Status == "pending_approval" || r.Status == "rejected"))
                .OrderByDescending(r => r.SrNumber != null && r.SrNumber != "")
                .ThenByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            var built = BuildNisOnsiteReport(nisTicket, dto, nisCmpId, "pending_approval");
            if (report == null)
            {
                report = built;
                _context.NisOnsiteReports.Add(report);
            }
            else
            {
                CopyOnsiteReportContent(report, built);
            }

            // อีเมลปิดงาน (ชื่อ field เดียวกับ onsite/submit) — ส่งจริงตอน close-approve
            report.CloseEmailJson = SerializeJson(new NisOnsiteStoredCloseEmailDto
            {
                RecipientEmail = TrimOrNull(dto.RecipientEmail, 320),
                EmailSubject = TrimOrNull(dto.EmailSubject, 500),
                EmailMessage = TrimOrNull(dto.EmailMessage, 4000),
                ContactName = TrimOrNull(dto.ContactName, 200),
                SenderName = TrimOrNull(dto.SenderName, 200),
                SenderPosition = TrimOrNull(dto.SenderPosition, 200),
                SenderMobile = TrimOrNull(dto.SenderMobile, 50),
                ReportPdfFileName = TrimOrNull(dto.ReportPdfFileName, 200),
            });
            report.RejectReason = null;

            // PDF รอบใหม่แทนของเดิม · ไม่ส่งมา = ล้าง reference เก่า (เนื้อหาอาจเปลี่ยนหลังถูกตีกลับ)
            if (pdfBytes != null)
            {
                var stored = await _pdfStorage.SaveAsync(nisCmpId, nisTicket.TicketId, report.SrNumber ?? string.Empty, pdfBytes);
                report.ReportPdfPath = stored.RelativePath;
                report.ReportPdfSize = stored.Size;
                report.ReportPdfSha256 = stored.Sha256;
            }
            else
            {
                report.ReportPdfPath = null;
                report.ReportPdfSize = null;
                report.ReportPdfSha256 = null;
            }

            nisTicket.Status = "Waiting Close Approval";
            // งานหน้างานจบแล้ว รอ SM อนุมัติ — ค้างที่ 90 จนกว่าจะปิดจริง (100)
            nisTicket.Pct = NIS_PCT_CHECKED_OUT;
            ApplyClosedDate(nisTicket);
            nisTicket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            await NotifyTicketChangedAsync(nisTicket, "request_close", dto.User);

            return Ok(new
            {
                message = "Close approval requested",
                ticketId = nisTicket.TicketId,
                srNumber = report.SrNumber,
                pdfStored = report.ReportPdfPath != null,
            });
        }

        var (ticket, subTask) = await ResolveOnsiteTicketAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Onsite ticket '{id}' not found" });

        var cmpId = dto.CmpId ?? ticket.CmpId ?? string.Empty;
        var user = dto.User ?? string.Empty;

        _context.ServiceTicketSubTaskActions.Add(new ServiceTicketSubTaskAction
        {
            TaskActionId = Guid.NewGuid().ToString("N"),
            TicketId = ticket.TicketId,
            SubTaskId = subTask?.SubTaskId ?? string.Empty,
            CmpId = cmpId,
            Seq = 1,
            ActionDate = DateTime.Now,
            ActionDetails = "Onsite service report (pending close approval)",
            ActionStatus = "pending_approval",
            SrNumber = dto.SrNumber,
            WorkDetail = dto.WorkDetail,
            IssueDetail = dto.IssueDetail,
            ChecklistItemsJson = JsonSerializer.Serialize(dto.Checklist),
            RackPhotosJson = JsonSerializer.Serialize(dto.PmItems),
            DamagedProductJson = dto.DamagedProduct != null ? JsonSerializer.Serialize(dto.DamagedProduct) : null,
            OthersItemsJson = JsonSerializer.Serialize(dto.SupportCases),
            WorkPhotosJson = JsonSerializer.Serialize(dto.Photos),
            SignatureImageBase64 = dto.SignatureImg,
            CheckInLatitude = (decimal?)dto.CheckInLat,
            CheckInLongitude = (decimal?)dto.CheckInLng,
            CheckOutLatitude = (decimal?)dto.CheckOutLat,
            CheckOutLongitude = (decimal?)dto.CheckOutLng,
            UpdatedAt = DateTime.Now,
        });

        if (subTask != null)
        {
            subTask.StateSendApprove = "1";
            subTask.DateSendApprove = DateTime.Now;
            subTask.SendApproveBy = user;
            subTask.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Close approval requested",
            ticketId = ticket.TicketId,
            subTaskId = subTask?.SubTaskId,
            srNumber = dto.SrNumber,
        });
    }

    // ── NIS onsite progress (cross-device draft) ─────────────────────────────
    // Draft ความคืบหน้างาน onsite ที่ยังไม่ปิดงาน ต่อ 1 ตั๋ว/1 ช่าง — เก็บ snapshot
    // ทั้งก้อนแบบ opaque JSON (schema เป็นของ client · server เช็คแค่ savedAt)
    // ผู้ใช้: CRM เขียน+อ่าน (dual-write/reconcile ข้ามเครื่อง) · RN อ่านอย่างเดียว
    // {id} = ticketCode ("TK-BK-0014-10") — คีย์กลางที่ CRM/RN ตกลงใช้ร่วมกัน
    // Contract: go-crm-24v4/docs/nis-onsite-progress-api-contract.md

    /// <summary>
    /// อ่าน snapshot ล่าสุดของตั๋วนี้ "ข้ามทุก user" (draft ร่วมระหว่าง CRM ↔ RN — contract §3)
    /// เรียงตาม UpdatedAt (server time) ล่าสุดก่อน · ไม่มี = 204
    /// response = snapshot JSON เดิมที่ client save ไว้ + field เสริม updatedAt (ISO UTC) และ updatedBy (username เจ้าของแถว)
    /// {id} = ticketCode (หรือ TicketId — resolve เป็นทั้งสองคีย์ให้ CRM/RN ที่ใช้คีย์ต่างกันเห็นแถวเดียวกัน)
    /// user ไม่บังคับแล้ว (client เดิมยังส่งมาได้)
    /// </summary>
    [HttpGet("onsite/{id}/progress")]
    public async Task<IActionResult> GetOnsiteProgress(
        string id,
        [FromQuery] string? cmpid,
        [FromQuery] string? user)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var keys = new List<string> { id };
        var ticket = await _context.NisTickets
            .AsNoTracking()
            .Where(t => t.CmpId == cmpid && (t.TicketCode == id || t.TicketId == id))
            .Select(t => new { t.TicketId, t.TicketCode })
            .FirstOrDefaultAsync();
        if (ticket != null)
        {
            keys.Add(ticket.TicketId);
            if (!string.IsNullOrWhiteSpace(ticket.TicketCode)) keys.Add(ticket.TicketCode);
        }
        keys = keys.Distinct().ToList();

        var row = await _context.NisOnsiteProgresses
            .AsNoTracking()
            .Where(p => p.CmpId == cmpid && keys.Contains(p.TicketId))
            .OrderByDescending(p => p.UpdatedAt)
            .ThenByDescending(p => p.SavedAt)
            .FirstOrDefaultAsync();

        if (row == null || string.IsNullOrEmpty(row.SnapshotJson)) return NoContent();

        // เติม updatedAt/updatedBy ที่ root (additive) — snapshot อ่านไม่ได้เป็น object → คืน raw เดิม
        try
        {
            if (System.Text.Json.Nodes.JsonNode.Parse(row.SnapshotJson) is System.Text.Json.Nodes.JsonObject obj)
            {
                obj["updatedAt"] = DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture);
                obj["updatedBy"] = row.UserLogin;
                return Content(obj.ToJsonString(), "application/json");
            }
        }
        catch (JsonException)
        {
            // snapshot เสีย → คืน raw ตามเดิม
        }
        return Content(row.SnapshotJson, "application/json");
    }

    /// upsert snapshot — body = snapshot ทั้งก้อน + { cmpid, user } (client แนบมากับ root)
    /// เขียนทับเสมอด้วยก้อนล่าสุดตาม contract (client เป็นผู้คุม savedAt/ลำดับ)
    [HttpPost("onsite/{id}/progress")]
    public async Task<IActionResult> SaveOnsiteProgress(string id, [FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
            return BadRequest(new { message = "snapshot body is required" });

        // cmpid/user มากับ root ของ body (CRM แนบ {...snapshot, cmpid, user})
        var cmpid = ReadJsonString(body, "cmpid") ?? ReadJsonString(body, "cmpId");
        var user = ReadJsonString(body, "user");
        if (string.IsNullOrWhiteSpace(cmpid) || string.IsNullOrWhiteSpace(user))
            return BadRequest(new { message = "cmpid and user are required" });

        // savedAt ต้องเป็นเลข > 0 — client ใช้ตัวนี้ reconcile ฝั่งอ่าน
        long savedAt = 0;
        if (body.TryGetProperty("savedAt", out var savedAtProp) && savedAtProp.ValueKind == JsonValueKind.Number)
            savedAtProp.TryGetInt64(out savedAt);
        if (savedAt <= 0)
            return BadRequest(new { message = "savedAt (epoch ms) is required" });

        var row = await _context.NisOnsiteProgresses
            .FirstOrDefaultAsync(p => p.CmpId == cmpid && p.TicketId == id && p.UserLogin == user);

        if (row == null)
        {
            row = new NisOnsiteProgress { CmpId = cmpid, TicketId = id, UserLogin = user };
            _context.NisOnsiteProgresses.Add(row);
        }

        row.SnapshotJson = body.GetRawText();
        row.SavedAt = savedAt;
        row.UpdatedAt = DateTime.UtcNow;

        // sync % ไปตั๋วจริง — ให้ NisTicket.Pct สะท้อน progress ล่าสุดของ draft
        // {id} = ticketCode ("TK-…") ตาม contract · เพดาน 90 (100 เกิดเฉพาะ flow ปิดงาน)
        // ตั๋วที่พ้นมือช่างแล้ว (Waiting Close Approval / Done / Closed) ไม่แตะ — กัน autosave ค้างเขียนทับ
        var ticket = await _context.NisTickets
            .FirstOrDefaultAsync(t => t.CmpId == cmpid && (t.TicketCode == id || t.TicketId == id));
        if (ticket != null
            && ticket.Status != "Waiting Close Approval"
            && ticket.Status != "Done"
            && ticket.Status != "Closed")
        {
            // ยังไม่รับงาน (Open/Scheduled) → ฐาน 0 · รับแล้ว → ฐาน 10 (ตรง AcceptTicket / pctForStatus)
            var accepted = ticket.Status != "Open" && ticket.Status != "Scheduled";
            // snapshot เป็นของ user ที่เปิดฟอร์ม (draft ต่อ user) — อาจไม่มี check-in/out ที่ช่างทำจากอีกเครื่อง
            // จึงใช้คอลัมน์บนตั๋วเป็นแหล่งเสริม + floor กัน % ถอยหลัง (เคส SM เปิดดูใน CRM แล้ว 90 → 25)
            ticket.Pct = Math.Max(ComputeOnsiteProgressPct(body, accepted, ticket), TicketPctFloor(ticket));
            ticket.UpdatedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        // contract "ticket changed" — อีกเครื่อง (CRM ↔ RN) ดึง snapshot ล่าสุดไปใช้ (client กัน echo ทับตอนพิมพ์เอง)
        if (ticket != null)
            await NotifyTicketChangedAsync(ticket, "progress", user);

        return Ok(new { ok = true });
    }

    /// ล้าง draft — client เรียกตอนปิดงานสำเร็จ/ส่งขออนุมัติปิดงาน (idempotent: ไม่เจอก็ 200)
    [HttpDelete("onsite/{id}/progress")]
    public async Task<IActionResult> DeleteOnsiteProgress(
        string id,
        [FromQuery] string? cmpid,
        [FromQuery] string? user)
    {
        if (string.IsNullOrWhiteSpace(cmpid) || string.IsNullOrWhiteSpace(user))
            return BadRequest(new { message = "cmpid and user are required" });

        var row = await _context.NisOnsiteProgresses
            .FirstOrDefaultAsync(p => p.CmpId == cmpid && p.TicketId == id && p.UserLogin == user);

        if (row != null)
        {
            _context.NisOnsiteProgresses.Remove(row);
            await _context.SaveChangesAsync();
        }
        return Ok(new { ok = true });
    }

    /// คำนวณ % ความคืบหน้างาน onsite จาก snapshot draft (milestone-based ตาม NIS_PCT_*)
    /// เช็คอิน 25 · checklist ดันจาก 25 → 85 ตามสัดส่วนที่ติ๊ก · เช็คเอาท์ 90
    /// ฐานเริ่มที่ 10 เฉพาะตั๋วที่ช่างรับงานแล้ว (accepted) — ตั๋ว Open/Scheduled ที่แค่ถูกเปิดดู
    /// แล้ว client ยิง draft เปล่ามา ต้องคง 0 (parity NIS-OnsiteService utils/progress.ts computeTicketPct)
    /// เพดาน 90 — 100 เฉพาะ flow ปิดงาน
    /// ticket (optional) = ค่า check-in/out ที่ server รู้จริง — ใช้แทนเมื่อ snapshot ไม่มี
    /// (draft เป็นของ user ที่เปิดฟอร์ม ไม่ใช่ของช่างที่เช็คอิน/เช็คเอาท์จากอีกเครื่อง)
    private static int ComputeOnsiteProgressPct(JsonElement s, bool accepted, NisTicket? ticket = null)
    {
        var pct = accepted ? NIS_PCT_ACCEPTED : NIS_PCT_NOT_ACCEPTED;

        var checkedIn = !string.IsNullOrEmpty(ReadJsonString(s, "checkInTime")) || ticket?.CheckInTime != null;
        if (checkedIn)
            pct = NIS_PCT_CHECKED_IN;

        if (s.TryGetProperty("checklist", out var checklist) && checklist.ValueKind == JsonValueKind.Array)
        {
            int total = 0, done = 0;
            foreach (var item in checklist.EnumerateArray())
            {
                total++;
                if (item.ValueKind == JsonValueKind.Object
                    && item.TryGetProperty("checked", out var c)
                    && c.ValueKind == JsonValueKind.True)
                    done++;
            }

            // ติ๊กแล้วอย่างน้อย 1 ข้อจึงดัน % (ยังไม่ติ๊กเลย = คงค่าตาม milestone ก่อนหน้า)
            if (total > 0 && done > 0)
            {
                var band = NIS_PCT_CHECKED_IN
                    + (int)Math.Round((double)(NIS_PCT_CHECKLIST_DONE - NIS_PCT_CHECKED_IN) * done / total);
                pct = Math.Max(pct, band);
            }
        }

        var checkedOut = !string.IsNullOrEmpty(ReadJsonString(s, "checkOutTime")) || ticket?.CheckOutTime != null;
        if (checkedOut)
            pct = Math.Max(pct, NIS_PCT_CHECKED_OUT);

        return Math.Min(pct, NIS_PCT_CHECKED_OUT);
    }

    /// อ่าน string property จาก JsonElement แบบปลอดภัย (ไม่มี/ไม่ใช่ string → null)
    private static string? ReadJsonString(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;

}
