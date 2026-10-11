using goalongapi.Dtos.Nis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace goalongapi.Services;

/// <summary>
/// ประกอบ dataSnapshot — port จาก reports.service.ts <c>buildSnapshot()</c> + <c>buildCaseRows()</c>
///
/// 🔴 เรียกเฉพาะสิ่งที่ layout ต้องใช้ (ตาม <see cref="NisProjection"/>) · วนตามช่วงเวลา เพราะบล็อกใช้ช่วงต่างกันได้
/// 🔴 customerId เป็น**อาร์เรย์**เสมอ (SPEC §9 ข้อ 59) — รวมทุกรหัสในกลุ่ม · กันนับซ้ำด้วยเลขที่เคส
/// 🔴 ธงบันทึกย้อนหลัง — ตัวเลขระยะเวลาทุกจุด (KPI · breakdown.duration · คอลัมน์ resolutionHours) เป็น null ที่ชั้นนี้
///    จุดเดียวที่คุมได้แน่นอนว่าตัวเลขจะไม่ไหลออกไปจาก snapshot เลย
/// 🔴 แถวตารางผ่าน <see cref="NisReportSchema.PickCaseRow"/> เสมอ — เบอร์โทร/อีเมลลูกค้าต้องไม่ถูกแช่แข็งลง JSON ถาวร (Q25 · A4)
///
/// DTO ของ analytics ถูกแปลงเป็น JObject camelCase ก่อนหยิบ — path ใน <see cref="NisReportSchema.KpiMetrics"/>
/// (เช่น resolved.clockHours.median) จึงตรงกับ TS โดยไม่ผูกกับชนิด C# ของอีกชั้น
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface) · preview กับ create ใช้ตัวนี้ตัวเดียวกัน (CLAUDE.md ข้อ 15)
/// </summary>
public class NisReportSnapshotService
{
    /// <summary>pageSize สูงสุดของ archive = 500 · เพดานตาราง = 1000 — สองเลขนี้ไม่ผูกกัน จึงดึงทีละหน้าจนครบ</summary>
    private const int Page = NisCaseQuery.MaxPageSize;

    private static readonly JsonSerializer CamelCase = JsonSerializer.Create(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Include,
    });

    private readonly INisAnalyticsService _analytics;
    private readonly INisArchiveService _archive;

    public NisReportSnapshotService(INisAnalyticsService analytics, INisArchiveService archive)
    {
        _analytics = analytics;
        _archive = archive;
    }

    /// <summary>
    /// ประกอบ snapshot (รูป DataSnapshotSchema v2) — ผู้เรียกต้อง <see cref="NisReportSchema.AssertSnapshotWithinProjection"/> ก่อนบันทึก
    /// </summary>
    /// <param name="cmpId">tenant</param>
    /// <param name="layout">layout ที่ normalize แล้ว</param>
    /// <param name="projection">ผลของ <see cref="NisReportSchema.ProjectionOf"/> ของ layout เดียวกัน</param>
    /// <param name="group">กลุ่มลูกค้า ณ ตอนนี้ (รหัสหลัก + สมาชิก + ธง backdated)</param>
    /// <param name="holiday">ชุดวันหยุดที่ใช้ — แช่แข็งลง context</param>
    /// <param name="company">ข้อมูลบริษัทปัจจุบัน — ถูก freeze เหลือเฉพาะเซลส์ที่เอกสารใช้</param>
    public async Task<JObject> BuildAsync(
        string cmpId,
        JObject layout,
        NisProjection projection,
        NisCustomerGroupDto group,
        NisHolidaySet holiday,
        NisCompanyInfoDto company,
        CancellationToken ct = default)
    {
        var periods = new JObject();
        // ป้ายสองก้อนต้องถูกแช่แข็งไปด้วย ไม่งั้นรายงานเก่าจะใช้ป้ายของวันนี้ (SPEC 9.1 ข้อ 1)
        var intakeLabel = NisReportSchema.IntakeLabel;
        var resolvedLabel = NisReportSchema.ResolvedLabel;

        foreach (var need in projection.Needs)
        {
            if (!need.UsesData) continue;

            var filter = new NisCaseFilter { From = need.From, To = need.To, CustomerId = group.MemberCustomerIds.ToList() };
            var members = group.MemberCustomerIds;
            var outPeriod = new JObject { ["from"] = need.From, ["to"] = need.To, ["datasets"] = new JObject() };

            if (need.KpiMetrics.Count > 0)
            {
                var dto = await _analytics.SummaryAsync(cmpId, filter, members, ct);
                var j = ToJson(dto);
                var kpis = NisReportSchema.PickKpis(need, j["current"], j["previous"], j["mom"]?["kpis"] as JObject);
                intakeLabel = (string?)j["current"]?["intake"]?["label"] ?? intakeLabel;
                resolvedLabel = (string?)j["current"]?["resolved"]?["label"] ?? resolvedLabel;
                if (group.Backdated)
                {
                    foreach (var id in need.KpiMetrics)
                    {
                        var kind = NisReportSchema.KpiMetric(id).Kind;
                        if (kind is "clockHours" or "businessHours") kpis[id] = NisReportSchema.NullKpi(id);
                    }
                }
                outPeriod["kpis"] = kpis;
            }

            var datasets = (JObject)outPeriod["datasets"]!;
            foreach (var ds in need.Datasets)
            {
                if (ds is "trend.day" or "trend.month")
                {
                    var dto = await _analytics.TrendAsync(cmpId, filter, members, ds == "trend.day" ? "day" : "month", ct);
                    datasets[ds] = ToJson(dto);
                }
                else if (ds == "heatmap")
                {
                    datasets[ds] = ToJson(await _analytics.HeatmapAsync(cmpId, filter, members, ct));
                }
                else if (ds == "breakdown.duration" && group.Backdated)
                {
                    // ไม่ดึงข้อมูลจริงเลย แต่ต้องมีคีย์นี้ด้วยรูปที่ผ่าน Q25 — rows ว่าง ให้ตัววาดแสดง durationHiddenReason
                    datasets[ds] = new JObject { ["rows"] = new JArray() };
                }
                else if (ds.StartsWith("breakdown.", StringComparison.Ordinal))
                {
                    var by = ds["breakdown.".Length..];
                    var dto = await _analytics.BreakdownAsync(cmpId, filter, members, by, ct);
                    var j = ToJson(dto);
                    j.Remove("rowCount"); // คุณสมบัติช่วยของ C# ไม่ใช่รูปที่ TS เก็บ
                    datasets[ds] = ds == "breakdown.duration" ? NisReportSchema.PickDurationDataset(need, j) : j;
                }
            }

            // ดึงทีละคำขอ ไม่ใช่ทีละช่วงเวลา — ตารางที่ตั้งชุดแถว/ลำดับต่างกันคือคนละชุดข้อมูล
            for (var i = 0; i < need.CaseRequests.Count; i++)
            {
                var req = need.CaseRequests[i];
                if (req.Columns.Count == 0) continue;
                var bundle = await BuildCaseRowsAsync(cmpId, need, req, members, group.Backdated, ct);
                if (outPeriod["casesByKey"] is not JObject byKey) outPeriod["casesByKey"] = byKey = new JObject();
                byKey[req.Key] = bundle;
                // `cases` (ช่องเดี่ยว) = คำขอแรกเสมอ — รายงานก่อน 14 ก.ย. 2569 มีแต่ช่องนี้
                if (i == 0) outPeriod["cases"] = bundle.DeepClone();
            }

            periods[need.Key] = outPeriod;
        }

        var dataAsOf = await _archive.DataAsOfAsync(cmpId, ct);
        var frozenCompany = NisReportSchema.FreezeCompanyForReport(layout, company);

        return new JObject
        {
            ["snapshotVersion"] = NisReportSchema.SnapshotVersion,
            // เวอร์ชันของ grammar ที่ layout ข้าง ๆ ใช้ — สองค่านี้เดินคนละจังหวะได้
            ["templateSchemaVersion"] = (int?)layout["schemaVersion"] ?? NisReportSchema.SchemaVersion,
            ["generatedAt"] = NisServiceCaseDto.Iso(DateTimeOffset.UtcNow),
            ["period"] = new JObject { ["from"] = projection.DocFrom, ["to"] = projection.DocTo },
            ["context"] = new JObject
            {
                ["dataAsOf"] = dataAsOf.HasValue ? NisServiceCaseDto.Iso(dataAsOf.Value) : null,
                ["slaEnabled"] = _analytics.SlaEnabled,
                ["holiday"] = new JObject
                {
                    ["setId"] = holiday.SetId,
                    ["observedCount"] = holiday.ObservedCount,
                    ["observedYears"] = new JArray(holiday.ObservedYears),
                },
                // กลุ่ม ณ เวลานี้ — รายงานเก่าต้องอธิบายตัวเองได้ว่าตอนนั้นรวมรหัสอะไรบ้าง (ข้อ 59)
                ["customerGroup"] = new JArray(group.MemberCustomerIds),
                // ธงบันทึกย้อนหลัง ณ เวลานี้ — แช่แข็งไว้เหมือนกัน
                ["durationHiddenReason"] = group.Backdated ? NisReportSchema.DurationHiddenReason : null,
                // ข้อมูลบริษัท + เซลส์ที่เอกสารนี้ใช้ ณ ตอนสร้าง — สร้าง PDF ซ้ำภายหลังต้องได้ชุดเดิม (คุณ Tone 19 ก.ย. 2569)
                ["company"] = NisReportCompanyService.ToJson(frozenCompany),
            },
            ["countLabels"] = new JObject { ["intake"] = intakeLabel, ["resolved"] = resolvedLabel },
            ["periods"] = periods,
        };
    }

    /// <summary>
    /// แถวของตารางหนึ่งคำขอ (buildCaseRows) — ดึงทีละหน้าจนครบ rowLimit · dedupe ด้วย caseNo||sourceId ·
    /// pickCaseRow เฉพาะ field ที่คอลัมน์อ่าน · ตัดสิน truncated/omittedCount ที่นี่ตอนที่รู้ทั้ง limit และ total (SPEC §9 ข้อ 17)
    /// </summary>
    private async Task<JObject> BuildCaseRowsAsync(string cmpId, NisPeriodNeed need, NisCaseRequest req, IReadOnlyList<string> customerIds, bool backdated, CancellationToken ct)
    {
        var collected = new List<NisServiceCaseDto>();
        var total = 0;
        for (var page = 1; collected.Count < req.RowLimit; page++)
        {
            var want = Math.Min(Page, req.RowLimit - collected.Count);
            var query = new NisCaseQuery
            {
                From = need.From,
                To = need.To,
                CustomerId = customerIds.ToList(),
                DateField = req.DateField,
                // ลำดับที่ผู้ใช้สั่ง ไม่ใช่ค่าคงที่ — limit ตัดแถวหลังเรียง ถ้าเรียงผิดชุดแถวที่พิมพ์จะเป็นคนละชุดกับที่เห็นบนแผง
                Sort = req.Sort,
                Order = req.Order,
                Page = page,
                PageSize = want,
            };
            var issues = query.Validate();
            if (issues.Count > 0)
                throw new InvalidOperationException("คำขอตารางเคสจาก layout ไม่ผ่านตัวกรองของ archive: " + string.Join(" · ", issues));
            var r = await _archive.ListCasesAsync(cmpId, query, ct);
            total = r.Total;
            collected.AddRange(r.Rows);
            if (r.Rows.Count < want) break; // หมดแล้ว
        }

        // กันนับซ้ำด้วยเลขที่เคส (SPEC §9 ข้อ 59) — ตัดซ้ำที่นี่ จุดเดียวที่ผลลัพธ์นับจริง
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var deduped = new List<NisServiceCaseDto>();
        foreach (var row in collected)
        {
            var key = string.IsNullOrEmpty(row.CaseNo) ? row.SourceId : row.CaseNo;
            if (seen.Add(key)) deduped.Add(row);
        }
        var foundTotal = total - (collected.Count - deduped.Count);

        var rows = new JArray();
        foreach (var row in deduped)
        {
            var picked = NisReportSchema.PickCaseRow(req.Fields, ToJson(row));
            // ธงบันทึกย้อนหลัง — คอลัมน์ resolutionHours ต้อง null รายแถว ไม่ให้ตารางขัดกับการ์ด KPI ในเล่มเดียว
            if (backdated && picked.ContainsKey("resolutionHours")) picked["resolutionHours"] = null;
            rows.Add(picked);
        }

        return new JObject
        {
            ["columns"] = new JArray(req.Columns),
            ["rows"] = rows,
            ["total"] = foundTotal,
            // ป้ายเดินทางไปกับตัวเลขเสมอ — ประโยคใต้ตารางอ่านค่านี้
            ["countLabel"] = req.DateField == "createdAt" ? NisReportSchema.IntakeLabel : NisReportSchema.ResolvedLabel,
            ["dateField"] = req.DateField,
            ["truncated"] = foundTotal > rows.Count,
            ["omittedCount"] = Math.Max(0, foundTotal - rows.Count),
        };
    }

    /// <summary>DTO → JObject camelCase (รูปเดียวกับที่ endpoint analytics ตอบ)</summary>
    private static JObject ToJson(object dto) => JObject.FromObject(dto, CamelCase);
}
