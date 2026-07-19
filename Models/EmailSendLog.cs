using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models;

/// <summary>
/// Audit log for outgoing email attempts. NIS Onsite currently records failures only.
/// </summary>
public class EmailSendLog
{
    [Key]
    public long Id { get; set; }

    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(320)]
    public string RecipientEmail { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Provider { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    public string? ErrorDetail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
