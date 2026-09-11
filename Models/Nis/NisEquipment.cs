using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models.Nis;

// ── NisEquipment ────────────────────────────────────────────────────────────
// Master ข้อมูลอุปกรณ์ในตู้ Rack ของลูกค้าแต่ละราย (dbo.NisEquipment) — ผูกกับ
// customerCode อย่างเดียว (ไม่แยกตาม location/สาขา) เลือกใช้ตอนสร้าง NIS Project
// ที่มีเงื่อนไข Preventive Maintenance (pmPerYear > 0)

public class NisEquipment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CustomerCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string RackName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DeviceName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string SerialNo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Note { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
