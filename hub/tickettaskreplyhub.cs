using System.Data;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Hubs
{
    public class TicketTaskReplyHub : Hub
    {
          public async Task SendMessage(CustomerReqTicketRouteReply mt)
        {
            var message = await SendReplyMsg(mt.CmpId, mt.UpdUser);
            await Clients.All.SendAsync($"ReceiveTicketTaskReply{mt.CmpId}{mt.TicketId}{mt.RouteId}{mt.RemindId}", message);
        } 

        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }

     public async Task<List<CustomerReqTicketRouteReply>> SendReplyMsg(string cmpid, string userlogin)
{
    try
    {
        DataTable dtRoute = DB.DBConn.GetDataTable(
            $"exec dbo.[getReqFromCustomerRoute] @user='{userlogin}', @cmpid='{cmpid}'");

        DataTable dtRouteReply = DB.DBConn.GetDataTable(
            $"exec dbo.[getReqFromCustomerRoute_Reply] @user='{userlogin}', @cmpid='{cmpid}'");

        List<CustomerReqTicketRouteReply> item = new List<CustomerReqTicketRouteReply>();

        foreach (DataRow i in dtRoute.Rows)
        {
            foreach (
                DataRow a in dtRouteReply.Select($"TicketId='{i["TicketId"]}' and RouteId='{i["RouteId"]}'")
            )
            {
                var itemr = new CustomerReqTicketRouteReply
                {
                    CmpId = a["CmpId"].ToString(),
                    TicketId = a["TicketId"].ToString(),
                    UpdUser = a["updUser"].ToString(),
                    FileUrl = a["FileUrl"].ToString(),
                    Comment = a["Comment"].ToString(),
                    RouteId = a["RouteId"].ToString(),
                    RemindId = a["RemindId"].ToString(),
                    createAt = DateTime.Parse(a["createAt"].ToString()),
                    Seq = int.Parse(a["Seq"].ToString()),
                    ImgPath = a["ImgPath"].ToString()
                };

                item.Add(itemr);
            }

            // new comment
            item.Add(new CustomerReqTicketRouteReply
            {
                CmpId = cmpid,
                TicketId = i["TicketId"].ToString(),
                UpdUser = "",
                FileUrl = "",
                Comment = "",
                RouteId = i["RouteId"].ToString(),
                RemindId = i["RemindId"].ToString(),
                createAt = DateTime.Now.AddMinutes(1),
                Seq = 99999999,
                ImgPath = ""
            });
        }

        return item;
    }
    catch (Exception ex)
    {
        // Log หรือ throw error กลับ
        throw new Exception("SendReplyMsg failed: " + ex.Message, ex);
    }
}



         public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // ตรวจสอบและ log ค่า query string
            string cmpid = httpContext.Request.Query["cmpid"].ToString();
            string userlogin = httpContext.Request.Query["user"].ToString();
            string ticketId = httpContext.Request.Query["ticketId"].ToString();
            string routeId = httpContext.Request.Query["routeId"].ToString();
            string remindId = httpContext.Request.Query["remindId"].ToString();

            // เรียก SendMessage ทันทีเมื่อไคลเอนต์เชื่อมต่อ
            var data = new CustomerReqTicketRouteReply();
            data.CmpId = cmpid;
            data.UpdUser = userlogin;
            data.TicketId = ticketId;
            data.RouteId = routeId;
            data.RemindId = remindId;
            data.Seq = 0;
            data.Comment = "";
            data.FileUrl = "";
            data.createAt = DateTime.Now;
            data.ImgPath = "";
            
            await SendMessage(data);

            await base.OnConnectedAsync(); 
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

    }
}
