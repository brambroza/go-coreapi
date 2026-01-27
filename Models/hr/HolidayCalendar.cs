namespace goalongapi.Models;

public class HolidayCalendar
{
    public string HolidayId { get; set; }
    public DateOnly HolidayDate { get; set; }
    public string Name { get; set; } = null!;
    public bool IsCompanyHoliday { get; set; } = true;
    public string? Notes { get; set; }
    public string CmpId { get; set; } = null!;
    public string Color { get; set; }
}
