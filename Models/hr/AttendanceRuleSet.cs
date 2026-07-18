namespace goalongapi.Models;

public class AttendanceRuleSet
{
    public int RuleSetId { get; set; }              // DDL: int identity
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public string RuleJson { get; set; } = null!;
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CmpId { get; set; } = null!;
}
