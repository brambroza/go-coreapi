using System.Globalization;
using System.Text.Json;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace goalongapi.Dtos.Nis;

// ── DTO ของโมดูล NIS Service Report ส่วน archive (api/nis/archive) ───────────
// port จาก packages/shared/src/{api,service-case}.ts และ apps/api/src/cases/*
// ชื่อ field ใช้ camelCase ตอน serialize (System.Text.Json default ของ AddControllers)

// ═══════════════════════════════════════════════════════════════════════════
// 1. เคสงานบริการ
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// แถวจากตาราง NisServiceCase โดยไม่โหลด RawJson — ใช้เป็นเป้าหมาย Select ของ EF
/// (projection ไปยัง entity type โดยตรงทำไม่ได้ และไม่ควรดึง RawJson ทุกแถว)
/// </summary>
public sealed class NisCaseRow
{
    public string SourceId { get; set; } = string.Empty;
    public string CaseNo { get; set; } = string.Empty;
    public string ExternalGuid { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? Province { get; set; }
    public string? ContactName { get; set; }
    public string? RequestBy { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "minor";
    public string Status { get; set; } = "new";
    public string? Remark { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public double? ResolutionHours { get; set; }
    public string AssigneeIdsJson { get; set; } = "[]";
    public string AssigneeNamesJson { get; set; } = "[]";
    public string? PrimaryAssignee { get; set; }
    public string? ActionDetails { get; set; }
    public string ActionByNamesJson { get; set; } = "[]";
    public int? ServiceTypeCode { get; set; }
    public int? SatisfactionScore { get; set; }
    public bool ViaLine { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsTestData { get; set; }

    /// <summary>projection จาก entity — ไม่แตะ RawJson / SyncedAt / ช่องติดต่อส่วนบุคคล</summary>
    public static readonly System.Linq.Expressions.Expression<Func<NisServiceCase, NisCaseRow>> Projection = c => new NisCaseRow
    {
        SourceId = c.SourceId,
        CaseNo = c.CaseNo,
        ExternalGuid = c.ExternalGuid,
        CmpId = c.CmpId,
        CustomerId = c.CustomerId,
        CustomerName = c.CustomerName,
        BranchName = c.BranchName,
        Province = c.Province,
        ContactName = c.ContactName,
        RequestBy = c.RequestBy,
        Subject = c.Subject,
        Category = c.Category,
        Priority = c.Priority,
        Status = c.Status,
        Remark = c.Remark,
        CreatedAt = c.CreatedAt,
        StartedAt = c.StartedAt,
        ResolvedAt = c.ResolvedAt,
        ModifiedAt = c.ModifiedAt,
        ResolutionHours = c.ResolutionHours,
        AssigneeIdsJson = c.AssigneeIdsJson,
        AssigneeNamesJson = c.AssigneeNamesJson,
        PrimaryAssignee = c.PrimaryAssignee,
        ActionDetails = c.ActionDetails,
        ActionByNamesJson = c.ActionByNamesJson,
        ServiceTypeCode = c.ServiceTypeCode,
        SatisfactionScore = c.SatisfactionScore,
        ViaLine = c.ViaLine,
        AttachmentCount = c.AttachmentCount,
        IsTestData = c.IsTestData,
    };
}

/// <summary>
/// รูปที่ส่งข้าม HTTP ของเคส (ServiceCaseDto ใน TS) — วันที่เป็น ISO string (UTC)
///
/// 🔴 ตัด raw / syncedAt / requestPhone / requestEmail / requestPosition / anydeskId /
///    satisfactionComment ออกตาม cases.service.ts:toDto — ค่ายังอยู่ครบในฐาน ตัดเฉพาะขาออก HTTP
///    (ตรวจความปลอดภัย 24 ก.ย. 2569) · เก็บ contactName ไว้เพราะเป็นคอลัมน์รายงานที่ผู้ใช้เลือกได้
/// </summary>
public sealed class NisServiceCaseDto
{
    public string SourceId { get; set; } = string.Empty;
    public string CaseNo { get; set; } = string.Empty;
    public string ExternalGuid { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? Province { get; set; }
    public string? ContactName { get; set; }
    public string? RequestBy { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "minor";
    public string Status { get; set; } = "new";
    public string? Remark { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? StartedAt { get; set; }
    public string? ResolvedAt { get; set; }
    public string? ModifiedAt { get; set; }
    /// <summary>เวลานาฬิกา (แจ้ง→ปิด) ชั่วโมง · null = ยังไม่ปิด หรือกลุ่มลูกค้าบันทึกย้อนหลัง (ล้างค่า)</summary>
    public double? ResolutionHours { get; set; }
    public List<string> AssigneeIds { get; set; } = new();
    public List<string> AssigneeNames { get; set; } = new();
    public string? PrimaryAssignee { get; set; }
    public string? ActionDetails { get; set; }
    public List<string> ActionByNames { get; set; } = new();
    public int? ServiceTypeCode { get; set; }
    public int? SatisfactionScore { get; set; }
    public bool ViaLine { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsTestData { get; set; }

    /// <summary>ISO 8601 แบบ UTC ตรงกับ Date.toISOString() ของ TS</summary>
    public static string Iso(DateTimeOffset at) =>
        at.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

    /// <summary>ISO 8601 แบบ UTC หรือ null</summary>
    public static string? Iso(DateTimeOffset? at) => at.HasValue ? Iso(at.Value) : null;

    /// <summary>แปลงจากแถว projection</summary>
    public static NisServiceCaseDto From(NisCaseRow r) => new()
    {
        SourceId = r.SourceId,
        CaseNo = r.CaseNo,
        ExternalGuid = r.ExternalGuid,
        CompanyId = r.CmpId,
        CustomerId = r.CustomerId,
        CustomerName = r.CustomerName,
        BranchName = r.BranchName,
        Province = r.Province,
        ContactName = r.ContactName,
        RequestBy = r.RequestBy,
        Subject = r.Subject,
        Category = r.Category,
        Priority = r.Priority,
        Status = r.Status,
        Remark = r.Remark,
        CreatedAt = Iso(r.CreatedAt),
        StartedAt = Iso(r.StartedAt),
        ResolvedAt = Iso(r.ResolvedAt),
        ModifiedAt = Iso(r.ModifiedAt),
        ResolutionHours = r.ResolutionHours,
        AssigneeIds = ParseStringList(r.AssigneeIdsJson),
        AssigneeNames = ParseStringList(r.AssigneeNamesJson),
        PrimaryAssignee = r.PrimaryAssignee,
        ActionDetails = r.ActionDetails,
        ActionByNames = ParseStringList(r.ActionByNamesJson),
        ServiceTypeCode = r.ServiceTypeCode,
        SatisfactionScore = r.SatisfactionScore,
        ViaLine = r.ViaLine,
        AttachmentCount = r.AttachmentCount,
        IsTestData = r.IsTestData,
    };

    /// <summary>แปลงจาก entity เต็ม (ตัด RawJson ทิ้งที่นี่)</summary>
    public static NisServiceCaseDto From(NisServiceCase c) => From(NisCaseRow.Projection.Compile()(c));

    /// <summary>อ่าน JSON array ของ string — JSON เสียคืน list ว่าง ไม่โยน</summary>
    public static List<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try { return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 2. Query ของ GET cases (CaseQuerySchema ใน packages/shared/src/api.ts)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// model binder แยกค่าด้วยจุลภาค — <c>?status=new,finish</c> และ <c>?status=new&amp;status=finish</c> ได้ผลเท่ากัน
/// (default binder ของ ASP.NET รับเฉพาะแบบ key ซ้ำ)
/// </summary>
public sealed class NisCommaListModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var values = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (values == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }
        var list = values.Values
            .SelectMany(v => (v ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToList();
        bindingContext.Result = ModelBindingResult.Success(list.Count == 0 ? null : list);
        return Task.CompletedTask;
    }
}

/// <summary>
/// เฉพาะส่วนที่ใช้ "กรองแถว" (CaseFilter ใน TS) — analytics ใช้ตัวนี้ร่วมกับ archive
/// เพื่อให้ตัวเลขบนหน้าภาพรวมกับจำนวนแถวหน้ารายการ Case มาจาก where เดียวกัน
///
/// 🔴 <c>DateField</c> คือกติกาตัวเลขสองชุด (SPEC 9.1): createdAt = "งานที่แจ้งเข้า" · resolvedAt = "งานที่ปิดได้"
/// </summary>
public class NisCaseFilter
{
    /// <summary>ขอบต้นช่วง — 'YYYY-MM-DD' (ต้นวันเวลาไทย) หรือ ISO 8601</summary>
    public string? From { get; set; }

    /// <summary>ขอบท้ายช่วง — 'YYYY-MM-DD' (ท้ายวันเวลาไทย 23:59:59.999) หรือ ISO 8601</summary>
    public string? To { get; set; }

    /// <summary>createdAt | resolvedAt (ค่าเริ่มต้น createdAt)</summary>
    public string DateField { get; set; } = "createdAt";

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? CustomerId { get; set; }

    /// <summary>ค้นในชื่อผู้รับผิดชอบ (AssigneeNamesJson) — ช่องทางภายในเท่านั้น (SPEC §9 ข้อ 70)</summary>
    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Assignee { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Status { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Priority { get; set; }

    [ModelBinder(typeof(NisCommaListModelBinder))]
    public List<string>? Category { get; set; }

    /// <summary>ค้นหาในหัวข้อ · เลขที่เคส · ชื่อลูกค้า · วิธีแก้ไขปัญหา</summary>
    public string? Q { get; set; }

    /// <summary>true = เอาข้อมูลทดสอบเข้ามาด้วย (ค่าเริ่มต้น false — กรองที่ชั้นเดียว)</summary>
    public bool IncludeTest { get; set; }

    /// <summary>เฉพาะเคสที่ยังไม่ปิด (resolvedAt ว่าง)</summary>
    public bool Unresolved { get; set; }

    /// <summary>คะแนนความพึงพอใจไม่เกินค่านี้ (1–5) · เคสที่ไม่มีคะแนนไม่เข้าเงื่อนไข</summary>
    public int? MaxSatisfaction { get; set; }

    public static readonly string[] DateFields = { "createdAt", "resolvedAt" };
    public static readonly string[] Statuses = { "new", "inprogress", "finish" };
    public static readonly string[] Priorities = { "critical", "major", "medium", "minor" };

    /// <summary>ขอบต้นช่วงที่ parse แล้ว (null เมื่อไม่ส่งหรือรูปผิด — เรียก Validate ก่อน)</summary>
    public DateTimeOffset? FromAt => string.IsNullOrWhiteSpace(From) ? null : NisBusinessHours.ParseThaiRangeBoundary(From, endEdge: false);

    /// <summary>ขอบท้ายช่วงที่ parse แล้ว</summary>
    public DateTimeOffset? ToAt => string.IsNullOrWhiteSpace(To) ? null : NisBusinessHours.ParseThaiRangeBoundary(To, endEdge: true);

    /// <summary>ตรวจตัวกรอง — คืนรายการข้อผิดพลาด (ว่าง = ผ่าน) ให้ controller ตอบ 400</summary>
    public virtual List<string> Validate()
    {
        var issues = new List<string>();
        if (!string.IsNullOrWhiteSpace(From) && FromAt == null)
            issues.Add("from: รูปแบบวันที่ไม่ถูกต้อง — ใช้ YYYY-MM-DD หรือ ISO 8601 (และต้องเป็นวันที่ที่มีอยู่จริง)");
        if (!string.IsNullOrWhiteSpace(To) && ToAt == null)
            issues.Add("to: รูปแบบวันที่ไม่ถูกต้อง — ใช้ YYYY-MM-DD หรือ ISO 8601 (และต้องเป็นวันที่ที่มีอยู่จริง)");
        // 🔴 from ต้องมาก่อน to — สลับกันแล้วต้องไม่ได้ 200 (ศูนย์ที่ดูเหมือนคำตอบ อันตรายกว่า error)
        if (FromAt != null && ToAt != null && FromAt > ToAt)
            issues.Add($"to: ช่วงเวลากลับหัว — from ({From}) อยู่หลัง to ({To})");
        if (!DateFields.Contains(DateField))
            issues.Add("dateField: ต้องเป็น createdAt หรือ resolvedAt");
        if (Status != null && Status.Any(s => !Statuses.Contains(s)))
            issues.Add("status: ต้องเป็น new | inprogress | finish");
        if (Priority != null && Priority.Any(p => !Priorities.Contains(p)))
            issues.Add("priority: ต้องเป็น critical | major | medium | minor");
        if (MaxSatisfaction is < 1 or > 5)
            issues.Add("maxSatisfaction: ต้องอยู่ระหว่าง 1–5");
        if (Q != null && Q.Trim().Length == 0)
            issues.Add("q: ต้องไม่ว่าง");
        return issues;
    }
}

/// <summary>query เต็มของ GET api/nis/archive/cases — ตัวกรอง + เรียง + แบ่งหน้า</summary>
public sealed class NisCaseQuery : NisCaseFilter
{
    public static readonly string[] Sorts =
        { "createdAt", "resolvedAt", "priority", "customerName", "resolutionHours", "satisfactionScore" };

    public const int MaxPageSize = 500;

    public string Sort { get; set; } = "createdAt";

    /// <summary>asc | desc (ค่าเริ่มต้น desc)</summary>
    public string Order { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;

    /// <inheritdoc />
    public override List<string> Validate()
    {
        var issues = base.Validate();
        if (!Sorts.Contains(Sort)) issues.Add("sort: ค่าที่รองรับคือ " + string.Join(" | ", Sorts));
        if (Order != "asc" && Order != "desc") issues.Add("order: ต้องเป็น asc หรือ desc");
        if (Page < 1) issues.Add("page: ต้องเป็นจำนวนเต็ม ≥ 1");
        if (PageSize < 1 || PageSize > MaxPageSize) issues.Add($"pageSize: ต้องอยู่ระหว่าง 1–{MaxPageSize}");
        return issues;
    }
}

/// <summary>ผลของ ListCasesAsync — แถวของหน้าที่ขอ + ตัวเลขสรุปของ "ทั้งชุดที่ตัวกรองคัดไว้"</summary>
public sealed class NisCaseListResult
{
    public List<NisServiceCaseDto> Rows { get; set; } = new();
    public int Total { get; set; }
    /// <summary>เคสที่ยังไม่ปิดในชุดที่กรอง (นับที่ฐาน ไม่ใช่จากหน้าที่แสดง)</summary>
    public int UnresolvedCount { get; set; }
    /// <summary>เคสที่ช่องวิธีแก้สั้นกว่าเกณฑ์</summary>
    public int ShortSolutionCount { get; set; }
    /// <summary>เกณฑ์ความยาว — คุณ Tone ตัดสิน 5 ก.ย. 2569 ว่าใช้ 15</summary>
    public int ShortSolutionChars { get; set; } = 15;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public string DateField { get; set; } = "createdAt";
    /// <summary>"งานที่แจ้งเข้า" หรือ "งานที่ปิดได้" — หน้าเว็บเอาไปขึ้นป้ายเสมอ</summary>
    public string CountLabel { get; set; } = "งานที่แจ้งเข้า";
    public bool IncludeTest { get; set; }

    /// <summary>meta สำหรับ NisApiResponse (ไม่รวม total/dataAsOf ที่ envelope ใส่ให้)</summary>
    public Dictionary<string, object?> ToMeta() => new()
    {
        ["unresolvedCount"] = UnresolvedCount,
        ["shortSolutionCount"] = ShortSolutionCount,
        ["shortSolutionChars"] = ShortSolutionChars,
        ["page"] = Page,
        ["pageSize"] = PageSize,
        ["pageCount"] = PageCount,
        ["dateField"] = DateField,
        ["countLabel"] = CountLabel,
        ["includeTest"] = IncludeTest,
    };
}

/// <summary>รายชื่อลูกค้า + จำนวนเคสแจ้งเข้าทั้งคลัง — เติม dropdown ตัวกรอง</summary>
public sealed class NisCustomerSummaryDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>นับตาม createdAt ทั้งคลัง ไม่จำกัดช่วง (ชื่อ field บอกทั้งว่านับอะไรและช่วงไหน)</summary>
    public int IntakeAllTime { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 3. วันหยุด
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>แถว NisHoliday ที่ส่งออก</summary>
public sealed class NisHolidayDto
{
    public string Date { get; set; } = string.Empty;
    public int Year { get; set; }
    public string NameTh { get; set; } = string.Empty;
    public string Kind { get; set; } = "government";
    public bool Verified { get; set; }
    public bool Observed { get; set; }
    public string? Note { get; set; }

    /// <summary>แปลงจาก entity</summary>
    public static NisHolidayDto From(NisHoliday h) => new()
    {
        Date = h.Date, Year = h.Year, NameTh = h.NameTh, Kind = h.Kind,
        Verified = h.Verified, Observed = h.Observed, Note = h.Note,
    };
}

/// <summary>
/// body ของ POST holidays — parse จาก JsonElement เองเพื่อให้ strict (คีย์ที่ไม่รู้จัก = 400) เหมือน zod
/// </summary>
public sealed class NisHolidayCreateDto
{
    public string Date { get; set; } = string.Empty;
    public string NameTh { get; set; } = string.Empty;
    /// <summary>เพิ่มจากหน้าจอถือเป็นวันหยุดที่บริษัทตั้งเอง (ค่าเริ่มต้น company)</summary>
    public string Kind { get; set; } = "company";
    public bool Verified { get; set; }
    public bool Observed { get; set; }
    public string? Note { get; set; }

    private static readonly string[] Allowed = { "date", "nameTh", "kind", "verified", "observed", "note" };

    /// <summary>parse + validate · คืน false พร้อม issues เมื่อข้อมูลไม่ถูกต้อง</summary>
    public static bool TryParse(JsonElement body, out NisHolidayCreateDto dto, out List<string> issues)
    {
        dto = new NisHolidayCreateDto();
        issues = NisHolidayJson.CheckObject(body, Allowed);
        if (issues.Count > 0) return false;

        dto.Date = NisHolidayJson.GetString(body, "date", issues, required: true, max: 10) ?? string.Empty;
        if (dto.Date.Length > 0 && !NisHolidayJson.IsHolidayDate(dto.Date)) issues.Add("date: ต้องเป็นรูปแบบ 'YYYY-MM-DD' และเป็นวันที่ที่มีอยู่จริง");
        dto.NameTh = NisHolidayJson.GetString(body, "nameTh", issues, required: true, max: 200) ?? string.Empty;
        dto.Kind = NisHolidayJson.GetString(body, "kind", issues, required: false, max: 20) ?? "company";
        if (!NisHolidayJson.Kinds.Contains(dto.Kind)) issues.Add("kind: ต้องเป็น government หรือ company");
        dto.Verified = NisHolidayJson.GetBool(body, "verified", issues) ?? false;
        dto.Observed = NisHolidayJson.GetBool(body, "observed", issues) ?? false;
        dto.Note = NisHolidayJson.GetString(body, "note", issues, required: false, max: 500, allowNull: true);
        return issues.Count == 0;
    }
}

/// <summary>
/// body ของ PUT holidays/{date} — field ที่ไม่ได้ส่งมา = ไม่แตะ (ไม่ใช่ล้างเป็นค่าเริ่มต้น)
/// จึงต้องรู้ว่า "ส่งมาไหม" แยกจาก "ค่าเป็น null ไหม" — parse จาก JsonElement เอง
/// </summary>
public sealed class NisHolidayUpdateDto
{
    public bool HasNameTh { get; private set; }
    public string? NameTh { get; private set; }
    public bool HasKind { get; private set; }
    public string? Kind { get; private set; }
    public bool HasVerified { get; private set; }
    public bool? Verified { get; private set; }
    public bool HasObserved { get; private set; }
    public bool? Observed { get; private set; }
    public bool HasNote { get; private set; }
    public string? Note { get; private set; }

    private static readonly string[] Allowed = { "nameTh", "kind", "verified", "observed", "note" };

    /// <summary>parse + validate · คืน false พร้อม issues เมื่อข้อมูลไม่ถูกต้อง</summary>
    public static bool TryParse(JsonElement body, out NisHolidayUpdateDto dto, out List<string> issues)
    {
        dto = new NisHolidayUpdateDto();
        issues = NisHolidayJson.CheckObject(body, Allowed);
        if (issues.Count > 0) return false;

        if (body.TryGetProperty("nameTh", out _))
        {
            dto.HasNameTh = true;
            dto.NameTh = NisHolidayJson.GetString(body, "nameTh", issues, required: true, max: 200);
        }
        if (body.TryGetProperty("kind", out _))
        {
            dto.HasKind = true;
            dto.Kind = NisHolidayJson.GetString(body, "kind", issues, required: true, max: 20);
            if (dto.Kind != null && !NisHolidayJson.Kinds.Contains(dto.Kind)) issues.Add("kind: ต้องเป็น government หรือ company");
        }
        if (body.TryGetProperty("verified", out _))
        {
            dto.HasVerified = true;
            dto.Verified = NisHolidayJson.GetBool(body, "verified", issues);
        }
        if (body.TryGetProperty("observed", out _))
        {
            dto.HasObserved = true;
            dto.Observed = NisHolidayJson.GetBool(body, "observed", issues);
        }
        if (body.TryGetProperty("note", out _))
        {
            dto.HasNote = true;
            dto.Note = NisHolidayJson.GetString(body, "note", issues, required: false, max: 500, allowNull: true);
        }
        return issues.Count == 0;
    }
}

/// <summary>ตัวช่วยอ่าน JSON body ของวันหยุด (ใช้ร่วมสอง DTO)</summary>
public static class NisHolidayJson
{
    public static readonly string[] Kinds = { "government", "company" };

    /// <summary>ต้องเป็น object และไม่มีคีย์นอกรายการ (strict)</summary>
    public static List<string> CheckObject(JsonElement body, string[] allowed)
    {
        var issues = new List<string>();
        if (body.ValueKind != JsonValueKind.Object)
        {
            issues.Add("(ราก): ต้องเป็น JSON object");
            return issues;
        }
        foreach (var p in body.EnumerateObject())
            if (!allowed.Contains(p.Name)) issues.Add($"{p.Name}: คีย์ที่ไม่รู้จัก");
        return issues;
    }

    /// <summary>อ่าน string (trim) — ตรวจชนิด/ความยาว/ว่าง</summary>
    public static string? GetString(JsonElement body, string name, List<string> issues, bool required, int max, bool allowNull = false)
    {
        if (!body.TryGetProperty(name, out var el))
        {
            if (required) issues.Add($"{name}: ต้องระบุ");
            return null;
        }
        if (el.ValueKind == JsonValueKind.Null)
        {
            if (!allowNull) issues.Add($"{name}: ต้องไม่เป็น null");
            return null;
        }
        if (el.ValueKind != JsonValueKind.String)
        {
            issues.Add($"{name}: ต้องเป็นข้อความ");
            return null;
        }
        var v = el.GetString() ?? string.Empty;
        if (required && v.Trim().Length == 0) issues.Add($"{name}: ต้องไม่ว่าง");
        if (v.Length > max) issues.Add($"{name}: ยาวเกิน {max} ตัวอักษร");
        return v;
    }

    /// <summary>อ่าน boolean — รับเฉพาะ true/false จริง (ไม่รับ "true" ที่เป็นข้อความ)</summary>
    public static bool? GetBool(JsonElement body, string name, List<string> issues)
    {
        if (!body.TryGetProperty(name, out var el)) return null;
        if (el.ValueKind == JsonValueKind.True) return true;
        if (el.ValueKind == JsonValueKind.False) return false;
        issues.Add($"{name}: ต้องเป็น true หรือ false");
        return null;
    }

    /// <summary>'YYYY-MM-DD' และเป็นวันที่จริงในปฏิทิน</summary>
    public static bool IsHolidayDate(string s) =>
        s.Length == 10 && DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}

/// <summary>แถว NisHolidayChangeLog ที่ส่งออก</summary>
public sealed class NisHolidayChangeDto
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }

    /// <summary>แปลงจาก entity</summary>
    public static NisHolidayChangeDto From(NisHolidayChangeLog c) => new()
    {
        Id = c.Id, Date = c.Date, Field = c.Field, OldValue = c.OldValue, NewValue = c.NewValue,
        ChangedBy = c.ChangedBy, ChangedAt = c.ChangedAt,
    };
}

// ═══════════════════════════════════════════════════════════════════════════
// 4. Sync
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>ผลหนึ่งรอบ sync (SyncResult ใน TS) — ใช้ทั้งคืนจาก endpoint sync/run และเก็บเป็น last ใน status</summary>
public sealed class NisSyncResultDto
{
    /// <summary>ok | failed | skipped</summary>
    public string Status { get; set; } = "skipped";
    public int Fetched { get; set; }
    public int Parsed { get; set; }
    public int Invalid { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public int Missing { get; set; }
    public int DurationMs { get; set; }
    public string? Error { get; set; }
    /// <summary>เหตุผลตอน status='skipped' — รอบที่ถูกข้ามต้องบอกได้เสมอว่าข้ามเพราะอะไร</summary>
    public string? Reason { get; set; }
    /// <summary>เวลาที่รอบนี้จบ (ISO) — มีเมื่ออ่านจาก status.last</summary>
    public string? At { get; set; }

    /// <summary>ผลแบบข้าม</summary>
    public static NisSyncResultDto Skipped(string reason) => new() { Status = "skipped", Reason = reason };
}

/// <summary>สถานะการถอยจังหวะ (sync-backoff.ts)</summary>
public sealed class NisSyncBackoffDto
{
    public int EveryMinNow { get; set; }
    public int MaxMin { get; set; }
    /// <summary>window | backoff | null</summary>
    public string? BlockedBy { get; set; }
    public string? NextAttemptAt { get; set; }
    public int SkippedRounds { get; set; }
}

/// <summary>สถานะ runtime ของ sync ต่อบริษัท (in-memory ของ NisArchiveSyncService)</summary>
public sealed class NisSyncRuntimeDto
{
    public bool Running { get; set; }
    public int FailStreak { get; set; }
    public int EveryMin { get; set; }
    public NisSyncBackoffDto Backoff { get; set; } = new();
    /// <summary>all | biz</summary>
    public string Window { get; set; } = "all";
    public NisSyncResultDto? Last { get; set; }
}

/// <summary>ตัวเลขของ archive ต่อบริษัท</summary>
public sealed class NisSyncArchiveDto
{
    public int Cases { get; set; }
    public int TestCases { get; set; }
    public int Customers { get; set; }
    public string? OldestCase { get; set; }
    public string? NewestCase { get; set; }
}

/// <summary>แถว NisSyncLog ที่ส่งออก</summary>
public sealed class NisSyncLogDto
{
    public Guid Id { get; set; }
    public string StartedAt { get; set; } = string.Empty;
    public string? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public int? FetchedCount { get; set; }
    public int? ParsedCount { get; set; }
    public int? InvalidCount { get; set; }
    public int? InsertedCount { get; set; }
    public int? UpdatedCount { get; set; }
    public int? UnchangedCount { get; set; }
    public int? MissingCount { get; set; }
    public int? DurationMs { get; set; }
    public string? Error { get; set; }

    /// <summary>แปลงจาก entity</summary>
    public static NisSyncLogDto From(NisSyncLog l) => new()
    {
        Id = l.Id,
        StartedAt = NisServiceCaseDto.Iso(l.StartedAt),
        FinishedAt = NisServiceCaseDto.Iso(l.FinishedAt),
        Status = l.Status,
        Trigger = l.Trigger,
        FetchedCount = l.FetchedCount,
        ParsedCount = l.ParsedCount,
        InvalidCount = l.InvalidCount,
        InsertedCount = l.InsertedCount,
        UpdatedCount = l.UpdatedCount,
        UnchangedCount = l.UnchangedCount,
        MissingCount = l.MissingCount,
        DurationMs = l.DurationMs,
        Error = l.Error,
    };
}

/// <summary>GET sync/status</summary>
public sealed class NisSyncStatusDto
{
    public NisSyncArchiveDto Archive { get; set; } = new();
    public NisSyncRuntimeDto Sync { get; set; } = new();
    public List<NisSyncLogDto> RecentRuns { get; set; } = new();
}

/// <summary>ความสดของ archive — ให้หน้าจอบอกได้ว่า "ทำไมตราเวลาไม่ขยับ"</summary>
public sealed class NisFreshnessDto
{
    /// <summary>เวลาที่ข้อมูลครบถึง (รอบ ok ล่าสุดที่ invalid=0 และ missing% ต่ำกว่าเกณฑ์) · null = ยังไม่มีรอบที่ครบ</summary>
    public DateTimeOffset? DataAsOf { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
    public string? IncompleteReason { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 5. กลุ่มลูกค้า
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>ผล resolve กลุ่มของรหัสลูกค้าหนึ่งรหัส (GET customer-groups/{customerId}/group)</summary>
public sealed class NisCustomerGroupDto
{
    /// <summary>รหัสที่ถาม</summary>
    public string CustomerId { get; set; } = string.Empty;
    /// <summary>รหัสหลักของกลุ่ม — ทุกช่องระดับเอกสารมาจากรหัสนี้เท่านั้น</summary>
    public string PrimaryCustomerId { get; set; } = string.Empty;
    /// <summary>ทุกรหัสในกลุ่ม (รวมรหัสหลัก) — ตัวเลข/รายการเคสต้องรวมทั้งหมดนี้</summary>
    public List<string> MemberCustomerIds { get; set; } = new();
    /// <summary>true = รหัสที่ถามเป็นรหัสรองที่ถูกรวมเข้ารหัสอื่น</summary>
    public bool IsSecondary { get; set; }
    /// <summary>ธงบันทึกย้อนหลังของกลุ่ม (อ่านที่แถวของรหัสหลัก) — ตัวเลขระยะเวลาของทั้งกลุ่มแสดงเป็น null</summary>
    public bool Backdated { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// 6. Exception ที่ controller แปลงเป็น status code
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>ข้อมูลที่ส่งมาไม่ถูกต้อง → 400</summary>
public sealed class NisArchiveBadRequestException : Exception
{
    public NisArchiveBadRequestException(string message) : base(message) { }
}

/// <summary>ไม่พบข้อมูล → 404</summary>
public sealed class NisArchiveNotFoundException : Exception
{
    public NisArchiveNotFoundException(string message) : base(message) { }
}
