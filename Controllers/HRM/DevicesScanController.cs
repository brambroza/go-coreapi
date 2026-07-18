using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/devices-scan")]
public class DevicesScanController : ControllerBase
{
    private readonly HrDbContext _db;
    public DevicesScanController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId)
    {
        var data = await _db.DevicesScan
            .AsNoTracking()
            .Where(x => x.CmpId == cmpId)
            .OrderBy(x => x.DeviceId)
            .Select(x => new
            {
                x.DeviceId,
                x.Name,
                x.BrandModel,
                x.Host,
                x.Port,
                x.ProtocolType,
                x.Timezone,
                x.Location,
                x.SyncIntervalSec,
                x.IsActive,
                x.Status,
                x.LastSeenAt,
                x.LastSyncAt,
                x.Notes,
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
        var x = await _db.DevicesScan
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.CmpId == cmpId && d.DeviceId == id);

        if (x is null) return NotFound();

        return Ok(new
        {
            x.DeviceId,
            x.CmpId,
            x.Name,
            x.BrandModel,
            x.Host,
            x.Port,
            x.ProtocolType,
            x.Timezone,
            x.Location,
            x.SyncIntervalSec,
            x.IsActive,
            x.Status,
            x.LastSeenAt,
            x.LastSyncAt,
            x.Notes,
            x.CreatedAt,
            x.UpdatedAt,
            x.RowVer
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] DeviceScanCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        var entity = new DeviceScan
        {
            CmpId = cmpId,
            Name = dto.Name.Trim(),
            BrandModel = dto.BrandModel,
            Host = dto.Host.Trim(),
            Port = dto.Port,
            ProtocolType = dto.ProtocolType.Trim(),
            Timezone = dto.Timezone.Trim(),
            Location = dto.Location,
            SyncIntervalSec = dto.SyncIntervalSec,
            IsActive = dto.IsActive,
            Status = dto.Status.Trim(),
            LastSeenAt = dto.LastSeenAt,
            LastSyncAt = dto.LastSyncAt,
            Notes = dto.Notes
        };

        _db.DevicesScan.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.DeviceId }, new { entity.DeviceId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] DeviceScanUpdateDto dto)
    {
        var entity = await _db.DevicesScan.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.DeviceId == id);
        if (entity is null) return NotFound();

        if (dto.RowVer is not null && dto.RowVer.Length > 0)
            _db.Entry(entity).Property(x => x.RowVer).OriginalValue = dto.RowVer;

        entity.Name = dto.Name.Trim();
        entity.BrandModel = dto.BrandModel;
        entity.Host = dto.Host.Trim();
        entity.Port = dto.Port;
        entity.ProtocolType = dto.ProtocolType.Trim();
        entity.Timezone = dto.Timezone.Trim();
        entity.Location = dto.Location;
        entity.SyncIntervalSec = dto.SyncIntervalSec;
        entity.IsActive = dto.IsActive;
        entity.Status = dto.Status.Trim();
        entity.LastSeenAt = dto.LastSeenAt;
        entity.LastSyncAt = dto.LastSyncAt;
        entity.Notes = dto.Notes;

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
        var entity = await _db.DevicesScan.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.DeviceId == id);
        if (entity is null) return NotFound();

        _db.DevicesScan.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
