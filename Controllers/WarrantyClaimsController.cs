using goalongapi.Data;
using goalongapi.Dtos;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// GET  /api/WarrantyClaims                    — รายการใบเคลมทั้งหมด (filter by cmpId)
// POST /api/WarrantyClaims                    — สร้างใบเคลมใหม่ (auto-gen CLM-YYYY-NNNN)
// PATCH /api/WarrantyClaims/{id}/status       — อัพเดตสถานะใบเคลม
// GET  /api/WarrantyClaims/notifications      — notification queue (derived from claims)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class WarrantyClaimsController : ControllerBase
{
    private readonly DatabaseContext _context;

    public WarrantyClaimsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarrantyClaimDto>>> GetAll(
        [FromQuery] string? cmpId,
        [FromQuery] string? ticketId)
    {
        var query = _context.WarrantyClaims.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        if (!string.IsNullOrWhiteSpace(ticketId))
            query = query.Where(x => x.TicketId == ticketId);

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult<WarrantyClaimDto>> Create(
        [FromBody] WarrantyClaimCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Brand) ||
            string.IsNullOrWhiteSpace(dto.Model) ||
            string.IsNullOrWhiteSpace(dto.Sn) ||
            string.IsNullOrWhiteSpace(dto.ReporterStaff))
        {
            return BadRequest("Brand, Model, Sn และ ReporterStaff จำเป็นต้องระบุ");
        }

        // Auto-generate CLM-YYYY-NNNN
        var year = DateTime.Now.Year;
        var prefix = $"CLM-{year}-";
        var lastId = await _context.WarrantyClaims
            .Where(x => x.Id.StartsWith(prefix))
            .OrderByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        int seq = 1;
        if (lastId is not null &&
            int.TryParse(lastId[prefix.Length..], out int last))
            seq = last + 1;

        var claim = new WarrantyClaim
        {
            Id = $"{prefix}{seq:D4}",
            TicketId = dto.TicketId,
            Customer = dto.Customer,
            SalesName = dto.SalesName,
            ReporterStaff = dto.ReporterStaff,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNo = dto.Sn,
            WarrantyStatus = dto.WarrantyStatus,
            Detail = dto.Detail,
            CmpId = dto.CmpId,
            UpdUser = dto.UpdUser,
            ClaimDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.WarrantyClaims.Add(claim);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(claim));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<WarrantyClaimDto>> UpdateStatus(
        string id,
        [FromBody] WarrantyClaimUpdateStatusDto dto)
    {
        var claim = await _context.WarrantyClaims.FindAsync(id);
        if (claim is null) return NotFound();

        claim.Status = dto.Status;
        claim.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(MapToDto(claim));
    }

    /// <summary>ส่งคืน 20 claims ล่าสุดในรูปแบบ notification queue สำหรับ Sales team</summary>
    [HttpGet("notifications")]
    public async Task<ActionResult<IEnumerable<WarrantyClaimNotificationDto>>> GetNotifications(
        [FromQuery] string? cmpId)
    {
        var query = _context.WarrantyClaims.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cmpId))
            query = query.Where(x => x.CmpId == cmpId);

        var claims = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync();

        var result = claims.Select((c, i) => new WarrantyClaimNotificationDto
        {
            Id = i + 1,
            SalesName = c.SalesName ?? "-",
            Customer = c.Customer,
            Text = $"ตั๋วเคลม {c.Id} สำหรับ {c.Brand} {c.Model} (S/N: {c.SerialNo}) " +
                   $"— {(c.WarrantyStatus == "on" ? "อยู่ในประกัน" : "หมดประกัน")}",
            Time = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            IsRead = c.Status != "Claim Received",
        });

        return Ok(result);
    }

    private static WarrantyClaimDto MapToDto(WarrantyClaim c) => new()
    {
        Id = c.Id,
        TicketId = c.TicketId,
        Customer = c.Customer,
        SalesName = c.SalesName,
        ReporterStaff = c.ReporterStaff,
        Brand = c.Brand,
        Model = c.Model,
        Sn = c.SerialNo,
        WarrantyStatus = c.WarrantyStatus,
        Status = c.Status,
        Detail = c.Detail,
        Date = c.ClaimDate.ToString("yyyy-MM-dd"),
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// GET /api/WarrantyDevices/lookup?sn={serialNo} — ตรวจสอบรับประกันจาก S/N
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class WarrantyDevicesController : ControllerBase
{
    private readonly DatabaseContext _context;

    public WarrantyDevicesController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<WarrantyDeviceLookupDto>> Lookup([FromQuery] string sn)
    {
        if (string.IsNullOrWhiteSpace(sn))
            return BadRequest("sn is required");

        var device = await _context.WarrantyDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SerialNo.ToLower() == sn.ToLower().Trim());

        if (device is null)
            return NotFound();

        var status = device.WarrantyExpiry.HasValue && device.WarrantyExpiry < DateTime.UtcNow
            ? "Expired"
            : "Active";

        return Ok(new WarrantyDeviceLookupDto
        {
            SerialNo = device.SerialNo,
            Name = device.ProductName,
            Brand = device.Brand,
            Model = device.Model,
            Customer = device.Customer,
            WarrantyStatus = device.WarrantyStatus,
            Status = status,
            ExpiryDate = device.WarrantyExpiry?.ToString("yyyy-MM-dd"),
        });
    }
}
