using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;



namespace coreapi.Controllers
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

 
    }


}