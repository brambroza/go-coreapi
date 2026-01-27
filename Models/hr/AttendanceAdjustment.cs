namespace goalongapi.Models;

public class AttendanceAdjustment
{
    public int AdjustId { get; set; }               // DDL: int identity
    public long AttendanceId { get; set; }          // DDL: bigint
    public string FieldChanged { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CmpId { get; set; } = null!;
}
