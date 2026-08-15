using System.Text.Json;
using System.Globalization;

using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models;
using goalongapi.Models.Nis;
using goalongapi.Helpers;
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
    };

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
            .Include(p => p.Tickets)
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
            .Include(p => p.Tickets)
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

        // Serializable transaction so the MAX(ProjectNo)+1 read below can't race with a
        // concurrent CreateProject call for the same CmpId (avoids duplicate project numbers).
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var bangkokNow = BangkokNow();
        var projectPrefix = $"NIS-{bangkokNow.ToString("yy", CultureInfo.InvariantCulture)}";
        var existingProjectNos = await _context.NisProjects
            .Where(p => p.CmpId == cmpId && p.ProjectNo != null && p.ProjectNo.StartsWith(projectPrefix))
            .Select(p => p.ProjectNo)
            .ToListAsync();
        var lastProjectNo = existingProjectNos
            .Select(no => no != null
                && no.StartsWith(projectPrefix, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(no[projectPrefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0)
            .DefaultIfEmpty(0)
            .Max();
        var nextProjectNo = $"{projectPrefix}{lastProjectNo + 1:D5}";

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
            ContactName = dto.Contact?.Name,
            ContactPhone = dto.Contact?.Phone,
            ContactEmail = dto.Contact?.Email,
            SalesPMName = dto.SalesPM?.Name,
            SalesPMNickname = dto.SalesPM?.Nickname,
            SalesPMPhone = dto.SalesPM?.Phone,
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

        // RunNo continues from however many tickets of this Type already exist in the project.
        var existingCount = await _context.NisTickets
            .CountAsync(t => t.ProjectId == id && t.Type == dto.Type);
        var runNo = existingCount + 1;

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
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };

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

        ticket.Status = dto.Status;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Status updated" });
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

        var acceptedBy = string.IsNullOrWhiteSpace(dto?.AcceptedBy) ? ticket.Assignee : dto!.AcceptedBy!;

        // กดรับซ้ำ (retry ตอนเน็ตกระตุก / ปุ่มโดนกดสองครั้ง) → ไม่ถือเป็น error แต่ไม่ต้องแจ้งซ้ำ
        // (ตัวกัน dedupe จริงอยู่ที่ EventKey ของ push อีกชั้น)
        var alreadyAccepted = ticket.Status != "Scheduled";

        if (!alreadyAccepted)
        {
            ticket.Status = "In Progress";
            ticket.UpdatedBy = acceptedBy;
            ticket.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        if (!alreadyAccepted)
            await NotifyManagersTicketAcceptedAsync(ticket, acceptedBy);

        return Ok(new { message = "Ticket accepted", status = ticket.Status });
    }

    /// <summary>
    /// แจ้ง Service Manager ทุกคนในบริษัทว่าช่างกดรับงานแล้ว — best-effort ทั้งหมด
    /// (push ไม่สำเร็จห้ามทำให้การกดรับงานล้ม)
    /// ผู้รับ = Accounts ที่ Role.Name เป็น mng/admin (ตรงกับ roleMap ฝั่งแอป)
    /// </summary>
    private async Task NotifyManagersTicketAcceptedAsync(NisTicket ticket, string acceptedBy)
    {
        try
        {
            var managers = await _context.Accounts
                .Where(a => a.CmpId == ticket.CmpId
                    && (a.Role.Name == "mng" || a.Role.Name == "admin"))
                .Select(a => new { a.Username, a.FullName })
                .ToListAsync();

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
        var report = await _context.NisOnsiteReports
            .Where(r => r.NisTicketId == ticket.TicketId
                && (r.Status == "pending_approval" || r.Status == "submitted"))
            .OrderByDescending(r => r.CreatedDate)
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
        report.Status = "submitted";

        ticket.Status = "Closed";
        ticket.Pct = 100;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Ticket closed", srNumber = report.SrNumber });
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
        ticket.UpdatedDate = DateTime.Now;

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

        var previousAssignee = ticket.Assignee;

        ticket.Assignee = dto.Assignee;
        ticket.StartDate = ParseDate(dto.StartDate);
        ticket.EndDate = ParseDate(dto.EndDate);
        ticket.Due = ticket.EndDate ?? ticket.Due;

        if (dto.Assignee == "-")
        {
            // Unassigned → back to the Open / Assigned backlog.
            ticket.Status = "Open";
        }
        else if (previousAssignee != dto.Assignee
                 || ticket.Status == "Open"
                 || ticket.Status == "Scheduled"
                 || string.IsNullOrWhiteSpace(ticket.Status))
        {
            // Freshly assigned (or handed to a different engineer) → awaits the
            // assignee's acceptance in the Staff Portal, which surfaces the
            // "accept task" notification. Accepting moves it to "In Progress"
            // via PUT tickets/{id}/status.
            ticket.Status = "Scheduled";
        }
        // else: same assignee already working (In Progress / Pending / …) — a
        // date-only edit must not reset their progress, so keep the status.

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
    };

    // ── GET api/nis/pending-requests ─────────────────────────────────────────

    /// <summary>
    /// Returns pending requests. Manager view (no requestedBy) → only Status="Pending".
    /// Staff view (requestedBy set) → that engineer's requests in every status (history).
    /// Matches frontend fetchNisPendingRequests / fetchMyNisPendingRequests.
    /// </summary>
    [HttpGet("pending-requests")]
    public async Task<ActionResult<IEnumerable<NisPendingRequestResponseDto>>> GetPendingRequests(
        [FromQuery] string? cmpid,
        [FromQuery] string? requestedBy)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var query = _context.NisPendingRequests
            .AsNoTracking()
            .Where(r => r.CmpId == cmpid);

        if (!string.IsNullOrWhiteSpace(requestedBy))
            query = query.Where(r => r.RequestedBy == requestedBy);
        else
            query = query.Where(r => r.Status == "Pending");

        var list = await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return Ok(list.Select(MapPendingRequest));
    }

    // ── POST api/nis/pending-requests ────────────────────────────────────────

    /// <summary>Staff raises an "open ticket" request. Matches frontend createNisPendingRequest.</summary>
    [HttpPost("pending-requests")]
    public async Task<ActionResult<NisPendingRequestResponseDto>> CreatePendingRequest(
        [FromBody] NisPendingRequestCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "title is required" });

        var request = new NisPendingRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RequestedBy = dto.RequestedBy ?? string.Empty,
            Title = dto.Title,
            TicketType = dto.TicketType,
            SupportMethod = dto.SupportMethod,
            ProjectId = dto.ProjectId ?? string.Empty,
            Location = dto.Location,
            Detail = dto.Detail,
            Due = ParseDate(dto.Due),
            Status = "Pending",
            CmpId = dto.CmpId,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };

        _context.NisPendingRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(MapPendingRequest(request));
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
            CmpId = cmpId,
            CreatedBy = dto.ApprovedBy ?? string.Empty,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now,
        };

        request.Status = "Approved";
        request.ApprovedBy = dto.ApprovedBy;
        request.CreatedTicketId = ticket.TicketId;
        request.UpdatedDate = DateTime.Now;

        _context.NisTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(MapTicket(ticket));
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

        request.Status = "Rejected";
        request.RejectedBy = dto?.RejectedBy;
        request.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Request rejected" });
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
        var oldContacts = await _context.NisContacts
            .Where(c => c.CmpId == cmpId && c.DocType == "customer" && c.DocNo == code)
            .ToListAsync();
        _context.NisContacts.RemoveRange(oldContacts);

        var oldLocations = await _context.NisCustomerLocations
            .Where(l => l.CmpId == cmpId && l.CustomerCode == code)
            .ToListAsync();
        _context.NisCustomerLocations.RemoveRange(oldLocations);

        await _context.SaveChangesAsync();

        foreach (var con in dto.Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
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
            Contacts = dto.Contacts,
            Locations = dto.Locations,
        });
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

            var oldContacts = await _context.NisContacts
                .Where(c =>
                    c.CmpId == cmpId &&
                    c.DocType == "customer" &&
                    c.DocNo == code)
                .ToListAsync();

            _context.NisContacts.RemoveRange(oldContacts);

            var oldLocations = await _context.NisCustomerLocations
                .Where(l =>
                    l.CmpId == cmpId &&
                    l.CustomerCode == code)
                .ToListAsync();

            _context.NisCustomerLocations.RemoveRange(oldLocations);

            foreach (var con in dto.Contacts
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
            Contacts = dto.Contacts,
            Locations = dto.Locations,
        });
    }
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
        SlaOptions = ["8x5xNBD", "8x5", "24x7x4", "24x7xNBD"],
        WarningDays = new NisWarningDaysDto { Service = 60, Product = 30 },
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
            Subject = "Service Report [TK_NUMBER] - [COMPANY]",
            Body = "<p>เรียน คุณ[CONTACT]</p>"
                + "<p>บริษัทฯ ขอส่งใบรายงานการให้บริการ (Service Report) สำหรับงานที่ดำเนินการเสร็จสิ้นแล้ว ดังนี้</p>"
                + "<p>เลขที่ Ticket: <strong>[TK_NUMBER]</strong><br/>โครงการ / งาน: [PROJECT]<br/>"
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
            DamagedProductJson = dto.DamagedProduct != null ? JsonSerializer.Serialize(dto.DamagedProduct) : null,
            SupportCasesJson = JsonSerializer.Serialize(dto.SupportCases),
            PhotosJson = JsonSerializer.Serialize(dto.Photos),
            SignatureImageBase64 = dto.SignatureImg,
            SkipSignature = dto.SkipSignature,
            Status = status,
            CreatedDate = DateTime.Now,
        };

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

        var query = _context.NisOnsiteReports.AsNoTracking().Where(r => r.CmpId == cmpid);
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
                Date = r.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Status = "Closed",
            };
        }).ToList();

        return Ok(result);
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

            return Ok(new NisOnsiteTicketResponseDto
            {
                Id = nisTicket.TicketCode ?? nisTicket.TicketId,
                Title = nisTicket.Title,
                Customer = project?.Customer ?? string.Empty,
                Location = project?.Location ?? string.Empty,
                TicketType = MapNisOnsiteType(nisTicket.Type),
                ContactName = project?.ContactName ?? string.Empty,
                ContactPhone = project?.ContactPhone ?? string.Empty,
                ContactEmail = project?.ContactEmail,
                SalesName = project?.SalesPMName,
                SalesNickname = project?.SalesPMNickname,
                SalesPhone = project?.SalesPMPhone,
                SalesRole = project?.SalesPMRole,
                EngineerName = string.IsNullOrWhiteSpace(project?.EngineerName) ? nisTicket.Assignee : project!.EngineerName,
                EngineerNick = project?.EngineerNickname ?? string.Empty,
                EngineerPhone = project?.EngineerPhone,
                Status = nisTicket.Status,
                SkipSignature = false,
                RequireCloseApproval = false,
                Accepted = true,
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

        // NIS-first: report filed against a NisTicket → persist to NisOnsiteReport
        // and close the NisTicket. Damaged-product / support-case downstream tickets
        // are kept as report JSON (not auto-created for the NIS system).
        var nisTicket = await ResolveNisOnsiteTicketAsync(dto.TicketId);
        if (nisTicket != null)
        {
            var nisCmpId = dto.CmpId ?? nisTicket.CmpId ?? string.Empty;
            var report = BuildNisOnsiteReport(nisTicket, dto, nisCmpId, "submitted");

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
            nisTicket.Pct = 100;
            nisTicket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Attachment is gated by the feature flag so ops can fall back to HTML-only instantly.
            var attachPdf = _attachReportPdf && pdfAttachment != null;
            var nisEmailSent = false;
            string? nisEmailError = null;
            if (!string.IsNullOrWhiteSpace(dto.RecipientEmail))
            {
                try
                {
                    var customerName = await _context.NisProjects
                        .AsNoTracking()
                        .Where(p => p.ProjectId == nisTicket.ProjectId)
                        .Select(p => p.Customer)
                        .FirstOrDefaultAsync();
                    var (subject, body) = await BuildOnsiteCloseEmailAsync(
                        nisCmpId,
                        nisTicket.TicketCode ?? nisTicket.TicketId,
                        customerName ?? string.Empty,
                        nisTicket.Title,
                        nisTicket.Assignee,
                        dto);
                    var attachments = attachPdf ? new List<EmailAttachment> { pdfAttachment! } : null;
                    nisEmailSent = await SendOnsiteEmailAsync(nisCmpId, dto.RecipientEmail, subject, body, attachments);
                }
                catch (Exception ex)
                {
                    nisEmailError = ex.Message;
                }
            }

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
                    dto);
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
    /// <returns>subject และ body (HTML) ที่พร้อมส่ง</returns>
    private async Task<(string Subject, string Body)> BuildOnsiteCloseEmailAsync(
        string cmpId,
        string ticketNo,
        string customerName,
        string? projectTitle,
        string? assignee,
        NisOnsiteSubmitDto dto)
    {
        var config = await _context.NisSystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CmpId == cmpId);

        var templates = ParseJson(config?.EmailTemplatesJson, DefaultEmailTemplates());
        var signature = ParseJson(config?.EmailSignatureJson, new NisEmailSignatureDto());
        var template = NisEmailTemplateRenderer.FindTemplate(templates, NisEmailTemplateRenderer.CloseJobTemplateId);

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
            var fallbackSubject = string.IsNullOrWhiteSpace(dto.EmailSubject)
                ? $"[Service Report] {dto.SrNumber}"
                : dto.EmailSubject;
            return (fallbackSubject, BuildOnsiteReportEmailBody(ticketNo, customerName, dto) + signatureHtml);
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

    private static string BuildOnsiteReportEmailBody(string ticketNo, string customerName, NisOnsiteSubmitDto dto)
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
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;width:160px;">Ticket No</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{ticketNo}</td>
                </tr>
                <tr>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Customer</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{customerName}</td>
                </tr>
                <tr style="background:#f8fafc;">
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;font-weight:600;">Check-in</td>
                  <td style="padding:10px 12px;border:1px solid #e2e8f0;">{dto.CheckInTime}</td>
                </tr>
                <tr>
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
        var nisTicket = await ResolveNisOnsiteTicketAsync(id);
        if (nisTicket != null)
        {
            var nisCmpId = dto.CmpId ?? nisTicket.CmpId ?? string.Empty;
            _context.NisOnsiteReports.Add(BuildNisOnsiteReport(nisTicket, dto, nisCmpId, "pending_approval"));

            nisTicket.Status = "Waiting Close Approval";
            nisTicket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Close approval requested",
                ticketId = nisTicket.TicketId,
                srNumber = dto.SrNumber,
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

    /// อ่าน snapshot (CmpId, TicketId, UserLogin) → 200 JSON เดิมที่เคย save · ไม่มี = 204
    [HttpGet("onsite/{id}/progress")]
    public async Task<IActionResult> GetOnsiteProgress(
        string id,
        [FromQuery] string? cmpid,
        [FromQuery] string? user)
    {
        if (string.IsNullOrWhiteSpace(cmpid) || string.IsNullOrWhiteSpace(user))
            return BadRequest(new { message = "cmpid and user are required" });

        var row = await _context.NisOnsiteProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CmpId == cmpid && p.TicketId == id && p.UserLogin == user);

        if (row == null || string.IsNullOrEmpty(row.SnapshotJson)) return NoContent();
        // คืน raw JSON ตรงที่ client save ไว้ (ไม่ re-serialize — กัน field เพี้ยน/หาย)
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
            ticket.Pct = ComputeOnsiteProgressPct(body);
            ticket.UpdatedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
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

    /// คำนวณ % ความคืบหน้างาน onsite จาก snapshot draft (milestone-based)
    /// น้ำหนัก: check-in 20 · checklist 40 ตามสัดส่วนที่ติ๊ก · รายละเอียดงาน 10 · รูป 10 · ลายเซ็น 10
    /// check-out แล้ว = อย่างน้อย 90 (convention เดียวกับ RN/CRM) · เพดาน 90 — 100 เฉพาะ flow ปิดงาน
    private static int ComputeOnsiteProgressPct(JsonElement s)
    {
        var pct = 0;

        if (!string.IsNullOrEmpty(ReadJsonString(s, "checkInTime"))) pct += 20;

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
            if (total > 0) pct += (int)Math.Round(40.0 * done / total);
        }

        if (!string.IsNullOrWhiteSpace(ReadJsonString(s, "workDetail"))) pct += 10;

        var hasPhoto =
            (s.TryGetProperty("photos", out var photos)
                && photos.ValueKind == JsonValueKind.Array && photos.GetArrayLength() > 0)
            || (s.TryGetProperty("rackPhotos", out var racks)
                && racks.ValueKind == JsonValueKind.Array && racks.GetArrayLength() > 0);
        if (hasPhoto) pct += 10;

        if (!string.IsNullOrEmpty(ReadJsonString(s, "signatureImg"))) pct += 10;

        if (!string.IsNullOrEmpty(ReadJsonString(s, "checkOutTime"))) pct = Math.Max(pct, 90);

        return Math.Min(pct, 90);
    }

    /// อ่าน string property จาก JsonElement แบบปลอดภัย (ไม่มี/ไม่ใช่ string → null)
    private static string? ReadJsonString(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;

}
