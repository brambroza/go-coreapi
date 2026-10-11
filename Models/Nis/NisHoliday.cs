using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisHoliday ──────────────────────────────────────────────────────────────
// วันหยุดที่ใช้หักออกจากเวลาทำการของโมดูล NIS Service Report (dbo.NisHoliday)
//
// 🔴 "วันหยุดราชการ" กับ "วันที่ NIS ปิดทำการ" ไม่ใช่สิ่งเดียวกัน — มีแต่วันที่ Observed=true
//    เท่านั้นที่ถูกหักจากเวลาทำการ · seed ใส่ Kind=government, Observed=false เสมอ คนต้องยืนยันกับ HR
// 🔴 ตราบใดที่ปีใดมีวันหยุดที่ยังไม่ Observed ครบ ตัวเลขที่อิงเวลาทำการของปีนั้นจะไม่แสดง (SPEC §9 ข้อ 67)

public class NisHoliday
{
    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// 'YYYY-MM-DD' เวลาไทย — เก็บเป็นข้อความเพื่อไม่ให้ timezone เลื่อนวัน
    [MaxLength(10)]
    public string Date { get; set; } = string.Empty;

    public int Year { get; set; }

    [MaxLength(200)]
    public string NameTh { get; set; } = string.Empty;

    /// government = ปฏิทินราชการ (seed เป็นเจ้าของ) · company = บริษัทหยุดเอง
    [MaxLength(20)]
    public string Kind { get; set; } = "government";

    /// วันที่นี้ถูกต้องตามประกาศจริงไหม
    public bool Verified { get; set; }

    /// NIS ปิดทำการวันนี้จริงไหม — ค่าเริ่มต้น false เสมอ
    public bool Observed { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
