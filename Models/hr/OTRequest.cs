namespace goalongapi.Models;

public class OTRequest
{
    public int OTId { get; set; }                   // DDL: int identity
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }
    public string? OTType { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected/Cancelled
    public int? ApproverEmployeeId { get; set; }
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CmpId { get; set; } = null!;
}
