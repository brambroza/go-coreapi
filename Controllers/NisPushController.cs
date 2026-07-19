using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

/// <summary>
/// NIS Onsite push token registry (Track B MVP) — แอปช่างลงทะเบียน Expo Push token
/// ตอน login/app start และถอนตอน logout
/// Route prefix: api/nis/push
/// Auth: ไม่มี [Authorize] — pattern เดียวกับ NisController/ServiceTicketsController
/// </summary>
[ApiController]
[Route("api/nis/push")]
public class NisPushController : ControllerBase
{
    private readonly DatabaseContext _context;

    public NisPushController(DatabaseContext context)
    {
        _context = context;
    }

    // ── POST api/nis/push/register-token ─────────────────────────────────────

    /// <summary>
    /// Upsert token ต่อ (CmpId, StaffName, DeviceId) — เรียกทุกครั้งหลัง login สำเร็จ
    /// (Expo token เปลี่ยนได้หลัง reinstall/OS update จึงต้อง upsert ไม่ใช่ insert)
    /// เครื่องเดิมที่เปลี่ยนคน login → ลบ token ของคนเก่าบนเครื่องนั้นทิ้ง
    /// กันงานของคนเก่าเด้งเข้าเครื่องที่เปลี่ยนมือ
    /// </summary>
    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken([FromBody] NisPushRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId)
            || string.IsNullOrWhiteSpace(dto.StaffName)
            || string.IsNullOrWhiteSpace(dto.ExpoPushToken)
            || string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest(new { message = "CmpId, StaffName, ExpoPushToken, DeviceId are required" });

        // ลบ token ของ staff คนอื่นบนเครื่องเดียวกัน (เครื่องเปลี่ยนมือ)
        var otherStaffRows = await _context.NisPushTokens
            .Where(t => t.CmpId == dto.CmpId && t.DeviceId == dto.DeviceId && t.StaffName != dto.StaffName)
            .ToListAsync();
        if (otherStaffRows.Count > 0)
            _context.NisPushTokens.RemoveRange(otherStaffRows);

        var existing = await _context.NisPushTokens.FirstOrDefaultAsync(t =>
            t.CmpId == dto.CmpId && t.StaffName == dto.StaffName && t.DeviceId == dto.DeviceId);

        if (existing == null)
        {
            _context.NisPushTokens.Add(new NisPushToken
            {
                CmpId = dto.CmpId,
                StaffName = dto.StaffName,
                UserId = dto.UserId,
                ExpoPushToken = dto.ExpoPushToken,
                DeviceId = dto.DeviceId,
                Platform = dto.Platform,
                AppVersion = dto.AppVersion,
            });
        }
        else
        {
            existing.ExpoPushToken = dto.ExpoPushToken;
            existing.UserId = dto.UserId;
            existing.Platform = dto.Platform;
            existing.AppVersion = dto.AppVersion;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Token registered" });
    }

    // ── POST api/nis/push/unregister ─────────────────────────────────────────

    /// <summary>ถอนทุก token ของเครื่องนี้ (เรียกตอน logout) — idempotent</summary>
    [HttpPost("unregister")]
    public async Task<IActionResult> Unregister([FromBody] NisPushUnregisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CmpId) || string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest(new { message = "CmpId and DeviceId are required" });

        var rows = await _context.NisPushTokens
            .Where(t => t.CmpId == dto.CmpId && t.DeviceId == dto.DeviceId)
            .ToListAsync();

        if (rows.Count > 0)
        {
            _context.NisPushTokens.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Unregistered", removed = rows.Count });
    }
}
