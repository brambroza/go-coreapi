using System;

namespace goalongapi.Models
{
    public class WarrantyDevice
    {
        public int Id { get; set; }

        public string SerialNo { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string? Customer { get; set; }

        public string? ProjectNo { get; set; }

        /// <summary>on | off</summary>
        public string WarrantyStatus { get; set; } = "on";

        public DateTime? WarrantyExpiry { get; set; }

        public string? CmpId { get; set; }

        public string? UpdUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
