using goalongapi.Data;
using goalongapi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// NIS → CRM bell bridge — เขียนแถวลง SystemNotification (ผ่าน dbo.setNotification)
/// แล้ว emit SignalR event เดียวกับที่ CRM ฝั่งเว็บ subscribe อยู่
/// (`ReceiveNotification{cmpid}{userlogin}` / `ReceiveNotificationMenu{cmpid}{userlogin}`)
/// เพื่อให้กระดิ่งบน CRM เด้งทันทีโดยไม่ต้องรีเฟรชหน้า
///
/// ใช้คู่กับ <see cref="ExpoPushService"/> (แอปมือถือช่าง/SM) และ
/// <see cref="NisRealtimeNotifyService"/> (socket.io ของ RN) — คนละช่องทาง คนละ client
///
/// การใช้: NotifyAsync เป็น best-effort เสมอ — จับ exception ไว้ข้างในทั้งหมด
/// ห้ามให้แจ้งเตือนล้มแล้วพา request หลักล้มตาม
/// </summary>
public class NisCrmNotifyService
{
    private readonly DatabaseContext _context;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<NisCrmNotifyService> _logger;

    public NisCrmNotifyService(
        DatabaseContext context,
        IHubContext<NotificationHub> hub,
        ILogger<NisCrmNotifyService> logger)
    {
        _context = context;
        _hub = hub;
        _logger = logger;
    }

    /// <summary>
    /// ส่งการแจ้งเตือนเข้ากระดิ่ง CRM ให้ผู้ใช้หนึ่งคน
    /// </summary>
    /// <param name="cmpId">รหัสบริษัท (SystemNotification.CmpId)</param>
    /// <param name="toUsername">Accounts.Username ของผู้รับ (ไม่ใช่ FullName — group ของ hub ใช้ username)</param>
    /// <param name="fromUsername">Accounts.Username ของผู้ส่ง (ผู้ทำ action)</param>
    /// <param name="title">ข้อความบนกระดิ่ง (ใช้เป็นทั้ง Title และ Category ตาม payload ของ CRM)</param>
    /// <param name="linkTo">ลิงก์ที่กดแล้วเปิด (path ของ CRM)</param>
    /// <param name="moduleFormName">ชื่อเมนูสำหรับ badge ต่อเมนู เช่น "nis/serviceboard"</param>
    /// <param name="docNo">เลขเอกสารอ้างอิง เช่น TicketCode</param>
    public async Task NotifyAsync(
        string cmpId,
        string toUsername,
        string fromUsername,
        string title,
        string linkTo,
        string moduleFormName,
        string docNo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cmpId) || string.IsNullOrWhiteSpace(toUsername)) return;

            // ── persist ผ่าน SP เดิมที่ CRM ใช้ (parameterized — ห้าม concat ค่าเข้า SQL) ──
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.setNotification @CmpId={0}, @userTo={1}, @userFrom={2}, @Id={3}, "
                + "@Title={4}, @Category={5}, @Type={6}, @linkTo={7}, @ModuleFormName={8}, "
                + "@DocNo={9}, @RevNo={10}",
                cmpId,
                toUsername,
                fromUsername ?? string.Empty,
                Guid.NewGuid().ToString(),
                title,
                title,
                "friend", // ชนิดการ์ดที่ CRM รองรับ (เหมือนที่บอร์ดส่งตอนมอบหมายงาน)
                linkTo ?? string.Empty,
                moduleFormName,
                docNo ?? string.Empty,
                0);

            // ── ดันของใหม่ขึ้นกระดิ่งทันที (payload = ผลลัพธ์ getNoitfications เหมือน hub เดิม) ──
            var payload = await GetNotificationsJsonAsync(cmpId, toUsername);
            await _hub.Clients.All.SendAsync($"ReceiveNotification{cmpId}{toUsername}", payload);
            await _hub.Clients.All.SendAsync($"ReceiveNotificationMenu{cmpId}{toUsername}", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS CRM notify ส่งไม่สำเร็จ (to={To}, doc={Doc})", toUsername, docNo);
        }
    }

    /// อ่านรายการแจ้งเตือนของผู้ใช้เป็น JSON (รูปแบบเดียวกับ NotificationHub.SendNotifications)
    private static Task<string> GetNotificationsJsonAsync(string cmpId, string username)
    {
        var dt = DB.DBConn.GetDataTable(
            $"exec dbo.[getNoitfications] @CmpId={cmpId} ,  @userlogin='{username.Replace("'", "''")}'");
        return Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(dt));
    }
}
