namespace goalongapi.Models;

public class LeaveRequest
{
    public long LeaveId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }

    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public TimeOnly? TimeFrom { get; set; }   // optional: ลาเป็นชั่วโมง
    public TimeOnly? TimeTo { get; set; }

    // Draft / Pending / Approved / Rejected / Cancelled
    public string Status { get; set; } = "Pending";

    public int? ApproverEmployeeId { get; set; }
    public string? Reason { get; set; }
    public string? AttachmentUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string CmpId { get; set; } = null!;

    // navigation (optional)
    public LeaveType? LeaveType { get; set; }
}
