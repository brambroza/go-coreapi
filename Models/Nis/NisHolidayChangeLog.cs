using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisHolidayChangeLog ─────────────────────────────────────────────────────
// ประวัติการแก้ตาราง NisHoliday จากหน้าจอ (dbo.NisHolidayChangeLog)
// ตอบคำถาม "ทำไมตัวเลขเวลาทำการเปลี่ยน" — บันทึกทุก field ที่เปลี่ยนเป็นแถวแยก

public class NisHolidayChangeLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// ตรงกับ NisHoliday.Date ที่ถูกแก้
    [MaxLength(10)]
    public string Date { get; set; } = string.Empty;

    /// nameTh | kind | verified | observed | note | created
    [MaxLength(30)]
    public string Field { get; set; } = string.Empty;

    /// ค่าก่อนแก้ — string เสมอเพื่อเก็บ boolean/text ในคอลัมน์เดียว · null = แถวเพิ่งถูกสร้าง
    [MaxLength(500)]
    public string? OldValue { get; set; }

    [MaxLength(500)]
    public string? NewValue { get; set; }

    /// username จาก JWT ของคนที่แก้ (แทน source gate/human ของระบบเดิม)
    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
