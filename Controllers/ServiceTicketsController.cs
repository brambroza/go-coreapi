using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            /* MaintenanceOnsiteService = dto.Maintenances.OnsiteService,
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
            ReportSendDay = dto.ReportSendDay, */




            Status = dto.Status,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        if (dto.Maintenances != null)
        {
            entity.MaintenanceOnsiteService = dto.Maintenances.OnsiteService;
            entity.MaintenancePMService = dto.Maintenances.PMService;
            entity.MaintenanceSLAServiceLicense = dto.Maintenances.SLAServiceLicense;
            entity.MaintenanceServiceReplacement = dto.Maintenances.ServiceReplacement;
            entity.MaintenanceRemoteBackupConfig = dto.Maintenances.RemoteBackupConfig;
            entity.MaintenanceReport = dto.Maintenances.Report;
        }

        if (dto.MaintenanceOptions != null)
        {
            entity.OnsiteServiceCycle = dto.MaintenanceOptions.OnsiteServiceCycle;
            entity.PMServiceCycle = dto.MaintenanceOptions.PmServiceCycle;
            entity.SLAType = dto.MaintenanceOptions.SlaType;
            entity.ReplacementType = dto.MaintenanceOptions.ReplacementType;
            entity.RemoteBackupCycle = dto.MaintenanceOptions.RemoteBackupCycle;
            entity.ReportCycle = dto.MaintenanceOptions.ReportCycle;
            entity.ReportSendDay = dto.ReportSendDay;
        }

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

        entity.SubTasks = dto.SubTasks
        .Where(x => !string.IsNullOrWhiteSpace(x.Title))
        .Select((x, index) => new ServiceTicketSubTask
        {
            SubTaskId = x.SubTaskId,
            TicketId = entity.TicketId,
            Seq = x.Seq == 0 ? index + 1 : x.Seq,
            Name = x.Title.Trim(),
            Title = x.Title.Trim(),
            Source = string.IsNullOrWhiteSpace(x.Source) ? "additional" : x.Source.Trim(),
            IsDone = x.IsDone,
            DoneAt = x.IsDone ? x.DoneAt ?? DateTime.Now : null,
            DoneBy = x.IsDone ? (x.DoneBy ?? dto.UpdUser) : null,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CmpId = x.CmpId
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


    [HttpPost("subtask")]
    public async Task<ActionResult<ServiceTicketResponseDto>> CreateSubTask(
    [FromBody] List<ServiceTicketSubTaskDtoUpdate> dto)
    {
        if (dto == null || !dto.Any())
            return BadRequest("SubTask data is required.");

        var ticketId = dto.FirstOrDefault()?.TicketId;
        var ticketSubId = dto.FirstOrDefault()?.SubTaskId;

        if (string.IsNullOrWhiteSpace(ticketId))
            return BadRequest("TicketId is required.");

        var ticket = await _context.ServiceTickets
            .FirstOrDefaultAsync(x => x.TicketId == ticketId);

        if (ticket == null)
            return NotFound($"ServiceTicket '{ticketId}' not found.");

        var inputSubTaskIds = dto
            .Where(x => !string.IsNullOrWhiteSpace(x.SubTaskId))
            .Select(x => x.SubTaskId!)
            .Distinct()
            .ToList();

        var existingSubTasks = await _context.ServiceTicketSubTasks
            .Include(x => x.Assignments)
            .Include(x => x.AttachFiles)
            .Where(x => x.SubTaskId == ticketSubId)
            .ToListAsync();

        foreach (var x in dto.Where(x => !string.IsNullOrWhiteSpace(x.Title)).Select((item, index) => new { item, index }))
        {
            var item = x.item;
            var index = x.index;

            ServiceTicketSubTask? entity = null;

            if (!string.IsNullOrWhiteSpace(item.SubTaskId))
            {
                entity = existingSubTasks.FirstOrDefault(s => s.SubTaskId == item.SubTaskId);
            }

            var isNew = entity == null;

            if (isNew)
            {
                var subTaskId = string.IsNullOrWhiteSpace(item.SubTaskId)
                    ? Guid.NewGuid().ToString()
                    : item.SubTaskId!;

                entity = new ServiceTicketSubTask
                {
                    SubTaskId = subTaskId,
                    TicketId = item.TicketId ?? ticketId,
                    CreatedAt = DateTime.Now,
                };

                _context.ServiceTicketSubTasks.Add(entity);
                existingSubTasks.Add(entity);
            }

            entity.TicketId = item.TicketId ?? ticketId;
            entity.Seq = item.Seq == 0 ? index + 1 : item.Seq;
            entity.Name = string.IsNullOrWhiteSpace(item.Name)
                ? item.Title.Trim()
                : item.Name.Trim();
            entity.Title = item.Title.Trim();
            entity.Source = string.IsNullOrWhiteSpace(item.Source)
                ? "additional"
                : item.Source.Trim();
            entity.IsDone = item.IsDone;
            /*   entity.DoneAt = item.IsDone ? item.DoneAt ?? DateTime.Now : null;
              entity.DoneBy = item.IsDone ? (item.DoneBy ?? ticket.UpdUser) : null; */
            entity.Status = string.IsNullOrWhiteSpace(item.Status) ? "pending" : item.Status;
            entity.CmpId = item.CmpId;
            entity.StartDate = item.StartDate;
            entity.EndDate = item.EndDate;
            entity.UpdatedAt = DateTime.Now;
            entity.Remark = item.Remark;

            // replace assignments
            if (entity.Assignments == null)
                entity.Assignments = new List<ServiceTicketSubTaskAssign>();

            if (entity.Assignments.Any())
            {
                _context.ServiceTicketSubTaskAssigns.RemoveRange(entity.Assignments);
                entity.Assignments.Clear();
            }

            if (item.Assignments?.Any() == true)
            {
                foreach (var ass in item.Assignments)
                {
                    entity.Assignments.Add(new ServiceTicketSubTaskAssign
                    {
                        AssignId = Guid.NewGuid(),
                        SubTaskId = entity.SubTaskId,
                        TicketId = item.TicketId ?? ticketId,
                        AssignUserId = ass.AssignUserId,
                        AssignUserName = ass.AssignUserName,
                        RoleName = ass.RoleName,
                        AssignedAt = ass.AssignedAt,
                        AssignedBy = ass.AssignedBy ?? ticket.UpdUser,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
            }

            // replace attach files
            if (entity.AttachFiles == null)
                entity.AttachFiles = new List<ServiceTicketSubTaskFile>();

            if (entity.AttachFiles.Any())
            {
                _context.ServiceTicketSubTaskFiles.RemoveRange(entity.AttachFiles);
                entity.AttachFiles.Clear();
            }

            if (item.AttachFiles?.Any() == true)
            {
                foreach (var file in item.AttachFiles.Select((f, fileIndex) => new { f, fileIndex }))
                {
                    entity.AttachFiles.Add(new ServiceTicketSubTaskFile
                    {
                        FileId = Guid.NewGuid(),
                        SubTaskId = entity.SubTaskId,
                        Seq = file.f.Seq == 0 ? file.fileIndex + 1 : file.f.Seq,
                        FileName = file.f.FileName,
                        FilePath = file.f.FilePath,
                        UpdUser = file.f.UpdUser ?? ticket.UpdUser,
                        CmpId = file.f.CmpId ?? item.CmpId,
                        CreatedAt = DateTime.Now
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        var result = await _context.ServiceTickets
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.JobGroups)
            .Include(x => x.Attachments)
            .Include(x => x.SubTasks)
                .ThenInclude(st => st.Assignments)
            .Include(x => x.SubTasks)
                .ThenInclude(st => st.AttachFiles)
            .FirstAsync(x => x.TicketId == ticketId);

        return Ok(MapToResponse(result));
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
            .Include(x => x.SubTasks)
                .ThenInclude(x => x.Assignments)
            .Include(x => x.SubTasks)
                .ThenInclude(x => x.AttachFiles)
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


        if (entity.SubTasks?.Any() == true)
        {
            _context.ServiceTicketSubTasks.RemoveRange(entity.SubTasks);
        }

        entity.SubTasks = dto.SubTasks
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Select((x, index) => new ServiceTicketSubTask
            {
                SubTaskId = x.SubTaskId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TicketId = entity.TicketId,
                Seq = x.Seq == 0 ? index + 1 : x.Seq,
                Name = x.Name.Trim(),
                Title = x.Title.Trim(),
                Source = string.IsNullOrWhiteSpace(x.Source) ? "additional" : x.Source.Trim(),
                IsDone = x.IsDone,
                DoneAt = x.IsDone ? x.DoneAt ?? DateTime.Now : null,
                DoneBy = x.IsDone ? (x.DoneBy ?? dto.UpdUser) : null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
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
            .Include(x => x.Customer)
            .Include(x => x.SubTasks)
                .ThenInclude(x => x.Assignments)
            .Include(x => x.SubTasks)
                .ThenInclude(x => x.AttachFiles)
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

    [HttpPut("kanban/subtask/{id}/status")]
    public async Task<IActionResult> UpdateStatusSubtask(string id, [FromBody] UpdateServiceTicketStatusDto dto)
    {
        var entity = await _context.ServiceTicketSubTasks.FirstOrDefaultAsync(x => x.SubTaskId == id);

        if (entity == null)
            return NotFound();

        entity.Status = dto.Status;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok();
    }



    private static readonly string[] ColorPool =
{
    "default", "warning", "info", "success", "error", "primary", "secondary"
};

    private static string GetColorFromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "default";

        var hash = Math.Abs(key.GetHashCode());
        return ColorPool[hash % ColorPool.Length];
    }

    [HttpGet("column/serviceteams")]
    public async Task<ActionResult<IEnumerable<TeamServiceDto>>> Get([FromQuery] string cmpId)
    {
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest("cmpId is required.");

        var cmpIdParam = new SqlParameter("@CmpId", cmpId);

        var raw = await _context.TeamServiceSpResults
            .FromSqlRaw("EXEC dbo.sp_getteamservice @CmpId", cmpIdParam)
            .AsNoTracking()
            .ToListAsync();

        var data = raw.Select(x => new TeamServiceDto
        {
            Id = x.Id,
            Name = x.Name,
            Color = GetColorFromKey(x.Id)
        }).ToList();

        return Ok(data);
    }

    [HttpPost("subtasks/{id}/assign")]
    public async Task<ActionResult> AssignSubTask(string id, [FromBody] ServiceTicketSubTaskAssignDto dto)
    {
        var subTask = await _context.ServiceTicketSubTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SubTaskId == id);

        if (subTask == null)
            return NotFound("SubTask not found.");

        var exists = await _context.ServiceTicketSubTaskAssigns.AnyAsync(x =>
            x.SubTaskId == id &&
            x.AssignUserId == dto.AssignUserId &&
            x.IsActive);

        if (exists)
            return BadRequest("This user is already assigned.");

        var entity = new ServiceTicketSubTaskAssign
        {
            AssignId = dto.AssignId ?? Guid.NewGuid(),
            SubTaskId = id,
            TicketId = subTask.TicketId,
            AssignUserId = dto.AssignUserId,
            AssignUserName = dto.AssignUserName,
            RoleName = dto.RoleName,
            IsActive = true,
            AssignedAt = DateTime.Now,
            AssignedBy = "system", // หรือเอาจาก login user
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.ServiceTicketSubTaskAssigns.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            entity.AssignId,
            entity.SubTaskId,
            entity.TicketId,
            entity.AssignUserId,
            entity.AssignUserName,
            entity.RoleName,
            entity.IsActive,
            entity.AssignedAt
        });
    }


    [HttpDelete("subtasks/assignments/{assignId:guid}")]
    public async Task<ActionResult> UnassignSubTask(Guid assignId)
    {
        var entity = await _context.ServiceTicketSubTaskAssigns
            .FirstOrDefaultAsync(x => x.AssignId == assignId);

        if (entity == null)
            return NotFound();

        entity.IsActive = false;
        entity.UnassignedAt = DateTime.Now;
        entity.UnassignedBy = "system";
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{ticketId}/subtasks/assign-summary")]
    public async Task<ActionResult> GetAssignSummary(string ticketId)
    {
        var totalSubTasks = await _context.ServiceTicketSubTasks
            .CountAsync(x => x.TicketId == ticketId);

        var assignedSubTasks = await _context.ServiceTicketSubTasks
            .Where(x => x.TicketId == ticketId)
            .CountAsync(x => x.Assignments.Any(a => a.IsActive));

        var unassignedSubTasks = totalSubTasks - assignedSubTasks;

        return Ok(new
        {
            TicketId = ticketId,
            TotalSubTasks = totalSubTasks,
            AssignedSubTasks = assignedSubTasks,
            UnassignedSubTasks = unassignedSubTasks
        });
    }



    [HttpGet("my/tasks")]
    public async Task<ActionResult> GetMyTasks(
    [FromQuery] string cmpId,
    [FromQuery] string userId,
    [FromQuery] string? status,
    [FromQuery] string? keyword)
    {
        var query = _context.ServiceTicketSubTasks
            .AsNoTracking()
            .Include(x => x.ServiceTicket)
            .Include(x => x.Assignments)
            .Include(x => x.AttachFiles)
            .Where(x => x.CmpId == cmpId &&
                        x.Assignments.Any(a => a.IsActive && a.AssignUserId == userId));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Title.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                (x.ServiceTicket != null && (
                    (x.ServiceTicket.TicketNo ?? "").Contains(keyword) ||
                    (x.ServiceTicket.CustomerName ?? "").Contains(keyword) ||
                    (x.ServiceTicket.ProjectNo ?? "").Contains(keyword)
                )));
        }

        var rows = await query
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync();

        var result = rows.Select(x => new
        {
            x.SubTaskId,
            x.TicketId,
            TicketNo = x.ServiceTicket!.TicketNo,
            ProjectNo = x.ServiceTicket!.ProjectNo,
            CustomerName = x.ServiceTicket!.CustomerName,
            x.TeamId,
            x.TeamName,
            x.Seq,
            x.Title,
            x.Name,
            x.Source,
            x.Status,
            x.TaskStatus,
            x.IsDone,
            x.DoneAt,
            x.DoneBy,
            x.StartDate,
            x.EndDate,
            x.ProgressPercent,
            Assignments = x.Assignments
            .Where(a => a.IsActive && a.SubTaskId == x.SubTaskId)
            .Select(a => new
            {
                a.AssignId,
                a.SubTaskId,
                a.TicketId,
                a.AssignUserId,
                a.AssignUserName,
                a.RoleName,
                a.IsActive,
                a.AssignedAt,
                a.AssignedBy
            })
            .ToList(),
            AttachFiles = x.AttachFiles
                .OrderBy(f => f.Seq)
                .Select(f => new
                {
                    f.FileId,
                    f.Seq,
                    f.FileName,
                    f.FilePath
                }).ToList()
        });

        return Ok(result);
    }


    [HttpPut("my/tasks/{subTaskId}")]
    public async Task<IActionResult> UpdateMyTask(string subTaskId, [FromBody] UpdateMyTaskDto dto)
    {
        var entity = await _context.ServiceTicketSubTasks
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.SubTaskId == subTaskId);

        if (entity == null)
            return NotFound("SubTask not found.");

        var canEdit = entity.Assignments.Any(a => a.IsActive && a.AssignUserId == dto.UpdatedBy);
        if (!canEdit)
            return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.Status))
            entity.Status = dto.Status;

        if (dto.ProgressPercent.HasValue)
            entity.ProgressPercent = dto.ProgressPercent.Value;

        if (dto.StartDate.HasValue)
            entity.StartDate = dto.StartDate;

        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate;

        if (dto.IsDone.HasValue)
        {
            entity.IsDone = dto.IsDone.Value;

            if (dto.IsDone.Value)
            {
                entity.DoneAt = DateTime.Now;
                entity.DoneBy = dto.UpdatedBy;
                entity.Status = "completed";
                entity.ProgressPercent = 100;
            }
            else
            {
                entity.DoneAt = null;
                entity.DoneBy = null;
            }
        }

        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok();
    }


    [HttpPut("my/tasks/{subTaskId}/status")]
    public async Task<IActionResult> UpdateMyTaskStatus(string subTaskId, [FromBody] UpdateMyTaskStatusDto dto)
    {
        var entity = await _context.ServiceTicketSubTasks
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.SubTaskId == subTaskId);

        if (entity == null)
            return NotFound("SubTask not found.");

        var canEdit = entity.Assignments.Any(a => a.IsActive && a.AssignUserId == dto.UpdatedBy);
        if (!canEdit)
            return Forbid();

        entity.TaskStatus = dto.Status;
        entity.UpdatedAt = DateTime.Now;

        if (dto.Status == "inprogress" && entity.StartDate == null)
            entity.StartDate = DateTime.Now;

        if (dto.Status == "completed")
        {
            entity.IsDone = true;
            entity.DoneAt = DateTime.Now;
            entity.DoneBy = dto.UpdatedBy;
            entity.ProgressPercent = 100;
            entity.EndDate = entity.EndDate ?? DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }


    // Check in 



    [HttpGet("task/checkIn")]
    public async Task<ActionResult<IEnumerable<ServiceTicketSubTaskCheckInDto>>> CheckinGetAll(
        [FromQuery] string? cmpId,
        [FromQuery] string? ticketId,
        [FromQuery] string? subTaskId)
    {
        var query = _context.ServiceTicketSubTaskCheckIns
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (ticketId != "")
            query = query.Where(x => x.TicketId == ticketId);

        if (subTaskId != "")
            query = query.Where(x => x.SubTaskId == subTaskId);

        var result = await query
            .OrderByDescending(x => x.CheckInAt)
            .Select(x => new ServiceTicketSubTaskCheckInDto
            {
                CheckInId = x.CheckInId,
                TicketId = x.TicketId,
                SubTaskId = x.SubTaskId,
                CmpId = x.CmpId,
                CheckInAt = x.CheckInAt,
                CheckOutAt = x.CheckOutAt,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                LocationText = x.LocationText,
                CheckInBy = x.CheckInBy,
                CheckOutBy = x.CheckOutBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("task/checkIn/{checkInId:guid}")]
    public async Task<ActionResult<ServiceTicketSubTaskCheckInDto>> GetById(Guid checkInId)
    {
        var entity = await _context.ServiceTicketSubTaskCheckIns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CheckInId == checkInId);

        if (entity == null)
            return NotFound(new { message = "Check-in not found" });

        return Ok(new ServiceTicketSubTaskCheckInDto
        {
            CheckInId = entity.CheckInId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            CheckInAt = entity.CheckInAt,
            CheckOutAt = entity.CheckOutAt,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            LocationText = entity.LocationText,
            CheckInBy = entity.CheckInBy,
            CheckOutBy = entity.CheckOutBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        });
    }

    [HttpPost("task/checkIn")]
    public async Task<ActionResult<ServiceTicketSubTaskCheckInDto>> Create(
        [FromBody] CreateServiceTicketSubTaskCheckInDto request)
    {
        var entity = new ServiceTicketSubTaskCheckIn
        {
            CheckInId = Guid.NewGuid(),
            TicketId = request.TicketId,
            SubTaskId = request.SubTaskId,
            CmpId = request.CmpId,
            CheckInAt = request.CheckInAt ?? DateTime.Now,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationText = request.LocationText,
            CheckInBy = request.CheckInBy,
            UpdatedAt = DateTime.Now,
            UpdatedBy = request.CheckInBy
        };

        _context.ServiceTicketSubTaskCheckIns.Add(entity);
        await _context.SaveChangesAsync();

        var result = new ServiceTicketSubTaskCheckInDto
        {
            CheckInId = entity.CheckInId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            CheckInAt = entity.CheckInAt,
            CheckOutAt = entity.CheckOutAt,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            LocationText = entity.LocationText,
            CheckInBy = entity.CheckInBy,
            CheckOutBy = entity.CheckOutBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };

        return CreatedAtAction(nameof(GetById), new { checkInId = entity.CheckInId }, result);
    }

    [HttpPut("task/checkIn/{checkInId:guid}")]
    public async Task<ActionResult<ServiceTicketSubTaskCheckInDto>> Update(
        Guid checkInId,
        [FromBody] UpdateServiceTicketSubTaskCheckInDto request)
    {
        var entity = await _context.ServiceTicketSubTaskCheckIns
            .FirstOrDefaultAsync(x => x.CheckInId == checkInId);

        if (entity == null)
            return NotFound(new { message = "Check-in not found" });

        entity.TicketId = request.TicketId;
        entity.SubTaskId = request.SubTaskId;
        entity.CmpId = request.CmpId;
        entity.CheckInAt = request.CheckInAt;
        entity.CheckOutAt = request.CheckOutAt;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.LocationText = request.LocationText;
        entity.CheckInBy = request.CheckInBy;
        entity.CheckOutBy = request.CheckOutBy;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync();

        return Ok(new ServiceTicketSubTaskCheckInDto
        {
            CheckInId = entity.CheckInId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            CheckInAt = entity.CheckInAt,
            CheckOutAt = entity.CheckOutAt,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            LocationText = entity.LocationText,
            CheckInBy = entity.CheckInBy,
            CheckOutBy = entity.CheckOutBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        });
    }

    [HttpDelete("task/checkIn/{checkInId:guid}")]
    public async Task<IActionResult> Delete(Guid checkInId)
    {
        var entity = await _context.ServiceTicketSubTaskCheckIns
            .FirstOrDefaultAsync(x => x.CheckInId == checkInId);

        if (entity == null)
            return NotFound(new { message = "Check-in not found" });

        _context.ServiceTicketSubTaskCheckIns.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }
    // end check-in


    // sub task action 

    [HttpGet("subtask/actions")]
    public async Task<ActionResult<IEnumerable<ServiceTicketSubTaskActionDto>>> ActionsGetAll(
               [FromQuery] string? cmpId,
               [FromQuery] string? ticketId,
               [FromQuery] string? subTaskId)
    {
        var query = _context.ServiceTicketSubTaskActions
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (!string.IsNullOrWhiteSpace(ticketId))
            query = query.Where(x => x.TicketId == ticketId);

        if (!string.IsNullOrWhiteSpace(subTaskId))
            query = query.Where(x => x.SubTaskId == subTaskId);

        var result = await query
            .OrderBy(x => x.ActionDate)
            .ThenBy(x => x.Seq)
            .Select(x => new ServiceTicketSubTaskActionDto
            {
                TaskActionId = x.TaskActionId,
                TicketId = x.TicketId,
                SubTaskId = x.SubTaskId,
                CmpId = x.CmpId,
                Seq = x.Seq,
                ActionDate = x.ActionDate,
                ActionDetails = x.ActionDetails,
                ActionStatus = x.ActionStatus,
                Tomorrow = x.Tomorrow,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("subtask/actions/{taskActionId}")]
    public async Task<ActionResult<ServiceTicketSubTaskActionDto>> ActionsGetById(string taskActionId)
    {
        var entity = await _context.ServiceTicketSubTaskActions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TaskActionId == taskActionId);

        if (entity == null)
            return NotFound(new { message = "Task action not found" });

        return Ok(new ServiceTicketSubTaskActionDto
        {
            TaskActionId = entity.TaskActionId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            Seq = entity.Seq,
            ActionDate = entity.ActionDate,
            ActionDetails = entity.ActionDetails,
            ActionStatus = entity.ActionStatus,
            Tomorrow = entity.Tomorrow,
            UpdatedAt = entity.UpdatedAt
        });
    }

    [HttpPost("subtask/actions")]
    public async Task<ActionResult<ServiceTicketSubTaskActionDto>> ActionsCreate(
     [FromBody] CreateServiceTicketSubTaskActionDto request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request is required" });
        }

        ServiceTicketSubTaskAction? entity = null;


        entity = await _context.ServiceTicketSubTaskActions
            .FirstOrDefaultAsync(x => x.TaskActionId == request.TaskActionId);


        if (entity == null)
        {
            entity = new ServiceTicketSubTaskAction
            {
                TaskActionId = request.TaskActionId,
                TicketId = request.TicketId,
                SubTaskId = request.SubTaskId,
                CmpId = request.CmpId,
                Seq = request.Seq,
                ActionDate = request.ActionDate.Date,
                ActionDetails = request.ActionDetails,
                ActionStatus = request.ActionStatus,
                Tomorrow = request.Tomorrow,
                UpdatedAt = DateTime.Now
            };

            _context.ServiceTicketSubTaskActions.Add(entity);
        }
        else
        {
            entity.TicketId = request.TicketId;
            entity.SubTaskId = request.SubTaskId;
            entity.CmpId = request.CmpId;
            entity.Seq = request.Seq;
            entity.ActionDate = request.ActionDate.Date;
            entity.ActionDetails = request.ActionDetails;
            entity.ActionStatus = request.ActionStatus;
            entity.Tomorrow = request.Tomorrow;
            entity.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        var result = new ServiceTicketSubTaskActionDto
        {
            TaskActionId = entity.TaskActionId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            Seq = entity.Seq,
            ActionDate = entity.ActionDate,
            ActionDetails = entity.ActionDetails,
            ActionStatus = entity.ActionStatus,
            Tomorrow = entity.Tomorrow,
            UpdatedAt = entity.UpdatedAt
        };

        return Ok(result);
    }
    [HttpPut("subtask/actions/{taskActionId}")]
    public async Task<ActionResult<ServiceTicketSubTaskActionDto>> ActionsUpdate(
        string taskActionId,
        [FromBody] UpdateServiceTicketSubTaskActionDto request)
    {
        var entity = await _context.ServiceTicketSubTaskActions
            .FirstOrDefaultAsync(x => x.TaskActionId == taskActionId);

        if (entity == null)
            return NotFound(new { message = "Task action not found" });

        entity.TicketId = request.TicketId;
        entity.SubTaskId = request.SubTaskId;
        entity.CmpId = request.CmpId;
        entity.Seq = request.Seq;
        entity.ActionDate = request.ActionDate.Date;
        entity.ActionDetails = request.ActionDetails;
        entity.ActionStatus = request.ActionStatus;
        entity.Tomorrow = request.Tomorrow;
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new ServiceTicketSubTaskActionDto
        {
            TaskActionId = entity.TaskActionId,
            TicketId = entity.TicketId,
            SubTaskId = entity.SubTaskId,
            CmpId = entity.CmpId,
            Seq = entity.Seq,
            ActionDate = entity.ActionDate,
            ActionDetails = entity.ActionDetails,
            ActionStatus = entity.ActionStatus,
            Tomorrow = entity.Tomorrow,
            UpdatedAt = entity.UpdatedAt
        });
    }

    [HttpDelete("subtask/actions/{taskActionId}")]
    public async Task<IActionResult> ActionsDelete(string taskActionId)
    {
        var entity = await _context.ServiceTicketSubTaskActions
            .FirstOrDefaultAsync(x => x.TaskActionId == taskActionId);

        if (entity == null)
            return NotFound(new { message = "Task action not found" });

        _context.ServiceTicketSubTaskActions.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }




    [HttpGet("subtask/actions/attachment")]
    public async Task<ActionResult<IEnumerable<ServiceTicketSubTaskActionAttachmentDto>>> ActionAttachGetAll(
                [FromQuery] string? taskActionId)
    {
        var query = _context.ServiceTicketSubTaskActionAttachments
            .AsNoTracking()
            .AsQueryable();

        if (taskActionId != "")
        {
            query = query.Where(x => x.TaskActionId == taskActionId);
        }

        var result = await query
            .OrderBy(x => x.TaskActionId)
            .ThenBy(x => x.Seq)
            .Select(x => new ServiceTicketSubTaskActionAttachmentDto
            {
                AttachmentId = x.AttachmentId,
                TaskActionId = x.TaskActionId,
                Seq = x.Seq,
                FileName = x.FileName,
                FilePath = x.FilePath,
                FileExt = x.FileExt,
                FileSize = x.FileSize,
                ContentType = x.ContentType,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("subtask/actions/attachment/{taskActionId}/{attachmentId}")]
    public async Task<ActionResult<ServiceTicketSubTaskActionAttachmentDto>> GetById(
        string taskActionId,
        Guid attachmentId)
    {
        var entity = await _context.ServiceTicketSubTaskActionAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TaskActionId == taskActionId &&
                x.AttachmentId == attachmentId);

        if (entity == null)
        {
            return NotFound(new { message = "Attachment not found" });
        }

        return Ok(new ServiceTicketSubTaskActionAttachmentDto
        {
            AttachmentId = entity.AttachmentId,
            TaskActionId = entity.TaskActionId,
            Seq = entity.Seq,
            FileName = entity.FileName,
            FilePath = entity.FilePath,
            FileExt = entity.FileExt,
            FileSize = entity.FileSize,
            ContentType = entity.ContentType,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        });
    }

    [HttpPost("subtask/actions/attachment")]
    public async Task<ActionResult<ServiceTicketSubTaskActionAttachmentDto>> Create(
        [FromBody] CreateServiceTicketSubTaskActionAttachmentDto request)
    {
        var entity = new ServiceTicketSubTaskActionAttachment
        {
            AttachmentId = Guid.NewGuid(),
            TaskActionId = request.TaskActionId,
            Seq = request.Seq,
            FileName = request.FileName,
            FilePath = request.FilePath,
            FileExt = request.FileExt,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            CreatedAt = DateTime.Now,
            CreatedBy = request.CreatedBy
        };

        _context.ServiceTicketSubTaskActionAttachments.Add(entity);
        await _context.SaveChangesAsync();

        var result = new ServiceTicketSubTaskActionAttachmentDto
        {
            AttachmentId = entity.AttachmentId,
            TaskActionId = entity.TaskActionId,
            Seq = entity.Seq,
            FileName = entity.FileName,
            FilePath = entity.FilePath,
            FileExt = entity.FileExt,
            FileSize = entity.FileSize,
            ContentType = entity.ContentType,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };

        return CreatedAtAction(
            nameof(GetById),
            new { taskActionId = entity.TaskActionId, attachmentId = entity.AttachmentId },
            result
        );
    }

    [HttpPut("subtask/actions/attachment/{taskActionId:guid}/{attachmentId}")]
    public async Task<ActionResult<ServiceTicketSubTaskActionAttachmentDto>> Update(
        string taskActionId,
        Guid attachmentId,
        [FromBody] UpdateServiceTicketSubTaskActionAttachmentDto request)
    {
        var entity = await _context.ServiceTicketSubTaskActionAttachments
            .FirstOrDefaultAsync(x =>
                x.TaskActionId == taskActionId &&
                x.AttachmentId == attachmentId);

        if (entity == null)
        {
            return NotFound(new { message = "Attachment not found" });
        }

        entity.Seq = request.Seq;
        entity.FileName = request.FileName;
        entity.FilePath = request.FilePath;
        entity.FileExt = request.FileExt;
        entity.FileSize = request.FileSize;
        entity.ContentType = request.ContentType;
        entity.CreatedBy = request.CreatedBy;

        await _context.SaveChangesAsync();

        return Ok(new ServiceTicketSubTaskActionAttachmentDto
        {
            AttachmentId = entity.AttachmentId,
            TaskActionId = entity.TaskActionId,
            Seq = entity.Seq,
            FileName = entity.FileName,
            FilePath = entity.FilePath,
            FileExt = entity.FileExt,
            FileSize = entity.FileSize,
            ContentType = entity.ContentType,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        });
    }

    [HttpDelete("subtask/actions/attachment/{taskActionId}/{attachmentId:guid}")]
    public async Task<IActionResult> Delete(
        string taskActionId,
        Guid attachmentId)
    {
        var entity = await _context.ServiceTicketSubTaskActionAttachments
            .FirstOrDefaultAsync(x =>
                x.TaskActionId == taskActionId &&
                x.AttachmentId == attachmentId);

        if (entity == null)
        {
            return NotFound(new { message = "Attachment not found" });
        }

        _context.ServiceTicketSubTaskActionAttachments.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }


    // end sub task action 


    // subtask approve 
    [HttpPut("subtask/sendapprove")]
    public async Task<ActionResult> SendApprove([FromBody] SubTaskSendApproveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SubTaskId))
        {
            return BadRequest(new { message = "SubTaskId is required" });
        }

        var entity = await _context.ServiceTicketSubTasks
            .FirstOrDefaultAsync(x => x.SubTaskId == dto.SubTaskId);

        if (entity == null)
        {
            return NotFound(new { message = "SubTask not found" });
        }

        entity.StateSendApprove = "1";
        entity.DateSendApprove = DateTime.Now;
        entity.SendApproveBy = dto.SendApproveBy;
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Send approve successfully",
            entity.SubTaskId,
            entity.StateSendApprove,
            entity.DateSendApprove,
            entity.SendApproveBy
        });
    }


    [HttpPut("subtask/approve")]
    public async Task<ActionResult> Approve([FromBody] SubTaskApproveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SubTaskId))
        {
            return BadRequest(new { message = "SubTaskId is required" });
        }

        var entity = await _context.ServiceTicketSubTasks
            .FirstOrDefaultAsync(x => x.SubTaskId == dto.SubTaskId);

        if (entity == null)
        {
            return NotFound(new { message = "SubTask not found" });
        }

        entity.StateApprove = "1";
        entity.DateApprove = DateTime.Now;
        entity.ApproveBy = dto.ApproveBy;
        entity.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Approve successfully",
            entity.SubTaskId,
            entity.StateApprove,
            entity.DateApprove,
            entity.ApproveBy
        });
    }


    // end subtask approve 




    private static ServiceTicketResponseDto MapToResponse(ServiceTicket x)
    {
        return new ServiceTicketResponseDto
        {
            TicketId = x.TicketId,
            TicketNo = x.TicketNo,
            CustomerName = x.CustomerName,
            CustomerCode = x.CustomerCode,
            ImagePath = x.Customer?.ImgPath,
            JobType = x.JobType,
            JobGroups = x.JobGroups.Select(g => g.JobGroup).ToList(),
            AdditionalDetails = x.AdditionalDetails,
            Priority = x.Priority,
            ServiceDate = x.ServiceDate,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CmpId = x.CmpId,
            UpdUser = x.UpdUser,
            ProjectNo = x.ProjectNo,

            Procedures = new ServiceTicketProceduresDto
            {
                SiteSurvey = x.ProcedureSiteSurvey,
                PreConfig = x.ProcedurePreConfig,
                InstallConfig = x.ProcedureInstallConfig,
                UAT = x.ProcedureUAT,
                Handover = x.ProcedureHandover
            },

            Maintenances = new ServiceTicketMaintenancesDto
            {
                OnsiteService = x.MaintenanceOnsiteService,
                PMService = x.MaintenancePMService,
                SLAServiceLicense = x.MaintenanceSLAServiceLicense,
                ServiceReplacement = x.MaintenanceServiceReplacement,
                RemoteBackupConfig = x.MaintenanceRemoteBackupConfig,
                Report = x.MaintenanceReport
            },

            MaintenanceOptions = new ServiceTicketMaintenanceOptionsDto
            {
                OnsiteServiceCycle = x.OnsiteServiceCycle,
                PmServiceCycle = x.PMServiceCycle,
                SlaType = x.SLAType,
                ReplacementType = x.ReplacementType,
                RemoteBackupCycle = x.RemoteBackupCycle,
                ReportCycle = x.ReportCycle
            },

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
                .ToList(),

            SubTasks = x.SubTasks
            .OrderBy(x => x.Seq)
            .Select(x => new ServiceTicketSubTaskDto
            {
                SubTaskId = x.SubTaskId,
                Seq = x.Seq,
                Title = x.Title,
                Source = x.Source,
                IsDone = x.IsDone,
                DoneAt = x.DoneAt,
                DoneBy = x.DoneBy,
                Status = x.Status,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CmpId = x.CmpId,
                Remark = x.Remark,
                StateSendApprove = x.StateSendApprove,
                SendApproveBy = x.SendApproveBy,
                StateApprove = x.StateApprove,
                ApproveBy = x.ApproveBy,
                Assignments = x.Assignments
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.AssignUserName)
                    .Select(a => new ServiceTicketSubTaskAssignResponseDto
                    {
                        AssignId = a.AssignId,
                        SubTaskId = a.SubTaskId,
                        TicketId = a.TicketId,
                        AssignUserId = a.AssignUserId,
                        AssignUserName = a.AssignUserName,
                        RoleName = a.RoleName,
                        IsActive = a.IsActive,
                        AssignedAt = a.AssignedAt,
                        AssignedBy = a.AssignedBy
                    }).ToList(),

                AttachFiles = x.AttachFiles
                    .OrderBy(f => f.Seq)
                    .Select(f => new ProcedureTaskItemFileDto
                    {
                        UpdUser = f.UpdUser,
                        SubTaskId = f.SubTaskId.ToString(),
                        Seq = f.Seq,
                        FileName = f.FileName,
                        FilePath = f.FilePath,
                        CmpId = f.CmpId
                    })
                    .ToList()



            })
            .ToList()


        };
    }
}