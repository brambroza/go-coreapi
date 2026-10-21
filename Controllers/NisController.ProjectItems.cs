using System.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

// ── api/nis/projects/{id}/items (dbo.NisProjectItem) ─────────────────────────
// รายการสินค้าขายของโครงการ ดึงจาก Sale Order ใน NisProject.SoRef (SP getSaleOrderItem_All)
// งาน Implement: ตั๋ว Install ได้ projectItems ใน GET onsite/tickets/{id} → ช่าง "นำของออก" จากคลัง 1
// สินค้าสำหรับขาย ได้ไม่เกิน Qty — ยอดนำออกตรวจที่นี่ (409) ก่อน client สร้างใบเบิก WMS
// แยกไฟล์ (partial) เพื่อไม่ให้ NisController.cs โตเพิ่ม

public partial class NisController
{
    private static NisProjectItemDto MapProjectItem(NisProjectItem i) => new()
    {
        Id = i.Id,
        SoNo = i.SoNo,
        ProdCode = i.ProdCode,
        ProdName = i.ProdName,
        UnitCode = i.UnitCode,
        Qty = i.Qty,
        QtyIssued = i.QtyIssued,
        QtyRemaining = Math.Max(0, i.Qty - i.QtyIssued),
    };

    /// เลข SO ใน SoRef (คั่นด้วย , ; หรือขึ้นบรรทัด) — ตัดช่องว่าง/ซ้ำ
    private static List<string> SplitSoRef(string? soRef) =>
        (soRef ?? string.Empty)
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// แถวสินค้าของ SO ที่ต้องการ (revision ล่าสุดของแต่ละ SO · รวมบรรทัดสินค้าซ้ำ)
    private sealed record SoItemRow(string SoNo, string ProdCode, string ProdName, string UnitCode, decimal Qty);

    /// อ่านรายการ SO ผ่าน SP getSaleOrderItem_All (parameterized) แล้วกรองเฉพาะ SO ใน soNos
    private async Task<List<SoItemRow>> ReadSaleOrderItemsAsync(string cmpId, string username, List<string> soNos)
    {
        var raw = new List<(string SoNo, int RevNo, string ProdCode, string ProdName, string UnitCode, decimal Qty)>();
        var wanted = new HashSet<string>(soNos, StringComparer.OrdinalIgnoreCase);

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.getSaleOrderItem_All";
            command.CommandType = CommandType.StoredProcedure;
            var pCmp = command.CreateParameter();
            pCmp.ParameterName = "@CmpId";
            pCmp.Value = cmpId;
            command.Parameters.Add(pCmp);
            var pUser = command.CreateParameter();
            pUser.ParameterName = "@user";
            pUser.Value = username;
            command.Parameters.Add(pUser);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var soNo = Convert.ToString(reader["SaleOrderNo"])?.Trim() ?? string.Empty;
                if (!wanted.Contains(soNo)) continue;
                var prodCode = Convert.ToString(reader["ProdCode"])?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(prodCode)) continue;
                raw.Add((
                    soNo,
                    reader["RevNo"] is DBNull ? 0 : Convert.ToInt32(reader["RevNo"]),
                    prodCode,
                    Convert.ToString(reader["ProdDescription"])?.Trim() ?? string.Empty,
                    Convert.ToString(reader["UnitCode"])?.Trim() ?? string.Empty,
                    reader["Qty"] is DBNull ? 0 : Convert.ToDecimal(reader["Qty"])));
            }
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }

        return raw
            .GroupBy(r => r.SoNo, StringComparer.OrdinalIgnoreCase)
            .SelectMany(so =>
            {
                var lastRev = so.Max(r => r.RevNo);
                return so.Where(r => r.RevNo == lastRev)
                    .GroupBy(r => r.ProdCode, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new SoItemRow(
                        so.Key,
                        g.Key,
                        g.First().ProdName,
                        g.First().UnitCode,
                        g.Sum(r => r.Qty)));
            })
            .Where(r => r.Qty > 0)
            .ToList();
    }

    /// <summary>
    /// upsert รายการโครงการจาก SO — เพิ่มบรรทัดใหม่ · อัปเดตจำนวน/ชื่อ (ไม่ลด Qty ต่ำกว่า QtyIssued) ·
    /// บรรทัดที่หายจาก SO ลบเฉพาะที่ยังไม่นำออก (QtyIssued = 0) · ไม่ SaveChanges เอง
    /// </summary>
    private async Task<NisProjectItemSyncResultDto> UpsertProjectItemsFromSoAsync(NisProject project, string username)
    {
        var cmpId = project.CmpId ?? string.Empty;
        var soNos = SplitSoRef(project.SoRef);
        var rows = soNos.Count == 0 ? new List<SoItemRow>() : await ReadSaleOrderItemsAsync(cmpId, username, soNos);

        var existing = await _context.NisProjectItems
            .Where(i => i.ProjectId == project.ProjectId)
            .ToListAsync();
        var now = BangkokNow();

        foreach (var r in rows)
        {
            var hit = existing.FirstOrDefault(i =>
                string.Equals(i.SoNo, r.SoNo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(i.ProdCode, r.ProdCode, StringComparison.OrdinalIgnoreCase));
            if (hit == null)
            {
                var item = new NisProjectItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProjectId = project.ProjectId,
                    CmpId = cmpId,
                    SoNo = r.SoNo,
                    ProdCode = r.ProdCode,
                    ProdName = r.ProdName,
                    UnitCode = r.UnitCode,
                    Qty = r.Qty,
                    QtyIssued = 0,
                    CreatedDate = now,
                    UpdatedDate = now,
                };
                _context.NisProjectItems.Add(item);
                existing.Add(item);
            }
            else
            {
                hit.ProdName = r.ProdName;
                hit.UnitCode = r.UnitCode;
                hit.Qty = Math.Max(r.Qty, hit.QtyIssued);
                hit.UpdatedDate = now;
            }
        }

        var keep = rows.Select(r => $"{r.SoNo}|{r.ProdCode}".ToUpperInvariant()).ToHashSet();
        var stale = existing
            .Where(i => i.QtyIssued == 0 && !keep.Contains($"{i.SoNo}|{i.ProdCode}".ToUpperInvariant()))
            .ToList();
        _context.NisProjectItems.RemoveRange(stale);

        var foundSo = rows.Select(r => r.SoNo).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return new NisProjectItemSyncResultDto
        {
            Items = existing.Except(stale)
                .OrderBy(i => i.SoNo).ThenBy(i => i.ProdCode)
                .Select(MapProjectItem)
                .ToList(),
            MissingSoNos = soNos.Where(s => !foundSo.Contains(s)).ToList(),
        };
    }

    /// รายการสินค้าโครงการสำหรับหน้า onsite — เฉพาะตั๋ว Install · ไม่มีรายการ = null · ตารางยังไม่มี = null
    private async Task<List<NisProjectItemDto>?> LoadOnsiteProjectItemsAsync(string? projectId, string onsiteTicketType)
    {
        if (onsiteTicketType != "Install" || string.IsNullOrWhiteSpace(projectId)) return null;
        try
        {
            var items = await _context.NisProjectItems
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId)
                .OrderBy(i => i.SoNo).ThenBy(i => i.ProdCode)
                .ToListAsync();
            return items.Count == 0 ? null : items.Select(MapProjectItem).ToList();
        }
        catch (Exception ex)
        {
            // migration 20261022_AddNisProjectItem ยังไม่รัน — หน้า onsite ต้องเปิดได้ตามเดิม
            _logger.LogWarning(ex, "NIS project items: อ่านรายการไม่ได้ (project {ProjectId})", projectId);
            return null;
        }
    }

    /// <summary>Returns the sale items of a project (from its Sale Orders) with issued / remaining quantities.</summary>
    [HttpGet("projects/{id}/items")]
    public async Task<ActionResult<IEnumerable<NisProjectItemDto>>> GetProjectItems(string id, [FromQuery] string? cmpid)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var exists = await _context.NisProjects.AsNoTracking().AnyAsync(p => p.ProjectId == id && p.CmpId == cmpid);
        if (!exists)
            return NotFound(new { message = $"Project {id} not found" });

        var items = await _context.NisProjectItems
            .AsNoTracking()
            .Where(i => i.ProjectId == id)
            .OrderBy(i => i.SoNo).ThenBy(i => i.ProdCode)
            .ToListAsync();
        return Ok(items.Select(MapProjectItem));
    }

    /// <summary>
    /// Re-reads the project's Sale Orders (SoRef) and upserts its item list. Quantities already
    /// issued are never reduced; lines removed from the SO are dropped only when nothing was issued.
    /// </summary>
    [HttpPost("projects/{id}/items/sync-so")]
    public async Task<ActionResult<NisProjectItemSyncResultDto>> SyncProjectItemsFromSo(
        string id,
        [FromQuery] string? cmpid,
        [FromQuery] string? username)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(new { message = "username is required" });

        var project = await _context.NisProjects.FirstOrDefaultAsync(p => p.ProjectId == id && p.CmpId == cmpid);
        if (project == null)
            return NotFound(new { message = $"Project {id} not found" });

        var result = await UpsertProjectItemsFromSoAsync(project, username.Trim());
        await _context.SaveChangesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Records quantities taken out of the sale warehouse for a project ("นำของออก"). Positive qty
    /// reserves (409 when it would exceed the SO quantity); negative qty releases (e.g. the WMS
    /// issue document could not be created). Applied in one transaction — all lines or none.
    /// </summary>
    [HttpPut("projects/{id}/items/issued")]
    public async Task<ActionResult<IEnumerable<NisProjectItemDto>>> IssueProjectItems(
        string id,
        [FromBody] NisProjectItemIssueDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.UpdatedBy))
            return BadRequest(new { message = "updatedBy is required" });
        if (dto.Lines == null || dto.Lines.Count == 0 || dto.Lines.Count > 200)
            return BadRequest(new { message = "lines must contain 1-200 items" });
        if (dto.Lines.Any(l => string.IsNullOrWhiteSpace(l.ItemId) || l.Qty == 0 || Math.Abs(l.Qty) > 100000))
            return BadRequest(new { message = "each line needs itemId and a non-zero qty" });

        var exists = await _context.NisProjects.AsNoTracking().AnyAsync(p => p.ProjectId == id && p.CmpId == cmpId);
        if (!exists)
            return NotFound(new { message = $"Project {id} not found" });

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ids = dto.Lines.Select(l => l.ItemId).Distinct().ToList();
        var items = await _context.NisProjectItems
            .Where(i => i.ProjectId == id && ids.Contains(i.Id))
            .ToListAsync();
        if (items.Count != ids.Count)
            return BadRequest(new { message = "itemId not found in this project" });

        var now = BangkokNow();
        foreach (var g in dto.Lines.GroupBy(l => l.ItemId))
        {
            var item = items.First(i => i.Id == g.Key);
            var next = item.QtyIssued + g.Sum(l => l.Qty);
            if (next > item.Qty)
                return Conflict(new
                {
                    message = $"{item.ProdName} ({item.ProdCode}) นำออกเกินจำนวนในโครงการ — คงเหลือ {Math.Max(0, item.Qty - item.QtyIssued):0.##}",
                    itemId = item.Id,
                });
            item.QtyIssued = Math.Max(0, next);
            item.UpdatedDate = now;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation(
            "NIS project items issued: project {ProjectId} ticket {TicketId} by {User} docs {Docs}",
            id, dto.TicketId, dto.UpdatedBy,
            string.Join(",", dto.Lines.Select(l => l.IssueDocNo).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct()));

        return Ok(items.OrderBy(i => i.SoNo).ThenBy(i => i.ProdCode).Select(MapProjectItem));
    }
}
