namespace goalongapi.Dtos.Nis;

// ── Personal Todo / Note — แท็บบันทึกส่วนตัวของ staff (server-synced) ──
// response ตรงกับ RN PersonalTodo/PersonalNote (camelCase)

public class NisPersonalTodoDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string RemindDateTime { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class NisPersonalTodoCreateDto
{
    /// client gen id (เช่น "todo-1720000000000") — ว่างได้ (server gen ให้)
    public string? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? RemindDateTime { get; set; }
    public string? CmpId { get; set; }
    public long UserId { get; set; }
}

public class NisPersonalTodoToggleDto
{
    public bool Done { get; set; }
    public string? CmpId { get; set; }
    public long UserId { get; set; }
}

public class NisPersonalNoteDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Reminder { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class NisPersonalNoteCreateDto
{
    public string? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Reminder { get; set; }
    public string? CmpId { get; set; }
    public long UserId { get; set; }
}
