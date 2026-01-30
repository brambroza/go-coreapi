namespace goalongapi.Dtos;

public record AttendanceRawLogCreateDto(
    string CmpId,
    int DeviceId,
    int? DeviceUserId,
    string? UserCodeOnDevice,
    string? CardNo,
    DateTime DeviceLogTimeLocal,
    string? DeviceTimezone,
    string? DeviceLogId,
    DateTimeOffset? PunchTimeUtc,
    string? TimezoneUsed,
    int? DeviceClockDriftSec,
    string? VerifyMode,
    string? InOutState,
    string? WorkCode,
    string? RawPayloadJson,
    string? Source,
    Guid? SyncBatchId,
    string? IngestStatus,
    string? IngestError
);

public record AttendanceRawLogUpdateDto(
    int DeviceId,
    int? DeviceUserId,
    string? UserCodeOnDevice,
    string? CardNo,
    DateTime DeviceLogTimeLocal,
    string? DeviceTimezone,
    string? DeviceLogId,
    DateTimeOffset? PunchTimeUtc,
    string? TimezoneUsed,
    int? DeviceClockDriftSec,
    string? VerifyMode,
    string? InOutState,
    string? WorkCode,
    string? RawPayloadJson,
    string Source,
    Guid? SyncBatchId,
    string IngestStatus,
    string? IngestError
);

public record AttendanceRawLogIngestBulkDto(
    string CmpId,
    Guid? SyncBatchId,
    List<AttendanceRawLogCreateDto> Items
);
