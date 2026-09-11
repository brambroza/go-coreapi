using System.Text.Json;
using goalongapi.Data;
using goalongapi.Dtos.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// NIS Onsite — auto-close ตั๋วของโปรเจคเคส Accident หลังหมดสัญญา
///
/// เคส Accident (<c>NisServiceConditionsDto.OnsiteAccident</c> ที่ persist ไว้ใน
/// <c>NisProject.ServiceConditionsJson</c>) คือโครงการที่ตั๋ว onsite ถูกสร้างครบโควตาแล้ว
/// แต่ไม่ได้กำหนดรอบ/วันล่วงหน้า — เมื่อสัญญาโครงการหมดอายุ (วันนี้ > NisProject.EndDate)
/// ตั๋วที่ยังค้างอยู่จะไม่มีทางถูกปิดผ่าน flow ปกติอีก (ไม่มีช่างกลับไปทำต่อ) จึงต้อง
/// auto-close ทิ้งเพื่อไม่ให้ค้างเป็นงานเปิดตลอดไป
///
/// BackgroundService เช็ควันละครั้ง (ไม่ต้องถี่แบบ <see cref="NisOverduePushService"/> เพราะ
/// contract end date เปลี่ยนวันละครั้งอย่างมากที่สุด): โปรเจคที่ OnsiteAccident = true และ
/// EndDate ผ่านไปแล้ว → ตั๋วในโปรเจคนั้นที่ยังไม่ Closed ทั้งหมด ปิดตรง (Status = "Closed",
/// Pct = 100) โดย "ไม่" ผ่านขั้นตอน "Waiting Close Approval" ตามปกติ — เป็นการ bypass approval
/// ที่ตั้งใจ ตามที่ business owner ยืนยัน (เคส Accident ไม่มีช่างไปปิดงานจริงอีกแล้ว รออนุมัติไปก็ไม่มี
/// ใครกดรับ) best-effort เหมือน NisOverduePushService — ล้มรอบไหน log แล้วรอรอบถัดไป
/// </summary>
public class NisAccidentAutoCloseService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// ตรงกับ NisController.NIS_PCT_CLOSED — คนละคลาสกัน (private const) เลย mirror ค่าไว้ที่นี่
    /// ค่าเดียวกันนี้ mirror อยู่ฝั่ง RN ที่ NIS-OnsiteService/src/utils/progress.ts ด้วย
    private const int NisPctClosed = 100;

    /// tag ผู้แก้ไข — ใช้แยกจาก username จริงเวลาไล่ audit ว่าใครปิดตั๋วนี้
    private const string SystemActor = "system:accident-auto-close";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NisAccidentAutoCloseService> _logger;

    public NisAccidentAutoCloseService(IServiceScopeFactory scopeFactory, ILogger<NisAccidentAutoCloseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // หน่วงตอน start ให้ app/db พร้อมก่อนรอบแรก (offset จาก NisOverduePushService เล็กน้อย
        // กันสองงาน background เปิด scope/connection พร้อมกันตอน app เพิ่งขึ้น)
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAccidentContractsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NIS accident auto-close: รอบนี้ล้ม — รอรอบถัดไป");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task CheckAccidentContractsAsync(CancellationToken ct)
    {
        // BackgroundService เป็น singleton — DatabaseContext เป็น scoped ต้องเปิด scope ใหม่ทุกรอบ
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var today = BangkokNow().Date;

        // กรองด้วย EndDate ที่ SQL ได้ก่อน (index-friendly) — OnsiteAccident อยู่ใน JSON เลยต้อง
        // deserialize ต่อในหน่วยความจำ (จำนวนโปรเจคที่หมดสัญญาต่อวันน้อย ไม่กระทบ perf)
        var expiredProjects = await context.NisProjects
            .Where(p => p.EndDate != null && p.EndDate.Value.Date < today)
            .ToListAsync(ct);

        var accidentProjects = expiredProjects
            .Where(p => ParseServiceConditions(p.ServiceConditionsJson)?.OnsiteAccident == true)
            .ToList();

        if (accidentProjects.Count == 0) return;

        var projectIds = accidentProjects.Select(p => p.ProjectId).ToList();

        // "ยังไม่ปิด" = สถานะใดก็ได้ที่ไม่ใช่ Closed อยู่แล้ว (รวม "Waiting Close Approval" — ต้อง
        // bypass ขั้นตอนนั้นตามที่ตั้งใจ ไม่ใช่รอ manager กดอนุมัติ เพราะเคส Accident ไม่มีช่างไปทำต่อ)
        var openTickets = await context.NisTickets
            .Where(t => projectIds.Contains(t.ProjectId) && t.Status != "Closed")
            .ToListAsync(ct);

        if (openTickets.Count == 0) return;

        var now = BangkokNow();
        var closeNote = $"[ปิดงานอัตโนมัติ] เคส Accident — โครงการหมดสัญญาแล้ว "
            + $"ระบบปิดงานอัตโนมัติโดยไม่ผ่านขั้นตอนอนุมัติปกติ ({now:dd/MM/yyyy HH:mm})";

        foreach (var ticket in openTickets)
        {
            ticket.Status = "Closed";
            ticket.Pct = NisPctClosed;
            // NisTicket ไม่มีคอลัมน์ remarks/note โดยเฉพาะ — ต่อท้าย WorkDetail (ช่องเดียวที่แสดงบน
            // การ์ดตั๋ว/หน้ารายละเอียด) แทนการเขียนทับ เพื่อให้ยังเห็นรายละเอียดงานเดิมของช่างอยู่
            ticket.WorkDetail = string.IsNullOrWhiteSpace(ticket.WorkDetail)
                ? closeNote
                : $"{ticket.WorkDetail}\n\n{closeNote}";
            ticket.UpdatedBy = SystemActor;
            ticket.UpdatedDate = now;
        }

        await context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "NIS accident auto-close: ปิดตั๋วอัตโนมัติ {TicketCount} ใบ จาก {ProjectCount} โปรเจค accident ที่หมดสัญญาแล้ว",
            openTickets.Count,
            accidentProjects.Count);
    }

    private static NisServiceConditionsDto? ParseServiceConditions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<NisServiceConditionsDto>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static DateTime BangkokNow() =>
        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
}
