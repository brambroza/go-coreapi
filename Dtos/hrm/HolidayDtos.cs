namespace goalongapi.Dtos;

public record HolidayCreateDto(
    string CmpId,
    DateOnly HolidayDate,
    string Name,
    bool IsCompanyHoliday,
    string? Notes,
    string Color,
    string HolidayId 
);

public record HolidayUpdateDto(
    DateOnly HolidayDate,
    string Name,
    bool IsCompanyHoliday,
    string? Notes,
    string Color
);
