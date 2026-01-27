using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/ot-requests")]
public class OTRequestsController : ControllerBase
{
    private readonly HrDbContext _db;
    public OTRequestsController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? employeeId,
        [FromQuery] string? status
    )
    {
        var q = _db.OTRequests.AsNoTracking().Where(x => x.CmpId == cmpId);

        if (from.HasValue) q = q.Where(x => x.WorkDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.WorkDate <= to.Value);
        if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);

        var data = await q.OrderByDescending(x => x.OTId).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.OTRequests.AsNoTracking()
            .FirstOrDefaultAsync(o => o.CmpId == cmpId && o.OTId == id);

        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] OTCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");
        if (dto.TimeFrom >= dto.TimeTo) return BadRequest("TimeFrom must be < TimeTo");

        var entity = new OTRequest
        {
            CmpId = cmpId,
            EmployeeId = dto.EmployeeId,
            WorkDate = dto.WorkDate,
            TimeFrom = dto.TimeFrom,
            TimeTo = dto.TimeTo,
            OTType = dto.OTType,
            Reason = dto.Reason,
            Status = "Pending"
        };

        _db.OTRequests.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.OTId }, new { entity.OTId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] OTUpdateDto dto)
    {
        var entity = await _db.OTRequests.FirstOrDefaultAsync(o => o.CmpId == cmpId && o.OTId == id);
        if (entity is null) return NotFound();

        if (dto.TimeFrom >= dto.TimeTo) return BadRequest("TimeFrom must be < TimeTo");

        entity.EmployeeId = dto.EmployeeId;
        entity.WorkDate = dto.WorkDate;
        entity.TimeFrom = dto.TimeFrom;
        entity.TimeTo = dto.TimeTo;
        entity.OTType = dto.OTType;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? entity.Status : dto.Status.Trim();
        entity.ApproverEmployeeId = dto.ApproverEmployeeId;
        entity.Reason = dto.Reason;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.OTRequests.FirstOrDefaultAsync(o => o.CmpId == cmpId && o.OTId == id);
        if (entity is null) return NotFound();

        _db.OTRequests.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ✅ Approve/Reject/Cancel
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(string cmpId, int id, [FromBody] OTApproveDto dto)
    {
        var entity = await _db.OTRequests.FirstOrDefaultAsync(o => o.CmpId == cmpId && o.OTId == id);
        if (entity is null) return NotFound();

        var action = (dto.Action ?? "").Trim();

        if (action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
        {
            if (!entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return Conflict("Only Pending can be approved.");

            entity.Status = "Approved";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;
        }
        else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            if (!entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return Conflict("Only Pending can be rejected.");

            entity.Status = "Rejected";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;
        }
        else if (action.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
        {
            // policy ปรับได้ตามจริง
            if (!(entity.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                  entity.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                return Conflict("Only Pending/Approved can be cancelled.");

            entity.Status = "Cancelled";
            entity.ApproverEmployeeId = dto.ApproverEmployeeId;
        }
        else
        {
            return BadRequest("Action must be Approve | Reject | Cancel");
        }

        if (!string.IsNullOrWhiteSpace(dto.Note))
            entity.Reason = (entity.Reason ?? "") + "\n[APPROVE NOTE] " + dto.Note;

        await _db.SaveChangesAsync();

        // จุดเชื่อมต่อ: หลังอนุมัติ OT อาจต้อง trigger recalculation attendance ใน WorkDate
        return Ok(new { entity.OTId, entity.Status, entity.ApproverEmployeeId });
    }
}
