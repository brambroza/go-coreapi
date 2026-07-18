namespace goalongapi.Models;

public class AttendancePunch
{
    public int PunchId { get; set; }                // DDL: int identity
    public long AttendanceId { get; set; }          // DDL: bigint (แม้ Daily เป็น int)
    public DateTimeOffset PunchTime { get; set; }
    public string PunchType { get; set; } = null!;  // IN/OUT/...
    public string Source { get; set; } = "RawLog";
    public long? RawLogId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CmpId { get; set; } = null!;
}
