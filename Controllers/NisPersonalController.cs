using System.Globalization;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Personal Todo/Note — แท็บ "บันทึก" ส่วนตัวของ staff (server-synced)
/// owner = (CmpId, AccountId) · ตรวจสิทธิ์ทุก op ว่าเป็นของ user นั้นจริง
/// Auth: No [Authorize] — สอดคล้อง NisController pattern (กรองด้วย cmpid+userId)
/// Route prefix: api/nis/personal
/// </summary>
[ApiController]
[Route("api/nis/personal")]
public class NisPersonalController : ControllerBase
{
    private readonly DatabaseContext _context;

    public NisPersonalController(DatabaseContext context)
    {
        _context = context;
    }

    private static string Iso(DateTime dt) => dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

    private static NisPersonalTodoDto MapTodo(NisPersonalTodo t) => new()
    {
        Id = t.Id,
        Text = t.Text,
        RemindDateTime = t.RemindDateTime ?? string.Empty,
        Done = t.Done,
        CreatedAt = Iso(t.CreatedDate),
    };

    private static NisPersonalNoteDto MapNote(NisPersonalNote n) => new()
    {
        Id = n.Id,
        Text = n.Text,
        Reminder = n.Reminder ?? string.Empty,
        CreatedAt = Iso(n.CreatedDate),
    };

    // ── Todos ────────────────────────────────────────────────────────────────

    // GET api/nis/personal/todos?cmpid=&userId=
    [HttpGet("todos")]
    public async Task<ActionResult<IEnumerable<NisPersonalTodoDto>>> GetTodos(
        [FromQuery] string? cmpid,
        [FromQuery] long userId)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var rows = await _context.NisPersonalTodos
            .AsNoTracking()
            .Where(t => t.CmpId == cmpid && t.AccountId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();

        return Ok(rows.Select(MapTodo));
    }

    // POST api/nis/personal/todos
    [HttpPost("todos")]
    public async Task<ActionResult<NisPersonalTodoDto>> CreateTodo([FromBody] NisPersonalTodoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId))
            return BadRequest(new { message = "cmpId is required" });
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { message = "text is required" });

        var todo = new NisPersonalTodo
        {
            Id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id!,
            CmpId = dto.CmpId!,
            AccountId = dto.UserId,
            Text = dto.Text.Trim(),
            RemindDateTime = dto.RemindDateTime,
            Done = false,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };
        _context.NisPersonalTodos.Add(todo);
        await _context.SaveChangesAsync();
        return Ok(MapTodo(todo));
    }

    // PUT api/nis/personal/todos/{id}/toggle  body { done, cmpId, userId }
    [HttpPut("todos/{id}/toggle")]
    public async Task<IActionResult> ToggleTodo(string id, [FromBody] NisPersonalTodoToggleDto dto)
    {
        var todo = await _context.NisPersonalTodos
            .SingleOrDefaultAsync(t => t.Id == id && t.CmpId == dto.CmpId && t.AccountId == dto.UserId);
        if (todo == null) return NotFound();

        todo.Done = dto.Done;
        todo.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(MapTodo(todo));
    }

    // DELETE api/nis/personal/todos/{id}?cmpid=&userId=
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteTodo(string id, [FromQuery] string? cmpid, [FromQuery] long userId)
    {
        var todo = await _context.NisPersonalTodos
            .SingleOrDefaultAsync(t => t.Id == id && t.CmpId == cmpid && t.AccountId == userId);
        if (todo == null) return NotFound();

        _context.NisPersonalTodos.Remove(todo);
        await _context.SaveChangesAsync();
        return Ok(new { message = "deleted" });
    }

    // ── Notes ────────────────────────────────────────────────────────────────

    // GET api/nis/personal/notes?cmpid=&userId=
    [HttpGet("notes")]
    public async Task<ActionResult<IEnumerable<NisPersonalNoteDto>>> GetNotes(
        [FromQuery] string? cmpid,
        [FromQuery] long userId)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var rows = await _context.NisPersonalNotes
            .AsNoTracking()
            .Where(n => n.CmpId == cmpid && n.AccountId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();

        return Ok(rows.Select(MapNote));
    }

    // POST api/nis/personal/notes
    [HttpPost("notes")]
    public async Task<ActionResult<NisPersonalNoteDto>> CreateNote([FromBody] NisPersonalNoteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId))
            return BadRequest(new { message = "cmpId is required" });
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { message = "text is required" });

        var note = new NisPersonalNote
        {
            Id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id!,
            CmpId = dto.CmpId!,
            AccountId = dto.UserId,
            Text = dto.Text.Trim(),
            Reminder = dto.Reminder,
            CreatedDate = DateTime.UtcNow,
        };
        _context.NisPersonalNotes.Add(note);
        await _context.SaveChangesAsync();
        return Ok(MapNote(note));
    }

    // DELETE api/nis/personal/notes/{id}?cmpid=&userId=
    [HttpDelete("notes/{id}")]
    public async Task<IActionResult> DeleteNote(string id, [FromQuery] string? cmpid, [FromQuery] long userId)
    {
        var note = await _context.NisPersonalNotes
            .SingleOrDefaultAsync(n => n.Id == id && n.CmpId == cmpid && n.AccountId == userId);
        if (note == null) return NotFound();

        _context.NisPersonalNotes.Remove(note);
        await _context.SaveChangesAsync();
        return Ok(new { message = "deleted" });
    }
}
