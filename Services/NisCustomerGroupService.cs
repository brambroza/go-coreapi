using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// กลุ่มลูกค้า (SPEC §9 ข้อ 59) — ลูกค้าหลายรหัสที่เป็นองค์กรเดียวกัน · port จาก apps/api/src/customers/customers.service.ts
/// อ่าน/เขียนการตั้งค่า (<see cref="NisCustomerGroup"/>) + หน้า "ลูกค้า" (overview หนึ่งแถวต่อกลุ่ม)
///
/// กฎที่ตรวจที่นี่เพราะ SQL Server ไม่มี CHECK constraint ที่ตรวจแบบ recursive ได้:
///   (1) รวมเข้ากับตัวเองไม่ได้ · (2) รหัสที่ถือธง backdated เองห้ามไปเป็นรหัสรอง · (3) สมาชิกของกลุ่มที่ติดธงเปลี่ยนสมาชิกภาพต้องส่ง backdated มาชัดเจน
///   (4) ห้ามชี้ต่อเป็นทอด A→B→C และรหัสหลักที่มีรหัสอื่นรวมเข้ามาแล้วห้ามไปรวมกับใคร · (5) ตั้งธง backdated ได้ที่รหัสหลักเท่านั้น
/// ข้อความ error เป็นภาษาไทยชุดเดียวกับ TS (หน้าจอ CRM แสดงตรง ๆ)
///
/// การอ่านอย่างเดียวที่ analytics/archive ใช้ร่วมกันอยู่ที่ <see cref="NisCustomerGroupResolverService"/> — ไม่ซ้ำที่นี่
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface จึงไม่ถูก Autofac auto-register)
/// </summary>
public class NisCustomerGroupService
{
    private readonly DatabaseContext _db;
    private readonly INisArchiveService _archive;

    public NisCustomerGroupService(DatabaseContext db, INisArchiveService archive)
    {
        _db = db;
        _archive = archive;
    }

    /// <summary>การตั้งค่าของรหัสหนึ่งรหัส — ไม่มีแถว = ไม่รวม/ไม่ติดธง</summary>
    public async Task<NisCustomerSettingsDto> ReadSettingsAsync(string cmpId, string customerId, CancellationToken ct = default)
    {
        var row = await _db.NisCustomerGroups.AsNoTracking()
            .FirstOrDefaultAsync(g => g.CmpId == cmpId && g.CustomerId == customerId, ct);
        return ToDto(customerId, row);
    }

    private static NisCustomerSettingsDto ToDto(string customerId, NisCustomerGroup? row) => new()
    {
        CustomerId = customerId,
        MergeIntoCustomerId = string.IsNullOrWhiteSpace(row?.MergeIntoCustomerId) ? null : row!.MergeIntoCustomerId,
        Backdated = row?.Backdated ?? false,
    };

    /// <summary>
    /// ตั้งค่าที่ customerId — mergeIntoCustomerId/backdated เลือกส่งได้อิสระ (ไม่ส่ง = ไม่แตะ) · upsert แถว · updatedBy = username จาก JWT
    /// </summary>
    /// <exception cref="NisArchiveBadRequestException">ผิดกฎข้อใดข้อหนึ่งใน 5 ข้อ (ข้อความไทยตรงกับ TS)</exception>
    public async Task<NisCustomerSettingsDto> SetSettingsAsync(string cmpId, string customerId, NisSetCustomerSettingsDto input, string updatedBy, CancellationToken ct = default)
    {
        var current = await _db.NisCustomerGroups.FirstOrDefaultAsync(g => g.CmpId == cmpId && g.CustomerId == customerId, ct);
        var currentMerge = string.IsNullOrWhiteSpace(current?.MergeIntoCustomerId) ? null : current!.MergeIntoCustomerId;

        if (input.HasMergeIntoCustomerId)
        {
            var mergeInto = input.MergeIntoCustomerId;
            if (mergeInto == customerId)
                throw new NisArchiveBadRequestException("รวมรหัสเข้ากับตัวเองไม่ได้");

            // (A) รหัสนี้ถือธงอยู่เอง แล้วคำขอจะพาไปเป็นรหัสรอง → ปฏิเสธเสมอ (ระบบยังไม่มีกลไก "ยกธง" ไปรหัสหลักใหม่)
            if (current?.Backdated == true && mergeInto is not null)
            {
                throw new NisArchiveBadRequestException(
                    $"\"{customerId}\" ถือธงบันทึกย้อนหลังอยู่ — ปลดธงที่รหัสนี้ก่อน (ส่งคำขอแยกต่างหาก " +
                    "PUT .../settings {\"backdated\":false}) แล้วค่อยรวมเข้ากลุ่มอื่น ระบบยังไม่รองรับการยกธง" +
                    "ขึ้นไปเป็นของรหัสหลักใหม่โดยอัตโนมัติ — เป็นการตัดสินใจที่ต้องมีคนยืนยันว่ากลุ่มใหม่ทั้งกลุ่ม" +
                    "ควรติดธงไปด้วยหรือไม่ ไม่ใช่ทำแทนให้เงียบ ๆ");
            }

            // (B) สมาชิกของกลุ่มที่ติดธง (ตัวเองถือ หรือรหัสหลักที่สังกัดถือ) กำลังเปลี่ยนสมาชิกภาพ → ต้องส่ง backdated มาชัดเจน
            var currentGroupBackdated = current?.Backdated ?? false;
            if (!currentGroupBackdated && currentMerge is not null)
            {
                var currentPrimary = await _db.NisCustomerGroups.AsNoTracking()
                    .FirstOrDefaultAsync(g => g.CmpId == cmpId && g.CustomerId == currentMerge, ct);
                currentGroupBackdated = currentPrimary?.Backdated ?? false;
            }
            var membershipChanging = mergeInto != currentMerge;
            if (currentGroupBackdated && membershipChanging && !input.HasBackdated)
            {
                throw new NisArchiveBadRequestException(
                    $"\"{customerId}\" อยู่ในกลุ่มที่ติดธงบันทึกย้อนหลังอยู่ตอนนี้ — เปลี่ยนการรวมกลุ่ม (รวมใหม่/" +
                    "ยกเลิกการรวม) ตอนนี้จะทำให้ไม่ชัดเจนว่าธงควรตามไปด้วยหรือไม่ ส่งค่า \"backdated\" มาพร้อมกัน " +
                    "ในคำขอเดียวกันเสมอ (true = ให้ข้อมูลของรหัสนี้ยังถือว่าเชื่อถือไม่ได้ต่อไป, false = ยืนยันว่า" +
                    "ข้อมูลของรหัสนี้แยกต่างหากและเชื่อถือได้) — ห้ามปล่อยให้เป็นค่าเริ่มต้นเงียบ ๆ");
            }

            if (mergeInto is not null)
            {
                // 🔴 ห้ามชี้ไปหารหัสที่เป็นรหัสรองอยู่แล้ว (กันห่วง A→B→A และทอด A→B→C) — กลุ่มลึกชั้นเดียวเสมอ
                var target = await _db.NisCustomerGroups.AsNoTracking()
                    .FirstOrDefaultAsync(g => g.CmpId == cmpId && g.CustomerId == mergeInto, ct);
                if (!string.IsNullOrWhiteSpace(target?.MergeIntoCustomerId))
                {
                    throw new NisArchiveBadRequestException(
                        $"รหัส \"{mergeInto}\" เป็นรหัสรองของ \"{target!.MergeIntoCustomerId}\" อยู่แล้ว — " +
                        $"ชี้ต่อเป็นทอดไม่ได้ ให้รวมเข้ากับ \"{target.MergeIntoCustomerId}\" โดยตรงแทน");
                }
                // มีใครชี้มาหา customerId นี้อยู่แล้วไหม (= เป็นรหัสหลักของกลุ่มอื่น)
                var dependents = await _db.NisCustomerGroups.AsNoTracking()
                    .CountAsync(g => g.CmpId == cmpId && g.MergeIntoCustomerId == customerId, ct);
                if (dependents > 0)
                {
                    throw new NisArchiveBadRequestException(
                        $"รหัส \"{customerId}\" เป็นรหัสหลักที่มีรหัสอื่นรวมเข้ามาอยู่แล้ว — ต้องยกเลิกการรวมเหล่านั้นก่อน");
                }
            }
        }

        if (input.HasBackdated && input.Backdated == true)
        {
            // 🔴 ธงเป็นของกลุ่ม อยู่ที่รหัสหลักเท่านั้น — ถ้ารหัสนี้เป็น/กำลังจะเป็นรหัสรอง ปฏิเสธการ**ตั้ง**ธง (false ผ่านได้เสมอ)
            var willBeMerged = input.HasMergeIntoCustomerId ? input.MergeIntoCustomerId is not null : currentMerge is not null;
            if (willBeMerged)
            {
                throw new NisArchiveBadRequestException(
                    $"\"{customerId}\" เป็นรหัสรอง — ตั้งธงบันทึกย้อนหลังที่รหัสหลักแทน (ธงเป็นสมบัติของทั้งกลุ่ม)");
            }
        }

        var now = DateTimeOffset.UtcNow;
        if (current is null)
        {
            current = new NisCustomerGroup
            {
                CmpId = cmpId,
                CustomerId = customerId,
                MergeIntoCustomerId = null,
                Backdated = false,
                CreatedAt = now,
            };
            _db.NisCustomerGroups.Add(current);
        }
        if (input.HasMergeIntoCustomerId) current.MergeIntoCustomerId = input.MergeIntoCustomerId;
        if (input.HasBackdated) current.Backdated = input.Backdated ?? false;
        current.UpdatedAt = now;
        current.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync(ct);

        return ToDto(customerId, current);
    }

    /// <summary>
    /// หน้า "ลูกค้า" — หนึ่งแถวต่อกลุ่ม · ดึงเคสทั้งก้อนครั้งเดียว (ไม่รวมข้อมูลทดสอบ) แล้ว bucket ตามรหัสหลัก
    /// กันนับซ้ำด้วย CaseNo (fallback SourceId) · ขอบ "เดือนนี้"/"ตั้งแต่ ม.ค." เป็นปฏิทินไทย ปิดขอบบนด้วย now
    /// · ธงอ่านจากแถวรหัสหลักเท่านั้น · สมาชิกที่ยังไม่มีเคสถูกเติมจากตารางกลุ่ม (ไม่หายจากป้าย "รวม N รหัส")
    /// </summary>
    /// <returns>แถวเรียง intakeYtd มาก→น้อย + ปี ค.ศ. ที่ใช้คำนวณ</returns>
    public async Task<(List<NisCustomerOverviewRowDto> Rows, int YearAd)> OverviewAsync(string cmpId, CancellationToken ct = default)
    {
        var customerRows = await _db.NisCustomerGroups.AsNoTracking().Where(g => g.CmpId == cmpId).ToListAsync(ct);
        var rawCases = await _archive.BuildWhere(cmpId, new NisCaseFilter())
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.SourceId)
            .Select(c => new { c.CaseNo, c.SourceId, c.CustomerId, c.CustomerName, c.CreatedAt, c.ResolvedAt, c.ResolutionHours })
            .ToListAsync(ct);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var cases = rawCases.Where(c => seen.Add(string.IsNullOrEmpty(c.CaseNo) ? c.SourceId : c.CaseNo)).ToList();

        var mergeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var secondariesByPrimary = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var backdatedPrimaries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in customerRows)
        {
            if (!string.IsNullOrWhiteSpace(c.MergeIntoCustomerId))
            {
                mergeMap[c.CustomerId] = c.MergeIntoCustomerId!;
                if (!secondariesByPrimary.TryGetValue(c.MergeIntoCustomerId!, out var list)) secondariesByPrimary[c.MergeIntoCustomerId!] = list = new List<string>();
                list.Add(c.CustomerId);
            }
            if (c.Backdated) backdatedPrimaries.Add(c.CustomerId);
        }
        string ResolvePrimary(string id) => mergeMap.TryGetValue(id, out var p) ? p : id;

        var now = NisBangkokTime.Now();
        var (monthStart, _) = NisBangkokTime.MonthBounds(now);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, NisBangkokTime.Offset);

        var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
        Group GroupFor(string primary)
        {
            if (!groups.TryGetValue(primary, out var g))
                groups[primary] = g = new Group(primary, backdatedPrimaries.Contains(primary));
            return g;
        }

        foreach (var c in cases)
        {
            var g = GroupFor(ResolvePrimary(c.CustomerId));
            g.MemberCustomerIds.Add(c.CustomerId);
            g.Names.TryAdd(c.CustomerId, c.CustomerName);
            if (c.CreatedAt >= monthStart && c.CreatedAt <= now) g.IntakeThisMonth++;
            if (c.CreatedAt >= yearStart && c.CreatedAt <= now) g.IntakeYtd++;
            if (c.ResolvedAt is null) g.OpenNow++;
            if (c.ResolvedAt is { } ra && ra >= yearStart && ra <= now && c.ResolutionHours is { } h) g.ResolvedYtdHours.Add(h);
        }

        foreach (var g in groups.Values)
            if (secondariesByPrimary.TryGetValue(g.PrimaryCustomerId, out var secs))
                foreach (var s in secs) g.MemberCustomerIds.Add(s);

        var rows = groups.Values.Select(g =>
        {
            // ชื่อของรหัสหลักก่อนเสมอ · ไม่มีเคสของตัวเอง → ชื่อสมาชิกรหัสแรกตามลำดับ ASCII (ผลเดาได้ซ้ำ)
            var fallbackName = g.Names.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value).FirstOrDefault();
            var name = g.Names.TryGetValue(g.PrimaryCustomerId, out var n) ? n : fallbackName ?? g.PrimaryCustomerId;
            var (median, _, _, cnt) = NisHoursStat.HoursStat(g.ResolvedYtdHours);
            return new NisCustomerOverviewRowDto
            {
                CustomerId = g.PrimaryCustomerId,
                CustomerName = name,
                MemberCustomerIds = g.MemberCustomerIds.OrderBy(x => x, StringComparer.Ordinal).ToList(),
                Backdated = g.Backdated,
                IntakeThisMonth = g.IntakeThisMonth,
                IntakeYtd = g.IntakeYtd,
                OpenNow = g.OpenNow,
                ResolutionHoursMedian = g.Backdated ? null : median,
                ResolutionHoursN = g.Backdated ? 0 : cnt,
                // SLA ยังไม่คำนวณที่หน้านี้ — ไม่ให้มีสูตรที่สามที่ไม่ตรงกับ NisAnalyticsService.Sla (สวิตช์ปิด)
                SlaWithinPct = null,
            };
        })
        .OrderByDescending(r => r.IntakeYtd)
        .ToList();

        return (rows, now.Year);
    }

    private sealed class Group
    {
        public Group(string primary, bool backdated) { PrimaryCustomerId = primary; Backdated = backdated; }
        public string PrimaryCustomerId { get; }
        public bool Backdated { get; }
        public Dictionary<string, string> Names { get; } = new(StringComparer.Ordinal);
        public HashSet<string> MemberCustomerIds { get; } = new(StringComparer.Ordinal);
        public int IntakeThisMonth { get; set; }
        public int IntakeYtd { get; set; }
        public int OpenNow { get; set; }
        public List<double> ResolvedYtdHours { get; } = new();
    }
}
