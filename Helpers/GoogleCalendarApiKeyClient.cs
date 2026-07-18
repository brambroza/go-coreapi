using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace goalongapi.Helpers;

public class SyncedCalendarEvent
{
    public string Id { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsAllDay { get; set; }
}

// Reads events from a Google Calendar via the Calendar API using a project
// API key (GoogleCalendar:ApiKey in configuration) — no per-user OAuth.
// This only works if the target calendar's sharing setting is "Make
// available to public"; a private calendar returns 403 even with a valid
// key. Read-only — never writes back to Google.
public class GoogleCalendarApiKeyClient
{
    private readonly string? _apiKey;

    public GoogleCalendarApiKeyClient(IConfiguration config)
    {
        _apiKey = config["GoogleCalendar:ApiKey"];
    }

    public async Task<List<SyncedCalendarEvent>> GetEventsAsync(
        string calendarId, DateTime rangeStart, DateTime rangeEnd)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("GoogleCalendar:ApiKey is not configured.");

        using var service = new CalendarService(new BaseClientService.Initializer
        {
            ApiKey = _apiKey,
            ApplicationName = "goalongapi-calendar-sync",
        });

        var request = service.Events.List(calendarId);
        request.TimeMinDateTimeOffset = rangeStart;
        request.TimeMaxDateTimeOffset = rangeEnd;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var response = await request.ExecuteAsync();

        return (response.Items ?? new List<Google.Apis.Calendar.v3.Data.Event>())
            .Select(e =>
            {
                var isAllDay = e.Start?.DateTimeDateTimeOffset == null;
                var start = e.Start?.DateTimeDateTimeOffset?.UtcDateTime
                    ?? DateTime.Parse(e.Start?.Date ?? DateTime.UtcNow.ToString("yyyy-MM-dd"));
                var end = e.End?.DateTimeDateTimeOffset?.UtcDateTime
                    ?? DateTime.Parse(e.End?.Date ?? start.ToString("yyyy-MM-dd"));

                return new SyncedCalendarEvent
                {
                    Id = e.Id ?? Guid.NewGuid().ToString(),
                    Summary = e.Summary ?? "",
                    Description = e.Description ?? "",
                    Location = e.Location ?? "",
                    Start = start,
                    End = end,
                    IsAllDay = isAllDay,
                };
            })
            .ToList();
    }
}
