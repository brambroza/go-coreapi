using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Controllers;

// ── api/nis/stock-transfer-requests (dbo.NisStockTransferRequest) ─────────────
// ช่างขอโยกย้ายคลังจากหน้าเบิกของ (คลัง 2 Spare ลูกค้า / คลัง 3 ของใน Stock ไม่มีของ)
// → SM โยกย้ายจริงผ่าน WMS โอนย้ายคลัง (transferwh) แล้วกด "โยกแล้ว" / "ปฏิเสธ" ที่นี่
// endpoint ชุดนี้ไม่ขยับสต็อกเอง — แยกไฟล์ (partial) เพื่อไม่ให้ NisController.cs โตเพิ่ม

public partial class NisController
{
    private const string STOCK_TR_PENDING = "Pending";
    private const string STOCK_TR_DONE = "Done";
    private const string STOCK_TR_REJECTED = "Rejected";

    /// คลังที่ใช้เบิกของหน้างานของ tenant (null config = ค่าตั้งต้นของ NIS)
    private async Task<NisStockWarehousesDto> GetStockWarehousesAsync(string cmpId)
    {
        var json = await _context.NisSystemConfigs
            .AsNoTracking()
            .Where(c => c.CmpId == cmpId)
            .Select(c => c.StockWarehousesJson)
            .FirstOrDefaultAsync();
        return ParseJson(json, new NisStockWarehousesDto());
    }

    /// ผู้ใช้เป็น Service Manager ของ tenant หรือไม่ (เทียบ Username / FullName กับ SystemRole.StateManager)
    private async Task<bool> IsNisManagerAsync(string cmpId, string usernameOrFullName)
    {
        var key = usernameOrFullName.Trim();
        return (await GetNisManagersAsync(cmpId)).Any(m =>
            string.Equals(m.Username, key, StringComparison.OrdinalIgnoreCase)
            || string.Equals(m.FullName, key, StringComparison.OrdinalIgnoreCase));
    }

    /// เติม TicketCode + CustomerName (NisTicket → NisProject) + ชื่อผู้ขอ
    private async Task<List<NisStockTransferRequestDto>> MapStockTransferRequestsAsync(
        string cmpId, List<NisStockTransferRequest> rows)
    {
        if (rows.Count == 0) return new List<NisStockTransferRequestDto>();

        var ticketIds = rows.Where(r => !string.IsNullOrWhiteSpace(r.TicketId)).Select(r => r.TicketId!).Distinct().ToList();
        var tickets = ticketIds.Count == 0
            ? new Dictionary<string, (string? Code, string? Customer)>()
            : (await (from t in _context.NisTickets.AsNoTracking()
                      join p in _context.NisProjects.AsNoTracking() on t.ProjectId equals p.ProjectId into pj
                      from p in pj.DefaultIfEmpty()
                      where ticketIds.Contains(t.TicketId)
                      select new { t.TicketId, t.TicketCode, Customer = p != null ? p.Customer : null })
                    .ToListAsync())
                .ToDictionary(x => x.TicketId, x => (Code: (string?)x.TicketCode, Customer: x.Customer));

        var accountInfos = await ResolveAccountInfosAsync(cmpId, rows.Select(r => r.RequestedBy));

        return rows.Select(r =>
            {
                var hasTicket = r.TicketId != null && tickets.TryGetValue(r.TicketId, out _);
                var t = hasTicket ? tickets[r.TicketId!] : (Code: null, Customer: null);
                return new NisStockTransferRequestDto
                {
                    RequestId = r.RequestId,
                    CmpId = r.CmpId,
                    TicketId = r.TicketId,
                    TicketCode = t.Code,
                    CustomerName = t.Customer,
                    ProdCode = r.ProdCode,
                    ProdName = r.ProdName,
                    UnitCode = r.UnitCode,
                    Qty = r.Qty,
                    ToWhId = r.ToWhId,
                    Note = r.Note,
                    RequestedBy = r.RequestedBy,
                    RequestedByName = !string.IsNullOrWhiteSpace(r.RequestedBy)
                        && accountInfos.TryGetValue(r.RequestedBy.Trim(), out var info) ? info.FullName : null,
                    Status = r.Status,
                    HandledBy = r.HandledBy,
                    TransferDocNo = r.TransferDocNo,
                    RejectReason = r.RejectReason,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                };
            })
            .ToList();
    }

    /// <summary>
    /// Lists stock transfer requests for a company, newest first. Optional filters: status
    /// (Pending|Done|Rejected), ticketId, requestedBy. SM approve tab uses status=Pending;
    /// the onsite stock step uses ticketId.
    /// </summary>
    [HttpGet("stock-transfer-requests")]
    public async Task<ActionResult<IEnumerable<NisStockTransferRequestDto>>> GetStockTransferRequests(
        [FromQuery] string? cmpid,
        [FromQuery] string? status,
        [FromQuery] string? ticketId,
        [FromQuery] string? requestedBy)
    {
        if (string.IsNullOrWhiteSpace(cmpid))
            return BadRequest(new { message = "cmpid is required" });

        var query = _context.NisStockTransferRequests
            .AsNoTracking()
            .Where(r => r.CmpId == cmpid);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(ticketId))
            query = query.Where(r => r.TicketId == ticketId);
        if (!string.IsNullOrWhiteSpace(requestedBy))
            query = query.Where(r => r.RequestedBy == requestedBy);

        var rows = await query.OrderByDescending(r => r.CreatedDate).Take(500).ToListAsync();
        return Ok(await MapStockTransferRequestsAsync(cmpid, rows));
    }

    /// <summary>
    /// Creates a stock transfer request (Status = Pending) and notifies every Service Manager
    /// (Expo push + socket nis:notify + CRM bell). The destination must be the tenant's
    /// customer-spare or in-stock warehouse (NisSystemConfig.StockWarehouses).
    /// </summary>
    [HttpPost("stock-transfer-requests")]
    public async Task<ActionResult<NisStockTransferRequestDto>> CreateStockTransferRequest(
        [FromBody] NisStockTransferRequestCreateDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.ProdCode))
            return BadRequest(new { message = "prodCode is required" });
        if (string.IsNullOrWhiteSpace(dto.RequestedBy))
            return BadRequest(new { message = "requestedBy is required" });
        if (dto.Qty <= 0 || dto.Qty > 100000)
            return BadRequest(new { message = "qty must be between 0 and 100000" });

        var wh = await GetStockWarehousesAsync(cmpId);
        if (dto.ToWhId != wh.SpareWhId && dto.ToWhId != wh.StockWhId)
            return BadRequest(new { message = "toWhId must be the customer-spare or in-stock warehouse" });

        NisTicket? ticket = null;
        if (!string.IsNullOrWhiteSpace(dto.TicketId))
        {
            ticket = await _context.NisTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == dto.TicketId && t.CmpId == cmpId);
            if (ticket == null)
                return BadRequest(new { message = "ticketId not found" });
        }

        var now = BangkokNow();
        var row = new NisStockTransferRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            CmpId = cmpId,
            TicketId = ticket?.TicketId,
            ProdCode = dto.ProdCode.Trim(),
            ProdName = (dto.ProdName ?? string.Empty).Trim(),
            UnitCode = (dto.UnitCode ?? string.Empty).Trim(),
            Qty = dto.Qty,
            ToWhId = dto.ToWhId,
            Note = (dto.Note ?? string.Empty).Trim(),
            RequestedBy = dto.RequestedBy.Trim(),
            Status = STOCK_TR_PENDING,
            CreatedDate = now,
            UpdatedDate = now,
        };

        _context.NisStockTransferRequests.Add(row);
        await _context.SaveChangesAsync();

        await NotifyStockTransferRequestCreatedAsync(row, ticket);

        var mapped = await MapStockTransferRequestsAsync(cmpId, new List<NisStockTransferRequest> { row });
        return Ok(mapped[0]);
    }

    /// <summary>
    /// Marks a pending request as done after the manager has moved the stock in WMS
    /// (optionally recording the transfer document number) and notifies the requester.
    /// Only a Service Manager of the tenant may do this (403 otherwise).
    /// </summary>
    [HttpPut("stock-transfer-requests/{id}/done")]
    public async Task<ActionResult<NisStockTransferRequestDto>> CompleteStockTransferRequest(
        string id,
        [FromBody] NisStockTransferRequestDoneDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.HandledBy))
            return BadRequest(new { message = "handledBy is required" });
        if (!await IsNisManagerAsync(cmpId, dto.HandledBy))
            return StatusCode(403, new { message = "เฉพาะ Service Manager เท่านั้น" });

        var row = await _context.NisStockTransferRequests
            .FirstOrDefaultAsync(r => r.RequestId == id && r.CmpId == cmpId);
        if (row == null)
            return NotFound(new { message = $"Stock transfer request '{id}' not found" });
        if (row.Status != STOCK_TR_PENDING)
            return BadRequest(new { message = $"Request is already {row.Status}" });

        var docNo = dto.TransferDocNo?.Trim();
        if (docNo is { Length: > 50 })
            return BadRequest(new { message = "transferDocNo is too long" });

        row.Status = STOCK_TR_DONE;
        row.HandledBy = dto.HandledBy.Trim();
        row.TransferDocNo = string.IsNullOrWhiteSpace(docNo) ? null : docNo;
        row.UpdatedDate = BangkokNow();
        await _context.SaveChangesAsync();

        await NotifyStockTransferDecisionAsync(row, done: true);

        var mapped = await MapStockTransferRequestsAsync(cmpId, new List<NisStockTransferRequest> { row });
        return Ok(mapped[0]);
    }

    /// <summary>Rejects a pending stock transfer request (Service Manager only) and notifies the requester.</summary>
    [HttpPut("stock-transfer-requests/{id}/reject")]
    public async Task<ActionResult<NisStockTransferRequestDto>> RejectStockTransferRequest(
        string id,
        [FromBody] NisStockTransferRequestRejectDto dto)
    {
        var cmpId = dto.CmpId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cmpId))
            return BadRequest(new { message = "cmpid is required" });
        if (string.IsNullOrWhiteSpace(dto.HandledBy))
            return BadRequest(new { message = "handledBy is required" });
        if (!await IsNisManagerAsync(cmpId, dto.HandledBy))
            return StatusCode(403, new { message = "เฉพาะ Service Manager เท่านั้น" });

        var row = await _context.NisStockTransferRequests
            .FirstOrDefaultAsync(r => r.RequestId == id && r.CmpId == cmpId);
        if (row == null)
            return NotFound(new { message = $"Stock transfer request '{id}' not found" });
        if (row.Status != STOCK_TR_PENDING)
            return BadRequest(new { message = $"Request is already {row.Status}" });

        row.Status = STOCK_TR_REJECTED;
        row.HandledBy = dto.HandledBy.Trim();
        row.RejectReason = string.IsNullOrWhiteSpace(dto.RejectReason) ? null : dto.RejectReason.Trim();
        row.UpdatedDate = BangkokNow();
        await _context.SaveChangesAsync();

        await NotifyStockTransferDecisionAsync(row, done: false);

        var mapped = await MapStockTransferRequestsAsync(cmpId, new List<NisStockTransferRequest> { row });
        return Ok(mapped[0]);
    }

    /// <summary>คำขอใหม่ → SM ทุกคน (push + socket + กระดิ่ง CRM) · best-effort ห้าม throw</summary>
    private async Task NotifyStockTransferRequestCreatedAsync(NisStockTransferRequest row, NisTicket? ticket)
    {
        try
        {
            var managers = await GetNisManagersAsync(row.CmpId);
            if (managers.Count == 0) return;

            const string title = "📦 ขอโยกย้ายคลัง";
            var body = $"{(ticket?.TicketCode is { } code ? code + " · " : "")}{row.ProdName} ({row.ProdCode}) × {row.Qty:0.##}";

            foreach (var m in managers.Where(m => !string.IsNullOrWhiteSpace(m.FullName)))
            {
                await _push.SendToStaffAsync(
                    row.CmpId,
                    m.FullName,
                    eventKey: $"stocktr:{row.RequestId}:{m.FullName}",
                    title: title,
                    body: body,
                    ticketId: ticket?.TicketId,
                    data: new Dictionary<string, string> { ["type"] = "stock_transfer_request", ["requestId"] = row.RequestId });
            }

            var managerUsernames = managers.Select(m => m.Username).Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
            if (managerUsernames.Count == 0) return;

            await _nisRealtimeNotify.NotifyAsync(
                row.CmpId,
                users: managerUsernames,
                type: "stock_transfer_request",
                ticketId: ticket?.TicketId,
                title: title,
                body: body);

            var requesterUsername = await ResolveUsernameAsync(row.CmpId, row.RequestedBy);
            foreach (var username in managerUsernames)
            {
                await _crmNotify.NotifyAsync(
                    row.CmpId,
                    toUsername: username,
                    fromUsername: requesterUsername ?? row.RequestedBy,
                    title: $"{title}: {body}",
                    // แท็บอนุมัติของ Service Board รีเฟรชทันทีจาก moduleFormName นี้
                    linkTo: "/productservice/service-board",
                    moduleFormName: "nis/serviceboard",
                    docNo: ticket?.TicketCode ?? row.RequestId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS stock transfer request: แจ้งเตือน SM ไม่สำเร็จ (request {RequestId})", row.RequestId);
        }
    }

    /// <summary>
    /// ผลโยกแล้ว/ปฏิเสธ → ช่างผู้ขอ · ผูกตั๋ว = NotifyTicketChangedAsync (action stock_transfer_done|rejected
    /// ให้ RN/CRM ที่เปิดตั๋วอยู่ refresh onhand) · ไม่ผูกตั๋ว = socket ตรงถึงผู้ขอ · best-effort
    /// </summary>
    private async Task NotifyStockTransferDecisionAsync(NisStockTransferRequest row, bool done)
    {
        try
        {
            var what = $"{row.ProdName} ({row.ProdCode}) × {row.Qty:0.##}";
            var body = done
                ? $"โยกย้ายคลังแล้ว {what}{(row.TransferDocNo is { } doc ? $" · ใบโอน {doc}" : "")} — เบิกต่อได้"
                : $"ไม่อนุมัติคำขอโยกย้าย {what}{(string.IsNullOrWhiteSpace(row.RejectReason) ? "" : $" — {row.RejectReason}")}";
            var action = done ? "stock_transfer_done" : "stock_transfer_rejected";

            var ticket = string.IsNullOrWhiteSpace(row.TicketId)
                ? null
                : await _context.NisTickets.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.CmpId == row.CmpId && t.TicketId == row.TicketId);
            if (ticket != null)
            {
                await NotifyTicketChangedAsync(ticket, action, row.HandledBy, body, extraAssignee: row.RequestedBy);
                return;
            }

            var requester = await ResolveUsernameAsync(row.CmpId, row.RequestedBy);
            if (string.IsNullOrWhiteSpace(requester)) return;
            await _nisRealtimeNotify.NotifyAsync(
                row.CmpId,
                users: new[] { requester },
                type: "stock_transfer_update",
                title: "📦 คำขอโยกย้ายคลัง",
                body: body,
                action: action);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NIS stock transfer decision notify ไม่สำเร็จ (request {RequestId})", row.RequestId);
        }
    }
}
