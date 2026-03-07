using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{


    public class ServiceTicket
    {
        public string TicketId { get; set; }
        public string ProjectNo { get; set; }
        public string? TicketNo { get; set; }
        public string CustomerName { get; set; } = string.Empty;
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

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<ServiceTicketJobGroup> JobGroups { get; set; } = new List<ServiceTicketJobGroup>();
        public virtual ICollection<ServiceTicketAttachment> Attachments { get; set; } = new List<ServiceTicketAttachment>();
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

}