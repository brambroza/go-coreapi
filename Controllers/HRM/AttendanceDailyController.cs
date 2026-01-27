using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/attendance-daily")]
public class AttendanceDailyController : ControllerBase
{
    private readonly HrDbContext _db;
    public AttendanceDailyController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? employeeId,
        [FromQuery] string? status
    )
    {
        var q = _db.AttendanceDaily.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (from.HasValue) q = q.Where(x => x.WorkDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.WorkDate <= to.Value);
        if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);

        var data = await q
            .OrderByDescending(x => x.WorkDate)
            .ThenBy(x => x.EmployeeId)
            .Select(x => new
            {
                x.AttendanceId,
                x.EmployeeId,
                x.WorkDate,
                x.ShiftId,
                x.InTime,
                x.OutTime,
                x.WorkMin,
                x.BreakMin,
                x.LateMin,
                x.EarlyLeaveMin,
                x.OTMinBeforeShift,
                x.OTMinAfterShift,
                x.OTMinTotal,
                x.Status,
                x.Note,
                x.CalcVersion,
                x.CalcAt,
                x.CalcBy,
                x.CreatedAt,
                x.UpdatedAt,
                x.RowVer
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.AttendanceDaily.AsNoTracking()
            .FirstOrDefaultAsync(a => a.CmpId == cmpId && a.AttendanceId == id);

        if (x is null) return NotFound();

        return Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] AttendanceDailyCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        var entity = new AttendanceDaily
        {
            CmpId = cmpId,
            EmployeeId = dto.EmployeeId,
            WorkDate = dto.WorkDate,
            ShiftId = dto.ShiftId,
            InTime = dto.InTime,
            OutTime = dto.OutTime,

            WorkMin = dto.WorkMin,
            BreakMin = dto.BreakMin,
            LateMin = dto.LateMin,
            EarlyLeaveMin = dto.EarlyLeaveMin,
            OTMinBeforeShift = dto.OTMinBeforeShift,
            OTMinAfterShift = dto.OTMinAfterShift,
            OTMinTotal = dto.OTMinTotal ?? (dto.OTMinBeforeShift + dto.OTMinAfterShift),

            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Unknown" : dto.Status.Trim(),
            Note = dto.Note,

            CalcVersion = dto.CalcVersion <= 0 ? 1 : dto.CalcVersion,
            CalcAt = dto.CalcAt ?? DateTime.UtcNow,
            CalcBy = dto.CalcBy
        };

        _db.AttendanceDaily.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.AttendanceId }, new { entity.AttendanceId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] AttendanceDailyUpdateDto dto)
    {
        var entity = await _db.AttendanceDaily.FirstOrDefaultAsync(a => a.CmpId == cmpId && a.AttendanceId == id);
        if (entity is null) return NotFound();

        if (dto.RowVer is not null && dto.RowVer.Length > 0)
            _db.Entry(entity).Property(x => x.RowVer).OriginalValue = dto.RowVer;

        entity.EmployeeId = dto.EmployeeId;
        entity.WorkDate = dto.WorkDate;
        entity.ShiftId = dto.ShiftId;
        entity.InTime = dto.InTime;
        entity.OutTime = dto.OutTime;

        entity.WorkMin = dto.WorkMin;
        entity.BreakMin = dto.BreakMin;
        entity.LateMin = dto.LateMin;
        entity.EarlyLeaveMin = dto.EarlyLeaveMin;
        entity.OTMinBeforeShift = dto.OTMinBeforeShift;
        entity.OTMinAfterShift = dto.OTMinAfterShift;
        entity.OTMinTotal = dto.OTMinTotal ?? (dto.OTMinBeforeShift + dto.OTMinAfterShift);

        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? entity.Status : dto.Status.Trim();
        entity.Note = dto.Note;

        entity.CalcVersion = dto.CalcVersion <= 0 ? entity.CalcVersion : dto.CalcVersion;
        entity.CalcAt = dto.CalcAt ?? entity.CalcAt;
        entity.CalcBy = dto.CalcBy;

        try
        {
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("RowVer mismatch. Please reload and try again.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.AttendanceDaily.FirstOrDefaultAsync(a => a.CmpId == cmpId && a.AttendanceId == id);
        if (entity is null) return NotFound();

        _db.AttendanceDaily.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
