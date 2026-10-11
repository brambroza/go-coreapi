namespace goalongapi.Helpers;

/// <summary>
/// ค่ากลางของชั่วโมง (median / mean / p90) — port จาก apps/api/src/common/stats.ts
///
/// ค่าเฉลี่ยอย่างเดียวหลอกในชุดข้อมูลนี้ (median 0.21 ชม. แต่ mean 2.95 ชม. เพราะมีเคส 482 ชม. ในหาง)
/// จึงคืนสามค่าพร้อมกันเสมอ · ใช้ linear interpolation แบบ percentile_cont ของ Postgres
/// (ไม่ใช่ v[floor(p·n)] ซึ่งทำให้ p90 กลายเป็นค่าสูงสุดเมื่อ p·n ลงตัว)
/// ที่นี่เป็นที่เดียว — analytics และหน้าลูกค้าต้องใช้ตัวเดียวกัน ไม่งั้นค่ากลางสองหน้าไม่ตรงกัน
/// </summary>
public static class NisHoursStat
{
    /// <summary>
    /// คำนวณ median / mean / p90 (ปัด 2 ตำแหน่ง) และ N = จำนวนค่าที่ใช้คิด
    /// ค่าที่ไม่ใช่ตัวเลขจำกัด (NaN/Infinity) ถูกตัดทิ้ง · ชุดว่างคืน null ทั้งสามค่าและ N = 0
    /// </summary>
    public static (double? Median, double? Mean, double? P90, int N) HoursStat(IEnumerable<double> values)
    {
        var v = values.Where(double.IsFinite).OrderBy(x => x).ToArray();
        if (v.Length == 0) return (null, null, null, 0);

        double At(double p)
        {
            if (v.Length == 1) return v[0];
            var pos = p * (v.Length - 1);
            var lo = (int)Math.Floor(pos);
            var hi = (int)Math.Ceiling(pos);
            var a = v[lo];
            if (lo == hi) return a;
            return a + (v[hi] - a) * (pos - lo);
        }

        static double Round2(double x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);

        return (
            Round2(At(0.5)),
            Round2(v.Sum() / v.Length),
            Round2(At(0.9)),
            v.Length);
    }
}
