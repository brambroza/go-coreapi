using System.Text;
using System.Text.Json;

namespace goalongapi.Services;

/// <summary>
/// NIS Onsite realtime notify bridge — POSTs to go-chat-api's internal
/// POST /api/nis/realtime/notify, which emits socket.io "nis:notify" on the
/// /nis namespace to any RN client currently connected (foreground refresh),
/// complementing ExpoPushService (which covers background/killed app).
///
/// การใช้: NotifyAsync เป็น best-effort เสมอ — จับ exception ทั้งหมดไว้ข้างใน
/// ห้ามให้ยิงไม่สำเร็จแล้วพา request หลัก (assign/reject) ล้มตาม
///
/// No-op เงียบๆ ถ้ายังไม่ตั้งค่า NisRealtime:ChatApiBaseUrl / InternalSecret
/// (go-chat-api เองก็ 503 endpoint นี้ถ้า NIS_INTERNAL_SECRET ไม่ถูกตั้งเช่นกัน)
/// </summary>
public class NisRealtimeNotifyService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NisRealtimeNotifyService> _logger;

    public NisRealtimeNotifyService(
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<NisRealtimeNotifyService> logger)
    {
        _httpFactory = httpFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <param name="users">Usernames (login/email — Accounts.Username), ไม่ใช่ FullName. Socket room
    /// ฝั่ง go-chat-api (userRoom) key ด้วย JWT sub ซึ่งคือ Username.</param>
    /// <param name="type">ต้องตรงกับ NOTIFY_TYPES ฝั่ง go-chat-api: "assign" | "reject_close" | "overdue"</param>
    public async Task NotifyAsync(
        string cmpId,
        IEnumerable<string> users,
        string type,
        string? ticketId = null,
        string? title = null,
        string? body = null)
    {
        try
        {
            var baseUrl = _configuration["NisRealtime:ChatApiBaseUrl"];
            var secret = _configuration["NisRealtime:InternalSecret"];
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(secret))
                return;

            var userList = users.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();
            if (userList.Count == 0) return;

            var payload = new
            {
                cmpid = cmpId,
                users = userList,
                type,
                ticketId,
                title,
                body,
            };

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            using var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/api/nis/realtime/notify")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            req.Headers.Add("x-internal-secret", secret);

            using var res = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var resBody = await res.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "NIS realtime notify: go-chat-api ตอบ {Status} (type={Type}): {Body}",
                    (int)res.StatusCode, type, resBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS realtime notify ส่งไม่สำเร็จ (type={Type})", type);
        }
    }
}
