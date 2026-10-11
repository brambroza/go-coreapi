using System.Security.Claims;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Service Report — ตัวเลขทั้งหมดที่หน้าภาพรวม/ลูกค้า/เวลาที่ใช้/คู่มือ/ภาระงาน และรายงาน PDF ใช้
/// port จาก apps/api/src/{analytics,customers,effort,kb,workload}/*.controller.ts (MERGE_PLAN §1.4 E11–E15, E17–E19)
///
/// กติกาที่บังคับ**ที่ชั้นนี้** ไม่ปล่อยให้หน้าจอตีความเอง: จำนวนเคสแยกสองก้อนเสมอ · เวลาที่คืนเป็นเวลานาฬิกาต้องมีป้ายกำกับ ·
/// SLA มีสวิตช์ค่าเริ่มต้นปิด · เวลาทำการหักวันหยุดจากตาราง holiday
///
/// 🔴 ไม่มี endpoint ใดใน controller นี้ที่กรองด้วยชื่อเจ้าหน้าที่ได้ (query strict — ปฏิเสธ <c>assignee</c>/<c>dateField</c>) ·
///    endpoint เดียวที่คืนตัวเลขรายบุคคลคือ <c>workload</c> และต้องมี role claim "mng"
/// ทุก action รับ <c>cmpid</c> เป็น query (ตาม pattern NisController) · response เป็น <see cref="NisApiResponse{T}"/>
/// </summary>
[ApiController]
[Authorize]
[Route("api/nis/analytics")]
public class NisAnalyticsController : ControllerBase
{
    private readonly INisAnalyticsService _analytics;
    private readonly INisArchiveService _archive;
    private readonly NisCustomerGroupService _customers;
    private readonly NisEffortService _effort;
    private readonly NisKbService _kb;
    private readonly NisWorkloadService _workload;

    public NisAnalyticsController(
        INisAnalyticsService analytics,
        INisArchiveService archive,
        NisCustomerGroupService customers,
        NisEffortService effort,
        NisKbService kb,
        NisWorkloadService workload)
    {
        _analytics = analytics;
        _archive = archive;
        _customers = customers;
        _effort = effort;
        _kb = kb;
        _workload = workload;
    }

    // ── analytics (E11–E14) ──────────────────────────────────────────────

    /// <summary>
    /// GET api/nis/analytics/summary?cmpid=&amp;from=&amp;to=&amp;customerId=&amp;status=&amp;priority=&amp;category=&amp;includeTest=
    /// KPI ทั้งชุด — current/previous/mom · meta.total = จำนวนงานที่แจ้งเข้า (ไม่ใช่จำนวนรายการใน data)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] string? cmpid, [FromQuery] NisAnalyticsQuery query, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ValidateQuery(query) is { } invalid) return invalid;

        var data = await _analytics.SummaryAsync(cmpid!, query, ct);
        return Ok(NisApiResponse<NisSummaryDto>.Of(data, await _archive.DataAsOfAsync(cmpid!, ct), data.Current.Intake.Count, Meta(query, new()
        {
            ["totalMeans"] = "จำนวนงานที่แจ้งเข้าในช่วงที่ขอ (ไม่ใช่จำนวนรายการใน data)",
            ["countLabel"] = "แยกสองก้อน — intake = งานที่แจ้งเข้า · resolved = งานที่ปิดได้",
            ["holidayWarning"] = data.HolidayWarning,
        })));
    }

    /// <summary>GET api/nis/analytics/trend?cmpid=&amp;bucket=auto|day|week|month&amp;… — meta.bucket · meta.total = จำนวน bucket</summary>
    [HttpGet("trend")]
    public async Task<IActionResult> Trend([FromQuery] string? cmpid, [FromQuery] NisAnalyticsQuery query, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ValidateQuery(query) is { } invalid) return invalid;

        var data = await _analytics.TrendAsync(cmpid!, query, ct);
        return Ok(NisApiResponse<NisTrendDto>.Of(data, await _archive.DataAsOfAsync(cmpid!, ct), data.Rows.Count, Meta(query, new()
        {
            ["bucket"] = data.Bucket,
            ["totalMeans"] = "จำนวนช่วงเวลา (bucket) ใน data.rows",
        })));
    }

    /// <summary>GET api/nis/analytics/breakdown?cmpid=&amp;by=priority|category|customer|status|duration&amp;… — meta.by · meta.total = จำนวนกลุ่ม</summary>
    [HttpGet("breakdown")]
    public async Task<IActionResult> Breakdown([FromQuery] string? cmpid, [FromQuery] NisAnalyticsQuery query, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ValidateQuery(query) is { } invalid) return invalid;

        var data = await _analytics.BreakdownAsync(cmpid!, query, ct);
        return Ok(NisApiResponse<NisBreakdownDto>.Of(data, await _archive.DataAsOfAsync(cmpid!, ct), data.RowCount, Meta(query, new()
        {
            ["by"] = data.By,
            ["totalMeans"] = "จำนวนกลุ่มใน data.rows",
        })));
    }

    /// <summary>GET api/nis/analytics/heatmap?cmpid=&amp;… — ตาราง 7 วัน × 24 ชั่วโมง · meta.total = 168 เสมอ</summary>
    [HttpGet("heatmap")]
    public async Task<IActionResult> Heatmap([FromQuery] string? cmpid, [FromQuery] NisAnalyticsQuery query, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ValidateQuery(query) is { } invalid) return invalid;

        var data = await _analytics.HeatmapAsync(cmpid!, query, ct);
        return Ok(NisApiResponse<NisHeatmapDto>.Of(data, await _archive.DataAsOfAsync(cmpid!, ct), 7 * 24, Meta(query, new()
        {
            ["holidayWarning"] = data.HolidayWarning,
            ["totalMeans"] = "จำนวนช่องในตาราง 7 วัน × 24 ชั่วโมง (คงที่เสมอ)",
        })));
    }

    // ── customers overview (E15) ─────────────────────────────────────────

    /// <summary>GET api/nis/analytics/customers/overview?cmpid= — หนึ่งแถวต่อกลุ่มลูกค้า · meta: slaEnabled · yearBe (ปีที่ใช้คำนวณ "ตั้งแต่ ม.ค.")</summary>
    [HttpGet("customers/overview")]
    public async Task<IActionResult> CustomersOverview([FromQuery] string? cmpid, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;

        var (rows, yearAd) = await _customers.OverviewAsync(cmpid!, ct);
        return Ok(NisApiResponse<List<NisCustomerOverviewRowDto>>.Of(rows, await _archive.DataAsOfAsync(cmpid!, ct), rows.Count, new Dictionary<string, object?>
        {
            ["slaEnabled"] = _analytics.SlaEnabled,
            ["yearBe"] = yearAd + 543,
        }));
    }

    // ── effort (E17) ─────────────────────────────────────────────────────

    /// <summary>
    /// GET api/nis/analytics/effort?cmpid=&amp;from=&amp;to= — เวลาที่ใช้ต่อกลุ่มลูกค้า (คัดด้วย resolvedAt)
    /// meta: resolvedCount · totalHours · medianHours · customerCount · excludedBackdatedCases · trend · categories · distribution ·
    /// trustedCasesWithoutHours · hoursKind · 400 เมื่อวันที่อ่านไม่ได้หรือ from อยู่หลัง to
    /// </summary>
    [HttpGet("effort")]
    public async Task<IActionResult> Effort([FromQuery] string? cmpid, [FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ParseRange(from, to, out var fromD, out var toD) is { } invalid) return invalid;

        var ov = await _effort.OverviewAsync(cmpid!, fromD, toD, ct);
        return Ok(NisApiResponse<List<NisEffortRowDto>>.Of(ov.Rows, await _archive.DataAsOfAsync(cmpid!, ct), ov.Rows.Count, ov.ToMeta()));
    }

    // ── kb (E18) ─────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/nis/analytics/kb?cmpid=&amp;from=&amp;to= — หัวข้อที่แจ้งซ้ำ top 40 (คัดด้วย createdAt)
    /// meta: intakeCount · repeatedTopics · detailedCount · detailedChars · cumulativePct
    /// </summary>
    [HttpGet("kb")]
    public async Task<IActionResult> Kb([FromQuery] string? cmpid, [FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (ParseRange(from, to, out var fromD, out var toD) is { } invalid) return invalid;

        var ov = await _kb.OverviewAsync(cmpid!, fromD, toD, ct);
        return Ok(NisApiResponse<List<NisKbTopicRowDto>>.Of(ov.Topics, await _archive.DataAsOfAsync(cmpid!, ct), ov.Topics.Count, ov.ToMeta()));
    }

    // ── workload (E19) — เฉพาะผู้จัดการ ─────────────────────────────────

    /// <summary>
    /// GET api/nis/analytics/workload?cmpid=&amp;period=month|quarter|year — ตัวเลขรายบุคคล · **ต้องมี role claim = mng** ไม่งั้น 403
    /// meta: yearBe · teamResolvedInPeriod · period · periodLabel · periodFrom · periodTo · 400 เมื่อ period ไม่รู้จัก (ไม่ตกกลับเป็นค่าเริ่มต้นเงียบ ๆ)
    /// </summary>
    [HttpGet("workload")]
    public async Task<IActionResult> Workload([FromQuery] string? cmpid, [FromQuery] string? period, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (!IsManager()) return StatusCode(StatusCodes.Status403Forbidden, new { message = "เฉพาะผู้จัดการ" });

        // ไม่ส่งมาเลย = month · ส่งมาแต่ไม่รู้จัก (รวมค่าว่าง) = 400 ไม่ตกกลับเงียบ ๆ (ตรงกับ `periodRaw ?? 'month'` ของ TS)
        var p = period ?? "month";
        if (!NisAnalyticsText.WorkloadPeriodLabel.ContainsKey(p))
            return BadRequest(new { message = $"ช่วงเวลา \"{period}\" ไม่รู้จัก — รับได้เฉพาะ {string.Join(" · ", NisAnalyticsText.WorkloadPeriodLabel.Keys)}" });

        var ov = await _workload.OverviewAsync(cmpid!, p, ct);
        return Ok(NisApiResponse<List<NisWorkloadRowDto>>.Of(ov.Rows, await _archive.DataAsOfAsync(cmpid!, ct), ov.Rows.Count, ov.ToMeta()));
    }

    // ── helpers ──────────────────────────────────────────────────────────

    /// <summary>cmpid ต้องมี (tenant key) — ไม่มีคืน 400</summary>
    private IActionResult? RequireCmp(string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid) || cmpid.Length > 50)
            return BadRequest(new { message = "ต้องระบุ cmpid" });
        return null;
    }

    /// <summary>
    /// strict query — คีย์ที่ไม่รู้จัก (รวม assignee/dateField) ต้องถูกปฏิเสธ ไม่ใช่กลืนเงียบ · สะท้อนกลับแค่ชื่อคีย์ ไม่ใช่ค่า
    /// แล้วจึงตรวจค่าของตัวกรอง (รูปวันที่ · ช่วงกลับหัว · enum)
    /// </summary>
    private IActionResult? ValidateQuery(NisAnalyticsQuery query)
    {
        var issues = NisAnalyticsQuery.UnknownKeys(Request.Query.Keys)
            .Select(k => $"{k}: {NisAnalyticsQuery.UnknownKeyMessage}")
            .ToList();
        issues.AddRange(query.Validate());
        return issues.Count > 0 ? BadRequest(new { message = "พารามิเตอร์ไม่ถูกต้อง", issues }) : null;
    }

    /// <summary>meta ชุดเดียวกันทุก endpoint analytics — filters · slaEnabled · unavailable + extra ของแต่ละ endpoint</summary>
    private Dictionary<string, object?> Meta(NisAnalyticsQuery q, Dictionary<string, object?> extra)
    {
        var meta = new Dictionary<string, object?>
        {
            ["filters"] = new Dictionary<string, object?>
            {
                ["from"] = q.From,
                ["to"] = q.To,
                ["customerId"] = q.CustomerId,
                ["priority"] = q.Priority,
                ["category"] = q.Category,
                ["status"] = q.Status,
                ["includeTest"] = q.IncludeTest,
            },
            ["slaEnabled"] = _analytics.SlaEnabled,
            // KPI ที่ทำไม่ได้จากข้อมูลจริง — บอกเหตุผล ไม่เงียบและไม่คืน 0
            ["unavailable"] = NisAnalyticsText.UnavailableMetrics,
        };
        foreach (var kv in extra) meta[kv.Key] = kv.Value;
        return meta;
    }

    /// <summary>
    /// parse from/to เป็นเวลาไทย (วันที่ล้วน = ต้นวัน/ท้ายวัน) — ค่าที่แปลงไม่ได้ต้องปฏิเสธ ไม่ใช่กลืนเป็น "ไม่กรอง"
    /// (ถ้ากลืน คนเรียกจะส่งวันที่ผิดรูปแล้วเห็นตัวเลขทั้งคลังโดยนึกว่าเป็นของช่วงที่ขอ)
    /// </summary>
    private IActionResult? ParseRange(string? from, string? to, out DateTimeOffset? fromD, out DateTimeOffset? toD)
    {
        fromD = null;
        toD = null;
        if (!string.IsNullOrEmpty(from))
        {
            fromD = NisBusinessHours.ParseThaiRangeBoundary(from, endEdge: false);
            if (fromD is null) return BadRequest(new { message = $"from ไม่ใช่วันที่ที่อ่านได้: \"{from}\"" });
        }
        if (!string.IsNullOrEmpty(to))
        {
            toD = NisBusinessHours.ParseThaiRangeBoundary(to, endEdge: true);
            if (toD is null) return BadRequest(new { message = $"to ไม่ใช่วันที่ที่อ่านได้: \"{to}\"" });
        }
        if (fromD is not null && toD is not null && fromD > toD) return BadRequest(new { message = "from ต้องไม่อยู่หลัง to" });
        return null;
    }

    /// <summary>
    /// role claim จาก JWT — AccountService ใส่เป็น claim type "role" ค่า "mng" | "staff" · JwtBearer อาจ map เป็น ClaimTypes.Role จึงอ่านทั้งสองชื่อ
    /// </summary>
    private bool IsManager()
    {
        var role = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
        return string.Equals(role, "mng", StringComparison.OrdinalIgnoreCase);
    }
}
