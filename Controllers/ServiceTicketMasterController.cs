using goalongapi.Data;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceTicketMasterController : ControllerBase
{
    private readonly DatabaseContext _context;

    public ServiceTicketMasterController(DatabaseContext context)
    {
        _context = context;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CATEGORY
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<ServiceTicketMasterCategory>>> GetCategories(
        [FromQuery] string? cmpId,
        [FromQuery] bool? isActive)
    {
        var q = _context.ServiceTicketMasterCategories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(cmpId)) q = q.Where(x => x.CmpId == cmpId || x.CmpId == null);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return Ok(await q.OrderBy(x => x.Seq).ThenBy(x => x.Name).ToListAsync());
    }

    [HttpGet("categories/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterCategory>> GetCategory(int id)
    {
        var entity = await _context.ServiceTicketMasterCategories.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ServiceTicketMasterCategory>> CreateCategory(
        [FromBody] ServiceTicketMasterCategory dto)
    {
        dto.Id = 0;
        dto.CreatedAt = DateTime.Now;
        dto.UpdatedAt = DateTime.Now;
        _context.ServiceTicketMasterCategories.Add(dto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategory), new { id = dto.Id }, dto);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterCategory>> UpdateCategory(
        int id, [FromBody] ServiceTicketMasterCategory dto)
    {
        var entity = await _context.ServiceTicketMasterCategories.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.Seq = dto.Seq;
        entity.IsActive = dto.IsActive;
        entity.CmpId = dto.CmpId;
        entity.UpdUser = dto.UpdUser;
        entity.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var entity = await _context.ServiceTicketMasterCategories.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ServiceTicketMasterCategories.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TAG
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("tags")]
    public async Task<ActionResult<IEnumerable<ServiceTicketMasterTag>>> GetTags(
        [FromQuery] string? cmpId,
        [FromQuery] bool? isActive)
    {
        var q = _context.ServiceTicketMasterTags.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(cmpId)) q = q.Where(x => x.CmpId == cmpId || x.CmpId == null);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return Ok(await q.OrderBy(x => x.Seq).ThenBy(x => x.Name).ToListAsync());
    }

    [HttpGet("tags/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterTag>> GetTag(int id)
    {
        var entity = await _context.ServiceTicketMasterTags.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    [HttpPost("tags")]
    public async Task<ActionResult<ServiceTicketMasterTag>> CreateTag(
        [FromBody] ServiceTicketMasterTag dto)
    {
        dto.Id = 0;
        dto.CreatedAt = DateTime.Now;
        dto.UpdatedAt = DateTime.Now;
        _context.ServiceTicketMasterTags.Add(dto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTag), new { id = dto.Id }, dto);
    }

    [HttpPut("tags/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterTag>> UpdateTag(
        int id, [FromBody] ServiceTicketMasterTag dto)
    {
        var entity = await _context.ServiceTicketMasterTags.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.Seq = dto.Seq;
        entity.IsActive = dto.IsActive;
        entity.CmpId = dto.CmpId;
        entity.UpdUser = dto.UpdUser;
        entity.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("tags/{id:int}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var entity = await _context.ServiceTicketMasterTags.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ServiceTicketMasterTags.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CHECKLIST
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("checklists")]
    public async Task<ActionResult<IEnumerable<ServiceTicketMasterChecklist>>> GetChecklists(
        [FromQuery] string? cmpId,
        [FromQuery] string? checklistType,
        [FromQuery] bool? isActive)
    {
        var q = _context.ServiceTicketMasterChecklists.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(cmpId)) q = q.Where(x => x.CmpId == cmpId || x.CmpId == null);
        if (!string.IsNullOrWhiteSpace(checklistType)) q = q.Where(x => x.ChecklistType == checklistType);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return Ok(await q.OrderBy(x => x.ChecklistType).ThenBy(x => x.Seq).ThenBy(x => x.Name).ToListAsync());
    }

    [HttpGet("checklists/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterChecklist>> GetChecklist(int id)
    {
        var entity = await _context.ServiceTicketMasterChecklists.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    [HttpPost("checklists")]
    public async Task<ActionResult<ServiceTicketMasterChecklist>> CreateChecklist(
        [FromBody] ServiceTicketMasterChecklist dto)
    {
        dto.Id = 0;
        dto.CreatedAt = DateTime.Now;
        dto.UpdatedAt = DateTime.Now;
        _context.ServiceTicketMasterChecklists.Add(dto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetChecklist), new { id = dto.Id }, dto);
    }

    [HttpPut("checklists/{id:int}")]
    public async Task<ActionResult<ServiceTicketMasterChecklist>> UpdateChecklist(
        int id, [FromBody] ServiceTicketMasterChecklist dto)
    {
        var entity = await _context.ServiceTicketMasterChecklists.FindAsync(id);
        if (entity == null) return NotFound();
        entity.ChecklistType = dto.ChecklistType;
        entity.Name = dto.Name;
        entity.Seq = dto.Seq;
        entity.IsActive = dto.IsActive;
        entity.CmpId = dto.CmpId;
        entity.UpdUser = dto.UpdUser;
        entity.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("checklists/{id:int}")]
    public async Task<IActionResult> DeleteChecklist(int id)
    {
        var entity = await _context.ServiceTicketMasterChecklists.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ServiceTicketMasterChecklists.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BULK reorder (drag-and-drop support)
    // ─────────────────────────────────────────────────────────────────────────

    [HttpPut("categories/reorder")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<ReorderItem> items)
    {
        foreach (var item in items)
        {
            var entity = await _context.ServiceTicketMasterCategories.FindAsync(item.Id);
            if (entity != null) { entity.Seq = item.Seq; entity.UpdatedAt = DateTime.Now; }
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("tags/reorder")]
    public async Task<IActionResult> ReorderTags([FromBody] List<ReorderItem> items)
    {
        foreach (var item in items)
        {
            var entity = await _context.ServiceTicketMasterTags.FindAsync(item.Id);
            if (entity != null) { entity.Seq = item.Seq; entity.UpdatedAt = DateTime.Now; }
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("checklists/reorder")]
    public async Task<IActionResult> ReorderChecklists([FromBody] List<ReorderItem> items)
    {
        foreach (var item in items)
        {
            var entity = await _context.ServiceTicketMasterChecklists.FindAsync(item.Id);
            if (entity != null) { entity.Seq = item.Seq; entity.UpdatedAt = DateTime.Now; }
        }
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record ReorderItem(int Id, int Seq);
