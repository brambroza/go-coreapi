namespace goalongapi.Models;

public class ScanType
{
    public int ScanTypeId { get; set; }
    public string Name { get; set; } = null!;
    public int PunchCount { get; set; }
    public bool HasOT { get; set; }
    public bool IsStrictOrder { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CmpId { get; set; } = null!;

    public List<ScanTypeSlot> Slots { get; set; } = new();
}
