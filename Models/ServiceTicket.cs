using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace goalongapi.Models
{
    public class ServiceTicket
    {
        public string TicketId { get; set; }
        public string? ProjectNo { get; set; }
        public string? TicketNo { get; set; }


        public string JobType { get; set; } = "implement"; // implement, maintenance

        public string? AdditionalDetails { get; set; }

        public string Priority { get; set; } = "minor"; // minor, major, critical

        public DateTime? ServiceDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? CmpId { get; set; }

        public string UpdUser { get; set; } = string.Empty;

        // Implement Procedures
        public bool ProcedureSiteSurvey { get; set; }
        public bool ProcedurePreConfig { get; set; }
        public bool ProcedureInstallConfig { get; set; }
        public bool ProcedureUAT { get; set; }
        public bool ProcedureHandover { get; set; }

        // Maintenance Flags
        public bool MaintenanceOnsiteService { get; set; }
        public bool MaintenancePMService { get; set; }
        public bool MaintenanceSLAServiceLicense { get; set; }
        public bool MaintenanceServiceReplacement { get; set; }
        public bool MaintenanceRemoteBackupConfig { get; set; }
        public bool MaintenanceReport { get; set; }

        public string? OnsiteServiceCycle { get; set; }


        public string? PMServiceCycle { get; set; }


        public string? SLAType { get; set; }

        public string? ReplacementType { get; set; }

        public string? RemoteBackupCycle { get; set; }


        public string? ReportCycle { get; set; }

        public int? ReportSendDay { get; set; }


        public string Status { get; set; } = "draft";

        /// Set by Service Manager — onsite form skips the customer-signature requirement.
        public bool SkipSignature { get; set; }
        /// Set by Service Manager — closing this ticket goes through approval instead of direct email.
        public bool RequireCloseApproval { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<ServiceTicketJobGroup> JobGroups { get; set; } = new List<ServiceTicketJobGroup>();
        public virtual ICollection<ServiceTicketAttachment> Attachments { get; set; } = new List<ServiceTicketAttachment>();

        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        public virtual Customer? Customer { get; set; }

        [NotMapped]
        public string? ImagePath => Customer?.ImgPath;
        public virtual ICollection<ServiceTicketSubTask> SubTasks { get; set; } = new List<ServiceTicketSubTask>();

    }


    public class ServiceTicketSubTask
    {
        public string SubTaskId { get; set; }

        public string TicketId { get; set; } = string.Empty;

        public int Seq { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // standard, additional
        public string Source { get; set; } = "additional";

        public bool IsDone { get; set; }

        public DateTime? DoneAt { get; set; }

        public string? DoneBy { get; set; }
        public string? CmpId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "pending";
        public string TaskStatus { get; set; } = "pending";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TransDate { get; set; }

        public string TeamId { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public string? Remark { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public decimal? ProgressPercent { get; set; }

        public virtual ServiceTicket? ServiceTicket { get; set; }
        public virtual ICollection<ServiceTicketSubTaskAssign> Assignments { get; set; } = new List<ServiceTicketSubTaskAssign>();
        public virtual ICollection<ServiceTicketSubTaskFile> AttachFiles { get; set; } = new List<ServiceTicketSubTaskFile>();


        public string? StateApprove { get; set; }
        public DateTime? DateApprove { get; set; }
        public string? ApproveBy { get; set; }

        public string? StateSendApprove { get; set; }
        public DateTime? DateSendApprove { get; set; }
        public string? SendApproveBy { get; set; }

        public string? RejectBy { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? DateReject { get; set; }

    }

    public class ServiceTicketSubTaskFile
    {
        public Guid FileId { get; set; }
        public string SubTaskId { get; set; }
        public string CmpId { get; set; } = string.Empty;
        public int Seq { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string UpdUser { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public virtual ServiceTicketSubTask? SubTask { get; set; }
    }

    public class ServiceTicketSubTaskAssign
    {
        public Guid AssignId { get; set; }

        public string SubTaskId { get; set; }

        public string TicketId { get; set; } = string.Empty;

        public string AssignUserId { get; set; } = string.Empty;
        public string? AssignUserName { get; set; }

        public string? RoleName { get; set; }   // optional เช่น owner, support, implement

        public bool IsActive { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.Now;
        public string AssignedBy { get; set; } = string.Empty;

        public DateTime? UnassignedAt { get; set; }
        public string? UnassignedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ServiceTicketSubTask? SubTask { get; set; }
    }


    public class ServiceTicketJobGroup
    {
        public long Id { get; set; }

        public string TicketId { get; set; }

        public string JobGroup { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ServiceTicket? ServiceTicket { get; set; }
    }

    public class ServiceTicketAttachment
    {
        public Guid AttachmentId { get; set; } = Guid.NewGuid();
        public string TicketId { get; set; }

        public int Seq { get; set; } = 1;

        public string FileName { get; set; } = string.Empty;

        public string? FilePath { get; set; }
        public string? FileExt { get; set; }

        public long? FileSize { get; set; }

        public string? ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; }

        public virtual ServiceTicket? ServiceTicket { get; set; }
    }


    public class ServiceTicketSubTaskCheckIn
    {

        public Guid CheckInId { get; set; }

        public String TicketId { get; set; }

        public String SubTaskId { get; set; }

        public string CmpId { get; set; } = string.Empty;

        public DateTime CheckInAt { get; set; }

        public DateTime? CheckOutAt { get; set; }

        [Column(TypeName = "decimal(18,10)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(18,10)")]
        public decimal? Longitude { get; set; }


        public string? LocationText { get; set; }


        public string? CheckInBy { get; set; }


        public string? CheckOutBy { get; set; }

        public DateTime? UpdatedAt { get; set; }


        public string? UpdatedBy { get; set; }
    }

    public class ServiceTicketSubTaskAction
    {

        public string TaskActionId { get; set; }

        public string TicketId { get; set; } = string.Empty;


        public string SubTaskId { get; set; } = string.Empty;


        public string CmpId { get; set; } = string.Empty;

        public int Seq { get; set; }

        public DateTime ActionDate { get; set; }


        public string? ActionDetails { get; set; }


        public string? ActionStatus { get; set; }


        public string? Tomorrow { get; set; }

        // Extended fields
        public string? WorkDetail { get; set; }
        public string? IssueDetail { get; set; }
        public string? SignatureFilePath { get; set; }
        public string? ChecklistItemsJson { get; set; }
        public string? RackPhotosJson { get; set; }
        public string? DamagedProductJson { get; set; }
        public string? OthersItemsJson { get; set; }

        // Onsite service report fields (NIS onsite form)
        /// e.g. SR-202607-0001
        public string? SrNumber { get; set; }
        /// Customer signature, stored as a base64 data URL (same convention as RackPhotosJson/DamagedProductJson).
        public string? SignatureImageBase64 { get; set; }
        /// JSON array of work photo base64 data URLs.
        public string? WorkPhotosJson { get; set; }

        // GPS captured client-side at check-in / check-out (browser Geolocation API, best-effort).
        public decimal? CheckInLatitude { get; set; }
        public decimal? CheckInLongitude { get; set; }
        public decimal? CheckOutLatitude { get; set; }
        public decimal? CheckOutLongitude { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public ICollection<ServiceTicketSubTaskActionAttachment> Attachments { get; set; }
                  = new List<ServiceTicketSubTaskActionAttachment>();
    }

    public class ServiceTicketSubTaskActionAttachment
    {
        public string AttachmentId { get; set; }

        public string TaskActionId { get; set; }

        public int Seq { get; set; }


        public string FileName { get; set; } = string.Empty;

        public string? FilePath { get; set; }


        public string? FileExt { get; set; }

        public long? FileSize { get; set; }


        public string? ContentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public ServiceTicketSubTaskAction? TaskAction { get; set; }
    }





}