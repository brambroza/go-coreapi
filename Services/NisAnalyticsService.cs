using System.Globalization;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// analytics ของ NIS Service Report — port จาก apps/api/src/analytics/analytics.service.ts (1 ต่อ 1 เท่าที่ภาษาอำนวย)
///
/// · แถวทุกชุดดึงผ่าน <see cref="INisArchiveService.BuildWhere"/> (where เดียวกับ /cases) เลือกเฉพาะคอลัมน์ที่ใช้ — **ไม่ลาก RawJson**
///   (ที่นั่นมีชื่อ/อีเมล/เบอร์โทรผู้แจ้ง ไม่มีเหตุผลให้เข้ามาอยู่ในชั้นที่คำนวณตัวเลขสรุป)
/// · คำนวณในหน่วยความจำเหมือน TS (คลังหลักพันแถว) · ทุก bucket/ขอบวันคิดที่เวลาไทยผ่าน <see cref="NisBangkokTime"/>
/// · ตัวนับเคสที่ข้าม (<see cref="Skipped"/>) เป็นตัวแปรต่อคำขอ ห้ามเป็น field — service นี้เป็น scoped แต่กติกาเดิมยังใช้
/// · การปัดเลขใช้ <see cref="JsRound"/> (= Math.round ของ JS) เพื่อให้ตัวเลขตรงกับระบบเดิมเป๊ะตอนเทียบ (MERGE_PLAN slice 10)
///
/// ลงทะเบียน AddScoped ใน Program.cs (Autofac scan ก็ register เป็น interface ให้ด้วย — ซ้ำไม่เป็นไร)
/// </summary>
public class NisAnalyticsService : INisAnalyticsService
{
    private static readonly string[] ThaiMonths = { "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };

    private readonly DatabaseContext _db;
    private readonly INisArchiveService _archive;
    private readonly NisCustomerGroupResolverService _groups;
    private readonly NisBusinessHoursConfig _bizBase;

    /// <inheritdoc />
    public bool SlaEnabled { get; }

    public NisAnalyticsService(
        DatabaseContext db,
        INisArchiveService archive,
        NisCustomerGroupResolverService groups,
        IConfiguration configuration)
    {
        _db = db;
        _archive = archive;
        _groups = groups;
        _bizBase = NisBusinessHoursConfig.FromConfiguration(configuration);
        // 🔴 สวิตช์ SLA ไม่มีค่าเริ่มต้นเป็นเปิด — อ่านจาก config ทุกครั้ง (SPEC §9 ข้อ 65)
        SlaEnabled = configuration.GetValue<bool>("NisReport:SlaEnabled");
    }

    // ────────────────────────────────────────────────────────────────────
    // พื้นฐานที่ทุก endpoint ใช้ร่วมกัน
    // ────────────────────────────────────────────────────────────────────

    /// <summary>เฉพาะคอลัมน์ที่ analytics ใช้ (AnalyticsRow ใน TS)</summary>
    private sealed record Row(
        string SourceId,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ResolvedAt,
        double? ResolutionHours,
        string Priority,
        string Status,
        string Category,
        string CustomerId,
        string CustomerName,
        int? SatisfactionScore);

    /// <summary>ช่วงเวลาที่ตีความเป็นเวลาไทยแล้ว</summary>
    private sealed record Range(DateTimeOffset? From, DateTimeOffset? To);

    /// <summary>ตัวนับเคสที่คำนวณเวลาทำการไม่ได้ — Set ของ sourceId นับไม่ซ้ำ · ต่อคำขอเท่านั้น</summary>
    private sealed class Skipped
    {
        public HashSet<string> Ids { get; } = new(StringComparer.Ordinal);
    }

    /// <summary>คอนฟิกเวลาทำการ + ความน่าเชื่อถือของตาราง holiday สำหรับปีที่ช่วงคร่อม</summary>
    private sealed record BizSet(NisBusinessHoursConfig Config, string? Warning, bool NumbersBlocked);

    /// <summary>
    /// Math.round ของ JS (ปัดครึ่งขึ้นไปทาง +∞) — ไม่ใช่ banker's rounding ของ C#
    /// ใช้ทุกจุดที่ TS เขียน Math.round เพื่อให้ตัวเลขเทียบกับระบบเดิมได้ตรงตัว
    /// </summary>
    private static double JsRound(double x) => Math.Floor(x + 0.5);

    /// <summary>สำเนาตัวกรองที่กำหนด dateField และแทน customerId ด้วยรหัสสมาชิกกลุ่ม (ถ้าส่งมา)</summary>
    private static NisCaseFilter With(NisCaseFilter f, string dateField, IReadOnlyList<string>? customerIds) => new()
    {
        From = f.From,
        To = f.To,
        DateField = dateField,
        CustomerId = customerIds is null ? f.CustomerId : customerIds.ToList(),
        Assignee = f.Assignee,
        Status = f.Status,
        Priority = f.Priority,
        Category = f.Category,
        Q = f.Q,
        IncludeTest = f.IncludeTest,
        Unresolved = f.Unresolved,
        MaxSatisfaction = f.MaxSatisfaction,
    };

    /// <summary>ดึงแถวที่ตรงเงื่อนไข — ผ่าน BuildWhere เสมอ เรียงตาม createdAt</summary>
    private Task<List<Row>> RowsAsync(string cmpId, NisCaseFilter f, string dateField, IReadOnlyList<string>? customerIds, CancellationToken ct) =>
        _archive.BuildWhere(cmpId, With(f, dateField, customerIds))
            .OrderBy(c => c.CreatedAt)
            .Select(c => new Row(
                c.SourceId, c.CreatedAt, c.ResolvedAt, c.ResolutionHours, c.Priority, c.Status,
                c.Category, c.CustomerId, c.CustomerName, c.SatisfactionScore))
            .ToListAsync(ct);

    /// <summary>
    /// ล้าง resolutionHours ของแถวที่กลุ่มลูกค้าติดธงบันทึกย้อนหลัง (MERGE_PLAN §0 ข้อ 12) — จำนวนเคสยังนับ
    /// แต่ไม่เข้าสถิติเวลา/SLA · สถิติเวลาทำการใช้ ResolvedAt ที่ถูกล้างด้วย จึงไม่เข้าเช่นกัน
    /// (ต่างจาก analytics.service.ts เดิมที่ไม่ล้าง — การล้างอยู่ที่ cases.service เท่านั้น · ตั้งใจ)
    /// </summary>
    private static List<Row> ClearBackdated(List<Row> rows, HashSet<string> backdated)
    {
        if (backdated.Count == 0) return rows;
        return rows.Select(r => backdated.Contains(r.CustomerId) ? r with { ResolutionHours = null, ResolvedAt = null } : r).ToList();
    }

    private static Range RangeOf(NisCaseFilter f) => new(f.FromAt, f.ToAt);

    /// <summary>
    /// ช่วงก่อนหน้าสำหรับเทียบ MoM — ไม่ได้ระบุช่วง = เทียบไม่ได้ (null) ไม่เดาให้
    /// เดือนปฏิทินเต็มเดือน → เดือนก่อนหน้าเต็มเดือน (ช่วงยาวเท่ากันจะกินวันสุดท้ายของเดือนก่อนนั้นเข้ามาเมื่อ 31 วันตามหลัง 30 วัน) ·
    /// อื่น ๆ → ช่วงยาวเท่ากันถัดไปข้างหน้า และบอก mode กลับ
    /// </summary>
    private static (Range Range, string Mode)? PreviousRange(Range r)
    {
        if (r.From is null || r.To is null) return null;
        var a = NisBangkokTime.ToBangkok(r.From.Value);
        var b = NisBangkokTime.ToBangkok(r.To.Value);

        var isFullMonth =
            a.Day == 1 && a.Hour == 0 && a.Minute == 0 &&
            b.Month == a.Month && b.Year == a.Year &&
            b.Day == DateTime.DaysInMonth(b.Year, b.Month) &&
            // ต้องเป็นปลายวันจริง ไม่ใช่แค่ชั่วโมง 23 — to=…T23:00 ขาดไปหนึ่งชั่วโมง ป้าย "เทียบเดือนก่อน" จะโกหก
            b.Hour == 23 && b.Minute == 59;

        if (isFullMonth)
        {
            var y = a.Month == 1 ? a.Year - 1 : a.Year;
            var m = a.Month == 1 ? 12 : a.Month - 1;
            var last = DateTime.DaysInMonth(y, m);
            var from = NisBangkokTime.StartOfDay($"{y:0000}-{m:00}-01");
            var to = NisBangkokTime.EndOfDay($"{y:0000}-{m:00}-{last:00}");
            return (new Range(from, to), "calendar-month");
        }

        var span = r.To.Value - r.From.Value;
        return (new Range(r.From.Value - span - TimeSpan.FromMilliseconds(1), r.From.Value - TimeSpan.FromMilliseconds(1)), "equal-span");
    }

    /// <summary>
    /// ปีที่ช่วงคร่อม (เวลาไทย) — ไม่ระบุช่วง = ปีปัจจุบันเป็น fallback เดียว (SPEC §9 ข้อ 67 พูดถึง "ปีที่รายงานคร่อม")
    /// </summary>
    private static IReadOnlyList<int> YearsInRange(DateTimeOffset? from, DateTimeOffset? to) =>
        from is null || to is null
            ? new[] { NisBangkokTime.Now().Year }
            : NisBusinessHours.YearsInRange(from.Value, to.Value);

    /// <summary>
    /// คอนฟิกเวลาทำการ (หักวันหยุด Observed ทุกปี) + คำเตือน + ธงบล็อก สำหรับชุดปีที่ส่งมา
    /// numbersBlocked = true ถ้า**ปีใดปีหนึ่ง**ยังยืนยันไม่ครบ (รายงานที่คร่อมสองปีต้องตันทั้งคู่) · warning = ตัวแรกที่ไม่ null
    /// </summary>
    private async Task<BizSet> BusinessHoursAsync(string cmpId, IReadOnlyList<int> years, CancellationToken ct)
    {
        var holidays = await _db.NisHolidays.AsNoTracking().Where(h => h.CmpId == cmpId).ToListAsync(ct);
        var sets = years.Select(y => NisBusinessHours.Build(_bizBase, holidays, y)).ToList();
        return new BizSet(
            sets[0].Config,
            sets.Select(s => s.Warning).FirstOrDefault(w => w != null),
            sets.Any(s => s.NumbersBlocked));
    }

    /// <summary>ชั่วโมงทำการของเคสหนึ่งใบ — กันเคสเพี้ยนใบเดียว (ช่วงเกิน 2 ปี) ล้มทั้ง endpoint · นับใน skipped ไม่กลืนเงียบ</summary>
    private static double? SafeBusinessHours(Row c, NisBusinessHoursConfig cfg, Skipped skipped)
    {
        if (c.ResolvedAt is null) return null;
        try
        {
            return NisBusinessHours.BusinessHoursBetween(c.CreatedAt, c.ResolvedAt.Value, cfg);
        }
        catch (InvalidOperationException)
        {
            skipped.Ids.Add(c.SourceId);
            return null;
        }
    }

    /// <summary>คะแนนความพึงพอใจ — กรองที่ไม่ใช่ 1–5 ออก (ต้นทางส่ง 9 = ยังไม่ให้คะแนน)</summary>
    private static NisSatisfactionDto Satisfaction(List<Row> rows)
    {
        var scores = rows.Where(c => c.SatisfactionScore is >= 1 and <= 5).Select(c => (double)c.SatisfactionScore!.Value).ToList();
        return new NisSatisfactionDto
        {
            Avg = scores.Count > 0 ? JsRound(scores.Average() * 100) / 100 : null,
            Rated = scores.Count,
            NotRated = rows.Count - scores.Count,
        };
    }

    /// <summary>
    /// SLA สองวิธีนับ · เคสที่ยังไม่ปิด = ยังตัดสินไม่ได้ ห้ามนับเป็นตก · สวิตช์ปิดหรือถูกบล็อกวันหยุด → within/breached/pct เป็น null แต่ judged ยังจริง
    /// targets ถูกกลบด้วยเมื่อสวิตช์ปิด — ตัวเลขเป้าหมายคือค่าสมมติ ห้ามหลุดไปวาดเส้นบนกราฟ
    /// </summary>
    private NisSlaBlockDto Sla(List<Row> rows, NisBusinessHoursConfig cfg, Skipped skipped, bool holidayBlocked)
    {
        var enabled = SlaEnabled;
        var targets = NisAnalyticsText.DefaultResolutionTargetHours;

        NisSlaSideDto Tally(Func<Row, double?> hoursOf, bool blocked)
        {
            int within = 0, breached = 0;
            foreach (var c in rows)
            {
                var h = hoursOf(c);
                if (h is null) continue;
                if (!targets.TryGetValue(c.Priority, out var target)) continue;
                if (h.Value <= target) within++; else breached++;
            }
            var total = within + breached;
            if (!enabled || blocked) return new NisSlaSideDto { Within = null, Breached = null, Pct = null, Judged = total };
            return new NisSlaSideDto
            {
                Within = within,
                Breached = breached,
                Pct = total > 0 ? JsRound(within / (double)total * 1000) / 10 : null,
                Judged = total,
            };
        }

        return new NisSlaBlockDto
        {
            Enabled = enabled,
            Targets = enabled ? targets.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal) : null,
            Business = Tally(c => SafeBusinessHours(c, cfg, skipped), holidayBlocked),
            Clock = Tally(c => c.ResolutionHours, false),
            Note = enabled
                ? holidayBlocked
                    ? "คอลัมน์ SLA เวลาทำการยังไม่แสดง — วันหยุดของปีนี้ยังยืนยันไม่ครบ (SPEC §9 ข้อ 67) เกณฑ์นี้ยังเป็นค่าสมมติด้วย ยังไม่ยืนยันกับสัญญา (คำถาม C1/C2)"
                    : "เกณฑ์นี้ยังเป็นค่าสมมติ ยังไม่ยืนยันกับสัญญา (คำถาม C1/C2)"
                : "สวิตช์ SLA ปิดอยู่ (ค่าเริ่มต้น) — ยังไม่มีเกณฑ์จริงจากสัญญา ทั้งตัวเลขและเกณฑ์จึงถูกกลบไว้ · " +
                  "วิธีนับที่ระบบใช้คือ **เวลาทำการ 08:30–17:30 จ–ศ หักวันหยุด** (คุณ Tone ตัดสิน 5 ก.ย. 2569) " +
                  "เหลือแค่ตัวเลขเกณฑ์ 4 ระดับที่ยังต้องเปิดสัญญาหา · เปิดด้วย SLA_ENABLED=true เมื่อได้เกณฑ์จริงแล้วเท่านั้น",
        };
    }

    // ────────────────────────────────────────────────────────────────────
    // summary
    // ────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<NisSummaryDto> SummaryAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default) =>
        SummaryAsync(cmpId, query.ToFilter(), null, ct);

    /// <inheritdoc />
    public async Task<NisSummaryDto> SummaryAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, CancellationToken ct = default)
    {
        var skipped = new Skipped();
        var r = RangeOf(filter);
        var prev = PreviousRange(r);

        // ปีที่ต้องเทียบ — รวมช่วงปัจจุบันและช่วงก่อนหน้า ให้ "ตันพร้อมกันทั้งคู่" ไม่งั้น MoM % เทียบข้ามสถานะที่ไม่เท่ากัน
        var years = YearsInRange(r.From, r.To)
            .Concat(prev is { } p ? YearsInRange(p.Range.From, p.Range.To) : Array.Empty<int>())
            .Distinct().ToList();
        var biz = await BusinessHoursAsync(cmpId, years, ct);
        var backdated = await _groups.BackdatedCustomerIdsAsync(cmpId, ct);

        var current = await BlockAsync(cmpId, filter, customerIds, biz, backdated, skipped, ct);
        // ตัวนับของช่วงปัจจุบัน — คัดลอกไว้ก่อนคำนวณช่วงก่อนหน้า ไม่งั้นสองช่วงปนกัน
        var skippedCurrent = skipped.Ids.Count;

        NisSummaryBlockDto? previous = null;
        if (prev is { } pr)
        {
            var prevFilter = With(filter, filter.DateField, null);
            prevFilter.From = NisServiceCaseDto.Iso(pr.Range.From!.Value);
            prevFilter.To = NisServiceCaseDto.Iso(pr.Range.To!.Value);
            previous = await BlockAsync(cmpId, prevFilter, customerIds, biz, backdated, skipped, ct);
        }

        return new NisSummaryDto
        {
            Current = current,
            Previous = previous,
            Mom = previous is null ? null : new NisMomBlockDto { Mode = prev?.Mode, Kpis = Mom(current, previous) },
            MomNote = prev?.Mode == "calendar-month"
                ? "เทียบกับเดือนปฏิทินก่อนหน้าเต็มเดือน"
                : prev is not null
                    ? "ช่วงที่ขอไม่ใช่เดือนเต็ม จึงเทียบกับช่วงก่อนหน้าที่ยาวเท่ากัน ไม่ใช่เดือนปฏิทิน"
                    : "ไม่ได้ระบุช่วงเวลา จึงเทียบกับช่วงก่อนหน้าไม่ได้",
            HoursNote = NisAnalyticsText.HoursNote,
            SkippedRows = skippedCurrent,
            SkippedRowsPrevious = previous is null ? null : skipped.Ids.Count - skippedCurrent,
            HolidayWarning = biz.Warning,
            NumbersBlocked = biz.NumbersBlocked,
        };
    }

    /// <summary>KPI หนึ่งก้อน (ช่วงปัจจุบันหรือก่อนหน้า) — intake นับ createdAt · resolved นับ resolvedAt · spill นับจากเคสจริง</summary>
    private async Task<NisSummaryBlockDto> BlockAsync(
        string cmpId, NisCaseFilter f, IReadOnlyList<string>? customerIds, BizSet biz, HashSet<string> backdated, Skipped skipped, CancellationToken ct)
    {
        // แถวดิบ (ก่อนล้าง backdated) ใช้กับส่วนต่าง — "ยังไม่ปิด" เป็นข้อเท็จจริงของเคส ไม่เกี่ยวกับความเชื่อถือของเวลา
        var intakeRaw = await RowsAsync(cmpId, f, "createdAt", customerIds, ct);
        var resolvedRaw = await RowsAsync(cmpId, f, "resolvedAt", customerIds, ct);
        var intake = ClearBackdated(intakeRaw, backdated);
        var resolved = ClearBackdated(resolvedRaw, backdated);
        var rr = RangeOf(f);

        var off = new NisOffHoursDto();
        foreach (var c in intake)
        {
            var kind = NisBusinessHours.ClassifyOffHours(c.CreatedAt, biz.Config);
            if (kind is null) continue;
            off.Total++;
            switch (kind)
            {
                case NisOffHoursKind.Before: off.Before++; break;
                case NisOffHoursKind.After: off.After++; break;
                case NisOffHoursKind.Weekend: off.Weekend++; break;
                case NisOffHoursKind.Holiday: off.Holiday++; break;
            }
        }

        // 🔴 ส่วนต่างต้องนับจากเคสจริง ไม่ใช่เอาสองก้อนมาลบกัน (สองก้อนเป็นคนละประชากร)
        var spill = rr.From is not null && rr.To is not null
            ? new NisSpillDto
            {
                CarriedIn = resolvedRaw.Count(c => c.CreatedAt < rr.From.Value),
                NotClosedInPeriod = intakeRaw.Count(c => c.ResolvedAt is null || c.ResolvedAt > rr.To.Value),
                StillOpen = intakeRaw.Count(c => c.ResolvedAt is null),
                Note = "นับจากเคสจริง ไม่ใช่ผลลบของสองก้อน — สองก้อนเป็นคนละประชากร",
            }
            : new NisSpillDto
            {
                CarriedIn = null,
                NotClosedInPeriod = null,
                StillOpen = intakeRaw.Count(c => c.ResolvedAt is null),
                Note = "ไม่ได้ระบุช่วงเวลา จึงไม่มี \"ก่อน/หลังช่วง\" ให้เทียบ",
            };

        var businessStat = NisHourStatDto.From(resolved.Select(c => SafeBusinessHours(c, biz.Config, skipped)).Where(h => h.HasValue).Select(h => h!.Value));
        return new NisSummaryBlockDto
        {
            Spill = spill,
            Intake = new NisIntakeBlockDto
            {
                Label = NisAnalyticsText.IntakeLabel,
                Count = intake.Count,
                Customers = intake.Select(c => c.CustomerId).Distinct(StringComparer.Ordinal).Count(),
                OffHours = biz.NumbersBlocked ? null : off,
            },
            Resolved = new NisResolvedBlockDto
            {
                Label = NisAnalyticsText.ResolvedLabel,
                Count = resolved.Count,
                Customers = resolved.Select(c => c.CustomerId).Distinct(StringComparer.Ordinal).Count(),
                // 🔴 เวลานาฬิกา (แจ้ง→ปิด) ไม่ใช่เวลาลงมือทำ · ห้ามเอาไปคิดราคาต่อชั่วโมง
                ClockHours = NisHourStatDto.From(resolved.Where(c => c.ResolutionHours.HasValue).Select(c => c.ResolutionHours!.Value)),
                // SPEC §9 ข้อ 67 — n ยังเป็นจำนวนจริง (เหมือน judged ของ SLA) มีแค่ค่าสถิติที่ถูกกลบ
                BusinessHours = biz.NumbersBlocked
                    ? new NisHourStatDto { Median = null, Mean = null, P90 = null, N = businessStat.N }
                    : businessStat,
                Satisfaction = Satisfaction(resolved),
                Sla = Sla(resolved, biz.Config, skipped, biz.NumbersBlocked),
            },
        };
    }

    /// <summary>
    /// เทียบกับช่วงก่อนหน้า — ทุก KPI บนการ์ด 13 ตัว (ไม่รวม sla.* และ skippedRows) · baseUnit คือหน่วยของตัวเลขดิบ
    /// ฐานเป็น 0 หรือไม่มีค่า → pct เป็น null (ดีกว่า 0 หรือ Infinity)
    /// </summary>
    private static Dictionary<string, NisMomEntryDto> Mom(NisSummaryBlockDto cur, NisSummaryBlockDto prev)
    {
        static NisMomEntryDto Delta(double? a, double? b, string unit) => new()
        {
            Current = a,
            Previous = b,
            Diff = a is null || b is null ? null : JsRound((a.Value - b.Value) * 1000) / 1000,
            Pct = a is null || b is null || b.Value == 0 ? null : JsRound((a.Value - b.Value) / b.Value * 1000) / 10,
            BaseUnit = unit,
        };

        var paths = new (string Key, Func<NisSummaryBlockDto, double?> At, string Unit)[]
        {
            ("intakeCount", o => o.Intake.Count, "เคส"),
            ("intakeCustomers", o => o.Intake.Customers, "ราย"),
            ("intakeOffHours", o => o.Intake.OffHours?.Total, "เคส"),
            ("resolvedCount", o => o.Resolved.Count, "เคส"),
            ("resolvedCustomers", o => o.Resolved.Customers, "ราย"),
            ("clockHoursMedian", o => o.Resolved.ClockHours.Median, "ชม."),
            ("clockHoursMean", o => o.Resolved.ClockHours.Mean, "ชม."),
            ("clockHoursP90", o => o.Resolved.ClockHours.P90, "ชม."),
            ("businessHoursMedian", o => o.Resolved.BusinessHours.Median, "ชม.ทำการ"),
            ("businessHoursMean", o => o.Resolved.BusinessHours.Mean, "ชม.ทำการ"),
            ("businessHoursP90", o => o.Resolved.BusinessHours.P90, "ชม.ทำการ"),
            ("satisfactionAvg", o => o.Resolved.Satisfaction.Avg, "คะแนน"),
            ("satisfactionRated", o => o.Resolved.Satisfaction.Rated, "เคส"),
        };

        var out_ = new Dictionary<string, NisMomEntryDto>(StringComparer.Ordinal);
        foreach (var (key, at, unit) in paths) out_[key] = Delta(at(cur), at(prev), unit);
        return out_;
    }

    // ────────────────────────────────────────────────────────────────────
    // trend
    // ────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<NisTrendDto> TrendAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default) =>
        TrendAsync(cmpId, query.ToFilter(), null, query.Bucket, ct);

    /// <inheritdoc />
    public async Task<NisTrendDto> TrendAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, string bucket = "auto", CancellationToken ct = default)
    {
        var r = RangeOf(filter);
        var b = ResolveBucket(bucket, r);
        var intake = await RowsAsync(cmpId, filter, "createdAt", customerIds, ct);
        var resolved = await RowsAsync(cmpId, filter, "resolvedAt", customerIds, ct);

        string KeyOf(DateTimeOffset d) => b switch
        {
            "week" => NisBangkokTime.WeekKey(d),
            "month" => NisBangkokTime.MonthKey(d),
            _ => NisBangkokTime.DateKey(d),
        };

        var map = new Dictionary<string, NisTrendRowDto>(StringComparer.Ordinal);
        void Bump(string k, bool isIntake)
        {
            if (!map.TryGetValue(k, out var row)) map[k] = row = new NisTrendRowDto { Key = k };
            if (isIntake) row.Intake++; else row.Resolved++;
        }
        foreach (var c in intake) Bump(KeyOf(c.CreatedAt), true);
        foreach (var c in resolved) if (c.ResolvedAt is { } ra) Bump(KeyOf(ra), false);

        // ขอบเขตข้อมูลจริงใน**ทั้งคลัง** (ไม่กรองมิติ) — ใช้เติมช่องว่างและติดธง partial · ใช้ includeTest เดียวกับข้อมูล
        var archiveEdge = await EdgeAsync(cmpId, new NisCaseFilter { IncludeTest = filter.IncludeTest, DateField = "createdAt" }, ct);
        FillGaps(map, b, r, archiveEdge.Min, archiveEdge.Max);

        var rows = map.Values.OrderBy(x => x.Key, StringComparer.Ordinal).ToList();
        foreach (var row in rows)
        {
            // เดือนที่ลูกค้าไม่มีเคสเลย ต้องไม่ติดธง (คุณ Tone 9 ก.ย. 2569) — ทั้ง "คลังเก็บไม่เต็ม bucket" และ "ชุดนี้มีเคสจริง" พร้อมกัน
            row.Partial = row.Intake + row.Resolved > 0 && IsPartial(row.Key, b, archiveEdge.Min, archiveEdge.Max);
        }

        // ข้อความ partialNote ใช้ขอบเขตของ "ข้อมูลที่กรองแล้วจริง ๆ" (noteEdge) เมื่อมีตัวกรองมิติ
        var effectiveCustomerIds = customerIds ?? filter.CustomerId;
        var hasDimensionFilter =
            effectiveCustomerIds is { Count: > 0 } || filter.Status is { Count: > 0 } ||
            filter.Priority is { Count: > 0 } || filter.Category is { Count: > 0 };
        var noteEdge = hasDimensionFilter
            ? await EdgeAsync(cmpId, new NisCaseFilter
            {
                CustomerId = effectiveCustomerIds?.ToList(),
                Status = filter.Status,
                Priority = filter.Priority,
                Category = filter.Category,
                IncludeTest = filter.IncludeTest,
                DateField = "createdAt",
            }, ct)
            : archiveEdge;

        var viewMinKey = noteEdge.Min is { } nmin ? KeyOf(nmin) : null;
        var viewMaxKey = noteEdge.Max is { } nmax ? KeyOf(nmax) : null;
        var archiveMaxKey = archiveEdge.Max is { } amax ? KeyOf(amax) : null;
        var firstPartialRow = viewMinKey is null ? null : rows.FirstOrDefault(x => x.Key == viewMinKey);
        // ต้องเป็นทั้ง "ช่วงที่คลังหยุด" และ "ช่วงที่ข้อมูลชุดนี้ยังมีอยู่" พร้อมกัน — ลูกค้าที่เลิกแจ้งงานต้องไม่เข้าเงื่อนไข
        var lastPartialRow = archiveMaxKey is not null && archiveMaxKey == viewMaxKey && archiveMaxKey != viewMinKey
            ? rows.FirstOrDefault(x => x.Key == archiveMaxKey)
            : null;

        var notes = new List<string>();
        if (firstPartialRow is not null)
        {
            notes.Add(
                $"ช่วงแรกที่ข้อมูลชุดนี้มีงาน ({BucketPeriodLabel(firstPartialRow.Key, b)}) มีงานที่แจ้งเข้า {firstPartialRow.Intake} เคส " +
                "— เป็นช่วงที่งานชิ้นแรกของชุดนี้ปรากฏ ไม่ใช่ช่วงที่ครอบคลุมเต็มช่วงเสมอไป " +
                "จึงไม่ควรอ่านความสูงของแท่งนี้เป็นการเปลี่ยนแปลงของปริมาณงาน");
        }
        if (lastPartialRow is not null)
        {
            notes.Add(
                $"ช่วงล่าสุดที่คลังมีข้อมูล ({BucketPeriodLabel(lastPartialRow.Key, b)}) อาจยังไม่ครบเต็มช่วง เพราะเป็นข้อมูลล่าสุดเท่าที่ " +
                "คลังเก็บได้ตอนนี้ ไม่ได้แปลว่าช่วงนั้นจบแล้วจริง — ตัวเลขอาจยังขยับขึ้นต่อเมื่อ sync รอบถัดไป " +
                "เก็บเพิ่ม ไม่ควรเทียบกับช่วงก่อนหน้าที่ปิดแล้ว");
        }

        return new NisTrendDto
        {
            Bucket = b,
            IntakeLabel = NisAnalyticsText.IntakeLabel,
            ResolvedLabel = NisAnalyticsText.ResolvedLabel,
            Rows = rows,
            PartialNote = notes.Count > 0 ? string.Join(" / ", notes) : null,
        };
    }

    /// <summary>min/max ของ createdAt ตามตัวกรอง (aggregate ของ TS)</summary>
    private async Task<(DateTimeOffset? Min, DateTimeOffset? Max)> EdgeAsync(string cmpId, NisCaseFilter f, CancellationToken ct)
    {
        var q = _archive.BuildWhere(cmpId, f);
        var min = await q.MinAsync(c => (DateTimeOffset?)c.CreatedAt, ct);
        var max = await q.MaxAsync(c => (DateTimeOffset?)c.CreatedAt, ct);
        return (min, max);
    }

    /// <summary>ความละเอียด — auto: ≤62 วัน day · ≤270 วัน week · เกินนั้น month · ไม่ระบุช่วง = month</summary>
    private static string ResolveBucket(string bucket, Range r)
    {
        if (bucket != "auto") return bucket;
        if (r.From is null || r.To is null) return "month";
        var days = (r.To.Value - r.From.Value).TotalDays;
        if (days <= 62) return "day";
        return days <= 270 ? "week" : "month";
    }

    /// <summary>
    /// เติม bucket ที่ไม่มีเคสเลยให้เป็นศูนย์ — ขอบเขต = ช่วงที่ขอ ตัดด้วยขอบเขตข้อมูลที่มีจริง (ไม่เติมย้อนก่อนวันที่คลังเริ่มเก็บ)
    /// เดินทีละวัน/สัปดาห์/เดือนตามปฏิทินไทยจากคีย์ที่ปัดแล้ว (ไทยไม่มี DST — ก้าวปฏิทินเท่ากับก้าวเวลาจริงเสมอ)
    /// </summary>
    private static void FillGaps(Dictionary<string, NisTrendRowDto> map, string bucket, Range r, DateTimeOffset? min, DateTimeOffset? max)
    {
        if (min is null || max is null) return;
        var lo = r.From is { } f && f > min.Value ? f : min.Value;
        var hi = r.To is { } t && t < max.Value ? t : max.Value;
        if (lo > hi) return;

        var keys = new List<string>();
        if (bucket == "day")
        {
            var d = DateOnly.ParseExact(NisBangkokTime.DateKey(lo), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = DateOnly.ParseExact(NisBangkokTime.DateKey(hi), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            for (; d <= end; d = d.AddDays(1)) keys.Add(d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
        else if (bucket == "week")
        {
            var d = DateOnly.ParseExact(NisBangkokTime.WeekKey(lo), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = DateOnly.ParseExact(NisBangkokTime.WeekKey(hi), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            for (; d <= end; d = d.AddDays(7)) keys.Add(d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
        else
        {
            var a = NisBangkokTime.ToBangkok(lo);
            var bb = NisBangkokTime.ToBangkok(hi);
            for (int y = a.Year, m = a.Month; y < bb.Year || (y == bb.Year && m <= bb.Month);)
            {
                keys.Add($"{y:0000}-{m:00}");
                m++;
                if (m > 12) { m = 1; y++; }
            }
        }
        foreach (var k in keys)
            if (!map.ContainsKey(k)) map[k] = new NisTrendRowDto { Key = k };
    }

    /// <summary>bucket นี้เป็น bucket ขอบของ**คลัง**ไหม (คลังเริ่มเก็บกลาง bucket แรก · bucket ล่าสุดยังไม่จบ) — ตอบแค่เรื่องคลัง</summary>
    private static bool IsPartial(string key, string bucket, DateTimeOffset? min, DateTimeOffset? max)
    {
        if (min is null || max is null) return false;
        return bucket switch
        {
            "week" => key == NisBangkokTime.WeekKey(min.Value) || key == NisBangkokTime.WeekKey(max.Value),
            "day" => key == NisBangkokTime.DateKey(min.Value) || key == NisBangkokTime.DateKey(max.Value),
            _ => key == NisBangkokTime.MonthKey(min.Value) || key == NisBangkokTime.MonthKey(max.Value),
        };
    }

    /// <summary>วันที่ไทยสำหรับคนอ่าน "20 ม.ค. 2569" — เขียนเองไม่พึ่ง culture ของเครื่อง</summary>
    private static string ThaiDate(DateTimeOffset d)
    {
        var bkk = NisBangkokTime.ToBangkok(d);
        return $"{bkk.Day} {ThaiMonths[bkk.Month - 1]} {bkk.Year + 543}";
    }

    /// <summary>ป้ายของ bucket จาก**คีย์ของแถวนั้นตรง ๆ** (ไม่ใช่วันที่ดิบของเคสแรก/สุดท้าย) — คนอ่านต้องหาแท่งบนกราฟเจอ</summary>
    private static string BucketDateLabel(string key, string bucket)
    {
        if (bucket == "month")
        {
            var parts = key.Split('-');
            var y = int.Parse(parts[0], CultureInfo.InvariantCulture);
            var m = int.Parse(parts[1], CultureInfo.InvariantCulture);
            return $"เดือน {ThaiMonths[m - 1]} {y + 543}";
        }
        return ThaiDate(NisBangkokTime.StartOfDay(key));
    }

    /// <summary>ป้ายในวงเล็บของ partialNote — week ต้องบอกว่าเป็นจุดเริ่มของสัปดาห์ ไม่ใช่ยอดของวันเดียว</summary>
    private static string BucketPeriodLabel(string key, string bucket) =>
        bucket == "week" ? $"สัปดาห์ที่เริ่ม {BucketDateLabel(key, bucket)}" : BucketDateLabel(key, bucket);

    // ────────────────────────────────────────────────────────────────────
    // breakdown
    // ────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<NisBreakdownDto> BreakdownAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default) =>
        BreakdownAsync(cmpId, query.ToFilter(), null, query.By, ct);

    /// <inheritdoc />
    public async Task<NisBreakdownDto> BreakdownAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, string by, CancellationToken ct = default)
    {
        var r = RangeOf(filter);
        var biz = await BusinessHoursAsync(cmpId, YearsInRange(r.From, r.To), ct);
        var skipped = new Skipped();
        var intake = await RowsAsync(cmpId, filter, "createdAt", customerIds, ct);
        var resolved = await RowsAsync(cmpId, filter, "resolvedAt", customerIds, ct);

        if (by == "duration")
        {
            var backdated = await _groups.BackdatedCustomerIdsAsync(cmpId, ct);
            var bucketRows = DurationBuckets(ClearBackdated(resolved, backdated), biz.Config, skipped);
            // SPEC §9 ข้อ 67 — คอลัมน์ business ต้องไม่แสดงจนกว่าวันหยุดจะยืนยันครบ (clock ไม่กระทบ)
            if (biz.NumbersBlocked) foreach (var row in bucketRows) row.Business = null;
            return new NisDurationBreakdownDto
            {
                By = by,
                CountLabel = NisAnalyticsText.ResolvedLabel,
                HoursNote = NisAnalyticsText.HoursNote,
                HolidayWarning = biz.Warning,
                Rows = bucketRows,
                // อ่าน skipped **หลัง** คำนวณ rows — ไม่งั้นได้ 0 ทั้งที่ทิ้งเคสไปจริง
                SkippedRows = skipped.Ids.Count,
            };
        }

        (string Key, string Label) KeyOf(Row c) => by switch
        {
            "priority" => (c.Priority, NisAnalyticsText.PriorityLabelTh.TryGetValue(c.Priority, out var pl) ? pl : c.Priority),
            "status" => (c.Status, NisAnalyticsText.StatusLabelTh.TryGetValue(c.Status, out var sl) ? sl : c.Status),
            "customer" => (c.CustomerId, c.CustomerName),
            // '(ไม่ระบุ)' เป็นสตริงเซนทิเนล — overview.tsx ฝั่ง CRM กรองค่านี้ออกจาก dropdown ด้วยสตริงเดียวกัน ถ้าแก้ต้องแก้ทั้งคู่
            _ => (string.IsNullOrEmpty(c.Category) ? "(ไม่ระบุ)" : c.Category, string.IsNullOrEmpty(c.Category) ? "ไม่ระบุประเภท" : c.Category),
        };

        var map = new Dictionary<string, NisBreakdownRowDto>(StringComparer.Ordinal);
        var order = new List<string>();
        void Bump(Row c, bool isIntake)
        {
            var (key, label) = KeyOf(c);
            if (!map.TryGetValue(key, out var row))
            {
                map[key] = row = new NisBreakdownRowDto { Key = key, Label = label };
                order.Add(key);
            }
            if (isIntake) row.Intake++; else row.Resolved++;
        }
        foreach (var c in intake) Bump(c, true);
        foreach (var c in resolved) Bump(c, false);

        var rows = order.Select(k => map[k]).ToList();
        rows = by == "priority"
            // priority เรียงตามความด่วน ไม่ใช่ตามจำนวน — ไม่งั้น critical ไปอยู่ท้ายกราฟ (ค่าที่ไม่รู้จัก = -1 ไปหน้าสุดเหมือน indexOf)
            ? rows.OrderBy(x => Array.IndexOf(NisAnalyticsText.PriorityOrder, x.Key)).ToList()
            : rows.OrderByDescending(x => x.Intake).ThenByDescending(x => x.Resolved).ToList();

        return new NisCountBreakdownDto
        {
            By = by,
            IntakeLabel = NisAnalyticsText.IntakeLabel,
            ResolvedLabel = NisAnalyticsText.ResolvedLabel,
            Rows = rows,
        };
    }

    /// <summary>
    /// ฮิสโทแกรมเวลา 6 ช่วง — ขอบเป็นชั่วโมงเท่ากันทั้งสองคอลัมน์ แต่ **ป้ายต่างกัน** (24–72 ชม.ทำการ ≠ 1–3 วันปฏิทิน)
    /// "กี่วันทำการ" คำนวณจาก cfg ไม่ฝัง 9 ชม./วัน · ขอบและป้ายต้องตรงกับ NisEffortService.Distribution เป๊ะ (SPEC §9 ข้อ 66)
    /// </summary>
    private static List<NisDurationRowDto> DurationBuckets(List<Row> resolved, NisBusinessHoursConfig cfg, Skipped skipped)
    {
        NisBusinessHours.TryParseHhmm(cfg.Start, out var startMin);
        NisBusinessHours.TryParseHhmm(cfg.End, out var endMin);
        var hoursPerDay = (endMin - startMin) / 60d;
        string InDays(double h)
        {
            var d = h / hoursPerDay;
            return d < 1 ? "ไม่ถึง 1 วันทำการ" : $"ราว {(JsRound(d * 10) / 10).ToString("0.#", CultureInfo.InvariantCulture)} วันทำการ";
        }
        var edges = new (string Label, string BusinessLabel, double Max)[]
        {
            ("ไม่ถึง 1 ชม.", "ไม่ถึง 1 ชม.ทำการ", 1),
            ("1–4 ชม.", $"1–4 ชม.ทำการ ({InDays(4)})", 4),
            ("4–8 ชม.", $"4–8 ชม.ทำการ ({InDays(8)})", 8),
            ("8–24 ชม.", $"8–24 ชม.ทำการ ({InDays(24)})", 24),
            ("1–3 วัน", $"24–72 ชม.ทำการ ({InDays(72)})", 72),
            ("เกิน 3 วัน", $"เกิน 72 ชม.ทำการ (เกิน {InDays(72).Replace("ราว ", string.Empty)})", double.PositiveInfinity),
        };
        // key เป็นรหัสกลาง ไม่ใช่ป้ายของคอลัมน์ใดคอลัมน์หนึ่ง
        var rows = edges.Select((e, i) => new NisDurationRowDto { Key = $"b{i}", Label = e.Label, BusinessLabel = e.BusinessLabel, Clock = 0, Business = 0 }).ToList();
        void Put(double h, bool clock)
        {
            var i = Array.FindIndex(edges, e => h < e.Max);
            var row = rows[i == -1 ? rows.Count - 1 : i];
            if (clock) row.Clock++; else row.Business = (row.Business ?? 0) + 1;
        }
        foreach (var c in resolved)
        {
            if (c.ResolutionHours is { } rh) Put(rh, true);
            var bh = SafeBusinessHours(c, cfg, skipped);
            if (bh is { } b) Put(b, false);
        }
        return rows;
    }

    // ────────────────────────────────────────────────────────────────────
    // heatmap
    // ────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<NisHeatmapDto> HeatmapAsync(string cmpId, NisAnalyticsQuery query, CancellationToken ct = default) =>
        HeatmapAsync(cmpId, query.ToFilter(), null, ct);

    /// <inheritdoc />
    public async Task<NisHeatmapDto> HeatmapAsync(string cmpId, NisCaseFilter filter, IReadOnlyList<string>? customerIds, CancellationToken ct = default)
    {
        // TS เรียก businessHours() โดยไม่ส่งปี = ปีปัจจุบันเสมอ (heatmap ไม่คืนตัวเลขเวลาทำการ มีแค่ in/off ต่อช่อง)
        var biz = await BusinessHoursAsync(cmpId, new[] { NisBangkokTime.Now().Year }, ct);
        var intake = await RowsAsync(cmpId, filter, "createdAt", customerIds, ct);

        // grid[weekday][hour] — weekday 0 = อาทิตย์ ตรงกับ ToBangkokParts
        var grid = Enumerable.Range(0, 7)
            .Select(_ => Enumerable.Range(0, 24).Select(_ => new NisHeatCellDto()).ToList())
            .ToList();
        foreach (var c in intake)
        {
            var local = NisBangkokTime.ToBangkok(c.CreatedAt);
            var cell = grid[(int)local.DayOfWeek][local.Hour];
            cell.Total++;
            if (NisBusinessHours.ClassifyOffHours(c.CreatedAt, biz.Config) is null) cell.InHours++;
            else cell.OffHours++;
        }
        return new NisHeatmapDto
        {
            CountLabel = NisAnalyticsText.IntakeLabel,
            Grid = grid,
            HolidayWarning = biz.Warning,
        };
    }
}
