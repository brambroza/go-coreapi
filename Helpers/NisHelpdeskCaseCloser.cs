using System.Data;
using System.Data.Common;
using goalongapi.Data;
using goalongapi.Models.Nis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace goalongapi.Helpers;

/// <summary>
/// ปิดเคส helpdesk ต้นทาง (STProblem) เมื่อตั๋ว Onsite ที่เปิดจากเคสนั้น (NisTicket.SourceProblemId) ถูกปิด
///
/// เคส helpdesk "ปิด" = มีแถวใน dbo.STServiceActions — SP getProblem คืน status 'Finish' จากการมี Action
/// (ไม่ได้อ่าน STProblem.Status) จึงสร้าง Action ผ่าน SP ชุดเดียวกับหน้า helpdesk (POST setactionservice):
/// STServiceActions_Trans + STServiceActions_Trans_Assign · เลข ServiceActionId จาก sequence เดียวกับ DocNo type "action"
///
/// - idempotent: เคสที่มี Action อยู่แล้ว (staff ปิดเอง / ปิดซ้ำ / retry) ไม่สร้างเพิ่ม — ล็อกด้วย UPDLOCK+HOLDLOCK ใน transaction
/// - best-effort: ผู้เรียกต้องไม่ให้การปิดตั๋วล้มตาม — method นี้จับ exception เองแล้ว log
/// - ไม่ส่ง LINE ขอคะแนนลูกค้า (หน้า helpdesk ส่ง sendfinish เองตอน staff ปิด) — ตอนปิดตั๋วลูกค้าได้อีเมล + PDF แล้ว
/// - ตั๋วถูกเปิดกลับภายหลัง ไม่เปิดเคส helpdesk กลับ
/// </summary>
public static class NisHelpdeskCaseCloser
{
    /// ActionDetails ของตาราง STServiceActions เป็น nvarchar(500) — ยาวเกิน SP จะ error แล้วกลืนเงียบ
    private const int ActionDetailsMax = 500;
    /// STServiceActions.UpdUser เป็น nvarchar(50)
    private const int UpdUserMax = 50;
    private const string FallbackActor = "system:nis";

    /// <summary>
    /// สร้าง Action ปิดเคส helpdesk ต้นทางของตั๋ว ถ้าตั๋วปิดแล้ว (Closed/Done) และเคสยังไม่มี Action
    /// </summary>
    /// <param name="db">DbContext ของ request/scope ปัจจุบัน (ใช้ connection เดียวกัน)</param>
    /// <param name="ticket">ตั๋วที่เพิ่งบันทึกสถานะปิดแล้ว (เรียกหลัง SaveChanges)</param>
    /// <param name="actor">ผู้ทำรายการ (username) — ว่าง = system:nis</param>
    /// <param name="srNumber">เลข Service Report ถ้ามี — ใส่ในรายละเอียด Action</param>
    /// <param name="logger">logger ของผู้เรียก</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>ServiceActionId ที่สร้าง หรือ null เมื่อไม่ต้องสร้าง/สร้างไม่สำเร็จ</returns>
    public static async Task<string?> CloseSourceProblemAsync(
        DatabaseContext db,
        NisTicket ticket,
        string? actor,
        string? srNumber,
        ILogger logger,
        CancellationToken ct = default)
    {
        var problemId = ticket.SourceProblemId?.Trim();
        var cmpId = ticket.CmpId?.Trim();
        if (string.IsNullOrEmpty(problemId) || string.IsNullOrEmpty(cmpId)) return null;
        if (ticket.Status is not ("Closed" or "Done")) return null;

        try
        {
            var assignUserId = await ResolveAssigneeAccountIdAsync(db, cmpId, ticket.Assignee, ct);
            var updUser = Truncate(string.IsNullOrWhiteSpace(actor) ? FallbackActor : actor.Trim(), UpdUserMax);
            var details = BuildActionDetails(ticket, srNumber);

            var conn = db.Database.GetDbConnection();
            var openedHere = conn.State != ConnectionState.Open;
            if (openedHere) await db.Database.OpenConnectionAsync(ct);
            try
            {
                var sequence = await ResolveActionSequenceAsync(conn, cmpId, ct);

                await using var cmd = conn.CreateCommand();
                // ชื่อ sequence มาจาก sys.sequences (ไม่ใช่ input ผู้ใช้) และ escape ']' แล้ว — ค่าอื่นเป็น parameter ทั้งหมด
                cmd.CommandText = $@"
BEGIN TRAN;
DECLARE @id varchar(30) = NULL;
IF NOT EXISTS (SELECT 1 FROM dbo.STServiceActions WITH (UPDLOCK, HOLDLOCK)
               WHERE ProblemId = @ProblemId AND CmpId = @CmpId)
   AND EXISTS (SELECT 1 FROM dbo.STProblem WHERE ProblemId = @ProblemId AND CmpId = @CmpId)
BEGIN
    DECLARE @st int = (SELECT TOP 1 Id FROM dbo.SystemList
                       WHERE ListName = 'ServiceType' AND StateActive = 1 AND ListDescription LIKE N'%onsite%'
                       ORDER BY Id);
    SET @id = CAST(NEXT VALUE FOR {sequence} AS varchar(30));
    EXEC dbo.STServiceActions_Trans
         @UpdUser = @UpdUser, @ServiceActionId = @id, @ProblemId = @ProblemId,
         @ServiceType = @st, @ActionDetails = @ActionDetails,
         @FinishDate = '', @FinishTime = '', @CmpId = @CmpId;
    -- SP จับ error เอง (เขียน LogExecSql แล้วกลืน) → ยืนยันว่าแถวเข้าจริง
    IF NOT EXISTS (SELECT 1 FROM dbo.STServiceActions WHERE ServiceActionId = @id AND ProblemId = @ProblemId)
        SET @id = NULL;
    ELSE IF @AssignUserId IS NOT NULL
        EXEC dbo.STServiceActions_Trans_Assign @ServiceActionId = @id, @UserId = @AssignUserId, @CmpId = @CmpId;
END
COMMIT;
SELECT @id;";
                cmd.Parameters.Add(new SqlParameter("@ProblemId", SqlDbType.VarChar, 50) { Value = problemId });
                cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.VarChar, 50) { Value = cmpId });
                cmd.Parameters.Add(new SqlParameter("@UpdUser", SqlDbType.NVarChar, 100) { Value = updUser });
                cmd.Parameters.Add(new SqlParameter("@ActionDetails", SqlDbType.NVarChar, 1000) { Value = details });
                cmd.Parameters.Add(new SqlParameter("@AssignUserId", SqlDbType.NVarChar, 100)
                {
                    Value = (object?)assignUserId ?? DBNull.Value,
                });

                var result = await cmd.ExecuteScalarAsync(ct);
                var actionId = result is null or DBNull ? null : result.ToString();
                if (actionId != null)
                {
                    logger.LogInformation(
                        "NIS helpdesk close: ตั๋ว {TicketCode} ปิด → สร้าง Action {ActionId} ปิดเคส {ProblemId}",
                        ticket.TicketCode, actionId, problemId);
                }
                return actionId;
            }
            finally
            {
                if (openedHere) await db.Database.CloseConnectionAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "NIS helpdesk close: ปิดเคส {ProblemId} จากตั๋ว {TicketCode} ไม่สำเร็จ (ตั๋วปิดแล้วตามปกติ)",
                problemId, ticket.TicketCode);
            return null;
        }
    }

    /// รายละเอียด Action ที่ staff เห็นในหน้าเคส — อ้างตั๋ว/SR/ช่าง แล้วตามด้วยสรุปงาน ตัดให้พอดีคอลัมน์
    private static string BuildActionDetails(NisTicket ticket, string? srNumber)
    {
        var head = $"ปิดงานตามตั๋ว Onsite {ticket.TicketCode}";
        if (!string.IsNullOrWhiteSpace(srNumber)) head += $" · SR {srNumber.Trim()}";
        if (!string.IsNullOrWhiteSpace(ticket.Assignee) && ticket.Assignee != "-")
            head += $" · ผู้ปฏิบัติงาน {ticket.Assignee.Trim()}";

        var work = ticket.WorkDetail?.Trim();
        var text = string.IsNullOrEmpty(work) ? head : $"{head}\n{work}";
        return Truncate(text, ActionDetailsMax);
    }

    /// NisTicket.Assignee เก็บ FullName (บางแถวเป็น Username) → AccountId ที่ STServiceActions_Assign ใช้
    private static async Task<string?> ResolveAssigneeAccountIdAsync(
        DatabaseContext db, string cmpId, string? assignee, CancellationToken ct)
    {
        var key = assignee?.Trim();
        if (string.IsNullOrEmpty(key) || key == "-") return null;
        var id = await db.Accounts
            .AsNoTracking()
            .Where(a => a.CmpId == cmpId && (a.Username == key || a.FullName == key))
            .Select(a => (long?)a.AccountId)
            .FirstOrDefaultAsync(ct);
        return id?.ToString();
    }

    /// sequence ของเลข Action ตาม DocNoController (type "action"): มี dbo.[{cmpid}-ProblemActionId] ใช้ตัวนั้น ไม่มีใช้ dbo.ProblemActionId
    private static async Task<string> ResolveActionSequenceAsync(DbConnection conn, string cmpId, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 name FROM sys.sequences WHERE schema_id = SCHEMA_ID('dbo') AND name = @name";
        cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = $"{cmpId}-ProblemActionId" });
        var name = (await cmd.ExecuteScalarAsync(ct)) as string ?? "ProblemActionId";
        return $"[dbo].[{name.Replace("]", "]]")}]";
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
