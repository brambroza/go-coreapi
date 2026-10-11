using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// หน้า *เวลาที่ใช้ต่อลูกค้า* — port จาก apps/api/src/effort/effort.service.ts
///
/// 🔴 ตัวเลขในนี้คือเวลานาฬิกา (แจ้ง→บันทึกแก้ล่าสุด) ไม่ใช่เวลาลงมือทำ · ห้ามเอาไปคิดราคาต่อชั่วโมง
/// 🔴 คัดด้วย <c>ResolvedAt</c> เสมอ (หน้านี้พูดถึง**งานที่ปิดได้**เท่านั้น) · ไม่มีพารามิเตอร์ระดับบุคคลโดยตั้งใจ
/// 🔴 กลุ่มลูกค้า/ธง backdated ใช้กติกาเดียวกับ <see cref="NisCustomerGroupService"/> (รหัสรองชี้รหัสหลักชั้นเดียว · ธงที่รหัสหลัก)
///    กลุ่มติดธง: ไม่มีตัวเลขเวลา และ**ตัดออกจากตัวหารของสัดส่วน** · จำนวนเคสยังนับ (categories นับทุกกลุ่ม)
/// ลงทะเบียน AddScoped ใน Program.cs
/// </summary>
public class NisEffortService
{
    /// <summary>ขอบ 6 ช่วง — ต้องตรงกับ NisAnalyticsService.DurationBuckets เป๊ะ (SPEC §9 ข้อ 66)</summary>
    private static readonly double[] Edges = { 1, 4, 8, 24, 72 };
    private static readonly string[] BucketLabels = { "ไม่ถึง 1 ชม.", "1–4 ชม.", "4–8 ชม.", "8–24 ชม.", "1–3 วัน", "เกิน 3 วัน" };

    private readonly DatabaseContext _db;

    public NisEffortService(DatabaseContext db)
    {
        _db = db;
    }

    /// <summary>Math.round ของ JS</summary>
    private static double JsRound(double x) => Math.Floor(x + 0.5);

    /// <summary>ภาพรวมเวลาที่ใช้ต่อกลุ่มลูกค้าในช่วง [from, to] ของ ResolvedAt (ไม่รวมข้อมูลทดสอบ)</summary>
    public async Task<NisEffortOverviewDto> OverviewAsync(string cmpId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var q = _db.NisServiceCases.AsNoTracking().Where(c => c.CmpId == cmpId && !c.IsTestData && c.ResolvedAt != null);
        if (from.HasValue) q = q.Where(c => c.ResolvedAt >= from.Value);
        if (to.HasValue) q = q.Where(c => c.ResolvedAt <= to.Value);

        var rawCases = await q
            .OrderBy(c => c.ResolvedAt).ThenBy(c => c.SourceId)
            .Select(c => new { c.CaseNo, c.SourceId, c.CustomerId, c.CustomerName, c.Category, c.ResolvedAt, c.ResolutionHours })
            .ToListAsync(ct);
        var customerRows = await _db.NisCustomerGroups.AsNoTracking().Where(g => g.CmpId == cmpId).ToListAsync(ct);

        // กันนับซ้ำด้วยเลขที่เคส — กติกาเดียวกับ NisCustomerGroupService/NisWorkloadService
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var cases = rawCases.Where(c => seen.Add(string.IsNullOrEmpty(c.CaseNo) ? c.SourceId : c.CaseNo)).ToList();

        var mergeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var backdatedPrimaries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in customerRows)
        {
            if (!string.IsNullOrWhiteSpace(c.MergeIntoCustomerId)) mergeMap[c.CustomerId] = c.MergeIntoCustomerId!;
            if (c.Backdated) backdatedPrimaries.Add(c.CustomerId);
        }
        string ResolvePrimary(string id) => mergeMap.TryGetValue(id, out var p) ? p : id;

        var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
        var groupOrder = new List<string>();
        var catCount = new Dictionary<string, int>(StringComparer.Ordinal);
        var trendMap = new Dictionary<string, (double Hours, int ResolvedCount)>(StringComparer.Ordinal);
        foreach (var c in cases)
        {
            var primary = ResolvePrimary(c.CustomerId);
            if (!groups.TryGetValue(primary, out var g))
            {
                groups[primary] = g = new Group(primary, backdatedPrimaries.Contains(primary));
                groupOrder.Add(primary);
            }
            g.Names.TryAdd(c.CustomerId, c.CustomerName);
            g.ResolvedCount++;
            // ประเภทงานนับทุกกลุ่ม รวมกลุ่มติดธง — ธงทำให้ตัวเลข*เวลา*เชื่อไม่ได้ ไม่ใช่จำนวนเคส
            var cat = (c.Category ?? string.Empty).Trim();
            if (cat.Length == 0) cat = "ไม่ระบุ";
            catCount[cat] = (catCount.TryGetValue(cat, out var cc) ? cc : 0) + 1;
            if (!g.Backdated && c.ResolvedAt is { } ra)
            {
                // คีย์เดือนตามปฏิทินไทย — เคสที่ปิด 00:00–07:00 น. ต้องอยู่เดือนไทยที่ถูก
                var key = NisBangkokTime.MonthKey(ra);
                var t = trendMap.TryGetValue(key, out var tv) ? tv : (0d, 0);
                t.Item2++;
                if (c.ResolutionHours is { } h) t.Item1 += h;
                trendMap[key] = t;
            }
            // เก็บชั่วโมงไว้ทุกกลุ่มก่อน แล้วตัดตอนสร้างแถว — ให้นับ excludedBackdatedCases ได้
            if (c.ResolutionHours is { } rh) g.Hours.Add(rh);
        }

        // 🔴 ตัวหารของสัดส่วนไม่รวมกลุ่มที่ติดธง (§9 ข้อ 59 · D1)
        var trusted = groups.Values.Where(g => !g.Backdated).ToList();
        var grandTotal = trusted.Sum(g => g.Hours.Sum());
        var allTrustedHours = trusted.SelectMany(g => g.Hours).ToList();

        var rows = groupOrder.Select(k => groups[k]).Select(g =>
        {
            var fallback = g.Names.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value).FirstOrDefault();
            var name = g.Names.TryGetValue(g.Primary, out var n) ? n : fallback ?? g.Primary;
            var total = g.Hours.Sum();
            var (median, _, _, cnt) = NisHoursStat.HoursStat(g.Hours);
            return new NisEffortRowDto
            {
                CustomerId = g.Primary,
                CustomerName = name,
                ResolvedCount = g.ResolvedCount,
                TotalHours = g.Backdated ? null : JsRound(total * 10) / 10,
                MedianHours = g.Backdated ? null : median,
                HoursN = g.Backdated ? 0 : cnt,
                SharePct = g.Backdated || grandTotal == 0 ? null : JsRound(total / grandTotal * 1000) / 10,
                Backdated = g.Backdated,
            };
        }).ToList();

        // เรียงชั่วโมงรวมมาก→น้อย · กลุ่มติดธง (null) ท้ายสุดเสมอ · ตัวตัดสินรองคือชื่อ
        rows.Sort((a, b) =>
        {
            if (a.TotalHours is null && b.TotalHours is null) return string.CompareOrdinal(a.CustomerName, b.CustomerName) < 0 ? -1 : 1;
            if (a.TotalHours is null) return 1;
            if (b.TotalHours is null) return -1;
            var cmp = b.TotalHours.Value.CompareTo(a.TotalHours.Value);
            return cmp != 0 ? cmp : (string.CompareOrdinal(a.CustomerName, b.CustomerName) < 0 ? -1 : 1);
        });

        var trend = trendMap
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new NisEffortTrendPointDto { MonthKey = kv.Key, Hours = JsRound(kv.Value.Hours * 10) / 10, ResolvedCount = kv.Value.ResolvedCount })
            .ToList();

        var categories = catCount
            .Select(kv => new NisEffortCategoryRowDto { Category = kv.Key, ResolvedCount = kv.Value })
            .OrderByDescending(x => x.ResolvedCount).ThenBy(x => x.Category, StringComparer.Ordinal)
            .ToList();

        var counts = new int[BucketLabels.Length];
        foreach (var h in allTrustedHours)
        {
            var i = Array.FindIndex(Edges, e => h < e);
            counts[i == -1 ? BucketLabels.Length - 1 : i]++;
        }
        var distribution = BucketLabels.Select((label, i) => new NisEffortDistributionBucketDto { Label = label, N = counts[i] }).ToList();

        var trustedCases = trusted.Sum(g => g.ResolvedCount);
        return new NisEffortOverviewDto
        {
            Rows = rows,
            Trend = trend,
            // เคสที่เชื่อเวลาได้แต่ไม่มีชั่วโมงให้คิด — ต้องนับไว้ ไม่ปล่อยหาย (§9 ข้อ 78)
            TrustedCasesWithoutHours = trustedCases - allTrustedHours.Count,
            Categories = categories,
            Distribution = distribution,
            ResolvedCount = cases.Count,
            TotalHours = JsRound(grandTotal * 10) / 10,
            MedianHours = NisHoursStat.HoursStat(allTrustedHours).Median,
            CustomerCount = groups.Count,
            ExcludedBackdatedCases = groups.Values.Where(g => g.Backdated).Sum(g => g.ResolvedCount),
        };
    }

    private sealed class Group
    {
        public Group(string primary, bool backdated) { Primary = primary; Backdated = backdated; }
        public string Primary { get; }
        public bool Backdated { get; }
        public Dictionary<string, string> Names { get; } = new(StringComparer.Ordinal);
        public int ResolvedCount { get; set; }
        public List<double> Hours { get; } = new();
    }
}
