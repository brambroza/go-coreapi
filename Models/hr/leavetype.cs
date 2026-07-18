namespace goalongapi.Models;

public class LeaveType
{
    public int LeaveTypeId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsPaid { get; set; }
    public bool NeedAttachment { get; set; }
    public bool IsActive { get; set; }

    public string CmpId { get; set; } = null!;
}
