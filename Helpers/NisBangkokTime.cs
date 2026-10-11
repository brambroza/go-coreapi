using System.Globalization;

namespace goalongapi.Helpers;

/// <summary>
/// เวลาไทยสำหรับโมดูล NIS Service Report — ทุก bucket/ขอบวัน/ปี พ.ศ. ต้องคิดที่ +07:00
/// ห้ามใช้ <c>DateTime.Now</c> (ขึ้นกับ TZ ของ container) · ทุกคอลัมน์เก็บเป็น datetimeoffset
/// </summary>
public static class NisBangkokTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    /// <summary>เวลานี้ที่ +07:00</summary>
    public static DateTimeOffset Now() => DateTimeOffset.UtcNow.ToOffset(Offset);

    /// <summary>แปลงเวลาใด ๆ ให้แสดงที่ +07:00</summary>
    public static DateTimeOffset ToBangkok(DateTimeOffset at) => at.ToOffset(Offset);

    /// <summary>ต้นวัน (00:00:00.000 +07:00) ของวันที่ 'YYYY-MM-DD'</summary>
    public static DateTimeOffset StartOfDay(string isoDate)
    {
        var d = DateOnly.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, Offset);
    }

    /// <summary>ปลายวัน (23:59:59.999 +07:00) ของวันที่ 'YYYY-MM-DD'</summary>
    public static DateTimeOffset EndOfDay(string isoDate) => StartOfDay(isoDate).AddDays(1).AddMilliseconds(-1);

    /// <summary>ลอง parse 'YYYY-MM-DD' — คืน null ถ้ารูปผิด</summary>
    public static DateOnly? TryParseDate(string? isoDate) =>
        DateOnly.TryParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    /// <summary>วันที่ไทย 'YYYY-MM-DD' ของเวลานั้น</summary>
    public static string DateKey(DateTimeOffset at) => ToBangkok(at).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>เดือนไทย 'YYYY-MM'</summary>
    public static string MonthKey(DateTimeOffset at) => ToBangkok(at).ToString("yyyy-MM", CultureInfo.InvariantCulture);

    /// <summary>คีย์สัปดาห์ = วันที่ของวันจันทร์ในสัปดาห์นั้น (เวลาไทย)</summary>
    public static string WeekKey(DateTimeOffset at)
    {
        var local = ToBangkok(at).Date;
        var diff = ((int)local.DayOfWeek + 6) % 7; // จันทร์ = 0
        return local.AddDays(-diff).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>ปี พ.ศ. ของเวลานั้น</summary>
    public static int YearBe(DateTimeOffset at) => ToBangkok(at).Year + 543;

    /// <summary>ต้นเดือนและปลายเดือนปฏิทิน (เวลาไทย) ที่ครอบเวลานั้น</summary>
    public static (DateTimeOffset start, DateTimeOffset end) MonthBounds(DateTimeOffset at)
    {
        var local = ToBangkok(at);
        var start = new DateTimeOffset(local.Year, local.Month, 1, 0, 0, 0, Offset);
        return (start, start.AddMonths(1).AddMilliseconds(-1));
    }

    /// <summary>true เมื่อช่วง from–to คือเดือนปฏิทินเต็มเดือนพอดี (เวลาไทย)</summary>
    public static bool IsFullCalendarMonth(DateTimeOffset from, DateTimeOffset to)
    {
        var (s, e) = MonthBounds(from);
        return s == ToBangkok(from) && Math.Abs((e - ToBangkok(to)).TotalSeconds) < 1;
    }
}
