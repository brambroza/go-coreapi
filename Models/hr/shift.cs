namespace goalongapi.Models;

public class Shift
{
    public int ShiftId { get; set; }
    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool CrossMidnight { get; set; }

    public int? ScanTypeId { get; set; }
    public int GraceLateMin { get; set; }
    public int GraceEarlyLeaveMin { get; set; }
    public int MinWorkMinForPresent { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVer { get; set; } = Array.Empty<byte>();

    public string CmpId { get; set; } = null!;
    public ScanType? ScanType { get; set; }
}
