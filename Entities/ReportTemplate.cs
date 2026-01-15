using System.ComponentModel.DataAnnotations;

namespace goalongapi.Entities;

public class ReportTemplate
{
    [Key]
    public Guid TemplateId { get; set; }

    [Required, MaxLength(50)]
    public string TemplateCode { get; set; } = default!;

    [Required, MaxLength(200)]
    public string TemplateName { get; set; } = default!;

    public int Version { get; set; } = 1;

    [Required]
    public string ConfigJson { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
