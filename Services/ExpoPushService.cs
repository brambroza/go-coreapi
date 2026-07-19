using System.Text;
using System.Text.Json;
using goalongapi.Data;
using goalongapi.Models.Nis;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Services;

/// <summary>
/// NIS Onsite push (Track B MVP) — ส่ง notification ผ่าน Expo Push Service
/// (exp.host) ไปแอปช่างหน้างาน โดยไม่แตะ FCM/APNs ตรง
///
/// การใช้: SendToStaffAsync เป็น best-effort เสมอ — จับ exception ทั้งหมดไว้ข้างใน
/// ห้ามให้ push ล้มแล้วพา request หลัก (assign/reject) ล้มตาม
///
/// Dedupe: insert NisPushLog (EventKey unique) ก่อนส่ง — ถ้า key ชน = เคยส่งแล้ว → ข้าม
/// Token ตาย: Expo ตอบ DeviceNotRegistered → ลบแถว token นั้นทิ้ง กันส่งซ้ำไปเครื่องที่ uninstall
/// </summary>
public class ExpoPushService
{
    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    private readonly DatabaseContext _context;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ExpoPushService> _logger;

    public ExpoPushService(
        DatabaseContext context,
        IHttpClientFactory httpFactory,
        ILogger<ExpoPushService> logger)
    {
        _context = context;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    /// <summary>
    /// ส่ง push ไปช่างคนเดียว (ทุกเครื่องที่ลงทะเบียนไว้) — best-effort, ไม่ throw
    /// </summary>
    /// <param name="eventKey">dedupe key เช่น "assign:{ticketId}:{ชื่อช่าง}:{yyyyMMdd}" — key ซ้ำ = ไม่ส่งซ้ำ</param>
    public async Task SendToStaffAsync(
        string cmpId,
        string staffName,
        string eventKey,
        string title,
        string body,
        string? ticketId = null,
        Dictionary<string, string>? data = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(staffName) || staffName == "-") return;

            // ── dedupe: insert log ก่อนส่ง (EventKey unique → first-writer-wins) ──
            _context.NisPushLogs.Add(new NisPushLog
            {
                EventKey = eventKey,
                CmpId = cmpId,
                TicketId = ticketId,
                StaffName = staffName,
                Title = title,
                Body = body,
            });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // EventKey ชน = event นี้เคยส่งแล้ว → detach entry ที่ค้างแล้วจบเงียบ ๆ
                _context.ChangeTracker.Clear();
                return;
            }

            var tokens = await _context.NisPushTokens
                .Where(t => t.CmpId == cmpId && t.StaffName == staffName)
                .ToListAsync();
            if (tokens.Count == 0) return;

            // ── payload ตาม Expo Push API (batch ทุกเครื่องของช่างในคำขอเดียว) ──
            var messages = tokens.Select(t => new Dictionary<string, object?>
            {
                ["to"] = t.ExpoPushToken,
                ["title"] = title,
                ["body"] = body,
                ["sound"] = "default",
                ["priority"] = "high",
                ["channelId"] = "default", // Android channel ที่แอปสร้างไว้ (เสียง+สั่น)
                ["data"] = BuildData(ticketId, data),
            }).ToList();

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var json = JsonSerializer.Serialize(messages);
            using var res = await client.PostAsync(
                ExpoPushUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));
            var resBody = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "NIS push: Expo API ตอบ {Status} (staff={Staff}, event={Event}): {Body}",
                    (int)res.StatusCode, staffName, eventKey, resBody);
                return;
            }

            await RemoveDeadTokensAsync(tokens, resBody);
        }
        catch (Exception ex)
        {
            // best-effort — log แล้วปล่อยผ่าน ห้ามกระทบ request หลัก
            _logger.LogWarning(ex, "NIS push ส่งไม่สำเร็จ (staff={Staff}, event={Event})", staffName, eventKey);
        }
    }

    /// รวม ticketId + data เพิ่มเติม เป็น data payload สำหรับ deep-link ฝั่งแอป
    private static Dictionary<string, string> BuildData(string? ticketId, Dictionary<string, string>? extra)
    {
        var d = extra != null ? new Dictionary<string, string>(extra) : new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(ticketId)) d["ticketId"] = ticketId;
        return d;
    }

    /// <summary>
    /// อ่านผลรายข้อจาก Expo (data[] เรียงตาม messages ที่ส่ง) — token ไหนตอบ
    /// DeviceNotRegistered ให้ลบทิ้ง (เครื่อง uninstall/token หมดอายุ)
    /// </summary>
    private async Task RemoveDeadTokensAsync(List<NisPushToken> tokens, string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (!doc.RootElement.TryGetProperty("data", out var results)
                || results.ValueKind != JsonValueKind.Array)
                return;

            var dead = new List<NisPushToken>();
            var i = 0;
            foreach (var item in results.EnumerateArray())
            {
                if (i >= tokens.Count) break;
                if (item.TryGetProperty("details", out var details)
                    && details.ValueKind == JsonValueKind.Object
                    && details.TryGetProperty("error", out var err)
                    && err.GetString() == "DeviceNotRegistered")
                {
                    dead.Add(tokens[i]);
                }
                i++;
            }

            if (dead.Count > 0)
            {
                _context.NisPushTokens.RemoveRange(dead);
                await _context.SaveChangesAsync();
                _logger.LogInformation("NIS push: ลบ token ตายแล้ว {Count} แถว", dead.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS push: parse ผลจาก Expo ไม่ได้ (ข้ามการลบ token ตาย)");
        }
    }
}
