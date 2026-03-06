namespace goalongapi.Dtos;


public class ServiceTicketAttachmentDto
{
    public Guid? AttachmentId { get; set; }
    public int Seq { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExt { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? CreatedBy { get; set; }
}

public class ServiceTicketCreateUpdateDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string JobType { get; set; } = "implement";
    public List<string> JobGroups { get; set; } = new();

    public string? AdditionalDetails { get; set; }
    public string Priority { get; set; } = "minor";

    public DateTime? ServiceDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? CmpId { get; set; }
    public string UpdUser { get; set; } = string.Empty;

    public bool ProcedureSiteSurvey { get; set; }
    public bool ProcedurePreConfig { get; set; }
    public bool ProcedureInstallConfig { get; set; }
    public bool ProcedureUAT { get; set; }
    public bool ProcedureHandover { get; set; }

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

    public List<ServiceTicketAttachmentDto> Attachments { get; set; } = new();
}

public class ServiceTicketResponseDto
{
    public Guid TicketId { get; set; }
    public string? TicketNo { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public List<string> JobGroups { get; set; } = new();

    public string? AdditionalDetails { get; set; }
    public string Priority { get; set; } = string.Empty;

    public DateTime? ServiceDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? CmpId { get; set; }
    public string UpdUser { get; set; } = string.Empty;

    public bool ProcedureSiteSurvey { get; set; }
    public bool ProcedurePreConfig { get; set; }
    public bool ProcedureInstallConfig { get; set; }
    public bool ProcedureUAT { get; set; }
    public bool ProcedureHandover { get; set; }

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

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ServiceTicketAttachmentDto> Attachments { get; set; } = new();
}