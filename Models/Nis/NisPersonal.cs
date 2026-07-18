using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisPersonalTodo ───────────────────────────────────────────────────────────
// รายการ "สิ่งที่ต้องทำ" ส่วนตัวของ staff (แท็บบันทึก · server-synced)
// owner = (CmpId, AccountId) · Id client-generated (เช่น "todo-1720000000000")
public class NisPersonalTodo
{
    [Key]
    [MaxLength(60)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public long AccountId { get; set; }

    public string Text { get; set; } = string.Empty;

    /// 'YYYY-MM-DDTHH:MM' หรือว่าง (เตือนเมื่อ — optional)
    [MaxLength(30)]
    public string? RemindDateTime { get; set; }

    public bool Done { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}

// ── NisPersonalNote ───────────────────────────────────────────────────────────
// โน้ตส่วนตัวของ staff (แท็บบันทึก · server-synced)
public class NisPersonalNote
{
    [Key]
    [MaxLength(60)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public long AccountId { get; set; }

    public string Text { get; set; } = string.Empty;

    /// 'YYYY-MM-DDTHH:MM' หรือว่าง (reminder — optional)
    [MaxLength(30)]
    public string? Reminder { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
