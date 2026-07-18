namespace goalongapi.Dtos;

public record ScanTypeSlotUpsertDto(
    int? ScanTypeSlotId,
    int SeqNo,
    string SlotCode,
    string SlotName,
    TimeOnly? ExpectedFrom,
    TimeOnly? ExpectedTo,
    bool Required
);

public record ScanTypeCreateDto(
    string CmpId,
    string Name,
    int PunchCount,
    bool HasOT,
    bool IsStrictOrder,
    string? Notes,
    List<ScanTypeSlotUpsertDto>? Slots
);

public record ScanTypeUpdateDto(
    string Name,
    int PunchCount,
    bool HasOT,
    bool IsStrictOrder,
    string? Notes,
    List<ScanTypeSlotUpsertDto>? Slots
);
