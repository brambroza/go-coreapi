using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Dtos;
using goalongapi.Models;
using goalongapi.Data;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/hr/{cmpId}/attendance-raw-logs")]
public class AttendanceRawLogsController : ControllerBase
{
    private readonly HrDbContext _db; // เปลี่ยนเป็น DbContext ของคุณจริง
    public AttendanceRawLogsController(HrDbContext db) => _db = db;

    // ---------- GET: list for frontend ----------
    // filters: deviceId, from, to, status, userCode, cardNo, batchId
    [HttpGet]
    public async Task<IActionResult> List(
        string cmpId,
        [FromQuery] int? deviceId,
        [FromQuery] DateTime? from, // ใช้ datetime เพื่อ filter ช่วงเวลา
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] string? userCode,
        [FromQuery] string? cardNo,
        [FromQuery] Guid? batchId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 200);

        var q = _db.AttendanceRawLogs.AsNoTracking()
            .Where(x => x.CmpId == cmpId);

        if (deviceId.HasValue) q = q.Where(x => x.DeviceId == deviceId.Value);
        if (from.HasValue) q = q.Where(x => x.DeviceLogTimeLocal >= from.Value);
        if (to.HasValue) q = q.Where(x => x.DeviceLogTimeLocal <= to.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.IngestStatus == status);
        if (!string.IsNullOrWhiteSpace(userCode)) q = q.Where(x => x.UserCodeOnDevice!.Contains(userCode));
        if (!string.IsNullOrWhiteSpace(cardNo)) q = q.Where(x => x.CardNo!.Contains(cardNo));
        if (batchId.HasValue) q = q.Where(x => x.SyncBatchId == batchId.Value);

        var total = await q.CountAsync();

        var data = await q
            .OrderByDescending(x => x.DeviceLogTimeLocal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.RawLogId,
                x.DeviceId,
                DeviceName = x.Device != null ? x.Device.Name : null,
                x.DeviceUserId,
                DeviceUserName = x.DeviceUser != null ? x.DeviceUser.DisplayName : null,

                x.UserCodeOnDevice,
                x.CardNo,

                x.DeviceLogTimeLocal,
                x.PunchTimeUtc,

                x.VerifyMode,
                x.InOutState,
                x.WorkCode,

                x.Source,
                x.SyncBatchId,
                x.ReceivedAt,
                x.IngestStatus,
                x.IngestError,

                UniqueHashBase64 = Convert.ToBase64String(x.UniqueHash)
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items = data });
    }

    // ---------- GET by id ----------
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(string cmpId, long id)
    {
        var x = await _db.AttendanceRawLogs.AsNoTracking()
            .Where(r => r.CmpId == cmpId && r.RawLogId == id)
            .Select(r => new
            {
                r.RawLogId,
                r.CmpId,
                r.DeviceId,
                DeviceName = r.Device != null ? r.Device.Name : null,
                r.DeviceUserId,
                DeviceUserName = r.DeviceUser != null ? r.DeviceUser.DisplayName : null,

                r.UserCodeOnDevice,
                r.CardNo,
                r.DeviceLogTimeLocal,
                r.DeviceTimezone,
                r.DeviceLogId,

                r.PunchTimeUtc,
                r.TimezoneUsed,
                r.DeviceClockDriftSec,

                r.VerifyMode,
                r.InOutState,
                r.WorkCode,

                r.RawPayloadJson,
                r.Source,
                r.SyncBatchId,
                r.ReceivedAt,
                r.IngestStatus,
                r.IngestError,

                UniqueHashBase64 = Convert.ToBase64String(r.UniqueHash)
            })
            .FirstOrDefaultAsync();

        return x is null ? NotFound() : Ok(x);
    }

    // ---------- POST: create single (manual / test) ----------
    [HttpPost]
    public async Task<IActionResult> Create(string cmpId, [FromBody] AttendanceRawLogCreateDto dto)
    {
        if (dto.CmpId != cmpId) return BadRequest("cmpId mismatch");

        // Validate device belongs to cmpId (จาก devicescan ก่อนหน้า)
        var deviceOk = await _db.DevicesScan.AnyAsync(d => d.CmpId == cmpId && d.DeviceId == dto.DeviceId);
        if (!deviceOk) return BadRequest("DeviceId not found for this cmpId");

        var hash = ComputeUniqueHash(dto);

        // กันซ้ำแบบ application-level (ถ้า DB ยังไม่ unique)
        var exists = await _db.AttendanceRawLogs.AnyAsync(x => x.CmpId == cmpId && x.UniqueHash == hash);
        if (exists) return Conflict("Duplicate raw log (UniqueHash).");

        var entity = MapCreateDtoToEntity(cmpId, dto, hash);

        _db.AttendanceRawLogs.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { cmpId, id = entity.RawLogId }, new { entity.RawLogId });
    }

    // ---------- POST: bulk ingest (เหมาะกับ job ดึงจากเครื่อง) ----------
    [HttpPost("ingest")]
    public async Task<IActionResult> IngestBulk(string cmpId, [FromBody] AttendanceRawLogIngestBulkDto bulk)
    {
        if (bulk.CmpId != cmpId) return BadRequest("cmpId mismatch");
        if (bulk.Items == null || bulk.Items.Count == 0) return Ok(new { inserted = 0, skipped = 0 });

        // device validation (รวมทุก device ที่ส่งมา)
        var deviceIds = bulk.Items.Select(x => x.DeviceId).Distinct().ToList();
        var validDeviceIds = await _db.DevicesScan
            .Where(d => d.CmpId == cmpId && deviceIds.Contains(d.DeviceId))
            .Select(d => d.DeviceId)
            .ToListAsync();

        var validSet = validDeviceIds.ToHashSet();
        var inserted = 0;
        var skipped = 0;

        foreach (var dto in bulk.Items)
        {
            if (dto.CmpId != cmpId || !validSet.Contains(dto.DeviceId))
            {
                skipped++;
                continue;
            }

            var hash = ComputeUniqueHash(dto);

            // เช็คซ้ำ (best effort)
            var dup = await _db.AttendanceRawLogs.AnyAsync(x => x.CmpId == cmpId && x.UniqueHash == hash);
            if (dup)
            {
                skipped++;
                continue;
            }

            var entity = MapCreateDtoToEntity(cmpId, dto, hash);
            entity.SyncBatchId = bulk.SyncBatchId ?? dto.SyncBatchId;

            _db.AttendanceRawLogs.Add(entity);
            inserted++;

            // กัน memory โตเกิน
            if (inserted % 200 == 0)
                await _db.SaveChangesAsync();
        }

        await _db.SaveChangesAsync();

        return Ok(new { inserted, skipped, batchId = bulk.SyncBatchId });
    }

    // ---------- PUT: update ----------
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(string cmpId, long id, [FromBody] AttendanceRawLogUpdateDto dto)
    {
        var entity = await _db.AttendanceRawLogs
            .FirstOrDefaultAsync(x => x.CmpId == cmpId && x.RawLogId == id);

        if (entity is null) return NotFound();

        // validate device belongs to cmpId
        var deviceOk = await _db.DevicesScan.AnyAsync(d => d.CmpId == cmpId && d.DeviceId == dto.DeviceId);
        if (!deviceOk) return BadRequest("DeviceId not found for this cmpId");

        // ปรับ field
        entity.DeviceId = dto.DeviceId;
        entity.DeviceUserId = dto.DeviceUserId;
        entity.UserCodeOnDevice = dto.UserCodeOnDevice;
        entity.CardNo = dto.CardNo;
        entity.DeviceLogTimeLocal = dto.DeviceLogTimeLocal;
        entity.DeviceTimezone = dto.DeviceTimezone;
        entity.DeviceLogId = dto.DeviceLogId;
        entity.PunchTimeUtc = dto.PunchTimeUtc;
        entity.TimezoneUsed = dto.TimezoneUsed;
        entity.DeviceClockDriftSec = dto.DeviceClockDriftSec;
        entity.VerifyMode = dto.VerifyMode;
        entity.InOutState = dto.InOutState;
        entity.WorkCode = dto.WorkCode;
        entity.RawPayloadJson = dto.RawPayloadJson;
        entity.Source = string.IsNullOrWhiteSpace(dto.Source) ? entity.Source : dto.Source.Trim();
        entity.SyncBatchId = dto.SyncBatchId;
        entity.IngestStatus = string.IsNullOrWhiteSpace(dto.IngestStatus) ? entity.IngestStatus : dto.IngestStatus.Trim();
        entity.IngestError = dto.IngestError;

        // recompute hash (เพราะข้อมูลที่ประกอบ hash เปลี่ยนได้)
        entity.UniqueHash = ComputeUniqueHash(new AttendanceRawLogCreateDto(
            cmpId,
            dto.DeviceId,
            dto.DeviceUserId,
            dto.UserCodeOnDevice,
            dto.CardNo,
            dto.DeviceLogTimeLocal,
            dto.DeviceTimezone,
            dto.DeviceLogId,
            dto.PunchTimeUtc,
            dto.TimezoneUsed,
            dto.DeviceClockDriftSec,
            dto.VerifyMode,
            dto.InOutState,
            dto.WorkCode,
            dto.RawPayloadJson,
            dto.Source,
            dto.SyncBatchId,
            dto.IngestStatus,
            dto.IngestError
        ));

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------- DELETE ----------
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(string cmpId, long id)
    {
        var entity = await _db.AttendanceRawLogs
            .FirstOrDefaultAsync(x => x.CmpId == cmpId && x.RawLogId == id);

        if (entity is null) return NotFound();

        _db.AttendanceRawLogs.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------- helpers ----------------

    private static AttendanceRawLog MapCreateDtoToEntity(string cmpId, AttendanceRawLogCreateDto dto, byte[] hash)
    {
        return new AttendanceRawLog
        {
            CmpId = cmpId,
            DeviceId = dto.DeviceId,
            DeviceUserId = dto.DeviceUserId,
            UserCodeOnDevice = dto.UserCodeOnDevice,
            CardNo = dto.CardNo,
            DeviceLogTimeLocal = dto.DeviceLogTimeLocal,
            DeviceTimezone = dto.DeviceTimezone,
            DeviceLogId = dto.DeviceLogId,
            PunchTimeUtc = dto.PunchTimeUtc,
            TimezoneUsed = dto.TimezoneUsed,
            DeviceClockDriftSec = dto.DeviceClockDriftSec,
            VerifyMode = dto.VerifyMode,
            InOutState = dto.InOutState,
            WorkCode = dto.WorkCode,
            RawPayloadJson = dto.RawPayloadJson,
            Source = string.IsNullOrWhiteSpace(dto.Source) ? "ZKTeco" : dto.Source.Trim(),
            SyncBatchId = dto.SyncBatchId,
            IngestStatus = string.IsNullOrWhiteSpace(dto.IngestStatus) ? "New" : dto.IngestStatus.Trim(),
            IngestError = dto.IngestError,
            UniqueHash = hash
            // ReceivedAt: ปล่อยให้ default ใน DB ทำงาน (sysutcdatetime)
        };
    }

    // SHA256 => 32 bytes
    private static byte[] ComputeUniqueHash(AttendanceRawLogCreateDto dto)
    {
        var s = string.Join("|", new[]
        {
            dto.CmpId,
            dto.DeviceId.ToString(),
            dto.DeviceLogId ?? "",
            dto.UserCodeOnDevice ?? "",
            dto.CardNo ?? "",
            dto.DeviceLogTimeLocal.ToString("yyyy-MM-dd HH:mm:ss"),
            dto.InOutState ?? "",
            dto.VerifyMode ?? "",
            dto.WorkCode ?? ""
        });

        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(s));
    }
}
