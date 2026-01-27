using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;
using System.Drawing;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/holidays")]
public class HolidaysController : ControllerBase
{
    private readonly HrDbContext _db; // ✅ เปลี่ยนเป็น DbContext ของคุณจริง
    public HolidaysController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to
    )
    {
        var q = _db.HolidayCalendars.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (from.HasValue) q = q.Where(x => x.HolidayDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.HolidayDate <= to.Value);

        var data = await q
            .OrderBy(x => x.HolidayDate)
            .Select(x => new
            {
                x.HolidayId,
                x.HolidayDate,
                x.Name,
                x.IsCompanyHoliday,
                x.Notes,
                x.CmpId,
                x.Color
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string cmpId, string id)
    {
        var x = await _db.HolidayCalendars.AsNoTracking()
            .FirstOrDefaultAsync(h => h.CmpId == cmpId && h.HolidayId == id);

        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] HolidayCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");

        // เช็คซ้ำ (กัน error 500 จาก unique index)
        var exists = await _db.HolidayCalendars.AnyAsync(x =>
            x.CmpId == cmpId && x.HolidayId == dto.HolidayId  && x.HolidayDate == dto.HolidayDate);

        if (exists) return Conflict("HolidayDate already exists.");

        var entity = new HolidayCalendar
        {
            HolidayId = dto.HolidayId,
            CmpId = cmpId,
            HolidayDate = dto.HolidayDate,
            Name = dto.Name.Trim(),
            IsCompanyHoliday = dto.IsCompanyHoliday,
            Notes = dto.Notes,
            Color = dto.Color
        };

        _db.HolidayCalendars.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // เผื่อโดน unique index ที่ DB (กรณี race condition)
            return Conflict("HolidayDate already exists.");
        }

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.HolidayId }, new { entity.HolidayId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string cmpId, string id, [FromBody] HolidayUpdateDto dto)
    {
        var entity = await _db.HolidayCalendars.FirstOrDefaultAsync(h => h.CmpId == cmpId && h.HolidayId == id);
        if (entity is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");

        // ถ้าเปลี่ยนวัน ให้เช็คซ้ำ
        if (entity.HolidayDate != dto.HolidayDate)
        {
            var exists = await _db.HolidayCalendars.AnyAsync(x =>
                x.CmpId == cmpId &&
                x.HolidayDate == dto.HolidayDate &&
                x.HolidayId != id);

            if (exists) return Conflict("HolidayDate already exists.");
        }

        entity.HolidayDate = dto.HolidayDate;
        entity.Name = dto.Name.Trim();
        entity.IsCompanyHoliday = dto.IsCompanyHoliday;
        entity.Notes = dto.Notes;
        entity.Color = dto.Color;

        try
        {
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict("HolidayDate already exists.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string cmpId, string id)
    {
        var entity = await _db.HolidayCalendars.FirstOrDefaultAsync(h => h.CmpId == cmpId && h.HolidayId == id);
        if (entity is null) return NotFound();

        _db.HolidayCalendars.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
