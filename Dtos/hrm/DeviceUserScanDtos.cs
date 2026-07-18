namespace goalongapi.Dtos;

public record DeviceUserScanCreateDto(
    string CmpId,
    int DeviceId,
    string UserCodeOnDevice,
    string? CardNo,
    string? DisplayName
);

public record DeviceUserScanUpdateDto(
    int DeviceId,
    string UserCodeOnDevice,
    string? CardNo,
    string? DisplayName,
    byte[]? RowVer
);
