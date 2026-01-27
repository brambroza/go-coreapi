namespace goalongapi.Models;

public class ScanTypeSlot
{
    public int ScanTypeSlotId { get; set; }
    public int ScanTypeId { get; set; }

    public int SeqNo { get; set; }
    public string SlotCode { get; set; } = null!;
    public string SlotName { get; set; } = null!;
    public TimeOnly? ExpectedFrom { get; set; }
    public TimeOnly? ExpectedTo { get; set; }
    public bool Required { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CmpId { get; set; } = null!;

    public ScanType? ScanType { get; set; }
}


