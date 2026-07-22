using System.Data;
using goalongapi.Models;
using Microsoft.Data.SqlClient;

namespace goalongapi.Helpers;

public sealed class GoogleCalendarEventMappingRepository
{
    private readonly string _connectionString;

    public GoogleCalendarEventMappingRepository(string connectionString) => _connectionString = connectionString;

    public async Task<GoogleCalendarEventMapping?> GetAsync(string? cmpId, string settingName, string ticketId)
    {
        const string sql = @"
SELECT TOP 1 CmpId, SettingName, TicketId, GoogleEventId, CalendarId
FROM dbo.NisGoogleCalendarEventMappings
WHERE SettingName = @SettingName AND TicketId = @TicketId
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId);";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        AddKeyParameters(cmd, cmpId, settingName, ticketId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new GoogleCalendarEventMapping
        {
            CmpId = reader["CmpId"] as string,
            SettingName = (string)reader["SettingName"],
            TicketId = (string)reader["TicketId"],
            GoogleEventId = (string)reader["GoogleEventId"],
            CalendarId = (string)reader["CalendarId"],
        };
    }

    /// <summary>
    /// คืน map จาก GoogleEventId → TicketId ของทุก ticket ที่ sync ขึ้น calendar นี้ —
    /// ใช้เติม TicketId กลับตอน read events (Google ไม่ได้เก็บ ticketId ให้ในตัว event)
    /// เพื่อให้ฝั่ง client แยก event ที่ mirror มาจาก ticket ออกจากนัดหมายทั่วไปได้แม่นยำ
    /// โดยไม่ต้องเดาจาก title.
    /// </summary>
    public async Task<Dictionary<string, string>> GetTicketIdByEventIdAsync(string? cmpId, string settingName)
    {
        const string sql = @"
SELECT GoogleEventId, TicketId
FROM dbo.NisGoogleCalendarEventMappings
WHERE SettingName = @SettingName
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId = @CmpId);";
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CmpId", (object?)cmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", settingName);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eventId = reader["GoogleEventId"] as string;
            var ticketId = reader["TicketId"] as string;
            if (!string.IsNullOrWhiteSpace(eventId) && !string.IsNullOrWhiteSpace(ticketId))
                result[eventId] = ticketId;
        }
        return result;
    }

    public async Task UpsertAsync(GoogleCalendarEventMapping mapping)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.NisGoogleCalendarEventMappings WHERE SettingName=@SettingName AND TicketId=@TicketId AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId=@CmpId))
    UPDATE dbo.NisGoogleCalendarEventMappings
    SET GoogleEventId=@GoogleEventId, CalendarId=@CalendarId, UpdatedAt=SYSUTCDATETIME()
    WHERE SettingName=@SettingName AND TicketId=@TicketId AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId=@CmpId);
ELSE
    INSERT INTO dbo.NisGoogleCalendarEventMappings (CmpId, SettingName, TicketId, GoogleEventId, CalendarId)
    VALUES (@CmpId, @SettingName, @TicketId, @GoogleEventId, @CalendarId);";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        AddKeyParameters(cmd, mapping.CmpId, mapping.SettingName, mapping.TicketId);
        cmd.Parameters.AddWithValue("@GoogleEventId", mapping.GoogleEventId);
        cmd.Parameters.AddWithValue("@CalendarId", mapping.CalendarId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string? cmpId, string settingName, string ticketId)
    {
        const string sql = @"
DELETE FROM dbo.NisGoogleCalendarEventMappings
WHERE SettingName=@SettingName AND TicketId=@TicketId
  AND ((@CmpId IS NULL AND CmpId IS NULL) OR CmpId=@CmpId);";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        AddKeyParameters(cmd, cmpId, settingName, ticketId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddKeyParameters(SqlCommand cmd, string? cmpId, string settingName, string ticketId)
    {
        cmd.Parameters.AddWithValue("@CmpId", (object?)cmpId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SettingName", settingName);
        cmd.Parameters.AddWithValue("@TicketId", ticketId);
    }
}
