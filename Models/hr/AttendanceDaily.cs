namespace goalongapi.Models;

public class AttendanceDaily
{
    public int AttendanceId { get; set; }          // DDL: int identity
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public int? ShiftId { get; set; }

    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }

    public int WorkMin { get; set; }
    public int BreakMin { get; set; }
    public int LateMin { get; set; }
    public int EarlyLeaveMin { get; set; }
    public int OTMinBeforeShift { get; set; }
    public int OTMinAfterShift { get; set; }
    public int? OTMinTotal { get; private set; }           // DDL: int nullable

    public string Status { get; set; } = "Unknown";
    public string? Note { get; set; }

    public int CalcVersion { get; set; } = 1;
    public DateTime CalcAt { get; set; }
    public string? CalcBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public byte[] RowVer { get; set; } = Array.Empty<byte>();
    public string CmpId { get; set; } = null!;
}
