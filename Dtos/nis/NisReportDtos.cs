using System.Text.Json;
using goalongapi.Models.Nis;
using Newtonsoft.Json.Linq;

namespace goalongapi.Dtos.Nis;

// ── DTO ของโมดูล NIS Service Report ส่วน report builder (api/nis/report) ──────
// port จาก apps/api/src/reports/reports.controller.ts (zod schemas) · templates.service.ts (TemplateDto)
// · reports.service.ts (ReportDto) · packages/report-schema/src/company.ts (CompanyInfoSchema)
// ชื่อ field ใช้ camelCase ตอน serialize (System.Text.Json default ของ AddControllers)
//
// 🔴 layout / snapshot ส่งผ่านเป็น JsonElement (passthrough) — ชั้น service ตรวจรูปด้วย NisReportSchema
//    (Newtonsoft JObject) ก่อนเขียนทุกครั้ง ไม่เชื่อว่า JSON ที่ส่งมาถูก

// ═══════════════════════════════════════════════════════════════════════════
// 1. ข้อมูลบริษัท (CompanyInfoSchema · SPEC §9 ข้อ 27)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>ผู้ติดต่อฝ่ายขายหนึ่งคน (SalesContactSchema) — ข้อมูลบุคคลอยู่ในฐานเท่านั้น ไม่ฝังในโค้ด</summary>
public sealed class NisSalesContactDto
{
    /// <summary>รหัสที่บล็อก salesContact อ้างถึง — ตั้งตอนเพิ่มในหน้าตั้งค่า ไม่ใช่ชื่อคน</summary>
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>QR LINE ของคนนี้ (อัปโหลดแล้ว /api/nis/report/assets/…) · ว่าง = ไม่มีกล่อง QR</summary>
    public string LineQrPath { get; set; } = string.Empty;
    /// <summary>ลูกค้าที่เซลส์คนนี้ดูแล (รหัสลูกค้า) — บล็อกที่ตั้ง "อัตโนมัติ" หยิบคนที่ดูแลลูกค้าของเอกสาร</summary>
    public List<string> CustomerIds { get; set; } = new();
}

/// <summary>
/// ข้อมูลบริษัทที่พิมพ์ลงเอกสาร (CompanyInfoSchema) — ชื่อ field ตรงกับ TS ทุกตัว
/// ยังไม่กรอก = สตริงว่าง = ไม่พิมพ์บรรทัดนั้น · ไม่บล็อก export
/// </summary>
public class NisCompanyInfoDto
{
    public string NameTh { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string AddressTh { get; set; } = string.Empty;
    public string AddressEn { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public string LineQrPath { get; set; } = string.Empty;
    public List<NisSalesContactDto> SalesContacts { get; set; } = new();
}

/// <summary>
/// GET/PUT company — ข้อมูลบริษัท + ช่องที่ยังว่างซึ่งติดป้าย "ต้องกรอกก่อนขึ้น production"
/// <c>masterFields</c> = ช่องที่อ่านจากข้อมูลบริษัทของระบบหลัก (dbo.getcmpinfo) แก้ที่หน้านั้นเท่านั้น · PUT ปฏิเสธ
/// <c>masterAvailable=false</c> = อ่านระบบหลักไม่ได้ ค่าช่องบริษัทที่เห็นเป็นของที่ค้างใน InfoJson
/// </summary>
public sealed class NisCompanyInfoResponseDto : NisCompanyInfoDto
{
    public List<string> GapsBeforeProduction { get; set; } = new();
    public List<string> MasterFields { get; set; } = new();
    public bool MasterAvailable { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 2. Template
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>แถว NisReportTemplate ที่ส่งออก (TemplateDto ใน TS) — layout เป็น JSON ทั้งก้อน</summary>
public sealed class NisReportTemplateDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DocKind { get; set; } = "book";
    public int Version { get; set; }
    public bool IsDefault { get; set; }
    public string? ArchivedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public JsonElement Layout { get; set; }

    /// <summary>แปลงจาก entity — LayoutJson ในฐานถูก parse เป็น JsonElement (ถ้า JSON เสียจะโยน ให้เห็นทันที)</summary>
    public static NisReportTemplateDto From(NisReportTemplate t) => new()
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        Name = t.Name,
        DocKind = t.DocKind,
        Version = t.Version,
        IsDefault = t.IsDefault,
        ArchivedAt = NisServiceCaseDto.Iso(t.ArchivedAt),
        CreatedAt = NisServiceCaseDto.Iso(t.CreatedAt),
        UpdatedAt = NisServiceCaseDto.Iso(t.UpdatedAt),
        Layout = NisReportJson.ToElement(t.LayoutJson),
    };
}

/// <summary>body ของ POST templates (CreateTemplateSchema)</summary>
public sealed class NisCreateTemplateDto
{
    public string CustomerId { get; set; } = string.Empty;
    /// <summary>1–120 ตัวอักษร</summary>
    public string Name { get; set; } = string.Empty;
    public JsonElement? Layout { get; set; }
    public bool? IsDefault { get; set; }
}

/// <summary>body ของ POST templates/starter (StarterSchema) — เริ่มจากโครงสำเร็จรูป</summary>
public sealed class NisCreateStarterDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>book | brief (ค่าเริ่มต้น book)</summary>
    public string DocKind { get; set; } = "book";
    /// <summary>โครงสำเร็จรูปแบบพิเศษ — 'service' = เอกสารบริการ NIS (แม่แบบ MA)</summary>
    public string? Preset { get; set; }
    /// <summary>'YYYY-MM-DD' — บังคับ เพราะ template ต้องตอบได้ตั้งแต่แรกว่าเป็นของเดือนไหน</summary>
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

/// <summary>body ของ PUT templates/{id} (UpdateTemplateSchema) — field ที่ไม่ส่ง = ไม่แตะ</summary>
public sealed class NisUpdateTemplateDto
{
    public string? Name { get; set; }
    public JsonElement? Layout { get; set; }
    public bool? IsDefault { get; set; }
}

/// <summary>body ของ PATCH templates/{id}/archive</summary>
public sealed class NisArchiveTemplateDto
{
    public bool? Archived { get; set; }
}

/// <summary>ความต้องการข้อมูลของช่วงเวลาหนึ่งช่วง (PeriodNeed) ที่ส่งออกจาก POST templates/{id}/projection</summary>
public sealed class NisProjectionNeedDto
{
    public string Key { get; set; } = string.Empty;
    public NisPeriodDto Period { get; set; } = new();
    public List<string> KpiMetrics { get; set; } = new();
    public List<string> Datasets { get; set; } = new();
    public List<string> CaseColumns { get; set; } = new();
    public List<string> CaseFields { get; set; } = new();
    public int CaseRowLimit { get; set; }
    public string CaseDateField { get; set; } = "createdAt";
    /// <summary>ป้ายคอลัมน์ที่มีชื่อคน — คำเตือน E1 ของหน้าออกแบบ (ข้อ 5.1.2)</summary>
    public List<string> PersonalColumns { get; set; } = new();
    public NisWidthMeterDto? WidthMeter { get; set; }
}

/// <summary>ช่วงเวลา 'YYYY-MM-DD' (Period)</summary>
public sealed class NisPeriodDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

/// <summary>มาตรวัดความกว้างตาราง (widthMeter · ข้อ 5.4) — ขึ้นเฉพาะหน้าออกแบบ</summary>
public sealed class NisWidthMeterDto
{
    public double Ratio { get; set; }
    public double FontSize { get; set; }
    /// <summary>fits | shrink | overflow</summary>
    public string Verdict { get; set; } = "fits";
    public string Message { get; set; } = string.Empty;
}

/// <summary>POST templates/{id}/projection — สิ่งที่ layout ฉบับนี้จะดึงและจะเก็บลง snapshot (กฎ Q25 มองเห็นได้)</summary>
public sealed class NisProjectionDto
{
    public NisPeriodDto DocPeriod { get; set; } = new();
    /// <summary>มีบล็อกที่ใช้ช่วงต่างจากเอกสารไหม — หน้าออกแบบต้องขึ้นป้ายเตือน</summary>
    public bool HasOverride { get; set; }
    public List<NisProjectionNeedDto> Needs { get; set; } = new();
    public NisHolidayGateDto HolidayGate { get; set; } = new();
    public string? CustomerNameWarning { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 3. ผลของด่าน (gates) — รูปเดียวกับ TS เพื่อให้หน้าออกแบบขึ้นป้ายได้ก่อนกดออกรายงาน
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>จุดในรายงานที่ตัวเลขมาจากเวลาทำการ (BusinessHourUse)</summary>
public sealed class NisBusinessHourUseDto
{
    /// <summary>kpi | chart</summary>
    public string Kind { get; set; } = "kpi";
    public string BlockId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>ด่านวันหยุด (HolidayGateResult) — SPEC §9 ข้อ 67</summary>
public sealed class NisHolidayGateDto
{
    public bool Blocked { get; set; }
    public List<NisBusinessHourUseDto> Uses { get; set; } = new();
    public List<int> Years { get; set; } = new();
    public List<int> MissingYears { get; set; } = new();
    public string? Message { get; set; }
}

/// <summary>รายละเอียดตารางที่รายการล้น (TruncationBlock.details[])</summary>
public sealed class NisTruncationDetailDto
{
    public string PeriodKey { get; set; } = string.Empty;
    public int Shown { get; set; }
    public int Total { get; set; }
    public int Omitted { get; set; }
}

/// <summary>ด่านภาคผนวก (TruncationBlock) — SPEC §9 ข้อ 17</summary>
public sealed class NisTruncationGateDto
{
    public bool Blocked { get; set; }
    public int OmittedTotal { get; set; }
    public List<NisTruncationDetailDto> Details { get; set; } = new();
    public string? Message { get; set; }
}

/// <summary>ช่วงที่ล้ำเกินข้อมูลที่มี (FreshnessBlock.beyond[])</summary>
public sealed class NisFreshnessBeyondDto
{
    public string PeriodKey { get; set; } = string.Empty;
    public string NeedUntil { get; set; } = string.Empty;
}

/// <summary>ด่านความสด (FreshnessBlock) — SPEC §9 ข้อ 41</summary>
public sealed class NisFreshnessGateDto
{
    public bool Blocked { get; set; }
    public string? Message { get; set; }
    public string? DataAsOf { get; set; }
    public List<NisFreshnessBeyondDto> Beyond { get; set; } = new();
}

/// <summary>ด่าน "พิมพ์ได้ไหม" (PrintabilityBlock) — snapshot ที่ไม่รู้ว่านับชุดไหนออกเป็นเอกสารไม่ได้</summary>
public sealed class NisPrintabilityGateDto
{
    public bool Blocked { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Periods { get; set; } = new();
}

/// <summary>คู่สีที่ตรวจบนหน้าปก (ContrastFinding)</summary>
public sealed class NisContrastFindingDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Style { get; set; } = "network";
    public double Required { get; set; }
    public double Actual { get; set; }
    public string Fg { get; set; } = string.Empty;
    public string Bg { get; set; } = string.Empty;
    /// <summary>true = สีนี้ผู้ใช้ตั้งเองรายบรรทัด ไม่ใช่สีที่ระบบคำนวณ</summary>
    public bool UserSet { get; set; }
    public bool Passes { get; set; }
}

/// <summary>เฉดใกล้เคียงที่ผ่านเกณฑ์ให้กดรับ (ColorSuggestion) — ห้ามใช้แทนที่แบบเงียบ ๆ</summary>
public sealed class NisColorSuggestionDto
{
    public string Main { get; set; } = string.Empty;
    public string Accent { get; set; } = string.Empty;
    public int MainShiftPct { get; set; }
    public int AccentShiftPct { get; set; }
}

/// <summary>ด่านสีหน้าปก (CoverContrastGate) — SPEC §9 ข้อ 20</summary>
public sealed class NisCoverContrastGateDto
{
    public bool Blocked { get; set; }
    public List<NisContrastFindingDto> Failures { get; set; } = new();
    public NisColorSuggestionDto? Suggestion { get; set; }
    public string? Message { get; set; }
}

/// <summary>ด่านหน้าติดต่อฝ่ายขาย (salesContactGate) — คุณ Tone 18 ก.ย. 2569 "ห้าม ข้อมูลต้องถูกต้องก่อนส่งมอบลูกค้า"</summary>
public sealed class NisSalesContactGateDto
{
    public bool Blocked { get; set; }
    public string? Message { get; set; }
    public string? ContactName { get; set; }
}

/// <summary>POST templates/{id}/preview — snapshot ตัวอย่าง (รูปเดียวกับที่จะถูกแช่แข็ง) + สถานะทุกด่าน</summary>
public sealed class NisPreviewDto
{
    public JsonElement Snapshot { get; set; }
    public NisHolidayGateDto HolidayGate { get; set; } = new();
    public NisTruncationGateDto TruncationGate { get; set; } = new();
    public NisCoverContrastGateDto CoverContrastGate { get; set; } = new();
    public NisSalesContactGateDto SalesContactGate { get; set; } = new();
    public string? CustomerNameWarning { get; set; }
    public NisFreshnessGateDto FreshnessGate { get; set; } = new();
    /// <summary>เลขฉบับล่าสุดของลูกค้ารายนี้ — หน้าจอเตือนเมื่อเลขข้ามลำดับ</summary>
    public int? LastIssueNo { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 4. Report
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>แถว NisReport ที่ส่งออก (ReportDto ใน TS) — ไม่ส่ง PdfBlob</summary>
public sealed class NisReportDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    /// <summary>ชื่อ template ณ ตอนที่เรียกดู — เป็นแค่ label ไม่ได้แช่แข็ง (ต่างจาก templateVersion/layoutSnapshot)</summary>
    public string TemplateName { get; set; } = string.Empty;
    public int TemplateVersion { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int? IssueNo { get; set; }
    public string PeriodStart { get; set; } = string.Empty;
    public string PeriodEnd { get; set; } = string.Empty;
    /// <summary>draft | generating | ready | failed</summary>
    public string Status { get; set; } = "draft";
    public string? PdfPath { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public JsonElement LayoutSnapshot { get; set; }
    public JsonElement DataSnapshot { get; set; }

    /// <summary>แปลงจาก entity + ชื่อ template</summary>
    public static NisReportDto From(NisReport r, string templateName) => new()
    {
        Id = r.Id,
        TemplateId = r.TemplateId,
        TemplateName = templateName,
        TemplateVersion = r.TemplateVersion,
        CustomerId = r.CustomerId,
        CustomerName = r.CustomerName,
        IssueNo = r.IssueNo,
        PeriodStart = NisServiceCaseDto.Iso(r.PeriodStart),
        PeriodEnd = NisServiceCaseDto.Iso(r.PeriodEnd),
        Status = r.Status,
        PdfPath = r.PdfPath,
        CreatedAt = NisServiceCaseDto.Iso(r.CreatedAt),
        LayoutSnapshot = NisReportJson.ToElement(r.LayoutSnapshotJson),
        DataSnapshot = NisReportJson.ToElement(r.DataSnapshotJson),
    };
}

/// <summary>body ของ POST reports (CreateReportSchema)</summary>
public sealed class NisCreateReportDto
{
    public string TemplateId { get; set; } = string.Empty;
    /// <summary>ไม่บังคับ · ถ้าส่งมาต้องตรงกับ template.period — ช่วงเวลาอยู่ที่เอกสารที่เดียว</summary>
    public string? From { get; set; }
    public string? To { get; set; }
    /// <summary>1–999 หรือ null</summary>
    public int? IssueNo { get; set; }
}

/// <summary>แถว NisReportJob ที่ส่งออก — หน้าจอ poll ดูสถานะจริงที่งานผ่าน (SPEC §9 ข้อ 39)</summary>
public sealed class NisReportJobDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    /// <summary>queued | running | done | failed</summary>
    public string Status { get; set; } = "queued";
    /// <summary>rendering | writing</summary>
    public string? Stage { get; set; }
    public int Attempts { get; set; }
    public string? LeaseUntil { get; set; }
    public string? WorkerId { get; set; }
    public string? Error { get; set; }
    public string? PdfPath { get; set; }
    public int? PdfBytes { get; set; }
    public int? RenderMs { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? StartedAt { get; set; }
    public string? FinishedAt { get; set; }

    /// <summary>แปลงจาก entity</summary>
    public static NisReportJobDto From(NisReportJob j) => new()
    {
        Id = j.Id,
        ReportId = j.ReportId,
        Status = j.Status,
        Stage = j.Stage,
        Attempts = j.Attempts,
        LeaseUntil = NisServiceCaseDto.Iso(j.LeaseUntil),
        WorkerId = j.WorkerId,
        Error = j.Error,
        PdfPath = j.PdfPath,
        PdfBytes = j.PdfBytes,
        RenderMs = j.RenderMs,
        CreatedAt = NisServiceCaseDto.Iso(j.CreatedAt),
        StartedAt = NisServiceCaseDto.Iso(j.StartedAt),
        FinishedAt = NisServiceCaseDto.Iso(j.FinishedAt),
    };
}

/// <summary>ผลอัปโหลดโลโก้/QR (StoredLogo) — urlPath เอาไปใส่ docMeta.customerLogoPath / company.lineQrPath ตรง ๆ</summary>
public sealed class NisAssetUploadResultDto
{
    public string UrlPath { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public string Mime { get; set; } = string.Empty;
}

/// <summary>
/// GET reports/{id}/print-data?key= — ข้อมูลที่หน้าพิมพ์บน CRM (/nsr-print/{id}) ใช้วาด
/// company = ชุดที่แช่แข็งไว้ใน snapshot (รายงานก่อนมีช่องนี้ใช้ค่าปัจจุบัน)
/// </summary>
public sealed class NisPrintDataDto
{
    public NisReportDto Report { get; set; } = new();
    public NisCompanyInfoDto Company { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════════════
// 5. Exception ที่ controller แปลงเป็น status code (เพิ่มจากชุดของ archive)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>ข้อมูลที่ส่งมาไม่ถูกต้อง → 400 · Details = ข้อมูลเพิ่มเติม (issues, inLayout, …) แบบที่ NestJS แนบใน body</summary>
public sealed class NisReportBadRequestException : Exception
{
    public IDictionary<string, object?>? Details { get; }

    public NisReportBadRequestException(string message, IDictionary<string, object?>? details = null) : base(message)
    {
        Details = details;
    }
}

/// <summary>ด่านธุรกิจปฏิเสธ (holiday / truncation / freshness / printability / contrast / sales) → 422</summary>
public sealed class NisUnprocessableException : Exception
{
    public IDictionary<string, object?>? Details { get; }

    public NisUnprocessableException(string message, IDictionary<string, object?>? details = null) : base(message)
    {
        Details = details;
    }
}

/// <summary>ชนกับสถานะ/คีย์ที่มีอยู่ (ชื่อ template ซ้ำ · แก้ template ที่เก็บเข้ากรุ) → 409</summary>
public sealed class NisConflictException : Exception
{
    public NisConflictException(string message) : base(message) { }
}

/// <summary>print key ไม่ถูกต้องหรือหมดอายุ → 401</summary>
public sealed class NisUnauthorizedException : Exception
{
    public NisUnauthorizedException(string message) : base(message) { }
}

// ═══════════════════════════════════════════════════════════════════════════
// 6. ตัวช่วยแปลง JSON ระหว่าง System.Text.Json (ขอบ HTTP) กับ Newtonsoft (ชั้นตรวจ/ประกอบ)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// สะพานระหว่าง JsonElement (MVC serialize) กับ JObject (NisReportSchema ใช้ตรวจ/ประกอบ snapshot)
/// — JObject ที่ serialize ด้วย System.Text.Json ตรง ๆ จะได้รูปผิด จึงต้องแปลงผ่านข้อความเสมอ
/// </summary>
public static class NisReportJson
{
    /// <summary>JSON string → JsonElement อิสระ (ไม่ผูกกับ JsonDocument ที่ถูก dispose) · ข้อความว่าง/เสีย = object ว่าง</summary>
    public static JsonElement ToElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return JsonSerializer.Deserialize<JsonElement>("{}");
        try { return JsonSerializer.Deserialize<JsonElement>(json); }
        catch (JsonException) { return JsonSerializer.Deserialize<JsonElement>("{}"); }
    }

    /// <summary>JToken → JsonElement</summary>
    public static JsonElement ToElement(JToken token) =>
        JsonSerializer.Deserialize<JsonElement>(token.ToString(Newtonsoft.Json.Formatting.None));

    /// <summary>JsonElement → JToken (null/undefined → JValue null)</summary>
    public static JToken ToToken(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return JValue.CreateNull();
        return JToken.Parse(element.GetRawText());
    }

    /// <summary>JSON string → JObject · ข้อความว่าง/เสีย/ไม่ใช่ object → null</summary>
    public static JObject? TryParseObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JToken.Parse(json) as JObject; }
        catch (Newtonsoft.Json.JsonException) { return null; }
    }

    /// <summary>serialize แบบ compact ไม่มีช่องว่าง — ใช้เทียบว่า layout เปลี่ยนจริงไหม และใช้เก็บลงฐาน</summary>
    public static string Compact(JToken token) => token.ToString(Newtonsoft.Json.Formatting.None);
}
