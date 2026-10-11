namespace goalongapi.Dtos.Nis;

/// <summary>
/// Envelope มาตรฐานของ endpoint โมดูล NIS Service Report (api/nis/archive · api/nis/analytics · api/nis/report)
/// <c>{ data, meta: { generatedAt, dataAsOf, total, ... } }</c> — frontend อ่าน <c>meta.dataAsOf</c> ไปแสดง "ข้อมูล ณ"
/// และ <c>meta.countLabel</c> บอกว่าตัวเลขเป็นชุด "แจ้งเข้า" หรือ "ปิดได้" (กติกาตัวเลขสองชุด)
/// </summary>
public sealed class NisApiResponse<T>
{
    public T Data { get; init; } = default!;

    public Dictionary<string, object?> Meta { get; init; } = new();

    /// <summary>สร้าง envelope พร้อม generatedAt — ค่าใน extra จะทับคีย์มาตรฐานได้</summary>
    public static NisApiResponse<T> Of(T data, DateTimeOffset? dataAsOf, int? total = null, IDictionary<string, object?>? extra = null)
    {
        var meta = new Dictionary<string, object?>
        {
            ["generatedAt"] = DateTimeOffset.UtcNow,
            ["dataAsOf"] = dataAsOf,
        };
        if (total.HasValue) meta["total"] = total.Value;
        if (extra != null)
            foreach (var kv in extra) meta[kv.Key] = kv.Value;
        return new NisApiResponse<T> { Data = data, Meta = meta };
    }
}
