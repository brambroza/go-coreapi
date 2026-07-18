using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceModeController : ControllerBase
{
    private readonly DatabaseContext _context;

    public ServiceModeController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MServiceMode>>> GetAll(
        [FromQuery] string cmpId)
    {
        var query = _context.MServiceModes
            .AsNoTracking()
            .Where(x => x.CmpId == cmpId);



        var result = await query
            .OrderBy(x => x.ServiceModeId)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{cmpId}/{serviceModeId}")]
    public async Task<ActionResult<MServiceMode>> GetById(
        string cmpId,
        string serviceModeId)
    {
        var entity = await _context.MServiceModes
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CmpId == cmpId &&
                x.ServiceModeId == serviceModeId);

        if (entity == null)
        {
            return NotFound(new { message = "Service mode not found" });
        }

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<MServiceMode>> Create([FromBody] ServiceModeDto dto)
    {
        var exists = await _context.MServiceModes.AnyAsync(x =>
            x.CmpId == dto.CmpId &&
            x.ServiceModeId == dto.ServiceModeId);

        if (exists)
        {
            return Conflict(new { message = "ServiceModeId already exists" });
        }

        var now = DateTime.Now;

        var entity = new MServiceMode
        {
            CmpId = dto.CmpId,
            ServiceModeId = dto.ServiceModeId,
            Descriptions = dto.Descriptions,
            StateActive = dto.StateActive ?? 1,
            UpdUser = dto.UpdUser,
            UpdDate = now.Date,
            UpdTime = now.TimeOfDay
        };

        _context.MServiceModes.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(entity);
    }

    [HttpPut("{cmpId}/{serviceModeId}")]
    public async Task<ActionResult<MServiceMode>> Update(
        string cmpId,
        string serviceModeId,
        [FromBody] ServiceModeDto dto)
    {
        var entity = await _context.MServiceModes
            .FirstOrDefaultAsync(x =>
                x.CmpId == cmpId &&
                x.ServiceModeId == serviceModeId);

        if (entity == null)
        {
            return NotFound(new { message = "Service mode not found" });
        }

        var now = DateTime.Now;

        entity.Descriptions = dto.Descriptions;
        entity.StateActive = dto.StateActive;
        entity.UpdUser = dto.UpdUser;
        entity.UpdDate = now.Date;
        entity.UpdTime = now.TimeOfDay;

        await _context.SaveChangesAsync();

        return Ok(entity);
    }

    [HttpDelete("{cmpId}/{serviceModeId}")]
    public async Task<IActionResult> Delete(
        string cmpId,
        string serviceModeId)
    {
        var entity = await _context.MServiceModes
            .FirstOrDefaultAsync(x =>
                x.CmpId == cmpId &&
                x.ServiceModeId == serviceModeId);

        if (entity == null)
        {
            return NotFound(new { message = "Service mode not found" });
        }

        _context.MServiceModes.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }

}