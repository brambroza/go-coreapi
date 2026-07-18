using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/attendance-punches")]
public class AttendancePunchesController : ControllerBase
{
    private readonly HrDbContext _db;
    public AttendancePunchesController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId, [FromQuery] long? attendanceId)
    {
        var q = _db.AttendancePunches.AsNoTracking().Where(x => x.CmpId == cmpId);
        if (attendanceId.HasValue) q = q.Where(x => x.AttendanceId == attendanceId.Value);

        var data = await q.OrderByDescending(x => x.PunchTime).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.AttendancePunches.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CmpId == cmpId && p.PunchId == id);

        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] AttendancePunchCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        var entity = new AttendancePunch
        {
            CmpId = cmpId,
            AttendanceId = dto.AttendanceId,
            PunchTime = dto.PunchTime,
            PunchType = dto.PunchType.Trim(),
            Source = string.IsNullOrWhiteSpace(dto.Source) ? "RawLog" : dto.Source.Trim(),
            RawLogId = dto.RawLogId
        };

        _db.AttendancePunches.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.PunchId }, new { entity.PunchId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] AttendancePunchUpdateDto dto)
    {
        var entity = await _db.AttendancePunches.FirstOrDefaultAsync(p => p.CmpId == cmpId && p.PunchId == id);
        if (entity is null) return NotFound();

        entity.AttendanceId = dto.AttendanceId;
        entity.PunchTime = dto.PunchTime;
        entity.PunchType = dto.PunchType.Trim();
        entity.Source = dto.Source.Trim();
        entity.RawLogId = dto.RawLogId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.AttendancePunches.FirstOrDefaultAsync(p => p.CmpId == cmpId && p.PunchId == id);
        if (entity is null) return NotFound();

        _db.AttendancePunches.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
