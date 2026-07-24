using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models.Nis;

// ── NisProject ────────────────────────────────────────────────────────────────
// Represents a NIS service project (Implement / MA / Runrate)

public class NisProject
{
    [Key]
    [MaxLength(50)]
    public string ProjectId { get; set; } = Guid.NewGuid().ToString();

    /// Yearly sequential number per CmpId, stored as varchar (e.g. "NIS-2600001").
    /// Format: NIS-YYXXXXX, where XXXXX resets to 00001 each Bangkok calendar year.
    [MaxLength(50)]
    public string? ProjectNo { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Customer { get; set; } = string.Empty;

    /// Master customer code (msb.mCustomer.CustomerCode). Kept so the project can
    /// resolve the customer's saved locations (msb.mCustomerLocations) later.
    [MaxLength(50)]
    public string? CustomerCode { get; set; }

    /// Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
    [MaxLength(50)]
    public string Type { get; set; } = "Implement";

    /// High | Medium | Low
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    public int Progress { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// Primary staff name (display only)
    [MaxLength(200)]
    public string Staff { get; set; } = string.Empty;

    /// Sales Order reference number
    [MaxLength(100)]
    public string SoRef { get; set; } = string.Empty;

    /// Pipe-delimited tag list, e.g. "Firewall|Network|WiFi"
    [MaxLength(1000)]
    public string TagsRaw { get; set; } = string.Empty;

    // ── Contact ───────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    // ── SalesPM ───────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? SalesPMName { get; set; }

    [MaxLength(100)]
    public string? SalesPMNickname { get; set; }

    [MaxLength(50)]
    public string? SalesPMPhone { get; set; }

    [MaxLength(100)]
    public string? SalesPMRole { get; set; }

    // ── Engineer ──────────────────────────────────────────────────────────────

    [MaxLength(200)]
    public string? EngineerName { get; set; }

    [MaxLength(100)]
    public string? EngineerNickname { get; set; }

    [MaxLength(50)]
    public string? EngineerPhone { get; set; }

    // ── Location / Tenant ─────────────────────────────────────────────────────

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual ICollection<NisTicket> Tickets { get; set; } = new List<NisTicket>();

    /// Documents attached when the project was created (PDF/Excel/Word/Visio/Image).
    public virtual ICollection<NisProjectFile> Files { get; set; } = new List<NisProjectFile>();
}

// ── NisProjectFile ────────────────────────────────────────────────────────────
// One attached document for a NIS project. Binary is stored on disk via the shared
// upload endpoints (/uploadallfile + /movefile); this row only keeps the metadata.

public class NisProjectFile
{
    [Key]
    [MaxLength(50)]
    public string FileId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    /// Original file name shown to the user, e.g. "Network-Diagram.pdf"
    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    /// Full URL/path where the file was moved, e.g. "{serverUrl}/{cmpId}/nis/{projectNo}/{fileName}"
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    /// Display ordering (1-based) in the attachment list.
    public int Seq { get; set; } = 1;

    public long FileSize { get; set; } = 0;

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual NisProject? Project { get; set; }
}

// ── NisTicket ─────────────────────────────────────────────────────────────────

public class NisTicket
{
    [Key]
    [MaxLength(50)]
    public string TicketId { get; set; } = Guid.NewGuid().ToString();

    /// Human-readable code, e.g. TK-BK-0007-01 (TK-{TypePrefix}-{ProjectNo}-{RunNo}).
    /// Assigned by the backend on create — RunNo resets per (ProjectId, Type).
    [MaxLength(50)]
    public string? TicketCode { get; set; }

    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// Open | In Progress | Pending | Done | Closed | Scheduled
    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [MaxLength(200)]
    public string Assignee { get; set; } = "-";

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime? Due { get; set; }

    public int Pct { get; set; } = 0;

    /// Install | PM | MA Onsite | Support | Backup | Report | Delivery | MA
    [MaxLength(50)]
    public string? Type { get; set; }

    /// High | Medium | Low
    [MaxLength(20)]
    public string? Priority { get; set; }

    /// Pipe-delimited
    [MaxLength(500)]
    public string? TagsRaw { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public virtual NisProject? Project { get; set; }
}

// ── NisSalesOrder ─────────────────────────────────────────────────────────────

public class NisSalesOrder
{
    [Key]
    [MaxLength(50)]
    public string SoId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(100)]
    public string QuoteRef { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Customer { get; set; } = string.Empty;

    public DateTime? Date { get; set; }

    /// Runrate | Implement | MA-Device | MA-Fortigate | MA-Software | MA-Network
    [MaxLength(50)]
    public string Type { get; set; } = "Implement";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    [MaxLength(200)]
    public string? Project { get; set; }

    [MaxLength(100)]
    public string? PoNumber { get; set; }

    public DateTime? PoDate { get; set; }

    [MaxLength(200)]
    public string? SalesName { get; set; }

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
