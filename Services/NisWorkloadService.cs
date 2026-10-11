using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// หน้าภาระงานทีม — port จาก apps/api/src/workload/workload.service.ts · **ที่เดียวในระบบที่คำนวณตัวเลขรายบุคคล**
/// (controller กั้นด้วย role claim "mng" — MERGE_PLAN E19)
///
/// 🔴 ตัวเลขสามชนิด อย่าปน: ผูกช่วง (resolvedInPeriod · teamResolvedInPeriod) · YTD เสมอ (resolvedYtd · ค่ากลาง · excludedBackdatedCount ·
///    คะแนน) · สแนปช็อต (openNow) — ค่ากลาง/คะแนนคง YTD โดยตั้งใจ (ASK.md ข้อ 85 · ตัวหารของช่วงสั้นเล็กเกินไป)
/// 🔴 เคสหนึ่งใบนับให้ได้หลายคน (AssigneeNames เป็น array) · เคสของกลุ่มติดธง backdated นับเข้าจำนวนงานแต่ไม่เข้าเวลา
/// ลงทะเบียน AddScoped ใน Program.cs
/// </summary>
public class NisWorkloadService
{
    private readonly DatabaseContext _db;

    public NisWorkloadService(DatabaseContext db)
    {
        _db = db;
    }

    /// <summary>Math.round ของ JS</summary>
    private static double JsRound(double x) => Math.Floor(x + 0.5);

    /// <summary>ตัวเลขต่อเจ้าหน้าที่ · period = month | quarter | year (ช่วง**ปฏิทิน**ไทยที่วันนี้อยู่ ไม่ใช่ย้อนหลัง N วัน)</summary>
    public async Task<NisWorkloadOverviewDto> OverviewAsync(string cmpId, string period = "month", CancellationToken ct = default)
    {
        var cases = await _db.NisServiceCases.AsNoTracking()
            .Where(c => c.CmpId == cmpId && !c.IsTestData)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.SourceId)
            .Select(c => new { c.CaseNo, c.SourceId, c.CustomerId, c.AssigneeNamesJson, c.CreatedAt, c.ResolvedAt, c.ResolutionHours, c.SatisfactionScore })
            .ToListAsync(ct);
        var customerRows = await _db.NisCustomerGroups.AsNoTracking().Where(g => g.CmpId == cmpId).ToListAsync(ct);

        // ธงบันทึกย้อนหลังอ่านที่รหัสหลักของกลุ่ม — กติกาเดียวกับ NisCustomerGroupService
        var mergeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var backdatedPrimaries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in customerRows)
        {
            if (!string.IsNullOrWhiteSpace(c.MergeIntoCustomerId)) mergeMap[c.CustomerId] = c.MergeIntoCustomerId!;
            if (c.Backdated) backdatedPrimaries.Add(c.CustomerId);
        }
        bool IsBackdated(string customerId) => backdatedPrimaries.Contains(mergeMap.TryGetValue(customerId, out var p) ? p : customerId);

        // กันนับซ้ำด้วยเลขที่เคส — "เคสหนึ่งใบนับครั้งเดียวต่อคน"
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var deduped = cases.Where(c => seen.Add(string.IsNullOrEmpty(c.CaseNo) ? c.SourceId : c.CaseNo)).ToList();

        // ทุกขอบเขตคิดที่เวลาไทย — เคสที่ปิด 00:00–07:00 ของวันที่ 1 ต้องไม่ตกไปเดือนก่อน
        var now = NisBangkokTime.Now();
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, NisBangkokTime.Offset);
        var quarterStart = new DateTimeOffset(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1, 0, 0, 0, NisBangkokTime.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, NisBangkokTime.Offset);
        var periodStart = period switch { "year" => yearStart, "quarter" => quarterStart, _ => monthStart };

        var agents = new Dictionary<string, Agent>(StringComparer.Ordinal);
        var agentOrder = new List<string>();
        Agent AgentFor(string name)
        {
            if (!agents.TryGetValue(name, out var a))
            {
                agents[name] = a = new Agent();
                agentOrder.Add(name);
            }
            return a;
        }

        var resolvedCaseNosInPeriod = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in deduped)
        {
            // ชื่อซ้ำในเคสเดียวกัน (ข้อมูลต้นทางเพี้ยน) ไม่นับซ้ำให้คนเดียว
            var names = NisServiceCaseDto.ParseStringList(c.AssigneeNamesJson).Distinct(StringComparer.Ordinal).ToList();
            var inPeriod = c.ResolvedAt is { } ra1 && ra1 >= periodStart && ra1 <= now;
            var resolvedInYear = c.ResolvedAt is { } ra2 && ra2 >= yearStart && ra2 <= now;
            if (inPeriod) resolvedCaseNosInPeriod.Add(string.IsNullOrEmpty(c.CaseNo) ? c.SourceId : c.CaseNo);
            foreach (var name in names)
            {
                var a = AgentFor(name);
                if (inPeriod) a.ResolvedInPeriod++;
                if (resolvedInYear) a.ResolvedYtd++;
                if (c.ResolvedAt is null) a.OpenNow++;
                // เคสของลูกค้าที่ติดธง — เวลานาฬิกาเชื่อไม่ได้ ไม่เอาเข้าค่ากลาง (จำนวนงานยังนับ)
                if (resolvedInYear && c.ResolutionHours is { } h)
                {
                    if (IsBackdated(c.CustomerId)) a.ExcludedBackdatedCount++;
                    else a.ResolvedYtdHours.Add(h);
                }
                // ค่า 9 = ยังไม่ให้คะแนน — กติกาเดียวกับ NisAnalyticsService.Satisfaction
                if (resolvedInYear && c.SatisfactionScore is >= 1 and <= 5) a.SatisfactionScores.Add(c.SatisfactionScore.Value);
            }
        }

        var rows = agentOrder.Select(name =>
        {
            var a = agents[name];
            var (median, _, _, cnt) = NisHoursStat.HoursStat(a.ResolvedYtdHours);
            var satN = a.SatisfactionScores.Count;
            return new NisWorkloadRowDto
            {
                AgentName = name,
                ResolvedInPeriod = a.ResolvedInPeriod,
                ResolvedYtd = a.ResolvedYtd,
                OpenNow = a.OpenNow,
                ResolutionHoursMedian = median,
                ResolutionHoursN = cnt,
                ExcludedBackdatedCount = a.ExcludedBackdatedCount,
                SatisfactionAvg = satN > 0 ? JsRound(a.SatisfactionScores.Average() * 100) / 100 : null,
                SatisfactionN = satN,
            };
        })
        // ตัวตัดสินรอง (YTD แล้วชื่อ) ให้ผลลัพธ์เดาได้ซ้ำเสมอ
        .OrderByDescending(r => r.ResolvedInPeriod)
        .ThenByDescending(r => r.ResolvedYtd)
        .ThenBy(r => r.AgentName, StringComparer.Ordinal)
        .ToList();

        return new NisWorkloadOverviewDto
        {
            Rows = rows,
            YearBe = now.Year + 543,
            TeamResolvedInPeriod = resolvedCaseNosInPeriod.Count,
            Period = period,
            PeriodFrom = NisServiceCaseDto.Iso(periodStart),
            PeriodTo = NisServiceCaseDto.Iso(now),
        };
    }

    private sealed class Agent
    {
        public int ResolvedInPeriod { get; set; }
        public int ResolvedYtd { get; set; }
        public int OpenNow { get; set; }
        public List<double> ResolvedYtdHours { get; } = new();
        public int ExcludedBackdatedCount { get; set; }
        public List<int> SatisfactionScores { get; } = new();
    }
}
