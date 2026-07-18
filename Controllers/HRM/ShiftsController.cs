using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/shifts")]
public class ShiftsController : ControllerBase
{
    private readonly HrDbContext _db;
    public ShiftsController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId)
    {
        var data = await _db.Shifts
            .AsNoTracking()
            .Where(x => x.CmpId == cmpId)
            .OrderBy(x => x.ShiftId)
            .Select(x => new
            {
                x.ShiftId,
                x.Name,
                x.StartTime,
                x.EndTime,
                x.CrossMidnight,
                x.ScanTypeId,
                ScanTypeName = x.ScanType != null ? x.ScanType.Name : null,
                x.GraceLateMin,
                x.GraceEarlyLeaveMin,
                x.MinWorkMinForPresent,
                x.IsActive,
                x.RowVer
            }) 
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.Shifts
            .AsNoTracking()
            .Include(s => s.ScanType)
            .FirstOrDefaultAsync(s => s.CmpId == cmpId && s.ShiftId == id);

        if (x is null) return NotFound();

        return Ok(new
        {
            x.ShiftId,
            x.CmpId,
            x.Name,
            x.StartTime,
            x.EndTime,
            x.CrossMidnight,
            x.ScanTypeId,
            ScanTypeName = x.ScanType?.Name,
            x.GraceLateMin,
            x.GraceEarlyLeaveMin,
            x.MinWorkMinForPresent,
            x.IsActive,
            x.RowVer
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] ShiftCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        // validate scantype in same cmp
        if (dto.ScanTypeId.HasValue)
        {
            var ok = await _db.ScanTypes.AnyAsync(x => x.CmpId == cmpId && x.ScanTypeId == dto.ScanTypeId.Value);
            if (!ok) return BadRequest("ScanTypeId not found for this cmpId");
        }

        var entity = new Shift
        {
            CmpId = cmpId,
            Name = dto.Name.Trim(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CrossMidnight = dto.CrossMidnight,
            ScanTypeId = dto.ScanTypeId,
            GraceLateMin = dto.GraceLateMin,
            GraceEarlyLeaveMin = dto.GraceEarlyLeaveMin,
            MinWorkMinForPresent = dto.MinWorkMinForPresent,
            IsActive = dto.IsActive
        };

        _db.Shifts.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.ShiftId }, new { entity.ShiftId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] ShiftUpdateDto dto)
    {
        var entity = await _db.Shifts.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.ShiftId == id);
        if (entity is null) return NotFound();

        if (dto.ScanTypeId.HasValue)
        {
            var ok = await _db.ScanTypes.AnyAsync(x => x.CmpId == cmpId && x.ScanTypeId == dto.ScanTypeId.Value);
            if (!ok) return BadRequest("ScanTypeId not found for this cmpId");
        }

        // optimistic concurrency (RowVer)
        if (dto.RowVer is not null && dto.RowVer.Length > 0)
            _db.Entry(entity).Property(x => x.RowVer).OriginalValue = dto.RowVer;

        entity.Name = dto.Name.Trim();
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.CrossMidnight = dto.CrossMidnight;
        entity.ScanTypeId = dto.ScanTypeId;
        entity.GraceLateMin = dto.GraceLateMin;
        entity.GraceEarlyLeaveMin = dto.GraceEarlyLeaveMin;
        entity.MinWorkMinForPresent = dto.MinWorkMinForPresent;
        entity.IsActive = dto.IsActive;

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
        var entity = await _db.Shifts.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.ShiftId == id);
        if (entity is null) return NotFound();

        _db.Shifts.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
