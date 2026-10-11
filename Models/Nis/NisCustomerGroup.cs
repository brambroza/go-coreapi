using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisCustomerGroup ────────────────────────────────────────────────────────
// กลุ่มลูกค้าของโมดูล NIS Service Report (dbo.NisCustomerGroup) — ลูกค้าหลายรหัส = รายงานฉบับเดียว
// แถวถูกสร้างเมื่อมีคนตั้งค่าที่ customerId นั้นเป็นครั้งแรกเท่านั้น (ไม่ seed ทุกรหัส)
//
// MergeIntoCustomerId: ประกาศว่ารหัสนี้เป็น "รหัสรอง" ให้รวมเคส/ตัวเลขไปกับรหัสที่ระบุ
//   ห้ามเป็นห่วง (A→B→A) และห้ามชี้ต่อเป็นทอด (A→B→C) — ตรวจที่ NisCustomerGroupService ก่อนเขียน
// Backdated: ธง "บันทึกย้อนหลัง" ระดับกลุ่ม อ่านที่แถวของรหัสหลักเท่านั้น — ตัวเลขระยะเวลาของทั้งกลุ่ม
//   แสดงเป็น null พร้อมคำอธิบาย แต่จำนวนเคสยังนับปกติ
// ไม่มี FK ไป NisServiceCase (รหัสอาจถูกตั้งค่าไว้ก่อนมีเคสจริง) · ชื่อไม่ชน NisCustomerDirectory

public class NisCustomerGroup
{
    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MergeIntoCustomerId { get; set; }

    public bool Backdated { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
