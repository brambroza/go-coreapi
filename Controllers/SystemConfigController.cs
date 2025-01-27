using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
 
namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class SystemConfigController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getSystemRoute([FromQuery] string cmpid, [FromQuery] string system)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + cmpid + "', @System='" + system + "'";

            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getSystemEventLog([FromQuery] string cmpid)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.sp_system_getSystemMarketingTickerEvent @CmpId='" + cmpid + "'";

            dt = DB.DBConn.GetDataTable(_cmd);

            List<SystemEventLog> systemlogs = new List<SystemEventLog>();

            foreach (DataRow r in dt.Rows)
            {
                var systemlog = new SystemEventLog()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    Id = r["Id"].ToString(),
                    RepeatEveryId = r["RepeatEveryId"].ToString(),
                    DocNo = r["DocNo"].ToString(),
                    DocType = r["DocType"].ToString(),
                    ExpiresType = r["ExpiresType"].ToString(),
                    EveryDay = DateTime
                        .Parse(r["EveryDay"].ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture),
                    CmpId = r["CmpId"].ToString(),
                    EventName = r["EventName"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    ImgPath = r["ImgPath"].ToString(),
                    Status = Convert.ToInt32(r["Status"].ToString()),

                    Msg = r["Msg"].ToString(),
                    ModifyDate = r["ModifyDate"].ToString(),
                    ModifyBy = r["ModifyBy"].ToString(),
                    DocNoNew = r["DocNoNew"].ToString(),
                };

                systemlogs.Add(systemlog);
            }
            return Ok(systemlogs);
        }

        [HttpPost("[action]")]
        public IActionResult setSystemEventLog([FromBody] SystemEventLog data)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_system_setSystemMarketingTickerEvent @Id='" + data.Id + "'  ";
                _cmd += " ,@CmpId='" + data.CmpId + "'";
                _cmd += " ,@User='" + data.UpdUser + "'";
                _cmd += " ,@DocNo='" + data.DocNo + "'";
                _cmd += " ,@DocType='" + data.DocType + "'";
                _cmd += " ,@Status=" + data.Status;

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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setSystemEventLogDate([FromBody] SystemEventLog data)
        {
            MsgReturn msgretrun = new MsgReturn();
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec  dbo.sp_system_setSystemMarketingTickerEvent_UpdateDate @Id='"
                    + data.Id
                    + "'  ";
                _cmd += " ,@CmpId='" + data.CmpId + "'";
                _cmd += " ,@User='" + data.UpdUser + "'";
                _cmd += " ,@DocNo='" + data.DocNo + "'";
                _cmd += " ,@DocType='" + data.DocType + "'";
                _cmd +=
                    " ,@EventDay='"
                    + DateTime.Parse(data.EveryDay).ToString("yyyy-MM-dd HH:mm", thaiCulture)
                    + "'";

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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult delSystemEventLog([FromQuery] string CmpId, [FromQuery] string Id)
        {
            MsgReturn msgretrun = new MsgReturn();
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string _cmd = "";
                _cmd = " delete dbo.SystemMarketingTickerEvent where  Id='" + Id + "'  ";
                _cmd += " and CmpId='" + CmpId + "'";

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
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }
    }
}
