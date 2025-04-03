using System.Data;
using System.Threading.Tasks;
using goalongapi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Hubs
{
    public class NotificationHub : Hub
    {
        // ฟังก์ชันสำหรับส่งข้อความไปยังไคลเอนต์
        public async Task SendMessage(string cmpid, string userlogin)
        {
            var message = await SendNotifications(cmpid, userlogin);
            await Clients.All.SendAsync($"ReceiveNotification{cmpid}{userlogin}", message);
        }

        public async Task ReceiveMessage(
            string cmpid,
            string userlogin,
            string userFrom,
            string message
        )
        {
            /*  await Clients.All.SendAsync($"ReceiveNotification{cmpid}{userlogin}", message); */
            await SetNotifications(cmpid, userlogin, message, userFrom);
            await SendMessage(cmpid, userlogin);
        }

        public async Task<string> SetNotifications(
            string cmpid,
            string userlogin,
            string message,
            string userfrom
        )
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Notification[]>(message);

                DataTable dt = new System.Data.DataTable();
                string _cmd;
                if (data.Length <= 0)
                    return "";
                _cmd = "exec dbo.[setNotification] @CmpId='" + cmpid + "'";
                _cmd += " ,  @userTo='" + userlogin + "'";
                _cmd += " ,  @userFrom='" + userfrom + "'";
                _cmd += " , @Id='" + data[0].Id.ToString() + "'";
                _cmd += " , @Title='" + data[0].Title.ToString() + "'";
                _cmd += " , @Category='" + data[0].Category.ToString() + "'";
                _cmd += " , @Type='" + data[0].Type.ToString() + "'";
                _cmd += " , @linkTo='" + data[0].urllink.ToString() + "'";
                _cmd += " , @ModuleFormName='" + data[0].ModuleFormName.ToString() + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    return "200";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                // จัดการข้อผิดพลาดที่นี่
                return "";
            }
        }

        // ฟังก์ชันเรียก Stored Procedure อย่างปลอดภัย
        public async Task<string> SendNotifications(string cmpid, string userlogin)
        {
            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd;
                _cmd =
                    "exec dbo.[getNoitfications] @CmpId="
                    + cmpid
                    + " ,  @userlogin='"
                    + userlogin
                    + "'";
                dt = DB.DBConn.GetDataTable(_cmd);
                string qdetail = string.Empty;
                qdetail = JsonConvert.SerializeObject(dt);

                return qdetail;
            }
            catch (Exception ex)
            {
                // จัดการข้อผิดพลาดที่นี่
                return "";
            }
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // ตรวจสอบและ log ค่า query string
            string cmpid = httpContext.Request.Query["cmpid"].ToString();
            string userlogin = httpContext.Request.Query["user"].ToString();

            // เรียก SendMessage ทันทีเมื่อไคลเอนต์เชื่อมต่อ
            await SendMessage(cmpid, userlogin);

            await base.OnConnectedAsync(); 
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
