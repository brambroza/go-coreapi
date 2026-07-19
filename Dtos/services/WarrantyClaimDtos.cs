namespace goalongapi.Dtos;

// ─── Response DTOs ────────────────────────────────────────────────────────────

public class WarrantyClaimDto
{
    public string Id { get; set; } = string.Empty;
    public string? TicketId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string? SalesName { get; set; }
    public string ReporterStaff { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    /// <summary>on | off</summary>
    public string WarrantyStatus { get; set; } = "on";
    /// <summary>Claim Received | Under Inspection | In Progress | Completed | Rejected</summary>
    public string Status { get; set; } = "Claim Received";
    public string? Detail { get; set; }
    public string Date { get; set; } = string.Empty;
}

public class WarrantyDeviceLookupDto
{
    public string SerialNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Customer { get; set; }
    /// <summary>on | off</summary>
    public string WarrantyStatus { get; set; } = "on";
    public string Status { get; set; } = "Active";
    public string? ExpiryDate { get; set; }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public class WarrantyClaimCreateDto
{
    public string? TicketId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string? SalesName { get; set; }
    public string ReporterStaff { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string WarrantyStatus { get; set; } = "on";
    public string? Detail { get; set; }
    public string? CmpId { get; set; }
    public string? UpdUser { get; set; }
}

public class WarrantyClaimUpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>สำหรับ GET /api/WarrantyClaims/notifications — notification ที่ derive จาก claims ล่าสุด</summary>
public class WarrantyClaimNotificationDto
{
    public int Id { get; set; }
    public string SalesName { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
