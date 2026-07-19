namespace goalongapi.Dtos;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

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
    public string TicketId { get; set; }
    public string? TicketNo { get; set; }
    public string? ProjectNo { get; set; }
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

    public ServiceTicketProceduresDto? Procedures { get; set; }

    public ServiceTicketMaintenancesDto? Maintenances { get; set; }
    public ServiceTicketMaintenanceOptionsDto? MaintenanceOptions { get; set; }

    public int? ReportSendDay { get; set; }

    public string Status { get; set; } = "draft";

    public List<ServiceTicketAttachmentDto> Attachments { get; set; } = new();
    public List<ServiceTicketSubTaskDto> SubTasks { get; set; } = new();
}

public class ServiceTicketProceduresDto
{
    public bool SiteSurvey { get; set; }
    public bool PreConfig { get; set; }
    public bool InstallConfig { get; set; }
    public bool UAT { get; set; }
    public bool Handover { get; set; }
}

public class ServiceTicketMaintenancesDto
{
    public bool OnsiteService { get; set; }
    public bool PMService { get; set; }
    public bool SLAServiceLicense { get; set; }
    public bool ServiceReplacement { get; set; }
    public bool RemoteBackupConfig { get; set; }
    public bool Report { get; set; }
}

public class ServiceTicketMaintenanceOptionsDto
{
    public string? OnsiteServiceCycle { get; set; }
    public string? PmServiceCycle { get; set; }
    public string? SlaType { get; set; }
    public string? ReplacementType { get; set; }
    public string? RemoteBackupCycle { get; set; }
    public string? ReportCycle { get; set; }


}

public class ServiceTicketResponseDto
{
    public string TicketId { get; set; }
    public string? ProjectNo { get; set; }
    public string? TicketNo { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
    public string JobType { get; set; } = string.Empty;
    public List<string> JobGroups { get; set; } = new();

    public string? AdditionalDetails { get; set; }
    public string Priority { get; set; } = string.Empty;

    public DateTime? ServiceDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? CmpId { get; set; }
    public string UpdUser { get; set; } = string.Empty;
    public ServiceTicketProceduresDto? Procedures { get; set; }

    public ServiceTicketMaintenancesDto? Maintenances { get; set; }
    public ServiceTicketMaintenanceOptionsDto? MaintenanceOptions { get; set; }
    public int? ReportSendDay { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ServiceTicketAttachmentDto> Attachments { get; set; } = new();
    public List<ServiceTicketSubTaskDto> SubTasks { get; set; } = new();
}


public class ServiceTicketSubTaskDto
{
    public string? SubTaskId { get; set; }
    public string TicketId { get; set; }
    public int Seq { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = "additional";
    public bool IsDone { get; set; }
    public DateTime? DoneAt { get; set; }
    public string? DoneBy { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TransDate { get; set; }

    public List<ServiceTicketSubTaskAssignResponseDto> Assignments { get; set; } = new();

    public List<ProcedureTaskItemFileDto>? AttachFiles { get; set; }
    public string CmpId { get; set; } = string.Empty;
    public string? Remark { get; set; }

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

public class ServiceTicketSubTaskDtoUpdate
{
    public string? TicketId { get; set; }
    public string? SubTaskId { get; set; }
    public int Seq { get; set; }
    public string? Name { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Source { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DoneAt { get; set; }
    public string? DoneBy { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TransDate { get; set; }
    public string? CmpId { get; set; }
    public string? Remark { get; set; }

    public List<ServiceTicketSubTaskAssignResponseDto> Assignments { get; set; } = new();
    public List<ProcedureTaskItemFileDto>? AttachFiles { get; set; }
}


public class ProcedureTaskItemFileDto
{
    public string UpdUser { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public string SubTaskId { get; set; } = string.Empty;
    public int Seq { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
}

public class UpdateServiceTicketStatusDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

}

public class TeamServiceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "default";
}

public class TeamServiceSpResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ServiceTicketSubTaskAssignResponseDto
{
    public Guid AssignId { get; set; }
    public string SubTaskId { get; set; }
    public string TicketId { get; set; } = string.Empty;
    public string AssignUserId { get; set; } = string.Empty;
    public string? AssignUserName { get; set; }
    public string? RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}

public class ServiceTicketSubTaskAssignDto
{
    public Guid? AssignId { get; set; }
    public string SubTaskId { get; set; }
    public string AssignUserId { get; set; } = string.Empty;
    public string? AssignUserName { get; set; }
    public string? RoleName { get; set; }
    public bool IsActive { get; set; } = true;
}



public class UpdateMyTaskDto
{
    public string? Status { get; set; }
    public decimal? ProgressPercent { get; set; }
    public bool? IsDone { get; set; }
    public string? Remark { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class UpdateMyTaskStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}


public class CreateServiceTicketSubTaskCheckInDto
{
    public string TicketId { get; set; }
    public string SubTaskId { get; set; }
    public string CmpId { get; set; } = string.Empty;

    public DateTime? CheckInAt { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationText { get; set; }

    public string? CheckInBy { get; set; }
}


public class UpdateServiceTicketSubTaskCheckInDto
{
    public string TicketId { get; set; }
    public string SubTaskId { get; set; }
    public string CmpId { get; set; } = string.Empty;

    public DateTime CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationText { get; set; }

    public string? CheckInBy { get; set; }
    public string? CheckOutBy { get; set; }

    public string? UpdatedBy { get; set; }
}

public class ServiceTicketSubTaskCheckInDto
{
    public Guid CheckInId { get; set; }
    public string TicketId { get; set; }
    public string SubTaskId { get; set; }
    public string CmpId { get; set; } = string.Empty;

    public DateTime CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationText { get; set; }

    public string? CheckInBy { get; set; }
    public string? CheckOutBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}


public class CreateServiceTicketSubTaskActionDto
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
    public string? WorkDetail { get; set; }
    public string? IssueDetail { get; set; }
    public string? SignatureFilePath { get; set; }
    public string? ChecklistItemsJson { get; set; }
    public string? RackPhotosJson { get; set; }
    public string? DamagedProductJson { get; set; }
    public string? OthersItemsJson { get; set; }
}


public class UpdateServiceTicketSubTaskActionDto
{
    public string TicketId { get; set; } = string.Empty;
    public string SubTaskId { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public int Seq { get; set; }
    public DateTime ActionDate { get; set; }
    public string? ActionDetails { get; set; }
    public string? ActionStatus { get; set; }
    public string? Tomorrow { get; set; }
    public string? WorkDetail { get; set; }
    public string? IssueDetail { get; set; }
    public string? SignatureFilePath { get; set; }
    public string? ChecklistItemsJson { get; set; }
    public string? RackPhotosJson { get; set; }
    public string? DamagedProductJson { get; set; }
    public string? OthersItemsJson { get; set; }
}


public class ServiceTicketSubTaskActionDto
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
    public string? WorkDetail { get; set; }
    public string? IssueDetail { get; set; }
    public string? SignatureFilePath { get; set; }
    public string? ChecklistItemsJson { get; set; }
    public string? RackPhotosJson { get; set; }
    public string? DamagedProductJson { get; set; }
    public string? OthersItemsJson { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ServiceTicketSubTaskActionAttachmentDto> Attachments { get; set; } = new();
}

/// <summary>
/// Payload สำหรับ POST /api/ServiceTickets/{id}/close-request
/// อัพสถานะ ticket เป็น "Waiting Close Approval" และส่ง email แจ้งลูกค้า
/// </summary>
public class CloseRequestDto
{
    public string CmpId { get; set; } = string.Empty;
    /// <summary>อีเมลปลายทาง (required)</summary>
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    /// <summary>HTML body แบบกำหนดเอง — ถ้าไม่ส่งมาระบบจะสร้างอัตโนมัติ</summary>
    public string? Body { get; set; }
    /// <summary>Data URL ของลายเซ็นลูกค้า (canvas.toDataURL) — optional</summary>
    public string? SignatureBase64 { get; set; }
    /// <summary>true เมื่อลูกค้าไม่ได้เซ็นและ staff เลือก Skip</summary>
    public bool SkipSignature { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CloseRequestResponseDto
{
    public string TicketId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
    public string? EmailError { get; set; }
}

public class SendReportEmailDto
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PdfBase64 { get; set; }
    public string? FileName { get; set; }
    public string TicketId { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
}

public class CreateReplacementTicketDto
{
    public string SourceTicketId { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNo { get; set; } = string.Empty;
    public string Warranty { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateHelpdeskCaseDto
{
    public string SourceTicketId { get; set; } = string.Empty;
    public string CmpId { get; set; } = string.Empty;
    public string Reporter { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}



public class CreateServiceTicketSubTaskActionAttachmentDto
{
    public string TaskActionId { get; set; }
    public int Seq { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExt { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? CreatedBy { get; set; }
}


public class UpdateServiceTicketSubTaskActionAttachmentDto
{
    public int Seq { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExt { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? CreatedBy { get; set; }
}


public class ServiceTicketSubTaskActionAttachmentDto
{
    public string? AttachmentId { get; set; }
    public string TaskActionId { get; set; } = string.Empty;
    public int Seq { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExt { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}


public class SubTaskSendApproveDto
{
    public string SubTaskId { get; set; } = string.Empty;
    public string SendApproveBy { get; set; } = string.Empty;
}

public class SubTaskApproveDto
{
    public string SubTaskId { get; set; } = string.Empty;
    public string ApproveBy { get; set; } = string.Empty;
}

public class SubTaskRejectDto
{
    public string SubTaskId { get; set; } = string.Empty;
    public string RejectBy { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
}

public class BulkAssignDto
{
    public List<string> TicketIds { get; set; } = new();
    public string AssignUserId { get; set; } = string.Empty;
    public string AssignUserName { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
}

public class BulkStatusDto
{
    public List<string> TicketIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}