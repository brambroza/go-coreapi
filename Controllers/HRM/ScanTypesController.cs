using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/scan-types")]
public class ScanTypesController : ControllerBase
{
    private readonly HrDbContext _db;
    public ScanTypesController(HrDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string cmpId)
    {
        var data = await _db.ScanTypes
            .AsNoTracking()
            .Where(x => x.CmpId == cmpId)
            .OrderBy(x => x.ScanTypeId)
            .Select(x => new
            {
                x.ScanTypeId,
                x.Name,
                x.PunchCount,
                x.HasOT,
                x.IsStrictOrder,
                x.Notes,
                Slots = x.Slots.OrderBy(s => s.SeqNo).Select(s => new {
                    s.ScanTypeSlotId, s.SeqNo, s.SlotCode, s.SlotName, s.ExpectedFrom, s.ExpectedTo, s.Required
                })
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(string cmpId, int id)
    {
        var x = await _db.ScanTypes
            .AsNoTracking()
            .Include(t => t.Slots)
            .FirstOrDefaultAsync(t => t.CmpId == cmpId && t.ScanTypeId == id);

        if (x is null) return NotFound();

        return Ok(new
        {
            x.ScanTypeId,
            x.CmpId,
            x.Name,
            x.PunchCount,
            x.HasOT,
            x.IsStrictOrder,
            x.Notes,
            Slots = x.Slots.OrderBy(s => s.SeqNo).Select(s => new {
                s.ScanTypeSlotId, s.SeqNo, s.SlotCode, s.SlotName, s.ExpectedFrom, s.ExpectedTo, s.Required
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] ScanTypeCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        // basic validation
        if (dto.PunchCount <= 0 || dto.PunchCount > 12) return BadRequest("PunchCount must be 1..12");
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");

        var entity = new ScanType
        {
            CmpId = cmpId,
            Name = dto.Name.Trim(),
            PunchCount = dto.PunchCount,
            HasOT = dto.HasOT,
            IsStrictOrder = dto.IsStrictOrder,
            Notes = dto.Notes
        };

        if (dto.Slots?.Any() == true)
        {
            foreach (var s in dto.Slots)
            {
                entity.Slots.Add(new ScanTypeSlot
                {
                    CmpId = cmpId,
                    SeqNo = s.SeqNo,
                    SlotCode = s.SlotCode.Trim(),
                    SlotName = s.SlotName.Trim(),
                    ExpectedFrom = s.ExpectedFrom,
                    ExpectedTo = s.ExpectedTo,
                    Required = s.Required
                });
            }
        }

        _db.ScanTypes.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.ScanTypeId }, new { entity.ScanTypeId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(string cmpId, int id, [FromBody] ScanTypeUpdateDto dto)
    {
        var entity = await _db.ScanTypes
            .Include(x => x.Slots)
            .FirstOrDefaultAsync(x => x.CmpId == cmpId && x.ScanTypeId == id);

        if (entity is null) return NotFound();

        if (dto.PunchCount <= 0 || dto.PunchCount > 12) return BadRequest("PunchCount must be 1..12");

        entity.Name = dto.Name.Trim();
        entity.PunchCount = dto.PunchCount;
        entity.HasOT = dto.HasOT;
        entity.IsStrictOrder = dto.IsStrictOrder;
        entity.Notes = dto.Notes;

        // --- Upsert Slots (replace strategy: sync by id)
        if (dto.Slots is not null)
        {
            var incomingIds = dto.Slots.Where(x => x.ScanTypeSlotId.HasValue).Select(x => x.ScanTypeSlotId!.Value).ToHashSet();

            // delete removed
            var toRemove = entity.Slots.Where(s => !incomingIds.Contains(s.ScanTypeSlotId)).ToList();
            _db.ScanTypeSlots.RemoveRange(toRemove);

            // update / add
            foreach (var s in dto.Slots)
            {
                if (s.ScanTypeSlotId.HasValue)
                {
                    var slot = entity.Slots.FirstOrDefault(x => x.ScanTypeSlotId == s.ScanTypeSlotId.Value);
                    if (slot is null) continue;

                    slot.SeqNo = s.SeqNo;
                    slot.SlotCode = s.SlotCode.Trim();
                    slot.SlotName = s.SlotName.Trim();
                    slot.ExpectedFrom = s.ExpectedFrom;
                    slot.ExpectedTo = s.ExpectedTo;
                    slot.Required = s.Required;
                }
                else
                {
                    entity.Slots.Add(new ScanTypeSlot
                    {
                        CmpId = cmpId,
                        SeqNo = s.SeqNo,
                        SlotCode = s.SlotCode.Trim(),
                        SlotName = s.SlotName.Trim(),
                        ExpectedFrom = s.ExpectedFrom,
                        ExpectedTo = s.ExpectedTo,
                        Required = s.Required
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string cmpId, int id)
    {
        var entity = await _db.ScanTypes.FirstOrDefaultAsync(x => x.CmpId == cmpId && x.ScanTypeId == id);
        if (entity is null) return NotFound();

        _db.ScanTypes.Remove(entity); // cascade slots
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
