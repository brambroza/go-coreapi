namespace goalongapi.Dtos;

public record ShiftCreateDto(
    string CmpId,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool CrossMidnight,
    int? ScanTypeId,
    int GraceLateMin,
    int GraceEarlyLeaveMin,
    int MinWorkMinForPresent,
    bool IsActive
);

public record ShiftUpdateDto(
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool CrossMidnight,
    int? ScanTypeId,
    int GraceLateMin,
    int GraceEarlyLeaveMin,
    int MinWorkMinForPresent,
    bool IsActive,
    byte[]? RowVer // ส่งมาจาก GET เพื่อกันชนกัน (optimistic concurrency)
);
