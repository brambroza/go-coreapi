using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/leave-requests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly HrDbContext _db; // ✅ ใช้ DbContext ของคุณเอง (เปลี่ยนเป็นชื่อจริง เช่น AppDbContext)
    public LeaveRequestsController(HrDbContext db) => _db = db;

    // helper เพื่อ cast DbSet แบบปลอดภัย
    private DbSet<LeaveRequest> LeaveRequests => _db.Set<LeaveRequest>();
    private DbSet<LeaveType> LeaveTypes => _db.Set<LeaveType>();

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft","Pending","Approved","Rejected","Cancelled"
    };

    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? employeeId,
        [FromQuery] string? status
    )
    {
        var q = LeaveRequests.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (from.HasValue) q = q.Where(x => x.DateTo >= from.Value);
        if (to.HasValue) q = q.Where(x => x.DateFrom <= to.Value);
        if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            q = q.Where(x => x.Status == status);
        }

        var data = await q
            .OrderByDescending(x => x.LeaveId)
            .Select(x => new
            {
                x.LeaveId,
                x.EmployeeId,
                x.LeaveTypeId,
                LeaveTypeName = x.LeaveType != null ? x.LeaveType.Name : null,
                x.DateFrom,
                x.DateTo,
                x.TimeFrom,
                x.TimeTo,
                x.Status,
                x.ApproverEmployeeId,
                x.Reason,
                x.AttachmentUrl,
                x.CreatedAt,
                x.UpdatedAt
            })
            // OK: Include ก่อน Select จะชัวร์ แต่แบบนี้ยังทำงานได้ (เพราะยังเป็น entity ก่อน Select ใน EF Core)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(string cmpId, long id)
    {
        var x = await LeaveRequests.AsNoTracking()
            .Include(r => r.LeaveType)
            .FirstOrDefaultAsync(r => r.CmpId == cmpId && r.LeaveId == id);

        if (x is null) return NotFound();

        return Ok(new
        {
            x.LeaveId,
            x.CmpId,
            x.EmployeeId,
            x.LeaveTypeId,
            LeaveTypeName = x.LeaveType?.Name,
            x.DateFrom,
            x.DateTo,
            x.TimeFrom,
            x.TimeTo,
            x.Status,
            x.ApproverEmployeeId,
            x.Reason,
            x.AttachmentUrl,
            x.CreatedAt,
            x.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] LeaveRequestCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        // Validate date range
        if (dto.DateFrom > dto.DateTo) return BadRequest("DateFrom must be <= DateTo");

        // Validate time range if provided
        if ((dto.TimeFrom.HasValue ^ dto.TimeTo.HasValue))
            return BadRequest("TimeFrom and TimeTo must both be provided (or both null)");

        if (dto.TimeFrom.HasValue && dto.TimeTo.HasValue && dto.TimeFrom.Value >= dto.TimeTo.Value)
            return BadRequest("TimeFrom must be < TimeTo");

        // Validate LeaveType in same company
        var leaveTypeOk = await LeaveTypes.AnyAsync(x => x.CmpId == cmpId && x.LeaveTypeId == dto.LeaveTypeId && x.IsActive);
        if (!leaveTypeOk) return BadRequest("LeaveTypeId not found/active for this cmpId");

        var entity = new LeaveRequest
        {
            CmpId = cmpId,
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            DateFrom = dto.DateFrom,
            DateTo = dto.DateTo,
            TimeFrom = dto.TimeFrom,
            TimeTo = dto.TimeTo,
            Reason = dto.Reason,
            AttachmentUrl = dto.AttachmentUrl,
            Status = "Pending"
        };

        LeaveRequests.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.LeaveId }, new { entity.LeaveId });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(string cmpId, long id, [FromBody] LeaveRequestUpdateDto dto)
    {
        var entity = await LeaveRequests.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.LeaveId == id);
        if (entity is null) return NotFound();

        // ป้องกันแก้หลังอนุมัติ (ปรับ policy ได้)
        if (entity.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            return Conflict("Cannot edit an approved leave request.");

        if (dto.DateFrom > dto.DateTo) return BadRequest("DateFrom must be <= DateTo");
        if ((dto.TimeFrom.HasValue ^ dto.TimeTo.HasValue))
            return BadRequest("TimeFrom and TimeTo must both be provided (or both null)");
        if (dto.TimeFrom.HasValue && dto.TimeTo.HasValue && dto.TimeFrom.Value >= dto.TimeTo.Value)
            return BadRequest("TimeFrom must be < TimeTo");

        var leaveTypeOk = await LeaveTypes.AnyAsync(x => x.CmpId == cmpId && x.LeaveTypeId == dto.LeaveTypeId && x.IsActive);
        if (!leaveTypeOk) return BadRequest("LeaveTypeId not found/active for this cmpId");

        entity.LeaveTypeId = dto.LeaveTypeId;
        entity.DateFrom = dto.DateFrom;
        entity.DateTo = dto.DateTo;
        entity.TimeFrom = dto.TimeFrom;
        entity.TimeTo = dto.TimeTo;
        entity.Reason = dto.Reason;
        entity.AttachmentUrl = dto.AttachmentUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(string cmpId, long id)
    {
        var entity = await LeaveRequests.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.LeaveId == id);
        if (entity is null) return NotFound();

        // policy: ลบได้เฉพาะ Draft/Pending/Rejected
        if (entity.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            return Conflict("Cannot delete an approved leave request.");

        LeaveRequests.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ✅ Approve/Reject/Cancel
    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(string cmpId, long id, [FromBody] LeaveApproveDto dto)
    {
        var entity = await LeaveRequests.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.LeaveId == id);
        if (entity is null) return NotFound();

        var action = (dto.Action ?? "").Trim();

        if (action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
        {
            if (!entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return Conflict("Only Pending can be approved.");


            entity.Status = "Approved";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;

            // (optional) เก็บหมายเหตุใน Reason ต่อท้าย หรือทำฟิลด์ ApproveNote แยกต่างหาก
            if (!string.IsNullOrWhiteSpace(dto.Note))
                entity.Reason = (entity.Reason ?? "") + "\n[APPROVE NOTE] " + dto.Note;
        }
        else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            if (!entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return Conflict("Only Pending can be rejected.");

            entity.Status = "Rejected";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;

            if (!string.IsNullOrWhiteSpace(dto.Note))
                entity.Reason = (entity.Reason ?? "") + "\n[REJECT NOTE] " + dto.Note;
        }
        else if (action.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
        {
            // policy: Cancel ได้ถ้า Pending/Approved (แล้วแต่ธุรกิจ)
            if (!(entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                  entity.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                return Conflict("Only Pending/Approved can be cancelled.");

            entity.Status = "Cancelled";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;

            if (!string.IsNullOrWhiteSpace(dto.Note))
                entity.Reason = (entity.Reason ?? "") + "\n[CANCEL NOTE] " + dto.Note;
        }
        else
        {
            return BadRequest("Action must be Approve | Reject | Cancel");
        }

        await _db.SaveChangesAsync();

        // ✅ จุดเชื่อมสำคัญ: หลัง Approve/Reject อาจ trigger ให้คำนวณ attendance ใหม่ (ทำเป็น background job)
        // e.g. enqueue recalculation for (EmployeeId, DateFrom..DateTo)

        return Ok(new { entity.LeaveId, entity.Status, entity.ApproverEmployeeId });
    }
}
