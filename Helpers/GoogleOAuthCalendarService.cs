using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using goalongapi.Models;
using Microsoft.Extensions.Logging;

namespace goalongapi.Helpers;

public sealed class GoogleOAuthCalendarService
{
    private readonly EmailSettingRepository _repo;
    private readonly GoogleOAuthMailService _oauth;
    private readonly GoogleCalendarEventMappingRepository _mappingRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleOAuthCalendarService> _logger;

    public GoogleOAuthCalendarService(EmailSettingRepository repo, GoogleOAuthMailService oauth, GoogleCalendarEventMappingRepository mappingRepo, IHttpClientFactory httpClientFactory, ILogger<GoogleOAuthCalendarService> logger)
    {
        _repo = repo;
        _oauth = oauth;
        _mappingRepo = mappingRepo;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GoogleCalendarAppointment>> GetEventsAsync(string? cmpId, string settingName, DateTime start, DateTime end)
    {
        var setting = await GetConnectedSettingAsync(cmpId, settingName);
        var calendarId = string.IsNullOrWhiteSpace(setting.CalendarId) ? "primary" : setting.CalendarId;
        var timeMin = start.ToUniversalTime().ToString("O");
        var timeMax = end.ToUniversalTime().ToString("O");
        var url = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events" +
                  $"?singleEvents=true&orderBy=startTime&timeMin={Uri.EscapeDataString(timeMin)}" +
                  $"&timeMax={Uri.EscapeDataString(timeMax)}";
        using var response = await SendAsync(setting, HttpMethod.Get, url, null);
        var payload = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Google Calendar read failed: " + payload);

        using var json = JsonDocument.Parse(payload);
        var events = json.RootElement.TryGetProperty("items", out var items)
            ? items.EnumerateArray().Select(MapEvent).ToList()
            : new List<GoogleCalendarAppointment>();

        // Google ไม่คืน ticketId มากับ event → เติมจากตาราง mapping (event ที่ sync มาจาก ticket)
        // ให้ client แยก event ที่ mirror จาก ticket ออกจากนัดหมายทั่วไปได้โดยไม่ต้องเดาจาก title
        var ticketIdByEventId = await _mappingRepo.GetTicketIdByEventIdAsync(cmpId, settingName);
        if (ticketIdByEventId.Count > 0)
        {
            foreach (var ev in events)
            {
                if (ticketIdByEventId.TryGetValue(ev.GoogleEventId, out var ticketId))
                    ev.TicketId = ticketId;
            }
        }

        _logger.LogInformation(
            "NIS calendar READ: cmp {CmpId} calendar {CalendarId} range [{TimeMin}..{TimeMax}] → {Count} event(s), payloadLen {Len}",
            cmpId, calendarId, timeMin, timeMax, events.Count, payload.Length);
        return events;
    }

    public async Task<GoogleCalendarAppointment> CreateOrUpdateEventAsync(GoogleCalendarAppointmentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) throw new InvalidOperationException("Appointment title is required.");
        // All-day ยึด end แบบ inclusive (โค้ดสร้าง body บวก 1 วันให้เอง) → งานวันเดียว start == end ถือว่าถูกต้อง
        // เฉพาะ timed event เท่านั้นที่ต้องการ end มากกว่า start จริงๆ
        if (dto.AllDay ? dto.End < dto.Start : dto.End <= dto.Start)
            throw new InvalidOperationException("Appointment end must be later than start.");

        var setting = await GetConnectedSettingAsync(dto.CmpId, dto.SettingName);
        var calendarId = string.IsNullOrWhiteSpace(setting.CalendarId) ? "primary" : setting.CalendarId;
        object body = dto.AllDay
            ? new
            {
                summary = dto.Title, description = dto.Description, location = dto.Location,
                start = new { date = dto.Start.ToString("yyyy-MM-dd") },
                end = new { date = dto.End.AddDays(1).ToString("yyyy-MM-dd") },
                attendees = dto.AttendeeEmails.Where(x => !string.IsNullOrWhiteSpace(x)).Select(email => new { email }),
            }
            : new
            {
                summary = dto.Title, description = dto.Description, location = dto.Location,
                start = new { dateTime = dto.Start.ToString("O"), timeZone = "Asia/Bangkok" },
                end = new { dateTime = dto.End.ToString("O"), timeZone = "Asia/Bangkok" },
                attendees = dto.AttendeeEmails.Where(x => !string.IsNullOrWhiteSpace(x)).Select(email => new { email }),
            };
        var mapping = string.IsNullOrWhiteSpace(dto.TicketId)
            ? null
            : await _mappingRepo.GetAsync(dto.CmpId, dto.SettingName, dto.TicketId);
        var method = mapping == null ? HttpMethod.Post : HttpMethod.Patch;
        var url = mapping == null
            ? $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events?sendUpdates=all"
            : $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events/{Uri.EscapeDataString(mapping.GoogleEventId)}?sendUpdates=all";
        using var response = await SendAsync(setting, method, url, JsonSerializer.Serialize(body));
        var payload = await response.Content.ReadAsStringAsync();
        // The event may have been manually removed in Google Calendar. Recreate it
        // and replace the stale mapping instead of failing the ticket update.
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound && mapping != null)
        {
            var createUrl = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events?sendUpdates=all";
            using var createResponse = await SendAsync(setting, HttpMethod.Post, createUrl, JsonSerializer.Serialize(body));
            payload = await createResponse.Content.ReadAsStringAsync();
            if (!createResponse.IsSuccessStatusCode) throw new InvalidOperationException("Google Calendar create failed: " + payload);
        }
        else if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Google Calendar save failed: " + payload);
        using var json = JsonDocument.Parse(payload);
        var appointment = MapEvent(json.RootElement);
        appointment.TicketId = dto.TicketId;
        appointment.CalendarId = calendarId;
        if (!string.IsNullOrWhiteSpace(dto.TicketId))
        {
            await _mappingRepo.UpsertAsync(new GoogleCalendarEventMapping
            {
                CmpId = dto.CmpId,
                SettingName = dto.SettingName,
                TicketId = dto.TicketId,
                GoogleEventId = appointment.GoogleEventId,
                CalendarId = calendarId,
            });
        }
        return appointment;
    }

    public async Task DeleteEventForTicketAsync(string? cmpId, string settingName, string ticketId)
    {
        var mapping = await _mappingRepo.GetAsync(cmpId, settingName, ticketId)
            ?? throw new InvalidOperationException("No Google Calendar appointment is mapped to this ticket.");
        var setting = await GetConnectedSettingAsync(cmpId, settingName);
        var url = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(mapping.CalendarId)}/events/{Uri.EscapeDataString(mapping.GoogleEventId)}?sendUpdates=all";
        using var response = await SendAsync(setting, HttpMethod.Delete, url, null);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            throw new InvalidOperationException("Google Calendar delete failed: " + await response.Content.ReadAsStringAsync());
        await _mappingRepo.DeleteAsync(cmpId, settingName, ticketId);
    }

    private async Task<EmailSmtpSetting> GetConnectedSettingAsync(string? cmpId, string settingName)
    {
        var setting = await _repo.GetActiveAsync(cmpId, settingName)
            ?? throw new InvalidOperationException("Email SMTP setting not found.");
        if (setting.GoogleOAuthRefreshTokenEnc.Length == 0)
            throw new InvalidOperationException("Google Calendar OAuth is not connected. Complete the OAuth authorization first.");
        return setting;
    }

    private async Task<HttpResponseMessage> SendAsync(EmailSmtpSetting setting, HttpMethod method, string url, string? json)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _oauth.GetAccessTokenAsync(setting));
        if (json != null) request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        // Factory-managed client — the underlying handler is pooled, so we neither
        // create a fresh socket per call nor leak an undisposed HttpClient.
        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request);
    }

    private static GoogleCalendarAppointment MapEvent(JsonElement e)
    {
        var start = ReadDate(e.GetProperty("start"));
        var end = ReadDate(e.GetProperty("end"));
        return new GoogleCalendarAppointment
        {
            GoogleEventId = e.GetProperty("id").GetString() ?? "",
            Title = e.TryGetProperty("summary", out var title) ? title.GetString() ?? "" : "",
            Description = e.TryGetProperty("description", out var description) ? description.GetString() : null,
            Location = e.TryGetProperty("location", out var location) ? location.GetString() : null,
            Start = start.Value,
            End = end.Value,
            AllDay = start.AllDay,
        };
    }

    private static (DateTime Value, bool AllDay) ReadDate(JsonElement value) =>
        value.TryGetProperty("date", out var date)
            ? (DateTime.Parse(date.GetString()!), true)
            : (DateTime.Parse(value.GetProperty("dateTime").GetString()!), false);
}
