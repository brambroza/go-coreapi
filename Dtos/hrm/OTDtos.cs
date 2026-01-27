namespace goalongapi.Dtos;

public record OTCreateDto(
    string CmpId,
    int EmployeeId,
    DateOnly WorkDate,
    TimeOnly TimeFrom,
    TimeOnly TimeTo,
    string? OTType,
    string? Reason
);

public record OTUpdateDto(
    int EmployeeId,
    DateOnly WorkDate,
    TimeOnly TimeFrom,
    TimeOnly TimeTo,
    string? OTType,
    string Status,
    int? ApproverEmployeeId,
    string? Reason
);

public record OTApproveDto(
    int ApproverEmployeeId,
    string Action,   // Approve | Reject | Cancel
    string? Note
);
