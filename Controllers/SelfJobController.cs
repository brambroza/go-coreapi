// REQ-004 — Self-Job Request Controller
// Engineer สร้างคำของาน → Manager อนุมัติ → auto-create ServiceTicket + SubTask

using goalongapi.Data;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SelfJobController : ControllerBase
{
    private readonly DatabaseContext _context;

    public SelfJobController(DatabaseContext context)
    {
        _context = context;
    }

    // ─── GET /api/SelfJob/requests ────────────────────────────────────────────
    // Query: cmpId (required), status? (optional filter), requestedBy? (filter by engineer)

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<SelfJobRequestResponseDto>>> GetRequests(
        [FromQuery] string? cmpId,
        [FromQuery] string? status,
        [FromQuery] string? requestedBy)
    {
        var query = _context.SelfJobRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(requestedBy))
            query = query.Where(x => x.RequestedBy == requestedBy);

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(data);
    }

    // ─── GET /api/SelfJob/requests/{id} ───────────────────────────────────────

    [HttpGet("requests/{id}")]
    public async Task<ActionResult<SelfJobRequestResponseDto>> GetById(string id)
    {
        var entity = await _context.SelfJobRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestId == id);

        if (entity == null) return NotFound(new { message = $"SelfJobRequest '{id}' not found." });

        return Ok(MapToResponse(entity));
    }

    // ─── POST /api/SelfJob/requests ───────────────────────────────────────────
    // Create new request with status = Draft

    [HttpPost("requests")]
    public async Task<ActionResult<SelfJobRequestResponseDto>> Create([FromBody] CreateSelfJobRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RequestTitle))
            return BadRequest(new { message = "RequestTitle is required." });
        if (string.IsNullOrWhiteSpace(dto.RequestType))
            return BadRequest(new { message = "RequestType is required." });
        if (string.IsNullOrWhiteSpace(dto.RequestDetail))
            return BadRequest(new { message = "RequestDetail is required." });
        if (string.IsNullOrWhiteSpace(dto.CmpId))
            return BadRequest(new { message = "CmpId is required." });

        var now = DateTime.Now;
        var requestId = Guid.NewGuid().ToString();
        var requestNo = $"SJR-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var entity = new SelfJobRequest
        {
            RequestId = requestId,
            RequestNo = requestNo,
            RequestTitle = dto.RequestTitle,
            RequestType = dto.RequestType,
            RequestDetail = dto.RequestDetail,
            Reason = dto.Reason,
            Status = "Draft",
            CmpId = dto.CmpId,
            CustomerCode = dto.CustomerCode,
            CustomerName = dto.CustomerName,
            SiteName = dto.SiteName,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            Priority = dto.Priority ?? "medium",
            ExpectedServiceDate = dto.ExpectedServiceDate,
            EstimatedHours = dto.EstimatedHours,
            EstimatedCost = dto.EstimatedCost,
            RequestedBy = dto.RequestedBy,
            RequestedDate = now,
            CreatedAt = now,
            UpdatedAt = now,
            UpdatedBy = dto.RequestedBy,
        };

        _context.SelfJobRequests.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = requestId }, MapToResponse(entity));
    }

    // ─── POST /api/SelfJob/requests/{id}/submit ───────────────────────────────
    // Draft → PendingApproval

    [HttpPost("requests/{id}/submit")]
    public async Task<ActionResult> Submit(string id)
    {
        var entity = await _context.SelfJobRequests.FirstOrDefaultAsync(x => x.RequestId == id);
        if (entity == null) return NotFound(new { message = $"SelfJobRequest '{id}' not found." });
        if (entity.Status != "Draft")
            return BadRequest(new { message = $"Cannot submit: current status is '{entity.Status}'. Expected 'Draft'." });

        entity.Status = "PendingApproval";
        entity.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { requestId = id, status = "PendingApproval" });
    }

    // ─── POST /api/SelfJob/requests/{id}/approve ──────────────────────────────
    // PendingApproval → Approved + auto-create ServiceTicket + SubTask

    [HttpPost("requests/{id}/approve")]
    public async Task<ActionResult> Approve(string id, [FromBody] ApproveSelfJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ApprovedBy))
            return BadRequest(new { message = "ApprovedBy is required." });

        var entity = await _context.SelfJobRequests.FirstOrDefaultAsync(x => x.RequestId == id);
        if (entity == null) return NotFound(new { message = $"SelfJobRequest '{id}' not found." });
        if (entity.Status != "PendingApproval")
            return BadRequest(new { message = $"Cannot approve: current status is '{entity.Status}'. Expected 'PendingApproval'." });

        var now = DateTime.Now;
        var ticketId = Guid.NewGuid().ToString();
        var subTaskId = Guid.NewGuid().ToString();
        var ticketNo = $"STK-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // Auto-create ServiceTicket
        var ticket = new ServiceTicket
        {
            TicketId = ticketId,
            TicketNo = ticketNo,
            CustomerCode = entity.CustomerCode ?? string.Empty,
            CustomerName = entity.CustomerName ?? string.Empty,
            AdditionalDetails = $"[Self-Job] {entity.RequestTitle}\n{entity.RequestDetail}",
            Priority = MapPriority(entity.Priority),
            JobType = "maintenance",
            Status = "draft",
            CmpId = entity.CmpId,
            UpdUser = dto.ApprovedBy,
            ServiceDate = entity.ExpectedServiceDate,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.ServiceTickets.Add(ticket);

        // Auto-create SubTask linked to the ticket
        var subTask = new ServiceTicketSubTask
        {
            SubTaskId = subTaskId,
            TicketId = ticketId,
            Seq = 1,
            Title = entity.RequestTitle,
            Name = entity.RequestTitle,
            Source = "additional",
            Status = "pending",
            TaskStatus = "pending",
            CmpId = entity.CmpId,
            StartDate = entity.ExpectedServiceDate,
            Remark = entity.Reason,
            UpdatedBy = dto.ApprovedBy,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.ServiceTicketSubTasks.Add(subTask);

        // Update SelfJobRequest
        entity.Status = "Approved";
        entity.ApprovedBy = dto.ApprovedBy;
        entity.ApprovedDate = now;
        entity.TicketId = ticketId;
        entity.SubTaskId = subTaskId;
        entity.UpdatedAt = now;
        entity.UpdatedBy = dto.ApprovedBy;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            requestId = id,
            status = "Approved",
            approvedBy = dto.ApprovedBy,
            approvedDate = now.ToString("o"),
            ticketId,
            ticketNo,
            subTaskId,
        });
    }

    // ─── POST /api/SelfJob/requests/{id}/reject ───────────────────────────────
    // PendingApproval → Rejected

    [HttpPost("requests/{id}/reject")]
    public async Task<ActionResult> Reject(string id, [FromBody] RejectSelfJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RejectedBy))
            return BadRequest(new { message = "RejectedBy is required." });
        if (string.IsNullOrWhiteSpace(dto.RejectReason))
            return BadRequest(new { message = "RejectReason is required." });

        var entity = await _context.SelfJobRequests.FirstOrDefaultAsync(x => x.RequestId == id);
        if (entity == null) return NotFound(new { message = $"SelfJobRequest '{id}' not found." });
        if (entity.Status != "PendingApproval")
            return BadRequest(new { message = $"Cannot reject: current status is '{entity.Status}'. Expected 'PendingApproval'." });

        var now = DateTime.Now;
        entity.Status = "Rejected";
        entity.RejectedBy = dto.RejectedBy;
        entity.RejectedDate = now;
        entity.RejectReason = dto.RejectReason;
        entity.UpdatedAt = now;
        entity.UpdatedBy = dto.RejectedBy;

        await _context.SaveChangesAsync();

        return Ok(new { requestId = id, status = "Rejected", rejectReason = dto.RejectReason });
    }

    // ─── POST /api/SelfJob/requests/{id}/cancel ───────────────────────────────
    // Draft | PendingApproval → Cancelled

    [HttpPost("requests/{id}/cancel")]
    public async Task<ActionResult> Cancel(string id, [FromBody] CancelSelfJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CancelledBy))
            return BadRequest(new { message = "CancelledBy is required." });

        var entity = await _context.SelfJobRequests.FirstOrDefaultAsync(x => x.RequestId == id);
        if (entity == null) return NotFound(new { message = $"SelfJobRequest '{id}' not found." });
        if (entity.Status == "Approved" || entity.Status == "Cancelled")
            return BadRequest(new { message = $"Cannot cancel from status '{entity.Status}'." });

        var now = DateTime.Now;
        entity.Status = "Cancelled";
        entity.CancelledBy = dto.CancelledBy;
        entity.CancelledDate = now;
        entity.UpdatedAt = now;
        entity.UpdatedBy = dto.CancelledBy;

        await _context.SaveChangesAsync();

        return Ok(new { requestId = id, status = "Cancelled" });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static SelfJobRequestResponseDto MapToResponse(SelfJobRequest x) => new()
    {
        RequestId = x.RequestId,
        RequestNo = x.RequestNo,
        RequestTitle = x.RequestTitle,
        RequestType = x.RequestType,
        RequestDetail = x.RequestDetail,
        Reason = x.Reason,
        Status = x.Status,
        CmpId = x.CmpId,
        CustomerCode = x.CustomerCode,
        CustomerName = x.CustomerName,
        SiteName = x.SiteName,
        ContactName = x.ContactName,
        ContactPhone = x.ContactPhone,
        Priority = x.Priority,
        ExpectedServiceDate = x.ExpectedServiceDate?.ToString("o"),
        EstimatedHours = x.EstimatedHours,
        EstimatedCost = x.EstimatedCost,
        RequestedBy = x.RequestedBy,
        RequestedDate = x.RequestedDate.ToString("o"),
        ApprovedBy = x.ApprovedBy,
        ApprovedDate = x.ApprovedDate?.ToString("o"),
        RejectedBy = x.RejectedBy,
        RejectReason = x.RejectReason,
        CancelledBy = x.CancelledBy,
        TicketId = x.TicketId,
        SubTaskId = x.SubTaskId,
        CreatedAt = x.CreatedAt.ToString("o"),
        UpdatedAt = x.UpdatedAt.ToString("o"),
    };

    /// Maps Self-Job priority (low/medium/high/urgent) → ServiceTicket priority (minor/major/critical)
    private static string MapPriority(string? priority) => priority switch
    {
        "urgent" => "critical",
        "high" => "major",
        _ => "minor",
    };
}
