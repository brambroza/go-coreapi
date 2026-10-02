using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisEquipmentChangeRequest ───────────────────────────────────────────────────
// คำขอแก้ทะเบียนอุปกรณ์ในตู้ Rack (dbo.NisEquipmentChangeRequest) — ช่างยื่นจากหน้า
// onsite ตอนตรวจ PM แล้วพบว่าทะเบียนไม่ตรงหน้างาน SM อนุมัติใน Service Board จึงจะ
// แก้ NisEquipment จริง (insert / delete / update ตาม Reason) — ก่อนอนุมัติทะเบียนไม่เปลี่ยน
// ช่างตรวจตามทะเบียนเดิมต่อได้

public class NisEquipmentChangeRequest
{
    [Key]
    [MaxLength(50)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerCode { get; set; } = string.Empty;

    /// NisTicket.TicketId ของงานที่พบปัญหา (optional — audit link)
    [MaxLength(50)]
    public string? TicketId { get; set; }

    /// ชื่อตู้ (= NisEquipment.RackName) ที่ขอแก้
    [MaxLength(200)]
    public string RackName { get; set; } = string.Empty;

    /// missing_in_registry (เพิ่มเข้าทะเบียน) | not_in_rack (ลบออก) | incorrect_info (แก้ข้อมูล)
    [MaxLength(50)]
    public string Reason { get; set; } = string.Empty;

    /// NisEquipment.Id เป้าหมาย — บังคับเมื่อ Reason = not_in_rack / incorrect_info
    [MaxLength(50)]
    public string? EquipmentId { get; set; }

    [MaxLength(200)]
    public string DeviceName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(200)]
    public string SerialNo { get; set; } = string.Empty;

    public int? UPosition { get; set; }

    [MaxLength(1000)]
    public string Note { get; set; } = string.Empty;

    /// ช่าง (FullName) ผู้ยื่นคำขอ
    [MaxLength(200)]
    public string RequestedBy { get; set; } = string.Empty;

    /// Pending | Approved | Rejected
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    [MaxLength(100)]
    public string? RejectedBy { get; set; }

    [MaxLength(500)]
    public string? RejectReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
