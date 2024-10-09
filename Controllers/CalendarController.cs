using coreapi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Net;
using System.Text;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google;
using System.Text.Json;


namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]
    public class CalendarController : ControllerBase
    {


        [HttpGet("[action]")]

        public IActionResult getCalendarEvent([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getCalendarEvent @CmpId='" + cmpid + "' , @user='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            /*  string JSONString = string.Empty;
             JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt); */

            _cmd = "exec dbo.getAccountlist @User='" + user + "' , @CmpId='" + cmpid + "'";
            DataTable dtAcc = DB.DBConn.GetDataTable(_cmd);

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
        public IActionResult setCalendarEvent(Calendar mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string userIds = string.Join(",", mt.invite.Select(invite => invite.UserId));

                string _cmd = "";
                _cmd = "exec  dbo.setCalendarEvent";
                _cmd += " @user  ='" + mt.username + "'";
                _cmd += ",@cmpid ='" + mt.cmpId + "'";
                _cmd += ",@calendarid ='" + mt.calendarId + "'";
                _cmd += ",@color  ='" + mt.color + "'";
                _cmd += ",@allday  ='" + mt.allDay + "'";
                _cmd += ",@description  ='" + mt.description + "'";
                _cmd += ",@start  ='" + mt.start.ToString("yyyy-MM-dd HH:mm", thaiCulture) + "'";
                _cmd += ",@end  ='" + mt.end.ToString("yyyy-MM-dd HH:mm", thaiCulture) + "'";
                _cmd += ",@location  ='" + mt.location + "'";
                _cmd += ",@title='" + mt.title + "'";
                _cmd += ",@customerName  ='" + mt.customerName + "'";
                _cmd += ",@ticketId='" + mt.ticketId + "'";
                _cmd += " ,@invite='" + userIds + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
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






    }


}