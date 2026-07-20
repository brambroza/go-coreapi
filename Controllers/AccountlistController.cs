using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class AccountlistController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getAccountlist([FromQuery] string user, [FromQuery] string CmpId, [FromQuery] string serviceteam)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getAccountlist @User='" + user + "' , @CmpId='" + CmpId + "' , @serviceteam='" + serviceteam + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }
    }
}
