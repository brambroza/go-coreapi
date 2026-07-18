namespace goalongapi.Dtos.Nis;

/// POST api/nis/push/register-token — ลงทะเบียน/อัปเดต Expo Push token ของเครื่อง
public class NisPushRegisterDto
{
    public string CmpId { get; set; } = string.Empty;

    /// Account.FullName — ต้องตรงกับ NisTicket.Assignee (identity ที่ใช้ assign งาน)
    public string StaffName { get; set; } = string.Empty;

    public string? UserId { get; set; }

    /// ExponentPushToken[xxxx]
    public string ExpoPushToken { get; set; } = string.Empty;

    /// UUID ที่แอปสร้างครั้งแรกแล้วเก็บถาวร (คงที่ต่อเครื่อง)
    public string DeviceId { get; set; } = string.Empty;

    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
}

/// POST api/nis/push/unregister — ถอนเครื่องออกตอน logout
public class NisPushUnregisterDto
{
    public string CmpId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}
