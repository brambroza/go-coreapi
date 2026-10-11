using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// วันหยุดของโมดูล NIS Service Report (<see cref="NisHoliday"/> + <see cref="NisHolidayChangeLog"/>)
/// port จาก cases.controller.ts:124-273 · "ใคร" = username จาก JWT (แทน header x-nsr-source gate/human ของระบบเดิม)
///
/// 🔴 NisHolidayChangeLog เป็น append-only — ไม่มีเมธอดลบที่นี่ และห้ามเพิ่ม
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface จึงไม่ถูก Autofac auto-register)
/// </summary>
public class NisHolidayService
{
    private static readonly string[] Fields = { "nameTh", "kind", "verified", "observed", "note" };

    private readonly DatabaseContext _db;
    private readonly NisBusinessHoursConfig _bizBase;

    public NisHolidayService(DatabaseContext db, IConfiguration configuration)
    {
        _db = db;
        _bizBase = NisBusinessHoursConfig.FromConfiguration(configuration);
    }

    /// <summary>
    /// รายการวันหยุด (ทั้งหมดหรือเฉพาะปี) + meta ความมั่นใจ {observed, unverified, company, warning}
    /// warning คิดกับปีปัจจุบัน (เวลาไทย) เสมอ — ชั้น Build ห้ามอ่านนาฬิกาเอง จึงส่งปีเข้าไปจากที่นี่
    /// </summary>
    public async Task<(List<NisHolidayDto> Rows, Dictionary<string, object?> Meta)> ListAsync(string cmpId, int? year, CancellationToken ct = default)
    {
        var q = _db.NisHolidays.AsNoTracking().Where(h => h.CmpId == cmpId);
        if (year.HasValue) q = q.Where(h => h.Year == year.Value);
        var rows = await q.OrderBy(h => h.Date).ToListAsync(ct);

        var set = NisBusinessHours.Build(_bizBase, rows, NisBangkokTime.Now().Year);
        var meta = new Dictionary<string, object?>
        {
            ["observed"] = set.ObservedCount,
            ["unverified"] = set.UnverifiedCount,
            ["company"] = set.CompanyCount,
            ["warning"] = set.Warning,
            ["numbersBlocked"] = set.NumbersBlocked,
        };
        return (rows.Select(NisHolidayDto.From).ToList(), meta);
    }

    /// <summary>
    /// เพิ่มวันหยุดจากหน้าจอ — บันทึก change log field 'created' (newValue = ชื่อวันหยุด ไม่ปน kind ซึ่งเป็นศัพท์ภายใน)
    /// </summary>
    /// <exception cref="NisArchiveBadRequestException">มีวันหยุดของวันที่นี้อยู่แล้ว</exception>
    public async Task<NisHolidayDto> CreateAsync(string cmpId, NisHolidayCreateDto b, string changedBy, CancellationToken ct = default)
    {
        var exists = await _db.NisHolidays.AnyAsync(h => h.CmpId == cmpId && h.Date == b.Date, ct);
        if (exists)
            throw new NisArchiveBadRequestException($"มีวันหยุดของวันที่ {b.Date} อยู่แล้ว — แก้ที่แถวเดิมแทนการเพิ่มซ้ำ");

        var row = new NisHoliday
        {
            CmpId = cmpId,
            Date = b.Date,
            Year = int.Parse(b.Date[..4], System.Globalization.CultureInfo.InvariantCulture),
            NameTh = b.NameTh.Trim(),
            Kind = b.Kind,
            Verified = b.Verified,
            Observed = b.Observed,
            Note = string.IsNullOrWhiteSpace(b.Note) ? null : b.Note.Trim(),
        };
        _db.NisHolidays.Add(row);
        _db.NisHolidayChangeLogs.Add(new NisHolidayChangeLog
        {
            CmpId = cmpId,
            Date = b.Date,
            Field = "created",
            OldValue = null,
            NewValue = row.NameTh,
            ChangedBy = changedBy,
            ChangedAt = DateTimeOffset.UtcNow,
        });
        // SaveChanges เดียว = transaction เดียว — แถววันหยุดกับ log ต้องเกิดพร้อมกันหรือไม่เกิดเลย
        await _db.SaveChangesAsync(ct);
        return NisHolidayDto.From(row);
    }

    /// <summary>
    /// แก้วันหยุดที่มีอยู่ — เทียบทีละ field ก่อนบันทึก หนึ่งแถว log ต่อหนึ่ง field ที่เปลี่ยนจริง
    /// field ที่ไม่ได้ส่งมา = ไม่แตะ · ค่าที่ส่งมาเหมือนเดิม = ไม่บันทึก (ไม่เขียน log เปล่า)
    /// </summary>
    /// <exception cref="NisArchiveBadRequestException">รูปแบบวันที่ผิด</exception>
    /// <exception cref="NisArchiveNotFoundException">ไม่พบวันหยุด</exception>
    public async Task<NisHolidayDto> UpdateAsync(string cmpId, string date, NisHolidayUpdateDto b, string changedBy, CancellationToken ct = default)
    {
        if (!NisHolidayJson.IsHolidayDate(date))
            throw new NisArchiveBadRequestException("date: ต้องเป็นรูปแบบ 'YYYY-MM-DD'");

        var existing = await _db.NisHolidays.FirstOrDefaultAsync(h => h.CmpId == cmpId && h.Date == date, ct)
            ?? throw new NisArchiveNotFoundException($"ไม่พบวันหยุดของวันที่ {date}");

        var now = DateTimeOffset.UtcNow;
        var changes = new List<NisHolidayChangeLog>();

        void Track(string field, string? prev, string? next, Action apply)
        {
            if (string.Equals(prev ?? "null", next ?? "null", StringComparison.Ordinal)) return; // เหมือนเดิม — ไม่ใช่การเปลี่ยนแปลง
            apply();
            changes.Add(new NisHolidayChangeLog
            {
                CmpId = cmpId, Date = date, Field = field, OldValue = prev, NewValue = next, ChangedBy = changedBy, ChangedAt = now,
            });
        }

        if (b.HasNameTh && b.NameTh != null)
        {
            var next = b.NameTh.Trim();
            Track("nameTh", existing.NameTh, next, () => existing.NameTh = next);
        }
        if (b.HasKind && b.Kind != null)
            Track("kind", existing.Kind, b.Kind, () => existing.Kind = b.Kind);
        if (b.HasVerified && b.Verified.HasValue)
            Track("verified", Bool(existing.Verified), Bool(b.Verified.Value), () => existing.Verified = b.Verified.Value);
        if (b.HasObserved && b.Observed.HasValue)
            Track("observed", Bool(existing.Observed), Bool(b.Observed.Value), () => existing.Observed = b.Observed.Value);
        if (b.HasNote)
        {
            var next = string.IsNullOrWhiteSpace(b.Note) ? null : b.Note.Trim();
            Track("note", existing.Note, next, () => existing.Note = next);
        }

        if (changes.Count == 0) return NisHolidayDto.From(existing);

        _db.NisHolidayChangeLogs.AddRange(changes);
        await _db.SaveChangesAsync(ct); // แถว + log ทั้งหมดใน transaction เดียว
        return NisHolidayDto.From(existing);
    }

    /// <summary>
    /// ประวัติการแก้ — เรียงใหม่→เก่า · limit clamp 1..200 (ค่าเริ่มต้น 50) · กรองตามคนแก้ได้
    /// meta: total (ตามตัวกรอง) · earliestAt (แถวเก่าสุด**ทั้งบริษัท** ไม่ผูกกับตัวกรอง — "บันทึกเริ่มเก็บเมื่อไร") · filteredBy
    /// </summary>
    public async Task<(List<NisHolidayChangeDto> Rows, int Total, Dictionary<string, object?> Meta)> ChangesAsync(
        string cmpId, int? limit, string? changedBy, CancellationToken ct = default)
    {
        var take = Math.Clamp(limit ?? 50, 1, 200);
        var all = _db.NisHolidayChangeLogs.AsNoTracking().Where(c => c.CmpId == cmpId);
        var filtered = string.IsNullOrWhiteSpace(changedBy) ? all : all.Where(c => c.ChangedBy == changedBy);

        var rows = await filtered.OrderByDescending(c => c.ChangedAt).ThenByDescending(c => c.Id).Take(take).ToListAsync(ct);
        var total = await filtered.CountAsync(ct);
        var earliest = await all.OrderBy(c => c.ChangedAt).Select(c => (DateTimeOffset?)c.ChangedAt).FirstOrDefaultAsync(ct);

        var meta = new Dictionary<string, object?>
        {
            ["earliestAt"] = earliest,
            ["filteredBy"] = string.IsNullOrWhiteSpace(changedBy) ? null : changedBy,
            ["limit"] = take,
        };
        return (rows.Select(NisHolidayChangeDto.From).ToList(), total, meta);
    }

    /// <summary>boolean → "true"/"false" เหมือน String(bool) ของ TS — เก็บใน OldValue/NewValue</summary>
    private static string Bool(bool v) => v ? "true" : "false";

    /// <summary>รายชื่อ field ที่แก้ได้ (ใช้ตรวจความครบของ DTO)</summary>
    public static IReadOnlyList<string> EditableFields => Fields;
}
