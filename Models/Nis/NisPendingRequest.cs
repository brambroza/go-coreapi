using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisPendingRequest ───────────────────────────────────────────────────────────
// A "self open ticket" request raised by a field engineer in the Staff Portal.
// It lands in the Service Board → Staff Requests tab where a manager either
// approves it (→ creates a real NisTicket assigned to an engineer) or rejects it.

public class NisPendingRequest
{
    [Key]
    [MaxLength(50)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// Engineer (FullName) who raised the request.
    [MaxLength(200)]
    public string RequestedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// Install | MA | PM | Support | Backup | Report | งานภายใน
    [MaxLength(50)]
    public string TicketType { get; set; } = string.Empty;

    /// Onsite | Remote | Telephone
    [MaxLength(50)]
    public string? SupportMethod { get; set; }

    /// Project the resulting ticket should be created under (optional at request
    /// time, but required before a manager can approve).
    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(2000)]
    public string? Detail { get; set; }

    public DateTime? Due { get; set; }

    public bool NoOnsite { get; set; } = false;
    public bool SkipSignature { get; set; } = false;
    public bool RequireCloseApproval { get; set; } = false;

    /// Set when the request is opened against an existing ticket (sub-request).
    [MaxLength(50)]
    public string? ParentTicketId { get; set; }

    /// Pending | Approved | Rejected
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// TicketId created when the request is approved (audit link).
    [MaxLength(50)]
    public string? CreatedTicketId { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    [MaxLength(100)]
    public string? RejectedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
