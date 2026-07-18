using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/device-users-scan")]
public class DeviceUsersScanController : ControllerBase
{
    private readonly HrDbContext _db;
    public DeviceUsersScanController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] int? deviceId
    )
    {
        var q = _db.DeviceUsersScan.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (deviceId.HasValue)
            q = q.Where(x => x.DeviceId == deviceId.Value);

        var data = await q
            .OrderBy(x => x.DeviceUserId)
            .Select(x => new
            {
                x.DeviceUserId,
                x.DeviceId,
                DeviceName = x.Device != null ? x.Device.Name : null,
                x.UserCodeOnDevice,
                x.CardNo,
                x.DisplayName,
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
        var x = await _db.DeviceUsersScan.AsNoTracking()
            .FirstOrDefaultAsync(u => u.CmpId == cmpId && u.DeviceUserId == id);

        if (x is null) return NotFound();

        return Ok(new
        {
            x.DeviceUserId,
            x.CmpId,
            x.DeviceId,
            x.UserCodeOnDevice,
            x.CardNo,
            x.DisplayName,
            x.CreatedAt,
            x.UpdatedAt,
            x.RowVer
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] DeviceUserScanCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        // Validate: device ต้องอยู่ใน cmp เดียวกัน
        var deviceOk = await _db.DevicesScan.AnyAsync(d => d.CmpId == cmpId && d.DeviceId == dto.DeviceId);
        if (!deviceOk) return BadRequest("DeviceId not found for this cmpId");

        var entity = new DeviceUserScan
        {
            CmpId = cmpId,
            DeviceId = dto.DeviceId,
            UserCodeOnDevice = dto.UserCodeOnDevice.Trim(),
            CardNo = dto.CardNo?.Trim(),
            DisplayName = dto.DisplayName?.Trim()
        };

        _db.DeviceUsersScan.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.DeviceUserId }, new { entity.DeviceUserId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] DeviceUserScanUpdateDto dto)
    {
        var entity = await _db.DeviceUsersScan.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.DeviceUserId == id);
        if (entity is null) return NotFound();

        // Validate device in same cmp
        var deviceOk = await _db.DevicesScan.AnyAsync(d => d.CmpId == cmpId && d.DeviceId == dto.DeviceId);
        if (!deviceOk) return BadRequest("DeviceId not found for this cmpId");

        if (dto.RowVer is not null && dto.RowVer.Length > 0)
            _db.Entry(entity).Property(x => x.RowVer).OriginalValue = dto.RowVer;

        entity.DeviceId = dto.DeviceId;
        entity.UserCodeOnDevice = dto.UserCodeOnDevice.Trim();
        entity.CardNo = dto.CardNo?.Trim();
        entity.DisplayName = dto.DisplayName?.Trim();

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
        var entity = await _db.DeviceUsersScan.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.DeviceUserId == id);
        if (entity is null) return NotFound();

        _db.DeviceUsersScan.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
