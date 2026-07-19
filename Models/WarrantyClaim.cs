using System;

namespace goalongapi.Models
{
    /// <summary>ใบแจ้งเคลมสินค้า — รหัส CLM-YYYY-NNNN</summary>
    public class WarrantyClaim
    {
        /// <summary>รหัสใบเคลม เช่น CLM-2026-0001</summary>
        public string Id { get; set; } = string.Empty;

        public string? TicketId { get; set; }

        public string Customer { get; set; } = string.Empty;

        public string? SalesName { get; set; }

        public string ReporterStaff { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNo { get; set; } = string.Empty;

        /// <summary>on | off</summary>
        public string WarrantyStatus { get; set; } = "on";

        /// <summary>Claim Received | Under Inspection | In Progress | Completed | Rejected</summary>
        public string Status { get; set; } = "Claim Received";

        public string? Detail { get; set; }

        public string? CmpId { get; set; }

        public string? UpdUser { get; set; }

        public DateTime ClaimDate { get; set; } = DateTime.UtcNow.Date;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
