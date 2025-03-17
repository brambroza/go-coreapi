using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Interfaces;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    public class ReasonTicketController : ControllerBase
    {


        [HttpPost("[action]")]
        public ActionResult setReasonCloseticket([FromBody] ReasonCloseTicket ma
             )
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                        "th-TH"
                    );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();


            try
            {
                string _cmd = "exec dbo.set_reasoncloseticket";
                _cmd += $" @User='{ma.updUser}',";
                _cmd += $"@Reason='{ma.Reason}',";
                _cmd += $"@CmpId={ma.CmpId}";
                _cmd += $" , @TicketId='{ma.TicketId}'";
                _cmd += $" , @type='{ma.type}'";
                _cmd += $" , @NotificationAgain='{ma.NotificationAgain.ToString("yyyy-MM-dd HH:mm", thaiCulture)}'";
                _cmd += $" ,@ExpiresDate  ='" + ma.ExpiresDate.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += $" ,@EveryDay  ='" + ma.EveryDay.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += $" ,@ExpiresCount =" + ma.ExpiresCount;
                _cmd += $" ,@IntervalType =" + ma.IntervalType;
                _cmd += $" ,@RecurringEvery =" + ma.RecurringEvery;
                _cmd += $" ,@ExpiresType  ='" + ma.ExpiresType + "'";


                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while saving data.", Details = ex.Message });
            }
        }


        [HttpPost("setSeasonTicket")]
        public ActionResult setReasonticket([FromBody] ReasonTicket ma
        )
        {


            try
            {
                string _cmd = "exec dbo.set_ReasonTicket";
                _cmd += $" @User='{ma.updUser}',";
                _cmd += $"@Reason='{ma.Reason}',";
                _cmd += $"@CmpId={ma.CmpId}";

                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while saving data.", Details = ex.Message });
            }
        }


        [HttpGet("[action]")]
        public ActionResult getReasonTicket([FromQuery] string cmpid)
        {
            string _cmd;
            List<ReasonTicket> reasons = new List<ReasonTicket>();

            _cmd = "exec dbo.get_ReasonTicket @CmpId='" + cmpid + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);


            foreach (DataRow r in datatable.Rows)
            {
                var reason = new ReasonTicket();

                reason.updUser = r["UpdUser"].ToString();
                reason.CmpId = r["CmpId"].ToString();
                reason.Reason = r["Reason"].ToString();


                reasons.Add(reason);
            }

            return Ok(reasons);
        }


    }
}
