using System.Security.Claims;
using System.Text.Json;
using goalongapi.Dtos.Nis;
using goalongapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Service Report — archive ของเคส · วันหยุด · sync · กลุ่มลูกค้า (อ่าน)
/// port จาก apps/api/src/cases/cases.controller.ts + sync.controller.ts (MERGE_PLAN §1.4 E2–E10, E16 ส่วน resolve)
///
/// ทุก action รับ <c>cmpid</c> เป็น query (ตาม pattern NisController) · response เป็น <see cref="NisApiResponse{T}"/>
/// ยกเว้น POST sync/run ที่คืนผลดิบเหมือนระบบเดิม · ไม่แตะ NisController.cs
/// </summary>
[ApiController]
[Authorize]
[Route("api/nis/archive")]
public class NisArchiveController : ControllerBase
{
    private readonly INisArchiveService _archive;
    private readonly NisHolidayService _holidays;
    private readonly NisArchiveSyncService _sync;
    private readonly NisCustomerGroupResolverService _groups;
    private readonly NisCustomerGroupService _groupSettings;

    public NisArchiveController(
        INisArchiveService archive,
        NisHolidayService holidays,
        NisArchiveSyncService sync,
        NisCustomerGroupResolverService groups,
        NisCustomerGroupService groupSettings)
    {
        _archive = archive;
        _holidays = holidays;
        _sync = sync;
        _groups = groups;
        _groupSettings = groupSettings;
    }

    // ── cases ────────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/nis/archive/cases?cmpid=&amp;from=&amp;to=&amp;dateField=&amp;customerId=&amp;assignee=&amp;status=&amp;priority=&amp;category=&amp;q=
    /// &amp;includeTest=&amp;unresolved=&amp;maxSatisfaction=&amp;sort=&amp;order=&amp;page=&amp;pageSize=
    /// รายการเคส + ตัวเลขสรุปของทั้งชุดที่กรอง (unresolvedCount · shortSolutionCount) มากับคำตอบเดียวกัน
    /// </summary>
    [HttpGet("cases")]
    public async Task<IActionResult> ListCases([FromQuery] string? cmpid, [FromQuery] NisCaseQuery query, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        var issues = query.Validate();
        if (issues.Count > 0) return BadRequest(new { message = "พารามิเตอร์ไม่ถูกต้อง", issues });

        var result = await _archive.ListCasesAsync(cmpid!, query, ct);
        var dataAsOf = await _archive.DataAsOfAsync(cmpid!, ct);
        return Ok(NisApiResponse<List<NisServiceCaseDto>>.Of(result.Rows, dataAsOf, result.Total, result.ToMeta()));
    }

    /// <summary>GET api/nis/archive/cases/{id}?cmpid= — id = SourceId (problemId) · CaseNo · ExternalGuid (taskId)</summary>
    [HttpGet("cases/{id}")]
    public async Task<IActionResult> GetCase([FromQuery] string? cmpid, string id, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (string.IsNullOrWhiteSpace(id) || id.Length > 100) return BadRequest(new { message = "id ไม่ถูกต้อง" });

        var row = await _archive.GetCaseAsync(cmpid!, id.Trim(), ct);
        if (row is null) return NotFound(new { message = $"ไม่พบเคส \"{id}\"" });
        var dataAsOf = await _archive.DataAsOfAsync(cmpid!, ct);
        return Ok(NisApiResponse<NisServiceCaseDto>.Of(row, dataAsOf, 1));
    }

    /// <summary>GET api/nis/archive/customers?cmpid= — รายชื่อลูกค้า + intakeAllTime (แจ้งเข้าทั้งคลัง ไม่รวมข้อมูลทดสอบ)</summary>
    [HttpGet("customers")]
    public async Task<IActionResult> ListCustomers([FromQuery] string? cmpid, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        var rows = await _archive.ListCustomersAsync(cmpid!, ct);
        var dataAsOf = await _archive.DataAsOfAsync(cmpid!, ct);
        return Ok(NisApiResponse<List<NisCustomerSummaryDto>>.Of(rows, dataAsOf, rows.Count, new Dictionary<string, object?>
        {
            ["countLabel"] = "งานที่แจ้งเข้า (ทั้งคลัง ไม่จำกัดช่วงเวลา)",
        }));
    }

    // ── holidays ─────────────────────────────────────────────────────────

    /// <summary>GET api/nis/archive/holidays?cmpid=&amp;year= — meta: observed · unverified · company · warning · numbersBlocked</summary>
    [HttpGet("holidays")]
    public async Task<IActionResult> ListHolidays([FromQuery] string? cmpid, [FromQuery] int? year, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (year is < 1900 or > 2600) return BadRequest(new { message = $"ปีต้องเป็น ค.ศ. ที่สมเหตุสมผล (ได้: {year})" });

        var (rows, meta) = await _holidays.ListAsync(cmpid!, year, ct);
        return Ok(NisApiResponse<List<NisHolidayDto>>.Of(rows, null, rows.Count, meta));
    }

    /// <summary>
    /// GET api/nis/archive/holidays/changes?cmpid=&amp;limit=&amp;changedBy= — ประวัติการแก้ (append-only)
    /// meta.total = ตามตัวกรอง · meta.earliestAt = แถวเก่าสุดทั้งบริษัท (ไม่ผูกกับตัวกรอง)
    /// </summary>
    [HttpGet("holidays/changes")]
    public async Task<IActionResult> HolidayChanges([FromQuery] string? cmpid, [FromQuery] int? limit, [FromQuery] string? changedBy, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (changedBy is { Length: > 100 }) return BadRequest(new { message = "changedBy ยาวเกิน 100 ตัวอักษร" });

        var (rows, total, meta) = await _holidays.ChangesAsync(cmpid!, limit, changedBy, ct);
        return Ok(NisApiResponse<List<NisHolidayChangeDto>>.Of(rows, null, total, meta));
    }

    /// <summary>POST api/nis/archive/holidays?cmpid= — body {date, nameTh, kind?, verified?, observed?, note?} (strict) · 400 ถ้ามีวันนี้แล้ว</summary>
    [HttpPost("holidays")]
    public async Task<IActionResult> CreateHoliday([FromQuery] string? cmpid, [FromBody] JsonElement body, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (!NisHolidayCreateDto.TryParse(body, out var dto, out var issues))
            return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        try
        {
            var row = await _holidays.CreateAsync(cmpid!, dto, CurrentUsername(), ct);
            return Ok(NisApiResponse<NisHolidayDto>.Of(row, null, 1));
        }
        catch (NisArchiveBadRequestException e) { return BadRequest(new { message = e.Message }); }
    }

    /// <summary>PUT api/nis/archive/holidays/{date}?cmpid= — body {nameTh?, kind?, verified?, observed?, note?} (strict) · field ที่ไม่ส่ง = ไม่แตะ</summary>
    [HttpPut("holidays/{date}")]
    public async Task<IActionResult> UpdateHoliday([FromQuery] string? cmpid, string date, [FromBody] JsonElement body, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (!NisHolidayJson.IsHolidayDate(date))
            return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues = new[] { "date: ต้องเป็นรูปแบบ 'YYYY-MM-DD'" } });
        if (!NisHolidayUpdateDto.TryParse(body, out var dto, out var issues))
            return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        try
        {
            var row = await _holidays.UpdateAsync(cmpid!, date, dto, CurrentUsername(), ct);
            return Ok(NisApiResponse<NisHolidayDto>.Of(row, null, 1));
        }
        catch (NisArchiveBadRequestException e) { return BadRequest(new { message = e.Message }); }
        catch (NisArchiveNotFoundException e) { return NotFound(new { message = e.Message }); }
    }

    // ── sync ─────────────────────────────────────────────────────────────

    /// <summary>
    /// POST api/nis/archive/sync/run?cmpid= — sync เดี๋ยวนี้ (trigger=manual · ไม่ผ่านด่านถอยจังหวะ/หน้าต่างเวลา)
    /// คืนผลดิบ {status, fetched, parsed, invalid, inserted, updated, unchanged, missing, durationMs, error?, reason?}
    /// status=skipped เมื่อรอบก่อนยังไม่จบ · lock ถูก instance อื่นถือ · ยังไม่ตั้งค่า SyncUsername
    /// </summary>
    [HttpPost("sync/run")]
    public async Task<IActionResult> RunSync([FromQuery] string? cmpid, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        var result = await _sync.RunOnceAsync(cmpid!, "manual", ct);
        return Ok(result);
    }

    /// <summary>GET api/nis/archive/sync/status?cmpid= — {archive, sync, recentRuns} · meta.dataAsOf = เวลาที่ข้อมูลครบถึง</summary>
    [HttpGet("sync/status")]
    public async Task<IActionResult> SyncStatus([FromQuery] string? cmpid, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        var status = await _archive.SyncStatusAsync(cmpid!, ct);
        var fresh = await _archive.FreshnessAsync(cmpid!, ct);
        return Ok(NisApiResponse<NisSyncStatusDto>.Of(status, fresh.DataAsOf, null, new Dictionary<string, object?>
        {
            ["lastRunAt"] = fresh.LastRunAt,
            ["lastRunStatus"] = fresh.LastRunStatus,
            ["incompleteReason"] = fresh.IncompleteReason,
        }));
    }

    // ── customer groups (อ่าน) ───────────────────────────────────────────

    /// <summary>GET api/nis/archive/customer-groups/{customerId}/group?cmpid= — resolve รหัสหลัก/สมาชิก/ธง backdated</summary>
    [HttpGet("customer-groups/{customerId}/group")]
    public async Task<IActionResult> ResolveGroup([FromQuery] string? cmpid, string customerId, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (string.IsNullOrWhiteSpace(customerId) || customerId.Length > 50) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });

        var group = await _groups.ResolveGroupAsync(cmpid!, customerId.Trim(), ct);
        return Ok(NisApiResponse<NisCustomerGroupDto>.Of(group, null, 1));
    }

    // ── customer groups (ตั้งค่า — E16) ──────────────────────────────────

    /// <summary>GET api/nis/archive/customer-groups/{customerId}?cmpid= — การตั้งค่า {customerId, mergeIntoCustomerId, backdated} · ไม่มีแถว = ไม่รวม/ไม่ติดธง</summary>
    [HttpGet("customer-groups/{customerId}")]
    public async Task<IActionResult> ReadGroupSettings([FromQuery] string? cmpid, string customerId, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (string.IsNullOrWhiteSpace(customerId) || customerId.Length > 50) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });

        var s = await _groupSettings.ReadSettingsAsync(cmpid!, customerId.Trim(), ct);
        return Ok(NisApiResponse<NisCustomerSettingsDto>.Of(s, null, 1));
    }

    /// <summary>
    /// PUT api/nis/archive/customer-groups/{customerId}?cmpid= — body {mergeIntoCustomerId?: string|null, backdated?: bool} · ไม่ส่ง = ไม่แตะ
    /// 400 เมื่อผิดกฎกลุ่ม (รวมกับตัวเอง · ชี้ต่อเป็นทอด · รหัสหลักที่มีรหัสรองแล้ว · ธงที่รหัสรอง · ถือธงแล้วจะไปเป็นรหัสรอง) · updatedBy = username จาก JWT
    /// </summary>
    [HttpPut("customer-groups/{customerId}")]
    public async Task<IActionResult> SetGroupSettings([FromQuery] string? cmpid, string customerId, [FromBody] JsonElement body, CancellationToken ct)
    {
        if (RequireCmp(cmpid) is { } bad) return bad;
        if (string.IsNullOrWhiteSpace(customerId) || customerId.Length > 50) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });
        if (!NisSetCustomerSettingsDto.TryParse(body, out var dto, out var issues))
            return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        try
        {
            var s = await _groupSettings.SetSettingsAsync(cmpid!, customerId.Trim(), dto, CurrentUsername(), ct);
            return Ok(NisApiResponse<NisCustomerSettingsDto>.Of(s, null, 1));
        }
        catch (NisArchiveBadRequestException e) { return BadRequest(new { message = e.Message }); }
    }

    // ── helpers ──────────────────────────────────────────────────────────

    /// <summary>cmpid ต้องมี (tenant key) — ไม่มีคืน 400</summary>
    private IActionResult? RequireCmp(string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid) || cmpid.Length > 50)
            return BadRequest(new { message = "ต้องระบุ cmpid" });
        return null;
    }

    /// <summary>username จาก JWT (claim sub = Accounts.Username · ถูก map เป็น NameIdentifier ได้) — ลง ChangedBy</summary>
    private string CurrentUsername() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.Identity?.Name
        ?? "unknown";
}
