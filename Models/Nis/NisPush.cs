using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisPushToken ──────────────────────────────────────────────────────────────
// Expo Push token ของเครื่องช่าง (NIS Onsite app · Track B MVP)
// upsert key = (CmpId, StaffName, DeviceId) — ช่างหลายเครื่อง = หลายแถว
// StaffName = Account.FullName ตรงกับ NisTicket.Assignee (identity เดียวกับที่ใช้ assign งาน)
public class NisPushToken
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string StaffName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UserId { get; set; }

    /// ExponentPushToken[xxxx] — เปลี่ยนได้หลัง reinstall/OS update (upsert ทุก app start)
    [MaxLength(255)]
    public string ExpoPushToken { get; set; } = string.Empty;

    /// UUID ที่ app สร้างครั้งแรกแล้วเก็บใน AsyncStorage
    [MaxLength(255)]
    public string DeviceId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Platform { get; set; }

    [MaxLength(50)]
    public string? AppVersion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ── NisPushLog ────────────────────────────────────────────────────────────────
// ประวัติ push ที่ส่งแล้ว + กันยิงซ้ำ (dedupe) ด้วย EventKey unique
// insert ก่อนส่งเสมอ — ถ้า EventKey ชน (unique violation) = เคยส่งแล้ว → ข้าม (first-writer-wins)
public class NisPushLog
{
    [Key]
    public int Id { get; set; }

    /// เช่น 'assign:TICKET_ID:ชื่อช่าง:202607161530' / 'overdue:TICKET_ID:20260716'
    [MaxLength(255)]
    public string EventKey { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CmpId { get; set; }

    [MaxLength(100)]
    public string? TicketId { get; set; }

    [MaxLength(200)]
    public string? StaffName { get; set; }

    [MaxLength(255)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Body { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
