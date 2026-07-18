namespace goalongapi.Dtos;

public record DeviceScanCreateDto(
    string CmpId,
    string Name,
    string? BrandModel,
    string Host,
    int Port,
    string ProtocolType,
    string Timezone,
    string? Location,
    int SyncIntervalSec,
    bool IsActive,
    string Status,
    DateTime? LastSeenAt,
    DateTime? LastSyncAt,
    string? Notes
);

public record DeviceScanUpdateDto(
    string Name,
    string? BrandModel,
    string Host,
    int Port,
    string ProtocolType,
    string Timezone,
    string? Location,
    int SyncIntervalSec,
    bool IsActive,
    string Status,
    DateTime? LastSeenAt,
    DateTime? LastSyncAt,
    string? Notes,
    byte[]? RowVer
);
