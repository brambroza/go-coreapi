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
    [Route("api/[controller]")]
    public class SystemLogController : ControllerBase
    {
        private readonly RabbitMQService _rabbitMQService;

        public SystemLogController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet("[action]")]
        public IActionResult getVersionInfo([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.get_versioninfo @CmpId='" + cmpid + "'  ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            var res = new List<IVersionInfo>();
            foreach (DataRow r in dt.Rows)
            {
                var rd = new IVersionInfo();
                rd.CreateAt = r["CreateAt"].ToString();
                rd.Seq = int.Parse(r["Seq"].ToString());
                rd.Version = r["VersionInfo"].ToString();
                rd.Descriptions = r["Descriptions"].ToString();
                res.Add(rd);


            }



            return Ok(res);
        }

        [HttpPost("[action]")]
        public IActionResult setVersionInfo([FromBody] IVersionInfo info)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd;
                _cmd = "exec dbo.set_versioninfo @CmpId='" + info.CmpId + "'  ";
                _cmd += " , @VersionInfo='" + info.Version + "'";
                _cmd += " , @Description='" + info.Descriptions + "'";


                if (!DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return BadRequest(msgretrun);
                }


                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }
        }






        [HttpPost("[action]")]
        public IActionResult setLogClick([FromBody] LogRequest log)
        {
            _rabbitMQService.SendLog(log);
            return Ok();
        }




        [HttpGet("[action]")]
        public IActionResult getLogClick([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.getLogSystemClick @CmpId='" + cmpid + "', @UserName='" + user + "'";

            var logclicklists = new List<GetLogRequest>();

            dt = DB.DBConn.GetDataTable(_cmd);
            foreach (DataRow r in dt.Rows)
            {
                var logclicklist = new GetLogRequest()
                {
                    Username = r["UserName"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    MenuName = r["MenuName"].ToString(),
                    ObjectName = r["ObjectName"].ToString(),
                };
                logclicklists.Add(logclicklist);
            }

            return Ok(logclicklists);
        }
    }
}
