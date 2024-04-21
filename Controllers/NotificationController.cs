using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using goalongapi.Installers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getnotification([FromQuery] string cmpid, [FromQuery] string userlogin)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getNoitfication] @CmpId=" + cmpid + " ,  @userlogin='" + userlogin + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);

        }

        [HttpPost("[action]")]
        public IActionResult setReadNotification(setNotitfication noti)
        {
            string _cmd = "";
            _cmd = "exec  dbo.setReadNotification";
            _cmd += "  @userlogin  ='" + noti.userlogin + "'";
            _cmd += " , @CmpId='" + noti.cmpid + "'";



            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();

        }



    }
}