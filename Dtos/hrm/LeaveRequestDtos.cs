namespace goalongapi.Dtos;

public record LeaveRequestCreateDto(
    string CmpId,
    int EmployeeId,
    int LeaveTypeId,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeOnly? TimeFrom,
    TimeOnly? TimeTo,
    string? Reason,
    string? AttachmentUrl
);

public record LeaveRequestUpdateDto(
    int LeaveTypeId,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeOnly? TimeFrom,
    TimeOnly? TimeTo,
    string? Reason,
    string? AttachmentUrl
);

public record LeaveApproveDto(
    int ApproverEmployeeId,
    string Action,     // "Approve" | "Reject" | "Cancel"
    string? Note       // optional
);
