using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using goalongapi.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using goalongapi.Models;
using System.Globalization;

namespace goalongapi.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmpWorkingOnsiteController : ControllerBase
    {
        private readonly DbConnectionFactory _dbFactory;
        private readonly ILogger<EmpWorkingOnsiteController> _logger;

        public EmpWorkingOnsiteController(
            DbConnectionFactory dbFactory,
            ILogger<EmpWorkingOnsiteController> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        // helper: parse yyyy-MM-dd เป็น ค.ศ. เสมอ
        private static bool TryParseDateInvariant(string input, out DateTime date)
        {
            return DateTime.TryParseExact(
                input,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date
            );
        }

        // --------------------------------------------------------------------
        // GET: api/EmpWorkingOnsite?cmpId=NIS&userLogin=123
        // --------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpWorkingOnsite>>> GetList(
            [FromQuery] string cmpId,
            [FromQuery(Name = "userLogin")] string userLogin,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cmpId))
            {
                return BadRequest("cmpId is required.");
            }

            

            try
            {
                var result = new List<EmpWorkingOnsite>();

                await using var conn = _dbFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string sql = @"
                    SELECT 
                        UpdUser,
                        CmpId,
                        AccountId,
                        Customer,
                        SiteName,
                        [Description],
                        CONVERT(varchar(10), TransDate, 23) AS TransDate,
                        CONVERT(varchar(8), StartTime, 108) AS StartTime,
                        CONVERT(varchar(8), EndTime, 108) AS EndTime,
                        EmployeeCode
                    FROM [hr].[EmpWorkingOnsite]
                    WHERE CmpId = @CmpId
                    ORDER BY TransDate DESC, StartTime DESC;
                ";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.VarChar, 50) { Value = cmpId });
                cmd.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = accountId });

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(new EmpWorkingOnsite
                    {
                        UpdUser = reader["UpdUser"] as string,
                        CmpId = reader["CmpId"].ToString()!,
                        AccountId = (int)reader["AccountId"],
                        Customer = reader["Customer"].ToString()!,
                        SiteName = reader["SiteName"] as string,
                        Description = reader["Description"] as string,
                        TransDate = reader["TransDate"].ToString()!,
                        StartTime = reader["StartTime"].ToString()!,
                        EndTime = reader["EndTime"].ToString()!,
                        EmployeeCode = reader["EmployeeCode"] as string
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in GetList for CmpId {CmpId}, AccountId {AccountId}", cmpId, accountId);

                return StatusCode(500, "An error occurred while retrieving data.");
            }
        }

        // --------------------------------------------------------------------
        // GET: api/EmpWorkingOnsite/{cmpId}/{accountId}/{transDate}/{startTime}
        // --------------------------------------------------------------------
        [HttpGet("{cmpId}/{accountId:int}/{transDate}/{startTime}")]
        public async Task<ActionResult<EmpWorkingOnsite>> GetOne(
            string cmpId,
            int accountId,
            string transDate,
            string startTime,
            CancellationToken cancellationToken)
        {
            if (!TryParseDateInvariant(transDate, out var transDateParsed))
            {
                return BadRequest("รูปแบบ TransDate ไม่ถูกต้อง (ควรเป็น yyyy-MM-dd)");
            }

            if (!TimeSpan.TryParse(startTime, out var startTimeParsed))
            {
                return BadRequest("รูปแบบ StartTime ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");
            }

            try
            {
                await using var conn = _dbFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string sql = @"
                    SELECT 
                        UpdUser,
                        CmpId,
                        AccountId,
                        Customer,
                        SiteName,
                        [Description],
                        CONVERT(varchar(10), TransDate, 23) AS TransDate,
                        CONVERT(varchar(8), StartTime, 108) AS StartTime,
                        CONVERT(varchar(8), EndTime, 108) AS EndTime,
                        EmployeeCode
                    FROM [hr].[EmpWorkingOnsite]
                    WHERE CmpId = @CmpId
                      AND AccountId = @AccountId
                      AND TransDate = @TransDate
                      AND StartTime = @StartTime;
                ";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.VarChar, 50) { Value = cmpId });
                cmd.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = accountId });
                cmd.Parameters.Add(new SqlParameter("@TransDate", SqlDbType.Date) { Value = transDateParsed });
                cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = startTimeParsed });

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                if (!await reader.ReadAsync(cancellationToken))
                {
                    return NotFound();
                }

                var entity = new EmpWorkingOnsite
                {
                    UpdUser = reader["UpdUser"] as string,
                    CmpId = reader["CmpId"].ToString()!,
                    AccountId = (int)reader["AccountId"],
                    Customer = reader["Customer"].ToString()!,
                    SiteName = reader["SiteName"] as string,
                    Description = reader["Description"] as string,
                    TransDate = reader["TransDate"].ToString()!,
                    StartTime = reader["StartTime"].ToString()!,
                    EndTime = reader["EndTime"].ToString()!,
                    EmployeeCode = reader["EmployeeCode"] as string
                };

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in GetOne for CmpId {CmpId}, AccountId {AccountId}, TransDate {TransDate}, StartTime {StartTime}",
                    cmpId, accountId, transDate, startTime);

                return StatusCode(500, "An error occurred while retrieving data.");
            }
        }

        // --------------------------------------------------------------------
        // POST: api/EmpWorkingOnsite
        // --------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult> Create(
            [FromBody] EmpWorkingOnsite model,
            CancellationToken cancellationToken)
        {
            if (!TryParseDateInvariant(model.TransDate, out var transDateParsed))
            {
                return BadRequest("รูปแบบ TransDate ไม่ถูกต้อง (ควรเป็น yyyy-MM-dd)");
            }

            if (!TimeSpan.TryParse(model.StartTime, out var startTimeParsed))
            {
                return BadRequest("รูปแบบ StartTime ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");
            }

            if (!TimeSpan.TryParse(model.EndTime, out var endTimeParsed))
            {
                return BadRequest("รูปแบบ EndTime ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");
            }

            try
            {
                await using var conn = _dbFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string sql = @"
                    INSERT INTO [hr].[EmpWorkingOnsite] (
                        UpdUser,
                        CmpId,
                        AccountId,
                        Customer,
                        SiteName,
                        [Description],
                        TransDate,
                        StartTime,
                        EndTime,
                        EmployeeCode
                    )
                    VALUES (
                        @UpdUser,
                        @CmpId,
                        @AccountId,
                        @Customer,
                        @SiteName,
                        @Description,
                        @TransDate,
                        @StartTime,
                        @EndTime,
                        @EmployeeCode
                    );
                ";

                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add(new SqlParameter("@UpdUser", SqlDbType.NVarChar, 50)
                {
                    Value = (object?)model.UpdUser ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.VarChar, 50) { Value = model.CmpId });
                cmd.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = model.AccountId });
                cmd.Parameters.Add(new SqlParameter("@Customer", SqlDbType.NVarChar, 500) { Value = model.Customer });
                cmd.Parameters.Add(new SqlParameter("@SiteName", SqlDbType.NVarChar, 500)
                {
                    Value = (object?)model.SiteName ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500)
                {
                    Value = (object?)model.Description ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@TransDate", SqlDbType.Date) { Value = transDateParsed });
                cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = startTimeParsed });
                cmd.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.Time) { Value = endTimeParsed });
                cmd.Parameters.Add(new SqlParameter("@EmployeeCode", SqlDbType.NVarChar, 30)
                {
                    Value = (object?)model.EmployeeCode ?? DBNull.Value
                });

                await cmd.ExecuteNonQueryAsync(cancellationToken);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in Create EmpWorkingOnsite for CmpId {CmpId}, AccountId {AccountId}",
                    model.CmpId, model.AccountId);

                return StatusCode(500, $"SQL error: {ex.Message}");
            }
        }

        // --------------------------------------------------------------------
        // PUT: api/EmpWorkingOnsite/{cmpId}/{accountId}/{transDate}/{startTime}
        // --------------------------------------------------------------------
        [HttpPut("{cmpId}/{accountId:int}/{transDate}/{startTime}")]
        public async Task<ActionResult> Update(
            string cmpId,
            int accountId,
            string transDate,
            string startTime,
            [FromBody] EmpWorkingOnsite model,
            CancellationToken cancellationToken)
        {
            if (!TryParseDateInvariant(transDate, out var transDateKey))
                return BadRequest("รูปแบบ TransDate key ไม่ถูกต้อง (ควรเป็น yyyy-MM-dd)");

            if (!TimeSpan.TryParse(startTime, out var startTimeKey))
                return BadRequest("รูปแบบ StartTime key ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");

            if (!TryParseDateInvariant(model.TransDate, out var transDateNew))
                return BadRequest("รูปแบบ TransDate ใน body ไม่ถูกต้อง (ควรเป็น yyyy-MM-dd)");

            if (!TimeSpan.TryParse(model.StartTime, out var startTimeNew))
                return BadRequest("รูปแบบ StartTime ใน body ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");

            if (!TimeSpan.TryParse(model.EndTime, out var endTimeNew))
                return BadRequest("รูปแบบ EndTime ใน body ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");

            try
            {
                await using var conn = _dbFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string sql = @"
                    UPDATE [hr].[EmpWorkingOnsite]
                    SET
                        UpdUser     = @UpdUser,
                        Customer    = @Customer,
                        SiteName    = @SiteName,
                        [Description] = @Description,
                        TransDate   = @TransDateNew,
                        StartTime   = @StartTimeNew,
                        EndTime     = @EndTimeNew,
                        EmployeeCode = @EmployeeCode
                    WHERE CmpId      = @CmpIdKey
                      AND AccountId  = @AccountIdKey
                      AND TransDate  = @TransDateKey
                      AND StartTime  = @StartTimeKey;
                ";

                await using var cmd = new SqlCommand(sql, conn);

                // key
                cmd.Parameters.Add(new SqlParameter("@CmpIdKey", SqlDbType.VarChar, 50) { Value = cmpId });
                cmd.Parameters.Add(new SqlParameter("@AccountIdKey", SqlDbType.Int) { Value = accountId });
                cmd.Parameters.Add(new SqlParameter("@TransDateKey", SqlDbType.Date) { Value = transDateKey });
                cmd.Parameters.Add(new SqlParameter("@StartTimeKey", SqlDbType.Time) { Value = startTimeKey });

                // new values
                cmd.Parameters.Add(new SqlParameter("@UpdUser", SqlDbType.NVarChar, 50)
                {
                    Value = (object?)model.UpdUser ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@Customer", SqlDbType.NVarChar, 500) { Value = model.Customer });
                cmd.Parameters.Add(new SqlParameter("@SiteName", SqlDbType.NVarChar, 500)
                {
                    Value = (object?)model.SiteName ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500)
                {
                    Value = (object?)model.Description ?? DBNull.Value
                });
                cmd.Parameters.Add(new SqlParameter("@TransDateNew", SqlDbType.Date) { Value = transDateNew });
                cmd.Parameters.Add(new SqlParameter("@StartTimeNew", SqlDbType.Time) { Value = startTimeNew });
                cmd.Parameters.Add(new SqlParameter("@EndTimeNew", SqlDbType.Time) { Value = endTimeNew });
                cmd.Parameters.Add(new SqlParameter("@EmployeeCode", SqlDbType.NVarChar, 30)
                {
                    Value = (object?)model.EmployeeCode ?? DBNull.Value
                });

                var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (rows == 0)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in Update EmpWorkingOnsite for CmpId {CmpId}, AccountId {AccountId}, TransDate {TransDate}, StartTime {StartTime}",
                    cmpId, accountId, transDate, startTime);

                return StatusCode(500, "An error occurred while updating data.");
            }
        }

        // --------------------------------------------------------------------
        // DELETE: api/EmpWorkingOnsite/{cmpId}/{accountId}/{transDate}/{startTime}
        // --------------------------------------------------------------------
        [HttpDelete("{cmpId}/{accountId:int}/{transDate}/{startTime}")]
        public async Task<ActionResult> Delete(
            string cmpId,
            int accountId,
            string transDate,
            string startTime,
            CancellationToken cancellationToken)
        {
            if (!TryParseDateInvariant(transDate, out var transDateParsed))
                return BadRequest("รูปแบบ TransDate ไม่ถูกต้อง (ควรเป็น yyyy-MM-dd)");

            if (!TimeSpan.TryParse(startTime, out var startTimeParsed))
                return BadRequest("รูปแบบ StartTime ไม่ถูกต้อง (ควรเป็น HH:mm:ss)");

            try
            {
                await using var conn = _dbFactory.CreateConnection();
                await conn.OpenAsync(cancellationToken);

                const string sql = @"
                    DELETE FROM [hr].[EmpWorkingOnsite]
                    WHERE CmpId     = @CmpId
                      AND AccountId = @AccountId
                      AND TransDate = @TransDate
                      AND StartTime = @StartTime;
                ";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.VarChar, 50) { Value = cmpId });
                cmd.Parameters.Add(new SqlParameter("@AccountId", SqlDbType.Int) { Value = accountId });
                cmd.Parameters.Add(new SqlParameter("@TransDate", SqlDbType.Date) { Value = transDateParsed });
                cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = startTimeParsed });

                var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (rows == 0)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in Delete EmpWorkingOnsite for CmpId {CmpId}, AccountId {AccountId}, TransDate {TransDate}, StartTime {StartTime}",
                    cmpId, accountId, transDate, startTime);

                return StatusCode(500, "An error occurred while deleting data.");
            }
        }
    }
}
