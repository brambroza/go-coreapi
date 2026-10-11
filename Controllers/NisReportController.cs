using System.Security.Claims;
using System.Text.Json;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Service Report — report builder: ข้อมูลบริษัท · template · asset (โลโก้/QR) · รายงาน · คิว PDF · หน้าพิมพ์
/// port จาก apps/api/src/reports/{reports.controller.ts,print.controller.ts} (MERGE_PLAN §1.4 E20, E22–E29)
///
/// ทุก action รับ <c>cmpid</c> เป็น query (ตาม pattern NisController) · response เป็น <see cref="NisApiResponse{T}"/>
/// ยกเว้น 2 endpoint ที่ <c>[AllowAnonymous]</c> โดยตั้งใจ: <c>GET reports/{id}/print-data?key=</c> (print key คือสิทธิ์ —
/// Playwright ไม่มี JWT) และ <c>GET assets/{name}</c> (ชื่อไฟล์เป็น sha256 ที่ระบบตั้ง เดาไม่ได้ · หน้าพิมพ์ใส่ใน img src)
/// </summary>
[ApiController]
[Authorize]
[Route("api/nis/report")]
public class NisReportController : ControllerBase
{
    private const long UploadLimitBytes = NisReportAssetStorage.MaxBytes + 1024;

    private readonly NisReportCompanyService _company;
    private readonly NisReportTemplateService _templates;
    private readonly NisReportService _reports;
    private readonly NisReportAssetStorage _assets;
    private readonly NisReportPdfQueueService _queue;
    private readonly INisArchiveService _archive;

    public NisReportController(
        NisReportCompanyService company,
        NisReportTemplateService templates,
        NisReportService reports,
        NisReportAssetStorage assets,
        NisReportPdfQueueService queue,
        INisArchiveService archive)
    {
        _company = company;
        _templates = templates;
        _reports = reports;
        _assets = assets;
        _queue = queue;
        _archive = archive;
    }

    // ── company (E20) ────────────────────────────────────────────────────

    /// <summary>GET api/nis/report/company?cmpid= — ข้อมูลบริษัท + gapsBeforeProduction (ยังไม่กรอก = ไม่พิมพ์ · ไม่บล็อก export)</summary>
    [HttpGet("company")]
    public Task<IActionResult> ReadCompany([FromQuery] string? cmpid, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var c = await _company.ReadWithGapsAsync(cmp, ct);
        return Ok(NisApiResponse<NisCompanyInfoResponseDto>.Of(c, null, 1));
    });

    /// <summary>PUT api/nis/report/company?cmpid= — body = patch (merge กับของเดิม) · ลูกค้าผูกเซลสองคน → 400</summary>
    [HttpPut("company")]
    public Task<IActionResult> WriteCompany([FromQuery] string? cmpid, [FromBody] JsonElement body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var c = await _company.WriteAsync(cmp, NisReportJson.ToToken(body), CurrentUsername(), ct);
        return Ok(NisApiResponse<NisCompanyInfoResponseDto>.Of(c, null, 1));
    });

    // ── templates (E22–E25) ──────────────────────────────────────────────

    /// <summary>GET api/nis/report/templates?cmpid=&amp;customerId=&amp;includeArchived= — default ก่อน · meta.dataAsOf</summary>
    [HttpGet("templates")]
    public Task<IActionResult> ListTemplates([FromQuery] string? cmpid, [FromQuery] string? customerId, [FromQuery] bool includeArchived, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        if (customerId is { Length: > 64 }) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });
        var rows = await _templates.ListAsync(cmp, customerId, includeArchived, ct);
        return Ok(NisApiResponse<List<NisReportTemplateDto>>.Of(rows, await _archive.DataAsOfAsync(cmp, ct), rows.Count));
    });

    /// <summary>GET api/nis/report/templates/{id}?cmpid=</summary>
    [HttpGet("templates/{id:guid}")]
    public Task<IActionResult> GetTemplate([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
        Ok(NisApiResponse<NisReportTemplateDto>.Of(await _templates.ByIdAsync(cmp, id, ct), null, 1)));

    /// <summary>POST api/nis/report/templates?cmpid= — body {customerId, name, layout, isDefault?} · 400 layout ผิด/รหัสรอง · 409 ชื่อซ้ำ</summary>
    [HttpPost("templates")]
    public Task<IActionResult> CreateTemplate([FromQuery] string? cmpid, [FromBody] NisCreateTemplateDto body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var issues = new List<string>();
        if (!NisReportSchema.CustomerIdRegex.IsMatch(body.CustomerId ?? "") || body.CustomerId!.Length > 64)
            issues.Add("customerId: รหัสลูกค้าใช้ได้เฉพาะ A-Z a-z 0-9 _ - (ค่านี้ถูกใช้เป็นชื่อไฟล์ PDF)");
        if (string.IsNullOrWhiteSpace(body.Name) || body.Name.Length > 120) issues.Add("name: ต้องยาว 1–120 ตัวอักษร");
        if (body.Layout is null || body.Layout.Value.ValueKind == JsonValueKind.Undefined) issues.Add("layout: ต้องระบุ");
        if (issues.Count > 0) return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        var row = await _templates.CreateAsync(cmp, body.CustomerId!, body.Name, NisReportJson.ToToken(body.Layout!.Value), body.IsDefault, CurrentUsername(), ct);
        return Ok(NisApiResponse<NisReportTemplateDto>.Of(row, null, 1));
    });

    /// <summary>POST api/nis/report/templates/starter?cmpid= — body {customerId, customerName, name, docKind=book, preset?, from, to} · เริ่มจากโครงสำเร็จรูป (ตั้งเป็น default)</summary>
    [HttpPost("templates/starter")]
    public Task<IActionResult> CreateStarter([FromQuery] string? cmpid, [FromBody] NisCreateStarterDto body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var issues = new List<string>();
        if (!NisReportSchema.CustomerIdRegex.IsMatch(body.CustomerId ?? "") || body.CustomerId!.Length > 64)
            issues.Add("customerId: รหัสลูกค้าใช้ได้เฉพาะ A-Z a-z 0-9 _ - (ค่านี้ถูกใช้เป็นชื่อไฟล์ PDF)");
        if (string.IsNullOrWhiteSpace(body.CustomerName)) issues.Add("customerName: ต้องไม่ว่าง");
        if (string.IsNullOrWhiteSpace(body.Name) || body.Name.Length > 120) issues.Add("name: ต้องยาว 1–120 ตัวอักษร");
        body.DocKind = string.IsNullOrWhiteSpace(body.DocKind) ? "book" : body.DocKind;
        if (!NisReportSchema.DocKinds.Contains(body.DocKind)) issues.Add("docKind: ต้องเป็น book หรือ brief");
        if (body.Preset != null && !NisReportSchema.StarterPresets.Contains(body.Preset)) issues.Add("preset: ต้องเป็น service");
        if (!NisReportSchema.IsThaiDate(body.From ?? "")) issues.Add("from: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");
        if (!NisReportSchema.IsThaiDate(body.To ?? "")) issues.Add("to: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");
        if (issues.Count == 0 && string.CompareOrdinal(body.From, body.To) > 0) issues.Add("from: วันเริ่มอยู่หลังวันสิ้นสุด");
        if (issues.Count > 0) return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        var row = await _templates.CreateFromStarterAsync(cmp, body, CurrentUsername(), ct);
        return Ok(NisApiResponse<NisReportTemplateDto>.Of(row, null, 1));
    });

    /// <summary>PUT api/nis/report/templates/{id}?cmpid= — body {name?, layout?, isDefault?} · version +1 เฉพาะ layout เปลี่ยน · 409 ถ้าเก็บเข้ากรุ</summary>
    [HttpPut("templates/{id:guid}")]
    public Task<IActionResult> UpdateTemplate([FromQuery] string? cmpid, Guid id, [FromBody] NisUpdateTemplateDto body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        if (body.Name != null && (body.Name.Trim().Length == 0 || body.Name.Length > 120))
            return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues = new[] { "name: ต้องยาว 1–120 ตัวอักษร" } });
        var row = await _templates.UpdateAsync(cmp, id, body, CurrentUsername(), ct);
        return Ok(NisApiResponse<NisReportTemplateDto>.Of(row, null, 1));
    });

    /// <summary>PATCH api/nis/report/templates/{id}/archive?cmpid= — body {archived} · "ลบ" = เก็บเข้ากรุ ไม่มี DELETE</summary>
    [HttpPatch("templates/{id:guid}/archive")]
    public Task<IActionResult> ArchiveTemplate([FromQuery] string? cmpid, Guid id, [FromBody] NisArchiveTemplateDto body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        if (body.Archived == null) return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues = new[] { "archived: ต้องเป็น true หรือ false" } });
        var row = await _templates.ArchiveAsync(cmp, id, body.Archived.Value, CurrentUsername(), ct);
        return Ok(NisApiResponse<NisReportTemplateDto>.Of(row, null, 1));
    });

    /// <summary>POST api/nis/report/templates/{id}/projection?cmpid= — สิ่งที่ layout จะดึง/เก็บ (Q25) + holidayGate + customerNameWarning</summary>
    [HttpPost("templates/{id:guid}/projection")]
    public Task<IActionResult> Projection([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
        Ok(NisApiResponse<NisProjectionDto>.Of(await _reports.ProjectionAsync(cmp, id, ct), null, 1)));

    /// <summary>POST api/nis/report/templates/{id}/preview?cmpid= — snapshot ตัวอย่าง + สถานะทุกด่าน + lastIssueNo · ไม่รับช่วงเวลาจาก body</summary>
    [HttpPost("templates/{id:guid}/preview")]
    public Task<IActionResult> Preview([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
        Ok(NisApiResponse<NisPreviewDto>.Of(await _reports.PreviewAsync(cmp, id, ct), await _archive.DataAsOfAsync(cmp, ct), 1)));

    // ── assets (E26) ─────────────────────────────────────────────────────

    /// <summary>POST api/nis/report/assets/customer-logo/{customerId}?cmpid= — body ดิบ ≤512 KB · header x-file-name · ตรวจไบต์จริง</summary>
    [HttpPost("assets/customer-logo/{customerId}")]
    public Task<IActionResult> UploadCustomerLogo([FromQuery] string? cmpid, string customerId, CancellationToken ct) => Guarded(cmpid, async _ =>
    {
        if (!NisReportSchema.CustomerIdRegex.IsMatch(customerId) || customerId.Length > 64) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });
        var bytes = await ReadRawBodyAsync(ct);
        if (bytes == null) return BadRequest(new { message = "ไฟล์เกิน 512 KB (ข้อ 4.1)" });
        var stored = await _assets.SaveAsync("customer-logo", customerId, bytes, FileNameHint("logo"), ct);
        return Ok(NisApiResponse<NisAssetUploadResultDto>.Of(stored, null, 1));
    });

    /// <summary>POST api/nis/report/assets/company-qr?cmpid=&amp;for=line|contact — QR LINE ของบริษัท (line) หรือของผู้ติดต่อฝ่ายขาย (contact)</summary>
    [HttpPost("assets/company-qr")]
    public Task<IActionResult> UploadCompanyQr([FromQuery] string? cmpid, [FromQuery(Name = "for")] string? forWhom, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var bytes = await ReadRawBodyAsync(ct);
        if (bytes == null) return BadRequest(new { message = "ไฟล์เกิน 512 KB (ข้อ 4.1)" });
        // ชื่อไฟล์ขึ้นต้นต่างกันให้ไล่ย้อนได้ว่าเป็นของอะไร · owner = cmpid เพราะ QR บริษัทเป็นของ tenant
        var kind = forWhom == "contact" ? "contact-qr" : "company-line";
        var stored = await _assets.SaveAsync(kind, cmp, bytes, FileNameHint("line-qr"), ct);
        return Ok(NisApiResponse<NisAssetUploadResultDto>.Of(stored, null, 1));
    });

    /// <summary>
    /// GET api/nis/report/assets/{name} — เสิร์ฟไฟล์ที่นำเข้าผ่านระบบ · ไม่ต้องมี JWT (หน้าพิมพ์ใส่ใน img src · ชื่อเป็น sha256 เดาไม่ได้)
    /// Cache-Control immutable · CSP default-src 'none' ซ้ำอีกชั้นเพราะไฟล์นี้ถูก Chromium เรนเดอร์ตอนออก PDF
    /// </summary>
    [HttpGet("assets/{name}")]
    [AllowAnonymous]
    public IActionResult ServeAsset(string name)
    {
        var f = _assets.Read(name);
        if (f == null) return NotFound(new { message = $"ไม่พบโลโก้ \"{name}\"" });
        Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(f.Value.Bytes, f.Value.Mime);
    }

    // ── reports (E27–E29) ────────────────────────────────────────────────

    /// <summary>GET api/nis/report/reports?cmpid=&amp;customerId= — 200 ฉบับล่าสุด · meta.dataAsOf</summary>
    [HttpGet("reports")]
    public Task<IActionResult> ListReports([FromQuery] string? cmpid, [FromQuery] string? customerId, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        if (customerId is { Length: > 64 }) return BadRequest(new { message = "customerId ไม่ถูกต้อง" });
        var rows = await _reports.ListAsync(cmp, customerId, ct);
        return Ok(NisApiResponse<List<NisReportDto>>.Of(rows, await _archive.DataAsOfAsync(cmp, ct), rows.Count));
    });

    /// <summary>
    /// POST api/nis/report/reports?cmpid= — body {templateId, from?, to?, issueNo?} · 400 ช่วงไม่ตรง/รหัสรอง/archived ·
    /// 422 ด่านธุรกิจ (holiday · truncation · freshness · printability · contrast · sales) · meta.issueWarning เตือนเลขฉบับข้ามลำดับ
    /// </summary>
    [HttpPost("reports")]
    public Task<IActionResult> CreateReport([FromQuery] string? cmpid, [FromBody] NisCreateReportDto body, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(body.TemplateId)) issues.Add("templateId: ต้องระบุ");
        if (body.From != null && !NisReportSchema.IsThaiDate(body.From)) issues.Add("from: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");
        if (body.To != null && !NisReportSchema.IsThaiDate(body.To)) issues.Add("to: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");
        if (body.IssueNo is < 1 or > 999) issues.Add("issueNo: ต้องอยู่ระหว่าง 1–999");
        if (issues.Count > 0) return BadRequest(new { message = "ข้อมูลที่ส่งมาไม่ถูกต้อง", issues });

        var row = await _reports.CreateAsync(cmp, body, CurrentUsername(), ct);
        // เตือน ไม่ห้าม — การข้ามลำดับอาจถูกต้องได้ถ้าสัญญาเริ่มกลางปี · ไม่นับฉบับที่เพิ่งสร้างเป็น "ฉบับก่อน" ของตัวเอง
        var previous = await _reports.LastIssueNoAsync(cmp, row.CustomerId, row.Id, ct);
        return Ok(NisApiResponse<NisReportDto>.Of(row, await _archive.DataAsOfAsync(cmp, ct), 1, new Dictionary<string, object?>
        {
            ["issueWarning"] = NisReportSchema.IssueSequenceWarning(previous, row.IssueNo),
        }));
    });

    /// <summary>GET api/nis/report/reports/{id}?cmpid=</summary>
    [HttpGet("reports/{id:guid}")]
    public Task<IActionResult> GetReport([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
        Ok(NisApiResponse<NisReportDto>.Of(await _reports.GetAsync(cmp, id, ct), null, 1)));

    /// <summary>POST api/nis/report/reports/{id}/generate?cmpid= — 404 ไม่มีรายงาน · 422 assertRenderable ตก · คืน {jobId, status} (ใบที่ค้างอยู่ใช้ซ้ำ)</summary>
    [HttpPost("reports/{id:guid}/generate")]
    public Task<IActionResult> Generate([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        if (!await _reports.ExistsAsync(cmp, id, ct)) return NotFound(new { message = $"ไม่พบรายงาน \"{id}\"" });
        await _reports.AssertRenderableAsync(id, ct);
        var job = await _queue.EnqueueAsync(cmp, id, ct);
        return Ok(NisApiResponse<object>.Of(new { jobId = job.Id, status = job.Status }, null, 1));
    });

    /// <summary>GET api/nis/report/reports/{id}/job?cmpid= — ใบล่าสุดของคิว (สถานะจริง queued→running(rendering/writing)→done/failed + error อ่านออก)</summary>
    [HttpGet("reports/{id:guid}/job")]
    public Task<IActionResult> JobStatus([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var job = await _queue.LatestForAsync(cmp, id, ct);
        return Ok(NisApiResponse<NisReportJobDto?>.Of(job, null, job == null ? 0 : 1));
    });

    /// <summary>GET api/nis/report/reports/{id}/pdf?cmpid= — ไฟล์จากฐานก่อน ดิสก์ทีหลัง · 404 ถ้ายังไม่เรนเดอร์ · X-Content-SHA256</summary>
    [HttpGet("reports/{id:guid}/pdf")]
    public Task<IActionResult> DownloadPdf([FromQuery] string? cmpid, Guid id, CancellationToken ct) => Guarded(cmpid, async cmp =>
    {
        var found = await _reports.PdfOfAsync(cmp, id, ct);
        if (found == null) return NotFound(new { message = $"รายงาน \"{id}\" ยังไม่มีไฟล์ PDF — สั่งสร้างก่อน" });
        // ชื่อไฟล์ตาม SPEC 6.4 — รายงานบริการ_[ชื่อลูกค้า]_[YYYY-MM].pdf (RFC 5987)
        Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(found.Value.Name)}";
        Response.Headers["X-Content-SHA256"] = found.Value.Sha ?? string.Empty;
        return File(found.Value.Bytes, "application/pdf");
    });

    /// <summary>
    /// GET api/nis/report/reports/{id}/print-data?key= — ข้อมูลให้หน้าพิมพ์ /nsr-print/{id} บน CRM (Playwright เปิด) · ไม่ต้องมี JWT
    /// key ออกโดยคิวต่อรอบเรนเดอร์ · ตรวจ SHA-256 + หมดอายุ (PrintKeyTtlSeconds) · ผิด/หมดอายุ → 401 · ไม่ consume (เปิดได้หลายครั้งภายในอายุ)
    /// </summary>
    [HttpGet("reports/{id:guid}/print-data")]
    [AllowAnonymous]
    public async Task<IActionResult> PrintData(Guid id, [FromQuery] string? key, CancellationToken ct)
    {
        try
        {
            Response.Headers["Cache-Control"] = "no-store";
            var data = await _reports.GetPrintDataAsync(id, key, ct);
            return Ok(NisApiResponse<NisPrintDataDto>.Of(data, null, 1));
        }
        catch (NisUnauthorizedException e) { return Unauthorized(new { message = e.Message }); }
        catch (NisArchiveNotFoundException e) { return NotFound(new { message = e.Message }); }
    }

    // ── helpers ──────────────────────────────────────────────────────────

    /// <summary>cmpid ต้องมี (tenant key) → 400 · map exception ของโมดูลเป็น status code เดียวกันทุก action</summary>
    private async Task<IActionResult> Guarded(string? cmpid, Func<string, Task<IActionResult>> body)
    {
        if (string.IsNullOrWhiteSpace(cmpid) || cmpid.Length > 50)
            return BadRequest(new { message = "ต้องระบุ cmpid" });
        try
        {
            return await body(cmpid);
        }
        catch (NisReportBadRequestException e) { return BadRequest(Problem(e.Message, e.Details)); }
        catch (NisArchiveBadRequestException e) { return BadRequest(new { message = e.Message }); }
        catch (NisArchiveNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (NisConflictException e) { return Conflict(new { message = e.Message }); }
        catch (NisUnprocessableException e) { return UnprocessableEntity(Problem(e.Message, e.Details)); }
        catch (NisUnauthorizedException e) { return Unauthorized(new { message = e.Message }); }
        catch (InvalidOperationException e)
        {
            // บั๊กของเรา (เช่น Q25 ตก) — 500 แต่ต้องเห็นข้อความ ไม่ใช่ "Internal server error" ลอย ๆ
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = e.Message });
        }
    }

    private static Dictionary<string, object?> Problem(string message, IDictionary<string, object?>? details)
    {
        var body = new Dictionary<string, object?> { ["message"] = message };
        if (details != null) foreach (var kv in details) body[kv.Key] = kv.Value;
        return body;
    }

    /// <summary>อ่าน body ดิบทั้งก้อน — ตัดตั้งแต่ระหว่างรับ (เกินเพดาน → null) ไม่ใช่รับครบแล้วค่อยวัด</summary>
    private async Task<byte[]?> ReadRawBodyAsync(CancellationToken ct)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[64 * 1024];
        int read;
        while ((read = await Request.Body.ReadAsync(buffer, ct)) > 0)
        {
            if (ms.Length + read > UploadLimitBytes) return null;
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }

    private string FileNameHint(string fallback)
    {
        var v = Request.Headers["x-file-name"].ToString();
        return string.IsNullOrWhiteSpace(v) ? fallback : (v.Length > 200 ? v[..200] : v);
    }

    /// <summary>username จาก JWT (claim sub = Accounts.Username · map เป็น NameIdentifier ได้) — ลง CreatedBy/UpdatedBy</summary>
    private string CurrentUsername() =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.Identity?.Name
        ?? "unknown";
}
