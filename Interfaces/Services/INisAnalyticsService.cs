using goalongapi.Dtos.Nis;

namespace goalongapi.Services;

/// <summary>
/// ตัวเลขทั้งหมดที่หน้าภาพรวมและรายงาน PDF ของ NIS Service Report ใช้ — port จาก apps/api/src/analytics/analytics.service.ts
///
/// กติกาที่บังคับ**ที่ชั้นนี้** ไม่ปล่อยให้หน้าจอตีความเอง (MERGE_PLAN §0):
///   · ข้อ 1 — จำนวนเคสแยกสองก้อนเสมอ (แจ้งเข้า createdAt / ปิดได้ resolvedAt)
///   · ข้อ 3 — เวลาที่คืนคือเวลานาฬิกา ต้องมีป้ายกำกับติดไปกับข้อมูล (hoursNote)
///   · ข้อ 4 — SLA มีสวิตช์ ค่าเริ่มต้นปิด (NisReport:SlaEnabled)
///   · ข้อ 5 — เวลาทำการหักวันหยุดจาก NisHoliday · ตัวเลขที่อิงเวลาทำการเป็น null จนกว่าวันหยุดของปีที่ช่วงคร่อมจะยืนยันครบ
///   · ข้อ 12 — กลุ่มลูกค้า: ผู้เรียก (report builder) ส่งรหัสสมาชิกทั้งกลุ่มมาทาง <c>customerIds</c> · กลุ่มที่ติดธง backdated
///              นับจำนวนเคสปกติแต่ไม่เข้าสถิติเวลา
///
/// ทุกเมธอดมีสองรูป: รับ <see cref="NisAnalyticsQuery"/> (จาก HTTP) หรือรับ <see cref="NisCaseFilter"/> + <c>customerIds</c>
/// (จาก NisReportSnapshotService ที่ต้องส่งรหัสที่ resolve กลุ่มแล้ว) — <c>customerIds</c> ที่ไม่ null จะ**แทน** filter.CustomerId ·
/// filter.DateField ถูกเพิกเฉย (analytics เลือกเองต่อก้อน) · แถวถูกดึงผ่าน <see cref="INisArchiveService.BuildWhere"/> เสมอ
/// </summary>
public interface INisAnalyticsService
{
    /// <summary>KPI ทั้งชุด — สองก้อน (intake/resolved) ของช่วงปัจจุบัน + ช่วงก่อนหน้า + MoM 13 ตัว + คำเตือนวันหยุด</summary>
    Task<NisSummaryDto> SummaryAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default);

    /// <summary>KPI ทั้งชุดจากตัวกรอง archive — ใช้โดย report builder</summary>
    Task<NisSummaryDto> SummaryAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, CancellationToken ct = default);

    /// <summary>กราฟปริมาณงาน — สองเส้น (intake/resolved) ต่อ bucket · เติมช่องว่างเป็น 0 · ติดธง partial ที่ขอบคลัง</summary>
    Task<NisTrendDto> TrendAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default);

    /// <summary>กราฟปริมาณงานจากตัวกรอง archive · bucket = auto | day | week | month</summary>
    Task<NisTrendDto> TrendAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, string bucket = "auto", CancellationToken ct = default);

    /// <summary>สัดส่วนตามมิติ — by = priority | category | customer | status (สองคอลัมน์) หรือ duration (ฮิสโทแกรม 6 ช่วง clock+business)</summary>
    Task<NisBreakdownDto> BreakdownAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default);

    /// <summary>สัดส่วนตามมิติจากตัวกรอง archive</summary>
    Task<NisBreakdownDto> BreakdownAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, string by, CancellationToken ct = default);

    /// <summary>ตารางวัน × ชั่วโมงที่ลูกค้าแจ้งเข้า (7×24) + ใน/นอกเวลาทำการต่อช่อง · วันหยุดเทียบปีปัจจุบัน</summary>
    Task<NisHeatmapDto> HeatmapAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default);

    /// <summary>heatmap จากตัวกรอง archive</summary>
    Task<NisHeatmapDto> HeatmapAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, CancellationToken ct = default);

    /// <summary>สวิตช์ SLA จาก NisReport:SlaEnabled — controller ใส่ใน meta.slaEnabled</summary>
    bool SlaEnabled { get; }
}
