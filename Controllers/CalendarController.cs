using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using goalongapi.Helpers;
using goalongapi.Models;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly EmailSettingRepository _mailRepo;
        private readonly GoogleCalendarApiKeyClient _calendarClient;
        private readonly GoogleOAuthCalendarService _googleOAuthCalendar;

        public CalendarController(EmailSettingRepository mailRepo, GoogleCalendarApiKeyClient calendarClient, GoogleOAuthCalendarService googleOAuthCalendar)
        {
            _mailRepo = mailRepo;
            _calendarClient = calendarClient;
            _googleOAuthCalendar = googleOAuthCalendar;
        }

        [HttpGet("google-events")]
        public async Task<IActionResult> GetGoogleEvents(
            [FromQuery] string? cmpid,
            [FromQuery] string settingName = "nis",
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            try
            {
                var rangeStart = start ?? DateTime.Today.AddMonths(-1);
                var rangeEnd = end ?? DateTime.Today.AddMonths(2);
                if (rangeEnd <= rangeStart) return BadRequest(new { message = "end must be later than start" });
                var events = await _googleOAuthCalendar.GetEventsAsync(cmpid, settingName, rangeStart, rangeEnd);
                return Ok(new { events });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("google-events")]
        public async Task<IActionResult> CreateGoogleEvent([FromBody] GoogleCalendarAppointmentCreateDto dto)
        {
            try
            {
                var appointment = await _googleOAuthCalendar.CreateOrUpdateEventAsync(dto);
                return Created($"google-events/{appointment.GoogleEventId}", appointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("google-events/ticket/{ticketId}")]
        public async Task<IActionResult> UpdateGoogleEventForTicket(string ticketId, [FromBody] GoogleCalendarAppointmentCreateDto dto)
        {
            dto.TicketId = ticketId;
            try
            {
                return Ok(await _googleOAuthCalendar.CreateOrUpdateEventAsync(dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("google-events/ticket/{ticketId}")]
        public async Task<IActionResult> DeleteGoogleEventForTicket(
            string ticketId,
            [FromQuery] string? cmpid,
            [FromQuery] string settingName = "nis")
        {
            try
            {
               // await _googleOAuthCalendar.DeleteEventForTicketAsync(cmpid, settingName, ticketId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Read-only: pulls events from the Google Calendar ID configured on
        // the Mail Setting page (EmailSmtpSettings.CalendarId) via the
        // Calendar API using a project API key (no per-user OAuth). Only
        // works if that calendar's sharing is set to public. Never writes
        // back to Google.
        [HttpGet("[action]")]
        public async Task<IActionResult> getCalendarEventFromMail(
            [FromQuery] string? cmpid,
            [FromQuery] string settingName = "nis",
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            var setting = await _mailRepo.GetActiveAsync(cmpid, settingName);
            if (setting == null || string.IsNullOrWhiteSpace(setting.CalendarId))
            {
                return Ok(new { events = Array.Empty<object>() });
            }

            var rangeStart = start ?? DateTime.UtcNow.AddMonths(-1);
            var rangeEnd = end ?? DateTime.UtcNow.AddMonths(2);

            List<SyncedCalendarEvent> googleEvents;
            try
            {
                googleEvents = await _calendarClient.GetEventsAsync(setting.CalendarId, rangeStart, rangeEnd);
            }
            catch (Exception ex)
            {
                return StatusCode(502, new
                {
                    message = "Failed to fetch calendar from mail account.",
                    detail = ex.Message,
                });
            }

            var events = googleEvents.Select(e => new
            {
                calendarId = "gmail_" + e.Id,
                color = "#00B8D9",
                title = e.Summary,
                allDay = e.IsAllDay,
                description = e.Description,
                start = e.Start,
                end = e.End,
                location = e.Location,
                cmpid,
                username = setting.Username,
                customerName = "",
                ticketId = "",
                invite = Array.Empty<object>(),
                stateReadOnly = 1,
                ticketIdRef = "",
            });

            return Ok(new { events });
        }

        [HttpGet("[action]")]
        public IActionResult getCalendarEvent([FromQuery] string cmpid, [FromQuery] string user)
        {
            DataTable dt = DB.DBConn.GetDataTableParam(
                "exec dbo.getCalendarEvent @CmpId=@CmpId, @user=@user",
                new[]
                {
                    new SqlParameter("@CmpId", (object?)cmpid ?? DBNull.Value),
                    new SqlParameter("@user", (object?)user ?? DBNull.Value),
                });

            DataTable dtAcc = DB.DBConn.GetDataTableParam(
                "exec dbo.getAccountlist @User=@User, @CmpId=@CmpId",
                new[]
                {
                    new SqlParameter("@User", (object?)user ?? DBNull.Value),
                    new SqlParameter("@CmpId", (object?)cmpid ?? DBNull.Value),
                });

            var eventList = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var eventObj = new Dictionary<string, object>();
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName != "invite")
                    {
                        eventObj[column.ColumnName] = row[column];
                    }
                }

                var inviteList = new List<Invite>();
                if (row["invite"]?.ToString() != "")
                {
                    var invites = row["invite"]?.ToString().Split(',');

                    if (invites.Length > 0)
                    {
                        foreach (var invite in invites)
                        {
                            var datain = new Invite();

                            // Find matching account data for each invite
                            foreach (DataRow x in dtAcc.Select("AccountId=" + invite))
                            {
                                datain.UserId = x["AccountId"].ToString();
                                datain.UserName = x["Username"].ToString();
                                datain.FullName = x["FullName"].ToString();
                                datain.ImgPath = x["ImgPath"].ToString();
                            }

                            // Add invite data to the list
                            inviteList.Add(datain);
                        }

                        // Add the invite list to the event object
                        eventObj["invite"] = inviteList;
                    }
                }
                else
                {
                    eventObj["invite"] = inviteList;
                }

                eventList.Add(eventObj);
            }

            return Ok(new { events = eventList });
        }

        [HttpPost("[action]")]
        public IActionResult setCalendarEvent(CalendarModel mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string userIds = string.Join(",", mt.invite.Select(invite => invite.UserId));

                const string _cmd =
                    "exec dbo.setCalendarEvent @user=@user, @cmpid=@cmpid, @calendarid=@calendarid, "
                    + "@color=@color, @allday=@allday, @description=@description, @start=@start, "
                    + "@end=@end, @location=@location, @title=@title, @customerName=@customerName, "
                    + "@ticketId=@ticketId, @invite=@invite, @ticketIdRef=@ticketIdRef";

                var _params = new[]
                {
                    new SqlParameter("@user", (object?)mt.username ?? DBNull.Value),
                    new SqlParameter("@cmpid", (object?)mt.cmpId ?? DBNull.Value),
                    new SqlParameter("@calendarid", (object?)mt.calendarId ?? DBNull.Value),
                    new SqlParameter("@color", (object?)mt.color ?? DBNull.Value),
                    // Preserve the original wire value ("True"/"False") the stored proc expects.
                    new SqlParameter("@allday", mt.allDay.ToString()),
                    new SqlParameter("@description", (object?)mt.description ?? DBNull.Value),
                    new SqlParameter("@start", mt.start.ToString("yyyy-MM-dd HH:mm", thaiCulture)),
                    new SqlParameter("@end", mt.end.ToString("yyyy-MM-dd HH:mm", thaiCulture)),
                    new SqlParameter("@location", (object?)mt.location ?? DBNull.Value),
                    new SqlParameter("@title", (object?)mt.title ?? DBNull.Value),
                    new SqlParameter("@customerName", (object?)mt.customerName ?? DBNull.Value),
                    new SqlParameter("@ticketId", (object?)mt.ticketId ?? DBNull.Value),
                    new SqlParameter("@invite", (object?)userIds ?? DBNull.Value),
                    new SqlParameter("@ticketIdRef", (object?)mt.ticketIdRef ?? DBNull.Value),
                };

                if (DB.DBConn.ExecuteOnlyParam(_cmd, _params))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }


  [HttpPost("[action]")]
        public IActionResult setCalendarEventBusy(CalendarBusyModel mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                 
                const string _cmd =
                    "exec dbo.setCalendarEventBusy @cmpid=@cmpid, @calendarid=@calendarid, @invite=@invite";
                var _params = new[]
                {
                    new SqlParameter("@cmpid", (object?)mt.cmpId ?? DBNull.Value),
                    new SqlParameter("@calendarid", (object?)mt.calendarId ?? DBNull.Value),
                    new SqlParameter("@invite", (object?)mt.invite?.UserId ?? DBNull.Value),
                };

                if (DB.DBConn.ExecuteOnlyParam(_cmd, _params))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }


        [HttpPatch("[action]")]
        public IActionResult delCalendarEvent(calendarDel model)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                // @EventId is referenced by both statements — a single parameter is fine.
                const string _cmd =
                    "delete r from CalendarEvent a "
                    + "inner join CustomerReqTicketRoute r on a.TicketId = r.TicketId and a.CalendarId = r.RemindId "
                    + "and a.CmpId = r.CmpId where r.RouteId = '240002' and a.CalendarId = @EventId; "
                    + "delete from CalendarEvent where CalendarId = @EventId;";

                if (DB.DBConn.ExecuteOnlyParam(_cmd, new SqlParameter("@EventId", (object?)model.EventId ?? DBNull.Value)))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        public class calendarDel
        {
            public string EventId { get; set; }
        }
    }
}
