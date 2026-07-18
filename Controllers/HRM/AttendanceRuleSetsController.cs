using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/attendance-rulesets")]
public class AttendanceRuleSetsController : ControllerBase
{
    private readonly HrDbContext _db;
    public AttendanceRuleSetsController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId)
    {
        var data = await _db.AttendanceRuleSets.AsNoTracking()
            .Where(x => x.CmpId == cmpId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.RuleSetId)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.AttendanceRuleSets.AsNoTracking()
            .FirstOrDefaultAsync(r => r.CmpId == cmpId && r.RuleSetId == id);

        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] AttendanceRuleSetCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        using var tx = await _db.Database.BeginTransactionAsync();

        if (dto.IsDefault)
        {
            var old = await _db.AttendanceRuleSets.Where(x => x.CmpId == cmpId && x.IsDefault).ToListAsync();
            foreach (var r in old) r.IsDefault = false;
            await _db.SaveChangesAsync();
        }

        var entity = new AttendanceRuleSet
        {
            CmpId = cmpId,
            Name = dto.Name.Trim(),
            IsDefault = dto.IsDefault,
            RuleJson = dto.RuleJson,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo
        };

        _db.AttendanceRuleSets.Add(entity);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.RuleSetId }, new { entity.RuleSetId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] AttendanceRuleSetUpdateDto dto)
    {
        var entity = await _db.AttendanceRuleSets.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.RuleSetId == id);
        if (entity is null) return NotFound();

        using var tx = await _db.Database.BeginTransactionAsync();

        if (dto.IsDefault && !entity.IsDefault)
        {
            var old = await _db.AttendanceRuleSets.Where(x => x.CmpId == cmpId && x.IsDefault).ToListAsync();
            foreach (var r in old) r.IsDefault = false;
        }

        entity.Name = dto.Name.Trim();
        entity.IsDefault = dto.IsDefault;
        entity.RuleJson = dto.RuleJson;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/set-default")]
    public async Task<IActionResult> SetDefault(string cmpId, int id)
    {
        var entity = await _db.AttendanceRuleSets.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.RuleSetId == id);
        if (entity is null) return NotFound();

        using var tx = await _db.Database.BeginTransactionAsync();

        var old = await _db.AttendanceRuleSets.Where(x => x.CmpId == cmpId && x.IsDefault).ToListAsync();
        foreach (var r in old) r.IsDefault = false;

        entity.IsDefault = true;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { entity.RuleSetId, entity.IsDefault });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.AttendanceRuleSets.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.RuleSetId == id);
        if (entity is null) return NotFound();

        _db.AttendanceRuleSets.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
