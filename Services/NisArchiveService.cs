using System.Linq.Expressions;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// อ่าน archive ของ NIS Service Report — port จาก apps/api/src/cases/cases.service.ts (+ ส่วน status ของ sync.controller)
/// EF Core LINQ ล้วน ไม่ string-concat SQL · Autofac auto-register เป็น <see cref="INisArchiveService"/>
/// </summary>
public class NisArchiveService : INisArchiveService
{
    /// <summary>เกณฑ์ความยาวช่อง "วิธีแก้ไขปัญหา" ที่ถือว่าสั้น — คุณ Tone ตัดสิน 5 ก.ย. 2569 ว่าใช้ 15</summary>
    public const int ShortSolutionChars = 15;

    private readonly DatabaseContext _db;
    private readonly NisCustomerGroupResolverService _groups;
    private readonly NisArchiveSyncService _sync;
    private readonly double _alertMissingPct;

    public NisArchiveService(
        DatabaseContext db,
        NisCustomerGroupResolverService groups,
        NisArchiveSyncService sync,
        IConfiguration configuration)
    {
        _db = db;
        _groups = groups;
        _sync = sync;
        _alertMissingPct = configuration.GetValue<double?>("NisReport:AlertMissingPct") ?? 5;
    }

    /// <inheritdoc />
    public IQueryable<NisServiceCase> BuildWhere(string cmpId, NisCaseFilter f)
    {
        var q = _db.NisServiceCases.AsNoTracking().Where(c => c.CmpId == cmpId);

        if (!f.IncludeTest) q = q.Where(c => !c.IsTestData);

        var from = f.FromAt;
        var to = f.ToAt;
        var hasRange = from.HasValue || to.HasValue;
        var byResolved = f.DateField == "resolvedAt";

        // 🔴 กติกาตัวเลขสองชุด — ช่วงวันที่กรองด้วย dateField เสมอ (createdAt = แจ้งเข้า · resolvedAt = ปิดได้)
        if (!byResolved)
        {
            if (from.HasValue) q = q.Where(c => c.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(c => c.CreatedAt <= to.Value);
        }

        if (f.Unresolved)
        {
            // "ยังไม่ปิด" กับ "ปิดในช่วงนี้" ขัดกันโดยนิยาม — เลือกทั้งคู่ต้องได้เซตว่าง ไม่ใช่เงื่อนไขใดชนะเงียบ ๆ
            q = byResolved && hasRange
                ? q.Where(c => c.ResolvedAt < DateTimeOffset.UnixEpoch)
                : q.Where(c => c.ResolvedAt == null);
        }
        else if (byResolved)
        {
            if (from.HasValue) q = q.Where(c => c.ResolvedAt >= from.Value);
            if (to.HasValue) q = q.Where(c => c.ResolvedAt <= to.Value);
            // เลือกนับ "ปิดได้" แปลว่าเคสที่ยังไม่ปิดไม่นับ แม้จะไม่ได้ใส่ช่วงวันที่
            if (!hasRange) q = q.Where(c => c.ResolvedAt != null);
        }

        if (f.CustomerId is { Count: > 0 }) { var ids = f.CustomerId; q = q.Where(c => ids.Contains(c.CustomerId)); }
        if (f.Status is { Count: > 0 }) { var st = f.Status; q = q.Where(c => st.Contains(c.Status)); }
        if (f.Priority is { Count: > 0 }) { var pr = f.Priority; q = q.Where(c => pr.Contains(c.Priority)); }
        if (f.Category is { Count: > 0 }) { var cat = f.Category; q = q.Where(c => cat.Contains(c.Category)); }
        if (f.Assignee is { Count: > 0 }) q = q.Where(AnyAssignee(f.Assignee));

        // null ไม่เข้าเงื่อนไข — "ไม่ได้ให้คะแนน" ไม่ใช่ "ให้คะแนนต่ำ" (SQL เทียบ NULL ตกไปเองอยู่แล้ว)
        if (f.MaxSatisfaction.HasValue) { var max = f.MaxSatisfaction.Value; q = q.Where(c => c.SatisfactionScore <= max); }

        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            // ค้น 4 ช่อง — actionDetails คือ "วิธีแก้ไขปัญหา" ที่คนหาย้อนหลังบ่อยที่สุด ห้ามขาด
            var s = f.Q.Trim();
            q = q.Where(c =>
                c.Subject.Contains(s) ||
                c.CaseNo.Contains(s) ||
                c.CustomerName.Contains(s) ||
                (c.ActionDetails != null && c.ActionDetails.Contains(s)));
        }
        return q;
    }

    /// <summary>OR ของ AssigneeNamesJson LIKE '%"ชื่อ"%' ต่อชื่อ — ค้นในสตริง JSON ตรง ๆ (ยอมรับได้ที่ขนาดคลังหลักพันแถว)</summary>
    private static Expression<Func<NisServiceCase, bool>> AnyAssignee(IEnumerable<string> names)
    {
        var p = Expression.Parameter(typeof(NisServiceCase), "c");
        var prop = Expression.Property(p, nameof(NisServiceCase.AssigneeNamesJson));
        var contains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        Expression? body = null;
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var call = Expression.Call(prop, contains, Expression.Constant(NisCaseAdapter.JsonNeedle(name.Trim())));
            body = body is null ? call : Expression.OrElse(body, call);
        }
        return Expression.Lambda<Func<NisServiceCase, bool>>(body ?? Expression.Constant(false), p);
    }

    /// <summary>
    /// เรียง — 🔴 แถวที่ยังไม่ปิด / ไม่มีคะแนน ต้องไปท้ายสุดเสมอไม่ว่าทิศไหน (SPEC §9 ข้อ 28)
    /// priority เรียงตัวอักษร (critical &lt; major &lt; medium &lt; minor) ซึ่ง **บังเอิญ** ตรงลำดับความด่วน
    /// </summary>
    private static IOrderedQueryable<NisServiceCase> ApplySort(IQueryable<NisServiceCase> q, string sort, bool desc)
    {
        IOrderedQueryable<NisServiceCase> ordered = sort switch
        {
            "resolvedAt" => desc
                ? q.OrderBy(c => c.ResolvedAt == null).ThenByDescending(c => c.ResolvedAt)
                : q.OrderBy(c => c.ResolvedAt == null).ThenBy(c => c.ResolvedAt),
            "satisfactionScore" => desc
                ? q.OrderBy(c => c.SatisfactionScore == null).ThenByDescending(c => c.SatisfactionScore)
                : q.OrderBy(c => c.SatisfactionScore == null).ThenBy(c => c.SatisfactionScore),
            "priority" => desc ? q.OrderByDescending(c => c.Priority) : q.OrderBy(c => c.Priority),
            "customerName" => desc ? q.OrderByDescending(c => c.CustomerName) : q.OrderBy(c => c.CustomerName),
            _ => desc ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
        };
        return ordered.ThenBy(c => c.SourceId);
    }

    /// <summary>
    /// ล้างตัวเลขระยะเวลาของกลุ่มลูกค้าที่ติดธงบันทึกย้อนหลัง (SPEC §9 ข้อ 59/D1) — "ในทุกที่ที่แสดงผล"
    /// จำนวนเคสยังนับปกติ · คะแนนความพึงพอใจไม่ล้าง (มาจากลูกค้า ไม่ได้มาจากตราเวลา)
    /// </summary>
    private static NisServiceCaseDto ClearBackdated(NisServiceCaseDto d, HashSet<string> backdated)
    {
        if (backdated.Count > 0 && backdated.Contains(d.CustomerId)) d.ResolutionHours = null;
        return d;
    }

    /// <inheritdoc />
    public async Task<NisCaseListResult> ListCasesAsync(string cmpId, NisCaseQuery q, CancellationToken ct = default)
    {
        var where = BuildWhere(cmpId, q);

        // ตัวเลขสรุปของ "ทั้งชุดที่ตัวกรองคัดไว้" — นับที่ฐานจาก where เดียวกับตาราง ไม่ใช่จากหน้าที่แสดง
        var total = await where.CountAsync(ct);
        var unresolved = await where.CountAsync(c => c.ResolvedAt == null, ct);
        // NULL นับเป็น 0 (ไม่บันทึกเลย = สั้นที่สุด) — เทียบกับ docs/23-ACTIONDETAILS-LENGTH.md ได้ตรง ๆ
        var shortSolution = await where.CountAsync(c => (c.ActionDetails ?? "").Trim().Length < ShortSolutionChars, ct);

        var backdated = await _groups.BackdatedCustomerIdsAsync(cmpId, ct);
        var desc = q.Order == "desc";
        List<NisServiceCaseDto> rows;

        if (q.Sort == "resolutionHours")
        {
            // 🔴 ต้องเรียง**หลัง**ล้างค่าของกลุ่มบันทึกย้อนหลัง — ไม่งั้นแถวที่ถูกล้างจะแทรกกลางแถวที่มีตัวเลขจริง
            // แทนที่จะไปกองท้ายสุด · ดึงทั้งชุดที่กรองแล้วมาเรียง+แบ่งหน้าในโค้ด (คลังมีหลักพันแถว ปลอดภัย)
            var all = await where.OrderBy(c => c.SourceId).Select(NisCaseRow.Projection).ToListAsync(ct);
            var cleared = all.Select(NisServiceCaseDto.From).Select(d => ClearBackdated(d, backdated)).ToList();
            var dir = desc ? -1 : 1;
            cleared.Sort((a, b) =>
            {
                if (a.ResolutionHours is null && b.ResolutionHours is null) return string.CompareOrdinal(a.SourceId, b.SourceId);
                if (a.ResolutionHours is null) return 1; // null ไปท้ายสุดเสมอไม่ว่าทิศไหน
                if (b.ResolutionHours is null) return -1;
                var cmp = a.ResolutionHours.Value.CompareTo(b.ResolutionHours.Value) * dir;
                return cmp != 0 ? cmp : string.CompareOrdinal(a.SourceId, b.SourceId);
            });
            rows = cleared.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToList();
        }
        else
        {
            var page = await ApplySort(where, q.Sort, desc)
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(NisCaseRow.Projection)
                .ToListAsync(ct);
            rows = page.Select(NisServiceCaseDto.From).Select(d => ClearBackdated(d, backdated)).ToList();
        }

        return new NisCaseListResult
        {
            Rows = rows,
            Total = total,
            UnresolvedCount = unresolved,
            ShortSolutionCount = shortSolution,
            ShortSolutionChars = ShortSolutionChars,
            Page = q.Page,
            PageSize = q.PageSize,
            PageCount = (int)Math.Ceiling(total / (double)q.PageSize),
            DateField = q.DateField,
            // บอกกลับเสมอว่านับด้วยตัวไหน — หน้าเว็บต้องเอาไปขึ้นป้าย (SPEC 9.1 ข้อ 1)
            CountLabel = q.DateField == "createdAt" ? "งานที่แจ้งเข้า" : "งานที่ปิดได้",
            IncludeTest = q.IncludeTest,
        };
    }

    /// <inheritdoc />
    public async Task<NisServiceCaseDto?> GetCaseAsync(string cmpId, string id, CancellationToken ct = default)
    {
        var row = await _db.NisServiceCases.AsNoTracking()
            .Where(c => c.CmpId == cmpId && (c.SourceId == id || c.CaseNo == id || c.ExternalGuid == id))
            .OrderBy(c => c.SourceId)
            .Select(NisCaseRow.Projection)
            .FirstOrDefaultAsync(ct);
        if (row is null) return null;
        var backdated = await _groups.BackdatedCustomerIdsAsync(cmpId, ct);
        return ClearBackdated(NisServiceCaseDto.From(row), backdated);
    }

    /// <inheritdoc />
    public async Task<List<NisCustomerSummaryDto>> ListCustomersAsync(string cmpId, CancellationToken ct = default)
    {
        var groups = await _db.NisServiceCases.AsNoTracking()
            .Where(c => c.CmpId == cmpId && !c.IsTestData)
            .GroupBy(c => new { c.CustomerId, c.CustomerName })
            .Select(g => new NisCustomerSummaryDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                IntakeAllTime = g.Count(),
            })
            .ToListAsync(ct);
        return groups.OrderByDescending(g => g.IntakeAllTime).ThenBy(g => g.CustomerId, StringComparer.Ordinal).ToList();
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset?> DataAsOfAsync(string cmpId, CancellationToken ct = default) =>
        (await FreshnessAsync(cmpId, ct)).DataAsOf;

    private sealed record LogCols(DateTimeOffset StartedAt, DateTimeOffset? FinishedAt, string Status, int? InvalidCount, int? MissingCount, int? ParsedCount);

    /// <inheritdoc />
    public async Task<NisFreshnessDto> FreshnessAsync(string cmpId, CancellationToken ct = default)
    {
        var logs = _db.NisSyncLogs.AsNoTracking().Where(l => l.CmpId == cmpId);

        var last = await logs.OrderByDescending(l => l.StartedAt)
            .Select(l => new LogCols(l.StartedAt, l.FinishedAt, l.Status, l.InvalidCount, l.MissingCount, l.ParsedCount))
            .FirstOrDefaultAsync(ct);

        // หา "รอบที่ครบล่าสุด" จากรอบ ok เท่านั้น ไม่ใช่กวาดหน้าต่าง N แถวล่าสุดที่ความล้มกินหมด
        var oks = await logs.Where(l => l.Status == "ok")
            .OrderByDescending(l => l.StartedAt)
            .Take(20)
            .Select(l => new LogCols(l.StartedAt, l.FinishedAt, l.Status, l.InvalidCount, l.MissingCount, l.ParsedCount))
            .ToListAsync(ct);

        var complete = oks.FirstOrDefault(r => WhyIncomplete(r) is null);
        return new NisFreshnessDto
        {
            DataAsOf = complete is null ? null : (complete.FinishedAt ?? complete.StartedAt),
            LastRunAt = last is null ? null : (last.FinishedAt ?? last.StartedAt),
            LastRunStatus = last?.Status,
            IncompleteReason = last is null ? "ยังไม่เคย sync เลย" : WhyIncomplete(last),
        };
    }

    /// <summary>เกณฑ์ความครบถ้วนสามข้อ (SPEC §9 ข้อ 43) — status ok · invalid = 0 · missing% ต่ำกว่าเกณฑ์เตือน</summary>
    private string? WhyIncomplete(LogCols r)
    {
        if (r.Status != "ok") return $"รอบล่าสุดสถานะ {r.Status}";
        if ((r.InvalidCount ?? 0) > 0) return $"แปลงไม่ผ่าน {r.InvalidCount} record — archive ขาดแถวเหล่านั้น";
        var total = (r.MissingCount ?? 0) + (r.ParsedCount ?? 0);
        var pct = total > 0 ? (r.MissingCount ?? 0) * 100.0 / total : 0;
        if (pct >= _alertMissingPct)
            return $"ต้นทางส่งน้อยลง {pct:F1}% ({r.MissingCount} record ที่เคยมีหายไปจากรอบนี้)";
        return null;
    }

    /// <inheritdoc />
    public async Task<NisSyncStatusDto> SyncStatusAsync(string cmpId, CancellationToken ct = default)
    {
        var cases = _db.NisServiceCases.AsNoTracking().Where(c => c.CmpId == cmpId);
        var archive = new NisSyncArchiveDto
        {
            Cases = await cases.CountAsync(ct),
            TestCases = await cases.CountAsync(c => c.IsTestData, ct),
            Customers = await cases.Where(c => !c.IsTestData).Select(c => c.CustomerId).Distinct().CountAsync(ct),
            OldestCase = NisServiceCaseDto.Iso(await cases.MinAsync(c => (DateTimeOffset?)c.CreatedAt, ct)),
            NewestCase = NisServiceCaseDto.Iso(await cases.MaxAsync(c => (DateTimeOffset?)c.CreatedAt, ct)),
        };

        var recent = await _db.NisSyncLogs.AsNoTracking()
            .Where(l => l.CmpId == cmpId)
            .OrderByDescending(l => l.StartedAt)
            .Take(5)
            .ToListAsync(ct);

        return new NisSyncStatusDto
        {
            Archive = archive,
            Sync = _sync.GetStatus(cmpId),
            RecentRuns = recent.Select(NisSyncLogDto.From).ToList(),
        };
    }
}
