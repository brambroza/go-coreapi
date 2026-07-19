using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models.Nis;

// ── Read-only EF mappings for the Service Board → Customer directory ────────────
// These map EXISTING master tables (no migration). The NIS customer tab reads:
//   msb.mCustomer (via Models.Customer) + msb.mCustomerLocations
//   + msb.mCustomerAssignEmp (→ Account.FullName) + dbo.Contact (DocType='customer').

/// msb.mCustomerLocations
public class NisCustomerLocation
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public int Seq { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lon { get; set; }
    public string? LocationName { get; set; }
    public string? Remark { get; set; }
    public string? LocationURL { get; set; }
    public string? UpdUser { get; set; }
}

/// msb.mCustomerAssignEmp — staff assigned to a customer (AccountID → Account.FullName).
public class NisCustomerAssignEmp
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public long AccountID { get; set; }
    public int? Priority { get; set; }
    public string? UpdUser { get; set; }
}

/// dbo.Contact — generic contacts linked to a customer via DocType='customer', DocNo=CustomerCode.
[Table("Contact", Schema = "dbo")]
public class NisContactRow
{
    public string ContactId { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? ContactLineId { get; set; }
    public string CmpId { get; set; } = string.Empty;
    public string? DocNo { get; set; }
    public string? DocType { get; set; }
}
