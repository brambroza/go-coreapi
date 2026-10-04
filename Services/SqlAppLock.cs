using System.Data;
using Microsoft.Data.SqlClient;

namespace goalongapi.Services;

/// <summary>
/// Distributed lock ข้าม replica ด้วย SQL Server <c>sp_getapplock</c> (<c>@LockOwner = 'Session'</c>)
///
/// lock ผูกกับ SQL session ของ connection ที่เปิดค้างไว้ใน handle ที่คืนกลับ — dispose handle
/// (หรือ process ตายทำให้ connection หลุด) = lock ปล่อยเอง ไม่ต้องทำ TTL / renew
/// connection นี้ปิด pooling ตั้งใจ: ถ้าเข้า pool การ dispose แค่คืน connection ไม่ได้จบ session
/// release ที่ล้ม/ถูก cancel จะทิ้ง lock ค้างใน pool แล้วทุกรอบถัดไปขอ lock ไม่ได้จนกว่าจะรีสตาร์ท
///
/// ให้ได้แค่ "ไม่รันพร้อมกันข้าม replica" ในแต่ละรอบ — ไม่ใช่ "รันตัวเดียว" เพราะ lock ปล่อยตอนจบรอบ
/// instance ที่ถึงรอบทีหลังจะได้ lock แล้วรันซ้ำแบบเรียงกัน งานที่ครอบด้วย helper นี้จึงต้อง idempotent
/// (ถ้าวันหน้ามี job ที่รันซ้ำไม่ได้จริง ค่อยเพิ่มโหมดถือ lock ค้างตลอดอายุ process ใน helper นี้)
///
/// ชื่อ class ไม่ลงท้าย <c>Service</c> ตั้งใจ — กัน Autofac auto-register ซ้อน register เองใน Program.cs
/// </summary>
public class SqlAppLock
{
    private readonly string _connectionString;
    private readonly ILogger<SqlAppLock> _logger;

    public SqlAppLock(IConfiguration configuration, ILogger<SqlAppLock> logger)
    {
        var baseConnectionString = configuration.GetConnectionString("ConnectionSQLServer")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'ConnectionSQLServer' not found.");
        // Pooling=false ให้ปิด connection = จบ SQL session จริง lock หลุดแน่นอน
        // ApplicationName ให้เห็นใน sys.dm_exec_sessions ว่า session ไหนเป็นของ lock
        _connectionString = new SqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = false,
            ApplicationName = "goalongapi-applock",
        }.ConnectionString;
        _logger = logger;
    }

    /// <summary>
    /// ขอ lock แบบไม่รอ (<c>@LockTimeout = 0</c>)
    /// คืน handle ที่ต้อง <c>await using</c> ถ้าได้ lock, คืน <c>null</c> ถ้า instance อื่นถืออยู่
    /// ถ้าเปิด connection / เรียก SP ไม่ได้ จะ throw ให้ caller จัดการ (ถือว่ารอบนั้นล้ม)
    /// </summary>
    /// <param name="resource">ชื่อ lock ไม่เกิน 255 ตัวอักษร เช่น <c>goalongapi:nis-overdue-push</c></param>
    /// <param name="ct">cancellation token ของ BackgroundService</param>
    public async Task<IAsyncDisposable?> TryAcquireAsync(string resource, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resource) || resource.Length > 255)
            throw new ArgumentException("resource ต้องไม่ว่างและยาวไม่เกิน 255 ตัวอักษร", nameof(resource));

        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = "sp_getapplock";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Resource", SqlDbType.NVarChar, 255) { Value = resource });
            command.Parameters.Add(new SqlParameter("@LockMode", SqlDbType.VarChar, 32) { Value = "Exclusive" });
            command.Parameters.Add(new SqlParameter("@LockOwner", SqlDbType.VarChar, 32) { Value = "Session" });
            command.Parameters.Add(new SqlParameter("@LockTimeout", SqlDbType.Int) { Value = 0 });
            var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue,
            };
            command.Parameters.Add(returnValue);

            await command.ExecuteNonQueryAsync(ct);

            // >= 0 ได้ lock (0 ทันที, 1 หลังรอ) · -1 timeout (คนอื่นถือ) · -2 cancel · -3 deadlock victim · -999 param error
            var result = returnValue.Value is int code ? code : -999;
            if (result >= 0)
                return new Handle(connection, resource, _logger);

            if (result != -1)
                _logger.LogWarning("SqlAppLock: sp_getapplock '{Resource}' คืนค่า {Code}", resource, result);

            await connection.DisposeAsync();
            return null;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// ถือ connection ที่เป็นเจ้าของ lock — dispose แล้วพยายาม <c>sp_releaseapplock</c> ก่อนปิด connection
    /// ถ้า release ไม่สำเร็จ (connection ตาย / ถูก cancel) ไม่เป็นไร เพราะ Pooling=false ปิด connection = จบ session = lock หลุด
    /// </summary>
    private sealed class Handle : IAsyncDisposable
    {
        private readonly SqlConnection _connection;
        private readonly string _resource;
        private readonly ILogger _logger;
        private bool _disposed;

        public Handle(SqlConnection connection, string resource, ILogger logger)
        {
            _connection = connection;
            _resource = resource;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = "sp_releaseapplock";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Resource", SqlDbType.NVarChar, 255) { Value = _resource });
                command.Parameters.Add(new SqlParameter("@LockOwner", SqlDbType.VarChar, 32) { Value = "Session" });
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "SqlAppLock: release '{Resource}' ไม่สำเร็จ — lock จะหลุดเองเมื่อ connection ปิด", _resource);
            }
            finally
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
