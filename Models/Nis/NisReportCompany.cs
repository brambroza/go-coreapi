using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisReportCompany ────────────────────────────────────────────────────────
// ส่วนเสริมข้อมูลบริษัทของโมดูล NIS Service Report (dbo.NisReportCompany) — **ไม่ใช่ที่เก็บชื่อ/ที่อยู่บริษัท**
// ชื่อ/ที่อยู่/โทร/อีเมล/เว็บ/เลขภาษี/LINE ID อ่านจากข้อมูลบริษัทของระบบหลัก (SP dbo.getcmpinfo) เสมอ
// ที่นี่เก็บเฉพาะที่ระบบหลักไม่มี: QR LINE ของบริษัท (lineQrPath) + รายชื่อเซลติดต่อ (salesContacts[])
// เก็บเป็น JSON ก้อนเดียว (รูป CompanyInfo ใน Dtos/nis/NisReportDtos.cs) เพราะ snapshot ของรายงาน
// ต้อง freeze ทั้งก้อน ณ ตอนออก — แยกคอลัมน์จะทำให้ประกอบไม่ตรงกับตอนที่ออกไป
// ช่องบริษัทที่ค้างอยู่ใน InfoJson (ข้อมูล import จาก NSR เดิม) ใช้เป็น fallback เมื่ออ่านระบบหลักไม่ได้เท่านั้น
// ช่องที่ยังไม่กรอก = ไม่พิมพ์ลงกระดาษ (ไม่พิมพ์ค่าว่าง) และไม่บล็อก export

public class NisReportCompany
{
    [Key]
    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    public string InfoJson { get; set; } = "{}";

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
