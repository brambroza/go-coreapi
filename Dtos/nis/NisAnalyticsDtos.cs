using System.Text.Json;
using System.Text.Json.Serialization;
using goalongapi.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Dtos.Nis;

// ── DTO ของโมดูล NIS Service Report ส่วน analytics (api/nis/analytics) ────────
// port จาก packages/shared/src/{analytics-dto,api,customer-group,effort,kb,workload,hours-kind}.ts
// ชื่อ field ใช้ camelCase ตอน serialize (System.Text.Json default ของ AddControllers) — ต้องตรงกับ TS ทุกตัว
// เพราะ frontend (go-crm-24v4) และ snapshot ของรายงาน PDF อ่านรูปนี้ตรง ๆ

// ═══════════════════════════════════════════════════════════════════════════
// 0. ถ้อยคำที่ล็อกแล้ว (hours-kind.ts · customer-group.ts · analytics.service.ts)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// ป้ายชนิดเวลาและข้อความคงที่ของ analytics — **คัดลอกจาก TS ตรงตัว ห้ามเกลาคำ**
/// (คำตัดสินคุณ Tone 16 ก.ย. 2569 · hours-kind.ts เป็นแหล่งเดียวของถ้อยคำฝั่งจอ)
/// </summary>
public static class NisAnalyticsText
{
    /// <summary>ศัพท์ของเวลาตั้งแต่แจ้งจนปิด (CLOCK_KIND_SCREEN)</summary>
    public const string ClockKindScreen = "ระยะเวลาแก้ไข";

    /// <summary>บรรทัดนิยามของหน้าจอภายใน (CLOCK_NOTE_SCREEN)</summary>
    public const string ClockNoteScreen =
        "ระยะเวลาแก้ไข นับตั้งแต่แจ้งเข้ามาจนบันทึกแก้ไขครั้งล่าสุด รวมนอกเวลาทำการ — ไม่ใช่เวลาที่ทีมลงมือทำ ใช้คิดราคาต่อชั่วโมงไม่ได้";

    /// <summary>ป้ายค่ากลางต่อเคสที่ปิดได้ (CLOCK_MEDIAN_PER_RESOLVED_CASE)</summary>
    public const string ClockMedianPerResolvedCase = "ระยะเวลาแก้ไขของงานที่ปิดได้ — ค่ากลาง";

    /// <summary>ประโยคที่พิมพ์แทนตัวเลขระยะเวลาเมื่อกลุ่มติดธงบันทึกย้อนหลัง (DURATION_HIDDEN_REASON — เป็นข้อความที่ลูกค้าอ่าน)</summary>
    public const string DurationHiddenReason =
        "รายงานฉบับนี้ไม่แสดงตัวเลขระยะเวลาการทำงาน เนื่องจากงานของท่านบางส่วน" +
        "บันทึกเข้าระบบหลังปฏิบัติงานจริง ตัวเลขเวลาที่คำนวณจากบันทึกจึงไม่ตรงกับ" +
        "เวลาที่ใช้ทำงานจริง · จำนวนงานทั้งหมดในรายงานนี้ถูกต้องครบถ้วน";

    /// <summary>ป้ายกำกับเวลาที่ติดไปกับทุก endpoint ที่คืนตัวเลขเวลา (HOURS_NOTE ใน analytics.service.ts)</summary>
    public const string HoursNote =
        "\"เวลาทำการ\" นับเฉพาะ 08:30–17:30 วันจันทร์–ศุกร์ ไม่รวมวันหยุด · " +
        "ทั้ง" + ClockKindScreen + "และเวลาทำการใช้คิดราคาต่อชั่วโมงไม่ได้ · " +
        "ค่ากลาง คือค่าตรงกลางเมื่อเรียงทุกเรื่องจากเร็วไปช้า · p90 คือ 90% ของเรื่องปิดได้ภายในเวลานั้น " +
        "(ประมาณค่าระหว่างสองเรื่องที่ใกล้ที่สุด จึงอาจไม่ตรงกับเวลาของเรื่องใดเรื่องหนึ่งพอดี)";

    public const string IntakeLabel = "งานที่แจ้งเข้า";
    public const string ResolvedLabel = "งานที่ปิดได้";

    /// <summary>ลำดับความด่วน (PRIORITY_ORDER) — ใช้เรียง breakdown by=priority</summary>
    public static readonly string[] PriorityOrder = { "critical", "major", "medium", "minor" };

    /// <summary>PRIORITY_LABEL_TH</summary>
    public static readonly IReadOnlyDictionary<string, string> PriorityLabelTh = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["critical"] = "วิกฤต",
        ["major"] = "สูง",
        ["medium"] = "ปานกลาง",
        ["minor"] = "ต่ำ",
    };

    /// <summary>STATUS_LABEL_TH</summary>
    public static readonly IReadOnlyDictionary<string, string> StatusLabelTh = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["new"] = "รับเรื่อง",
        ["inprogress"] = "กำลังทำ",
        ["finish"] = "ปิดงานแล้ว",
    };

    /// <summary>
    /// เกณฑ์เวลาปิดงานต่อระดับความด่วน (ชม.ทำการ) — 🔴 **ค่าสมมติเพื่อให้ระบบทำงานได้ ไม่ใช่ SLA ตามสัญญา**
    /// (DEFAULT_RESOLUTION_TARGET_HOURS · คำถาม C1 ยังไม่มีคำตอบ) ถูกกลบเป็น null เมื่อสวิตช์ SLA ปิด
    /// </summary>
    public static readonly IReadOnlyDictionary<string, double> DefaultResolutionTargetHours = new Dictionary<string, double>(StringComparer.Ordinal)
    {
        ["critical"] = 4,
        ["major"] = 8,
        ["medium"] = 16,
        ["minor"] = 40,
    };

    /// <summary>ป้ายภาษาไทยของช่วงหน้าภาระงานทีม (WORKLOAD_PERIOD_LABEL) — คีย์คือค่าที่รับได้</summary>
    public static readonly IReadOnlyDictionary<string, string> WorkloadPeriodLabel = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["month"] = "เดือนนี้",
        ["quarter"] = "ไตรมาสนี้",
        ["year"] = "ปีนี้",
    };

    /// <summary>KPI ที่ SPEC ระบุแต่ทำไม่ได้จากข้อมูลจริง (UNAVAILABLE_METRICS) — ส่งใน meta.unavailable ไม่คืนเป็น 0</summary>
    public static readonly IReadOnlyList<NisUnavailableMetricDto> UnavailableMetrics = new[]
    {
        new NisUnavailableMetricDto { Key = "firstResponse", Reason = "ข้อมูลต้นทางไม่มี firstResponseAt — ระบบ NIS ไม่บันทึกเวลาตอบกลับครั้งแรก" },
        new NisUnavailableMetricDto { Key = "reopenRate", Reason = "ข้อมูลต้นทางไม่มี reopenCount — ไม่มีทางรู้ว่าเคสไหนถูกเปิดซ้ำ" },
        new NisUnavailableMetricDto { Key = "channel", Reason = "ข้อมูลต้นทางไม่มี channel — มีแต่ธง viaLine ซึ่งบอกได้แค่ LINE/ไม่ใช่ LINE" },
        new NisUnavailableMetricDto { Key = "backlog", Reason = "status เป็น finish 99.8% — ตัวเลขงานค้างจะเป็น 0-3 ตลอด ไม่มีความหมาย" },
    };
}

/// <summary>KPI ที่ทำไม่ได้จากข้อมูลจริง — ประกาศไว้ ไม่ใช่หายไปเงียบ ๆ (SPEC ข้อ 8)</summary>
public sealed class NisUnavailableMetricDto
{
    public string Key { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════════
// 1. Query ของ api/nis/analytics/{summary,trend,breakdown,heatmap} (AnalyticsQuerySchema)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// ตัวกรองของ analytics — **ชุดย่อยของ <see cref="NisCaseFilter"/>** (from · to · customerId · status · priority ·
/// category · includeTest) + bucket (trend) + by (breakdown)
///
/// 🔴 ตัดออกโดยตั้งใจ: <c>dateField</c> (analytics คืนสองก้อนพร้อมกันเสมอ) · <c>assignee</c> (ปิดช่องรั่วตัวเลขรายบุคคล —
/// กรองทีละชื่อ 12 รอบก็ได้ตารางภาระงานครบ) · <c>q</c>/sort/page · schema เป็น strict: คีย์ที่ไม่รู้จักต้องถูก**ปฏิเสธ**
/// ไม่ใช่กลืนเงียบ — controller ตรวจคีย์ดิบใน Request.Query ด้วย <see cref="UnknownKeys"/>
/// </summary>
public sealed class NisAnalyticsQuery
{
    public static readonly string[] Buckets = { "auto", "day", "week", "month" };
    public static readonly string[] Bys = { "priority", "category", "customer", "status", "duration" };

    /// <summary>คีย์ query ที่รับได้ (รวม cmpid ของ coreapi) — เทียบแบบไม่สนตัวพิมพ์ตาม model binding ของ ASP.NET</summary>
    public static readonly string[] AllowedKeys =
        { "cmpid", "from", "to", "customerId", "status", "priority", "category", "includeTest", "bucket", "by" };

    /// <summary>ข้อความเมื่อพบคีย์ที่ไม่รู้จัก — คัดลอกจาก AnalyticsQueryFields.strict()</summary>
    public const string UnknownKeyMessage =
        "พารามิเตอร์ที่ไม่รู้จัก — endpoint นี้รับเฉพาะตัวกรองชุดเดียวกับ /api/cases " +
        "(assignee และ dateField ใช้ไม่ได้ในเฟสนี้ ดูเหตุผลใน packages/shared/src/api.ts)";

    /// <summary>ขอบต้นช่วง — 'YYYY-MM-DD' (ต้นวันเวลาไทย) หรือ ISO 8601</summary>
    public string? From { get; set; }

    /// <summary>ขอบท้ายช่วง — 'YYYY-MM-DD' (ท้ายวันเวลาไทย) หรือ ISO 8601</summary>
    public string? To { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? CustomerId { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Status { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Priority { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Category { get; set; }

    /// <summary>true = เอาข้อมูลทดสอบเข้ามาด้วย (ค่าเริ่มต้น false)</summary>
    public bool IncludeTest { get; set; }

    /// <summary>ความละเอียดของ trend — auto (ค่าเริ่มต้น: ≤62 วัน day · ≤270 วัน week · เกินนั้น month) | day | week | month</summary>
    public string Bucket { get; set; } = "auto";

    /// <summary>มิติของ breakdown — priority (ค่าเริ่มต้น) | category | customer | status | duration</summary>
    public string By { get; set; } = "priority";

    /// <summary>แปลงเป็นตัวกรองของ archive (where เดียวกับ /cases) โดยกำหนด dateField ตามก้อนที่ต้องการ</summary>
    public NisCaseFilter ToFilter(string dateField = "createdAt") => new()
    {
        From = From,
        To = To,
        DateField = dateField,
        CustomerId = CustomerId,
        Status = Status,
        Priority = Priority,
        Category = Category,
        IncludeTest = IncludeTest,
    };

    /// <summary>คีย์ใน query string ที่ไม่อยู่ใน <see cref="AllowedKeys"/> — ใช้ตอบ 400 แบบ strict (สะท้อนกลับแค่ชื่อคีย์ ไม่ใช่ค่า)</summary>
    public static List<string> UnknownKeys(IEnumerable<string> queryKeys) =>
        queryKeys.Where(k => !AllowedKeys.Contains(k, StringComparer.OrdinalIgnoreCase)).ToList();

    /// <summary>ตรวจตัวกรอง (ใช้กติกาเดียวกับ NisCaseFilter) + bucket/by — คืนรายการข้อผิดพลาด (ว่าง = ผ่าน)</summary>
    public List<string> Validate()
    {
        var issues = ToFilter().Validate();
        if (!Buckets.Contains(Bucket)) issues.Add("bucket: ต้องเป็น auto | day | week | month");
        if (!Bys.Contains(By)) issues.Add("by: ต้องเป็น priority | category | customer | status | duration");
        return issues;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 2. Summary (SummaryDto)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// สถิติเวลา — สามค่ามาด้วยกันเสมอ (ค่าเฉลี่ยหลอกในชุดข้อมูลนี้: median 0.21 vs mean 2.95)
/// median/mean/p90 เป็น null ได้สองเหตุ: ไม่มีเคส (N=0) หรือเป็นสถิติเวลาทำการที่ถูกบล็อกตาม SPEC §9 ข้อ 67 — N ยังเป็นจำนวนจริงเสมอ
/// </summary>
public sealed class NisHourStatDto
{
    public double? Median { get; set; }
    public double? Mean { get; set; }
    public double? P90 { get; set; }
    public int N { get; set; }

    /// <summary>จาก <see cref="NisHoursStat.HoursStat"/></summary>
    public static NisHourStatDto From(IEnumerable<double> values)
    {
        var (median, mean, p90, n) = NisHoursStat.HoursStat(values);
        return new NisHourStatDto { Median = median, Mean = mean, P90 = p90, N = n };
    }
}

/// <summary>ด้านหนึ่งของ SLA — within/breached/pct เป็น null เมื่อสวิตช์ปิดหรือถูกบล็อกวันหยุด · judged เป็นจำนวนจริงเสมอ</summary>
public sealed class NisSlaSideDto
{
    public int? Within { get; set; }
    public int? Breached { get; set; }
    public double? Pct { get; set; }
    public int Judged { get; set; }
}

/// <summary>
/// SLA สองวิธีนับเสมอ — business คือคำตอบของระบบ (เวลาทำการ 08:30–17:30 จ–ศ หักวันหยุด · คุณ Tone 5 ก.ย. 2569) ·
/// clock เก็บไว้เทียบตอนตรวจสอบเท่านั้น **ห้ามแสดงเป็นตัวเลข SLA** · สวิตช์ปิด = within/breached/pct และ targets เป็น null
/// </summary>
public sealed class NisSlaBlockDto
{
    public bool Enabled { get; set; }
    public Dictionary<string, double>? Targets { get; set; }
    public NisSlaSideDto Business { get; set; } = new();
    public NisSlaSideDto Clock { get; set; } = new();
    public string Note { get; set; } = string.Empty;
}

/// <summary>งานนอกเวลาทำการแยกชนิด (OffHours)</summary>
public sealed class NisOffHoursDto
{
    public int Total { get; set; }
    public int Before { get; set; }
    public int After { get; set; }
    public int Weekend { get; set; }
    public int Holiday { get; set; }
}

/// <summary>คะแนนความพึงพอใจ — กรองค่าที่ไม่ใช่ 1–5 ("ยังไม่ให้คะแนน") ออกก่อนเฉลี่ย</summary>
public sealed class NisSatisfactionDto
{
    public double? Avg { get; set; }
    public int Rated { get; set; }
    public int NotRated { get; set; }
}

/// <summary>ก้อน "งานที่แจ้งเข้า" — นับตาม createdAt (SPEC 9.1) · offHours = null เมื่อวันหยุดของปีที่ช่วงคร่อมยังยืนยันไม่ครบ</summary>
public sealed class NisIntakeBlockDto
{
    public string Label { get; set; } = NisAnalyticsText.IntakeLabel;
    public int Count { get; set; }
    public int Customers { get; set; }
    public NisOffHoursDto? OffHours { get; set; }
}

/// <summary>ก้อน "งานที่ปิดได้" — นับตาม resolvedAt · เวลาเฉลี่ยคิดจากก้อนนี้เท่านั้น · clockHours = เวลานาฬิกา ห้ามคิดราคาต่อชั่วโมง</summary>
public sealed class NisResolvedBlockDto
{
    public string Label { get; set; } = NisAnalyticsText.ResolvedLabel;
    public int Count { get; set; }
    public int Customers { get; set; }
    public NisHourStatDto ClockHours { get; set; } = new();
    public NisHourStatDto BusinessHours { get; set; } = new();
    public NisSatisfactionDto Satisfaction { get; set; } = new();
    public NisSlaBlockDto Sla { get; set; } = new();
}

/// <summary>
/// ส่วนต่างระหว่างสองก้อน — **นับจากเคสจริง ห้ามเอาผลลบมาใช้** (สองก้อนเป็นคนละประชากร)
/// carriedIn/notClosedInPeriod เป็น null เมื่อไม่ได้ระบุช่วงเวลา
/// </summary>
public sealed class NisSpillDto
{
    public int? CarriedIn { get; set; }
    public int? NotClosedInPeriod { get; set; }
    public int? StillOpen { get; set; }
    public string Note { get; set; } = string.Empty;
}

/// <summary>KPI ชุดหนึ่ง (ช่วงปัจจุบันหรือช่วงก่อนหน้า)</summary>
public sealed class NisSummaryBlockDto
{
    public NisIntakeBlockDto Intake { get; set; } = new();
    public NisResolvedBlockDto Resolved { get; set; } = new();
    public NisSpillDto Spill { get; set; } = new();
}

/// <summary>หนึ่ง KPI เทียบช่วงก่อนหน้า · baseUnit = หน่วยของตัวเลขดิบ ไม่ใช่หน่วยที่ควรพิมพ์บนจอ</summary>
public sealed class NisMomEntryDto
{
    public double? Current { get; set; }
    public double? Previous { get; set; }
    public double? Diff { get; set; }
    public double? Pct { get; set; }
    public string BaseUnit { get; set; } = string.Empty;
}

/// <summary>MoM — mode: calendar-month (เดือนปฏิทินเต็มเดือน → เดือนก่อน) | equal-span (ช่วงยาวเท่ากัน)</summary>
public sealed class NisMomBlockDto
{
    public string? Mode { get; set; }
    public Dictionary<string, NisMomEntryDto> Kpis { get; set; } = new();
}

/// <summary>GET api/nis/analytics/summary (SummaryDto) — numbersBlocked = ตัวเลขเวลาทำการถูกกลบเป็น null ตาม SPEC §9 ข้อ 67</summary>
public sealed class NisSummaryDto
{
    public NisSummaryBlockDto Current { get; set; } = new();
    public NisSummaryBlockDto? Previous { get; set; }
    public NisMomBlockDto? Mom { get; set; }
    public string MomNote { get; set; } = string.Empty;
    public string HoursNote { get; set; } = NisAnalyticsText.HoursNote;
    /// <summary>เคสที่คำนวณเวลาทำการไม่ได้ (ข้อมูลเพี้ยน) ในช่วงปัจจุบัน — นับไม่ซ้ำ ไม่กลืนเงียบ</summary>
    public int SkippedRows { get; set; }
    public int? SkippedRowsPrevious { get; set; }
    public string? HolidayWarning { get; set; }
    public bool NumbersBlocked { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 3. Trend / Breakdown / Heatmap
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>หนึ่ง bucket ของกราฟปริมาณงาน · partial = คลังเก็บไม่เต็ม bucket นี้ **และ** ชุดที่เลือกมีเคสใน bucket นี้จริง</summary>
public sealed class NisTrendRowDto
{
    public string Key { get; set; } = string.Empty;
    public int Intake { get; set; }
    public int Resolved { get; set; }
    public bool Partial { get; set; }
}

/// <summary>GET api/nis/analytics/trend (TrendDto) — ไม่มี holidayWarning โดยตั้งใจ (นับจำนวนล้วน ไม่ใช้เวลาทำการ)</summary>
public sealed class NisTrendDto
{
    /// <summary>day | week | month</summary>
    public string Bucket { get; set; } = "month";
    public string IntakeLabel { get; set; } = NisAnalyticsText.IntakeLabel;
    public string ResolvedLabel { get; set; } = NisAnalyticsText.ResolvedLabel;
    public List<NisTrendRowDto> Rows { get; set; } = new();
    public string? PartialNote { get; set; }
}

/// <summary>แถว breakdown แบบนับจำนวน (priority | category | customer | status)</summary>
public sealed class NisBreakdownRowDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Intake { get; set; }
    public int Resolved { get; set; }
}

/// <summary>แถวฮิสโทแกรมเวลา 6 ช่วง · label = คอลัมน์เวลานาฬิกา · businessLabel = คอลัมน์ชั่วโมงทำการ (หน่วยต่างกัน) · business = null เมื่อถูกบล็อกวันหยุด</summary>
public sealed class NisDurationRowDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string BusinessLabel { get; set; } = string.Empty;
    public int Clock { get; set; }
    public int? Business { get; set; }
}

/// <summary>
/// GET api/nis/analytics/breakdown — discriminated union บน <c>by</c> (BreakdownDto ใน TS)
/// serialize เป็นชนิดลูกจริงเสมอ (JsonDerivedType ไม่มี discriminator) จึงได้รูปเดียวกับ TS ทั้งสองกิ่ง
/// </summary>
[JsonDerivedType(typeof(NisCountBreakdownDto))]
[JsonDerivedType(typeof(NisDurationBreakdownDto))]
public abstract class NisBreakdownDto
{
    public string By { get; set; } = "priority";

    /// <summary>จำนวนกลุ่มใน rows — ใช้เป็น meta.total</summary>
    [JsonIgnore]
    public abstract int RowCount { get; }
}

/// <summary>กิ่ง by = priority | category | customer | status (CountBreakdown)</summary>
public sealed class NisCountBreakdownDto : NisBreakdownDto
{
    public string IntakeLabel { get; set; } = NisAnalyticsText.IntakeLabel;
    public string ResolvedLabel { get; set; } = NisAnalyticsText.ResolvedLabel;
    public List<NisBreakdownRowDto> Rows { get; set; } = new();

    /// <inheritdoc />
    public override int RowCount => Rows.Count;
}

/// <summary>กิ่ง by = duration (DurationBreakdown) — ประชากรคือเคสที่ปิดได้เท่านั้น</summary>
public sealed class NisDurationBreakdownDto : NisBreakdownDto
{
    public string CountLabel { get; set; } = NisAnalyticsText.ResolvedLabel;
    public string HoursNote { get; set; } = NisAnalyticsText.HoursNote;
    public string? HolidayWarning { get; set; }
    public int SkippedRows { get; set; }
    public List<NisDurationRowDto> Rows { get; set; } = new();

    /// <inheritdoc />
    public override int RowCount => Rows.Count;
}

/// <summary>หนึ่งช่องของ heatmap</summary>
public sealed class NisHeatCellDto
{
    public int Total { get; set; }
    public int InHours { get; set; }
    public int OffHours { get; set; }
}

/// <summary>GET api/nis/analytics/heatmap (HeatmapDto) — grid[weekday][hour] · weekday 0 = อาทิตย์ (ป้ายอยู่ใน weekdays)</summary>
public sealed class NisHeatmapDto
{
    public string CountLabel { get; set; } = NisAnalyticsText.IntakeLabel;
    public List<string> Weekdays { get; set; } = new() { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
    public string HourLabel { get; set; } = "ชั่วโมงเริ่มต้น เวลาไทย (0 = 00:00–00:59)";
    public List<List<NisHeatCellDto>> Grid { get; set; } = new();
    public string? HolidayWarning { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 4. กลุ่มลูกค้า (customer-group.ts)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>หนึ่งแถวของหน้า "ลูกค้า" — หนึ่งแถว = หนึ่ง**กลุ่ม** (รหัสหลัก + สมาชิก) · ตัวเลขทุกช่องรวมทั้งกลุ่ม (CustomerOverviewRow)</summary>
public sealed class NisCustomerOverviewRowDto
{
    /// <summary>รหัสหลักของกลุ่ม</summary>
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>ทุกรหัสในกลุ่ม รวมรหัสหลัก — มากกว่า 1 แปลว่ากลุ่มหลายรหัส</summary>
    public List<string> MemberCustomerIds { get; set; } = new();
    /// <summary>ธงบันทึกย้อนหลังของกลุ่ม — true แล้ว resolutionHoursMedian เป็น null เสมอ</summary>
    public bool Backdated { get; set; }
    /// <summary>แจ้งเข้าเดือนนี้ (createdAt · เดือนปฏิทินไทย)</summary>
    public int IntakeThisMonth { get; set; }
    /// <summary>แจ้งเข้าตั้งแต่ ม.ค. ปีนี้ (createdAt)</summary>
    public int IntakeYtd { get; set; }
    /// <summary>ยังไม่ปิด ณ ตอนนี้ — สแนปช็อต</summary>
    public int OpenNow { get; set; }
    /// <summary>ค่ากลางเวลานาฬิกา (ชม.) ของเคสที่ปิดได้ตั้งแต่ ม.ค. — null เมื่อ backdated หรือไม่มีเคสปิด</summary>
    public double? ResolutionHoursMedian { get; set; }
    public int ResolutionHoursN { get; set; }
    /// <summary>null เสมอในเฟสนี้ — หน้านี้ยังไม่คำนวณ SLA (สวิตช์ปิด · ไม่ให้มีสูตรที่สาม)</summary>
    public double? SlaWithinPct { get; set; }
}

/// <summary>การตั้งค่ากลุ่มของรหัสหนึ่งรหัส (CustomerSettings) — GET/PUT api/nis/archive/customer-groups/{customerId}</summary>
public sealed class NisCustomerSettingsDto
{
    public string CustomerId { get; set; } = string.Empty;
    /// <summary>รหัสที่ customerId นี้ถูกรวมเข้าไป — null = ไม่ถูกรวม (ลูกค้าเดี่ยว หรือเป็นรหัสหลัก)</summary>
    public string? MergeIntoCustomerId { get; set; }
    /// <summary>ธงบันทึกย้อนหลัง — มีความหมายเมื่อ customerId นี้เป็นรหัสหลักของกลุ่มตัวเอง</summary>
    public bool Backdated { get; set; }
}

/// <summary>
/// body ของ PUT customer-groups/{customerId} (SetCustomerSettingsSchema) — ทั้งสองช่อง**เลือกส่งได้อิสระ**
/// ไม่ส่ง = ไม่แตะ · <c>mergeIntoCustomerId: null</c> = ยกเลิกการรวม — จึงต้องรู้ว่า "ส่งมาไหม" แยกจาก "เป็น null ไหม"
/// (parse จาก JsonElement เอง) · zod ต้นทางไม่ strict — คีย์ที่ไม่รู้จักถูกเพิกเฉยเหมือนเดิม
/// </summary>
public sealed class NisSetCustomerSettingsDto
{
    public bool HasMergeIntoCustomerId { get; private set; }
    public string? MergeIntoCustomerId { get; private set; }
    public bool HasBackdated { get; private set; }
    public bool? Backdated { get; private set; }

    /// <summary>parse + validate · คืน false พร้อม issues เมื่อข้อมูลไม่ถูกต้อง</summary>
    public static bool TryParse(JsonElement body, out NisSetCustomerSettingsDto dto, out List<string> issues)
    {
        dto = new NisSetCustomerSettingsDto();
        issues = new List<string>();
        if (body.ValueKind != JsonValueKind.Object)
        {
            issues.Add("(ราก): ต้องเป็น JSON object");
            return false;
        }
        if (body.TryGetProperty("mergeIntoCustomerId", out var merge))
        {
            dto.HasMergeIntoCustomerId = true;
            switch (merge.ValueKind)
            {
                case JsonValueKind.Null:
                    dto.MergeIntoCustomerId = null;
                    break;
                case JsonValueKind.String:
                    var v = (merge.GetString() ?? string.Empty).Trim();
                    // ค่าว่าง = ไม่รวม (resolver ถือ "" เป็นไม่รวมอยู่แล้ว) — เก็บเป็น null ให้ความหมายเดียว
                    dto.MergeIntoCustomerId = v.Length == 0 ? null : v;
                    if (v.Length > 50) issues.Add("mergeIntoCustomerId: ยาวเกิน 50 ตัวอักษร");
                    break;
                default:
                    issues.Add("mergeIntoCustomerId: ต้องเป็นข้อความหรือ null");
                    break;
            }
        }
        if (body.TryGetProperty("backdated", out var bd))
        {
            dto.HasBackdated = true;
            if (bd.ValueKind == JsonValueKind.True) dto.Backdated = true;
            else if (bd.ValueKind == JsonValueKind.False) dto.Backdated = false;
            else issues.Add("backdated: ต้องเป็น true หรือ false");
        }
        return issues.Count == 0;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 5. Effort (effort.ts) — เวลาที่ใช้ต่อลูกค้า · เวลานาฬิกา ห้ามคิดราคาต่อชั่วโมง
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>หนึ่งแถว = หนึ่งกลุ่มลูกค้า (รหัสหลัก) · ตัวเลขเวลาเป็น null เมื่อกลุ่มติดธงบันทึกย้อนหลัง (EffortRow)</summary>
public sealed class NisEffortRowDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>จำนวนเคสที่ **ปิดได้** ในช่วง — ไม่ใช่แจ้งเข้า</summary>
    public int ResolvedCount { get; set; }
    public double? TotalHours { get; set; }
    public double? MedianHours { get; set; }
    public int HoursN { get; set; }
    /// <summary>สัดส่วนของ totalHours เทียบผลรวม (%) — ตัวหารไม่รวมกลุ่มที่ติดธง</summary>
    public double? SharePct { get; set; }
    public bool Backdated { get; set; }
}

/// <summary>จุดบนกราฟแนวโน้มรายเดือน — เฉพาะกลุ่มที่ไม่ติดธง ทั้งชั่วโมงและจำนวนเคส (ชุดแถวเดียวกันทั้งสองแถบ)</summary>
public sealed class NisEffortTrendPointDto
{
    /// <summary>'YYYY-MM' ค.ศ. ตามปฏิทินไทย</summary>
    public string MonthKey { get; set; } = string.Empty;
    public double Hours { get; set; }
    public int ResolvedCount { get; set; }
}

/// <summary>จำนวนเคสปิดได้ตามประเภทงาน — **รวมทุกกลุ่ม รวมกลุ่มที่ติดธง** (ธงทำให้เวลาเชื่อไม่ได้ ไม่ใช่จำนวน)</summary>
public sealed class NisEffortCategoryRowDto
{
    public string Category { get; set; } = string.Empty;
    public int ResolvedCount { get; set; }
}

/// <summary>หนึ่งช่องของฮิสโทแกรมเวลานาฬิกา — ขอบและป้ายต้องตรงกับ durationBuckets ของ summary เป๊ะ (SPEC §9 ข้อ 66)</summary>
public sealed class NisEffortDistributionBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int N { get; set; }
}

/// <summary>ผลของ NisEffortService (EffortOverview) — rows เป็น data · ที่เหลือลง meta</summary>
public sealed class NisEffortOverviewDto
{
    public List<NisEffortRowDto> Rows { get; set; } = new();
    public int ResolvedCount { get; set; }
    public double TotalHours { get; set; }
    public double? MedianHours { get; set; }
    public int CustomerCount { get; set; }
    public int ExcludedBackdatedCases { get; set; }
    public List<NisEffortTrendPointDto> Trend { get; set; } = new();
    public List<NisEffortCategoryRowDto> Categories { get; set; } = new();
    public List<NisEffortDistributionBucketDto> Distribution { get; set; } = new();
    public int TrustedCasesWithoutHours { get; set; }

    /// <summary>meta ตาม effort.controller.ts (ไม่รวม total/dataAsOf ที่ envelope ใส่ให้)</summary>
    public Dictionary<string, object?> ToMeta() => new()
    {
        ["resolvedCount"] = ResolvedCount,
        ["totalHours"] = TotalHours,
        ["medianHours"] = MedianHours,
        ["customerCount"] = CustomerCount,
        ["excludedBackdatedCases"] = ExcludedBackdatedCases,
        ["trend"] = Trend,
        ["categories"] = Categories,
        ["distribution"] = Distribution,
        ["trustedCasesWithoutHours"] = TrustedCasesWithoutHours,
        ["hoursKind"] = NisAnalyticsText.ClockKindScreen,
    };
}

// ═══════════════════════════════════════════════════════════════════════════
// 6. KB (kb.ts) — งานที่แจ้งซ้ำ & คู่มือแก้ปัญหา · คัดด้วย createdAt
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>หนึ่งหัวข้อที่แจ้งซ้ำ (KbTopicRow) — จัดกลุ่มด้วยหัวข้อที่ตัดช่องว่าง/ตัวพิมพ์ แสดงรูปที่พบบ่อยสุด</summary>
public sealed class NisKbTopicRowDto
{
    public string Subject { get; set; } = string.Empty;
    /// <summary>นับจาก createdAt (งานที่แจ้งเข้า)</summary>
    public int IntakeCount { get; set; }
    public int CustomerCount { get; set; }
    /// <summary>เวลานาฬิการวม (ชม.) — null เมื่อไม่มีเคสในกลุ่มที่มีเวลาให้คิด</summary>
    public double? ClockHours { get; set; }
    /// <summary>จำนวนเคสที่บันทึกวิธีแก้ตั้งแต่ detailedChars ตัวอักษรขึ้นไป</summary>
    public int DetailedCount { get; set; }
}

/// <summary>ผลของ NisKbService (KbOverview) — topics เป็น data · ที่เหลือลง meta</summary>
public sealed class NisKbOverviewDto
{
    public List<NisKbTopicRowDto> Topics { get; set; } = new();
    public int IntakeCount { get; set; }
    public int RepeatedTopics { get; set; }
    public int DetailedCount { get; set; }
    public int DetailedChars { get; set; }
    /// <summary>เปอร์เซ็นต์สะสม (Pareto) จาก**ทุกหัวข้อ** ไม่ใช่แค่ที่ส่งกลับ — ตัวสุดท้ายเป็น 100 เสมอ</summary>
    public List<double> CumulativePct { get; set; } = new();

    /// <summary>meta ตาม kb.controller.ts</summary>
    public Dictionary<string, object?> ToMeta() => new()
    {
        ["intakeCount"] = IntakeCount,
        ["repeatedTopics"] = RepeatedTopics,
        ["detailedCount"] = DetailedCount,
        ["detailedChars"] = DetailedChars,
        ["cumulativePct"] = CumulativePct,
    };
}

// ═══════════════════════════════════════════════════════════════════════════
// 7. Workload (workload.ts) — ที่เดียวในระบบที่มีตัวเลขรายบุคคล (เฉพาะ role mng)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// ตัวเลขผลงานรายบุคคล (WorkloadRow) — สามชนิดอย่าปน: ผูกช่วง (resolvedInPeriod) · YTD (resolvedYtd · ค่ากลาง · คะแนน) · สแนปช็อต (openNow)
/// เคสหนึ่งใบนับให้ได้หลายคน (assigneeNames เป็น array) · เคสของกลุ่มติดธงนับเข้าจำนวนงานแต่ไม่เข้าเวลา
/// </summary>
public sealed class NisWorkloadRowDto
{
    public string AgentName { get; set; } = string.Empty;
    public int ResolvedInPeriod { get; set; }
    public int ResolvedYtd { get; set; }
    public int OpenNow { get; set; }
    /// <summary>ค่ากลางเวลานาฬิกา (ชม.) ของเคสที่ปิดได้ตั้งแต่ ม.ค. — ป้าย "แจ้ง→บันทึกแก้ล่าสุด" เสมอ</summary>
    public double? ResolutionHoursMedian { get; set; }
    public int ResolutionHoursN { get; set; }
    /// <summary>เคสปิดได้ตั้งแต่ ม.ค. ที่ถูกตัดออกจากค่ากลางเพราะกลุ่มลูกค้าติดธงบันทึกย้อนหลัง</summary>
    public int ExcludedBackdatedCount { get; set; }
    public double? SatisfactionAvg { get; set; }
    public int SatisfactionN { get; set; }
}

/// <summary>ผลของ NisWorkloadService (WorkloadOverview) — rows เป็น data · ที่เหลือลง meta</summary>
public sealed class NisWorkloadOverviewDto
{
    public List<NisWorkloadRowDto> Rows { get; set; } = new();
    public int YearBe { get; set; }
    /// <summary>เคสปิดได้ในช่วงทั้งทีม นับไม่ซ้ำ (ต่างจากผลรวม resolvedInPeriod ที่เคสมอบหมายร่วมถูกนับซ้ำ)</summary>
    public int TeamResolvedInPeriod { get; set; }
    /// <summary>month | quarter | year</summary>
    public string Period { get; set; } = "month";
    /// <summary>ขอบช่วงที่คำนวณจริง (ISO UTC) — ให้เทียบกับ SQL อิสระได้</summary>
    public string PeriodFrom { get; set; } = string.Empty;
    public string PeriodTo { get; set; } = string.Empty;

    /// <summary>meta ตาม workload.controller.ts</summary>
    public Dictionary<string, object?> ToMeta() => new()
    {
        ["yearBe"] = YearBe,
        ["teamResolvedInPeriod"] = TeamResolvedInPeriod,
        ["period"] = Period,
        ["periodLabel"] = NisAnalyticsText.WorkloadPeriodLabel.TryGetValue(Period, out var l) ? l : Period,
        ["periodFrom"] = PeriodFrom,
        ["periodTo"] = PeriodTo,
    };
}
