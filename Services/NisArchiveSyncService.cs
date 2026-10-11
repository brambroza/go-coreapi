using System.Collections.Concurrent;
using System.Diagnostics;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Helpers;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// นโยบายถอยจังหวะเมื่อต้นทางล้มติดกัน (port จาก packages/shared/src/sync-backoff.ts)
/// · ล้มน้อยกว่า StepAt[0] ครั้ง → จังหวะปกติ · ถึงขั้นถัดไป → คูณขึ้นแต่ไม่เกิน MaxMin (เพดานแข็ง 60 นาที)
/// · สำเร็จหนึ่งครั้งกลับสู่จังหวะปกติทันที · ใช้กับรอบอัตโนมัติเท่านั้น ไม่ใช้กับการกดปุ่มเอง
/// </summary>
public sealed record NisSyncBackoffPolicy(int BaseMin, int MaxMin, int[] StepAt, int[] Factor)
{
    /// <summary>เพดานแข็ง — ตั้งค่าเกินนี้ไม่ได้ (ถอยนานกว่านี้ทำให้ด่านความสดปฏิเสธการออกรายงาน)</summary>
    public const int CeilingMin = 60;

    /// <summary>ค่าเผื่อตัวจับเวลาที่ยิงคลาด — ค่าคงที่เล็ก ไม่ใช่สัดส่วนของจังหวะ</summary>
    private const long TimerSlackMs = 30_000;

    /// <summary>ประกอบนโยบาย — ที่เดียวที่ตัด MaxMin ให้อยู่ในเพดาน</summary>
    public static NisSyncBackoffPolicy Create(int baseMin, int maxMin) =>
        new(Math.Max(1, baseMin), Math.Min(CeilingMin, Math.Max(1, maxMin)), new[] { 2, 4 }, new[] { 2, 4 });

    /// <summary>จังหวะที่ควรใช้ตอนนี้ (นาที) — คิดจากจำนวนครั้งที่ล้มติดกัน</summary>
    public int BackoffMinutes(int failStreak)
    {
        var mult = 1;
        for (var i = 0; i < StepAt.Length && i < Factor.Length; i++)
            if (failStreak >= StepAt[i]) mult = Factor[i];
        return Math.Min(Math.Min(CeilingMin, Math.Max(1, MaxMin)), Math.Max(1, BaseMin) * mult);
    }

    /// <summary>เวลาที่ควรลองรอบถัดไป (ms epoch) — นับจากเวลาที่รอบล่าสุด**เริ่ม**</summary>
    public long NextAttemptAt(long lastAttemptMs, int failStreak) => lastAttemptMs + BackoffMinutes(failStreak) * 60_000L;

    /// <summary>ถึงเวลาลองรอบถัดไปหรือยัง</summary>
    public bool ShouldAttempt(long nowMs, long? lastAttemptMs, int failStreak)
    {
        if (lastAttemptMs is null) return true;
        var slack = Math.Min(TimerSlackMs, Math.Max(1, BaseMin) * 60_000L / 4);
        return nowMs >= NextAttemptAt(lastAttemptMs.Value, failStreak) - slack;
    }
}

/// <summary>
/// NIS Service Report — คัดลอกเคสจาก SP getProblem* (ผ่าน <see cref="NisProblemSourceService"/>) ลง archive
/// <see cref="NisServiceCase"/> ทุก <c>NisReport:SyncEveryMinutes</c> นาที ต่อบริษัทใน <c>NisReport:CmpIds</c>
/// port จาก apps/api/src/sync/sync.service.ts
///
/// · upsert ด้วย (CmpId, SourceId) · แถวที่ ModifiedAt เท่าเดิม (และไม่ null) นับ unchanged ไม่เขียนซ้ำ
/// · 🔴 ไม่ลบแถวที่ต้นทางไม่ส่งมาแล้ว — นับเป็น missing ใน NisSyncLog เท่านั้น
/// · ทุกรอบครอบด้วย <see cref="SqlAppLock"/> (<c>goalongapi:nis-archive-sync</c>) ให้ไม่รันพร้อมกันข้าม replica
///   งาน idempotent อยู่แล้ว (upsert) จึงรันซ้ำแบบเรียงกันได้
/// · fail streak / backoff / ผลล่าสุด เก็บในหน่วยความจำต่อบริษัท — ต่อ replica (status ที่เห็นคือของ instance ที่ตอบ)
/// · แจ้งเตือน (fail ติดกัน ≥ AlertFailStreak · missing ≥ AlertMissingPct%) = LogWarning ข้อความไทยเดิม วันละครั้งต่อเรื่อง
///
/// register เป็น singleton + AddHostedService(sp => sp.GetRequiredService&lt;...&gt;()) ใน Program.cs ให้ controller
/// (sync/run · sync/status) ใช้ instance เดียวกับ loop — ไม่ใช่ register ซ้อน
/// </summary>
public class NisArchiveSyncService : BackgroundService
{
    /// <summary>ชื่อ lock ใน sp_getapplock — ต้องไม่ซ้ำกับ BackgroundService ตัวอื่น</summary>
    public const string LockResource = "goalongapi:nis-archive-sync";

    private const int Chunk = 200;
    private static readonly TimeSpan BootDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// แถว running ที่เก่ากว่านี้ถือว่าถูกตัดกลางคัน — ใช้เกณฑ์อายุแทน "StartedAt &lt; เวลาบูต" ของ TS
    /// เพราะ prod มีหลาย replica: replica ที่เพิ่งบูตห้ามไปตีตรา interrupted ให้รอบที่ replica อื่นกำลังวิ่งอยู่จริง
    /// (รอบปกติจบในไม่กี่สิบวินาที · 30 นาทีคือเพดานที่ไม่มีทางเป็นรอบจริง)
    /// </summary>
    private static readonly TimeSpan StaleRunningAfter = TimeSpan.FromMinutes(30);

    private sealed class CmpState
    {
        public readonly SemaphoreSlim Gate = new(1, 1);
        public volatile bool Running;
        public int FailStreak;
        public long? LastAttemptMs;
        public int BackoffSkips;
        public NisSyncResultDto? Last;
    }

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SqlAppLock _appLock;
    private readonly ILogger<NisArchiveSyncService> _logger;

    private readonly int _everyMin;
    private readonly string _window;
    private readonly bool _onBoot;
    private readonly int _alertFailStreak;
    private readonly double _alertMissingPct;
    private readonly string _syncUsername;
    private readonly string[] _cmpIds;
    private readonly NisBusinessHoursConfig _biz;
    private readonly NisSyncBackoffPolicy _policy;

    private readonly ConcurrentDictionary<string, CmpState> _states = new(StringComparer.Ordinal);
    /// <summary>dedupeKey → วันไทย 'YYYY-MM-DD' ที่เตือนไปแล้ว (เตือนซ้ำได้วันละครั้ง)</summary>
    private readonly ConcurrentDictionary<string, string> _alertState = new(StringComparer.Ordinal);

    public NisArchiveSyncService(
        IServiceScopeFactory scopeFactory,
        SqlAppLock appLock,
        IConfiguration configuration,
        ILogger<NisArchiveSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _appLock = appLock;
        _logger = logger;

        _everyMin = Math.Max(1, configuration.GetValue<int?>("NisReport:SyncEveryMinutes") ?? 15);
        _window = (configuration["NisReport:SyncWindow"] ?? "all").Trim().ToLowerInvariant() == "biz" ? "biz" : "all";
        _onBoot = configuration.GetValue<bool?>("NisReport:SyncOnBoot") ?? true;
        _alertFailStreak = Math.Max(1, configuration.GetValue<int?>("NisReport:AlertFailStreak") ?? 3);
        _alertMissingPct = configuration.GetValue<double?>("NisReport:AlertMissingPct") ?? 5;
        _syncUsername = (configuration["NisReport:SyncUsername"] ?? string.Empty).Trim();
        _cmpIds = configuration.GetSection("NisReport:CmpIds").Get<string[]>()?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct().ToArray()
                  ?? Array.Empty<string>();
        // ตรวจรูป HH:mm / start<end ตั้งแต่ startup — คอนฟิกผิดต้องล้มดัง ไม่ใช่ sync เงียบนอกหน้าต่าง
        _biz = NisBusinessHoursConfig.FromConfiguration(configuration);
        _policy = NisSyncBackoffPolicy.Create(_everyMin, configuration.GetValue<int?>("NisReport:BackoffMaxMinutes") ?? NisSyncBackoffPolicy.CeilingMin);
    }

    /// <summary>บริษัทที่เปิดใช้ sync (จาก NisReport:CmpIds)</summary>
    public IReadOnlyList<string> CmpIds => _cmpIds;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // หน่วงตอน start ให้ app/db พร้อมก่อน (offset จาก hosted service ตัวอื่น)
        try { await Task.Delay(BootDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        if (_cmpIds.Length == 0)
            _logger.LogWarning("NIS archive sync: NisReport:CmpIds ว่าง — ไม่มีบริษัทให้ sync");
        if (string.IsNullOrEmpty(_syncUsername))
            _logger.LogWarning("NIS archive sync: ยังไม่ตั้งค่า NisReport:SyncUsername — จะข้ามทุกบริษัทจนกว่าจะตั้งค่า");

        _logger.LogInformation(
            "NIS archive sync: ตั้งรอบ sync ทุก {EveryMin} นาที (window={Window}) · ถอยจังหวะได้ถึง {MaxMin} นาทีเมื่อล้มติดกัน · บริษัท {Cmps}",
            _everyMin, _window, _policy.MaxMin, string.Join(",", _cmpIds));

        await CloseOrphanRunsAsync(stoppingToken);

        if (_onBoot)
            foreach (var cmp in _cmpIds)
                await RunSafeAsync(cmp, "boot", stoppingToken);

        // ติ๊กทุก everyMin เท่าเดิม · การถอยจังหวะตัดสินใน TickAsync ไม่ใช่ด้วยการตั้ง timer ใหม่
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_everyMin));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CloseOrphanRunsAsync(stoppingToken);
                foreach (var cmp in _cmpIds)
                    await TickAsync(cmp, stoppingToken);
            }
        }
        catch (OperationCanceledException) { /* app กำลังปิด */ }
    }

    /// <summary>สถานะ runtime ของ sync ของบริษัทหนึ่ง (ใช้ใน sync/status)</summary>
    public NisSyncRuntimeDto GetStatus(string cmpId)
    {
        var s = StateOf(cmpId);
        var inWindow = InWindow();
        return new NisSyncRuntimeDto
        {
            Running = s.Running,
            FailStreak = s.FailStreak,
            EveryMin = _everyMin,
            Backoff = new NisSyncBackoffDto
            {
                EveryMinNow = _policy.BackoffMinutes(s.FailStreak),
                MaxMin = _policy.MaxMin,
                // นอกหน้าต่าง รอบถัดไปไม่ได้ขึ้นกับการถอยจังหวะแต่ขึ้นกับเวลาเปิดทำการ
                BlockedBy = !inWindow ? "window" : s.FailStreak > 0 ? "backoff" : null,
                NextAttemptAt = s.LastAttemptMs is null || !inWindow
                    ? null
                    : NisServiceCaseDto.Iso(DateTimeOffset.FromUnixTimeMilliseconds(_policy.NextAttemptAt(s.LastAttemptMs.Value, s.FailStreak))),
                SkippedRounds = s.BackoffSkips,
            },
            Window = _window,
            Last = s.Last,
        };
    }

    /// <summary>
    /// รัน sync หนึ่งรอบของบริษัทหนึ่ง — ใช้ทั้งจาก loop และ endpoint POST sync/run
    /// คืน status=skipped พร้อม reason เมื่อ: รอบก่อนยังไม่จบ · นอกหน้าต่าง (cron เท่านั้น) · ไม่มี SyncUsername · lock ถูก instance อื่นถือ
    /// </summary>
    /// <param name="cmpId">รหัสบริษัท</param>
    /// <param name="trigger">cron | boot | manual — เก็บลง NisSyncLog.Trigger</param>
    public async Task<NisSyncResultDto> RunOnceAsync(string cmpId, string trigger, CancellationToken ct)
    {
        var state = StateOf(cmpId);

        if (trigger == "cron" && !InWindow())
        {
            _logger.LogDebug("NIS archive sync ({Cmp}): นอกช่วง SYNC_WINDOW — ข้ามรอบนี้", cmpId);
            return NisSyncResultDto.Skipped("นอกช่วง SYNC_WINDOW");
        }
        if (string.IsNullOrEmpty(_syncUsername))
        {
            _logger.LogWarning("NIS archive sync ({Cmp}): ข้าม — ยังไม่ตั้งค่า NisReport:SyncUsername", cmpId);
            return NisSyncResultDto.Skipped("ยังไม่ตั้งค่า NisReport:SyncUsername");
        }
        if (!await state.Gate.WaitAsync(0, ct))
        {
            _logger.LogWarning("NIS archive sync ({Cmp}, {Trigger}): ถูกข้าม — รอบก่อนยังไม่จบ", cmpId, trigger);
            return NisSyncResultDto.Skipped("รอบก่อนยังไม่จบ");
        }

        try
        {
            // lock ถือแค่ช่วงรอบนี้ — await using ปล่อยทันทีที่จบ
            await using var lockHandle = await _appLock.TryAcquireAsync(LockResource, ct);
            if (lockHandle is null)
            {
                _logger.LogInformation("NIS archive sync ({Cmp}, {Trigger}): ข้ามรอบ — instance อื่นถือ lock อยู่", cmpId, trigger);
                return NisSyncResultDto.Skipped("instance อื่นกำลัง sync อยู่ (lock ไม่ว่าง)");
            }
            return await RunLockedAsync(cmpId, trigger, state, ct);
        }
        finally
        {
            state.Gate.Release();
        }
    }

    // ── loop ──────────────────────────────────────────────────────────────

    /// <summary>รอบอัตโนมัติ — ผ่านด่านถอยจังหวะก่อน · รอบที่ถูกข้ามที่นี่ไม่เขียนแถวลง NisSyncLog (ยังไม่ได้เรียกต้นทาง)</summary>
    private async Task TickAsync(string cmpId, CancellationToken ct)
    {
        var s = StateOf(cmpId);
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (!_policy.ShouldAttempt(nowMs, s.LastAttemptMs, s.FailStreak))
        {
            s.BackoffSkips++;
            var next = DateTimeOffset.FromUnixTimeMilliseconds(_policy.NextAttemptAt(s.LastAttemptMs ?? nowMs, s.FailStreak));
            _logger.LogWarning(
                "NIS archive sync ({Cmp}): ข้ามรอบนี้ — ล้มติดกัน {Streak} ครั้ง ถอยเป็นทุก {WaitMin} นาที · จะลองอีกครั้ง {ThaiClock} ({Iso})",
                cmpId, s.FailStreak, _policy.BackoffMinutes(s.FailStreak), ThaiClock(next), NisServiceCaseDto.Iso(next));
            return;
        }
        await RunSafeAsync(cmpId, "cron", ct);
    }

    private async Task RunSafeAsync(string cmpId, string trigger, CancellationToken ct)
    {
        try { await RunOnceAsync(cmpId, trigger, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception e) { _logger.LogError(e, "NIS archive sync ({Cmp}, {Trigger}) ล้มแบบไม่คาดคิด", cmpId, trigger); }
    }

    /// <summary>
    /// แถวที่ค้างสถานะ running จากรอบที่ถูกฆ่ากลางคัน (คอนเทนเนอร์ restart) — ถ้าไม่ปิดจะค้างตลอดไป
    /// และคนอ่าน log จะนึกว่ายังมีรอบวิ่งอยู่ · ใช้เกณฑ์อายุ (ดู StaleRunningAfter)
    /// </summary>
    private async Task CloseOrphanRunsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var cutoff = DateTimeOffset.UtcNow - StaleRunningAfter;
            var now = DateTimeOffset.UtcNow;
            var count = await db.NisSyncLogs
                .Where(l => l.Status == "running" && l.StartedAt < cutoff)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.Status, "interrupted")
                    .SetProperty(l => l.FinishedAt, now)
                    .SetProperty(l => l.Error, "ถูกตัดกลางคัน — แอปเริ่มใหม่"), ct);
            if (count > 0) _logger.LogWarning("NIS archive sync: ปิดแถว sync ที่ค้างสถานะ running {Count} แถว", count);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception e)
        {
            _logger.LogError(e, "NIS archive sync: ปิดแถวค้างไม่สำเร็จ");
        }
    }

    // ── หนึ่งรอบ (ถือ lock แล้ว) ────────────────────────────────────────────

    private async Task<NisSyncResultDto> RunLockedAsync(string cmpId, string trigger, CmpState state, CancellationToken ct)
    {
        state.Running = true;
        var sw = Stopwatch.StartNew();
        // นับจากเวลา**เริ่ม** ไม่ใช่เวลาจบ — รอบที่ล้มเพราะ timeout กินเวลานาน ถ้านับจากเวลาจบ จังหวะจะเลื่อนออกไปเรื่อย ๆ
        state.LastAttemptMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // BackgroundService เป็น singleton — DatabaseContext / NisProblemSourceService เป็น scoped ต้องเปิด scope ใหม่ทุกรอบ
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var source = scope.ServiceProvider.GetRequiredService<NisProblemSourceService>();

        var logRow = new NisSyncLog
        {
            CmpId = cmpId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "running",
            Trigger = trigger,
        };
        db.NisSyncLogs.Add(logRow);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();

        try
        {
            var rawList = await source.FetchAsync(cmpId, _syncUsername, ct);
            var (cases, errors) = NisCaseAdapter.ToServiceCases(rawList, cmpId,
                (field, value) => _logger.LogWarning("NIS archive sync ({Cmp}): ค่า {Field} ที่ยังไม่รู้จัก: \"{Value}\"", cmpId, field, value));

            var persisted = await PersistAsync(db, cmpId, cases, ct);

            var result = new NisSyncResultDto
            {
                Status = "ok",
                Fetched = rawList.Count,
                Parsed = cases.Count,
                Invalid = errors.Count,
                Inserted = persisted.Inserted,
                Updated = persisted.Updated,
                Unchanged = persisted.Unchanged,
                Missing = persisted.Missing,
                DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds),
            };

            var finishedAt = DateTimeOffset.UtcNow;
            await db.NisSyncLogs.Where(l => l.Id == logRow.Id).ExecuteUpdateAsync(s => s
                .SetProperty(l => l.FinishedAt, finishedAt)
                .SetProperty(l => l.Status, "ok")
                .SetProperty(l => l.FetchedCount, result.Fetched)
                .SetProperty(l => l.ParsedCount, result.Parsed)
                .SetProperty(l => l.InvalidCount, result.Invalid)
                .SetProperty(l => l.InsertedCount, result.Inserted)
                .SetProperty(l => l.UpdatedCount, result.Updated)
                .SetProperty(l => l.UnchangedCount, result.Unchanged)
                .SetProperty(l => l.MissingCount, result.Missing)
                .SetProperty(l => l.DurationMs, result.DurationMs), ct);

            if (state.FailStreak >= _alertFailStreak)
                Raise("sync-recovered", "sync กลับมาทำงานแล้ว", $"หลังล้มติดกัน {state.FailStreak} ครั้ง (บริษัท {cmpId})");
            state.FailStreak = 0;
            // รีเซ็ตคู่กับ failStreak เสมอ — ตัวนับที่ข้ามเหตุการณ์ล่มคนละครั้งจะทำให้หน้าจอบอกเลขผิด
            state.BackoffSkips = 0;
            result.At = NisServiceCaseDto.Iso(finishedAt);
            state.Last = result;

            _logger.LogInformation(
                "NIS archive sync ({Cmp}, {Trigger}) สำเร็จ · ได้ {Fetched} record · เพิ่ม {Inserted} · แก้ {Updated} · เท่าเดิม {Unchanged} · แปลงไม่ผ่าน {Invalid} · หาย {Missing} · {Ms} ms",
                cmpId, trigger, result.Fetched, result.Inserted, result.Updated, result.Unchanged, result.Invalid, result.Missing, result.DurationMs);
            if (errors.Count > 0)
                _logger.LogWarning("NIS archive sync ({Cmp}): แปลงไม่ผ่าน {Count} record: {Samples}",
                    cmpId, errors.Count, string.Join(" | ", errors.Take(3).Select(e => e.Message)));

            CheckMissing(cmpId, result);
            return result;
        }
        catch (Exception e) when (e is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            state.FailStreak++;
            var result = new NisSyncResultDto
            {
                Status = "failed",
                DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds),
                Error = e.Message,
                At = NisServiceCaseDto.Iso(DateTimeOffset.UtcNow),
            };
            state.Last = result;

            try
            {
                var finishedAt = DateTimeOffset.UtcNow;
                var message = e.Message;
                await db.NisSyncLogs.Where(l => l.Id == logRow.Id).ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.FinishedAt, finishedAt)
                    .SetProperty(l => l.Status, "failed")
                    .SetProperty(l => l.Error, message)
                    .SetProperty(l => l.DurationMs, result.DurationMs), CancellationToken.None);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "NIS archive sync ({Cmp}): บันทึกผลรอบที่ล้มไม่สำเร็จ", cmpId);
            }

            _logger.LogError(e, "NIS archive sync ({Cmp}, {Trigger}) ล้มครั้งที่ {Streak}", cmpId, trigger, state.FailStreak);

            if (state.FailStreak >= _alertFailStreak)
                Raise("sync-fail-streak",
                    $"sync ล้มติดกัน {state.FailStreak} ครั้ง",
                    $"ครั้งล่าสุด: {e.Message} (บริษัท {cmpId})",
                    // เตือนวันละครั้งพอ · ถ้ากลับมาปกติแล้วล้มใหม่ จะมี sync-recovered คั่นให้เห็น
                    $"sync-fail-streak:{cmpId}");
            return result;
        }
        finally
        {
            state.Running = false;
        }
    }

    /// <summary>
    /// upsert ด้วย (CmpId, SourceId) เท่านั้น · ทีละ 200 แถวต่อ SaveChanges (หนึ่ง transaction ต่อก้อน)
    /// 🔴 ห้ามลบแถวที่ต้นทางไม่ส่งมาแล้ว — archive ต้องเก็บทุกอย่าง
    /// </summary>
    private async Task<(int Inserted, int Updated, int Unchanged, int Missing)> PersistAsync(
        DatabaseContext db, string cmpId, List<NisServiceCase> cases, CancellationToken ct)
    {
        var existing = await db.NisServiceCases.AsNoTracking()
            .Where(c => c.CmpId == cmpId)
            .Select(c => new { c.SourceId, c.ModifiedAt })
            .ToListAsync(ct);
        var known = existing.ToDictionary(e => e.SourceId, e => e.ModifiedAt, StringComparer.Ordinal);

        // SourceId ซ้ำในชุดเดียว (ต้นทางส่ง problem เดียวสองแถว) — แถวหลังชนะ ไม่ให้ AddRange ชน PK
        var bySource = new Dictionary<string, NisServiceCase>(StringComparer.Ordinal);
        foreach (var c in cases) bySource[c.SourceId] = c;
        if (bySource.Count != cases.Count)
            _logger.LogWarning("NIS archive sync ({Cmp}): ต้นทางส่ง sourceId ซ้ำ {Dupes} แถว — ใช้แถวหลังสุด", cmpId, cases.Count - bySource.Count);

        var toInsert = new List<NisServiceCase>();
        var toUpdate = new List<NisServiceCase>();
        var unchanged = 0;
        foreach (var c in bySource.Values)
        {
            if (!known.TryGetValue(c.SourceId, out var before))
            {
                toInsert.Add(c);
                continue;
            }
            var after = c.ModifiedAt;
            // ต้นทางบอกเองว่าแถวนี้ไม่ได้แก้ → ไม่ต้องเขียนซ้ำ (ลดการเขียนทับทุกแถวทุก 15 นาที)
            if (before.HasValue && after.HasValue && before.Value == after.Value) unchanged++;
            else toUpdate.Add(c);
        }

        for (var i = 0; i < toInsert.Count; i += Chunk)
        {
            var chunk = toInsert.Skip(i).Take(Chunk).ToList();
            db.NisServiceCases.AddRange(chunk);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }

        for (var i = 0; i < toUpdate.Count; i += Chunk)
        {
            var chunk = toUpdate.Skip(i).Take(Chunk).ToList();
            var ids = chunk.Select(c => c.SourceId).ToList();
            var tracked = await db.NisServiceCases
                .Where(c => c.CmpId == cmpId && ids.Contains(c.SourceId))
                .ToDictionaryAsync(c => c.SourceId, ct);
            foreach (var c in chunk)
            {
                if (tracked.TryGetValue(c.SourceId, out var target)) CopyInto(target, c);
                else db.NisServiceCases.Add(c); // หายไประหว่างทาง (ไม่ควรเกิดภายใต้ lock) — insert แทน
            }
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }

        var missing = known.Keys.Count(k => !bySource.ContainsKey(k));
        return (toInsert.Count, toUpdate.Count, unchanged, missing);
    }

    /// <summary>ฟิลด์ที่อัปเดตได้ตอน upsert — ทุกอย่างยกเว้นคีย์ (CmpId, SourceId)</summary>
    private static void CopyInto(NisServiceCase target, NisServiceCase src)
    {
        target.CaseNo = src.CaseNo;
        target.ExternalGuid = src.ExternalGuid;
        target.CustomerId = src.CustomerId;
        target.CustomerName = src.CustomerName;
        target.BranchName = src.BranchName;
        target.Province = src.Province;
        target.ContactName = src.ContactName;
        target.RequestBy = src.RequestBy;
        target.RequestPhone = src.RequestPhone;
        target.RequestEmail = src.RequestEmail;
        target.RequestPosition = src.RequestPosition;
        target.Subject = src.Subject;
        target.Category = src.Category;
        target.Priority = src.Priority;
        target.Status = src.Status;
        target.Remark = src.Remark;
        target.CreatedAt = src.CreatedAt;
        target.StartedAt = src.StartedAt;
        target.ResolvedAt = src.ResolvedAt;
        target.ModifiedAt = src.ModifiedAt;
        target.ResolutionHours = src.ResolutionHours;
        target.AssigneeIdsJson = src.AssigneeIdsJson;
        target.AssigneeNamesJson = src.AssigneeNamesJson;
        target.PrimaryAssignee = src.PrimaryAssignee;
        target.ActionDetails = src.ActionDetails;
        target.ActionByNamesJson = src.ActionByNamesJson;
        target.ServiceTypeCode = src.ServiceTypeCode;
        target.SatisfactionScore = src.SatisfactionScore;
        target.SatisfactionComment = src.SatisfactionComment;
        target.ViaLine = src.ViaLine;
        target.AnydeskId = src.AnydeskId;
        target.AttachmentCount = src.AttachmentCount;
        target.IsTestData = src.IsTestData;
        target.RawJson = src.RawJson;
        target.SyncedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>ต้นทางเคยส่งแล้วหายไป — ไม่ลบ แต่ต้องรู้ตัว</summary>
    private void CheckMissing(string cmpId, NisSyncResultDto r)
    {
        var total = r.Missing + r.Parsed;
        if (r.Missing == 0 || total == 0) return;
        var pct = r.Missing * 100.0 / total;
        if (pct < _alertMissingPct) return;
        Raise("records-missing",
            $"ต้นทางส่งข้อมูลน้อยลง {pct:F1}%",
            $"{r.Missing} record ที่เคยมีใน archive ไม่ถูกส่งมาในรอบนี้ · ระบบไม่ลบข้อมูลให้ แต่ควรตรวจว่าต้นทางเปลี่ยนอะไร (บริษัท {cmpId})",
            // วันในคีย์ตามเวลาไทย — เตือนตอนตี 2 ของไทยต้องอยู่ในวันนั้น ไม่ใช่วันก่อนหน้า
            $"missing:{cmpId}:{NisBangkokTime.DateKey(DateTimeOffset.UtcNow)}");
    }

    /// <summary>แจ้งเตือน = LogWarning ข้อความเดิมของ AlertService · dedupeKey = เตือนซ้ำได้วันละครั้ง (วันไทย)</summary>
    private void Raise(string kind, string title, string message, string? dedupeKey = null)
    {
        if (dedupeKey != null)
        {
            var today = NisBangkokTime.DateKey(DateTimeOffset.UtcNow);
            if (_alertState.TryGetValue(dedupeKey, out var last) && last == today)
            {
                _logger.LogDebug("NIS archive sync: ข้ามการเตือนซ้ำวันนี้: {Key}", dedupeKey);
                return;
            }
            _alertState[dedupeKey] = today;
        }
        _logger.LogWarning("แจ้งเตือน [{Kind}] {Title} — {Message}", kind, title, message);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private CmpState StateOf(string cmpId) => _states.GetOrAdd(cmpId, _ => new CmpState());

    /// <summary>true = อยู่ในช่วงที่อนุญาตให้ sync (NisReport:SyncWindow = all | biz) — คิดที่เวลาไทย</summary>
    private bool InWindow()
    {
        if (_window != "biz") return true;
        var now = NisBangkokTime.Now();
        if (now.DayOfWeek == DayOfWeek.Sunday && !_biz.Sunday) return false;
        if (now.DayOfWeek == DayOfWeek.Saturday && !_biz.Saturday) return false;
        var mins = now.Hour * 60 + now.Minute;
        NisBusinessHours.TryParseHhmm(_biz.Start, out var start);
        NisBusinessHours.TryParseHhmm(_biz.End, out var end);
        return mins >= start && mins <= end;
    }

    /// <summary>เวลาไทยสำหรับ log ที่คนอ่าน — dd/MM/พ.ศ. HH:mm น.</summary>
    private static string ThaiClock(DateTimeOffset at)
    {
        var d = NisBangkokTime.ToBangkok(at);
        return $"{d.Day:00}/{d.Month:00}/{d.Year + 543} {d.Hour:00}:{d.Minute:00} น.";
    }
}
