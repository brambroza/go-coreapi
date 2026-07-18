using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models
{
    // ─── Entity ───────────────────────────────────────────────────────────────

    public class SelfJobRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string? RequestNo { get; set; }
        public string RequestTitle { get; set; } = string.Empty;
        public string? RequestType { get; set; }
        public string? RequestDetail { get; set; }
        public string? Reason { get; set; }

        /// Draft | PendingApproval | Approved | Rejected | Cancelled
        public string Status { get; set; } = "Draft";

        public string CmpId { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? SiteName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }

        /// low | medium | high | urgent
        public string? Priority { get; set; } = "medium";

        public DateTime? ExpectedServiceDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        public string? RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectReason { get; set; }

        public string? CancelledBy { get; set; }
        public DateTime? CancelledDate { get; set; }

        /// Populated after Approve — linked ServiceTicket
        public string? TicketId { get; set; }
        /// Populated after Approve — linked SubTask
        public string? SubTaskId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    public class CreateSelfJobRequestDto
    {
        public string RequestTitle { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string RequestDetail { get; set; } = string.Empty;
        public string CmpId { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? SiteName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? Reason { get; set; }
        public string? Priority { get; set; }
        public DateTime? ExpectedServiceDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? RequestedBy { get; set; }
    }

    public class ApproveSelfJobDto
    {
        public string ApprovedBy { get; set; } = string.Empty;
        public string CmpId { get; set; } = string.Empty;
    }

    public class RejectSelfJobDto
    {
        public string RejectedBy { get; set; } = string.Empty;
        public string RejectReason { get; set; } = string.Empty;
        public string CmpId { get; set; } = string.Empty;
    }

    public class CancelSelfJobDto
    {
        public string CancelledBy { get; set; } = string.Empty;
        public string CmpId { get; set; } = string.Empty;
    }

    public class SelfJobRequestResponseDto
    {
        public string RequestId { get; init; } = string.Empty;
        public string? RequestNo { get; init; }
        public string RequestTitle { get; init; } = string.Empty;
        public string? RequestType { get; init; }
        public string? RequestDetail { get; init; }
        public string? Reason { get; init; }
        public string Status { get; init; } = string.Empty;
        public string CmpId { get; init; } = string.Empty;
        public string? CustomerCode { get; init; }
        public string? CustomerName { get; init; }
        public string? SiteName { get; init; }
        public string? ContactName { get; init; }
        public string? ContactPhone { get; init; }
        public string? Priority { get; init; }
        public string? ExpectedServiceDate { get; init; }
        public decimal? EstimatedHours { get; init; }
        public decimal? EstimatedCost { get; init; }
        public string? RequestedBy { get; init; }
        public string RequestedDate { get; init; } = string.Empty;
        public string? ApprovedBy { get; init; }
        public string? ApprovedDate { get; init; }
        public string? RejectedBy { get; init; }
        public string? RejectReason { get; init; }
        public string? CancelledBy { get; init; }
        public string? TicketId { get; init; }
        public string? SubTaskId { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;
    }
}
