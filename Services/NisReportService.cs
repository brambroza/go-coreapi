using System.Security.Cryptography;
using System.Text;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace goalongapi.Services;

/// <summary>ชุดวันหยุดที่ใช้คำนวณ (HolidaySet) — ถูกแช่แข็งลง dataSnapshot.context.holiday</summary>
public sealed class NisHolidaySet
{
    /// <summary>ลายนิ้วมือของวันที่ Observed ทั้งชุด (sha256 12 ตัว) · 'none' เมื่อยังไม่มีสักวัน</summary>
    public string SetId { get; init; } = "none";
    public int ObservedCount { get; init; }
    /// <summary>ปีที่ทุกแถวในตาราง Observed=true แล้ว (ไม่ใช่แค่อย่างน้อยหนึ่งวัน — reviewer รอบ 6)</summary>
    public List<int> ObservedYears { get; init; } = new();
}

/// <summary>
/// รายงานบริการ — port จาก apps/api/src/reports/reports.service.ts (create · preview · list · byId · assertRenderable
/// · pdfOf · holidaySet · lastIssueNo · customerNameWarning) + print key สำหรับ E29
///
/// 🔴 ลำดับใน <see cref="CreateAsync"/> สำคัญ: คำนวณ projection จาก layout **ก่อน** ไปดึงข้อมูล (กฎ Q25) ·
///    ด่านวันหยุดตรวจก่อนดึงข้อมูล · ด่านความสดต้องอยู่หลัง buildSnapshot (dataAsOf อยู่ใน context)
/// 🔴 ช่วงเวลามาจาก **เอกสาร** (template.period) ไม่ใช่จาก body — สองแหล่งความจริงคือบั๊ก 2.3 บนกระดาษ
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface)
/// </summary>
public class NisReportService
{
    private readonly DatabaseContext _db;
    private readonly NisReportTemplateService _templates;
    private readonly NisReportCompanyService _company;
    private readonly NisCustomerGroupResolverService _groups;
    private readonly INisArchiveService _archive;
    private readonly NisReportSnapshotService _snapshots;
    private readonly int _printKeyTtlSeconds;

    public NisReportService(
        DatabaseContext db,
        NisReportTemplateService templates,
        NisReportCompanyService company,
        NisCustomerGroupResolverService groups,
        INisArchiveService archive,
        NisReportSnapshotService snapshots,
        IConfiguration configuration)
    {
        _db = db;
        _templates = templates;
        _company = company;
        _groups = groups;
        _archive = archive;
        _snapshots = snapshots;
        _printKeyTtlSeconds = configuration.GetValue<int?>("NisReport:PrintKeyTtlSeconds") ?? 300;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. สร้างรายงาน (14 ขั้นตามลำดับใน reports.service.ts:92)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// สร้างรายงานหนึ่งฉบับ — ด่านทุกด่านอยู่ที่ทางออกนี้ · ตกด่านธุรกิจ → 422 · ตก Q25 → 500 พร้อมข้อความ
    /// </summary>
    public async Task<NisReportDto> CreateAsync(string cmpId, NisCreateReportDto input, string createdBy, CancellationToken ct = default)
    {
        if (!Guid.TryParse(input.TemplateId, out var templateId))
            throw new NisReportBadRequestException("ข้อมูลที่ส่งมาไม่ถูกต้อง", new Dictionary<string, object?> { ["issues"] = new[] { "templateId: ต้องเป็น GUID" } });

        // 1. template ต้องมีและไม่ถูกเก็บเข้ากรุ
        var tpl = await _templates.EntityAsync(cmpId, templateId, ct);
        if (tpl.ArchivedAt != null)
            throw new NisReportBadRequestException($"template \"{tpl.Name}\" ถูกเก็บเข้ากรุแล้ว — ออกรายงานใหม่จากมันไม่ได้");
        var layout = NisReportTemplateService.LayoutOf(tpl);
        var (docFrom, docTo) = NisReportSchema.DocPeriod(layout);

        // 2. ช่วงเวลา (ถ้าส่งมา) ต้องตรงกับช่วงของเอกสาร — ช่วงอยู่ที่ template.period ที่เดียว
        if (input.From != null || input.To != null)
        {
            if (input.From != docFrom || input.To != docTo)
                throw new NisReportBadRequestException(
                    "ช่วงเวลาที่ส่งมาไม่ตรงกับช่วงของเอกสาร — ช่วงอยู่ที่ `template.period` ที่เดียว แก้ที่ template ก่อนแล้วค่อยออกรายงาน",
                    new Dictionary<string, object?>
                    {
                        ["templatePeriod"] = new { from = docFrom, to = docTo },
                        ["requested"] = new { from = input.From, to = input.To },
                    });
        }

        // 3. docMeta.customerId เป็นตัวจริงของ "ตัวเลขนี้เป็นของใคร" — ไม่ตรงเจ้าของ template = ปฏิเสธ
        var inLayout = NisReportSchema.DocCustomerId(layout);
        if (inLayout != tpl.CustomerId)
            throw new NisReportBadRequestException("รหัสลูกค้าในเอกสารไม่ตรงกับเจ้าของ template — ตัวเลขในรายงานจะเป็นของคนละราย",
                new Dictionary<string, object?> { ["inLayout"] = inLayout, ["templateOwner"] = tpl.CustomerId });

        // 4. projection + periods จาก layout ก่อนดึงข้อมูล
        var projection = NisReportSchema.ProjectionOf(layout);
        var periods = NisReportSchema.PeriodsOf(layout);

        // 5. ด่านวันหยุด — ก่อนดึงข้อมูล · เทียบรายปี
        var holiday = await HolidaySetAsync(cmpId, ct);
        var gate = NisReportSchema.HolidayGate(layout, periods, holiday.ObservedYears);
        if (gate.Blocked)
            throw new NisUnprocessableException(gate.Message!, new Dictionary<string, object?>
            {
                ["blocks"] = gate.Uses.Select(u => new { blockId = u.BlockId, label = u.Label }).ToList(),
                ["missingYears"] = gate.MissingYears,
            });

        // 6. กลุ่มลูกค้า ณ ตอนออกรายงาน (SPEC §9 ข้อ 59) — รหัสรอง → ปฏิเสธ (ช่องโหว่ตามลำดับเวลา · reviewer 26 ส.ค. 2569)
        var group = await _groups.ResolveGroupAsync(cmpId, tpl.CustomerId, ct);
        if (group.IsSecondary)
            throw new NisReportBadRequestException(
                $"template นี้ผูกกับรหัส \"{tpl.CustomerId}\" ซึ่งถูกรวมเป็นรหัสรองของ \"{group.PrimaryCustomerId}\" " +
                "ไปแล้วหลังจากสร้าง template นี้ — ออกรายงานจาก template ของรหัสหลักแทน " +
                "(ช่องระดับเอกสารต้องมาจากรหัสหลักเท่านั้น ห้ามผสม)");

        // 7. ประกอบ snapshot — ทางเดียวกับ preview
        var company = await _company.ReadAsync(cmpId, ct);
        var snapshot = await _snapshots.BuildAsync(cmpId, layout, projection, group, holiday, company, ct);

        // 8. ด่านภาคผนวก (SPEC §9 ข้อ 17)
        var trunc = NisReportSchema.TruncationGate(snapshot, NisReportSchema.HasAppendixBlock(layout));
        if (trunc.Blocked)
            throw new NisUnprocessableException(trunc.Message!, new Dictionary<string, object?>
            {
                ["omittedCount"] = trunc.OmittedTotal,
                ["details"] = trunc.Details,
            });

        // 9. ด่านความสด (SPEC §9 ข้อ 41) — หลัง buildSnapshot
        var fresh = NisReportSchema.FreshnessGate(snapshot);
        if (fresh.Blocked)
            throw new NisUnprocessableException(fresh.Message!, new Dictionary<string, object?>
            {
                ["dataAsOf"] = fresh.DataAsOf,
                ["beyond"] = fresh.Beyond,
            });

        // 10. ด่าน "พิมพ์ได้ไหม" — snapshot ที่ไม่รู้ว่านับชุดไหน
        var printable = NisReportSchema.PrintabilityGate(snapshot);
        if (printable.Blocked)
            throw new NisUnprocessableException(printable.Message, new Dictionary<string, object?> { ["periods"] = printable.Periods });

        // 11. ด่านสีหน้าปก (SPEC §9 ข้อ 20)
        var contrast = NisReportCoverContrast.Gate(layout);
        if (contrast.Blocked)
            throw new NisUnprocessableException(contrast.Message!, new Dictionary<string, object?>
            {
                ["failures"] = contrast.Failures,
                ["suggestion"] = contrast.Suggestion,
            });

        // 12. ด่านหน้าติดต่อฝ่ายขาย — ตรวจกับตั้งค่าปัจจุบัน (ชุดที่จะถูกแช่แข็ง)
        var sales = NisReportSchema.SalesContactGate(layout, company);
        if (sales.Blocked)
            throw new NisUnprocessableException(sales.Message!, new Dictionary<string, object?> { ["contactName"] = sales.ContactName });

        // 13. ด่านสุดท้ายก่อนลงฐาน — ตกที่นี่ = บั๊กของเรา (500 แต่ต้องเห็นข้อความ)
        try
        {
            NisReportSchema.AssertSnapshotWithinProjection(projection, snapshot);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException($"dataSnapshot ไม่ผ่านกฎ Q25 — ไม่บันทึก\n{e.Message}", e);
        }

        // 14. บันทึก — ขอบเขตช่วงเวลาเป็น +07:00 คงที่ (ไทยไม่มี DST)
        var meta = layout["docMeta"] as JObject ?? new JObject();
        var now = DateTimeOffset.UtcNow;
        var row = new NisReport
        {
            CmpId = cmpId,
            TemplateId = tpl.Id,
            TemplateVersion = tpl.Version,
            CustomerId = tpl.CustomerId,
            CustomerName = (string?)meta["customerNameTh"] ?? string.Empty,
            IssueNo = input.IssueNo ?? (meta["issueNo"] is { Type: JTokenType.Integer } iss ? (int)iss : null),
            PeriodStart = NisBangkokTime.StartOfDay(docFrom),
            PeriodEnd = NisBangkokTime.EndOfDay(docTo),
            LayoutSnapshotJson = NisReportJson.Compact(layout),
            DataSnapshotJson = NisReportJson.Compact(snapshot),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
        };
        _db.NisReports.Add(row);
        await _db.SaveChangesAsync(ct);
        return NisReportDto.From(row, tpl.Name);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. preview · projection
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ข้อมูลตัวอย่างสำหรับหน้าออกแบบ — ผ่าน buildSnapshot ตัวเดียวกับตอนออกรายงานจริง · ไม่บันทึก · ไม่กันด้วยด่าน
    /// แต่ส่งสถานะทุกด่านกลับไปให้หน้าจอขึ้นป้ายได้ก่อนกดออกรายงาน
    /// </summary>
    public async Task<NisPreviewDto> PreviewAsync(string cmpId, Guid templateId, CancellationToken ct = default)
    {
        var tpl = await _templates.EntityAsync(cmpId, templateId, ct);
        var layout = NisReportTemplateService.LayoutOf(tpl);
        var projection = NisReportSchema.ProjectionOf(layout);
        var holiday = await HolidaySetAsync(cmpId, ct);
        var group = await _groups.ResolveGroupAsync(cmpId, tpl.CustomerId, ct);
        var company = await _company.ReadAsync(cmpId, ct);
        var snapshot = await _snapshots.BuildAsync(cmpId, layout, projection, group, holiday, company, ct);
        // ตรวจด้วยกฎเดียวกับตอนบันทึกจริง — ถ้าตัวอย่างผ่านแต่ของจริงตกคือกับดัก
        NisReportSchema.AssertSnapshotWithinProjection(projection, snapshot);
        return new NisPreviewDto
        {
            Snapshot = NisReportJson.ToElement(snapshot),
            HolidayGate = NisReportSchema.HolidayGate(layout, NisReportSchema.PeriodsOf(layout), holiday.ObservedYears),
            FreshnessGate = NisReportSchema.FreshnessGate(snapshot),
            TruncationGate = NisReportSchema.TruncationGate(snapshot, NisReportSchema.HasAppendixBlock(layout)),
            CoverContrastGate = NisReportCoverContrast.Gate(layout),
            SalesContactGate = NisReportSchema.SalesContactGate(layout, company),
            CustomerNameWarning = await CustomerNameWarningAsync(cmpId, layout, ct),
            LastIssueNo = await LastIssueNoAsync(cmpId, tpl.CustomerId, null, ct),
        };
    }

    /// <summary>POST templates/{id}/projection — ทำให้กฎ Q25 มองเห็นได้ก่อนตัดสินใจ + ด่านวันหยุด + คำเตือนชื่อลูกค้า</summary>
    public async Task<NisProjectionDto> ProjectionAsync(string cmpId, Guid templateId, CancellationToken ct = default)
    {
        var tpl = await _templates.EntityAsync(cmpId, templateId, ct);
        var layout = NisReportTemplateService.LayoutOf(tpl);
        var p = NisReportSchema.ProjectionOf(layout);
        var orientation = (string?)layout["page"]?["orientation"] ?? "portrait";
        var holiday = await HolidaySetAsync(cmpId, ct);
        return new NisProjectionDto
        {
            DocPeriod = new NisPeriodDto { From = p.DocFrom, To = p.DocTo },
            HasOverride = p.HasOverride,
            Needs = p.Needs.Select(n => new NisProjectionNeedDto
            {
                Key = n.Key,
                Period = new NisPeriodDto { From = n.From, To = n.To },
                KpiMetrics = n.KpiMetrics,
                Datasets = n.Datasets,
                CaseColumns = n.CaseColumns,
                CaseFields = n.CaseFields.OrderBy(f => f, StringComparer.Ordinal).ToList(),
                CaseRowLimit = n.CaseRowLimit,
                CaseDateField = n.CaseDateField,
                PersonalColumns = NisReportSchema.PersonalColumnLabels(n.CaseColumns),
                WidthMeter = n.CaseColumns.Count > 0 ? NisReportSchema.WidthMeter(n.CaseColumns, orientation) : null,
            }).ToList(),
            HolidayGate = NisReportSchema.HolidayGate(layout, NisReportSchema.PeriodsOf(layout), holiday.ObservedYears),
            CustomerNameWarning = await CustomerNameWarningAsync(cmpId, layout, ct),
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. list · get · pdf · job
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>รายงาน 200 ฉบับล่าสุดของบริษัท (กรองลูกค้าได้) พร้อมชื่อ template ปัจจุบัน</summary>
    public async Task<List<NisReportDto>> ListAsync(string cmpId, string? customerId, CancellationToken ct = default)
    {
        var q = _db.NisReports.AsNoTracking().Where(r => r.CmpId == cmpId);
        if (!string.IsNullOrWhiteSpace(customerId)) q = q.Where(r => r.CustomerId == customerId);
        // projection ไม่ดึง PdfBlob — 200 ฉบับ × ไฟล์ PDF จะหนักเกินไปสำหรับหน้าคลังรายงาน
        var rows = await q
            .OrderByDescending(r => r.CreatedAt)
            .Take(200)
            .Select(r => new
            {
                r.Id, r.TemplateId, TemplateName = r.Template!.Name, r.TemplateVersion, r.CustomerId, r.CustomerName, r.IssueNo,
                r.PeriodStart, r.PeriodEnd, r.Status, r.PdfPath, r.CreatedAt, r.LayoutSnapshotJson, r.DataSnapshotJson,
            })
            .ToListAsync(ct);
        return rows.Select(x => new NisReportDto
        {
            Id = x.Id,
            TemplateId = x.TemplateId,
            TemplateName = x.TemplateName,
            TemplateVersion = x.TemplateVersion,
            CustomerId = x.CustomerId,
            CustomerName = x.CustomerName,
            IssueNo = x.IssueNo,
            PeriodStart = NisServiceCaseDto.Iso(x.PeriodStart),
            PeriodEnd = NisServiceCaseDto.Iso(x.PeriodEnd),
            Status = x.Status,
            PdfPath = x.PdfPath,
            CreatedAt = NisServiceCaseDto.Iso(x.CreatedAt),
            LayoutSnapshot = NisReportJson.ToElement(x.LayoutSnapshotJson),
            DataSnapshot = NisReportJson.ToElement(x.DataSnapshotJson),
        }).ToList();
    }

    /// <summary>รายงานหนึ่งฉบับ — 404 ถ้าไม่พบในบริษัทนี้</summary>
    public async Task<NisReportDto> GetAsync(string cmpId, Guid id, CancellationToken ct = default)
    {
        var row = await _db.NisReports.AsNoTracking().Include(r => r.Template).FirstOrDefaultAsync(r => r.CmpId == cmpId && r.Id == id, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบรายงาน \"{id}\"");
        return NisReportDto.From(row, row.Template?.Name ?? string.Empty);
    }

    /// <summary>มีรายงานนี้ในบริษัทนี้ไหม — ให้ generate ตอบ 404 ก่อนเข้าคิว</summary>
    public async Task<bool> ExistsAsync(string cmpId, Guid id, CancellationToken ct = default) =>
        await _db.NisReports.AsNoTracking().AnyAsync(r => r.CmpId == cmpId && r.Id == id, ct);

    /// <summary>
    /// ด่านก่อนเรนเดอร์ PDF จริง (assertRenderable) — snapshot ที่แช่แข็งต้องตรงกับสิ่งที่รู้ตอนนี้ **ทั้งสองทิศ**
    /// (ธงบันทึกย้อนหลัง · สมาชิกกลุ่ม) + ด่านหน้าติดต่อฝ่ายขายซ้ำกับข้อมูลที่แช่แข็ง · snapshot v1 (ไม่มี context) ปล่อยผ่าน
    /// เรียกทั้งจาก controller ก่อน enqueue และจาก queue ณ วินาทีที่ผลิตจริง
    /// </summary>
    /// <exception cref="NisArchiveNotFoundException">ไม่พบรายงาน</exception>
    /// <exception cref="NisUnprocessableException">snapshot ไม่ตรงกับสถานะปัจจุบัน หรือหน้าติดต่อฝ่ายขายอ้างเกินจริง</exception>
    public async Task AssertRenderableAsync(Guid reportId, CancellationToken ct = default)
    {
        var row = await _db.NisReports.AsNoTracking()
                      .Select(r => new { r.Id, r.CmpId, r.CustomerId, r.LayoutSnapshotJson, r.DataSnapshotJson })
                      .FirstOrDefaultAsync(r => r.Id == reportId, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบรายงาน \"{reportId}\"");
        var group = await _groups.ResolveGroupAsync(row.CmpId, row.CustomerId, ct);
        var snapshot = NisReportJson.TryParseObject(row.DataSnapshotJson);
        var layout = NisReportJson.TryParseObject(row.LayoutSnapshotJson) ?? new JObject();
        var context = snapshot?["context"] as JObject;

        // ด่านหน้าติดต่อฝ่ายขาย — ใช้ข้อมูลที่แช่แข็งไว้กับรายงาน (ถ้ามี) ไม่งั้นค่าปัจจุบัน
        var frozenCo = context?["company"] as JObject;
        var company = frozenCo != null ? NisReportCompanyService.FromJson(frozenCo) : await _company.ReadAsync(row.CmpId, ct);
        var sales = NisReportSchema.SalesContactGate(layout, company);
        if (sales.Blocked)
            throw new NisUnprocessableException(sales.Message!, new Dictionary<string, object?> { ["contactName"] = sales.ContactName });

        if (context == null) return;

        var frozenReason = (string?)context["durationHiddenReason"];
        var expectedReason = group.Backdated ? NisReportSchema.DurationHiddenReason : null;
        var reasonStale = frozenReason != expectedReason;
        var frozenGroup = (context["customerGroup"] as JArray)?.Select(x => (string?)x ?? "").OrderBy(x => x, StringComparer.Ordinal).ToList() ?? new List<string>();
        var currentGroup = group.MemberCustomerIds.OrderBy(x => x, StringComparer.Ordinal).ToList();
        var groupStale = !frozenGroup.SequenceEqual(currentGroup, StringComparer.Ordinal);
        if (!reasonStale && !groupStale) return;

        static string JsonList(List<string> xs) => "[" + string.Join(",", xs.Select(x => $"\"{x}\"")) + "]";
        var message = groupStale
            ? $"กลุ่มลูกค้าของ \"{group.PrimaryCustomerId}\" เปลี่ยนไปแล้วตั้งแต่รายงานฉบับนี้ถูกสร้าง " +
              $"(ตอนนั้นรวมรหัส {JsonList(frozenGroup)} ตอนนี้คือ {JsonList(currentGroup)}) — " +
              "เรนเดอร์เป็น PDF ฉบับใหม่จากตัวเลขชุดนี้ไม่ได้ เพราะจำนวนเคส/ตัวเลขจะไม่ครบกลุ่มปัจจุบัน " +
              "ดาวน์โหลดฉบับที่เคยเรนเดอร์ไว้แล้ว (ถ้ามี) ได้ตามปกติ — ถ้าต้องการฉบับใหม่ที่ถูกต้อง " +
              "ให้สร้างรายงานฉบับใหม่จาก template นี้อีกครั้ง"
            : group.Backdated
                ? $"กลุ่มลูกค้า \"{group.PrimaryCustomerId}\" ติดธงบันทึกย้อนหลังอยู่ตอนนี้ แต่รายงานฉบับนี้ถูก" +
                  "สร้าง (แช่แข็งตัวเลข) ไว้ก่อนหน้านั้นหรือด้วยถ้อยคำรุ่นเก่า — เรนเดอร์เป็น PDF ฉบับใหม่จาก" +
                  "ตัวเลขชุดนี้ไม่ได้ เพราะจะไม่สะท้อนสิ่งที่รู้ตอนนี้ ดาวน์โหลดฉบับที่เคยเรนเดอร์ไว้แล้ว (ถ้ามี) " +
                  "ได้ตามปกติ — ถ้าต้องการฉบับใหม่ที่ถูกต้อง ให้สร้างรายงานฉบับใหม่จาก template นี้อีกครั้ง"
                : $"กลุ่มลูกค้า \"{group.PrimaryCustomerId}\" ไม่ได้ติดธงบันทึกย้อนหลังแล้ว แต่รายงานฉบับนี้ถูก" +
                  "สร้างไว้ตอนที่ยังติดธงอยู่ (แช่แข็งประโยคว่าข้อมูลเชื่อไม่ได้ไว้แล้ว) — เรนเดอร์เป็น PDF ฉบับ" +
                  "ใหม่จาก snapshot นี้ไม่ได้ เพราะจะพิมพ์คำกล่าวอ้างที่ไม่ตรงกับสิ่งที่รู้ตอนนี้ ดาวน์โหลดฉบับที่" +
                  "เคยเรนเดอร์ไว้แล้ว (ถ้ามี) ได้ตามปกติ — ถ้าต้องการฉบับใหม่ที่ถูกต้อง ให้สร้างรายงานฉบับใหม่จาก " +
                  "template นี้อีกครั้ง";
        throw new NisUnprocessableException(message, new Dictionary<string, object?> { ["primaryCustomerId"] = group.PrimaryCustomerId });
    }

    /// <summary>ไฟล์ PDF ของรายงาน — จากฐาน (PdfBlob) ก่อน ดิสก์ (PdfPath) ทีหลัง · null = ยังไม่เคยเรนเดอร์สำเร็จ</summary>
    public async Task<(byte[] Bytes, string Name, string? Sha)?> PdfOfAsync(string cmpId, Guid id, CancellationToken ct = default)
    {
        var r = await _db.NisReports.AsNoTracking().FirstOrDefaultAsync(x => x.CmpId == cmpId && x.Id == id, ct)
                ?? throw new NisArchiveNotFoundException($"ไม่พบรายงาน \"{id}\"");
        // เดือนต้องมาจากเวลาไทย ไม่ใช่ UTC — periodStart ของ ส.ค. เก็บเป็น 31 ก.ค. 17:00Z
        var period = NisBangkokTime.MonthKey(r.PeriodStart);
        var name = $"รายงานบริการ_{r.CustomerName}_{period}.pdf";
        if (r.PdfBlob is { Length: > 0 }) return (r.PdfBlob, name, r.PdfSha256);
        if (!string.IsNullOrEmpty(r.PdfPath) && File.Exists(r.PdfPath)) return (await File.ReadAllBytesAsync(r.PdfPath, ct), name, r.PdfSha256);
        return null;
    }

    /// <summary>เลขฉบับล่าสุดของลูกค้ารายนี้ — หน้าจอเตือนเมื่อเลขข้ามลำดับ · excludeReportId = ไม่นับฉบับที่เพิ่งสร้าง</summary>
    public async Task<int?> LastIssueNoAsync(string cmpId, string customerId, Guid? excludeReportId = null, CancellationToken ct = default) =>
        await _db.NisReports.AsNoTracking()
            .Where(r => r.CmpId == cmpId && r.CustomerId == customerId && r.IssueNo != null && (excludeReportId == null || r.Id != excludeReportId))
            .OrderByDescending(r => r.PeriodStart)
            .Select(r => r.IssueNo)
            .FirstOrDefaultAsync(ct);

    // ═══════════════════════════════════════════════════════════════════════
    // 4. วันหยุด · ชื่อลูกค้า
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ชุดวันหยุดที่ระบบใช้อยู่ ณ ตอนนี้ (holidaySet) — setId เป็นลายนิ้วมือของวันที่ Observed ทั้งชุด ·
    /// observedYears = ปีที่ทุกแถว Observed แล้ว (ผ่าน <see cref="NisBusinessHours.Build"/>)
    /// </summary>
    public async Task<NisHolidaySet> HolidaySetAsync(string cmpId, CancellationToken ct = default)
    {
        var rows = await _db.NisHolidays.AsNoTracking().Where(h => h.CmpId == cmpId).OrderBy(h => h.Date).ToListAsync(ct);
        var observed = rows.Where(r => r.Observed).Select(r => r.Date).ToList();
        var set = NisBusinessHours.Build(NisBusinessHoursConfig.Default, rows);
        var setId = observed.Count == 0
            ? "none"
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(",", observed)))).ToLowerInvariant()[..12];
        return new NisHolidaySet { SetId = setId, ObservedCount = observed.Count, ObservedYears = set.ObservedYears.ToList() };
    }

    /// <summary>ชื่อจริงของลูกค้าใน archive — null เมื่อยังไม่มีเคสของรหัสนั้น</summary>
    public async Task<string?> ActualCustomerNameAsync(string cmpId, string customerId, CancellationToken ct = default) =>
        await _db.NisServiceCases.AsNoTracking()
            .Where(c => c.CmpId == cmpId && c.CustomerId == customerId && !c.IsTestData)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => c.CustomerName)
            .FirstOrDefaultAsync(ct);

    /// <summary>คำเตือนชื่อลูกค้าไม่ตรง — เตือน ไม่บล็อก</summary>
    public async Task<string?> CustomerNameWarningAsync(string cmpId, JObject layout, CancellationToken ct = default) =>
        NisReportSchema.CustomerNameMismatch(layout, await ActualCustomerNameAsync(cmpId, NisReportSchema.DocCustomerId(layout), ct));

    // ═══════════════════════════════════════════════════════════════════════
    // 5. print key (E29) — ให้ Playwright เปิดหน้าพิมพ์บน CRM โดยไม่ต้องมี JWT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ออก print key ใหม่ (32 ไบต์สุ่ม · hex 64 ตัว) — เก็บ SHA-256 ลง PrintKeyHash + หมดอายุใน PrintKeyTtlSeconds
    /// คืน key ดิบให้ตัวเรนเดอร์ใส่ ?key= · ออกใหม่ทุกรอบที่พยายามเรนเดอร์ (key เดิมถูกแทนที่)
    /// </summary>
    public async Task<string> IssuePrintKeyAsync(Guid reportId, CancellationToken ct = default)
    {
        var row = await _db.NisReports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบรายงาน \"{reportId}\"");
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        row.PrintKeyHash = HashKey(key);
        row.PrintKeyExpiresAt = DateTimeOffset.UtcNow.AddSeconds(_printKeyTtlSeconds);
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return key;
    }

    /// <summary>
    /// ข้อมูลหน้าพิมพ์ — ตรวจ hash + หมดอายุ (ผิด/หมดอายุ → 401) · **ไม่ consume key** เพราะ Playwright เปิดหน้าได้ถึง 3 ครั้ง
    /// (ปก / เนื้อ / ปกหลัง) · อายุคือขีดจำกัดเดียว · company = ชุดที่แช่แข็งใน snapshot (รายงานเก่าใช้ค่าปัจจุบัน)
    /// </summary>
    public async Task<NisPrintDataDto> GetPrintDataAsync(Guid reportId, string? key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new NisUnauthorizedException("ต้องมี key สำหรับหน้าพิมพ์");
        var row = await _db.NisReports.AsNoTracking().Include(r => r.Template).FirstOrDefaultAsync(r => r.Id == reportId, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบรายงาน \"{reportId}\"");
        if (string.IsNullOrEmpty(row.PrintKeyHash) || row.PrintKeyExpiresAt == null || row.PrintKeyExpiresAt < DateTimeOffset.UtcNow)
            throw new NisUnauthorizedException("print key หมดอายุหรือยังไม่ได้ออก — สั่งสร้าง PDF ใหม่");
        var expected = Encoding.ASCII.GetBytes(row.PrintKeyHash);
        var actual = Encoding.ASCII.GetBytes(HashKey(key.Trim()));
        if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(expected, actual))
            throw new NisUnauthorizedException("print key ไม่ถูกต้อง");

        var snapshot = NisReportJson.TryParseObject(row.DataSnapshotJson);
        var frozenCo = snapshot?["context"]?["company"] as JObject;
        var company = frozenCo != null ? NisReportCompanyService.FromJson(frozenCo) : await _company.ReadAsync(row.CmpId, ct);
        row.PdfBlob = null;
        return new NisPrintDataDto { Report = NisReportDto.From(row, row.Template?.Name ?? string.Empty), Company = company };
    }

    /// <summary>SHA-256 hex (ตัวพิมพ์เล็ก) ของ key ดิบ</summary>
    public static string HashKey(string key) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
}
