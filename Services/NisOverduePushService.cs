using goalongapi.Data;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// NIS Onsite push event 3/3 — งานเลยกำหนด (overdue)
/// BackgroundService เช็คทุก 15 นาที: ตั๋วที่ Due เลยวันนี้ + ยังไม่ปิด + มีช่างถือ
/// → push เตือนช่าง วันละครั้งต่อตั๋ว (dedupe ด้วย EventKey "overdue:{ticketId}:{yyyyMMdd}")
/// best-effort — ล้มรอบไหน log แล้วรอรอบถัดไป
/// prod รันหลาย replica — แต่ละรอบครอบด้วย <see cref="SqlAppLock"/> ให้ไม่รันพร้อมกันข้าม replica
/// ตัวที่ขอ lock ไม่ได้ข้ามรอบนั้นไป; รอบที่เรียงกันยังรันซ้ำได้ แต่ idempotent อยู่แล้วด้วย EventKey
/// </summary>
public class NisOverduePushService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    /// ชื่อ lock ใน sp_getapplock — ต้องไม่ซ้ำกับ BackgroundService ตัวอื่น
    private const string LockResource = "goalongapi:nis-overdue-push";

    /// สถานะที่ยังนับว่า "งานค้าง" (ก่อนเข้าสู่ Waiting Close Approval / Done / Closed)
    private static readonly string[] ActiveStatuses = { "Open", "Scheduled", "In Progress", "Pending" };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SqlAppLock _appLock;
    private readonly ILogger<NisOverduePushService> _logger;

    public NisOverduePushService(
        IServiceScopeFactory scopeFactory,
        SqlAppLock appLock,
        ILogger<NisOverduePushService> logger)
    {
        _scopeFactory = scopeFactory;
        _appLock = appLock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // หน่วงตอน start ให้ app/db พร้อมก่อนรอบแรก
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // lock ถือแค่ช่วงรอบนี้ — await using ปล่อยทันทีที่จบ ไม่ถือข้าม interval
                await using var lockHandle = await _appLock.TryAcquireAsync(LockResource, stoppingToken);
                if (lockHandle is null)
                {
                    _logger.LogInformation("NIS overdue push: ข้ามรอบ — instance อื่นถือ lock อยู่");
                }
                else
                {
                    await CheckOverdueAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NIS overdue push: รอบนี้ล้ม — รอรอบถัดไป");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task CheckOverdueAsync(CancellationToken ct)
    {
        // BackgroundService เป็น singleton — DatabaseContext/ExpoPushService เป็น scoped
        // ต้องเปิด scope ใหม่ทุกรอบ
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var push = scope.ServiceProvider.GetRequiredService<ExpoPushService>();

        var today = DateTime.Today;

        var overdueTickets = await context.NisTickets
            .Where(t => t.Due != null
                        && t.Due < today
                        && ActiveStatuses.Contains(t.Status)
                        && t.Assignee != "-"
                        && t.Assignee != "")
            .ToListAsync(ct);

        foreach (var t in overdueTickets)
        {
            // วันละครั้งต่อตั๋ว — dedupe ใน ExpoPushService (EventKey unique)
            await push.SendToStaffAsync(
                t.CmpId,
                t.Assignee,
                eventKey: $"overdue:{t.TicketId}:{today:yyyyMMdd}",
                title: "⚠️ งานเลยกำหนด",
                body: $"{t.TicketCode} · {t.Title} — กำหนด {t.Due:dd/MM/yyyy}",
                ticketId: t.TicketId,
                data: new Dictionary<string, string> { ["type"] = "overdue" });
        }
    }
}
