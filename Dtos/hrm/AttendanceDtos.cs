namespace goalongapi.Dtos;

public record AttendanceDailyCreateDto(
    string CmpId,
    int EmployeeId,
    DateOnly WorkDate,
    int? ShiftId,
    DateTimeOffset? InTime,
    DateTimeOffset? OutTime,
    int WorkMin,
    int BreakMin,
    int LateMin,
    int EarlyLeaveMin,
    int OTMinBeforeShift,
    int OTMinAfterShift,
    int? OTMinTotal,
    string Status,
    string? Note,
    int CalcVersion,
    DateTime? CalcAt,
    string? CalcBy
);

public record AttendanceDailyUpdateDto(
    int EmployeeId,
    DateOnly WorkDate,
    int? ShiftId,
    DateTimeOffset? InTime,
    DateTimeOffset? OutTime,
    int WorkMin,
    int BreakMin,
    int LateMin,
    int EarlyLeaveMin,
    int OTMinBeforeShift,
    int OTMinAfterShift,
    int? OTMinTotal,
    string Status,
    string? Note,
    int CalcVersion,
    DateTime? CalcAt,
    string? CalcBy,
    byte[]? RowVer
);

public record AttendancePunchCreateDto(
    string CmpId,
    long AttendanceId,
    DateTimeOffset PunchTime,
    string PunchType,
    string? Source,
    long? RawLogId
);

public record AttendancePunchUpdateDto(
    long AttendanceId,
    DateTimeOffset PunchTime,
    string PunchType,
    string Source,
    long? RawLogId
);

public record AttendanceAdjustmentCreateDto(
    string CmpId,
    long AttendanceId,
    string FieldChanged,
    string? OldValue,
    string? NewValue,
    string? Reason,
    string? CreatedBy
);

public record AttendanceRuleSetCreateDto(
    string CmpId,
    string Name,
    bool IsDefault,
    string RuleJson,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo
);

public record AttendanceRuleSetUpdateDto(
    string Name,
    bool IsDefault,
    string RuleJson,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo
);
