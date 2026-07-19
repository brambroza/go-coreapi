using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models.Nis;

// ── NisOnsiteReport ─────────────────────────────────────────────────────────────
// Stores an onsite service report filed against a NisTicket (NIS Service Portal).
// The ServiceTicket-based onsite flow writes to ServiceTicketSubTaskAction; NIS
// tickets have no such table, so their reports are persisted here.

public class NisOnsiteReport
{
    [Key]
    [MaxLength(50)]
    public string ReportId { get; set; } = Guid.NewGuid().ToString();

    /// NisTicket.TicketId this report belongs to.
    [MaxLength(50)]
    public string NisTicketId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TicketCode { get; set; }

    [MaxLength(50)]
    public string? SrNumber { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// Engineer (userlogin) who filed the report.
    [MaxLength(200)]
    public string? Engineer { get; set; }

    /// Localized check-in/out timestamps as sent by the client (display strings).
    [MaxLength(100)]
    public string? CheckInTime { get; set; }

    [MaxLength(100)]
    public string? CheckOutTime { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal? CheckInLatitude { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal? CheckInLongitude { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal? CheckOutLatitude { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal? CheckOutLongitude { get; set; }

    // Free-text + JSON blobs (nvarchar(max) — no MaxLength).
    public string? WorkDetail { get; set; }
    public string? IssueDetail { get; set; }
    public string? ChecklistJson { get; set; }
    public string? PmItemsJson { get; set; }
    public string? DamagedProductJson { get; set; }
    public string? SupportCasesJson { get; set; }
    public string? PhotosJson { get; set; }
    public string? SignatureImageBase64 { get; set; }

    public bool SkipSignature { get; set; }

    // ── Persisted Service Report PDF (client-generated, attached to the closing email) ──
    // The blob itself is written to disk/storage (NisOnsite:ReportPdfDir); only a reference is
    // kept here so the row stays small. Enables resend/audit with the exact file the customer signed.
    /// Storage path (relative to the configured report-pdf directory), null when no PDF was attached.
    [MaxLength(400)]
    public string? ReportPdfPath { get; set; }

    /// Size of the stored PDF in bytes.
    public long? ReportPdfSize { get; set; }

    /// SHA-256 (hex) of the stored PDF — integrity/audit trail.
    [MaxLength(64)]
    public string? ReportPdfSha256 { get; set; }

    /// submitted | pending_approval
    [MaxLength(30)]
    public string Status { get; set; } = "submitted";

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
