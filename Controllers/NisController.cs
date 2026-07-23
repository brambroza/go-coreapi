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
    private readonly NisReportPdfStorage _pdfStorage;
    private readonly ILogger<NisController> _logger;

    /// Feature flag (NisOnsite:AttachReportPdf, default true) — lets ops disable PDF attachment
    /// instantly if the shared mail path misbehaves, without a redeploy.
    private readonly bool _attachReportPdf;

    public NisController(
        DatabaseContext context,
        EmailSettingRepository emailRepo,
        AesCrypto crypto,
        GoogleOAuthMailService googleOAuthMail,
        GoogleOAuthCalendarService googleOAuthCalendar,
        goalongapi.Services.ExpoPushService push,
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
        _pdfStorage = pdfStorage;
        _logger = logger;
        _attachReportPdf = configuration.GetValue<bool?>("NisOnsite:AttachReportPdf") ?? true;
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
    };

    private static NisProjectResponseDto MapProject(NisProject p) => new()
    {
        Id = p.ProjectId,
        ProjectNo = FormatProjectNo(p.ProjectNo),
        Name = p.Name,
        Customer = p.Customer,
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

    // ── PUT api/nis/tickets/{id}/close-approve ───────────────────────────────

    /// <summary>Manager approves a "Waiting Close Approval" ticket → Closed. Matches frontend approveNisCloseTicket.</summary>
    [HttpPut("tickets/{id}/close-approve")]
    public async Task<IActionResult> ApproveCloseTicket(string id, [FromBody] NisTicketCloseDto? dto)
    {
        var ticket = await _context.NisTickets.FindAsync(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket {id} not found" });

        ticket.Status = "Closed";
        ticket.Pct = 100;
        ticket.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Ticket closed" });
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

        var project = await _context.NisProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

        if (project == null)
            return BadRequest(new { message = "Request has no valid project — cannot create a ticket" });

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
            CmpId = dto.CmpId ?? request.CmpId,
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
        SlaOptions = SplitTags(e.SlaOptionsRaw),
        WarningDays = new NisWarningDaysDto
        {
            Service = e.WarningDaysService,
            Product = e.WarningDaysProduct,
        },
    };

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
        SlaOptions = ["8x5xNBD", "8x5", "24x7x4", "24x7xNBD"],
        WarningDays = new NisWarningDaysDto { Service = 60, Product = 30 },
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
        var cmpId = dto.CmpId ?? string.Empty;
        var prefix = $"SR-{DateTime.Now:yyyyMM}-";

        // Count across both the ServiceTicket and NIS report tables so SR numbers
        // stay unique regardless of which onsite flow generated them.
        var svcCount = await _context.ServiceTicketSubTaskActions
            .Where(a => a.CmpId == cmpId && a.SrNumber != null && a.SrNumber.StartsWith(prefix))
            .CountAsync();

        var nisCount = await _context.NisOnsiteReports
            .Where(r => r.CmpId == cmpId && r.SrNumber != null && r.SrNumber.StartsWith(prefix))
            .CountAsync();

        return Ok(new { srNumber = $"{prefix}{(svcCount + nisCount + 1):D4}" });
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
                    var subject = string.IsNullOrWhiteSpace(dto.EmailSubject) ? $"[Service Report] {dto.SrNumber}" : dto.EmailSubject;
                    var body = BuildOnsiteReportEmailBody(nisTicket.TicketCode ?? nisTicket.TicketId, customerName ?? string.Empty, dto);
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
                var subject = string.IsNullOrWhiteSpace(dto.EmailSubject) ? $"[Service Report] {dto.SrNumber}" : dto.EmailSubject;
                emailSent = await SendOnsiteEmailAsync(cmpId, dto.RecipientEmail, subject, BuildOnsiteReportEmailBody(ticket.TicketNo ?? ticket.TicketId, ticket.CustomerName, dto));
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

    private static string BuildOnsiteReportEmailBody(string ticketNo, string customerName, NisOnsiteSubmitDto dto)
    {
        var signatureSection = dto.SkipSignature
            ? "<p style=\"color:#64748b;\">* ลูกค้าไม่ได้ลงนาม (skipped)</p>"
            : !string.IsNullOrWhiteSpace(dto.SignatureImg)
                ? $"<p style=\"font-weight:600;\">ลายเซ็นลูกค้า:</p><img src=\"{dto.SignatureImg}\" style=\"max-width:300px;border:1px solid #e2e8f0;border-radius:6px;padding:4px;\" />"
                : "";

        return $"""
            <div style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:24px;">
              <h2 style="color:#312e81;margin-bottom:4px;">NIS Service Report</h2>
              <p style="color:#64748b;margin-top:0;">SR: {dto.SrNumber}</p>
              <hr style="border:none;border-top:1px solid #e2e8f0;margin:16px 0;" />
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
              <p style="color:#64748b;font-size:12px;">This is an automated email from GoAlong NIS System. Please do not reply to this email.</p>
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
