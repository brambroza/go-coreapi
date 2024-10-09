using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using goalongapi.Installers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using coreapi.Hubs;


namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly IHubContext<NotificationHub> _hubContext;

        // Inject IHubContext to communicate with the Hub
        public NotificationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

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


        [HttpPost("sendFromDB")]
        public async Task<IActionResult> SendNotifications([FromQuery] string cmpid  , [FromQuery] string userlogin)
        {
            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd;
                _cmd = "exec dbo.[getNoitfications] @CmpId=" + cmpid + " ,  @userlogin='" + userlogin + "'";
                dt = DB.DBConn.GetDataTable(_cmd);

                foreach (DataRow notification in dt.Rows)
                {
                    // ส่งข้อความไปยังทุกคนที่เชื่อมต่อกับ Hub
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", notification);
                }

                return Ok(new { Message = "Notifications sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error while sending notifications", Error = ex.Message });
            }
        }







    }
}