using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisOnsiteProgress ─────────────────────────────────────────────────────────
// Draft ความคืบหน้างาน onsite ที่ "ยังไม่ปิดงาน" ต่อ 1 ตั๋ว/1 ช่าง (cross-device resume)
// เขียนโดย CRM (go-crm-24v4 dual-write ระหว่างทำงาน) · อ่านโดย CRM + RN (NIS-OnsiteService)
// upsert key = (CmpId, TicketId, UserLogin) — last-write-wins ด้วย SavedAt จาก client
// Contract: go-crm-24v4/docs/nis-onsite-progress-api-contract.md
// Migration: Database/Migrations/20260722_AddNisOnsiteProgressTable.sql
public class NisOnsiteProgress
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string CmpId { get; set; } = string.Empty;

    /// id ตั๋ว onsite ที่ client ส่งมา — ตกลงใช้ ticketCode ("TK-BK-0014-10") ให้ CRM/RN ตรงกัน
    [MaxLength(100)]
    public string TicketId { get; set; } = string.Empty;

    /// ช่างเจ้าของ draft (userlogin) — แยก draft คนละคนบนตั๋วเดียวกัน
    [MaxLength(200)]
    public string UserLogin { get; set; } = string.Empty;

    /// ทั้งก้อน INisOnsiteProgressSnapshot (มี base64 รูป/ลายเซ็น → nvarchar(max))
    public string SnapshotJson { get; set; } = string.Empty;

    /// epoch ms จาก snapshot.savedAt — client ใช้เทียบ new/old ตอน reconcile
    public long SavedAt { get; set; }

    /// server time (audit + เกณฑ์ cron ล้าง draft ค้าง)
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
