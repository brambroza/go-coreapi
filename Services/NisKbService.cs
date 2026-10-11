using System.Text.RegularExpressions;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// หน้า *งานที่แจ้งซ้ำ &amp; คู่มือแก้ปัญหา* — port จาก apps/api/src/kb/kb.service.ts
///
/// 🔴 คัดด้วย <c>CreatedAt</c> (งานที่แจ้งเข้า) ไม่ใช่ ResolvedAt — คำถามคือ "เรื่องอะไรถูกแจ้งซ้ำ" เรื่องที่ยังไม่ปิดก็ยังเป็นเรื่องที่แจ้งซ้ำ
/// 🔴 เกณฑ์ "บันทึกวิธีแก้ที่ละเอียดพอ" = 60 ตัวอักษร (คุณ Tone 5 ก.ย. 2569) — ความยาวไม่ใช่ตัวแทนที่ดีของคุณภาพ และเรารู้ตัว
/// ไม่มีพารามิเตอร์ระดับบุคคล · ลงทะเบียน AddScoped ใน Program.cs
/// </summary>
public class NisKbService
{
    /// <summary>เกณฑ์ความยาววิธีแก้ที่ถือว่า "ละเอียดพอ" — ส่งกลับใน meta ให้หน้าจอไม่ต้องพิมพ์เลขซ้ำ</summary>
    public const int DetailedChars = 60;

    /// <summary>จำนวนหัวข้อที่ส่งกลับ — หางยาวของ Pareto แบนจนไม่มีใครอ่าน</summary>
    public const int TopicLimit = 40;

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    private readonly DatabaseContext _db;

    public NisKbService(DatabaseContext db)
    {
        _db = db;
    }

    /// <summary>Math.round ของ JS</summary>
    private static double JsRound(double x) => Math.Floor(x + 0.5);

    /// <summary>หัวข้อที่แจ้งซ้ำ top 40 + ตัวเลขรวม + เปอร์เซ็นต์สะสม (คิดจากทุกหัวข้อ) ในช่วง [from, to] ของ CreatedAt</summary>
    public async Task<NisKbOverviewDto> OverviewAsync(string cmpId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var q = _db.NisServiceCases.AsNoTracking().Where(c => c.CmpId == cmpId && !c.IsTestData);
        if (from.HasValue) q = q.Where(c => c.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(c => c.CreatedAt <= to.Value);

        var cases = await q
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.SourceId)
            .Select(c => new { c.CaseNo, c.SourceId, c.Subject, c.CustomerId, c.ActionDetails, c.ResolutionHours })
            .ToListAsync(ct);

        // กันนับซ้ำด้วยเลขที่เคส — กติกาเดียวกับ NisCustomerGroupService/NisEffortService
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var rows = cases.Where(c => seen.Add(string.IsNullOrEmpty(c.CaseNo) ? c.SourceId : c.CaseNo)).ToList();

        // จัดกลุ่มด้วยรูปที่ตัดช่องว่างซ้ำและตัวพิมพ์ออกแล้ว · แต่แสดงด้วยรูปที่พบบ่อยสุดในกลุ่ม (ถ้อยคำจริงที่เจ้าหน้าที่พิมพ์)
        var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
        var order = new List<string>();
        var detailedTotal = 0;
        foreach (var c in rows)
        {
            var raw = (c.Subject ?? string.Empty).Trim();
            if (raw.Length == 0) continue;
            var key = Whitespace.Replace(raw, " ").ToLowerInvariant();
            if (!groups.TryGetValue(key, out var g))
            {
                groups[key] = g = new Group();
                order.Add(key);
            }
            g.Variants[raw] = (g.Variants.TryGetValue(raw, out var v) ? v : 0) + 1;
            if (!g.VariantOrder.Contains(raw)) g.VariantOrder.Add(raw);
            g.Count++;
            g.Customers.Add(c.CustomerId);
            if (c.ResolutionHours is { } h) { g.Hours += h; g.HoursN++; }
            if ((c.ActionDetails ?? string.Empty).Trim().Length >= DetailedChars) { g.Detailed++; detailedTotal++; }
        }

        var sorted = order.Select(k => groups[k]).OrderByDescending(g => g.Count).ToList();

        // เปอร์เซ็นต์สะสม — คำนวณจากทุกหัวข้อ ไม่ใช่แค่ที่ส่งกลับ ไม่งั้นเส้นจะไม่จบที่ 100
        var total = rows.Count == 0 ? 1 : rows.Count;
        var acc = 0;
        var cumulativePct = sorted.Select(g => { acc += g.Count; return JsRound(acc / (double)total * 1000) / 10; }).ToList();

        var topics = sorted.Take(TopicLimit).Select(g =>
        {
            var display = g.VariantOrder.OrderByDescending(vv => g.Variants[vv]).First();
            return new NisKbTopicRowDto
            {
                Subject = display,
                IntakeCount = g.Count,
                CustomerCount = g.Customers.Count,
                // ไม่มีเคสไหนในกลุ่มที่มีเวลาให้คิด → null ไม่ใช่ 0
                ClockHours = g.HoursN == 0 ? null : JsRound(g.Hours * 10) / 10,
                DetailedCount = g.Detailed,
            };
        }).ToList();

        return new NisKbOverviewDto
        {
            Topics = topics,
            IntakeCount = rows.Count,
            RepeatedTopics = sorted.Count(g => g.Count >= 2),
            DetailedCount = detailedTotal,
            DetailedChars = DetailedChars,
            CumulativePct = cumulativePct,
        };
    }

    private sealed class Group
    {
        public Dictionary<string, int> Variants { get; } = new(StringComparer.Ordinal);
        public List<string> VariantOrder { get; } = new();
        public int Count { get; set; }
        public HashSet<string> Customers { get; } = new(StringComparer.Ordinal);
        public double Hours { get; set; }
        public int HoursN { get; set; }
        public int Detailed { get; set; }
    }
}
