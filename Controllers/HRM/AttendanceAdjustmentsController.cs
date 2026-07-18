using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/attendance-adjustments")]
public class AttendanceAdjustmentsController : ControllerBase
{
    private readonly HrDbContext _db;
    public AttendanceAdjustmentsController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId, [FromQuery] long? attendanceId)
    {
        var q = _db.AttendanceAdjustments.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (attendanceId.HasValue) q = q.Where(x => x.AttendanceId == attendanceId.Value);

        var data = await q.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.AttendanceAdjustments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.CmpId == cmpId && a.AdjustId == id);

        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] AttendanceAdjustmentCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        var entity = new AttendanceAdjustment
        {
            CmpId = cmpId,
            AttendanceId = dto.AttendanceId,
            FieldChanged = dto.FieldChanged.Trim(),
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            Reason = dto.Reason,
            CreatedBy = dto.CreatedBy
        };

        _db.AttendanceAdjustments.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.AdjustId }, new { entity.AdjustId });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.AttendanceAdjustments.FirstOrDefaultAsync(a => a.CmpId == cmpId && a.AdjustId == id);
        if (entity is null) return NotFound();

        _db.AttendanceAdjustments.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
