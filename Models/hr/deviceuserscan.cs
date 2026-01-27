namespace goalongapi.Models;

public class DeviceUserScan
{
    public int DeviceUserId { get; set; }
    public int DeviceId { get; set; }
    public string UserCodeOnDevice { get; set; } = null!;
    public string? CardNo { get; set; }
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVer { get; set; } = Array.Empty<byte>();
    public string CmpId { get; set; } = null!;

    public DeviceScan? Device { get; set; }
}
