namespace goalongapi.Models;

public class DeviceScan
{
    public int DeviceId { get; set; }
    public string Name { get; set; } = null!;
    public string? BrandModel { get; set; }
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string ProtocolType { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public string? Location { get; set; }
    public int SyncIntervalSec { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? LastSeenAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVer { get; set; } = Array.Empty<byte>();
    public string CmpId { get; set; } = null!;

    public List<DeviceUserScan> Users { get; set; } = new();
}
