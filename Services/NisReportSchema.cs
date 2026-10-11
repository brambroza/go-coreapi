using System.Globalization;
using System.Text.RegularExpressions;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using Newtonsoft.Json.Linq;

namespace goalongapi.Services;

// ── NisReportSchema ─────────────────────────────────────────────────────────
// ฝั่ง server ของ packages/report-schema (zod) — ตรวจรูป layout ก่อนเขียน · projectionOf · starterTemplate
// · ด่านทั้งหก (holiday · truncation · freshness · printability · salesContact · coverContrast → NisReportCoverContrast)
// · pickKpis / pickDurationDataset / pickCaseRow · assertSnapshotWithinProjection (กฎ Q25)
//
// 🔴 ไม่ port zod ทั้งก้อน — port เฉพาะส่วนที่ server ต้องตัดสินจริง (MERGE_PLAN §5.4) แต่ครอบทุก field ที่
//    server อ่าน และเติมค่าเริ่มต้นให้เหมือน zod `.default()` เพื่อให้ LayoutJson ที่เก็บครบเหมือนฝั่ง TS
// 🔴 ข้อความภาษาไทยคัดลอกจาก TS ตรงตัว — เป็นถ้อยคำที่ผู้ใช้/เอกสารลูกค้าเห็น ห้ามเปลี่ยนเอง
// ทุกอย่างทำบน Newtonsoft JObject · ขอบ HTTP แปลงผ่าน NisReportJson

/// <summary>นิยามคอลัมน์ของตารางเคส (CaseColumnDef) — แหล่งเดียวของลำดับมาตรฐาน · reads · ธงชื่อคน</summary>
public sealed record NisCaseColumnDef(string Id, string Label, string LabelShort, int W, bool Fix, string[] Reads, bool Personal, string? Side);

/// <summary>นิยาม KPI (KpiMetricDef) — path ชี้เข้า SummaryBlock · momKey ชี้เข้า MomBlock.kpis</summary>
public sealed record NisKpiMetricDef(string Id, string Label, string Kind, string Path, string MomKey, bool NeedsHoliday);

/// <summary>คำขอข้อมูลตารางเคสหนึ่งรายการ (CaseRequest) — หนึ่งตารางที่ตั้งค่าต่างกัน = หนึ่งคำขอ</summary>
public sealed class NisCaseRequest
{
    public string Key { get; init; } = string.Empty;
    public List<string> Columns { get; init; } = new();
    public HashSet<string> Fields { get; init; } = new(StringComparer.Ordinal);
    public int RowLimit { get; init; }
    /// <summary>createdAt | resolvedAt</summary>
    public string DateField { get; init; } = "createdAt";
    public string Sort { get; init; } = "createdAt";
    public string Order { get; init; } = "asc";
}

/// <summary>ความต้องการข้อมูลของช่วงเวลาหนึ่งช่วง (PeriodNeed)</summary>
public sealed class NisPeriodNeed
{
    public string Key { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public List<string> KpiMetrics { get; init; } = new();
    public List<string> Datasets { get; init; } = new();
    /// <summary>clock | business | null = ไม่มีกราฟการกระจายเวลาในช่วงนี้</summary>
    public string? DurationBasis { get; init; }
    public List<NisCaseRequest> CaseRequests { get; init; } = new();

    /// <summary>คำขอรายการแรก — ช่องเดี่ยวของ TS คงไว้ให้รายงานก่อน 14 ก.ย. 2569 อ่านได้</summary>
    public NisCaseRequest? First => CaseRequests.Count > 0 ? CaseRequests[0] : null;
    public List<string> CaseColumns => First?.Columns ?? new List<string>();
    public HashSet<string> CaseFields => First?.Fields ?? new HashSet<string>(StringComparer.Ordinal);
    public int CaseRowLimit => First?.RowLimit ?? 0;
    public string CaseDateField => First?.DateField ?? "createdAt";

    /// <summary>ช่วงนี้ต้องดึงข้อมูลไหม — ช่วงของเอกสารที่ไม่มีบล็อกไหนใช้ไม่ต้อง</summary>
    public bool UsesData => KpiMetrics.Count > 0 || Datasets.Count > 0 || CaseRequests.Count > 0;
}

/// <summary>ผลของ projectionOf (Projection)</summary>
public sealed class NisProjection
{
    public string DocFrom { get; init; } = string.Empty;
    public string DocTo { get; init; } = string.Empty;
    public List<NisPeriodNeed> Needs { get; init; } = new();
    /// <summary>true = มีบล็อกที่ใช้ช่วงต่างจากเอกสาร → หน้ากระดาษต้องมีป้ายบอก</summary>
    public bool HasOverride { get; init; }

    /// <summary>ความต้องการของช่วงหนึ่ง · โยนถ้าไม่มี เพราะแปลว่า projection กับ layout หลุดกัน</summary>
    public NisPeriodNeed NeedFor(string key) =>
        Needs.FirstOrDefault(n => n.Key == key)
        ?? throw new InvalidOperationException($"ไม่พบความต้องการข้อมูลของช่วง \"{key}\" — projection กับ layout หลุดกัน");
}

/// <summary>ตัวเลือกของ starterTemplate (StarterOptions)</summary>
public sealed class NisStarterOptions
{
    public string CustomerId { get; init; } = string.Empty;
    public string CustomerNameTh { get; init; } = string.Empty;
    public string? CustomerNameEn { get; init; }
    public string? PeriodLabelTh { get; init; }
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    /// <summary>จำนวนเคสจริงของลูกค้ารายนี้ในช่วงนี้ — null = ผู้เรียกยังไม่รู้ (ฝั่ง API รู้เสมอ ต้องส่ง)</summary>
    public int? CaseCount { get; init; }
    /// <summary>'service' = เอกสารบริการ NIS (แม่แบบ MA) · ใช้กับ book เท่านั้น</summary>
    public string? Preset { get; init; }
}

/// <summary>จุดที่ snapshot ผิดกฎ Q25 (SnapshotViolation)</summary>
public sealed record NisSnapshotViolation(string Where, string Message);

/// <summary>
/// กฎของ layout/snapshot ฝั่ง server — port จาก packages/report-schema/src (template · blocks · doc-meta · datasets
/// · columns · metrics · period · holiday-gate · company · snapshot · defaults)
/// </summary>
public static class NisReportSchema
{
    // ═══════════════════════════════════════════════════════════════════════
    // 1. ค่าคงที่ (enum ของ schema)
    // ═══════════════════════════════════════════════════════════════════════

    public const int SchemaVersion = 1;
    public const int SnapshotVersion = 2;
    public static readonly int[] ReadableSnapshotVersions = { 1, 2 };

    /// <summary>เพดานจำนวนแถวของตารางเคสหนึ่งบล็อก — กันพัง ไม่ใช่กติกาธุรกิจ (SPEC §9 ข้อ 24)</summary>
    public const int CaseRowCeiling = 1000;

    public static readonly string[] DocKinds = { "book", "brief" };
    public static readonly string[] BlockTypes = { "cover", "heading", "text", "kpiRow", "chart", "caseTable", "signature", "salesContact", "titlePage" };
    /// <summary>บล็อกที่มีได้อย่างมากหนึ่งใบต่อรายงาน</summary>
    public static readonly string[] SingletonBlocks = { "cover", "signature", "salesContact" };

    public static readonly Dictionary<string, string> BlockLabelTh = new(StringComparer.Ordinal)
    {
        ["cover"] = "หน้าปก",
        ["heading"] = "หัวข้อ",
        ["text"] = "ข้อความ",
        ["kpiRow"] = "แถวตัวเลข KPI",
        ["chart"] = "กราฟ",
        ["caseTable"] = "ตารางรายการงานบริการ",
        ["signature"] = "ช่องลงนาม",
        ["salesContact"] = "ติดต่อฝ่ายขาย",
        ["titlePage"] = "หน้าคั่น",
    };

    public static readonly string[] CoverStyles = { "network", "solid", "clean", "split", "framed", "fullbleed", "corner", "minimal", "service" };
    public static readonly string[] CoverThemes = { "nis", "navy", "emerald", "wine", "graphite", "violet", "custom" };
    /// <summary>ฟอนต์ที่สคีมายอมรับ — รวมตัวที่เลิกให้เลือกแล้ว (ห้ามลบ · template เก่าอาจเก็บชื่อนั้น)</summary>
    public static readonly string[] ReportFonts = { "Sarabun", "Noto Sans Thai", "Prompt", "IBM Plex Sans Thai", "Trirong", "TH Sarabun New" };
    /// <summary>ฟอนต์ที่ฝังในภาพจริงและให้เลือกได้</summary>
    public static readonly string[] SelectableFonts = { "Sarabun", "Noto Sans Thai", "Prompt", "IBM Plex Sans Thai", "Trirong" };
    public static readonly string[] CoverLineWeights = { "normal", "medium", "bold", "black" };
    public static readonly string[] FooterLogoSizes = { "small", "medium", "large" };
    public static readonly string[] FooterCompanyNames = { "en", "th", "both" };
    public static readonly string[] ColumnJoinLayouts = { "inline", "stack" };
    public static readonly string[] LineClamps = { "auto", "one", "two", "three", "full" };
    public static readonly string[] CaseTableModes = { "table", "twoLine", "card" };
    public static readonly string[] CaseSortBy = { "createdAt", "resolvedAt", "priority", "resolutionHours" };
    public static readonly string[] StarterPresets = { "service" };

    public static readonly string[] DatasetIds =
    {
        "trend.day", "trend.month", "breakdown.priority", "breakdown.category", "breakdown.customer",
        "breakdown.status", "breakdown.duration", "heatmap",
    };
    public static readonly string[] ChartTypes = { "line", "bar", "donut", "stacked", "heatmap", "histogram", "column" };
    public static readonly string[] ChartColors = { "auto", "multi", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8" };

    /// <summary>ชนิดกราฟที่ใช้กับแต่ละแหล่งข้อมูลได้ — ตัวแรกคือค่าตั้งต้น (CHART_COMPAT)</summary>
    public static readonly Dictionary<string, string[]> ChartCompat = new(StringComparer.Ordinal)
    {
        ["trend.day"] = new[] { "line", "bar" },
        ["trend.month"] = new[] { "line", "bar" },
        ["breakdown.priority"] = new[] { "donut", "bar", "stacked", "column" },
        ["breakdown.category"] = new[] { "bar", "donut", "stacked", "column" },
        ["breakdown.customer"] = new[] { "bar", "column" },
        ["breakdown.status"] = new[] { "stacked", "bar", "donut", "column" },
        ["breakdown.duration"] = new[] { "histogram", "column" },
        ["heatmap"] = new[] { "heatmap" },
    };

    /// <summary>ชื่อตั้งต้นของกราฟตามแหล่งข้อมูล (DEFAULT_CHART_TITLES) — ถ้อยคำที่คุณ Tone ตัดสินแล้ว ห้ามเปลี่ยน</summary>
    public static readonly Dictionary<string, string> DefaultChartTitles = new(StringComparer.Ordinal)
    {
        ["trend.day"] = "งานที่แจ้งเข้ากับงานที่ปิดได้ เทียบกันรายวัน",
        ["trend.month"] = "งานที่แจ้งเข้ากับงานที่ปิดได้ เทียบกันรายเดือน",
        ["breakdown.priority"] = "งานแต่ละระดับความสำคัญมีมากน้อยแค่ไหน",
        ["breakdown.category"] = "งานประเภทไหนมีมากที่สุด",
        ["breakdown.customer"] = "ลูกค้ารายไหนมีงานมากที่สุด",
        ["breakdown.status"] = "งานแต่ละสถานะมีมากน้อยแค่ไหน",
        ["breakdown.duration"] = "งานหนึ่งเรื่องใช้ระยะเวลาแก้ไขนานแค่ไหน",
        ["heatmap"] = "ลูกค้าแจ้งงานเข้ามาวันไหน เวลาไหนมากที่สุด",
    };

    /// <summary>คำบรรยายใต้กราฟที่ระบบตั้งให้ (DEFAULT_CHART_CAPTIONS)</summary>
    public static readonly Dictionary<string, string> DefaultChartCaptions = new(StringComparer.Ordinal)
    {
        ["trend.day"] = "ถ้างานที่ปิดได้มากกว่างานที่แจ้งเข้า งานค้างของวันนั้นลดลง · ถ้าน้อยกว่า มีงานยกไปวันถัดไป",
        ["trend.month"] = "ถ้างานที่ปิดได้มากกว่างานที่แจ้งเข้า งานค้างของเดือนนั้นลดลง · ถ้าน้อยกว่า มีงานยกไปเดือนถัดไป",
        ["breakdown.priority"] = "",
        ["breakdown.category"] = "",
        ["breakdown.customer"] = "",
        ["breakdown.status"] = "",
        ["breakdown.duration"] = "ไม่ใช่เวลาที่ทีมลงมือทำจริง",
        ["heatmap"] = "",
    };

    public const string BriefIntakeChartTitle = "งานที่แจ้งเข้า แยกตามระดับความสำคัญ";
    public const string BriefIntakeChartCaption = "สัดส่วนของงานที่ลูกค้าแจ้งเข้ามาในรอบนี้ แยกตามความเร่งด่วน";

    /// <summary>ศัพท์เรื่องเวลาบนกระดาษ (time-terms.ts) — คำตัดสินคุณ Tone 16 ก.ย. 2569</summary>
    public const string DurationTerm = "ระยะเวลาแก้ไข";
    public const string DurationTermBusiness = "ระยะเวลาแก้ไขในเวลาทำการ";

    /// <summary>ถ้อยคำธงบันทึกย้อนหลัง (DURATION_HIDDEN_REASON ใน @nsr/shared) — reviewer อนุมัติ 26 ส.ค. 2569</summary>
    public const string DurationHiddenReason =
        "รายงานฉบับนี้ไม่แสดงตัวเลขระยะเวลาการทำงาน เนื่องจากงานของท่านบางส่วน" +
        "บันทึกเข้าระบบหลังปฏิบัติงานจริง ตัวเลขเวลาที่คำนวณจากบันทึกจึงไม่ตรงกับ" +
        "เวลาที่ใช้ทำงานจริง · จำนวนงานทั้งหมดในรายงานนี้ถูกต้องครบถ้วน";

    /// <summary>ค่า contactId ที่หมายถึง "เลือกตามลูกค้าของเอกสาร"</summary>
    public const string SalesContactAuto = "auto";
    /// <summary>วลีในข้อความตั้งต้นของหน้าติดต่อฝ่ายขาย ที่อ้างว่าผู้ติดต่อเป็นคนดูแลลูกค้าของเอกสาร</summary>
    public const string SalesNoteClaim = "ผู้ดูแลลูกค้ารายนี้";
    public const string SalesContactDefaultNote =
        "หากมีข้อสงสัยเกี่ยวกับรายงานฉบับนี้ ไม่ว่าจะเป็นขอบเขตบริการ การต่ออายุ การเพิ่มอุปกรณ์ หรือขอใบเสนอราคาใหม่ กรุณาติดต่อเจ้าหน้าที่ NIS ผู้ดูแลลูกค้ารายนี้";

    /// <summary>ช่องที่ต้องกรอกก่อนขึ้น production (COMPANY_REQUIRED_BEFORE_PRODUCTION) — ไม่ใช่ช่องบังคับของ schema</summary>
    public static readonly string[] CompanyRequiredBeforeProduction = { "nameTh", "addressTh", "tel", "email", "website" };

    /// <summary>ป้ายของตัวเลขสองก้อน (SPEC 9.1)</summary>
    public const string IntakeLabel = "งานที่แจ้งเข้า";
    public const string ResolvedLabel = "งานที่ปิดได้";

    public static readonly Regex CustomerIdRegex = new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);
    public static readonly Regex ThaiDateRegex = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);
    public static readonly Regex HexColorRegex = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);
    /// <summary>
    /// path ของรูปที่ระบบเก็บให้ — ต้องเป็นรูปแบบที่ NisReportAssetStorage สร้างเท่านั้น (กัน SSRF ตอนเรนเดอร์ PDF)
    /// รับ `/api/logos/…` ของระบบเดิมด้วย เพื่อให้ข้อมูลที่ import จาก Postgres (slice 10) ยังอ่านผ่าน
    /// </summary>
    public static readonly Regex AssetPathRegex = new(@"^/api/(nis/report/assets|logos)/[A-Za-z0-9_-]{1,80}\.(png|jpg|webp|svg)$", RegexOptions.Compiled);

    /// <summary>คลังคอลัมน์ 18 คอลัมน์ — เรียงตามลำดับมาตรฐาน (CASE_COLUMNS)</summary>
    public static readonly NisCaseColumnDef[] CaseColumns =
    {
        new("caseNo", "เลขที่", "เลขที่", 12, true, new[] { "caseNo" }, false, null),
        new("createdDate", "วันที่แจ้ง", "วันแจ้ง", 10, true, new[] { "createdAt" }, false, null),
        new("createdTime", "เวลาแจ้ง", "เวลาแจ้ง", 8, true, new[] { "createdAt" }, false, null),
        new("createdDateTime", "วันเวลาที่แจ้ง", "แจ้งเมื่อ", 12, true, new[] { "createdAt" }, false, null),
        new("resolvedDate", "วันที่ปิด", "วันปิด", 10, true, new[] { "resolvedAt" }, false, null),
        new("resolvedTime", "เวลาปิด", "เวลาปิด", 8, true, new[] { "resolvedAt" }, false, null),
        new("resolvedDateTime", "วันเวลาที่ปิด", "ปิดเมื่อ", 12, true, new[] { "resolvedAt" }, false, null),
        new("requestBy", "ผู้แจ้ง", "ผู้แจ้ง", 14, false, new[] { "requestBy" }, true, "customer"),
        new("contactName", "ผู้ติดต่อ", "ผู้ติดต่อ", 14, false, new[] { "contactName" }, true, "customer"),
        new("assignee", "ผู้ดำเนินการ", "ผู้ทำ", 14, false, new[] { "primaryAssignee" }, true, "nis"),
        new("branchName", "สาขา", "สาขา", 10, false, new[] { "branchName" }, false, null),
        new("subject", "เรื่องที่แจ้ง", "เรื่อง", 22, false, new[] { "subject" }, false, null),
        new("actionDetails", "วิธีแก้ไขปัญหา", "วิธีแก้", 21, false, new[] { "actionDetails" }, false, null),
        new("category", "ประเภท", "ประเภท", 12, false, new[] { "category" }, false, null),
        new("priority", "ระดับ", "ระดับ", 9, true, new[] { "priority" }, false, null),
        new("status", "สถานะ", "สถานะ", 13, true, new[] { "status" }, false, null),
        new("resolutionHours", DurationTerm, DurationTerm, 11, true, new[] { "resolutionHours" }, false, null),
        new("satisfactionScore", "คะแนน", "คะแนน", 8, true, new[] { "satisfactionScore" }, false, null),
    };

    public static readonly string[] ColumnIds = CaseColumns.Select(c => c.Id).ToArray();
    private static readonly Dictionary<string, NisCaseColumnDef> ColumnById = CaseColumns.ToDictionary(c => c.Id, StringComparer.Ordinal);

    /// <summary>ชุดสำเร็จรูป (COLUMN_PRESETS) — assignee ห้ามอยู่ใน preset ไหนเลย (SPEC §9 ข้อ 68)</summary>
    public static readonly Dictionary<string, string[]> ColumnPresets = new(StringComparer.Ordinal)
    {
        ["brief"] = new[] { "caseNo", "createdDate", "subject", "priority", "resolutionHours" },
        ["standard"] = new[] { "caseNo", "createdDate", "subject", "actionDetails", "priority", "resolutionHours" },
        ["detailed"] = new[] { "caseNo", "createdDateTime", "resolvedDateTime", "requestBy", "subject", "actionDetails", "priority", "status", "resolutionHours" },
        ["contact"] = new[] { "caseNo", "createdDate", "requestBy", "contactName", "branchName", "subject", "priority", "resolutionHours" },
    };

    /// <summary>คลัง KPI 13 ตัว (KPI_METRICS) — ไม่มี SLA โดยตั้งใจ (CLAUDE.md ข้อ 12)</summary>
    public static readonly NisKpiMetricDef[] KpiMetrics =
    {
        new("resolvedCount", "งานที่ปิดได้", "count", "resolved.count", "resolvedCount", false),
        new("intakeCount", "งานที่แจ้งเข้า", "count", "intake.count", "intakeCount", false),
        new("intakeCustomers", "ลูกค้าที่แจ้งเข้า", "count", "intake.customers", "intakeCustomers", false),
        new("resolvedCustomers", "ลูกค้าที่มีงานปิดได้", "count", "resolved.customers", "resolvedCustomers", false),
        new("intakeOffHours", "งานที่แจ้งเข้านอกเวลาทำการ", "count", "intake.offHours.total", "intakeOffHours", true),
        new("clockHoursMedian", $"{DurationTerm}ของงานที่ปิดได้ — ค่ากลาง", "clockHours", "resolved.clockHours.median", "clockHoursMedian", false),
        new("clockHoursMean", $"{DurationTerm}ของงานที่ปิดได้ — เฉลี่ย", "clockHours", "resolved.clockHours.mean", "clockHoursMean", false),
        new("clockHoursP90", $"{DurationTerm}ของงานที่ปิดได้ — 90% ปิดภายใน", "clockHours", "resolved.clockHours.p90", "clockHoursP90", false),
        new("businessHoursMedian", $"{DurationTermBusiness}ของงานที่ปิดได้ — ค่ากลาง", "businessHours", "resolved.businessHours.median", "businessHoursMedian", true),
        new("businessHoursMean", $"{DurationTermBusiness}ของงานที่ปิดได้ — เฉลี่ย", "businessHours", "resolved.businessHours.mean", "businessHoursMean", true),
        new("businessHoursP90", $"{DurationTermBusiness}ของงานที่ปิดได้ — 90% ปิดภายใน", "businessHours", "resolved.businessHours.p90", "businessHoursP90", true),
        new("satisfactionAvg", "ความพึงพอใจ (จากงานที่ปิดได้)", "score", "resolved.satisfaction.avg", "satisfactionAvg", false),
        new("satisfactionRated", "งานที่ปิดได้ซึ่งลูกค้าให้คะแนน", "count", "resolved.satisfaction.rated", "satisfactionRated", false),
    };

    public static readonly string[] KpiMetricIds = KpiMetrics.Select(m => m.Id).ToArray();
    private static readonly Dictionary<string, NisKpiMetricDef> KpiById = KpiMetrics.ToDictionary(m => m.Id, StringComparer.Ordinal);

    /// <summary>คอลัมน์หนึ่งตัว — โยนถ้าไม่รู้จัก (คอลัมน์ที่หายไปคือช่องว่างในเอกสารที่ส่งลูกค้า)</summary>
    public static NisCaseColumnDef CaseColumn(string id) =>
        ColumnById.TryGetValue(id, out var c) ? c : throw new InvalidOperationException($"ไม่รู้จักคอลัมน์ \"{id}\" — ดูรายการที่รองรับใน CASE_COLUMN_IDS");

    /// <summary>KPI หนึ่งตัว — โยนถ้าไม่รู้จัก</summary>
    public static NisKpiMetricDef KpiMetric(string id) =>
        KpiById.TryGetValue(id, out var m) ? m : throw new InvalidOperationException($"ไม่รู้จัก KPI \"{id}\" — ดูรายการที่รองรับใน KPI_METRIC_IDS");

    /// <summary>จัดคอลัมน์ให้เรียงตามลำดับมาตรฐานและตัดตัวซ้ำ (กติกา 5.6 ข้อ 1)</summary>
    public static List<string> OrderCaseColumns(IEnumerable<string> ids)
    {
        var want = new HashSet<string>(ids, StringComparer.Ordinal);
        return CaseColumns.Where(c => want.Contains(c.Id)).Select(c => c.Id).ToList();
    }

    /// <summary>ป้ายของคอลัมน์ที่มีชื่อคน (personal) — คำเตือน E1 บนหน้าออกแบบ</summary>
    public static List<string> PersonalColumnLabels(IEnumerable<string> ids) =>
        OrderCaseColumns(ids).Where(id => CaseColumn(id).Personal).Select(id => CaseColumn(id).Label).ToList();

    /// <summary>คีย์ของช่วงเวลา — ดัชนีใน dataSnapshot (periodKey)</summary>
    public static string PeriodKey(string from, string to) => $"{from}..{to}";

    /// <summary>คีย์ของคำขอตารางเคส (caseRequestKey) — ฝั่งสร้างและฝั่งวาดต้องประกอบตรงกันตัวอักษรเดียว</summary>
    public static string CaseRequestKey(string? rowSet, string? sortBy, string? sortDir) =>
        $"{rowSet ?? "intake"}|{sortBy ?? "createdAt"}|{sortDir ?? "asc"}";

    /// <summary>ชนิดกราฟตั้งต้นของแหล่งข้อมูล — ตัวแรกของ CHART_COMPAT</summary>
    public static string DefaultChartType(string dataset) =>
        ChartCompat.TryGetValue(dataset, out var list) && list.Length > 0
            ? list[0]
            : throw new InvalidOperationException($"แหล่งข้อมูล \"{dataset}\" ไม่มีชนิดกราฟที่ใช้ได้เลย");

    // ═══════════════════════════════════════════════════════════════════════
    // 2. ตรวจรูป layout + เติมค่าเริ่มต้น (TemplateSchema.parse)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ตรวจ layout และคืนสำเนาที่เติมค่าเริ่มต้นครบ (เหมือน zod parse) · issues ว่าง = ผ่าน
    /// ข้อความแต่ละบรรทัดอยู่ในรูป <c>path: ข้อความ</c> เหมือน `${i.path.join('.')}: ${i.message}` ของ TS
    /// </summary>
    /// <returns>layout ที่ normalize แล้ว (ใช้ได้เฉพาะเมื่อ issues ว่าง)</returns>
    public static JObject Normalize(JToken? raw, out List<string> issues)
    {
        issues = new List<string>();
        if (raw is not JObject src)
        {
            issues.Add("(ราก): ต้องเป็น JSON object");
            return new JObject();
        }
        var t = (JObject)src.DeepClone();

        // schemaVersion — literal 1 (default 1)
        if (t["schemaVersion"] is { Type: not JTokenType.Null } sv)
        {
            if (sv.Type != JTokenType.Integer || (int)sv != SchemaVersion) issues.Add($"schemaVersion: ต้องเป็น {SchemaVersion}");
        }
        t["schemaVersion"] = SchemaVersion;

        EnumField(t, "docKind", DocKinds, "book", "docKind", issues);

        // page
        var page = ObjField(t, "page", "page", issues);
        if (page != null)
        {
            if (page["size"] is { Type: not JTokenType.Null } size && (size.Type != JTokenType.String || (string?)size != "A4"))
                issues.Add("page.size: ต้องเป็น A4");
            page["size"] = "A4";
            EnumField(page, "orientation", new[] { "portrait", "landscape" }, "portrait", "page.orientation", issues);
            var margin = ObjField(page, "margin", "page.margin", issues);
            if (margin != null)
            {
                NumField(margin, "top", 0, 120, 40, "page.margin.top", issues);
                NumField(margin, "right", 0, 120, 33.5, "page.margin.right", issues);
                NumField(margin, "bottom", 0, 120, 44, "page.margin.bottom", issues);
                NumField(margin, "left", 0, 120, 33.5, "page.margin.left", issues);
            }
            var header = ObjField(page, "header", "page.header", issues);
            if (header != null)
            {
                BoolField(header, "enabled", false, "page.header.enabled", issues);
                BoolField(header, "showLogo", true, "page.header.showLogo", issues);
                StrField(header, "text", 0, 200, "", "page.header.text", issues);
                BoolField(header, "showContractNo", true, "page.header.showContractNo", issues);
                BoolField(header, "showIssueDate", true, "page.header.showIssueDate", issues);
            }
            var footer = ObjField(page, "footer", "page.footer", issues);
            if (footer != null)
            {
                BoolField(footer, "enabled", true, "page.footer.enabled", issues);
                BoolField(footer, "showLogo", true, "page.footer.showLogo", issues);
                BoolField(footer, "showPageNumber", true, "page.footer.showPageNumber", issues);
                StrField(footer, "text", 0, 200, "", "page.footer.text", issues);
                EnumField(footer, "logoSize", FooterLogoSizes, "medium", "page.footer.logoSize", issues);
                EnumField(footer, "companyName", FooterCompanyNames, "en", "page.footer.companyName", issues);
            }
        }

        // theme
        var theme = ObjField(t, "theme", "theme", issues);
        if (theme != null)
        {
            EnumField(theme, "fontFamily", ReportFonts, "Sarabun", "theme.fontFamily", issues);
            NumField(theme, "baseFontSize", 8, 14, 11, "theme.baseFontSize", issues);
        }

        // period — บังคับ
        if (t["period"] is not JObject period)
        {
            issues.Add("period: ต้องระบุ");
            period = null!;
        }
        else
        {
            ValidatePeriod(period, "period", issues);
        }

        // docMeta — บังคับ
        if (t["docMeta"] is not JObject meta)
        {
            issues.Add("docMeta: ต้องระบุ");
        }
        else
        {
            StrField(meta, "reportTitle", 1, 120, "รายงานสรุปงานบริการ", "docMeta.reportTitle", issues);
            StrField(meta, "eyebrow", 0, 60, "SERVICE REPORT", "docMeta.eyebrow", issues);
            StrField(meta, "titleAccent", 0, 60, "", "docMeta.titleAccent", issues);
            var cid = StrField(meta, "customerId", 1, 64, null, "docMeta.customerId", issues);
            if (cid != null && !CustomerIdRegex.IsMatch(cid))
                issues.Add("docMeta.customerId: รหัสลูกค้าใช้ได้เฉพาะ A-Z a-z 0-9 _ - (ค่านี้ถูกใช้เป็นชื่อไฟล์ PDF)");
            StrField(meta, "customerNameTh", 1, 160, null, "docMeta.customerNameTh", issues);
            StrField(meta, "customerNameEn", 0, 160, "", "docMeta.customerNameEn", issues);
            var logo = NullableStrField(meta, "customerLogoPath", 400, "docMeta.customerLogoPath", issues);
            if (logo != null && !AssetPathRegex.IsMatch(logo))
                issues.Add("docMeta.customerLogoPath: path โลโก้ต้องเป็นไฟล์ที่นำเข้าผ่านระบบ (/api/nis/report/assets/…)");
            StrField(meta, "contractNo", 0, 60, "", "docMeta.contractNo", issues);
            StrField(meta, "poNo", 0, 60, "", "docMeta.poNo", issues);
            var cs = NullableDateField(meta, "contractStart", "docMeta.contractStart", issues);
            var ce = NullableDateField(meta, "contractEnd", "docMeta.contractEnd", issues);
            var issueNo = NullableIntField(meta, "issueNo", 1, 999, "docMeta.issueNo", issues);
            var issueTotal = NullableIntField(meta, "issueTotal", 1, 999, "docMeta.issueTotal", issues);
            StrField(meta, "periodLabelTh", 0, 120, "", "docMeta.periodLabelTh", issues);
            StrField(meta, "periodLabelEn", 0, 120, "", "docMeta.periodLabelEn", issues);
            StrField(meta, "preparedBy", 0, 120, "", "docMeta.preparedBy", issues);
            StrField(meta, "approvedBy", 0, 120, "", "docMeta.approvedBy", issues);
            NullableDateField(meta, "preparedDate", "docMeta.preparedDate", issues);
            if (issueNo != null && issueTotal != null && issueNo > issueTotal)
                issues.Add($"docMeta.issueNo: ฉบับที่ {issueNo} มากกว่าจำนวนงวดทั้งหมด {issueTotal} — ลูกค้าใช้เลขคู่นี้ตรวจว่าได้รับครบไหม");
            if (cs != null && ce != null && string.CompareOrdinal(cs, ce) > 0)
                issues.Add("docMeta.contractEnd: วันสิ้นสุดสัญญาอยู่ก่อนวันเริ่ม");
        }

        // blocks — 1..60 · id ไม่ซ้ำ · singleton · หน้าปกเป็นบล็อกแรก
        if (t["blocks"] is not JArray blocks)
        {
            issues.Add("blocks: ต้องเป็น array ของบล็อก");
        }
        else
        {
            if (blocks.Count < 1) issues.Add("blocks: ต้องมีอย่างน้อย 1 บล็อก");
            if (blocks.Count > 60) issues.Add("blocks: มีได้ไม่เกิน 60 บล็อก");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var typeCount = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < blocks.Count; i++)
            {
                var path = $"blocks.{i}";
                if (blocks[i] is not JObject b)
                {
                    issues.Add($"{path}: ต้องเป็น object");
                    continue;
                }
                var id = StrField(b, "id", 1, 64, null, $"{path}.id", issues);
                if (id != null)
                {
                    if (!seen.Add(id))
                        issues.Add($"{path}.id: id ของบล็อกซ้ำ: \"{id}\" — undo/redo และการลากวางอ้างด้วย id นี้ ซ้ำแล้วจะแก้ผิดใบ");
                }
                var type = EnumField(b, "type", BlockTypes, null, $"{path}.type", issues);
                if (type == null) continue;
                typeCount[type] = typeCount.TryGetValue(type, out var n) ? n + 1 : 1;
                if (type == "cover" && i != 0)
                    issues.Add($"{path}: หน้าปกต้องเป็นบล็อกแรกของเอกสาร");
                var props = ObjField(b, "props", $"{path}.props", issues);
                if (props == null) continue;
                NormalizeBlockProps(type, props, $"{path}.props", issues);
            }
            foreach (var type in SingletonBlocks)
            {
                if (typeCount.TryGetValue(type, out var n) && n > 1)
                    issues.Add($"blocks: มี \"{BlockLabelTh[type]}\" {n} ใบ — ชนิดนี้มีได้อย่างมากหนึ่งใบต่อรายงาน");
            }
        }

        return t;
    }

    /// <summary>ตรวจ layout โดยไม่สนผลที่ normalize — คืนรายการข้อผิดพลาด (ว่าง = ผ่าน)</summary>
    public static List<string> Validate(JToken? raw)
    {
        Normalize(raw, out var issues);
        return issues;
    }

    private static void NormalizeBlockProps(string type, JObject p, string path, List<string> issues)
    {
        switch (type)
        {
            case "cover":
                EnumField(p, "style", CoverStyles, "network", $"{path}.style", issues);
                EnumField(p, "theme", CoverThemes, "nis", $"{path}.theme", issues);
                OptionalHexField(p, "mainColor", $"{path}.mainColor", issues);
                OptionalHexField(p, "accentColor", $"{path}.accentColor", issues);
                BoolField(p, "motif", true, $"{path}.motif", issues);
                BoolField(p, "confidential", true, $"{path}.confidential", issues);
                EnumField(p, "font", ReportFonts, "Sarabun", $"{path}.font", issues);
                if (p["lines"] is null || p["lines"]!.Type == JTokenType.Null) p["lines"] = new JArray();
                if (p["lines"] is not JArray lines)
                {
                    issues.Add($"{path}.lines: ต้องเป็น array");
                }
                else
                {
                    if (lines.Count > 40) issues.Add($"{path}.lines: มีได้ไม่เกิน 40 บรรทัด");
                    var ids = new HashSet<string>(StringComparer.Ordinal);
                    for (var i = 0; i < lines.Count; i++)
                    {
                        var lp = $"{path}.lines.{i}";
                        if (lines[i] is not JObject line) { issues.Add($"{lp}: ต้องเป็น object"); continue; }
                        var lid = StrField(line, "id", 1, 40, null, $"{lp}.id", issues);
                        if (lid != null && !ids.Add(lid)) issues.Add($"{path}.lines: มีบรรทัดที่ id ซ้ำกัน — แผงคุณสมบัติจะแก้ผิดบรรทัด");
                        OptionalBoolField(line, "show", $"{lp}.show", issues);
                        OptionalIntField(line, "size", 6, 72, $"{lp}.size", issues);
                        OptionalHexField(line, "color", $"{lp}.color", issues);
                        OptionalEnumField(line, "weight", CoverLineWeights, $"{lp}.weight", issues);
                        OptionalEnumField(line, "font", ReportFonts, $"{lp}.font", issues);
                    }
                }
                break;

            case "heading":
                StrField(p, "text", 1, 200, null, $"{path}.text", issues,
                    tooShort: "หัวข้อต้องมีข้อความ — ถ้าไม่ต้องการหัวข้อนี้ให้ลบบล็อกแทน", tooLong: "หัวข้อยาวได้ไม่เกิน 200 ตัวอักษร");
                if (p["level"] is { Type: not JTokenType.Null } lv)
                {
                    if (lv.Type != JTokenType.Integer || ((int)lv != 2 && (int)lv != 3)) issues.Add($"{path}.level: ต้องเป็น 2 หรือ 3");
                }
                else p["level"] = 2;
                BoolField(p, "rule", true, $"{path}.rule", issues);
                StrField(p, "kicker", 0, 60, "", $"{path}.kicker", issues);
                BoolField(p, "showKicker", true, $"{path}.showKicker", issues);
                break;

            case "text":
                StrField(p, "text", 0, 4000, null, $"{path}.text", issues, tooLong: "ข้อความยาวได้ไม่เกิน 4,000 ตัวอักษร");
                EnumField(p, "align", new[] { "left", "center" }, "left", $"{path}.align", issues);
                break;

            case "kpiRow":
                if (p["metrics"] is not JArray metrics)
                {
                    issues.Add($"{path}.metrics: ต้องเป็น array ของ KPI");
                }
                else
                {
                    var list = metrics.Select(m => (string?)m ?? "").ToList();
                    if (list.Count < 2) issues.Add($"{path}.metrics: แถวตัวเลขต้องมีอย่างน้อย 2 ตัว — ถอดออกอีกไม่ได้");
                    if (list.Count > 5) issues.Add($"{path}.metrics: แถวตัวเลขมีได้ไม่เกิน 5 ตัว — ถอดตัวอื่นออกก่อน แล้วจึงเลือกเพิ่ม");
                    foreach (var m in list.Where(m => !KpiMetricIds.Contains(m)))
                        issues.Add($"{path}.metrics: ไม่รู้จัก KPI \"{m}\"");
                    if (list.Distinct(StringComparer.Ordinal).Count() != list.Count)
                        issues.Add($"{path}.metrics: เลือก KPI ซ้ำกันไม่ได้ — การ์ดสองใบที่เหมือนกันทำให้ผู้อ่านคิดว่าระบบคำนวณผิด");
                }
                BoolField(p, "showComparison", true, $"{path}.showComparison", issues);
                PeriodOverrideField(p, $"{path}.periodOverride", issues);
                break;

            case "chart":
                var dataset = EnumField(p, "dataset", DatasetIds, null, $"{path}.dataset", issues);
                var chartType = EnumField(p, "chartType", ChartTypes, null, $"{path}.chartType", issues);
                if (dataset != null && chartType != null && !ChartCompat[dataset].Contains(chartType))
                    issues.Add($"{path}.chartType: ชนิดกราฟใช้กับแหล่งข้อมูลนี้ไม่ได้ — ดู CHART_COMPAT ใน datasets.ts");
                StrField(p, "title", 1, 120, null, $"{path}.title", issues,
                    tooShort: "กราฟต้องมีชื่อ — ผู้อ่านต้องรู้ว่ากราฟนี้นับอะไร", tooLong: "ชื่อกราฟยาวได้ไม่เกิน 120 ตัวอักษร");
                StrField(p, "caption", 0, 300, "", $"{path}.caption", issues);
                BoolField(p, "showLegend", true, $"{path}.showLegend", issues);
                IntField(p, "height", 80, 420, 180, $"{path}.height", issues);
                EnumField(p, "color", ChartColors, "auto", $"{path}.color", issues);
                var basis = EnumField(p, "basis", new[] { "clock", "business" }, "clock", $"{path}.basis", issues);
                if (dataset != null && dataset != "breakdown.duration" && basis == "business")
                    issues.Add($"{path}.basis: basis = \"business\" ใช้ได้เฉพาะกับ breakdown.duration — แหล่งข้อมูลอื่นไม่ได้คำนวณจากเวลาทำการ");
                EnumField(p, "series", new[] { "intake", "resolved" }, "intake", $"{path}.series", issues);
                PeriodOverrideField(p, $"{path}.periodOverride", issues);
                break;

            case "caseTable":
                StrField(p, "title", 1, 120, null, $"{path}.title", issues,
                    tooShort: "ตารางต้องมีชื่อ — ผู้อ่านต้องรู้ว่าเป็นรายการชุดไหน", tooLong: "ชื่อตารางยาวได้ไม่เกิน 120 ตัวอักษร");
                List<string> ordered = new();
                if (p["columns"] is not JArray cols)
                {
                    issues.Add($"{path}.columns: ต้องเป็น array ของคอลัมน์");
                }
                else
                {
                    var list = cols.Select(c => (string?)c ?? "").ToList();
                    if (list.Count < 1) issues.Add($"{path}.columns: ตารางต้องมีอย่างน้อย 1 คอลัมน์");
                    if (list.Count > ColumnIds.Length) issues.Add($"{path}.columns: มีได้ไม่เกิน {ColumnIds.Length} คอลัมน์");
                    foreach (var c in list.Where(c => !ColumnIds.Contains(c)))
                        issues.Add($"{path}.columns: ไม่รู้จักคอลัมน์ \"{c}\"");
                    // บังคับลำดับมาตรฐานตั้งแต่ชั้น schema (กติกา 5.6 ข้อ 1)
                    ordered = OrderCaseColumns(list);
                    p["columns"] = new JArray(ordered);
                }
                if (p["joins"] is null || p["joins"]!.Type == JTokenType.Null) p["joins"] = new JObject();
                if (p["joins"] is not JObject joins)
                {
                    issues.Add($"{path}.joins: ต้องเป็น object");
                }
                else
                {
                    foreach (var prop in joins.Properties().ToList())
                    {
                        if (!ColumnIds.Contains(prop.Name)) issues.Add($"{path}.joins.{prop.Name}: ไม่รู้จักคอลัมน์");
                        var v = (string?)prop.Value;
                        if (v == null || !ColumnJoinLayouts.Contains(v)) issues.Add($"{path}.joins.{prop.Name}: ต้องเป็น inline หรือ stack");
                    }
                }
                EnumField(p, "rowSet", new[] { "intake", "resolved" }, "intake", $"{path}.rowSet", issues);
                EnumField(p, "sortBy", CaseSortBy, "createdAt", $"{path}.sortBy", issues);
                EnumField(p, "sortDir", new[] { "asc", "desc" }, "asc", $"{path}.sortDir", issues);
                EnumField(p, "limitMode", new[] { "all", "fixed" }, "all", $"{path}.limitMode", issues);
                IntField(p, "limit", 1, CaseRowCeiling, 40, $"{path}.limit", issues);
                EnumField(p, "mode", CaseTableModes, "table", $"{path}.mode", issues);
                EnumField(p, "lineClamp", LineClamps, "two", $"{path}.lineClamp", issues);
                var ack = BoolField(p, "personalColumnsAcknowledged", false, $"{path}.personalColumnsAcknowledged", issues);
                PeriodOverrideField(p, $"{path}.periodOverride", issues);
                // normalizeColumnJoins บนคอลัมน์ที่พิมพ์จริง (printedCaseColumns) — ตัดการรวมที่ไม่มีความหมายทิ้ง
                if (p["joins"] is JObject j2 && issues.Count == 0)
                {
                    var printed = ack ? ordered : ordered.Where(id => CaseColumn(id).Side != "nis").ToList();
                    var normalized = new JObject();
                    for (var i = 0; i < printed.Count - 1; i++)
                    {
                        var v = (string?)j2[printed[i]];
                        if (v != null) normalized[printed[i]] = v;
                    }
                    p["joins"] = normalized;
                }
                break;

            case "signature":
                StrField(p, "leftLabel", 1, 60, "ผู้จัดทำ", $"{path}.leftLabel", issues);
                StrField(p, "rightLabel", 1, 60, "ผู้อนุมัติ", $"{path}.rightLabel", issues);
                BoolField(p, "showDate", true, $"{path}.showDate", issues);
                break;

            case "salesContact":
                StrField(p, "contactId", 0, 40, SalesContactAuto, $"{path}.contactId", issues);
                StrField(p, "kicker", 0, 60, "ผู้ประสานงานของ NIS", $"{path}.kicker", issues);
                StrField(p, "heading", 0, 80, "ติดต่อฝ่ายขาย", $"{path}.heading", issues);
                StrField(p, "note", 0, 400, SalesContactDefaultNote, $"{path}.note", issues);
                break;

            case "titlePage":
                StrField(p, "kicker", 0, 60, "", $"{path}.kicker", issues);
                StrField(p, "heading", 1, 80, null, $"{path}.heading", issues,
                    tooShort: "หน้าคั่นต้องมีหัวข้อ — ถ้าไม่ต้องการหน้านี้ให้ลบบล็อกแทน", tooLong: "หัวข้อยาวได้ไม่เกิน 80 ตัวอักษร");
                StrField(p, "note", 0, 2000, "", $"{path}.note", issues, tooLong: "ข้อความยาวได้ไม่เกิน 2,000 ตัวอักษร");
                break;
        }
    }

    // ── ตัวช่วยอ่าน/เติมค่า field (แทน zod primitives) ──────────────────

    private static JObject? ObjField(JObject o, string key, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { var n = new JObject(); o[key] = n; return n; }
        if (v is JObject jo) return jo;
        issues.Add($"{path}: ต้องเป็น object");
        return null;
    }

    private static string? StrField(JObject o, string key, int min, int max, string? dflt, string path, List<string> issues,
        string? tooShort = null, string? tooLong = null)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null)
        {
            if (dflt == null) { issues.Add($"{path}: ต้องระบุ"); return null; }
            o[key] = dflt;
            return dflt;
        }
        if (v.Type != JTokenType.String) { issues.Add($"{path}: ต้องเป็นข้อความ"); return null; }
        var s = (string)v!;
        if (s.Length < min) issues.Add($"{path}: {tooShort ?? (min == 1 ? "ต้องไม่ว่าง" : $"ต้องยาวอย่างน้อย {min} ตัวอักษร")}");
        if (s.Length > max) issues.Add($"{path}: {tooLong ?? $"ยาวเกิน {max} ตัวอักษร"}");
        return s;
    }

    private static string? NullableStrField(JObject o, string key, int max, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { o[key] = JValue.CreateNull(); return null; }
        if (v.Type != JTokenType.String) { issues.Add($"{path}: ต้องเป็นข้อความหรือ null"); return null; }
        var s = (string)v!;
        if (s.Length > max) issues.Add($"{path}: ยาวเกิน {max} ตัวอักษร");
        return s;
    }

    private static string? NullableDateField(JObject o, string key, string path, List<string> issues)
    {
        var s = NullableStrField(o, key, 10, path, issues);
        if (s != null && !IsThaiDate(s)) issues.Add($"{path}: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");
        return s;
    }

    private static int? NullableIntField(JObject o, string key, int min, int max, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { o[key] = JValue.CreateNull(); return null; }
        if (v.Type != JTokenType.Integer) { issues.Add($"{path}: ต้องเป็นจำนวนเต็มหรือ null"); return null; }
        var n = (int)v;
        if (n < min || n > max) issues.Add($"{path}: ต้องอยู่ระหว่าง {min}–{max}");
        return n;
    }

    private static string? EnumField(JObject o, string key, string[] allowed, string? dflt, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null)
        {
            if (dflt == null) { issues.Add($"{path}: ต้องระบุ"); return null; }
            o[key] = dflt;
            return dflt;
        }
        if (v.Type != JTokenType.String || !allowed.Contains((string)v!))
        {
            issues.Add($"{path}: ต้องเป็นค่าใดค่าหนึ่งใน {string.Join(" | ", allowed)}");
            return null;
        }
        return (string)v!;
    }

    private static void OptionalEnumField(JObject o, string key, string[] allowed, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null) return;
        if (v.Type == JTokenType.Null) { o.Remove(key); return; }
        if (v.Type != JTokenType.String || !allowed.Contains((string)v!))
            issues.Add($"{path}: ต้องเป็นค่าใดค่าหนึ่งใน {string.Join(" | ", allowed)}");
    }

    private static bool BoolField(JObject o, string key, bool dflt, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { o[key] = dflt; return dflt; }
        if (v.Type != JTokenType.Boolean) { issues.Add($"{path}: ต้องเป็น true หรือ false"); return dflt; }
        return (bool)v;
    }

    private static void OptionalBoolField(JObject o, string key, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null) return;
        if (v.Type == JTokenType.Null) { o.Remove(key); return; }
        if (v.Type != JTokenType.Boolean) issues.Add($"{path}: ต้องเป็น true หรือ false");
    }

    private static void NumField(JObject o, string key, double min, double max, double dflt, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { o[key] = dflt; return; }
        if (v.Type != JTokenType.Integer && v.Type != JTokenType.Float) { issues.Add($"{path}: ต้องเป็นตัวเลข"); return; }
        var n = (double)v;
        if (n < min || n > max) issues.Add($"{path}: ต้องอยู่ระหว่าง {min.ToString(CultureInfo.InvariantCulture)}–{max.ToString(CultureInfo.InvariantCulture)}");
    }

    private static void IntField(JObject o, string key, int min, int max, int dflt, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null || v.Type == JTokenType.Null) { o[key] = dflt; return; }
        if (v.Type != JTokenType.Integer) { issues.Add($"{path}: ต้องเป็นจำนวนเต็ม"); return; }
        var n = (int)v;
        if (n < min || n > max) issues.Add($"{path}: ต้องอยู่ระหว่าง {min}–{max}");
    }

    private static void OptionalIntField(JObject o, string key, int min, int max, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null) return;
        if (v.Type == JTokenType.Null) { o.Remove(key); return; }
        if (v.Type != JTokenType.Integer) { issues.Add($"{path}: ต้องเป็นจำนวนเต็ม"); return; }
        var n = (int)v;
        if (n < min || n > max) issues.Add($"{path}: ต้องอยู่ระหว่าง {min}–{max}");
    }

    private static void OptionalHexField(JObject o, string key, string path, List<string> issues)
    {
        var v = o[key];
        if (v is null) return;
        if (v.Type == JTokenType.Null) { o.Remove(key); return; }
        if (v.Type != JTokenType.String || !HexColorRegex.IsMatch((string)v!)) issues.Add($"{path}: ต้องเป็นสีรูปแบบ #rrggbb");
    }

    private static void ValidatePeriod(JObject p, string path, List<string> issues)
    {
        var from = StrField(p, "from", 1, 10, null, $"{path}.from", issues);
        var to = StrField(p, "to", 1, 10, null, $"{path}.to", issues);
        if (from != null && !IsThaiDate(from)) issues.Add($"{path}.from: ต้องเป็นรูปแบบ 'YYYY-MM-DD' (ค.ศ. — ที่แสดงผลค่อยแปลงเป็น พ.ศ.)");
        if (to != null && !IsThaiDate(to)) issues.Add($"{path}.to: ต้องเป็นรูปแบบ 'YYYY-MM-DD' (ค.ศ. — ที่แสดงผลค่อยแปลงเป็น พ.ศ.)");
        if (from != null && to != null && string.CompareOrdinal(from, to) > 0) issues.Add($"{path}.from: วันเริ่มอยู่หลังวันสิ้นสุด");
    }

    private static void PeriodOverrideField(JObject p, string path, List<string> issues)
    {
        var v = p["periodOverride"];
        if (v is null || v.Type == JTokenType.Null) { p["periodOverride"] = JValue.CreateNull(); return; }
        if (v is not JObject po) { issues.Add($"{path}: ต้องเป็น object หรือ null"); return; }
        ValidatePeriod(po, path, issues);
    }

    /// <summary>'YYYY-MM-DD' และเป็นวันที่จริงในปฏิทิน</summary>
    public static bool IsThaiDate(string s) =>
        ThaiDateRegex.IsMatch(s) && DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    // ═══════════════════════════════════════════════════════════════════════
    // 3. ช่วงเวลาของบล็อก · periodsOf · projectionOf
    // ═══════════════════════════════════════════════════════════════════════

    private static bool IsDataBlock(string type) => type is "kpiRow" or "chart" or "caseTable";

    /// <summary>ช่วงเวลาที่บล็อกหนึ่งใช้จริง (resolveBlockPeriod) — isOverride = ต้องขึ้นป้ายบนหน้ากระดาษ</summary>
    public static (string From, string To, bool IsOverride) ResolveBlockPeriod(string docFrom, string docTo, JObject block)
    {
        var type = (string?)block["type"] ?? "";
        var o = IsDataBlock(type) ? block["props"]?["periodOverride"] as JObject : null;
        if (o == null) return (docFrom, docTo, false);
        var from = (string?)o["from"] ?? docFrom;
        var to = (string?)o["to"] ?? docTo;
        return (from, to, from != docFrom || to != docTo);
    }

    /// <summary>ช่วงเวลาทุกช่วงที่เอกสารฉบับนี้ใช้จริง — ช่วงของเอกสารมาก่อนเสมอ (periodsOf)</summary>
    public static List<(string From, string To)> PeriodsOf(JObject layout)
    {
        var (docFrom, docTo) = DocPeriod(layout);
        var seen = new Dictionary<string, (string, string)>(StringComparer.Ordinal) { [PeriodKey(docFrom, docTo)] = (docFrom, docTo) };
        foreach (var b in Blocks(layout))
        {
            var r = ResolveBlockPeriod(docFrom, docTo, b);
            var k = PeriodKey(r.From, r.To);
            if (!seen.ContainsKey(k)) seen[k] = (r.From, r.To);
        }
        return seen.Values.ToList();
    }

    /// <summary>ช่วงของเอกสาร (template.period)</summary>
    public static (string From, string To) DocPeriod(JObject layout) =>
        ((string?)layout["period"]?["from"] ?? "", (string?)layout["period"]?["to"] ?? "");

    /// <summary>บล็อกทั้งหมดของ layout (เฉพาะที่เป็น object)</summary>
    public static IEnumerable<JObject> Blocks(JObject layout) =>
        (layout["blocks"] as JArray)?.OfType<JObject>() ?? Enumerable.Empty<JObject>();

    /// <summary>รหัสลูกค้าของเอกสาร (docMeta.customerId)</summary>
    public static string DocCustomerId(JObject layout) => (string?)layout["docMeta"]?["customerId"] ?? "";

    private sealed class CaseDraft
    {
        public string Key = "";
        public HashSet<string> Columns = new(StringComparer.Ordinal);
        public int RowLimit;
        public string DateField = "createdAt";
        public string Sort = "createdAt";
        public string Order = "asc";
    }

    private sealed class Draft
    {
        public string From = "";
        public string To = "";
        public HashSet<string> Kpis = new(StringComparer.Ordinal);
        public List<string> Datasets = new();
        public string? DurationBasis;
        public Dictionary<string, CaseDraft> Cases = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// สิ่งที่ layout ฉบับนี้จะดึงและจะเก็บลง snapshot (projectionOf) — คำนวณจาก layout **ก่อน** ไปดึงข้อมูล
    /// คอลัมน์ที่มีชื่อคนของ NIS ไม่ออกจากเซิร์ฟเวอร์เลยจนกว่าจะกดยอมรับ (SPEC §9 ข้อ 68)
    /// </summary>
    public static NisProjection ProjectionOf(JObject layout)
    {
        var (docFrom, docTo) = DocPeriod(layout);
        var drafts = new Dictionary<string, Draft>(StringComparer.Ordinal);
        var hasOverride = false;

        Draft DraftFor(string from, string to)
        {
            var k = PeriodKey(from, to);
            if (!drafts.TryGetValue(k, out var d))
            {
                d = new Draft { From = from, To = to };
                drafts[k] = d;
            }
            return d;
        }

        // ช่วงของเอกสารต้องอยู่ลำดับแรกเสมอ แม้ไม่มีบล็อกไหนใช้
        DraftFor(docFrom, docTo);

        foreach (var b in Blocks(layout))
        {
            var r = ResolveBlockPeriod(docFrom, docTo, b);
            if (r.IsOverride) hasOverride = true;
            var d = DraftFor(r.From, r.To);
            var type = (string?)b["type"] ?? "";
            var p = b["props"] as JObject ?? new JObject();

            if (type == "kpiRow")
            {
                foreach (var m in (p["metrics"] as JArray)?.Select(x => (string?)x).Where(x => x != null) ?? Enumerable.Empty<string?>())
                    d.Kpis.Add(m!);
            }
            else if (type == "chart")
            {
                var ds = (string?)p["dataset"] ?? "";
                if (ds.Length > 0 && !d.Datasets.Contains(ds)) d.Datasets.Add(ds);
                if (ds == "breakdown.duration")
                {
                    var basis = (string?)p["basis"] ?? "clock";
                    // สองกราฟที่ขอคนละแกนในช่วงเดียวกัน — ต้องเก็บทั้งคู่ ไม่ใช่ให้ตัวหลังทับตัวแรก
                    d.DurationBasis = d.DurationBasis != null && d.DurationBasis != basis ? "business" : basis;
                }
            }
            else if (type == "caseTable")
            {
                var cols = (p["columns"] as JArray)?.Select(x => (string?)x ?? "").Where(x => ColumnIds.Contains(x)).ToList() ?? new List<string>();
                var ack = (bool?)p["personalColumnsAcknowledged"] ?? false;
                var allowed = ack ? cols : cols.Where(c => CaseColumn(c).Side != "nis").ToList();
                var rowSet = (string?)p["rowSet"] ?? "intake";
                var sortBy = (string?)p["sortBy"] ?? "createdAt";
                var sortDir = (string?)p["sortDir"] ?? "asc";
                var ck = CaseRequestKey(rowSet, sortBy, sortDir);
                if (!d.Cases.TryGetValue(ck, out var cd))
                {
                    cd = new CaseDraft
                    {
                        Key = ck,
                        DateField = rowSet == "resolved" ? "resolvedAt" : "createdAt",
                        Sort = sortBy,
                        Order = sortDir,
                    };
                    d.Cases[ck] = cd;
                }
                foreach (var c in allowed) cd.Columns.Add(c);
                var limitMode = (string?)p["limitMode"] ?? "all";
                var limit = (int?)p["limit"] ?? 40;
                cd.RowLimit = Math.Max(cd.RowLimit, limitMode == "all" ? CaseRowCeiling : limit);
            }
        }

        var needs = drafts.Values.Select(d =>
        {
            var reqs = d.Cases.Values.Select(c =>
            {
                var columns = OrderCaseColumns(c.Columns);
                var fields = new HashSet<string>(StringComparer.Ordinal);
                foreach (var id in columns) foreach (var f in CaseColumn(id).Reads) fields.Add(f);
                return new NisCaseRequest
                {
                    Key = c.Key, Columns = columns, Fields = fields, RowLimit = c.RowLimit,
                    DateField = c.DateField, Sort = c.Sort, Order = c.Order,
                };
            }).ToList();
            return new NisPeriodNeed
            {
                Key = PeriodKey(d.From, d.To),
                From = d.From,
                To = d.To,
                KpiMetrics = d.Kpis.ToList(),
                Datasets = d.Datasets,
                DurationBasis = d.DurationBasis,
                CaseRequests = reqs,
            };
        }).ToList();

        return new NisProjection { DocFrom = docFrom, DocTo = docTo, Needs = needs, HasOverride = hasOverride };
    }

    /// <summary>รายงานมีบล็อกที่พิมพ์รายการเคสทั้งหมดไหม (hasAppendixBlock) — วันนี้ยังไม่มีชนิด caseAppendix จึง false เสมอ</summary>
    public static bool HasAppendixBlock(JObject layout) => Blocks(layout).Any(b => (string?)b["type"] == "caseAppendix");

    // ═══════════════════════════════════════════════════════════════════════
    // 4. ด่านวันหยุด (holiday-gate.ts)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>ทุกจุดในรายงานที่ตัวเลขมาจากการคำนวณเวลาทำการ (businessHourUses)</summary>
    public static List<NisBusinessHourUseDto> BusinessHourUses(JObject layout)
    {
        var out_ = new List<NisBusinessHourUseDto>();
        foreach (var b in Blocks(layout))
        {
            var type = (string?)b["type"];
            var id = (string?)b["id"] ?? "";
            var p = b["props"] as JObject ?? new JObject();
            if (type == "kpiRow")
            {
                foreach (var m in (p["metrics"] as JArray)?.Select(x => (string?)x) ?? Enumerable.Empty<string?>())
                {
                    if (m != null && KpiById.TryGetValue(m, out var def) && def.NeedsHoliday)
                        out_.Add(new NisBusinessHourUseDto { Kind = "kpi", BlockId = id, Label = def.Label });
                }
            }
            else if (type == "chart" && (string?)p["dataset"] == "breakdown.duration" && (string?)p["basis"] == "business")
            {
                out_.Add(new NisBusinessHourUseDto { Kind = "chart", BlockId = id, Label = $"{(string?)p["title"]} (แกนชั่วโมงทำการ)" });
            }
        }
        return out_;
    }

    /// <summary>ปีทั้งหมดที่ช่วงเวลาชุดหนึ่งครอบคลุม (yearsCovered)</summary>
    public static List<int> YearsCovered(IEnumerable<(string From, string To)> periods)
    {
        var set = new SortedSet<int>();
        foreach (var (from, to) in periods)
        {
            if (from.Length < 4 || to.Length < 4) continue;
            if (!int.TryParse(from[..4], out var a) || !int.TryParse(to[..4], out var b)) continue;
            for (var y = Math.Min(a, b); y <= Math.Max(a, b); y++) set.Add(y);
        }
        return set.ToList();
    }

    /// <summary>
    /// ด่านวันหยุด (holidayGate) — เทียบรายปี และต้อง "ครบทุกวัน" ไม่ใช่ "อย่างน้อยหนึ่งวัน" (SPEC §9 ข้อ 67)
    /// </summary>
    public static NisHolidayGateDto HolidayGate(JObject layout, IEnumerable<(string From, string To)> periods, IReadOnlyCollection<int> observedYears)
    {
        var uses = BusinessHourUses(layout);
        var years = YearsCovered(periods);
        var have = new HashSet<int>(observedYears);
        var missing = years.Where(y => !have.Contains(y)).ToList();
        if (uses.Count == 0 || missing.Count == 0)
            return new NisHolidayGateDto { Blocked = false, Uses = uses, Years = years, MissingYears = missing, Message = null };

        var list = string.Join("\n", uses.Select(u => $"  · {u.Label} (บล็อก {u.BlockId})"));
        var be = string.Join(" · ", missing.Select(y => (y + 543).ToString(CultureInfo.InvariantCulture)));
        return new NisHolidayGateDto
        {
            Blocked = true, Uses = uses, Years = years, MissingYears = missing,
            Message =
                "ออกรายงานไม่ได้ — มีตัวเลขที่คำนวณจากเวลาทำการอยู่ในเอกสาร " +
                $"แต่ยังไม่ได้ยืนยันวันหยุดครบทุกวันของปี {be} (ต้องครบทุกวัน ยืนยันแค่บางวันยังไม่พอ)\n" +
                $"{list}\n" +
                "ตัวเลขเหล่านี้จะผิดย้อนหลังทันทีที่ HR ยืนยันวันหยุดเพิ่ม และเอกสารที่ส่งลูกค้าไปแล้วเรียกคืนไม่ได้\n" +
                "ทางออกสองทาง — เอาบล็อกเหล่านี้ออกจากรายงาน หรือรอจนกว่า HR จะตอบตาราง " +
                "var/holiday-worksheet-2026.csv แล้วอัปเดต observed ให้ครบทุกวัน (ดูข้อบังคับข้ามเฟสใน QUESTIONS.md)",
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 5. ด่านของ snapshot (snapshot.ts) — truncation · freshness · printability · Q25
    // ═══════════════════════════════════════════════════════════════════════

    private static IEnumerable<KeyValuePair<string, JObject>> SnapshotPeriods(JObject snapshot) =>
        (snapshot["periods"] as JObject)?.Properties()
            .Where(p => p.Value is JObject)
            .Select(p => new KeyValuePair<string, JObject>(p.Name, (JObject)p.Value))
        ?? Enumerable.Empty<KeyValuePair<string, JObject>>();

    /// <summary>ด่านภาคผนวก (truncationGate) — รายการล้นและไม่มีบล็อกภาคผนวก = ออกรายงานไม่ได้ (SPEC §9 ข้อ 17)</summary>
    public static NisTruncationGateDto TruncationGate(JObject snapshot, bool hasAppendix)
    {
        var details = new List<NisTruncationDetailDto>();
        foreach (var (key, p) in SnapshotPeriods(snapshot))
        {
            // ดูทุกถัง — `cases` คือสำเนาของ casesByKey[คำขอแรก] · มี casesByKey เมื่อไหร่ให้ดูแต่ในนั้น
            var byKey = (p["casesByKey"] as JObject)?.Properties().Select(x => x.Value as JObject).Where(x => x != null).ToList() ?? new List<JObject?>();
            var buckets = byKey.Count > 0 ? byKey : new List<JObject?> { p["cases"] as JObject };
            foreach (var c in buckets)
            {
                if (c == null || !((bool?)c["truncated"] ?? false)) continue;
                details.Add(new NisTruncationDetailDto
                {
                    PeriodKey = key,
                    Shown = (c["rows"] as JArray)?.Count ?? 0,
                    Total = (int?)c["total"] ?? 0,
                    Omitted = (int?)c["omittedCount"] ?? 0,
                });
            }
        }
        var omittedTotal = details.Sum(d => d.Omitted);
        if (details.Count == 0 || hasAppendix)
            return new NisTruncationGateDto { Blocked = false, OmittedTotal = omittedTotal, Details = details, Message = null };

        var lines = string.Join("\n", details.Select(d => $"  · ช่วง {d.PeriodKey} — พิมพ์ {d.Shown} จาก {d.Total} เคส · ตกหล่น {d.Omitted}"));
        var needed = details.Max(d => d.Total);
        return new NisTruncationGateDto
        {
            Blocked = true, OmittedTotal = omittedTotal, Details = details,
            Message =
                $"ออกรายงานไม่ได้ — มี {omittedTotal} เคสที่ไม่ได้พิมพ์ลงเอกสาร และยังไม่มีบล็อกภาคผนวกรองรับ\n" +
                $"{lines}\n" +
                "ข้อ 5.5 บังคับว่าห้ามตัดข้อมูลเงียบ ๆ · เอกสารที่รายการไม่ครบคือเอกสารที่ห้ามส่งลูกค้า\n" +
                $"ทางออก — ตั้ง *จำนวนแถวที่พิมพ์* เป็น {needed} (หรือสลับกลับเป็นโหมด “ทั้งหมดในช่วง”) " +
                "แล้วรายการจะครบทันที · หรือรอบล็อกภาคผนวกใน v1.1",
        };
    }

    /// <summary>ISO UTC ของจุดเวลา (ตรงกับ Date.toISOString)</summary>
    private static string IsoUtc(DateTimeOffset at) => NisServiceCaseDto.Iso(at);

    /// <summary>dd/MM/พ.ศ. HH:mm น. เวลาไทย — คำนวณเองไม่พึ่ง locale</summary>
    private static string FmtThai(DateTimeOffset at)
    {
        var d = NisBangkokTime.ToBangkok(at);
        return $"{d.Day:00}/{d.Month:00}/{d.Year + 543} {d.Hour:00}:{d.Minute:00} น.";
    }

    /// <summary>ด่านความสด (freshnessGate) — ช่วงของรายงานต้องไม่ล้ำเกินเวลาที่ข้อมูลมีจริง (SPEC §9 ข้อ 41)</summary>
    public static NisFreshnessGateDto FreshnessGate(JObject snapshot)
    {
        var dataAsOfStr = (string?)snapshot["context"]?["dataAsOf"];
        DateTimeOffset? dataAsOf = dataAsOfStr != null && DateTimeOffset.TryParse(dataAsOfStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed : null;
        var beyond = new List<NisFreshnessBeyondDto>();
        foreach (var (key, p) in SnapshotPeriods(snapshot))
        {
            var to = (string?)p["to"] ?? "";
            if (!IsThaiDate(to)) continue;
            var endOfDay = NisBangkokTime.EndOfDay(to);
            if (dataAsOf == null || dataAsOf.Value < endOfDay)
                beyond.Add(new NisFreshnessBeyondDto { PeriodKey = key, NeedUntil = IsoUtc(endOfDay) });
        }
        if (beyond.Count == 0)
            return new NisFreshnessGateDto { Blocked = false, Message = null, DataAsOf = dataAsOfStr, Beyond = beyond };

        var lines = string.Join("\n", beyond.Select(b => $"  · ช่วง {b.PeriodKey} — ต้องมีข้อมูลถึง {FmtThai(DateTimeOffset.Parse(b.NeedUntil, CultureInfo.InvariantCulture))}"));
        return new NisFreshnessGateDto
        {
            Blocked = true, DataAsOf = dataAsOfStr, Beyond = beyond,
            Message =
                "ออกรายงานไม่ได้ — ช่วงของเอกสารล้ำเกินข้อมูลที่ระบบมีจริง\n" +
                $"{lines}\n" +
                $"แต่ข้อมูลล่าสุดที่ sync สำเร็จคือ {(dataAsOf == null ? "(ยังไม่เคย sync สำเร็จเลย)" : FmtThai(dataAsOf.Value))}\n" +
                "ตัวเลขในรายงานจะขาดช่วงที่ยังไม่ได้ดึงมา โดยที่ผู้อ่านไม่มีทางรู้ — " +
                "นั่นคือคำกล่าวอ้างที่เท็จบนกระดาษที่ส่งลูกค้า\n" +
                "ทางออก — รอให้ sync สำเร็จจนครอบคลุมช่วงนี้ แล้วสั่งออกใหม่ (ปุ่ม “ดึงข้อมูลเดี๋ยวนี้” ที่หน้าภาพรวม)",
        };
    }

    /// <summary>ด่าน "พิมพ์ได้ไหม" (printabilityGate) — snapshot ที่ไม่รู้ว่านับชุดไหนออกเป็นเอกสารไม่ได้ (SPEC 9.1 ข้อ 1)</summary>
    public static NisPrintabilityGateDto PrintabilityGate(JObject snapshot)
    {
        var periods = SnapshotPeriods(snapshot)
            .Where(kv => kv.Value["cases"] is JObject c && (c["countLabel"] == null || c["countLabel"]!.Type == JTokenType.Null || c["dateField"] == null || c["dateField"]!.Type == JTokenType.Null))
            .Select(kv => kv.Key)
            .ToList();
        var version = (int?)snapshot["snapshotVersion"] ?? 1;
        return new NisPrintabilityGateDto
        {
            Blocked = periods.Count > 0,
            Periods = periods,
            Message = periods.Count > 0
                ? $"รายงานฉบับนี้เก็บไว้ตั้งแต่ snapshot รุ่น {version} ซึ่งไม่ได้บันทึกว่าจำนวนเคส" +
                  $"ในรอบ {string.Join(" · ", periods)} เป็น \"งานที่แจ้งเข้า\" หรือ \"งานที่ปิดได้\" — " +
                  "ออกเป็นเอกสารไม่ได้เพราะข้อ 9.1 บังคับให้ทุกจำนวนเคสบอกชุดของตัวเอง " +
                  "ให้สร้างรายงานรอบนี้ใหม่ (ฉบับเดิมยังเปิดดูได้ ไม่ได้ถูกลบ)"
                : string.Empty,
        };
    }

    private static readonly string[] DurationFieldsClock = { "clock", "label" };
    private static readonly string[] DurationFieldsBusiness = { "business", "businessLabel" };

    /// <summary>
    /// กฎ Q25 (checkSnapshotWithinProjection) — snapshot เก็บเฉพาะสิ่งที่รายงานฉบับนั้นแสดงจริง ไม่ขาดไม่เกิน
    /// ตรวจทุกคำขอตารางเคส ไม่ใช่แค่ก้อนแรก · ธงล้นต้องตรงกับตัวเลขจริง
    /// </summary>
    public static List<NisSnapshotViolation> CheckSnapshotWithinProjection(NisProjection projection, JObject snapshot)
    {
        var out_ = new List<NisSnapshotViolation>();
        var periods = SnapshotPeriods(snapshot).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
        var wantKeys = new HashSet<string>(projection.Needs.Select(n => n.Key), StringComparer.Ordinal);
        foreach (var key in periods.Keys)
        {
            if (!wantKeys.Contains(key))
                out_.Add(new($"periods.{key}", $"เก็บข้อมูลของช่วง \"{key}\" ไว้ทั้งที่ไม่มีบล็อกไหนใช้"));
        }

        var snapshotVersion = (int?)snapshot["snapshotVersion"] ?? 1;

        foreach (var need in projection.Needs)
        {
            if (!periods.TryGetValue(need.Key, out var p))
            {
                if (need.UsesData)
                    out_.Add(new($"periods.{need.Key}", $"รายงานมีบล็อกที่ใช้ช่วง \"{need.Key}\" แต่ snapshot ไม่มีข้อมูล"));
                continue;
            }
            string At(string w) => $"periods.{need.Key}.{w}";

            // KPI — ต้องตรงชุดกับที่บล็อกเลือกไว้ ไม่ขาดไม่เกิน
            var wantKpis = new HashSet<string>(need.KpiMetrics, StringComparer.Ordinal);
            var haveKpis = new HashSet<string>((p["kpis"] as JObject)?.Properties().Select(x => x.Name) ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            foreach (var k in haveKpis.Where(k => !wantKpis.Contains(k)))
                out_.Add(new(At($"kpis.{k}"), $"เก็บ KPI \"{k}\" ไว้ทั้งที่ไม่มีการ์ดไหนแสดงมัน"));
            foreach (var k in wantKpis.Where(k => !haveKpis.Contains(k)))
                out_.Add(new(At($"kpis.{k}"), $"รายงานมีการ์ด \"{k}\" แต่ snapshot ไม่มีค่า — เปิดย้อนหลังจะได้การ์ดว่าง"));

            // dataset
            var datasets = p["datasets"] as JObject ?? new JObject();
            var allowed = new HashSet<string>(need.Datasets, StringComparer.Ordinal);
            foreach (var key in datasets.Properties().Select(x => x.Name).Where(k => !allowed.Contains(k)))
                out_.Add(new(At($"datasets.{key}"), $"เก็บแหล่งข้อมูล \"{key}\" ไว้ทั้งที่ไม่มีกราฟไหนผูกกับมัน"));
            foreach (var key in allowed.Where(k => datasets[k] == null))
                out_.Add(new(At($"datasets.{key}"), $"รายงานมีกราฟที่ผูกกับ \"{key}\" แต่ snapshot ไม่มีข้อมูล — เปิดย้อนหลังจะได้กราฟเปล่า"));
            if (datasets["breakdown.duration"] is JObject duration && need.DurationBasis != null)
            {
                var forbidden = need.DurationBasis == "clock" ? DurationFieldsBusiness : Array.Empty<string>();
                if (duration["rows"] is JArray rows)
                {
                    foreach (var f in forbidden)
                    {
                        if (rows.OfType<JObject>().Any(r => r[f] != null))
                            out_.Add(new(At($"datasets.breakdown.duration.rows[].{f}"),
                                $"กราฟใช้แกน \"{need.DurationBasis}\" แต่ snapshot เก็บ \"{f}\" มาด้วย — ตัวเลขเวลาทำการที่ไม่ได้พิมพ์ห้ามถูกแช่แข็งไว้ (ข้อบังคับข้ามเฟส · A4)"));
                    }
                }
            }

            // ตาราง — ทางที่ PII หลุดจริง · ตรวจทุกคำขอ
            var casesByKey = p["casesByKey"] as JObject;
            for (var i = 0; i < need.CaseRequests.Count; i++)
            {
                var req = need.CaseRequests[i];
                var inByKey = casesByKey?[req.Key] as JObject;
                var bundle = inByKey ?? (i == 0 ? p["cases"] as JObject : null);
                var slot = inByKey != null ? $"casesByKey.{req.Key}" : "cases";
                if (bundle == null)
                {
                    if (req.Columns.Count > 0)
                        out_.Add(new(At(i == 0 ? "cases" : $"casesByKey.{req.Key}"), "รายงานมีบล็อกตารางแต่ snapshot ไม่มีข้อมูลตาราง — เปิดดูย้อนหลังจะได้ตารางเปล่า"));
                    continue;
                }
                var allowedColumns = new HashSet<string>(req.Columns, StringComparer.Ordinal);
                foreach (var c in (bundle["columns"] as JArray)?.Select(x => (string?)x ?? "") ?? Enumerable.Empty<string>())
                {
                    if (!allowedColumns.Contains(c)) out_.Add(new(At($"{slot}.columns"), $"คอลัมน์ \"{c}\" ไม่ได้อยู่ในตารางของรายงานฉบับนี้"));
                }
                var offending = new SortedSet<string>(StringComparer.Ordinal);
                var rowsArr = bundle["rows"] as JArray ?? new JArray();
                foreach (var row in rowsArr.OfType<JObject>())
                    foreach (var key in row.Properties().Select(x => x.Name))
                        if (!req.Fields.Contains(key)) offending.Add(key);
                foreach (var key in offending)
                    out_.Add(new(At($"{slot}.rows[].{key}"),
                        $"แถวในตารางมี field \"{key}\" ซึ่งไม่มีคอลัมน์ไหนในรายงานอ่าน — " +
                        "Q25 บังคับว่า snapshot เก็บเฉพาะสิ่งที่พิมพ์ลงกระดาษจริง และข้อมูลที่เข้าไปแล้วลบไม่ได้ (A4)"));
                var dateField = bundle["dateField"];
                if (dateField != null && dateField.Type != JTokenType.Null && (string?)dateField != req.DateField)
                    out_.Add(new(At($"{slot}.dateField"), $"snapshot บอกว่านับด้วย \"{(string?)dateField}\" แต่ตารางเรียงแบบที่ต้องนับด้วย \"{req.DateField}\""));
                var countLabel = (string?)bundle["countLabel"];
                if (string.IsNullOrEmpty(countLabel) && snapshotVersion >= SnapshotVersion)
                    out_.Add(new(At($"{slot}.countLabel"), "ไม่มีป้ายว่าจำนวนที่เก็บเป็นชุดไหน — ประโยคใต้ตารางจะเป็นตัวเลขลอย (SPEC 9.1 ข้อ 1)"));
                var total = (int?)bundle["total"] ?? 0;
                var omitted = (int?)bundle["omittedCount"] ?? 0;
                var truncated = (bool?)bundle["truncated"] ?? false;
                var realOmitted = Math.Max(0, total - rowsArr.Count);
                if (omitted != realOmitted)
                    out_.Add(new(At($"{slot}.omittedCount"), $"บอกว่าตกหล่น {omitted} แต่ {total} − {rowsArr.Count} = {realOmitted}"));
                if (truncated != realOmitted > 0)
                    out_.Add(new(At($"{slot}.truncated"), $"ธง truncated={truncated.ToString().ToLowerInvariant()} ไม่ตรงกับความจริง (ตกหล่น {realOmitted} เคส) — ด่านภาคผนวกจะตัดสินผิด"));
            }
            var askedKeys = new HashSet<string>(need.CaseRequests.Select(r => r.Key), StringComparer.Ordinal);
            foreach (var k in casesByKey?.Properties().Select(x => x.Name) ?? Enumerable.Empty<string>())
                if (!askedKeys.Contains(k)) out_.Add(new(At($"casesByKey.{k}"), $"เก็บตารางเคสชุด \"{k}\" ไว้ทั้งที่ไม่มีบล็อกไหนขอ"));
        }
        return out_;
    }

    /// <summary>เวอร์ชันที่โยน (assertSnapshotWithinProjection) — ใช้ที่ชั้นเขียนก่อนบันทึกลงฐาน</summary>
    public static void AssertSnapshotWithinProjection(NisProjection projection, JObject snapshot)
    {
        var bad = CheckSnapshotWithinProjection(projection, snapshot);
        if (bad.Count == 0) return;
        throw new InvalidOperationException(
            $"dataSnapshot ไม่ผ่านกฎ Q25 ({bad.Count} จุด):\n" + string.Join("\n", bad.Select(v => $"  · {v.Where} — {v.Message}")));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 6. ตัวช่วยฝั่ง "ทำให้ถูกได้ง่าย" — pickCaseRow · pickKpis · pickDurationDataset
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>ตัดแถวข้อมูลให้เหลือเฉพาะ field ที่คอลัมน์อ่าน (pickCaseRow) — ใช้แทนการส่ง DTO ทั้งก้อนเสมอ</summary>
    public static JObject PickCaseRow(IReadOnlySet<string> fields, JObject row)
    {
        var out_ = new JObject();
        foreach (var f in fields)
            if (row.TryGetValue(f, out var v)) out_[f] = v.DeepClone();
        return out_;
    }

    private static double? ReadPath(JToken? obj, string path)
    {
        JToken? cur = obj;
        foreach (var part in path.Split('.'))
        {
            if (cur is not JObject o) return null;
            cur = o[part];
        }
        return cur is { Type: JTokenType.Integer or JTokenType.Float } ? (double)cur : null;
    }

    /// <summary>หน่วยของตัวเลขดิบตามชนิด KPI (baseUnit)</summary>
    public static string KpiBaseUnit(string kind) => kind switch
    {
        "count" => "เคส",
        "score" => "คะแนน",
        "businessHours" => "ชม.ทำการ",
        _ => "ชม.",
    };

    /// <summary>
    /// ดึงเฉพาะ KPI ที่บล็อกเลือกไว้ออกจาก SummaryDto (pickKpis) — รับ current/previous/mom.kpis เป็น JSON หลวม ๆ
    /// (JObject.FromObject ของ DTO ฝั่ง analytics ด้วย camelCase) เพื่อไม่ผูกกับชนิด C# ของอีกชั้น
    /// </summary>
    public static JObject PickKpis(NisPeriodNeed need, JToken? current, JToken? previous, JObject? momKpis)
    {
        var out_ = new JObject();
        foreach (var id in need.KpiMetrics)
        {
            var m = KpiMetric(id);
            var entry = momKpis?[m.MomKey] as JObject;
            out_[id] = new JObject
            {
                ["value"] = ReadPath(current, m.Path),
                ["previous"] = ReadPath(previous, m.Path),
                ["diff"] = entry?["diff"] is { Type: JTokenType.Integer or JTokenType.Float } d ? (double)d : null,
                ["pct"] = entry?["pct"] is { Type: JTokenType.Integer or JTokenType.Float } pc ? (double)pc : null,
                ["baseUnit"] = KpiBaseUnit(m.Kind),
            };
        }
        return out_;
    }

    /// <summary>KPI ที่ค่าทุกช่องเป็น null (ธงบันทึกย้อนหลัง) — คง baseUnit ไว้</summary>
    public static JObject NullKpi(string id) => new()
    {
        ["value"] = null, ["previous"] = null, ["diff"] = null, ["pct"] = null, ["baseUnit"] = KpiBaseUnit(KpiMetric(id).Kind),
    };

    /// <summary>ตัด DurationRow ให้เหลือแกนเดียวตามที่กราฟพิมพ์จริง (pickDurationDataset)</summary>
    public static JToken PickDurationDataset(NisPeriodNeed need, JToken dto)
    {
        if (need.DurationBasis == null || dto is not JObject src) return dto;
        var keep = new HashSet<string>(StringComparer.Ordinal) { "key" };
        foreach (var f in need.DurationBasis == "business" ? DurationFieldsBusiness : DurationFieldsClock) keep.Add(f);
        var copy = (JObject)src.DeepClone();
        if (copy["rows"] is JArray rows)
        {
            var picked = new JArray();
            foreach (var r in rows)
            {
                if (r is not JObject ro) { picked.Add(r.DeepClone()); continue; }
                var o = new JObject();
                foreach (var prop in ro.Properties()) if (keep.Contains(prop.Name)) o[prop.Name] = prop.Value.DeepClone();
                picked.Add(o);
            }
            copy["rows"] = picked;
        }
        return copy;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 7. ข้อมูลบริษัท + หน้าติดต่อฝ่ายขาย (company.ts)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>ช่องที่ยังไม่กรอกและติดป้ายว่าต้องกรอกก่อนขึ้น production (companyGapsBeforeProduction)</summary>
    public static List<string> CompanyGapsBeforeProduction(NisCompanyInfoDto c)
    {
        var gaps = new List<string>();
        if (c.NameTh.Trim().Length == 0) gaps.Add("nameTh");
        if (c.AddressTh.Trim().Length == 0) gaps.Add("addressTh");
        if (c.Tel.Trim().Length == 0) gaps.Add("tel");
        if (c.Email.Trim().Length == 0) gaps.Add("email");
        if (c.Website.Trim().Length == 0) gaps.Add("website");
        return gaps;
    }

    /// <summary>ลูกค้าที่ถูกผูกกับเซลส์มากกว่าหนึ่งคน (customerOwnerConflicts) — ตั้งค่าที่บันทึกต้องไม่มี</summary>
    public static List<(string CustomerId, List<string> Names)> CustomerOwnerConflicts(IEnumerable<NisSalesContactDto> contacts)
    {
        var owners = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var c in contacts)
            foreach (var id in c.CustomerIds)
            {
                if (!owners.TryGetValue(id, out var list)) owners[id] = list = new List<string>();
                list.Add(c.Name.Trim().Length > 0 ? c.Name.Trim() : "(ยังไม่มีชื่อ)");
            }
        return owners.Where(kv => kv.Value.Count > 1).Select(kv => (kv.Key, kv.Value)).ToList();
    }

    /// <summary>ผู้ติดต่อที่หน้า "ติดต่อฝ่ายขาย" จะพิมพ์ (resolveSalesContact) — auto = คนแรกที่ผูกลูกค้ารายนี้ · ไม่พบ/ไม่มีชื่อ = null</summary>
    public static NisSalesContactDto? ResolveSalesContact(NisCompanyInfoDto company, string contactId, string customerId)
    {
        var c = contactId == SalesContactAuto
            ? company.SalesContacts.FirstOrDefault(x => x.CustomerIds.Contains(customerId))
            : company.SalesContacts.FirstOrDefault(x => x.Id == contactId);
        return c != null && c.Name.Trim().Length > 0 ? c : null;
    }

    /// <summary>ผู้ติดต่อที่จะพิมพ์ไม่ได้ผูกกับลูกค้าของเอกสาร แต่ข้อความยังอ้างว่าเป็น "ผู้ดูแลลูกค้ารายนี้" (salesNoteClaimUnbound)</summary>
    public static bool SalesNoteClaimUnbound(NisCompanyInfoDto company, string contactId, string customerId, string note)
    {
        var c = ResolveSalesContact(company, contactId, customerId);
        return c != null && !c.CustomerIds.Contains(customerId) && note.Contains(SalesNoteClaim, StringComparison.Ordinal);
    }

    /// <summary>ด่าน export ของหน้าติดต่อฝ่ายขาย (salesContactGate) — คำกล่าวอ้างที่ไม่จริงบนกระดาษ → ปฏิเสธ</summary>
    public static NisSalesContactGateDto SalesContactGate(JObject layout, NisCompanyInfoDto company)
    {
        var customerId = DocCustomerId(layout);
        foreach (var b in Blocks(layout))
        {
            if ((string?)b["type"] != "salesContact") continue;
            var p = b["props"] as JObject ?? new JObject();
            var contactId = (string?)p["contactId"] ?? "";
            var note = (string?)p["note"] ?? "";
            if (!SalesNoteClaimUnbound(company, contactId, customerId, note)) continue;
            var name = ResolveSalesContact(company, contactId, customerId)?.Name ?? "";
            return new NisSalesContactGateDto
            {
                Blocked = true,
                ContactName = name,
                Message =
                    $"หน้าติดต่อฝ่ายขายระบุ \"{name}\" ซึ่งไม่ได้ผูกกับลูกค้ารายนี้ แต่ข้อความเขียนว่า \"{SalesNoteClaim}\" — " +
                    "ออกรายงานไม่ได้จนกว่าจะแก้: เลือก \"อัตโนมัติ\" · ติ๊กลูกค้ารายนี้ให้คนนี้ใน ตั้งค่า → ข้อมูลบริษัท · หรือแก้ข้อความ",
            };
        }
        return new NisSalesContactGateDto { Blocked = false };
    }

    /// <summary>
    /// ข้อมูลบริษัทที่แช่แข็งลงรายงานตอนสร้าง (freezeCompanyForReport) — ช่องบริษัททั้งหมด + เฉพาะเซลส์ที่บล็อกเลือกจริง
    /// รายการลูกค้าของเซลส์ตัดเหลือแค่ลูกค้าของเอกสาร (พอให้ "อัตโนมัติ" เลือกคนเดิมได้)
    /// </summary>
    public static NisCompanyInfoDto FreezeCompanyForReport(JObject layout, NisCompanyInfoDto company)
    {
        var cid = DocCustomerId(layout);
        var used = new Dictionary<string, NisSalesContactDto>(StringComparer.Ordinal);
        foreach (var b in Blocks(layout))
        {
            if ((string?)b["type"] != "salesContact") continue;
            var contactId = (string?)b["props"]?["contactId"] ?? "";
            var c = ResolveSalesContact(company, contactId, cid);
            if (c != null)
            {
                used[c.Id] = new NisSalesContactDto
                {
                    Id = c.Id, Name = c.Name, Title = c.Title, Tel = c.Tel, Email = c.Email, LineQrPath = c.LineQrPath,
                    CustomerIds = c.CustomerIds.Where(x => x == cid).ToList(),
                };
            }
        }
        return new NisCompanyInfoDto
        {
            NameTh = company.NameTh, NameEn = company.NameEn, AddressTh = company.AddressTh, AddressEn = company.AddressEn,
            Tel = company.Tel, Email = company.Email, Website = company.Website, TaxId = company.TaxId,
            LineId = company.LineId, LineQrPath = company.LineQrPath,
            SalesContacts = used.Values.ToList(),
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 8. คำเตือน (doc-meta.ts) — เตือน ไม่บล็อก
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>เตือนเมื่อชื่อลูกค้าที่พิมพ์บนปกไม่ตรงกับชื่อจริงของ customerId (customerNameMismatch) · null = ไม่มีอะไรผิดปกติ</summary>
    public static string? CustomerNameMismatch(JObject layout, string? actualName)
    {
        if (string.IsNullOrEmpty(actualName)) return null;
        var meta = layout["docMeta"] as JObject ?? new JObject();
        var nameTh = (string?)meta["customerNameTh"] ?? "";
        var customerId = (string?)meta["customerId"] ?? "";
        static string Norm(string s) => Regex.Replace(s, @"\s+", " ").Trim();
        if (Norm(nameTh) == Norm(actualName)) return null;
        return
            $"ชื่อบนหน้าปกคือ \"{nameTh}\" แต่ข้อมูลในรายงานดึงจากลูกค้ารหัส " +
            $"\"{customerId}\" ซึ่งชื่อจริงคือ \"{actualName}\" — " +
            "ตัวเลขทั้งฉบับเป็นของรหัสนั้น ไม่ใช่ของชื่อที่พิมพ์ · ตั้งใจหรือเลือกลูกค้าผิด";
    }

    /// <summary>เตือนเมื่อเลขฉบับข้ามลำดับ (issueSequenceWarning) · null = ปกติ</summary>
    public static string? IssueSequenceWarning(int? previousIssueNo, int? issueNo)
    {
        if (previousIssueNo == null || issueNo == null) return null;
        if (issueNo == previousIssueNo + 1) return null;
        if (issueNo <= previousIssueNo) return $"ฉบับที่ {issueNo} ซ้ำหรือย้อนหลังจากฉบับก่อน ({previousIssueNo})";
        var missing = issueNo.Value - previousIssueNo.Value - 1;
        return $"ข้ามจากฉบับที่ {previousIssueNo} มาเป็น {issueNo} — มี {missing} งวดที่ยังไม่ได้ส่ง";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 9. มาตรวัดความกว้างตาราง (columns.ts widthMeter) — ขึ้นเฉพาะหน้าออกแบบ
    // ═══════════════════════════════════════════════════════════════════════

    private const double PxPerUnitAt105 = 4.6;
    private const double CellPaddingX = 12;

    /// <summary>ความกว้างตารางเป็น px ที่ขนาดตัวอักษรหนึ่ง (tableWidthPx)</summary>
    public static double TableWidthPx(IEnumerable<string> ids, double fontSize)
    {
        var scale = fontSize / 10.5;
        return OrderCaseColumns(ids).Sum(id => CaseColumn(id).W * PxPerUnitAt105 * scale + CellPaddingX);
    }

    /// <summary>คำนวณมาตรวัดในข้อ 5.4 (widthMeter) — ย่อ 10.5px → 9px ให้เองก่อน แล้วค่อยรายงานว่าเกิน</summary>
    public static NisWidthMeterDto WidthMeter(IEnumerable<string> ids, string orientation)
    {
        var page = orientation == "landscape" ? 775 : 528;
        var list = ids.ToList();
        var atFull = TableWidthPx(list, 10.5);
        if (atFull <= page)
            return new NisWidthMeterDto { Ratio = atFull / page, FontSize = 10.5, Verdict = "fits", Message = "พอดีหน้ากระดาษที่ขนาดตัวอักษรปกติ" };
        var atSmall = TableWidthPx(list, 9);
        if (atSmall <= page)
            return new NisWidthMeterDto { Ratio = atSmall / page, FontSize = 9, Verdict = "shrink", Message = "ย่อตัวอักษรให้อัตโนมัติ — ยังอ่านสบาย ข้อความยาวตัดที่ 2 บรรทัด" };
        return new NisWidthMeterDto { Ratio = atSmall / page, FontSize = 9, Verdict = "overflow", Message = "⚠ เกินหน้ากระดาษแม้ย่อแล้ว — แก้ได้ 3 ทาง: เอาคอลัมน์ออก · 2 บรรทัด/การ์ด · แนวนอน" };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 10. โครงตั้งต้น (defaults.ts) — starterTemplate · standardBlocksFor · starterCaseLimit
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>จำนวนแถวตั้งต้นของตารางเคส — เผื่อ 20% ปัดขึ้นสิบ ไม่เกินเพดาน (starterCaseLimit)</summary>
    public static int StarterCaseLimit(int caseCount)
    {
        var withHeadroom = (int)Math.Ceiling(caseCount * 1.2 / 10) * 10;
        return Math.Min(CaseRowCeiling, Math.Max(10, withHeadroom));
    }

    private static JObject NewBlock(string type, string id) => type switch
    {
        "cover" => new JObject
        {
            ["id"] = id, ["type"] = "cover",
            ["props"] = new JObject { ["style"] = "network", ["theme"] = "nis", ["motif"] = true, ["confidential"] = true, ["font"] = "Sarabun", ["lines"] = new JArray() },
        },
        "heading" => new JObject
        {
            ["id"] = id, ["type"] = "heading",
            ["props"] = new JObject { ["text"] = "หัวข้อใหม่", ["level"] = 2, ["rule"] = true, ["kicker"] = "", ["showKicker"] = true },
        },
        "text" => new JObject { ["id"] = id, ["type"] = "text", ["props"] = new JObject { ["text"] = "", ["align"] = "left" } },
        "kpiRow" => new JObject
        {
            ["id"] = id, ["type"] = "kpiRow",
            ["props"] = new JObject
            {
                ["metrics"] = new JArray("resolvedCount", "intakeCount", "clockHoursMedian", "satisfactionAvg"),
                ["showComparison"] = true, ["periodOverride"] = JValue.CreateNull(),
            },
        },
        "chart" => Chart(id, "trend.day", DefaultChartTitles["trend.day"], "", 180),
        "signature" => new JObject
        {
            ["id"] = id, ["type"] = "signature",
            ["props"] = new JObject { ["leftLabel"] = "ผู้จัดทำ", ["rightLabel"] = "ผู้อนุมัติ", ["showDate"] = true },
        },
        "salesContact" => new JObject
        {
            ["id"] = id, ["type"] = "salesContact",
            ["props"] = new JObject { ["contactId"] = SalesContactAuto, ["kicker"] = "ผู้ประสานงานของ NIS", ["heading"] = "ติดต่อฝ่ายขาย", ["note"] = SalesContactDefaultNote },
        },
        "titlePage" => new JObject { ["id"] = id, ["type"] = "titlePage", ["props"] = new JObject { ["kicker"] = "", ["heading"] = "หัวข้อหน้าใหม่", ["note"] = "" } },
        "caseTable" => new JObject
        {
            ["id"] = id, ["type"] = "caseTable",
            ["props"] = new JObject
            {
                ["title"] = "รายการงานบริการในรอบนี้",
                ["columns"] = new JArray(ColumnPresets["standard"]),
                ["joins"] = new JObject(),
                ["rowSet"] = "intake",
                ["limitMode"] = "all",
                ["sortBy"] = "createdAt", ["sortDir"] = "asc", ["limit"] = 40,
                ["mode"] = "table", ["lineClamp"] = "two", ["personalColumnsAcknowledged"] = false, ["periodOverride"] = JValue.CreateNull(),
            },
        },
        _ => throw new InvalidOperationException($"ไม่รู้จักชนิดบล็อก \"{type}\""),
    };

    private static JObject Chart(string id, string dataset, string title, string caption, int height) => new()
    {
        ["id"] = id, ["type"] = "chart",
        ["props"] = new JObject
        {
            ["dataset"] = dataset, ["chartType"] = DefaultChartType(dataset), ["title"] = title, ["caption"] = caption,
            ["showLegend"] = true, ["height"] = height,
            ["basis"] = "clock", ["series"] = "intake", ["color"] = "auto", ["periodOverride"] = JValue.CreateNull(),
        },
    };

    private static JObject CaseTableBlock(string id, int? caseCount)
    {
        var b = NewBlock("caseTable", id);
        if (caseCount != null)
        {
            b["props"]!["limitMode"] = "all";
            b["props"]!["limit"] = StarterCaseLimit(caseCount.Value);
        }
        return b;
    }

    /// <summary>
    /// โครงตั้งต้นของทั้งสองชนิดเอกสาร (starterTemplate) — book = รายงานรูปเล่ม · brief = สรุปสั้น
    /// preset 'service' (book เท่านั้น) = ปกแบบ service + ฟอนต์ IBM Plex Sans Thai + หน้าติดต่อฝ่ายขายท้ายเล่ม
    /// ผลลัพธ์ผ่าน <see cref="Normalize"/> แล้ว (ค่าเริ่มต้นครบเหมือน TemplateSchema.parse)
    /// </summary>
    public static JObject StarterTemplate(string kind, NisStarterOptions opts)
    {
        var service = opts.Preset == "service" && kind == "book";
        var meta = new JObject
        {
            ["customerId"] = opts.CustomerId,
            ["customerNameTh"] = opts.CustomerNameTh,
            ["customerNameEn"] = opts.CustomerNameEn ?? "",
            ["periodLabelTh"] = opts.PeriodLabelTh ?? "",
        };
        if (service) meta["titleAccent"] = "สรุปงานบริการ";

        var blocks = kind == "brief"
            ? new JArray
            {
                NewBlock("heading", "h-summary"),
                NewBlock("kpiRow", "kpi-main"),
                Chart("c-intake", "breakdown.priority", BriefIntakeChartTitle, BriefIntakeChartCaption, 150),
                CaseTableBlock("t-cases", opts.CaseCount),
                NewBlock("signature", "sign"),
            }
            : new JArray
            {
                NewBlock("cover", "cover"),
                NewBlock("heading", "h-exec"),
                NewBlock("kpiRow", "kpi-main"),
                NewBlock("text", "txt-exec"),
                NewBlock("heading", "h-speed"),
                Chart("c-trend", "trend.day", DefaultChartTitles["trend.day"], DefaultChartCaptions["trend.day"], 180),
                Chart("c-duration", "breakdown.duration", DefaultChartTitles["breakdown.duration"], DefaultChartCaptions["breakdown.duration"], 160),
                NewBlock("heading", "h-cases"),
                CaseTableBlock("t-cases", opts.CaseCount),
            };

        var headings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["h-summary"] = "สรุปงานบริการรอบนี้",
            ["h-exec"] = "สรุปสำหรับผู้บริหาร",
            ["h-speed"] = "เราให้บริการได้เร็วแค่ไหน",
            ["h-cases"] = "รายการงานบริการ",
        };
        foreach (var b in blocks.OfType<JObject>())
        {
            var id = (string?)b["id"] ?? "";
            if ((string?)b["type"] == "heading" && headings.TryGetValue(id, out var text)) b["props"]!["text"] = text;
            if (service && (string?)b["type"] == "cover")
            {
                b["props"]!["style"] = "service";
                b["props"]!["font"] = "IBM Plex Sans Thai";
            }
        }
        if (service) blocks.Add(NewBlock("salesContact", "sales"));

        var footer = service
            ? new JObject { ["enabled"] = true, ["companyName"] = "th", ["logoSize"] = "small" }
            : new JObject { ["enabled"] = true };

        var raw = new JObject
        {
            ["schemaVersion"] = SchemaVersion,
            ["docKind"] = kind,
            ["period"] = new JObject { ["from"] = opts.From, ["to"] = opts.To },
            ["page"] = new JObject { ["header"] = new JObject { ["enabled"] = kind == "brief" }, ["footer"] = footer },
            ["docMeta"] = meta,
            ["blocks"] = blocks,
        };
        var normalized = Normalize(raw, out var issues);
        if (issues.Count > 0)
            throw new InvalidOperationException("starterTemplate สร้าง layout ที่ไม่ผ่าน schema ของตัวเอง:\n" + string.Join("\n", issues));
        return normalized;
    }

    /// <summary>ชุดบล็อกมาตรฐานของชนิดเอกสารหนึ่ง (standardBlocksFor) — ไม่แตะ docMeta · period · page</summary>
    public static JArray StandardBlocksFor(string kind, NisStarterOptions opts) =>
        (JArray)StarterTemplate(kind, opts)["blocks"]!.DeepClone();

    // ═══════════════════════════════════════════════════════════════════════
    // 9. กรอบหน้าของ PDF (template.ts · cover-theme.ts) — ค่าที่ตัวเรนเดอร์ PDF และหน้าพิมพ์ CRM ต้องอ่านตรงกัน
    //    🔴 ไม่ใช่ config — หน้าพิมพ์ยืดหน้าปกด้วยตัวเลขเดียวกันนี้ (printCoverMinHeightPx) ถ้าสองฝั่งใช้คนละตัว
    //       หน้าปกจะล้นไปหน้าถัดไปหรือเหลือช่องว่าง · เปลี่ยนที่นี่ต้องเปลี่ยน schemas/nsr/report/template.ts ของ CRM ด้วย
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>ระยะขอบบน/ล่างที่ page.pdf() ใช้กับเอกสารทั่วไป (มม.) — PDF_PAGE_MARGIN_MM</summary>
    public const int PdfPageMarginMm = 14;

    /// <summary>ระยะบนที่ page.pdf() ใช้กับเอกสารแบบ service (มม.) — SERVICE_PDF_MARGIN_MM.top · หัวกระดาษวาดในพื้นที่นี้</summary>
    public const int ServicePdfMarginTopMm = 30;

    /// <summary>ระยะล่างที่ page.pdf() ใช้กับเอกสารแบบ service (มม.) — SERVICE_PDF_MARGIN_MM.bottom · ท้ายกระดาษวาดในพื้นที่นี้</summary>
    public const int ServicePdfMarginBottomMm = 18;

    /// <summary>ความสูงโลโก้ท้ายกระดาษ (px) ตาม page.footer.logoSize — FOOTER_LOGO_PX</summary>
    public static readonly Dictionary<string, int> FooterLogoPx = new(StringComparer.Ordinal)
    {
        ["small"] = 11,
        ["medium"] = 16,
        ["large"] = 22,
    };

    private static readonly string[] ThMonthsLong =
    {
        "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน",
        "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม",
    };

    /// <summary>เอกสารใช้กรอบหน้าแบบ "เอกสารบริการ NIS" ไหม (isServiceLayout) — ปกแบบ service ต้องเป็นบล็อกแรก</summary>
    public static bool IsServiceLayout(JArray? blocks)
    {
        if (blocks == null || blocks.Count == 0 || blocks[0] is not JObject first) return false;
        return (string?)first["type"] == "cover" && (string?)first["props"]?["style"] == "service";
    }

    /// <summary>
    /// ฟอนต์ของทั้งเอกสาร (documentFont) — ฟอนต์ที่เลือกบนหน้าปก ถ้าไม่มีหน้าปกใช้ theme.fontFamily
    /// ฟอนต์ที่เลิกให้เลือก (TH Sarabun New) ถอยเป็น Sarabun ให้ทุกชั้นตรงกัน
    /// </summary>
    public static string DocumentFont(JArray? blocks, string? themeFontFamily)
    {
        var cover = blocks?.OfType<JObject>().FirstOrDefault(b => (string?)b["type"] == "cover");
        var font = (string?)cover?["props"]?["font"] ?? themeFontFamily ?? "Sarabun";
        return SelectableFonts.Contains(font) ? font : "Sarabun";
    }

    /// <summary>`2026-09-18` → `18 กันยายน 2569` (thaiLongDate) — วันที่ออกเอกสารบนหัวกระดาษแบบ service · อ่านไม่ออก = ""</summary>
    public static string ThaiLongDate(string? ymd)
    {
        if (ymd == null || !IsThaiDate(ymd)) return string.Empty;
        var y = int.Parse(ymd.AsSpan(0, 4), CultureInfo.InvariantCulture);
        var m = int.Parse(ymd.AsSpan(5, 2), CultureInfo.InvariantCulture);
        var d = int.Parse(ymd.AsSpan(8, 2), CultureInfo.InvariantCulture);
        return $"{d} {ThMonthsLong[m - 1]} {y + 543}";
    }

    /// <summary>สีกรอบหน้าของเอกสารแบบ service (serviceFrameColors) — จากชุดสีของหน้าปกบล็อกแรก · ไม่มีหน้าปก = ชุดของ NIS</summary>
    public static (string Navy, string Accent, string AccentInk, string Rule) ServiceFrameColors(JArray? blocks)
    {
        var cover = blocks?.OfType<JObject>().FirstOrDefault(b => (string?)b["type"] == "cover");
        var props = cover?["props"] as JObject;
        var palette = props == null
            ? NisReportCoverContrast.PaletteOf("nis", null, null)
            : NisReportCoverContrast.PaletteOf((string?)props["theme"] ?? "nis", (string?)props["mainColor"], (string?)props["accentColor"]);
        return (palette.C3, palette.Ca, palette.Cad, NisReportCoverContrast.CoverServiceRule);
    }
}
