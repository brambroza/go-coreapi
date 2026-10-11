using System.Globalization;
using System.Text.RegularExpressions;
using goalongapi.Models.Nis;

namespace goalongapi.Helpers;

// ── NisBusinessHours ────────────────────────────────────────────────────────
// เวลาทำการ 08:30–17:30 จันทร์–ศุกร์ · หักวันหยุดจากตาราง NisHoliday (เฉพาะ Observed=true)
// port จาก packages/shared/src/business-hours.ts — ฝั่ง C# เป็นตัวตัดสินจริง (MERGE_PLAN §6 ความเสี่ยงข้อ 1)
// ทุกอย่างคิดที่เวลาไทย +07:00 ผ่าน NisBangkokTime · ชั้นนี้ห้ามอ่านนาฬิกาเอง (ผู้เรียกส่ง forYear/now มา)

/// <summary>
/// คอนฟิกเวลาทำการ — Start/End เป็น 'HH:mm' · Holidays = เซตวันหยุด 'YYYY-MM-DD' (เวลาไทย) ที่ถูกหักจริง
/// </summary>
public sealed record NisBusinessHoursConfig(
    string Start,
    string End,
    bool Saturday,
    bool Sunday,
    IReadOnlySet<string> Holidays)
{
    /// <summary>ค่าเริ่มต้นของ NIS — 08:30–17:30 จ–ศ ไม่มีวันหยุด</summary>
    public static NisBusinessHoursConfig Default { get; } =
        new("08:30", "17:30", false, false, new HashSet<string>(StringComparer.Ordinal));

    /// <summary>
    /// อ่านจาก appsettings <c>NisReport:BizStart/BizEnd/BizSat/BizSun</c> — ตรวจรูป HH:mm และ start &lt; end
    /// โยน <see cref="InvalidOperationException"/> ทันทีตอน startup ถ้าคอนฟิกผิด (ดีกว่าให้ตัวเลขเพี้ยนเงียบ ๆ ทั้งปี)
    /// </summary>
    public static NisBusinessHoursConfig FromConfiguration(IConfiguration configuration)
    {
        var start = configuration["NisReport:BizStart"] ?? Default.Start;
        var end = configuration["NisReport:BizEnd"] ?? Default.End;
        var saturday = configuration.GetValue<bool>("NisReport:BizSat");
        var sunday = configuration.GetValue<bool>("NisReport:BizSun");

        if (!NisBusinessHours.TryParseHhmm(start, out var startMin))
            throw new InvalidOperationException($"NisReport:BizStart '{start}' ต้องเป็นรูปแบบ HH:mm");
        if (!NisBusinessHours.TryParseHhmm(end, out var endMin))
            throw new InvalidOperationException($"NisReport:BizEnd '{end}' ต้องเป็นรูปแบบ HH:mm");
        if (startMin >= endMin)
            throw new InvalidOperationException($"NisReport:BizStart ({start}) ต้องน้อยกว่า NisReport:BizEnd ({end})");

        return new NisBusinessHoursConfig(start, end, saturday, sunday, new HashSet<string>(StringComparer.Ordinal));
    }

    /// <summary>สำเนาที่แทนเซตวันหยุดใหม่</summary>
    public NisBusinessHoursConfig WithHolidays(IEnumerable<string> holidayDates) =>
        this with { Holidays = new HashSet<string>(holidayDates, StringComparer.Ordinal) };
}

/// <summary>นอกเวลาแบบไหน — หน้ารายงานแยกกลุ่มนี้ · null (จาก ClassifyOffHours) = ในเวลาทำการ</summary>
public enum NisOffHoursKind
{
    /// <summary>ก่อนเวลาเปิด (วันทำการ)</summary>
    Before,
    /// <summary>หลังเวลาปิด (วันทำการ)</summary>
    After,
    /// <summary>เสาร์/อาทิตย์ที่ไม่ทำการ</summary>
    Weekend,
    /// <summary>วันหยุดที่ Observed</summary>
    Holiday,
}

/// <summary>
/// ผลของ <see cref="NisBusinessHours.Build"/> — คอนฟิกที่หักวันหยุดแล้ว + ความมั่นใจของตารางวันหยุด
/// (HolidayConfidence ใน TS)
/// </summary>
public sealed class NisBusinessHoursSet
{
    /// <summary>คอนฟิกที่ Holidays = วันที่ Observed=true เท่านั้น</summary>
    public NisBusinessHoursConfig Config { get; init; } = NisBusinessHoursConfig.Default;

    /// <summary>จำนวนแถวทั้งหมดที่ส่งเข้ามา</summary>
    public int TotalListed { get; init; }

    /// <summary>จำนวนวันที่ Observed=true (ถูกหักจริง)</summary>
    public int ObservedCount { get; init; }

    /// <summary>จำนวนวันที่ Verified=false</summary>
    public int UnverifiedCount { get; init; }

    /// <summary>จำนวนแถว Kind='company'</summary>
    public int CompanyCount { get; init; }

    /// <summary>ปีที่ถาม (forYear) แต่ยังไม่มีแถวในตารางเลย · null = มีแล้ว หรือไม่ได้ถาม</summary>
    public int? MissingYear { get; init; }

    /// <summary>ข้อความเตือนพร้อม "ทิศทาง" ของความคลาดเคลื่อน · null = เชื่อตัวเลขได้</summary>
    public string? Warning { get; init; }

    /// <summary>
    /// true = ปีที่ถาม (forYear) ยังยืนยัน Observed ไม่ครบ → ตัวเลขที่อิงเวลาทำการต้องไม่แสดง (SPEC §9 ข้อ 67)
    /// ต่างจาก Warning != null ตรงที่ไม่รวมกรณี "Observed ครบแล้วแต่ยังมี Verified=false"
    /// </summary>
    public bool NumbersBlocked { get; init; }

    /// <summary>
    /// ปีที่ยืนยัน Observed ครบทุกแถวแล้ว (ทุกแถวของปีนั้น Observed=true และมีอย่างน้อย 1 แถว)
    /// = ปีที่ตัวเลขเวลาทำการแสดงได้ · ปีที่ไม่อยู่ในนี้ถูกบล็อก
    /// </summary>
    public IReadOnlyList<int> ObservedYears { get; init; } = Array.Empty<int>();
}

/// <summary>ตัวคำนวณเวลาทำการ (static) — ทุกเมธอดรับ/คืนเวลาเป็น DateTimeOffset และคิดที่ +07:00</summary>
public static class NisBusinessHours
{
    /// <summary>ส่วนประกอบเวลาไทยของจุดเวลาหนึ่ง — Weekday 0 = อาทิตย์ (ตรงกับ JS getDay)</summary>
    public readonly record struct BangkokParts(string Ymd, int MinutesOfDay, long MsOfDay, int Weekday);

    private const int MaxSpanDays = 366 * 2;

    private static readonly Regex HhmmRegex = new(@"^(\d{1,2}):(\d{2})$", RegexOptions.Compiled);

    /// <summary>parse 'HH:mm' → นาทีของวัน · false เมื่อรูปผิดหรือเกินช่วง</summary>
    public static bool TryParseHhmm(string? hhmm, out int minutes)
    {
        minutes = 0;
        if (string.IsNullOrWhiteSpace(hhmm)) return false;
        var m = HhmmRegex.Match(hhmm.Trim());
        if (!m.Success) return false;
        var h = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        var mi = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        if (h > 23 || mi > 59) return false;
        minutes = h * 60 + mi;
        return true;
    }

    /// <summary>'HH:mm' → นาทีของวัน (คอนฟิกผ่าน FromConfiguration มาแล้ว จึงไม่ควรผิด — ผิดคืน 0)</summary>
    private static int ToMinutes(string hhmm) => TryParseHhmm(hhmm, out var m) ? m : 0;

    /// <summary>
    /// แปลงเป็นเวลาไทยแล้วคืน { Ymd, MinutesOfDay, MsOfDay, Weekday }
    /// MinutesOfDay ปัดวินาทีทิ้ง — พอสำหรับตัดสิน "ใน/นอกเวลา" แต่ **ห้ามใช้คำนวณระยะเวลา** (ใช้ MsOfDay)
    /// </summary>
    public static BangkokParts ToBangkokParts(DateTimeOffset at)
    {
        var local = NisBangkokTime.ToBangkok(at);
        var msOfDay = (long)local.TimeOfDay.TotalMilliseconds;
        return new BangkokParts(
            local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (int)(msOfDay / 60_000),
            msOfDay,
            (int)local.DayOfWeek);
    }

    /// <summary>วันทำการไหม — เสาร์/อาทิตย์ตามคอนฟิก และต้องไม่ตรงวันหยุดที่ Observed</summary>
    public static bool IsWorkingDay(DateTimeOffset at, NisBusinessHoursConfig cfg)
    {
        var p = ToBangkokParts(at);
        return IsWorkingDay(p.Ymd, (DayOfWeek)p.Weekday, cfg);
    }

    private static bool IsWorkingDay(string ymd, DayOfWeek weekday, NisBusinessHoursConfig cfg)
    {
        if (weekday == DayOfWeek.Sunday && !cfg.Sunday) return false;
        if (weekday == DayOfWeek.Saturday && !cfg.Saturday) return false;
        return !cfg.Holidays.Contains(ymd);
    }

    /// <summary>
    /// อยู่ในเวลาทำการไหม — เทียบด้วย ms และใช้ขอบเดียวกับ <see cref="ClassifyOffHours"/> เป๊ะ
    /// (ขอบปิดรวม 17:30:00.000 แต่ 17:30:00.001 เป็น "หลังเลิกงาน" ทั้งสองตัวเหมือนกัน)
    /// </summary>
    public static bool IsWithinBusinessHours(DateTimeOffset at, NisBusinessHoursConfig cfg)
    {
        if (!IsWorkingDay(at, cfg)) return false;
        var ms = ToBangkokParts(at).MsOfDay;
        return ms >= ToMinutes(cfg.Start) * 60_000L && ms <= ToMinutes(cfg.End) * 60_000L;
    }

    /// <summary>
    /// นาทีทำการระหว่างสองเวลา — หัวใจของตัวเลขเวลาทำการที่ส่งลูกค้า
    /// เดินทีละวันตามปฏิทินไทย นับเฉพาะส่วนที่ทับกับช่วงเวลาทำการของวันทำการ · วันหยุด/เสาร์อาทิตย์ข้ามทั้งวัน
    /// คิดเป็น ms ทั้งหมด (ปัดวินาทีทิ้งที่ขอบทั้งสองข้างทำให้ผลเกินเวลานาฬิกาได้ ซึ่งเป็นไปไม่ได้)
    /// </summary>
    /// <returns>นาทีทำการ · 0 เมื่อ to &lt;= from (ไม่คืนค่าติดลบ)</returns>
    /// <exception cref="InvalidOperationException">ช่วงยาวเกิน 2 ปี — เคสจริงยาวสุด 20 วัน ถ้าเจอ 2 ปีคือข้อมูลเพี้ยน ต้องดังไม่ใช่วนเงียบ</exception>
    public static double BusinessMinutesBetween(DateTimeOffset from, DateTimeOffset to, NisBusinessHoursConfig cfg)
    {
        if (to <= from) return 0;

        var spanDays = (to - from).TotalDays;
        if (spanDays > MaxSpanDays)
            throw new InvalidOperationException(
                $"BusinessMinutesBetween: ช่วงยาว {Math.Round(spanDays)} วัน เกิน {MaxSpanDays} วัน — " +
                "น่าจะเป็นข้อมูลเพี้ยน (ปี พ.ศ. ปนมา หรือ resolvedAt ผิด) ไม่ใช่เคสจริง");

        var open = ToMinutes(cfg.Start) * 60_000L;
        var close = ToMinutes(cfg.End) * 60_000L;
        if (close <= open) return 0;

        var start = ToBangkokParts(from);
        var end = ToBangkokParts(to);
        var startDay = DateOnly.ParseExact(start.Ymd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endDay = DateOnly.ParseExact(end.Ymd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var day = startDay;

        long totalMs = 0;
        for (;;)
        {
            var ymd = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (IsWorkingDay(ymd, day.DayOfWeek, cfg))
            {
                // ขอบของช่วงที่ขอ ตกอยู่ตรงไหนของวันนี้
                var lo = day == startDay ? Math.Max(open, start.MsOfDay) : open;
                var hi = day == endDay ? Math.Min(close, end.MsOfDay) : close;
                if (hi > lo) totalMs += hi - lo;
            }
            if (day == endDay) break;
            day = day.AddDays(1);
        }
        return totalMs / 60_000d;
    }

    /// <summary>
    /// ชั่วโมงทำการ — **ไม่ปัดทศนิยม** ให้ชั้นแสดงผลเป็นคนปัด (ปัดที่นี่เคยทำให้เวลาทำการ 1.36 มากกว่าเวลานาฬิกา 1.355)
    /// </summary>
    /// <exception cref="InvalidOperationException">ช่วงยาวเกิน 2 ปี</exception>
    public static double BusinessHoursBetween(DateTimeOffset from, DateTimeOffset to, NisBusinessHoursConfig cfg) =>
        BusinessMinutesBetween(from, to, cfg) / 60d;

    /// <summary>
    /// แยกว่านอกเวลาแบบไหน · คืน null เมื่ออยู่ในเวลาทำการ
    /// ลำดับ: วันหยุด → เสาร์/อาทิตย์ → ก่อนเปิด → หลังปิด (ใช้ ms ให้ตรงกับ IsWithinBusinessHours)
    /// </summary>
    public static NisOffHoursKind? ClassifyOffHours(DateTimeOffset at, NisBusinessHoursConfig cfg)
    {
        var p = ToBangkokParts(at);
        if (cfg.Holidays.Contains(p.Ymd)) return NisOffHoursKind.Holiday;
        var weekday = (DayOfWeek)p.Weekday;
        if ((weekday == DayOfWeek.Sunday && !cfg.Sunday) || (weekday == DayOfWeek.Saturday && !cfg.Saturday))
            return NisOffHoursKind.Weekend;
        if (p.MsOfDay < ToMinutes(cfg.Start) * 60_000L) return NisOffHoursKind.Before;
        if (p.MsOfDay > ToMinutes(cfg.End) * 60_000L) return NisOffHoursKind.After;
        return null;
    }

    /// <summary>ชื่อ kind แบบ TS ('before' | 'after' | 'weekend' | 'holiday' | 'in') สำหรับ JSON</summary>
    public static string OffHoursLabel(NisOffHoursKind? kind) => kind switch
    {
        NisOffHoursKind.Before => "before",
        NisOffHoursKind.After => "after",
        NisOffHoursKind.Weekend => "weekend",
        NisOffHoursKind.Holiday => "holiday",
        _ => "in",
    };

    /// <summary>
    /// แปลงค่าช่วงวันที่จาก query string ให้เป็นเวลาไทยเสมอ (parseThaiRangeBoundary)
    /// · มี offset/Z มาแล้ว → เชื่อตามที่ส่งมา
    /// · 'YYYY-MM-DD' → ต้นวัน 00:00:00.000 หรือท้ายวัน 23:59:59.999 (+07:00)
    /// · มีเวลาแต่ไม่มี offset → ถือเป็นเวลาไทย
    /// คืน null เมื่อ parse ไม่ได้ **หรือวันที่ไม่มีอยู่จริง** (2026-02-31 ต้องถูกปฏิเสธ ไม่ใช่ปัดเป็น 2 มี.ค.)
    /// </summary>
    public static DateTimeOffset? ParseThaiRangeBoundary(string value, bool endEdge)
    {
        var v = value.Trim();
        if (v.Length == 0) return null;

        // วันที่ล้วน
        if (Regex.IsMatch(v, @"^\d{4}-\d{2}-\d{2}$"))
        {
            if (!DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) return null;
            return endEdge ? NisBangkokTime.EndOfDay(v) : NisBangkokTime.StartOfDay(v);
        }

        var hasOffset = Regex.IsMatch(v, @"[zZ]$|[+-]\d{2}:?\d{2}$");
        var text = hasOffset ? v : (Regex.IsMatch(v, @"^\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}") ? v + "+07:00" : v);

        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) return null;

        // วันที่ส่วนหน้าต้องตรงกับที่ parse ได้ (กันวันที่เกินขอบเดือนที่ parser บางตัวปัดให้)
        var m = Regex.Match(v, @"^(\d{4}-\d{2}-\d{2})(?:$|[T ])");
        if (m.Success && ToBangkokParts(parsed).Ymd != m.Groups[1].Value && !hasOffset) return null;
        return parsed;
    }

    /// <summary>
    /// สร้างคอนฟิกเวลาทำการจากตาราง NisHoliday — หักเฉพาะ Observed=true
    /// </summary>
    /// <param name="baseConfig">คอนฟิกจาก appsettings (Holidays จะถูกแทน)</param>
    /// <param name="rows">แถววันหยุด (ควรเป็นของ CmpId เดียว)</param>
    /// <param name="forYear">ปี ค.ศ. ที่ต้องการตรวจว่ามีวันหยุดในตารางหรือยัง — ผู้เรียกส่งมาเพราะชั้นนี้ห้ามอ่านนาฬิกาเอง</param>
    public static NisBusinessHoursSet Build(NisBusinessHoursConfig baseConfig, IEnumerable<NisHoliday> rows, int? forYear = null)
    {
        var list = rows.ToList();
        var observedRows = list.Where(r => r.Observed).ToList();
        var unverified = list.Count(r => !r.Verified);
        var company = list.Count(r => r.Kind == "company");

        // ปีที่ยังไม่มีวันหยุดในตาราง — ต้องแยกเป็นสาขาแรกสุด ไม่งั้นพอ HR ยืนยันปีนี้ครบ
        // วันที่ 1 ม.ค. ปีถัดไปจะได้ warning = null ทั้งที่ตารางมีวันหยุดปีใหม่ศูนย์วัน
        int? missingYear = forYear.HasValue && !list.Any(r => r.Year == forYear.Value) ? forYear : null;

        // ทุกเงื่อนไขเทียบเฉพาะปีที่กำลังใช้งาน ไม่นับรวมข้ามปี (ปีถัดไปที่ seed ล่วงหน้าจะทำให้นับรวมผิด)
        var yearRows = forYear.HasValue ? list.Where(r => r.Year == forYear.Value).ToList() : new List<NisHoliday>();
        var yearObserved = yearRows.Count(r => r.Observed);
        var forYearBe = forYear.HasValue ? (forYear.Value + 543).ToString(CultureInfo.InvariantCulture) : string.Empty;

        var numbersBlocked = false;
        string? warning = null;
        if (missingYear.HasValue)
        {
            numbersBlocked = true;
            warning = MissingYearWarning(missingYear.Value);
        }
        else if (forYear.HasValue ? yearObserved == 0 : observedRows.Count == 0)
        {
            numbersBlocked = true;
            var total = forYear.HasValue ? yearRows.Count : list.Count;
            warning = ZeroObservedWarning(forYearBe, total);
        }
        else
        {
            var partialYear = yearRows.Count > 0 && yearObserved > 0 && yearObserved < yearRows.Count;
            if (partialYear)
            {
                numbersBlocked = true;
                warning = PartialObservedWarning(forYearBe, yearObserved, yearRows.Count, unverified);
            }
            else if (unverified > 0)
            {
                warning = $"มีวันหยุด {unverified} วันที่ยังไม่ยืนยันวันที่ — ถ้ายืนยันแล้วต้องตั้ง observed ให้ตรงกับที่บริษัทหยุดจริง";
            }
        }

        var observedYears = list
            .GroupBy(r => r.Year)
            .Where(g => g.All(r => r.Observed))
            .Select(g => g.Key)
            .OrderBy(y => y)
            .ToList();

        return new NisBusinessHoursSet
        {
            Config = baseConfig.WithHolidays(observedRows.Select(r => r.Date)),
            TotalListed = list.Count,
            ObservedCount = observedRows.Count,
            UnverifiedCount = unverified,
            CompanyCount = company,
            MissingYear = missingYear,
            Warning = warning,
            NumbersBlocked = numbersBlocked,
            ObservedYears = observedYears,
        };
    }

    /// <summary>ปี ค.ศ. (เวลาไทย) ทุกปีที่ช่วง from–to คาบเกี่ยว</summary>
    public static IReadOnlyList<int> YearsInRange(DateTimeOffset from, DateTimeOffset to)
    {
        var a = NisBangkokTime.ToBangkok(from).Year;
        var b = NisBangkokTime.ToBangkok(to).Year;
        if (b < a) (a, b) = (b, a);
        return Enumerable.Range(a, b - a + 1).ToList();
    }

    /// <summary>
    /// ตัวเลขที่อิงเวลาทำการของช่วง from–to แสดงได้ไหม (SPEC §9 ข้อ 67) — ตรวจทุกปีที่ช่วงคาบเกี่ยว 3 กติกา:
    /// (1) ปีนั้นไม่มีแถวในตารางเลย · (2) มีแถวแต่ Observed = 0 · (3) Observed บางส่วน
    /// ข้อใดเข้า = บล็อก พร้อมเหตุผลเป็นภาษาคน (ปี พ.ศ.) · ไม่บล็อกเพราะ Verified=false (ข้อ 67 พูดถึง observed)
    /// </summary>
    /// <param name="set">ผลจาก <see cref="Build"/> (ใช้คอนฟิก/ตัวนับ)</param>
    /// <param name="holidays">แถววันหยุดชุดเดียวกับที่ใช้ Build</param>
    /// <returns>(Blocked, Reason) — Reason = null เมื่อไม่บล็อก</returns>
    public static (bool Blocked, string? Reason) NumbersBlocked(
        NisBusinessHoursSet set, DateTimeOffset from, DateTimeOffset to, IEnumerable<NisHoliday> holidays)
    {
        _ = set; // คอนฟิกไม่กระทบกติกา 3 ข้อ — รับไว้ให้ signature คงที่สำหรับผู้เรียกฝั่ง analytics
        var byYear = holidays.GroupBy(h => h.Year).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var year in YearsInRange(from, to))
        {
            if (!byYear.TryGetValue(year, out var rows) || rows.Count == 0)
                return (true, MissingYearWarning(year));
            var observed = rows.Count(r => r.Observed);
            var be = (year + 543).ToString(CultureInfo.InvariantCulture);
            if (observed == 0)
                return (true, ZeroObservedWarning(be, rows.Count));
            if (observed < rows.Count)
                return (true, PartialObservedWarning(be, observed, rows.Count, rows.Count(r => !r.Verified)));
        }
        return (false, null);
    }

    // ── ข้อความเตือน — ที่เดียว ใช้ทั้ง Build และ NumbersBlocked (สองที่จะพูดไม่ตรงกันเมื่อแก้ทีหลัง) ──

    private static string MissingYearWarning(int year) =>
        $"ยังไม่ได้ใส่วันหยุดของปี {year + 543} ลงตาราง holiday — " +
        "ระบบจะไม่หักวันหยุดของปีนี้เลย ตัวเลขเวลาทำการทั้งปีจะเพี้ยนไปในทิศเดียวกัน " +
        "(เวลาที่ใช้ต่อเคสยาวเกินจริง → % อยู่ในเกณฑ์ SLA ต่ำกว่าความจริง) " +
        "ต้องเพิ่มวันหยุดราชการของปีแล้วยืนยันกับ HR ว่าบริษัทหยุดวันไหนบ้าง";

    private static string ZeroObservedWarning(string yearBe, int total) =>
        $"ยืนยันวันหยุดของปี {yearBe} แล้ว 0 จาก {total} วัน — ระบบยังไม่หักวันหยุดออกจากเวลาทำการเลย " +
        "ยืนยันได้ที่ ตั้งค่า → วันหยุด · กระทบตัวเลขสองชุด และทั้งคู่ต่ำกว่าความจริง: " +
        "(1) จำนวน \"งานนอกเวลาทำการ\" ต่ำกว่าความจริง เพราะงานที่แจ้งเข้ามาในวันหยุดถูกนับเป็นงานในเวลา · " +
        "(2) เวลาที่ใช้ต่อเคสยาวเกินจริง เพราะชั่วโมงในวันหยุดถูกนับเป็นเวลาทำงาน ทำให้ " +
        "% อยู่ในเกณฑ์ SLA ต่ำกว่าความจริง — ทีมดูแย่กว่าที่เป็นจริงในรายงานที่ส่งลูกค้า";

    private static string PartialObservedWarning(string yearBe, int observed, int total, int unverified) =>
        $"ยืนยันวันหยุดของปี {yearBe} แล้ว {observed} จาก {total} วัน — ที่เหลือยังไม่ถูก" +
        "หักออกจากเวลาทำการ ตัวเลขเวลาทำการของปีนี้ยังไม่ครบเหมือนเดิม ต้องยืนยันให้ครบทุกวันก่อนเชื่อตัวเลขได้" +
        (unverified > 0 ? $" (และมีวันหยุด {unverified} วันที่ยังไม่ยืนยันวันที่ด้วย)" : string.Empty);
}
