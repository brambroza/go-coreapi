namespace goalongapi.Models;

public class AttendanceRawLog
{
    public long RawLogId { get; set; }

    public int DeviceId { get; set; }
    public int? DeviceUserId { get; set; }
    public string? UserCodeOnDevice { get; set; }
    public string? CardNo { get; set; }

    public DateTime DeviceLogTimeLocal { get; set; } // datetime2(0)

    public string? DeviceTimezone { get; set; }
    public string? DeviceLogId { get; set; }

    public DateTimeOffset? PunchTimeUtc { get; set; } // datetimeoffset(0)
    public string? TimezoneUsed { get; set; }
    public int? DeviceClockDriftSec { get; set; }

    public string? VerifyMode { get; set; }
    public string? InOutState { get; set; }
    public string? WorkCode { get; set; }

    public string? RawPayloadJson { get; set; }

    public string Source { get; set; } = "ZKTeco";
    public Guid? SyncBatchId { get; set; }

    public DateTime ReceivedAt { get; set; }         // datetime2(0)
    public string IngestStatus { get; set; } = "New";
    public string? IngestError { get; set; }

    public byte[] UniqueHash { get; set; } = Array.Empty<byte>(); // varbinary(32)
    public string CmpId { get; set; } = null!;

    // optional navigation (ถ้าคุณมี DeviceScan/DeviceUserScan อยู่แล้ว)
    public DeviceScan? Device { get; set; }
    public DeviceUserScan? DeviceUser { get; set; }
}
