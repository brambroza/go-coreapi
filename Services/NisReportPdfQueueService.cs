using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// คิวงานเรนเดอร์ PDF — ตาราง NisReportJob ในฐาน (SPEC §9 ข้อ 34) · port จาก apps/api/src/pdf/pdf-queue.service.ts
///
/// ## ความพร้อมกัน = 1 · poll 3 วิ · lease 5 นาที · ลองใหม่ 3 รอบ
/// ## หยิบงานด้วย UPDATE TOP(1) … WITH (READPAST, UPDLOCK, ROWLOCK) + OUTPUT (แทน FOR UPDATE SKIP LOCKED)
/// ## 🔴 lease — งานที่ค้างต้องถูกยึดคืน: กวาดตอนสตาร์ต (ignoreLease) + lease หมดอายุระหว่างทาง
/// ## prod หลาย replica — ทุกรอบ (รวมรอบสตาร์ต) ครอบด้วย <see cref="SqlAppLock"/> `goalongapi:nis-report-pdf`
///    lock ถือตลอดรอบรวมเวลาเรนเดอร์ → replica อื่นที่เพิ่งบูตจะยึดคืนงานที่ "กำลังทำอยู่จริง" ไม่ได้
///    (ถ้าขอ lock ไม่ได้ตอนบูต = มีคนทำงานอยู่ → ข้ามการกวาด ปล่อยให้ lease หมดอายุจัดการ)
/// ## งานล้มต้องมีเหตุผลอ่านออกใน Error ไม่ใช่แค่สถานะ failed
///
/// singleton · register ครั้งเดียวผ่าน <c>AddSingleton</c> + <c>AddHostedService(sp =&gt; sp.GetRequiredService&lt;…&gt;())</c>
/// (controller เรียก EnqueueAsync/LatestForAsync บน instance เดียวกัน)
/// </summary>
public class NisReportPdfQueueService : BackgroundService
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Poll = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(20);
    private const int MaxAttempts = 3;
    private const string LockResource = "goalongapi:nis-report-pdf";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SqlAppLock _appLock;
    private readonly ILogger<NisReportPdfQueueService> _logger;
    private readonly SemaphoreSlim _wake = new(0, 1);

    /// <summary>ตัวบอกว่า worker ไหนถืองาน — {pid}-{guid8}</summary>
    public string WorkerId { get; } = $"{Environment.ProcessId}-{Guid.NewGuid().ToString("N")[..8]}";

    public NisReportPdfQueueService(IServiceScopeFactory scopeFactory, SqlAppLock appLock, ILogger<NisReportPdfQueueService> logger)
    {
        _scopeFactory = scopeFactory;
        _appLock = appLock;
        _logger = logger;
    }

    private sealed record Claimed(Guid Id, Guid ReportId, int Attempts);

    // ═══════════════════════════════════════════════════════════════════════
    // API ให้ controller
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>เพิ่มงานเข้าคิว — ใบที่ยังค้าง (queued/running) ของรายงานเดียวกันถือว่าพอแล้ว ไม่สร้างซ้ำ</summary>
    public async Task<NisReportJobDto> EnqueueAsync(string cmpId, Guid reportId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var open = await db.NisReportJobs.AsNoTracking()
            .Where(j => j.CmpId == cmpId && j.ReportId == reportId && (j.Status == "queued" || j.Status == "running"))
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (open != null) return NisReportJobDto.From(open);

        var job = new NisReportJob { CmpId = cmpId, ReportId = reportId, Status = "queued", CreatedAt = DateTimeOffset.UtcNow };
        db.NisReportJobs.Add(job);
        await db.SaveChangesAsync(ct);
        // ปลุก loop ให้หยิบทันที ไม่ต้องรอครบ 3 วิ
        if (_wake.CurrentCount == 0) { try { _wake.Release(); } catch (SemaphoreFullException) { } }
        return NisReportJobDto.From(job);
    }

    /// <summary>ใบล่าสุดของรายงานหนึ่งฉบับ — หน้าจอใช้ถามความคืบหน้า · null = ยังไม่เคยสั่งสร้าง</summary>
    public async Task<NisReportJobDto?> LatestForAsync(string cmpId, Guid reportId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var job = await db.NisReportJobs.AsNoTracking()
            .Where(j => j.CmpId == cmpId && j.ReportId == reportId)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return job == null ? null : NisReportJobDto.From(job);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // loop
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        // ตอนสตาร์ตกวาดทุกใบที่ running โดยไม่ดู lease — ภายใต้ lock เท่านั้น (ดูเหตุผลที่หัวไฟล์)
        await RunTickAsync(bootReclaim: true, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await _wake.WaitAsync(Poll, stoppingToken); }
            catch (OperationCanceledException) { return; }
            await RunTickAsync(bootReclaim: false, stoppingToken);
        }
    }

    private async Task RunTickAsync(bool bootReclaim, CancellationToken ct)
    {
        try
        {
            await using var lockHandle = await _appLock.TryAcquireAsync(LockResource, ct);
            if (lockHandle is null)
            {
                if (bootReclaim) _logger.LogInformation("NIS report pdf queue: instance อื่นถือ lock อยู่ตอนบูต — ข้ามการกวาดงานค้าง (lease จะจัดการ)");
                return;
            }
            await TickAsync(bootReclaim, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception e)
        {
            _logger.LogError(e, "NIS report pdf queue: คิวสะดุด — {Message}", e.Message);
        }
    }

    /// <summary>หนึ่งรอบของคิว — ยึดคืนงานค้าง แล้วหยิบทีละใบจนหมด</summary>
    private async Task TickAsync(bool bootReclaim, CancellationToken ct)
    {
        if (bootReclaim) await ReclaimStaleAsync("สตาร์ตใหม่", ignoreLease: true, ct);
        await ReclaimStaleAsync("lease หมดอายุ", ignoreLease: false, ct);
        for (; ; )
        {
            var job = await ClaimOneAsync(ct);
            if (job == null) break;
            await RunOneAsync(job, ct);
        }
    }

    /// <summary>ยึดคืนงานที่เจ้าของหายไป — คืนจำนวนใบ</summary>
    private async Task<int> ReclaimStaleAsync(string reason, bool ignoreLease, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var note = $"งานถูกยึดคืน ({reason}) — เจ้าของเดิมไม่ได้ทำต่อ";
        var flag = ignoreLease ? 1 : 0;
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE dbo.NisReportJob
   SET Status = 'queued', Stage = NULL, LeaseUntil = NULL, WorkerId = NULL,
       Error = CONCAT(ISNULL(Error + CHAR(10), ''), {note})
 WHERE Status = 'running'
   AND ({flag} = 1 OR LeaseUntil IS NULL OR LeaseUntil < SYSDATETIMEOFFSET())", ct);
        if (rows > 0) _logger.LogWarning("NIS report pdf queue: ยึดคืนงานที่ค้าง {Rows} ใบ ({Reason})", rows, reason);
        return rows;
    }

    /// <summary>หยิบงาน queued ที่เก่าสุดหนึ่งใบแบบ atomic — READPAST ข้ามใบที่ถูกจองอยู่ ไม่รอ</summary>
    private async Task<Claimed?> ClaimOneAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
WITH picked AS (
    SELECT TOP(1) Id, ReportId, Attempts, Status, LeaseUntil, WorkerId, StartedAt
      FROM dbo.NisReportJob WITH (READPAST, UPDLOCK, ROWLOCK)
     WHERE Status = 'queued'
     ORDER BY CreatedAt
)
UPDATE picked
   SET Status = 'running', LeaseUntil = @lease, WorkerId = @worker, StartedAt = SYSDATETIMEOFFSET(), Attempts = Attempts + 1
OUTPUT inserted.Id, inserted.ReportId, inserted.Attempts;";
        AddParam(cmd, "@lease", DbType.DateTimeOffset, DateTimeOffset.UtcNow.Add(Lease));
        AddParam(cmd, "@worker", DbType.String, WorkerId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return new Claimed(reader.GetGuid(0), reader.GetGuid(1), reader.GetInt32(2));
    }

    private static void AddParam(DbCommand cmd, string name, DbType type, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.DbType = type;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    /// <summary>
    /// ทำงานหนึ่งใบ — เช็ค assertRenderable ซ้ำ ณ วินาทีที่ผลิตจริง → generating → เรนเดอร์ (print key ใหม่ต่อรอบ)
    /// → sha256 → transaction เดียว: job done + report ready (PdfPath · PdfBlob · PdfSha256) · ล้ม → requeue จนครบ 3 รอบ
    /// </summary>
    private async Task RunOneAsync(Claimed job, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var reports = scope.ServiceProvider.GetRequiredService<NisReportService>();
        var renderer = scope.ServiceProvider.GetRequiredService<INisReportPdfRenderer>();

        var report = await db.NisReports.FirstOrDefaultAsync(r => r.Id == job.ReportId, ct);
        if (report == null)
        {
            await FailAsync(db, job.Id, $"ไม่พบรายงาน {job.ReportId} — อาจถูกลบไปหลังเข้าคิว", ct);
            return;
        }

        // ด่านซ้ำ ณ วินาทีที่ผลิตจริง (ธง/กลุ่ม/หน้าติดต่อฝ่ายขายอาจเปลี่ยนหลังเข้าคิว) — ตกแล้วล้มถาวร ไม่ลองใหม่
        try
        {
            await reports.AssertRenderableAsync(job.ReportId, ct);
        }
        catch (NisUnprocessableException e)
        {
            await FailAsync(db, job.Id, e.Message, ct);
            report.Status = "failed";
            report.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        try
        {
            report.Status = "generating";
            report.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);

            // print key ใหม่ทุกรอบที่พยายาม — ตัวเรนเดอร์เปิด {CrmBaseUrl}/nsr-print/{id}?key=
            var printKey = await reports.IssuePrintKeyAsync(job.ReportId, ct);
            var result = await renderer.RenderAsync(report, printKey, stage => SetStage(job.Id, stage), ct);

            // เก็บตัวไฟล์ลงฐานด้วย ไม่ใช่แค่ path (SPEC 3.3) — backup ของฐานครบในตัวมันเอง
            var blob = await File.ReadAllBytesAsync(result.FilePath, ct);
            var sha = Convert.ToHexString(SHA256.HashData(blob)).ToLowerInvariant();

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var jobRow = await db.NisReportJobs.FirstAsync(j => j.Id == job.Id, ct);
            jobRow.Status = "done";
            jobRow.Stage = null;
            jobRow.FinishedAt = DateTimeOffset.UtcNow;
            jobRow.LeaseUntil = null;
            jobRow.WorkerId = null;
            jobRow.PdfPath = result.FilePath;
            jobRow.PdfBytes = (int)Math.Min(int.MaxValue, result.Bytes);
            jobRow.RenderMs = result.RenderMs;
            jobRow.Error = null;
            report.Status = "ready";
            report.PdfPath = result.FilePath;
            report.PdfBlob = blob;
            report.PdfSha256 = sha;
            report.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            _logger.LogInformation("NIS report pdf queue: เรนเดอร์ {ReportId} สำเร็จ ({Bytes} bytes · {Ms} ms)", job.ReportId, result.Bytes, result.RenderMs);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            var why = e.Message;
            if (job.Attempts < MaxAttempts)
            {
                var jobRow = await db.NisReportJobs.FirstAsync(j => j.Id == job.Id, ct);
                jobRow.Status = "queued";
                jobRow.Stage = null;
                jobRow.LeaseUntil = null;
                jobRow.WorkerId = null;
                jobRow.Error = $"รอบที่ {job.Attempts}: {why}";
                await db.SaveChangesAsync(ct);
                _logger.LogWarning("NIS report pdf queue: เรนเดอร์ {ReportId} ไม่สำเร็จ (รอบที่ {Attempt}) — {Why}", job.ReportId, job.Attempts, why);
            }
            else
            {
                await FailAsync(db, job.Id, $"ล้มครบ {job.Attempts} รอบ · ครั้งล่าสุด: {why}", ct);
                report.Status = "failed";
                report.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }
        }
    }

    /// <summary>บันทึกขั้นทันทีที่เกิด (rendering → writing) — scope แยก ไม่ชน DbContext ของงานหลัก</summary>
    private void SetStage(Guid jobId, string stage)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            db.Database.ExecuteSqlInterpolated($"UPDATE dbo.NisReportJob SET Stage = {stage} WHERE Id = {jobId}");
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "NIS report pdf queue: บันทึก stage {Stage} ของงาน {JobId} ไม่สำเร็จ", stage, jobId);
        }
    }

    private async Task FailAsync(DatabaseContext db, Guid jobId, string why, CancellationToken ct)
    {
        _logger.LogError("NIS report pdf queue: {Why}", why);
        var jobRow = await db.NisReportJobs.FirstAsync(j => j.Id == jobId, ct);
        jobRow.Status = "failed";
        jobRow.Stage = null;
        jobRow.FinishedAt = DateTimeOffset.UtcNow;
        jobRow.LeaseUntil = null;
        jobRow.WorkerId = null;
        jobRow.Error = why;
        await db.SaveChangesAsync(ct);
    }
}
