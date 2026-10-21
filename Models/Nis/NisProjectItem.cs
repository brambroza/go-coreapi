using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisProjectItem ──────────────────────────────────────────────────────────────
// รายการสินค้าขายของโครงการ (dbo.NisProjectItem) — ดึงจากรายการ Sale Order ที่อ้างใน NisProject.SoRef
// งาน Implement: ช่างกด "นำของออก" จากคลัง 1 สินค้าสำหรับขาย ได้ไม่เกิน Qty ของรายการนี้
// QtyIssued = ยอดที่สร้างใบเบิกแล้ว (backend ตรวจไม่ให้เกิน Qty — 409)

public class NisProjectItem
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// เลข Sale Order ต้นทาง
    [MaxLength(50)]
    public string SoNo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ProdCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ProdName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UnitCode { get; set; } = string.Empty;

    /// จำนวนตาม SO (revision ล่าสุด รวมบรรทัดสินค้าซ้ำ)
    public decimal Qty { get; set; }

    /// จำนวนที่นำออกแล้ว (สร้างใบเบิกแล้ว)
    public decimal QtyIssued { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
