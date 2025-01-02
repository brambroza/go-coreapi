using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
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
