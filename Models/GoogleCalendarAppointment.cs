namespace goalongapi.Models;

public sealed class GoogleCalendarAppointmentCreateDto
{
    public string? CmpId { get; set; }
    public string SettingName { get; set; } = "nis";
    public string? TicketId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }
    public List<string> AttendeeEmails { get; set; } = [];
}

public sealed class GoogleCalendarAppointment
{
    public string GoogleEventId { get; set; } = "";
    public string CalendarId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }
    public string? TicketId { get; set; }
}
