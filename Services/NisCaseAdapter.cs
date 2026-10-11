using System.Globalization;
using System.Text.RegularExpressions;
using goalongapi.Helpers;
using goalongapi.Models;
using goalongapi.Models.Nis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace goalongapi.Services;

/// <summary>
/// ชั้นเดียวในระบบที่ผูกกับรูปแบบข้อมูลของต้นทาง (STProblem จาก SP getProblem*) → <see cref="NisServiceCase"/>
/// port จาก apps/api/src/nis/nis-case.adapter.ts (เขียนจากข้อมูลจริง 1,819 record · docs/04-DATA-FINDINGS.md)
///
/// ถ้าต้นทางเปลี่ยน field → แก้ที่ไฟล์นี้ไฟล์เดียว · กฎทุกข้อ expose เป็น public static เพื่อให้ทดสอบได้ตรง ๆ
/// · RawJson = STProblem ทั้งก้อน (camelCase เหมือน response ของ gettaskservice) — ตีความผิดภายหลังย้อนคำนวณได้
/// </summary>
public static class NisCaseAdapter
{
    /// <summary>ค่า priority ที่รู้จัก (key = ค่าต้นทาง lower-case) · ค่าอื่น → minor + log</summary>
    public static readonly IReadOnlyDictionary<string, string> PriorityMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["critical"] = "critical",
        ["major"] = "major",
        ["medium"] = "medium",
        ["minor"] = "minor",
    };

    /// <summary>ค่า status ที่รู้จัก · ค่าอื่น → new + log</summary>
    public static readonly IReadOnlyDictionary<string, string> StatusMap = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["new"] = "new",
        ["inprogress"] = "inprogress",
        ["finish"] = "finish",
    };

    /// <summary>ชื่อลูกค้า/หัวข้อที่เข้าข่ายข้อมูลทดสอบ — กรองที่ชั้นนี้ชั้นเดียว (CLAUDE.md ข้อ 14)</summary>
    public static readonly Regex[] TestPatterns =
    {
        new(@"สำหรับ\s*TEST", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^test\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"test\s*time", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    };

    /// <summary>'01/01/1900' = ค่าว่างของระบบต้นทาง ไม่ใช่วันที่จริง</summary>
    private static readonly string[] NullDateMarkers = { "01/01/1900", "1900-01-01", "0001-01-01", "1/1/1900" };

    // ISO "2026-08-19" (+ เวลาแยก field) · US "08/19/2026 15:45:21" — สองรูปปนกันในชุดเดียว
    // ยอมรับเลข 1–2 หลักและ AM/PM เพิ่มจาก TS เพราะค่ามาจาก DataRow.ToString() ซึ่งขึ้นกับ culture ของ container
    private static readonly Regex IsoRegex = new(@"^(\d{4})-(\d{1,2})-(\d{1,2})(?:[ T](\d{1,2}):(\d{2})(?::(\d{2}))?)?", RegexOptions.Compiled);
    private static readonly Regex UsRegex = new(@"^(\d{1,2})/(\d{1,2})/(\d{4})(?:\s+(\d{1,2}):(\d{2})(?::(\d{2}))?\s*([AaPp][Mm])?)?", RegexOptions.Compiled);
    private static readonly Regex TimeRegex = new(@"^(\d{1,2}):(\d{2})(?::(\d{2}))?\s*([AaPp][Mm])?", RegexOptions.Compiled);

    private static readonly JsonSerializerSettings RawJsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Include,
        Formatting = Formatting.None,
    };

    /// <summary>
    /// แปลงวันที่จากต้นทางเป็น DateTimeOffset ที่ +07:00 (ต้นทางไม่มี timezone → ถือเป็นเวลาไทย)
    /// · ปี ≥ 2400 ถือว่าเป็น พ.ศ. → ลบ 543 (ข้อมูลชุดนี้ยืนยันแล้วว่าเป็น ค.ศ. แต่กันไว้)
    /// · 1900-01-01 / 0001-01-01 = ค่าว่าง → null · รูปที่อ่านไม่ออกหรือวันที่ไม่มีจริง → null
    /// </summary>
    /// <param name="dateStr">"2026-08-19" หรือ "08/19/2026 15:45:21"</param>
    /// <param name="timeStr">เวลาที่ส่งมาแยก field เช่น ReceiveTime "15:43:21" — ทับเวลาใน dateStr</param>
    public static DateTimeOffset? ParseNisDate(string? dateStr, string? timeStr = null)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        var raw = dateStr.Trim();
        if (NullDateMarkers.Any(m => raw.StartsWith(m, StringComparison.Ordinal))) return null;

        int y, mo, d, h = 0, mi = 0, s = 0;
        var iso = IsoRegex.Match(raw);
        var us = iso.Success ? null : UsRegex.Match(raw);

        if (iso.Success)
        {
            y = Int(iso.Groups[1]); mo = Int(iso.Groups[2]); d = Int(iso.Groups[3]);
            if (iso.Groups[4].Success) { h = Int(iso.Groups[4]); mi = Int(iso.Groups[5]); s = Int(iso.Groups[6]); }
        }
        else if (us is { Success: true })
        {
            mo = Int(us.Groups[1]); d = Int(us.Groups[2]); y = Int(us.Groups[3]); // MM/DD/YYYY
            if (us.Groups[4].Success)
            {
                h = Int(us.Groups[4]); mi = Int(us.Groups[5]); s = Int(us.Groups[6]);
                h = ApplyMeridiem(h, us.Groups[7]);
            }
        }
        else
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(timeStr))
        {
            var t = TimeRegex.Match(timeStr.Trim());
            if (t.Success)
            {
                h = Int(t.Groups[1]); mi = Int(t.Groups[2]); s = Int(t.Groups[3]);
                h = ApplyMeridiem(h, t.Groups[4]);
            }
        }

        if (y >= 2400) y -= 543;
        if (y < 1 || y > 9999) return null;

        try
        {
            return new DateTimeOffset(y, mo, d, h, mi, s, NisBangkokTime.Offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null; // วันที่ไม่มีจริง (เช่น 2026-02-31) — ปฏิเสธ ไม่ปัดไปวันถัดไป
        }
    }

    private static int Int(Group g) => g.Success && g.Value.Length > 0 ? int.Parse(g.Value, CultureInfo.InvariantCulture) : 0;

    private static int ApplyMeridiem(int hour, Group meridiem)
    {
        if (!meridiem.Success || meridiem.Value.Length == 0) return hour;
        var pm = meridiem.Value.Equals("PM", StringComparison.OrdinalIgnoreCase);
        if (pm && hour < 12) return hour + 12;
        if (!pm && hour == 12) return 0;
        return hour;
    }

    /// <summary>ค่า 9 และ 0 คือ "ยังไม่ได้ให้คะแนน" ไม่ใช่คะแนนจริง — รับเฉพาะ 1–5</summary>
    public static int? ParseRating(int? value) => value is >= 1 and <= 5 ? value : null;

    /// <summary>true เมื่อชื่อลูกค้าหรือหัวข้อเข้าข่าย TEST_PATTERNS</summary>
    public static bool IsTest(string? customerName, string? subject) =>
        TestPatterns.Any(p => p.IsMatch(customerName ?? string.Empty) || p.IsMatch(subject ?? string.Empty));

    /// <summary>"" · ช่องว่าง · "-" ถือว่าว่างเท่ากัน → null · อื่น ๆ trim</summary>
    public static string? Clean(string? value)
    {
        if (value is null) return null;
        var t = value.Trim();
        return t.Length == 0 || t == "-" ? null : t;
    }

    /// <summary>เวลานาฬิกา (แจ้ง→ปิด) เป็นชั่วโมง · null เมื่อยังไม่ปิดหรือปิดก่อนแจ้ง</summary>
    public static double? ResolutionHours(DateTimeOffset createdAt, DateTimeOffset? resolvedAt) =>
        resolvedAt.HasValue && resolvedAt.Value >= createdAt
            ? (resolvedAt.Value - createdAt).TotalMilliseconds / 3_600_000d
            : null;

    /// <summary>map enum — ค่าที่ map ไม่ได้ต้อง log ผ่าน onUnknown ไม่ใช่กลืนเงียบ</summary>
    private static string MapEnum(string? value, IReadOnlyDictionary<string, string> table, string fallback, string field, Action<string, string>? onUnknown)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (table.TryGetValue(key, out var hit)) return hit;
        if (key.Length > 0) onUnknown?.Invoke(field, value!);
        return fallback;
    }

    /// <summary>
    /// แปลง STProblem หนึ่งตัวเป็น NisServiceCase
    /// </summary>
    /// <param name="r">แถวจากต้นทาง</param>
    /// <param name="cmpId">รหัสบริษัทที่ sync (tenant key — ใช้ค่านี้ ไม่ใช่ r.CmpId เพื่อให้ตรงกับที่ query มา)</param>
    /// <param name="onUnknown">เรียกเมื่อเจอค่า enum ที่ไม่รู้จัก (field, value)</param>
    /// <exception cref="InvalidOperationException">แปลง ReceiveDate ไม่ได้ — record นี้ใช้ไม่ได้</exception>
    public static NisServiceCase ToServiceCase(STProblem r, string cmpId, Action<string, string>? onUnknown = null)
    {
        if (string.IsNullOrWhiteSpace(r.ProblemId))
            throw new InvalidOperationException("record ไม่มี problemId");

        var createdAt = ParseNisDate(r.ReceiveDate, r.ReceiveTime)
            ?? throw new InvalidOperationException($"problemId {r.ProblemId}: แปลง receiveDate ไม่ได้ (\"{r.ReceiveDate}\")");

        var resolvedAt = ParseNisDate(r.action?.FinishDate, r.action?.FinishTime);
        var startedAt = ParseNisDate(r.StartDate, r.StartTime);

        var assignees = (r.assign ?? new List<STProblem_Assign>())
            .Where(a => !string.IsNullOrWhiteSpace(a.UserFullName))
            .ToList();
        var actionBy = (r.action?.ActionBy ?? new List<STServiceActions_Assign>())
            .Where(a => !string.IsNullOrWhiteSpace(a.UserFullName))
            .ToList();

        var comment = Clean(r.FeedbackDescription);
        // "auto close by system" ไม่ใช่ความเห็นจากลูกค้า
        if (string.Equals(comment, "auto close by system", StringComparison.OrdinalIgnoreCase)) comment = null;

        var customerName = (r.CustomerName ?? string.Empty).Trim();
        var subject = (r.ProblemDetails ?? string.Empty).Trim();
        var taskId = (r.TaskId ?? string.Empty).Trim();

        return new NisServiceCase
        {
            CmpId = cmpId,
            SourceId = r.ProblemId.Trim(),
            CaseNo = (r.TaskNo ?? string.Empty).Trim(),
            // unique index (CmpId, ExternalGuid) — ถ้าต้นทางไม่มี TaskId ใช้ค่าสำรองที่ไม่ชนกัน แทนที่จะให้ sync ล้มทั้งรอบ
            ExternalGuid = taskId.Length > 0 ? taskId : $"problem:{r.ProblemId.Trim()}",

            CustomerId = (r.CustomerCode ?? string.Empty).Trim(),
            CustomerName = customerName,
            BranchName = Clean(r.CustBranchName),
            Province = Clean(r.ProvinceId), // ⚠️ ต้นทางเก็บ "ชื่อ" จังหวัดในคอลัมน์นี้ ไม่ใช่ id

            ContactName = Clean(r.ContactName),
            RequestBy = Clean(r.RequestBy),
            RequestPhone = Clean(r.requestPhone),
            RequestEmail = Clean(r.requestEmail),
            RequestPosition = Clean(r.requestPosition),

            Subject = subject,
            Category = (r.ProblemType ?? string.Empty).Trim(),
            Priority = MapEnum(r.Priority, PriorityMap, "minor", "priority", onUnknown),
            Status = MapEnum(r.Status, StatusMap, "new", "status", onUnknown),
            Remark = Clean(r.Remark),

            CreatedAt = createdAt,
            StartedAt = startedAt,
            ResolvedAt = resolvedAt,
            ModifiedAt = ParseNisDate(r.ModifiedAt),
            ResolutionHours = ResolutionHours(createdAt, resolvedAt),

            AssigneeIdsJson = ToJsonArray(assignees.Select(a => a.UserId).Where(id => !string.IsNullOrWhiteSpace(id))!),
            AssigneeNamesJson = ToJsonArray(assignees.Select(a => a.UserFullName.Trim())),
            PrimaryAssignee = assignees.Count > 0 ? assignees[0].UserFullName.Trim() : null,

            ActionDetails = Clean(r.action?.ActionDetails),
            ActionByNamesJson = ToJsonArray(actionBy.Select(a => a.UserFullName.Trim())),
            // gettaskservice ส่ง action เป็น object เสมอ (ServiceType ค่าเริ่มต้น 0) — คงค่าเดิมเพื่อให้ตัวเลขตรงกับระบบเก่า
            ServiceTypeCode = r.action?.ServiceType,

            SatisfactionScore = ParseRating(r.FeedbackRating),
            SatisfactionComment = comment,

            ViaLine = Clean(r.UserLineId) != null,
            AnydeskId = Clean(r.AnydeskId),
            AttachmentCount = r.attachfile?.Count ?? 0,
            IsTestData = IsTest(customerName, subject),

            RawJson = JsonConvert.SerializeObject(r, RawJsonSettings),
            SyncedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// แปลงทั้ง list · record ที่พังจะถูกข้ามและรายงานกลับ ไม่ทำให้ sync ล้มทั้งชุด
    /// </summary>
    public static (List<NisServiceCase> Cases, List<(int Index, string Message)> Errors) ToServiceCases(
        IEnumerable<STProblem> input, string cmpId, Action<string, string>? onUnknown = null)
    {
        var cases = new List<NisServiceCase>();
        var errors = new List<(int Index, string Message)>();
        var index = 0;
        foreach (var raw in input)
        {
            try { cases.Add(ToServiceCase(raw, cmpId, onUnknown)); }
            catch (Exception e) { errors.Add((index, e.Message)); }
            index++;
        }
        return (cases, errors);
    }

    /// <summary>JSON array ของ string — Newtonsoft ไม่ escape อักขระไทย จึง LIKE ค้นชื่อในคอลัมน์ JSON ได้ตรง ๆ</summary>
    public static string ToJsonArray(IEnumerable<string> values) =>
        JsonConvert.SerializeObject(values.ToList(), Formatting.None);

    /// <summary>ค่าที่ต้องค้นใน *NamesJson เมื่อหาชื่อหนึ่งชื่อ — รูปเดียวกับที่ ToJsonArray เขียน (มีเครื่องหมายคำพูดครอบ)</summary>
    public static string JsonNeedle(string value) => JsonConvert.ToString(value);
}
