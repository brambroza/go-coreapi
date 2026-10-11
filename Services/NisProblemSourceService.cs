using System.Data;
using goalongapi.DB;
using goalongapi.Models;
using Microsoft.Data.SqlClient;

namespace goalongapi.Services;

/// <summary>
/// ต้นทางข้อมูลของ NIS Service Report archive — ดึง <see cref="STProblem"/> ทั้งชุดของบริษัทหนึ่ง
/// จาก stored procedure ชุดเดียวกับ <c>ProblemReceiveController.gettaskservice</c>
/// (<c>getProblem</c> · <c>getProblem_Assign</c> · <c>getProblem_File</c> · <c>getProblemActions_All</c> ·
/// <c>getProblemActions_Actions_All</c> · <c>getProblemActions_Files_All</c> — ทุกตัวรับ <c>@CmpId, @User</c>)
///
/// ต่างจาก controller เดิมตรงที่ (1) เรียกผ่าน <see cref="DbConnectionFactory"/> แบบ parameterized ไม่ string concat
/// และไม่ใช้ <c>DBConn</c> static (2) จับคู่แถวลูกด้วย lookup แทน <c>DataTable.Select</c> ต่อแถว
/// (3) ค่าตัวเลขที่ parse ไม่ได้ไม่ทำให้ทั้งรอบล้ม — ตรรกะการประกอบ object คัดลอกจาก gettaskservice ไม่แก้ควบคู่
/// (ห้ามแตะ ProblemReceiveController)
///
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface จึงไม่ถูก Autofac auto-register แม้ชื่อลงท้าย Service)
/// </summary>
public class NisProblemSourceService
{
    /// <summary>SP 6 ตัวดึงทั้งชุด — ให้เวลาเหลือเฟือ (ค่าเริ่มต้น 30 วินาทีอาจไม่พอเมื่อข้อมูลโตหลายปี)</summary>
    private const int CommandTimeoutSeconds = 180;

    private readonly DbConnectionFactory _factory;
    private readonly ILogger<NisProblemSourceService> _logger;

    public NisProblemSourceService(DbConnectionFactory factory, ILogger<NisProblemSourceService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// ดึงเคสทั้งหมดของบริษัท — รูปเดียวกับ response ของ gettaskservice
    /// </summary>
    /// <param name="cmpId">รหัสบริษัท (@CmpId)</param>
    /// <param name="user">username ที่ใช้เรียก SP (@User — ค่าจาก NisReport:SyncUsername)</param>
    /// <param name="ct">cancellation</param>
    public async Task<List<STProblem>> FetchAsync(string cmpId, string user, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmpId)) throw new ArgumentException("cmpId ต้องไม่ว่าง", nameof(cmpId));
        if (string.IsNullOrWhiteSpace(user)) throw new ArgumentException("user ต้องไม่ว่าง", nameof(user));

        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        var dt = await ExecAsync(conn, "dbo.getProblem", cmpId, user, ct);
        var dta = await ExecAsync(conn, "dbo.getProblem_Assign", cmpId, user, ct);
        var dtf = await ExecAsync(conn, "dbo.getProblem_File", cmpId, user, ct);
        var dtba = await ExecAsync(conn, "dbo.getProblemActions_All", cmpId, user, ct);
        var dtbb = await ExecAsync(conn, "dbo.getProblemActions_Actions_All", cmpId, user, ct);
        var dtbf = await ExecAsync(conn, "dbo.getProblemActions_Files_All", cmpId, user, ct);

        // lookup แทน DataTable.Select — O(n) แทน O(n·m) และไม่ต้องประกอบ filter expression จากค่าในแถว
        var filesByProblem = dtf.Rows.Cast<DataRow>().ToLookup(r => Str(r, "ProblemId"));
        var assignByProblem = dta.Rows.Cast<DataRow>().ToLookup(r => Str(r, "ProblemId"));
        var actionsByProblem = dtba.Rows.Cast<DataRow>().ToLookup(r => Str(r, "ProblemId"));
        var actionByByAction = dtbb.Rows.Cast<DataRow>().ToLookup(r => Str(r, "ServiceActionId"));
        var actionFilesByAction = dtbf.Rows.Cast<DataRow>().ToLookup(r => Str(r, "ServiceActionId"));

        var problems = new List<STProblem>(dt.Rows.Count);
        foreach (DataRow b in dt.Rows)
        {
            var p = new STProblem
            {
                UpdUser = Str(b, "UpdUser"),
                ProblemId = Str(b, "ProblemId"),
                ReceiveDate = Str(b, "ReceiveDate"),
                CustomerCode = Str(b, "CustomerCode"),
                ContactName = Str(b, "ContactName"),
                ProblemDetails = Str(b, "ProblemDetails"),
                ReceiveTime = Str(b, "ReceiveTime"),
                ProblemType = Str(b, "ProblemType"),
                CustBranchName = Str(b, "CustBranchName"),
                CmpId = Str(b, "CmpId"),
                CustomerName = Str(b, "CustomerName"),
                RequestBy = Str(b, "RequestBy"),
                ProvinceId = Str(b, "ProvinceId"),
                Status = Str(b, "Status"),
                Priority = Str(b, "Priority"),
                GrpId = Str(b, "GrpId"),
                TaskNo = Str(b, "TaskNo"),
                TaskId = Str(b, "TaskId"),
                UserLineId = Str(b, "UserLineId"),
                OALineId = Str(b, "OALineId"),
                StartDate = Str(b, "StartDate"),
                StartTime = Str(b, "StartTime"),
                FeedbackRating = IntOrNull(b, "FeedbackRating"),
                FeedbackDate = Str(b, "FeedbackDate"),
                IsUnReadMsgCount = IntOrNull(b, "IsUnReadMsgCount"),
                FeedbackDescription = Str(b, "FeedbackDescription"),
                requestEmail = Str(b, "RequestEmail"),
                requestPhone = Str(b, "RequestPhone"),
                requestPosition = Str(b, "RequestPosition"),
                Remark = Str(b, "Remark"),
                IsReadMenu = Str(b, "IsReadMenu"),
                ModifiedAt = Str(b, "ModifiedAt"),
                UpdatedAt = Str(b, "UpdatedAt"),
                RequestByName = Str(b, "RequestByName"),
                AnydeskId = Str(b, "AnydeskId"),
                ImgPath = Str(b, "ImgPath"),
                attachfile = new List<STProblem_File>(),
                assign = new List<STProblem_Assign>(),
                // gettaskservice ตั้ง action เป็น object ว่างเสมอ แล้วทับด้วยแถวสุดท้ายที่พบ — คงพฤติกรรมเดิม
                action = new STServiceActions(),
            };

            foreach (var f in filesByProblem[p.ProblemId])
            {
                p.attachfile.Add(new STProblem_File
                {
                    UpdUser = Str(f, "UpdUser"),
                    ProblemId = Str(f, "ProblemId"),
                    Seq = IntOrNull(f, "Seq") ?? 0,
                    FileName = Str(f, "FileName"),
                    FilePath = Str(f, "FilePath"),
                    CmpId = Str(f, "CmpId"),
                });
            }

            foreach (var f in assignByProblem[p.ProblemId])
            {
                p.assign.Add(new STProblem_Assign
                {
                    UpdUser = Str(f, "UpdUser"),
                    ProblemId = Str(f, "ProblemId"),
                    UserFullName = Str(f, "UserFullName"),
                    ImgPath = Str(f, "ImgPath"),
                    Permission = Str(f, "Permission"),
                    RouteId = Str(f, "RouteId"),
                    RemindId = Str(f, "RemindId"),
                    UserId = Str(f, "UserId"),
                    CmpId = Str(f, "CmpId"),
                    StateOwner = Str(f, "StateOwner"),
                });
            }

            foreach (var f in actionsByProblem[p.ProblemId])
            {
                var action = new STServiceActions
                {
                    UpdUser = Str(f, "UpdUser"),
                    ProblemId = Str(f, "ProblemId"),
                    ServiceActionId = Str(f, "ServiceActionId"),
                    ServiceType = IntOrNull(f, "ServiceType") ?? 0,
                    ActionDetails = Str(f, "ActionDetails"),
                    FinishDate = Str(f, "FinishDate"),
                    FinishTime = Str(f, "FinishTime"),
                    CmpId = Str(f, "CmpId"),
                    ActionBy = new List<STServiceActions_Assign>(),
                    Attachfile = new List<STServiceActions_File>(),
                };

                foreach (var ac in actionByByAction[action.ServiceActionId])
                {
                    action.ActionBy.Add(new STServiceActions_Assign
                    {
                        UpdUser = Str(ac, "UpdUser"),
                        ServiceActionId = Str(ac, "ServiceActionId"),
                        UserFullName = Str(ac, "UserFullName"),
                        ImgPath = Str(ac, "ImgPath"),
                        Permission = Str(ac, "Permission"),
                        RouteId = Str(ac, "RouteId"),
                        RemindId = Str(ac, "RemindId"),
                        UserId = Str(ac, "UserId"),
                        CmpId = Str(ac, "CmpId"),
                    });
                }

                foreach (var fa in actionFilesByAction[action.ServiceActionId])
                {
                    action.Attachfile.Add(new STServiceActions_File
                    {
                        UpdUser = Str(fa, "UpdUser"),
                        ServiceActionId = Str(fa, "ServiceActionId"),
                        Seq = IntOrNull(fa, "Seq") ?? 0,
                        FileName = Str(fa, "FileName"),
                        FilePath = Str(fa, "FilePath"),
                        CmpId = Str(fa, "CmpId"),
                    });
                }

                p.action = action; // แถวสุดท้ายชนะ — ตรงกับ gettaskservice
            }

            problems.Add(p);
        }

        _logger.LogDebug("NisProblemSource: cmp {CmpId} ได้ {Count} problem", cmpId, problems.Count);
        return problems;
    }

    /// <summary>เรียก SP แบบ parameterized แล้วโหลดเป็น DataTable</summary>
    private static async Task<DataTable> ExecAsync(SqlConnection conn, string sp, string cmpId, string user, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sp;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = CommandTimeoutSeconds;
        cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.NVarChar, 50) { Value = cmpId });
        cmd.Parameters.Add(new SqlParameter("@User", SqlDbType.NVarChar, 100) { Value = user });

        var table = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        table.Load(reader);
        return table;
    }

    /// <summary>อ่านคอลัมน์เป็นข้อความ — DBNull/คอลัมน์ไม่มี = "" (ตรงกับ <c>row[col].ToString()</c> เดิม)</summary>
    private static string Str(DataRow row, string column)
    {
        if (!row.Table.Columns.Contains(column)) return string.Empty;
        var v = row[column];
        return v is null or DBNull ? string.Empty : Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>อ่านคอลัมน์เป็น int — ค่าว่าง/parse ไม่ได้คืน null (เดิม Convert.ToInt32 โยนแล้วทั้ง endpoint ล้ม)</summary>
    private static int? IntOrNull(DataRow row, string column)
    {
        var s = Str(row, column);
        if (s.Length == 0) return null;
        if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i)) return i;
        if (decimal.TryParse(s, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)) return (int)d;
        return null;
    }
}
