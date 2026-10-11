using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// อ่านกลุ่มลูกค้าของโมดูล NIS Service Report (<see cref="NisCustomerGroup"/>) — อ่านอย่างเดียว
/// ใช้ร่วมกันโดย archive (ล้าง resolutionHours ของกลุ่มบันทึกย้อนหลัง) และ analytics (รวมรหัสรองเข้ารหัสหลัก)
/// · การเขียน/validate (ห่วง · ทอด · ธงที่รหัสรอง) อยู่ที่ NisCustomerGroupService ของ customers controller — ไม่ใช่ที่นี่
///
/// ตารางนี้เล็กมาก (สร้างแถวเฉพาะตอนมีคนตั้งค่า ไม่ seed ทุกรหัส) — อ่านทั้งตารางของบริษัทครั้งเดียวต่อคำขอ
/// ถูกกว่า query ต่อแถวมาก (N+1) เมื่อหน้าหนึ่งมีได้ถึง 500 แถว
///
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface จึงไม่ถูก Autofac auto-register)
/// </summary>
public class NisCustomerGroupResolverService
{
    private readonly DatabaseContext _db;

    public NisCustomerGroupResolverService(DatabaseContext db)
    {
        _db = db;
    }

    /// <summary>
    /// resolve กลุ่มของรหัสหนึ่งรหัส — รหัสที่ไม่มีแถวในตาราง = กลุ่มของตัวเองคนเดียว ไม่ย้อนหลัง
    /// </summary>
    public async Task<NisCustomerGroupDto> ResolveGroupAsync(string cmpId, string customerId, CancellationToken ct = default)
    {
        var rows = await _db.NisCustomerGroups.AsNoTracking()
            .Where(g => g.CmpId == cmpId)
            .ToListAsync(ct);
        return Resolve(rows, customerId);
    }

    /// <summary>resolve จากแถวที่โหลดไว้แล้ว (ให้ analytics วนหลายรหัสโดยอ่านตารางครั้งเดียว)</summary>
    public static NisCustomerGroupDto Resolve(IReadOnlyList<NisCustomerGroup> rows, string customerId)
    {
        var self = rows.FirstOrDefault(r => r.CustomerId == customerId);
        var primary = string.IsNullOrWhiteSpace(self?.MergeIntoCustomerId) ? customerId : self!.MergeIntoCustomerId!;
        var members = new List<string> { primary };
        members.AddRange(rows.Where(r => r.MergeIntoCustomerId == primary && r.CustomerId != primary).Select(r => r.CustomerId));
        var primaryRow = rows.FirstOrDefault(r => r.CustomerId == primary);
        return new NisCustomerGroupDto
        {
            CustomerId = customerId,
            PrimaryCustomerId = primary,
            MemberCustomerIds = members.Distinct().ToList(),
            IsSecondary = !string.IsNullOrWhiteSpace(self?.MergeIntoCustomerId),
            // ธงอ่านที่แถวของรหัสหลักเท่านั้น (customers.service ปฏิเสธการตั้งธงที่รหัสรองอยู่แล้ว)
            Backdated = primaryRow?.Backdated ?? false,
        };
    }

    /// <summary>แผนที่ รหัสรอง → รหัสหลัก (เฉพาะแถวที่มี MergeIntoCustomerId)</summary>
    public async Task<Dictionary<string, string>> MergeMapAsync(string cmpId, CancellationToken ct = default)
    {
        return await _db.NisCustomerGroups.AsNoTracking()
            .Where(g => g.CmpId == cmpId && g.MergeIntoCustomerId != null && g.MergeIntoCustomerId != "")
            .ToDictionaryAsync(g => g.CustomerId, g => g.MergeIntoCustomerId!, ct);
    }

    /// <summary>รหัสหลักที่ติดธงบันทึกย้อนหลัง</summary>
    public async Task<HashSet<string>> BackdatedPrimariesAsync(string cmpId, CancellationToken ct = default)
    {
        var ids = await _db.NisCustomerGroups.AsNoTracking()
            .Where(g => g.CmpId == cmpId && g.Backdated)
            .Select(g => g.CustomerId)
            .ToListAsync(ct);
        return new HashSet<string>(ids, StringComparer.Ordinal);
    }

    /// <summary>
    /// ทุกรหัส (หลัก + รอง) ที่ตัวเลขระยะเวลาต้องแสดงเป็น null — คัดลอกตรรกะ cases.service.ts:backdatedCustomerIds
    /// แถวที่ถือธงเองอยู่ในผลเสมอ แม้จะมี MergeIntoCustomerId (สภาพที่ API ไม่ควรให้เกิด แต่กันไว้)
    /// </summary>
    public async Task<HashSet<string>> BackdatedCustomerIdsAsync(string cmpId, CancellationToken ct = default)
    {
        var rows = await _db.NisCustomerGroups.AsNoTracking()
            .Where(g => g.CmpId == cmpId)
            .ToListAsync(ct);
        var primaryOf = rows.ToDictionary(r => r.CustomerId, r => string.IsNullOrWhiteSpace(r.MergeIntoCustomerId) ? r.CustomerId : r.MergeIntoCustomerId!);
        var backdatedPrimaries = rows.Where(r => r.Backdated).Select(r => r.CustomerId).ToHashSet(StringComparer.Ordinal);
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in rows)
        {
            if (r.Backdated) result.Add(r.CustomerId);
            var primary = primaryOf.TryGetValue(r.CustomerId, out var p) ? p : r.CustomerId;
            if (backdatedPrimaries.Contains(primary)) result.Add(r.CustomerId);
        }
        return result;
    }
}
