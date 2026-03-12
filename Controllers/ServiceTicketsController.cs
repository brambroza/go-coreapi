using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceTicketsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public ServiceTicketsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceTicketResponseDto>>> GetAll(
        [FromQuery] string? cmpId,
        [FromQuery] string? jobType,
        [FromQuery] string? customerName)
    {
        var query = _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (!string.IsNullOrWhiteSpace(jobType))
            query = query.Where(x => x.JobType == jobType);

        if (!string.IsNullOrWhiteSpace(customerName))
            query = query.Where(x => x.CustomerName.Contains(customerName));

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceTicketResponseDto>> GetById(string id)
    {
        var entity = await _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.TicketId == id);

        if (entity == null)
            return NotFound();

        return Ok(MapToResponse(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceTicketResponseDto>> Create([FromBody] ServiceTicketCreateUpdateDto dto)
    {
        var entity = new ServiceTicket
        {
            TicketId = dto.TicketId,
            TicketNo = dto.TicketNo,
            ProjectNo = dto.ProjectNo,
            CustomerName = dto.CustomerName,
            JobType = dto.JobType,
            AdditionalDetails = dto.AdditionalDetails,
            Priority = dto.Priority,
            ServiceDate = dto.ServiceDate,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CmpId = dto.CmpId,
            UpdUser = dto.UpdUser,

            ProcedureSiteSurvey = dto.Procedures.SiteSurvey,
            ProcedurePreConfig = dto.Procedures.PreConfig,
            ProcedureInstallConfig = dto.Procedures.InstallConfig,
            ProcedureUAT = dto.Procedures.UAT,
            ProcedureHandover = dto.Procedures.Handover,

            MaintenanceOnsiteService = dto.Maintenances.OnsiteService,
            MaintenancePMService = dto.Maintenances.PMService,
            MaintenanceSLAServiceLicense = dto.Maintenances.SLAServiceLicense,
            MaintenanceServiceReplacement = dto.Maintenances.ServiceReplacement,
            MaintenanceRemoteBackupConfig = dto.Maintenances.RemoteBackupConfig,
            MaintenanceReport = dto.Maintenances.Report,

            OnsiteServiceCycle = dto.MaintenanceOptions.OnsiteServiceCycle,
            PMServiceCycle = dto.MaintenanceOptions.PmServiceCycle,
            SLAType = dto.MaintenanceOptions.SlaType,
            ReplacementType = dto.MaintenanceOptions.ReplacementType,
            RemoteBackupCycle = dto.MaintenanceOptions.RemoteBackupCycle,
            ReportCycle = dto.MaintenanceOptions.ReportCycle,
            ReportSendDay = dto.ReportSendDay,

            Status = dto.Status,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        entity.JobGroups = dto.JobGroups
            .Distinct()
            .Select(g => new ServiceTicketJobGroup
            {
                TicketId = entity.TicketId,
                JobGroup = g
            })
            .ToList();

        entity.Attachments = dto.Attachments
            .Select((a, index) => new ServiceTicketAttachment
            {
                AttachmentId = Guid.NewGuid(),
                TicketId = entity.TicketId,
                Seq = a.Seq == 0 ? index + 1 : a.Seq,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileExt = a.FileExt,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                CreatedBy = a.CreatedBy ?? dto.UpdUser
            })
            .ToList();

        _context.ServiceTickets.Add(entity);
        await _context.SaveChangesAsync();

        var result = await _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .FirstAsync(x => x.TicketId == entity.TicketId);

        return CreatedAtAction(nameof(GetById), new { id = entity.TicketId }, MapToResponse(result));
    }

    [HttpGet("project/{projectNo}")]
    public async Task<ActionResult<IEnumerable<ServiceTicketResponseDto>>> GetByProjectNo(string projectNo)
    {
        var entities = await _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .Where(x => x.ProjectNo == projectNo)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = entities.Select(MapToResponse).ToList();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceTicketResponseDto>> Update(string id, [FromBody] ServiceTicketCreateUpdateDto dto)
    {
        var entity = await _context.ServiceTickets
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.TicketId == id);

        if (entity == null)
            return NotFound();

        entity.CustomerName = dto.CustomerName;
        entity.TicketNo = dto.TicketNo;
        entity.ProjectNo = dto.ProjectNo;
        entity.JobType = dto.JobType;
        entity.AdditionalDetails = dto.AdditionalDetails;
        entity.Priority = dto.Priority;
        entity.ServiceDate = dto.ServiceDate;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.CmpId = dto.CmpId;
        entity.UpdUser = dto.UpdUser;

        entity.ProcedureSiteSurvey = dto.Procedures.SiteSurvey;
        entity.ProcedurePreConfig = dto.Procedures.PreConfig;
        entity.ProcedureInstallConfig = dto.Procedures.InstallConfig;
        entity.ProcedureUAT = dto.Procedures.UAT;
        entity.ProcedureHandover = dto.Procedures.Handover;

        entity.MaintenanceOnsiteService = dto.Maintenances.OnsiteService;
        entity.MaintenancePMService = dto.Maintenances.PMService;
        entity.MaintenanceSLAServiceLicense = dto.Maintenances.SLAServiceLicense;
        entity.MaintenanceServiceReplacement = dto.Maintenances.ServiceReplacement;
        entity.MaintenanceRemoteBackupConfig = dto.Maintenances.RemoteBackupConfig;
        entity.MaintenanceReport = dto.Maintenances.Report;

        entity.OnsiteServiceCycle = dto.MaintenanceOptions.OnsiteServiceCycle;
        entity.PMServiceCycle = dto.MaintenanceOptions.PmServiceCycle;
        entity.SLAType = dto.MaintenanceOptions.SlaType;
        entity.ReplacementType = dto.MaintenanceOptions.ReplacementType;
        entity.RemoteBackupCycle = dto.MaintenanceOptions.RemoteBackupCycle;
        entity.ReportCycle = dto.MaintenanceOptions.ReportCycle;
        entity.ReportSendDay = dto.ReportSendDay;

        entity.Status = dto.Status;
        entity.UpdatedAt = DateTime.Now;

        _context.ServiceTicketJobGroups.RemoveRange(entity.JobGroups);
        entity.JobGroups = dto.JobGroups
            .Distinct()
            .Select(g => new ServiceTicketJobGroup
            {
                TicketId = entity.TicketId,
                JobGroup = g
            })
            .ToList();

        _context.ServiceTicketAttachments.RemoveRange(entity.Attachments);
        entity.Attachments = dto.Attachments
            .Select((a, index) => new ServiceTicketAttachment
            {
                AttachmentId = a.AttachmentId ?? Guid.NewGuid(),
                TicketId = entity.TicketId,
                Seq = a.Seq == 0 ? index + 1 : a.Seq,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileExt = a.FileExt,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                CreatedBy = a.CreatedBy ?? dto.UpdUser
            })
            .ToList();

        await _context.SaveChangesAsync();

        var result = await _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .FirstAsync(x => x.TicketId == entity.TicketId);

        return Ok(MapToResponse(result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var entity = await _context.ServiceTickets
            .FirstOrDefaultAsync(x => x.TicketId == id);

        if (entity == null)
            return NotFound();

        _context.ServiceTickets.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("kanban")]
    public async Task<ActionResult<IEnumerable<ServiceTicketResponseDto>>> GetAll(
    [FromQuery] string? cmpId,
    [FromQuery] string? status,
    [FromQuery] string? jobType,
    [FromQuery] string? keyword
)
    {
        var query = _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(jobType))
            query = query.Where(x => x.JobType == jobType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                (x.TicketNo ?? "").Contains(keyword) ||
                (x.CustomerName ?? "").Contains(keyword) ||
                (x.ProjectNo ?? "").Contains(keyword) ||
                (x.AdditionalDetails ?? "").Contains(keyword)
            );
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        return Ok(rows.Select(MapToResponse).ToList());
    }

    [HttpPut("kanban/{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateServiceTicketStatusDto dto)
    {
        var entity = await _context.ServiceTickets.FirstOrDefaultAsync(x => x.TicketId == id);

        if (entity == null)
            return NotFound();

        entity.Status = dto.Status;
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok();
    }

    private static ServiceTicketResponseDto MapToResponse(ServiceTicket x)
    {
        return new ServiceTicketResponseDto
        {
            TicketId = x.TicketId,
            TicketNo = x.TicketNo,
            CustomerName = x.CustomerName,
            JobType = x.JobType,
            JobGroups = x.JobGroups.Select(g => g.JobGroup).ToList(),
            AdditionalDetails = x.AdditionalDetails,
            Priority = x.Priority,
            ServiceDate = x.ServiceDate,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CmpId = x.CmpId,
            UpdUser = x.UpdUser,

            ProcedureSiteSurvey = x.ProcedureSiteSurvey,
            ProcedurePreConfig = x.ProcedurePreConfig,
            ProcedureInstallConfig = x.ProcedureInstallConfig,
            ProcedureUAT = x.ProcedureUAT,
            ProcedureHandover = x.ProcedureHandover,

            MaintenanceOnsiteService = x.MaintenanceOnsiteService,
            MaintenancePMService = x.MaintenancePMService,
            MaintenanceSLAServiceLicense = x.MaintenanceSLAServiceLicense,
            MaintenanceServiceReplacement = x.MaintenanceServiceReplacement,
            MaintenanceRemoteBackupConfig = x.MaintenanceRemoteBackupConfig,
            MaintenanceReport = x.MaintenanceReport,

            OnsiteServiceCycle = x.OnsiteServiceCycle,
            PMServiceCycle = x.PMServiceCycle,
            SLAType = x.SLAType,
            ReplacementType = x.ReplacementType,
            RemoteBackupCycle = x.RemoteBackupCycle,
            ReportCycle = x.ReportCycle,
            ReportSendDay = x.ReportSendDay,

            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,

            Attachments = x.Attachments
                .OrderBy(a => a.Seq)
                .Select(a => new ServiceTicketAttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    Seq = a.Seq,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileExt = a.FileExt,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    CreatedBy = a.CreatedBy
                })
                .ToList()
        };
    }
}