using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using goalongapi.Models.Trial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using goalongapi.Hubs;
using System.Globalization;


namespace goalongapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CrmController : ControllerBase
    {

        private readonly IHubContext<TicketTaskReplyHub> _hubContext;

        public CrmController(IHubContext<TicketTaskReplyHub> hubContext)
        {
            _hubContext = hubContext;
        }



        [HttpGet("[action]")]
        public IActionResult getreqfromcustlist(
            [FromQuery] string userlogin,
            [FromQuery] string cmpid
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtAssign = new System.Data.DataTable();
            DataTable dtAssignDisplay = new System.Data.DataTable();
            DataTable dtComment = new DataTable();
            DataTable dtCommentReply = new DataTable();
            DataTable dtOwner = new DataTable();
            DataTable dtRoute = new DataTable();
            DataTable dtRouteReply = new DataTable();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            string _cmd;
            _cmd =
                "exec dbo.[getReqFromCustomer] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerItem] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerAssign] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtAssign = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerAssignDisplay] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtAssignDisplay = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerOwner] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtOwner = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerRoute] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerRoute_Reply] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtRouteReply = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[sp_getManageReqComment] @Operation='COMMENT' ,   @cmpid='" + cmpid + "'";
            dtComment = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[sp_getManageReqComment] @Operation='REPLY' ,   @cmpid='" + cmpid + "'";
            dtCommentReply = DB.DBConn.GetDataTable(_cmd);

            List<ReqFromCustList> crms = new List<ReqFromCustList>();

            foreach (DataRow r in dt.Rows)
            {
                var crm = new ReqFromCustList();
                crm.CmpId = r["CmpId"].ToString();
                crm.TicketId = r["TicketId"].ToString();
                crm.ticketIdRef = r["ticketIdRef"].ToString();
                crm.ServiceType = r["ServiceType"].ToString();
                crm.CustomerName = r["CustomerName"].ToString();
                crm.ContactName = r["ContactName"].ToString();
                crm.ContactPhone = r["ContactPhone"].ToString();
                crm.ContactEmail = r["ContactEmail"].ToString();
                crm.Address = r["Address"].ToString();
                crm.CreateAt = DateTime.Parse(r["CreateAt"].ToString());
                crm.Status = r["Status"].ToString();
                crm.FromApp = r["FromApp"].ToString();
                crm.todo = r["todo"].ToString();
                crm.AdditionalDetail = r["AdditionalDetail"].ToString();
                crm.AdditionalDetail2 = r["AdditionalDetail2"].ToString();
                crm.completepercent = decimal.Parse(r["completepercent"].ToString());
                crm.DueDate = DateTime
                    .Parse(r["DueDate"].ToString())
                    .ToString("yyyy-MM-dd HH:mm", thaiCulture);
                crm.UpdUser = r["updUser"].ToString();
                crm.ticketIdRef = r["TicketIdRef"].ToString();
                crm.Priority = r["Priority"].ToString();
                crm.CustomerImgPath = r["CustomerImgPath"].ToString();
                crm.TaskUnRead = int.Parse(r["taskUnRead"].ToString());
                crm.TodoStatus = int.Parse(r["TodoStatus"].ToString());
                crm.Labels = r["Labels"].ToString();
                crm.GrandAmt = decimal.Parse(r["GrandAmt"].ToString());
                crm.StateNotificationList = r["StateNotificationList"].ToString();


                crm.ReqRoute = new List<CustomerReqTicketRoute>();
                foreach (
                    DataRow i in dtRoute.Select(" TicketId='" + r["TicketId"].ToString() + "'  ")
                )
                {
                    var item = new CustomerReqTicketRoute();
                    item.CmpId = i["CmpId"].ToString();
                    item.TicketId = i["TicketId"].ToString();
                    item.RouteId = i["RouteId"].ToString();
                    item.RemindId = i["RemindId"].ToString();
                    item.RouteIdBefore = i["RouteIdBefore"].ToString();
                    item.StatusFinish = Int16.Parse(i["StatusFinish"].ToString());
                    item.DueDate = DateTime.Parse(i["DueDate"].ToString());
                    item.RouteName = i["RouteName"].ToString();
                    item.Department = i["Department"].ToString();
                    item.RemideDescription = i["RemideDescription"].ToString();
                    item.Seq = Int16.Parse(i["Seq"].ToString());
                    item.DateFinish = "";
                    item.UserFinish = "";
                    item.UpdUser = i["UpdUser"].ToString();

                    item.reqReply = new List<CustomerReqTicketRouteReply>();

                    foreach (
                        DataRow a in dtRouteReply.Select(
                            "TicketId='"
                                + item.TicketId
                                + "' and RemindId='"
                                + item.RemindId
                                + "' and RouteId='"
                                + item.RouteId
                                + "'"
                        )
                    )
                    {
                        var itemr = new CustomerReqTicketRouteReply();
                        itemr.CmpId = a["CmpId"].ToString();
                        itemr.TicketId = a["TicketId"].ToString();
                        itemr.UpdUser = a["updUser"].ToString();
                        itemr.FileUrl = a["FileUrl"].ToString();
                        itemr.Comment = a["Comment"].ToString();
                        itemr.RouteId = item.RouteId.ToString();
                        itemr.RemindId = item.RemindId.ToString();
                        itemr.createAt = DateTime.Parse(a["createAt"].ToString());
                        itemr.Seq = int.Parse(a["Seq"].ToString());
                        itemr.ImgPath = a["ImgPath"].ToString();

                        item.reqReply.Add(itemr);
                    }




                    item.reqAssign = new List<ReqFromCustAssign>();

                    foreach (
                        DataRow a in dtAssign.Select(
                            "TicketId='"
                                + item.TicketId
                                + "' and RemindId='"
                                + item.RemindId
                                + "' and RouteId='"
                                + item.RouteId
                                + "'"
                        )
                    )
                    {
                        var itema = new ReqFromCustAssign();
                        itema.CmpId = a["CmpId"].ToString();
                        itema.TicketId = a["TicketId"].ToString();
                        itema.UserFullName = a["FullName"].ToString();
                        itema.ImgPath = a["ImgPath"].ToString();
                        itema.Permission = a["Permission"].ToString();
                        itema.RouteId = item.RouteId.ToString();
                        itema.RemindId = item.RemindId.ToString();
                        itema.UserId = a["UserId"].ToString();

                        item.reqAssign.Add(itema);
                    }

                    crm.ReqRoute.Add(item);
                }

                crm.ReqOwner = new List<ReqFromCustOwner>();
                foreach (
                    DataRow i in dtOwner.Select(
                        " TicketId='"
                            + r["TicketId"].ToString()
                            + "' and  CmpId='"
                            + r["CmpId"].ToString()
                            + "'"
                    )
                )
                {
                    var item = new ReqFromCustOwner();
                    item.CmpId = i["CmpId"].ToString();
                    item.TicketId = i["TicketId"].ToString();
                    item.UserFullName = i["FullName"].ToString();
                    item.ImgPath = i["ImgPath"].ToString();
                    item.UserId = i["UserId"].ToString();


                    crm.ReqOwner.Add(item);
                }

                crm.ReqItem = new List<ReqFromCustItem>();


                foreach (
                    DataRow i in dtItem.Select(
                        " TicketId='"
                            + r["TicketId"].ToString()
                            + "' and  CmpId='"
                            + r["CmpId"].ToString()
                            + "'"
                    )
                )
                {
                    var item = new ReqFromCustItem();
                    item.CmpId = i["CmpId"].ToString();
                    item.TicketId = i["TicketId"].ToString();
                    item.ServiceType = i["ServiceType"].ToString();
                    item.ModelName = i["ModelName"].ToString();
                    item.SerialNo = i["SerialNo"].ToString();
                    item.PartNo = i["PartNo"].ToString();

                    item.Forticloud = i["Forticloud"].ToString();
                    item.MABy = i["MABy"].ToString();
                    item.MADuration = i["MADuration"].ToString();
                    item.AdvanceReplacement = i["AdvanceReplacement"].ToString() ?? string.Empty;

                    item.SLA = i["SLA"].ToString();
                    item.AdditionalDetail = i["AdditionalDetail"].ToString();
                    item.AdditionalDetail2 = i["AdditionalDetail2"].ToString();
                    item.DesiredService = i["DesiredService"].ToString();
                    item.FileUrl = i["FIleUrl"].ToString();
                    item.Seq = int.Parse(i["Seq"].ToString());
                    item.FileUrl1 = i["FIleUrl1"].ToString();
                    crm.ReqItem.Add(item);
                }

                crm.ReqAssign = new List<ReqFromCustAssign>();
                foreach (
                    DataRow i in dtAssignDisplay.Select(
                        " TicketId='"
                            + r["TicketId"].ToString()
                            + "' and  CmpId='"
                            + r["CmpId"].ToString()
                            + "'"
                    )
                )
                {
                    var item = new ReqFromCustAssign();
                    item.CmpId = i["CmpId"].ToString();
                    item.TicketId = i["TicketId"].ToString();
                    item.UserFullName = i["FullName"].ToString();
                    item.ImgPath = i["ImgPath"].ToString();
                    item.Permission = i["Permission"].ToString();
                    item.UserId = i["UserId"].ToString();
                    item.RemindId = i["RemindId"].ToString();
                    item.RouteId = i["RouteId"].ToString();

                    crm.ReqAssign.Add(item);
                }

                crm.ReqComments = new List<ReqComment>();
                foreach (
                    DataRow i in dtComment.Select(
                        " TicketId='"
                            + r["TicketId"].ToString()
                            + "' and  CmpId='"
                            + r["CmpId"].ToString()
                            + "'"
                    )
                )
                {
                    var comment = new ReqComment();

                    comment.CmpId = i["CmpId"].ToString();
                    comment.CommentId = i["CommentId"].ToString();
                    comment.TicketId = i["TicketId"].ToString();
                    comment.Id = i["Id"].ToString();
                    comment.Name = i["Name"].ToString();
                    comment.AvatarUrl = i["AvatarUrl"].ToString();
                    comment.Message = i["Message"].ToString();
                    comment.PostedAt = DateTime.Parse(i["PostedAt"].ToString());
                    comment.replyComment = new List<ReplyComment>();
                    foreach (
                        DataRow x in dtCommentReply.Select(
                            "TicketId='"
                                + i["TicketId"].ToString()
                                + "' and CmpId='"
                                + i["CmpId"].ToString()
                                + "' and CommentId='"
                                + i["CommentId"].ToString()
                                + "'"
                        )
                    )
                    {
                        var reply = new ReplyComment();
                        reply.CmpId = x["CmpId"].ToString();
                        reply.CommentId = x["Comment"].ToString();
                        reply.TicketId = x["TicketId"].ToString();
                        reply.Id = x["Id"].ToString();
                        reply.UserId = x["UserId"].ToString();
                        reply.Message = x["Message"].ToString();

                        reply.TagUser = x["TagUser"].ToString();
                        reply.PostedAt = DateTime.Parse(i["PostedAt"].ToString());

                        comment.replyComment.Add(reply);
                    }

                    crm.ReqComments.Add(comment);
                }

                crms.Add(crm);
            }

            return Ok(new { tickets = crms });
        }




        [HttpGet("[action]")]
        public IActionResult getreqfromcustlistNew([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            // *** แนะนำ: culture ไม่จำเป็นถ้า format เป็น yyyy-MM-dd HH:mm
            // ใช้ InvariantCulture เร็วและชัดเจนกว่า
            var fmt = CultureInfo.InvariantCulture;

            // 1) Load DataTables (เหมือนเดิม)
            DataTable dt = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomer] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtItem = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerItem] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtAssign = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerAssign] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtAssignDisplay = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerAssignDisplay] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtOwner = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerOwner] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtRoute = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerRoute] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtRouteReply = DB.DBConn.GetDataTable($"exec dbo.[getReqFromCustomerRoute_Reply] @user='{userlogin}', @cmpid='{cmpid}'");
            DataTable dtComment = DB.DBConn.GetDataTable($"exec dbo.[sp_getManageReqComment] @Operation='COMMENT', @cmpid='{cmpid}'");
            DataTable dtCommentReply = DB.DBConn.GetDataTable($"exec dbo.[sp_getManageReqComment] @Operation='REPLY', @cmpid='{cmpid}'");

            // 2) Helper: อ่านค่าแบบเร็ว/กัน DBNull
            static string S(DataRow r, string col) => r[col] == DBNull.Value ? "" : r[col].ToString();
            static int I(DataRow r, string col) => r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]);
            static short SH(DataRow r, string col) => r[col] == DBNull.Value ? (short)0 : Convert.ToInt16(r[col]);
            static decimal D(DataRow r, string col) => r[col] == DBNull.Value ? 0m : Convert.ToDecimal(r[col], CultureInfo.InvariantCulture);
            static DateTime? DT(DataRow r, string col) => r[col] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[col], CultureInfo.InvariantCulture);

            // 3) สร้าง Key สำหรับ Lookup (เร็วกว่า Select filter string)
            // TicketKey: ใช้ใน item/owner/assignDisplay/comment
            static string TK(string cmp, string ticket) => $"{cmp}||{ticket}";
            // RouteKey: ใช้ใน routeReply/assign (ตาม TicketId+RemindId+RouteId)
            static string RK(string ticket, string remind, string route) => $"{ticket}||{remind}||{route}";
            // CommentKey: ใช้ใน commentReply
            static string CK(string cmp, string ticket, string commentId) => $"{cmp}||{ticket}||{commentId}";

            // 4) ทำ Lookup ครั้งเดียว
            var routeByTicket = dtRoute.AsEnumerable()
                .GroupBy(x => S(x, "TicketId"))
                .ToDictionary(g => g.Key, g => g.ToList());

            var routeReplyByRouteKey = dtRouteReply.AsEnumerable()
                .GroupBy(x => RK(S(x, "TicketId"), S(x, "RemindId"), S(x, "RouteId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var assignByRouteKey = dtAssign.AsEnumerable()
                .GroupBy(x => RK(S(x, "TicketId"), S(x, "RemindId"), S(x, "RouteId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var ownerByTicketKey = dtOwner.AsEnumerable()
                .GroupBy(x => TK(S(x, "CmpId"), S(x, "TicketId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var itemByTicketKey = dtItem.AsEnumerable()
                .GroupBy(x => TK(S(x, "CmpId"), S(x, "TicketId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var assignDisplayByTicketKey = dtAssignDisplay.AsEnumerable()
                .GroupBy(x => TK(S(x, "CmpId"), S(x, "TicketId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var commentByTicketKey = dtComment.AsEnumerable()
                .GroupBy(x => TK(S(x, "CmpId"), S(x, "TicketId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            var commentReplyByCommentKey = dtCommentReply.AsEnumerable()
                .GroupBy(x => CK(S(x, "CmpId"), S(x, "TicketId"), S(x, "CommentId")))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 5) Map -> Object (ไม่มีลูปซ้อน Select อีกแล้ว)
            var crms = new List<ReqFromCustList>(dt.Rows.Count);

            foreach (DataRow r in dt.Rows)
            {
                var cmp = S(r, "CmpId");
                var ticket = S(r, "TicketId");
                var tkey = TK(cmp, ticket);

                var crm = new ReqFromCustList
                {
                    CmpId = cmp,
                    TicketId = ticket,
                    ticketIdRef = S(r, "TicketIdRef"),
                    ServiceType = S(r, "ServiceType"),
                    CustomerName = S(r, "CustomerName"),
                    ContactName = S(r, "ContactName"),
                    ContactPhone = S(r, "ContactPhone"),
                    ContactEmail = S(r, "ContactEmail"),
                    Address = S(r, "Address"),
                    CreateAt = DT(r, "CreateAt") ?? DateTime.MinValue,
                    Status = S(r, "Status"),
                    FromApp = S(r, "FromApp"),
                    todo = S(r, "todo"),
                    AdditionalDetail = S(r, "AdditionalDetail"),
                    AdditionalDetail2 = S(r, "AdditionalDetail2"),
                    completepercent = D(r, "completepercent"),
                    DueDate = (DT(r, "DueDate") is DateTime due) ? due.ToString("yyyy-MM-dd HH:mm", fmt) : "",
                    UpdUser = S(r, "updUser"),
                    Priority = S(r, "Priority"),
                    CustomerImgPath = S(r, "CustomerImgPath"),
                    TaskUnRead = I(r, "taskUnRead"),
                    TodoStatus = I(r, "TodoStatus"),
                    Labels = S(r, "Labels"),
                    GrandAmt = D(r, "GrandAmt"),
                    StateNotificationList = S(r, "StateNotificationList"),
                };

                // Routes + replies + assigns
                crm.ReqRoute = new List<CustomerReqTicketRoute>();
                if (routeByTicket.TryGetValue(ticket, out var routes))
                {
                    foreach (var i in routes)
                    {
                        var routeId = S(i, "RouteId");
                        var remindId = S(i, "RemindId");
                        var rkey = RK(ticket, remindId, routeId);

                        var route = new CustomerReqTicketRoute
                        {
                            CmpId = S(i, "CmpId"),
                            TicketId = S(i, "TicketId"),
                            RouteId = routeId,
                            RemindId = remindId,
                            RouteIdBefore = S(i, "RouteIdBefore"),
                            StatusFinish = SH(i, "StatusFinish"),
                            DueDate = DT(i, "DueDate") ?? DateTime.MinValue,
                            RouteName = S(i, "RouteName"),
                            Department = S(i, "Department"),
                            RemideDescription = S(i, "RemideDescription"),
                            Seq = SH(i, "Seq"),
                            DateFinish = "",
                            UserFinish = "",
                            UpdUser = S(i, "UpdUser"),
                        };

                        // Route replies
                        route.reqReply = new List<CustomerReqTicketRouteReply>();
                        if (routeReplyByRouteKey.TryGetValue(rkey, out var replies))
                        {
                            foreach (var a in replies)
                            {
                                route.reqReply.Add(new CustomerReqTicketRouteReply
                                {
                                    CmpId = S(a, "CmpId"),
                                    TicketId = S(a, "TicketId"),
                                    UpdUser = S(a, "updUser"),
                                    FileUrl = S(a, "FileUrl"),
                                    Comment = S(a, "Comment"),
                                    RouteId = routeId,
                                    RemindId = remindId,
                                    createAt = DT(a, "createAt") ?? DateTime.MinValue,
                                    Seq = I(a, "Seq"),
                                    ImgPath = S(a, "ImgPath"),
                                });
                            }
                        }

                        // Route assigns
                        route.reqAssign = new List<ReqFromCustAssign>();
                        if (assignByRouteKey.TryGetValue(rkey, out var assigns))
                        {
                            foreach (var a in assigns)
                            {
                                route.reqAssign.Add(new ReqFromCustAssign
                                {
                                    CmpId = S(a, "CmpId"),
                                    TicketId = S(a, "TicketId"),
                                    UserFullName = S(a, "FullName"),
                                    ImgPath = S(a, "ImgPath"),
                                    Permission = S(a, "Permission"),
                                    RouteId = routeId,
                                    RemindId = remindId,
                                    UserId = S(a, "UserId"),
                                });
                            }
                        }

                        crm.ReqRoute.Add(route);
                    }
                }

                // Owners
                crm.ReqOwner = new List<ReqFromCustOwner>();
                if (ownerByTicketKey.TryGetValue(tkey, out var owners))
                {
                    foreach (var i in owners)
                    {
                        crm.ReqOwner.Add(new ReqFromCustOwner
                        {
                            CmpId = S(i, "CmpId"),
                            TicketId = S(i, "TicketId"),
                            UserFullName = S(i, "FullName"),
                            ImgPath = S(i, "ImgPath"),
                            UserId = S(i, "UserId"),
                        });
                    }
                }

                // Items
                crm.ReqItem = new List<ReqFromCustItem>();
                if (itemByTicketKey.TryGetValue(tkey, out var items))
                {
                    foreach (var i in items)
                    {
                        crm.ReqItem.Add(new ReqFromCustItem
                        {
                            CmpId = S(i, "CmpId"),
                            TicketId = S(i, "TicketId"),
                            ServiceType = S(i, "ServiceType"),
                            ModelName = S(i, "ModelName"),
                            SerialNo = S(i, "SerialNo"),
                            PartNo = S(i, "PartNo"),
                            Forticloud = S(i, "Forticloud"),
                            MABy = S(i, "MABy"),
                            MADuration = S(i, "MADuration"),
                            AdvanceReplacement = S(i, "AdvanceReplacement") ?? "",
                            SLA = S(i, "SLA"),
                            AdditionalDetail = S(i, "AdditionalDetail"),
                            AdditionalDetail2 = S(i, "AdditionalDetail2"),
                            DesiredService = S(i, "DesiredService"),
                            FileUrl = S(i, "FIleUrl"),
                            Seq = I(i, "Seq"),
                            FileUrl1 = S(i, "FIleUrl1"),
                        });
                    }
                }

                // Assign display
                crm.ReqAssign = new List<ReqFromCustAssign>();
                if (assignDisplayByTicketKey.TryGetValue(tkey, out var assignsDisp))
                {
                    foreach (var i in assignsDisp)
                    {
                        crm.ReqAssign.Add(new ReqFromCustAssign
                        {
                            CmpId = S(i, "CmpId"),
                            TicketId = S(i, "TicketId"),
                            UserFullName = S(i, "FullName"),
                            ImgPath = S(i, "ImgPath"),
                            Permission = S(i, "Permission"),
                            UserId = S(i, "UserId"),
                            RemindId = S(i, "RemindId"),
                            RouteId = S(i, "RouteId"),
                        });
                    }
                }

                // Comments + replies
                crm.ReqComments = new List<ReqComment>();
                if (commentByTicketKey.TryGetValue(tkey, out var comments))
                {
                    foreach (var i in comments)
                    {
                        var commentId = S(i, "CommentId");
                        var ckey = CK(S(i, "CmpId"), S(i, "TicketId"), commentId);

                        var comment = new ReqComment
                        {
                            CmpId = S(i, "CmpId"),
                            CommentId = commentId,
                            TicketId = S(i, "TicketId"),
                            Id = S(i, "Id"),
                            Name = S(i, "Name"),
                            AvatarUrl = S(i, "AvatarUrl"),
                            Message = S(i, "Message"),
                            PostedAt = DT(i, "PostedAt") ?? DateTime.MinValue,
                            replyComment = new List<ReplyComment>()
                        };

                        if (commentReplyByCommentKey.TryGetValue(ckey, out var creplies))
                        {
                            foreach (var x in creplies)
                            {
                                comment.replyComment.Add(new ReplyComment
                                {
                                    CmpId = S(x, "CmpId"),
                                    CommentId = S(x, "CommentId"),   // (เดิมคุณใส่ x["Comment"] น่าจะผิดคอลัมน์)
                                    TicketId = S(x, "TicketId"),
                                    Id = S(x, "Id"),
                                    UserId = S(x, "UserId"),
                                    Message = S(x, "Message"),
                                    TagUser = S(x, "TagUser"),
                                    PostedAt = DT(x, "PostedAt") ?? DateTime.MinValue // (เดิมใช้ PostedAt ของ comment)
                                });
                            }
                        }

                        crm.ReqComments.Add(comment);
                    }
                }

                crms.Add(crm);
            }

            return Ok(new { tickets = crms });
        }


        [HttpGet("[action]")]
        public IActionResult getreqfromcustkanban(
            [FromQuery] string userlogin,
            [FromQuery] string cmpid
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtAssign = new System.Data.DataTable();
            DataTable dtComment = new DataTable();
            DataTable dtCommentReply = new DataTable();
            DataTable dtOwner = new DataTable();
            DataTable dtRoute = new DataTable();
            DataTable dtystemroute = new DataTable();
            DataTable dtRouteReply = new DataTable();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            string _cmd;

            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + cmpid + "', @System='Sales'";
            dtystemroute = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomer] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerItem] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerAssign] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtAssign = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerOwner] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtOwner = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerRoute] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getReqFromCustomerRoute_Reply] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtRouteReply = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[sp_getManageReqComment] @Operation='COMMENT' ,   @cmpid='" + cmpid + "'";
            dtComment = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[sp_getManageReqComment] @Operation='REPLY' ,   @cmpid='" + cmpid + "'";
            dtCommentReply = DB.DBConn.GetDataTable(_cmd);

            List<Models.Route> routes = new List<Models.Route>();
            var columns = new List<object>();
            var tasks = new Dictionary<string, List<object>>();
            foreach (DataRow rt in dtystemroute.Rows)
            {
                var route = new Models.Route();
                route.CmpId = rt["CmpId"].ToString();
                route.RouteId = rt["RouteId"].ToString();
                route.RouteName = rt["RouteName"].ToString();
                route.Department = rt["Department"].ToString();
                route.completepercent = double.Parse(rt["completepercent"].ToString());
                route.Seq = int.Parse(rt["Seq"].ToString());
                columns.Add(route);

                tasks[route.RouteId] = new List<object>();

                List<ReqFromCustList> crms = new List<ReqFromCustList>();
                foreach (DataRow r in dt.Select("RouteId='" + route.RouteId + "'"))
                {
                    var crm = new ReqFromCustList();
                    crm.CmpId = r["CmpId"].ToString();
                    crm.TicketId = r["TicketId"].ToString();
                    crm.ticketIdRef = r["ticketIdRef"].ToString();
                    crm.ServiceType = r["ServiceType"].ToString();
                    crm.CustomerName = r["CustomerName"].ToString();
                    crm.ContactName = r["ContactName"].ToString();
                    crm.ContactPhone = r["ContactPhone"].ToString();
                    crm.ContactEmail = r["ContactEmail"].ToString();
                    crm.Address = r["Address"].ToString();
                    crm.CreateAt = DateTime.Parse(r["CreateAt"].ToString());
                    crm.Status = r["Status"].ToString();
                    crm.FromApp = r["FromApp"].ToString();
                    crm.AdditionalDetail = r["AdditionalDetail"].ToString();
                    crm.AdditionalDetail2 = r["AdditionalDetail2"].ToString();
                    crm.todo = r["todo"].ToString();
                    crm.completepercent = decimal.Parse(r["completepercent"].ToString());
                    crm.DueDate = DateTime
                        .Parse(r["DueDate"].ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture);
                    crm.UpdUser = r["updUser"].ToString();
                    crm.ticketIdRef = r["TicketIdRef"].ToString();
                    crm.Priority = r["Priority"].ToString();
                    crm.CustomerImgPath = r["CustomerImgPath"].ToString();
                    crm.TaskUnRead = int.Parse(r["taskUnRead"].ToString());
                    crm.Labels = r["Labels"].ToString();
                    crm.GrandAmt = decimal.Parse(r["GrandAmt"].ToString());

                    crm.ReqRoute = new List<CustomerReqTicketRoute>();
                    foreach (
                        DataRow i in dtRoute.Select(
                            " TicketId='" + r["TicketId"].ToString() + "'  "
                        )
                    )
                    {
                        var item = new CustomerReqTicketRoute();
                        item.CmpId = i["CmpId"].ToString();
                        item.TicketId = i["TicketId"].ToString();
                        item.RouteId = i["RouteId"].ToString();
                        item.RemindId = i["RemindId"].ToString();
                        item.RouteIdBefore = i["RouteIdBefore"].ToString();
                        item.StatusFinish = Int16.Parse(i["StatusFinish"].ToString());
                        item.DueDate = DateTime.Parse(i["DueDate"].ToString());
                        item.RouteName = i["RouteName"].ToString();
                        item.Department = i["Department"].ToString();
                        item.RemideDescription = i["RemideDescription"].ToString();
                        item.Seq = Int16.Parse(i["Seq"].ToString());
                        item.DateFinish = "";
                        item.UserFinish = "";
                        item.UpdUser = i["UpdUser"].ToString();

                        item.reqReply = new List<CustomerReqTicketRouteReply>();

                        foreach (
                            DataRow a in dtRouteReply.Select(
                                "TicketId='"
                                    + item.TicketId
                                    + "' and RemindId='"
                                    + item.RemindId
                                    + "' and RouteId='"
                                    + item.RouteId
                                    + "'"
                            )
                        )
                        {
                            var itemr = new CustomerReqTicketRouteReply();
                            itemr.CmpId = a["CmpId"].ToString();
                            itemr.TicketId = a["TicketId"].ToString();
                            itemr.UpdUser = a["updUser"].ToString();
                            itemr.FileUrl = a["FileUrl"].ToString();
                            itemr.Comment = a["Comment"].ToString();
                            itemr.RouteId = item.RouteId.ToString();
                            itemr.RemindId = item.RemindId.ToString();
                            itemr.createAt = DateTime.Parse(a["createAt"].ToString());
                            itemr.Seq = int.Parse(a["Seq"].ToString());
                            itemr.ImgPath = a["ImgPath"].ToString();

                            item.reqReply.Add(itemr);
                        }

                        // new comment
                        var itemrb = new CustomerReqTicketRouteReply();
                        itemrb.CmpId = item.CmpId;
                        itemrb.TicketId = item.TicketId;
                        itemrb.UpdUser = "";
                        itemrb.FileUrl = "";
                        itemrb.Comment = "";
                        itemrb.RouteId = item.RouteId;
                        itemrb.RemindId = item.RemindId;
                        itemrb.createAt = DateTime.Now.AddMinutes(1);
                        itemrb.Seq = 99999999;
                        itemrb.ImgPath = "";

                        item.reqReply.Add(itemrb);

                        // end comment


                        item.reqAssign = new List<ReqFromCustAssign>();

                        foreach (
                            DataRow a in dtAssign.Select(
                                "TicketId='"
                                    + item.TicketId
                                    + "' and RemindId='"
                                    + item.RemindId
                                    + "' and RouteId='"
                                    + item.RouteId
                                    + "'"
                            )
                        )
                        {
                            var itema = new ReqFromCustAssign();
                            itema.CmpId = a["CmpId"].ToString();
                            itema.TicketId = a["TicketId"].ToString();
                            itema.UserFullName = a["FullName"].ToString();
                            itema.ImgPath = a["ImgPath"].ToString();
                            itema.Permission = a["Permission"].ToString();
                            itema.RouteId = item.RouteId.ToString();
                            itema.RemindId = item.RemindId.ToString();
                            itema.UserId = a["UserId"].ToString();

                            item.reqAssign.Add(itema);
                        }

                        crm.ReqRoute.Add(item);
                    }

                    crm.ReqOwner = new List<ReqFromCustOwner>();
                    foreach (
                        DataRow i in dtOwner.Select(
                            " TicketId='"
                                + r["TicketId"].ToString()
                                + "' and  CmpId='"
                                + r["CmpId"].ToString()
                                + "'"
                        )
                    )
                    {
                        var item = new ReqFromCustOwner();
                        item.CmpId = i["CmpId"].ToString();
                        item.TicketId = i["TicketId"].ToString();
                        item.UserFullName = i["FullName"].ToString();
                        item.ImgPath = i["ImgPath"].ToString();
                        item.UserId = i["UserId"].ToString();

                        crm.ReqOwner.Add(item);
                    }

                    crm.ReqItem = new List<ReqFromCustItem>();

                    foreach (
                        DataRow i in dtItem.Select(
                            " TicketId='"
                                + r["TicketId"].ToString()
                                + "' and  CmpId='"
                                + r["CmpId"].ToString()
                                + "'"
                        )
                    )
                    {
                        var item = new ReqFromCustItem();
                        item.CmpId = i["CmpId"].ToString();
                        item.TicketId = i["TicketId"].ToString();
                        item.ServiceType = i["ServiceType"].ToString();
                        item.ModelName = i["ModelName"].ToString();
                        item.SerialNo = i["SerialNo"].ToString();
                        item.PartNo = i["PartNo"].ToString();

                        item.Forticloud = i["Forticloud"].ToString();
                        item.MABy = i["MABy"].ToString();
                        item.MADuration = i["MADuration"].ToString();
                        item.AdvanceReplacement = i["AdvanceReplacement"].ToString();

                        item.SLA = i["SLA"].ToString();
                        item.AdditionalDetail = i["AdditionalDetail"].ToString();
                        item.AdditionalDetail2 = i["AdditionalDetail2"].ToString();
                        item.DesiredService = i["DesiredService"].ToString();
                        item.FileUrl = i["FIleUrl"].ToString();
                        item.Seq = int.Parse(i["Seq"].ToString());
                        item.FileUrl1 = i["FIleUrl1"].ToString();
                        crm.ReqItem.Add(item);
                    }

                    crm.ReqAssign = new List<ReqFromCustAssign>();
                    foreach (
                        DataRow i in dtAssign.Select(
                            " TicketId='"
                                + r["TicketId"].ToString()
                                + "' and  CmpId='"
                                + r["CmpId"].ToString()
                                + "'"
                        )
                    )
                    {
                        var item = new ReqFromCustAssign();
                        item.CmpId = i["CmpId"].ToString();
                        item.TicketId = i["TicketId"].ToString();
                        item.UserFullName = i["FullName"].ToString();
                        item.ImgPath = i["ImgPath"].ToString();
                        item.Permission = i["Permission"].ToString();
                        item.UserId = i["UserId"].ToString();
                        item.RemindId = i["RemindId"].ToString();
                        item.RouteId = i["RouteId"].ToString();

                        crm.ReqAssign.Add(item);
                    }

                    crm.ReqComments = new List<ReqComment>();
                    foreach (
                        DataRow i in dtComment.Select(
                            " TicketId='"
                                + r["TicketId"].ToString()
                                + "' and  CmpId='"
                                + r["CmpId"].ToString()
                                + "'"
                        )
                    )
                    {
                        var comment = new ReqComment();

                        comment.CmpId = i["CmpId"].ToString();
                        comment.CommentId = i["CommentId"].ToString();
                        comment.TicketId = i["TicketId"].ToString();
                        comment.Id = i["Id"].ToString();
                        comment.Name = i["Name"].ToString();
                        comment.AvatarUrl = i["AvatarUrl"].ToString();
                        comment.Message = i["Message"].ToString();
                        comment.PostedAt = DateTime.Parse(i["PostedAt"].ToString());
                        comment.replyComment = new List<ReplyComment>();
                        foreach (
                            DataRow x in dtCommentReply.Select(
                                "TicketId='"
                                    + i["TicketId"].ToString()
                                    + "' and CmpId='"
                                    + i["CmpId"].ToString()
                                    + "' and CommentId='"
                                    + i["CommentId"].ToString()
                                    + "'"
                            )
                        )
                        {
                            var reply = new ReplyComment();
                            reply.CmpId = x["CmpId"].ToString();
                            reply.CommentId = x["Comment"].ToString();
                            reply.TicketId = x["TicketId"].ToString();
                            reply.Id = x["Id"].ToString();
                            reply.UserId = x["UserId"].ToString();
                            reply.Message = x["Message"].ToString();

                            reply.TagUser = x["TagUser"].ToString();
                            reply.PostedAt = DateTime.Parse(i["PostedAt"].ToString());

                            comment.replyComment.Add(reply);
                        }

                        crm.ReqComments.Add(comment);
                    }

                    crms.Add(crm);
                    tasks[route.RouteId].Add(crm);
                }
            }

            var response = new { board = new { tasks, columns } };

            return Ok(response);
        }

        [HttpGet("[action]")]
        public IActionResult getreqfromroutesaledefault(
            [FromQuery] string userlogin,
            [FromQuery] string cmpid,
            [FromQuery] string ticketId
        )
        {
            DataTable dtRoute = new DataTable();

            string _cmd;

            _cmd =
                "exec dbo.[getReqFromCustomerRoute_Sale_Default] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "' , @ticketId = '"
                + ticketId
                + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);

            List<CustomerReqTicketRoute> crms = new List<CustomerReqTicketRoute>();
            foreach (DataRow i in dtRoute.Rows)
            {
                var item = new CustomerReqTicketRoute();
                item.CmpId = i["CmpId"].ToString();
                item.TicketId = i["TicketId"].ToString();
                item.RouteId = i["RouteId"].ToString();
                item.RemindId = i["RemindId"].ToString();
                item.RouteIdBefore = i["RouteIdBefore"].ToString();
                item.StatusFinish = Int16.Parse(i["StatusFinish"].ToString());
                item.DueDate = DateTime.Parse(i["DueDate"].ToString());
                item.RouteName = i["RouteName"].ToString();
                item.Department = i["Department"].ToString();
                item.RemideDescription = i["RemideDescription"].ToString();
                item.Seq = Int16.Parse(i["Seq"].ToString());

                item.reqAssign = new List<ReqFromCustAssign>();

                crms.Add(item);
            }
            return Ok(crms);
        }

        [HttpGet("[action]")]
        public IActionResult getCrmlist([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dttask = new System.Data.DataTable();
            DataTable dttaskfile = new System.Data.DataTable();
            DataTable dttaskcomment = new System.Data.DataTable();
            DataTable dtappointment = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCrmGrp] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTask] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttask = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskFile] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttaskfile = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskComment] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dttaskcomment = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskAppointment] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtappointment = DB.DBConn.GetDataTable(_cmd);

            List<getCrm> crm = new List<getCrm>();

            foreach (DataRow r in dt.Rows)
            {
                var crms = new getCrm();

                crms.items = new List<getCrmTask>();

                crms.grpid = r["grpid"].ToString();
                crms.grpname = r["grpname"].ToString();
                crms.grpdesciption = r["grpdescription"].ToString();
                crms.expRevenueTotal = 0;
                foreach (DataRow ct in dttask.Select("grpid='" + r["grpid"].ToString() + "'"))
                {
                    var ctask = new getCrmTask();
                    ctask.taskId = ct["TaskId"].ToString();
                    ctask.taskname = ct["TaskName"].ToString();
                    ctask.salesname = ct["SalesName"].ToString();
                    ctask.taskrating = Convert.ToInt32(ct["Taskrating"].ToString());
                    ctask.expRevenue = Convert.ToDecimal(ct["ExpRevenue"].ToString());
                    crms.expRevenueTotal = crms.expRevenueTotal + ctask.expRevenue;

                    ctask.customername = ct["CustomerName"].ToString();
                    ctask.customerEmail = ct["CustomerEmail"].ToString();
                    ctask.customerPhone = ct["CustomerPhone"].ToString();
                    ctask.imgPath = ct["ImgPath"].ToString();
                    ctask.customeraddress = ct["CustomerAddress"].ToString();
                    ctask.customerProvince = ct["CustomerProvince"].ToString();
                    ctask.customerDistrict = ct["CustomerDistrict"].ToString();
                    ctask.customerSubDistrict = ct["CustomerSubDistrict"].ToString();
                    ctask.customerPostCode = ct["CustomerPostCode"].ToString();
                    ctask.customerWebsite = ct["CustomerWebsite"].ToString();
                    ctask.customerContactName = ct["CustomerContactName"].ToString();
                    ctask.customerContactTile = ct["CustomerContactTile"].ToString();
                    ctask.customerContactJobPosition = ct["CustomerContactJobPosition"].ToString();
                    ctask.customerContactMobile = ct["CustomerContactMobile"].ToString();

                    ctask.note = ct["Note"].ToString();
                    ctask.Progress = Convert.ToInt32(0 + r["Progress"].ToString());

                    ctask.files = new List<getCrmFile>();

                    foreach (
                        DataRow cf in dttaskfile.Select("TaskId='" + ct["TaskId"].ToString() + "'")
                    )
                    {
                        var crmf = new getCrmFile();

                        crmf.filePath = cf["FilePath"].ToString();
                        crmf.description = cf["Description"].ToString();
                        crmf.Seq = Convert.ToInt32(cf["Seq"].ToString());
                        ctask.files.Add(crmf);
                    }

                    ctask.comments = new List<getCRMComment>();

                    foreach (
                        DataRow cm in dttaskcomment.Select(
                            "TaskId='" + ct["TaskId"].ToString() + "'"
                        )
                    )
                    {
                        var crmcmm = new getCRMComment();

                        crmcmm.commentId = cm["CommentId"].ToString();
                        crmcmm.author = cm["Author"].ToString();
                        crmcmm.avatar = cm["Avatar"].ToString();
                        crmcmm.content = cm["Content"].ToString();
                        crmcmm.datetime = cm["CommentDateTime"].ToString();
                        crmcmm.likes = Convert.ToInt32(cm["likes"].ToString());
                        crmcmm.dislikes = Convert.ToInt32(cm["dislikes"].ToString());
                        ctask.comments.Add(crmcmm);
                    }

                    ctask.appointment = new List<getCrmAppointment>();

                    foreach (
                        DataRow ap in dtappointment.Select(
                            "TaskId='" + ct["TaskId"].ToString() + "'"
                        )
                    )
                    {
                        var appoint = new getCrmAppointment();
                        appoint.Seq = Convert.ToInt32(ap["Seq"].ToString());
                        appoint.appointmentdescription = ap["AppointmentDescription"].ToString();
                        appoint.appointmentdate = ap["AppointmentDate"].ToString();
                        appoint.appointmenttime = ap["AppointmentTime"].ToString();
                        appoint.appointmenttype = ap["AppointmentType"].ToString();
                        ctask.appointment.Add(appoint);
                    }

                    crms.items.Add(ctask);
                }

                crm.Add(crms);
            }

            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(crm);
            return Ok(qdetail);
        }

        [HttpGet("[action]")]
        public IActionResult getCrmlistByCust(
            [FromQuery] string userlogin,
            [FromQuery] string cmpid,
            [FromQuery] string customername
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dttask = new System.Data.DataTable();
            DataTable dttaskfile = new System.Data.DataTable();
            DataTable dttaskcomment = new System.Data.DataTable();
            DataTable dtappointment = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCrmGrp] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskByCust] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "' , @cust='"
                + customername
                + "'";
            dttask = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskFile] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' ";
            dttaskfile = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskComment] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dttaskcomment = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getCrmTaskAppointment] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "' ";
            dtappointment = DB.DBConn.GetDataTable(_cmd);

            List<getCrm> crm = new List<getCrm>();

            foreach (DataRow r in dt.Rows)
            {
                var crms = new getCrm();

                crms.items = new List<getCrmTask>();

                crms.grpid = r["grpid"].ToString();
                crms.grpname = r["grpname"].ToString();
                crms.grpdesciption = r["grpdescription"].ToString();
                crms.expRevenueTotal = 0;
                foreach (DataRow ct in dttask.Select("grpid='" + r["grpid"].ToString() + "'"))
                {
                    var ctask = new getCrmTask();
                    ctask.taskId = ct["TaskId"].ToString();
                    ctask.taskname = ct["TaskName"].ToString();
                    ctask.salesname = ct["SalesName"].ToString();
                    ctask.taskrating = Convert.ToInt32(ct["Taskrating"].ToString());
                    ctask.expRevenue = Convert.ToDecimal(ct["ExpRevenue"].ToString());
                    crms.expRevenueTotal = crms.expRevenueTotal + ctask.expRevenue;

                    ctask.customername = ct["CustomerName"].ToString();
                    ctask.customerEmail = ct["CustomerEmail"].ToString();
                    ctask.customerPhone = ct["CustomerPhone"].ToString();
                    ctask.imgPath = ct["ImgPath"].ToString();
                    ctask.customeraddress = ct["CustomerAddress"].ToString();
                    ctask.customerProvince = ct["CustomerProvince"].ToString();
                    ctask.customerDistrict = ct["CustomerDistrict"].ToString();
                    ctask.customerSubDistrict = ct["CustomerSubDistrict"].ToString();
                    ctask.customerPostCode = ct["CustomerPostCode"].ToString();
                    ctask.customerWebsite = ct["CustomerWebsite"].ToString();
                    ctask.customerContactName = ct["CustomerContactName"].ToString();
                    ctask.customerContactTile = ct["CustomerContactTile"].ToString();
                    ctask.customerContactJobPosition = ct["CustomerContactJobPosition"].ToString();
                    ctask.customerContactMobile = ct["CustomerContactMobile"].ToString();

                    ctask.note = ct["Note"].ToString();
                    ctask.Progress = Convert.ToInt32(0 + r["Progress"].ToString());

                    ctask.files = new List<getCrmFile>();

                    foreach (
                        DataRow cf in dttaskfile.Select("TaskId='" + ct["TaskId"].ToString() + "'")
                    )
                    {
                        var crmf = new getCrmFile();

                        crmf.filePath = cf["FilePath"].ToString();
                        crmf.description = cf["Description"].ToString();
                        crmf.Seq = Convert.ToInt32(cf["Seq"].ToString());
                        ctask.files.Add(crmf);
                    }

                    ctask.comments = new List<getCRMComment>();

                    foreach (
                        DataRow cm in dttaskcomment.Select(
                            "TaskId='" + ct["TaskId"].ToString() + "'"
                        )
                    )
                    {
                        var crmcmm = new getCRMComment();

                        crmcmm.commentId = cm["CommentId"].ToString();
                        crmcmm.author = cm["Author"].ToString();
                        crmcmm.avatar = cm["Avatar"].ToString();
                        crmcmm.content = cm["Content"].ToString();
                        crmcmm.datetime = cm["CommentDateTime"].ToString();
                        crmcmm.likes = Convert.ToInt32(cm["likes"].ToString());
                        crmcmm.dislikes = Convert.ToInt32(cm["dislikes"].ToString());
                        ctask.comments.Add(crmcmm);
                    }

                    ctask.appointment = new List<getCrmAppointment>();

                    foreach (
                        DataRow ap in dtappointment.Select(
                            "TaskId='" + ct["TaskId"].ToString() + "'"
                        )
                    )
                    {
                        var appoint = new getCrmAppointment();
                        appoint.Seq = Convert.ToInt32(ap["Seq"].ToString());
                        appoint.appointmentdescription = ap["AppointmentDescription"].ToString();
                        appoint.appointmentdate = ap["AppointmentDate"].ToString();
                        appoint.appointmenttime = ap["AppointmentTime"].ToString();
                        appoint.appointmenttype = ap["AppointmentType"].ToString();
                        ctask.appointment.Add(appoint);
                    }

                    crms.items.Add(ctask);
                }

                crm.Add(crms);
            }

            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(crm);
            return Ok(qdetail);
        }

        [HttpGet("[action]")]
        public IActionResult getCrmlistTable([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getCrmTaskTable] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }

        [HttpGet("[action]")]
        public IActionResult getCrmAppointment(
            [FromQuery] string userlogin,
            [FromQuery] string cmpid
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getCrmTaskAppointment] @userlogin='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }

        [HttpPost("[action]")]
        public IActionResult setTaskMove(CrmTaskMoveModel mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMTaskMove";
                _cmd += " @CreateUser  ='" + mt.CreateUser + "'";
                _cmd += ",@CmpId ='" + mt.CmpId + "'";
                _cmd += ",@GrpId ='" + mt.GrpId + "'";
                _cmd += ",@TaskId  ='" + mt.TaskId + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmGrp(CrmGrpModel grp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMGrp";
                _cmd += " @CreateUser  ='" + grp.CreateUser + "'";
                _cmd += ",@CmpId ='" + grp.CmpId + "'";
                _cmd += ",@GrpId ='" + grp.GrpId + "'";
                _cmd += ",@GrpName  ='" + grp.GrpName + "'";
                _cmd += ",@GrpDescription  ='" + grp.GrpDescription + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public ActionResult setRouteReply(CustomerReqTicketRouteReply task)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {


                string _cmd = "";
                _cmd = "exec  dbo.setReqRouteReply";
                _cmd += " @TicketId  ='" + task.TicketId + "'";
                _cmd += ",@CmpId ='" + task.CmpId + "'";
                _cmd += ",@UpdUser ='" + task.UpdUser + "'";
                _cmd += ",@Comment ='" + task.Comment + "'";
                _cmd += ",@FileUrl ='" + task.FileUrl + "'";
                _cmd += ",@RouteId  ='" + task.RouteId + "'";
                _cmd += ",@RemindId  ='" + task.RemindId + "'";
                _cmd += ",@Seq =" + task.Seq;

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public ActionResult setRouteReplyRead(CustomerReqTicketRouteReplyRead task)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {


                string _cmd = "";
                _cmd = "exec  dbo.setReqRouteReplyRead";
                _cmd += " @TicketId  ='" + task.TicketId + "'";
                _cmd += ",@CmpId ='" + task.CmpId + "'";
                _cmd += ",@UpdUser ='" + task.UpdUser + "'";
                _cmd += ",@RouteId  ='" + task.RouteId + "'";
                _cmd += ",@RemindId  ='" + task.RemindId + "'";
                _cmd += ",@Seq =" + task.Seq;

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpGet("[action]")]
        public ActionResult getRouteReply([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            MsgReturn msgretrun = new MsgReturn();


            string _cmd = "";
            DataTable dtRouteReply = new DataTable();
            DataTable dtRoute = new DataTable();


            _cmd =
                           "exec dbo.[getReqFromCustomerRoute] @user='"
                           + userlogin
                           + "', @cmpid='"
                           + cmpid
                           + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);


            _cmd =
                "exec dbo.[getReqFromCustomerRoute_Reply] @user='"
                + userlogin
                + "', @cmpid='"
                + cmpid
                + "'";
            dtRouteReply = DB.DBConn.GetDataTable(_cmd);




            List<CustomerReqTicketRouteReply> item = new List<CustomerReqTicketRouteReply>();


            foreach (DataRow i in dtRoute.Rows)
            {
                foreach (
                    DataRow a in dtRouteReply.Select("TicketId='" + i["TicketId"].ToString() + "' and RouteId='" + i["RouteId"].ToString() + "'")
                )
                {
                    var itemr = new CustomerReqTicketRouteReply();
                    itemr.CmpId = a["CmpId"].ToString();
                    itemr.TicketId = a["TicketId"].ToString();
                    itemr.UpdUser = a["updUser"].ToString();
                    itemr.FileUrl = a["FileUrl"].ToString();
                    itemr.Comment = a["Comment"].ToString();
                    itemr.RouteId = a["RouteId"].ToString();
                    itemr.RemindId = a["RemindId"].ToString();
                    itemr.createAt = DateTime.Parse(a["createAt"].ToString());
                    itemr.Seq = int.Parse(a["Seq"].ToString());
                    itemr.ImgPath = a["ImgPath"].ToString();

                    item.Add(itemr);
                }

                // new comment
                var itemrb = new CustomerReqTicketRouteReply();
                itemrb.CmpId = cmpid;
                itemrb.TicketId = i["TicketId"].ToString();
                itemrb.UpdUser = "";
                itemrb.FileUrl = "";
                itemrb.Comment = "";
                itemrb.RouteId = i["RouteId"].ToString();
                itemrb.RemindId = i["RemindId"].ToString();
                itemrb.createAt = DateTime.Now.AddMinutes(1);
                itemrb.Seq = 99999999;
                itemrb.ImgPath = "";

                item.Add(itemrb);

                // end comment

            }


            return Ok(new { tickets = item });
        }

        [HttpPost("[action]")]
        public IActionResult delRouteReply(CustomerReqTicketRouteReply task)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                try
                {
                    if (task.FileUrl != "")
                    {
                        System.IO.File.Delete(task.FileUrl);
                    }
                }
                catch (System.Exception) { }

                string _cmd = "";
                _cmd = "exec  dbo.delReqRouteReply";
                _cmd += " @TicketId  ='" + task.TicketId + "'";
                _cmd += ",@CmpId ='" + task.CmpId + "'";
                _cmd += ",@UpdUser ='" + task.UpdUser + "'";
                _cmd += ",@Comment ='" + task.Comment + "'";
                _cmd += ",@FileUrl ='" + task.FileUrl + "'";
                _cmd += ",@RouteId  ='" + task.RouteId + "'";
                _cmd += ",@RemindId  ='" + task.RemindId + "'";
                _cmd += ",@Seq =" + task.Seq;

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmTask(CrmTaskModel task)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMTask";
                _cmd += " @CreateUser  ='" + task.CreateUser + "'";
                _cmd += ",@CmpId ='" + task.CmpId + "'";
                _cmd += ",@GrpId ='" + task.GrpId + "'";
                _cmd += ",@TaskId ='" + task.TaskId + "'";
                _cmd += ",@TaskName ='" + task.TaskName + "'";
                _cmd += ",@SalesName  ='" + task.SalesName + "'";
                _cmd += ",@Taskrating =" + task.Taskrating;
                _cmd += ",@ExpRevenue =" + task.ExpRevenue;
                _cmd += ",@CustomerName  ='" + task.CustomerName + "'";
                _cmd += ",@CustomerEmail  ='" + task.CustomerEmail + "'";
                _cmd += ",@CustomerPhone ='" + task.CustomerPhone + "'";
                _cmd += ",@ImgPath ='" + task.ImgPath + "'";
                _cmd += ",@CustomerAddress  ='" + task.CustomerAddress + "'";
                _cmd += ",@CustomerProvince  ='" + task.CustomerProvince + "'";
                _cmd += ",@CustomerDistrict  ='" + task.CustomerDistrict + "'";
                _cmd += ",@CustomerSubDistrict  ='" + task.CustomerSubDistrict + "'";
                _cmd += ",@CustomerPostCode  ='" + task.CustomerPostCode + "'";
                _cmd += ",@CustomerWebsite  ='" + task.CustomerWebsite + "'";
                _cmd += ",@CustomerContactName  ='" + task.CustomerContactName + "'";
                _cmd += ",@CustomerContactTile  ='" + task.CustomerContactTile + "'";
                _cmd += ",@CustomerContactJobPosition  ='" + task.CustomerContactJobPosition + "'";
                _cmd += ",@CustomerContactMobile ='" + task.CustomerContactMobile + "'";
                _cmd += ",@Note  ='" + task.Note + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmFiles(List<CRMFilesModel> f)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                if (f.Count > 0)
                {
                    _cmd =
                        "Delete From CRMFile where TaskId='"
                        + f[0].TaskId
                        + "' and CmpId='"
                        + f[0].CmpId
                        + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < f.Count; i++)
                {
                    _cmd = "exec  dbo.setCRMFiles";
                    _cmd += " @CreateUser  ='" + f[i].CreateUser + "'";
                    _cmd += ",@CmpId ='" + f[i].CmpId + "'";
                    _cmd += ",@TaskId ='" + f[i].TaskId + "'";
                    _cmd += ",@Seq =" + f[i].Seq;
                    _cmd += ",@FilePath  ='" + f[i].FilePath + "'";
                    _cmd += ",@Description  ='" + f[i].Description + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return NotFound(msgretrun);
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmAppointment(List<CRMAppointmentModel> appointment)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                if (appointment.Count > 0)
                {
                    _cmd =
                        "Delete From CRMAppointment where TaskId='"
                        + appointment[0].TaskId
                        + "' and CmpId='"
                        + appointment[0].CmpId
                        + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < appointment.Count; i++)
                {
                    _cmd = "exec  dbo.setCRMAppointment";
                    _cmd += " @CreateUser  ='" + appointment[i].CreateUser + "'";
                    _cmd += ",@CmpId ='" + appointment[i].CmpId + "'";
                    _cmd += ",@TaskId ='" + appointment[i].TaskId + "'";
                    _cmd += ",@Seq =" + appointment[i].Seq;
                    _cmd +=
                        ",@AppointmentDescription  ='"
                        + appointment[i].AppointmentDescription
                        + "'";
                    _cmd += ",@AppointmentType  ='" + appointment[i].AppointmentType + "'";
                    _cmd += ",@AppointmentDate  ='" + appointment[i].AppointmentDate + "'";
                    _cmd += ",@AppointmentTime  ='" + appointment[i].AppointmentTime + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return NotFound(msgretrun);
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmComment(CRMCommentModel cmnt)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMComment";
                _cmd += " @CreateUser  ='" + cmnt.CreateUser + "'";
                _cmd += ",@CmpId ='" + cmnt.CmpId + "'";
                _cmd += ",@TaskId ='" + cmnt.TaskId + "'";
                _cmd += ",@CommentId ='" + cmnt.CommentId + "'";
                _cmd += ",@Author  ='" + cmnt.Author + "'";
                _cmd += ",@Avatar  ='" + cmnt.Avatar + "'";
                _cmd += ",@Content  ='" + cmnt.Content + "'";
                _cmd += ",@CommentDateTime ='" + cmnt.CommentDateTime + "'";
                _cmd += ",@likes =" + cmnt.likes;
                _cmd += ",@dislikes =" + cmnt.dislikes;

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmCommentLikes(CRMComment_likesModel like)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMComment_likes";
                _cmd += " @CreateUser  ='" + like.CreateUser + "'";
                _cmd += ",@CmpId ='" + like.CmpId + "'";
                _cmd += ",@TaskId ='" + like.TaskId + "'";
                _cmd += ",@CommentId ='" + like.CommentId + "'";
                _cmd += ",@Seq =" + like.Seq;
                _cmd += ",@Userlikes  ='" + like.Userlikes + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmCommentDislikes(CRMComment_dislikesModel dis)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMComment_dislikes";
                _cmd += ",@CreateUser  ='" + dis.CreateUser + "'";
                _cmd += ",@CmpId ='" + dis.CmpId + "'";
                _cmd += ",@TaskId ='" + dis.TaskId + "'";
                _cmd += ",@CommentId ='" + dis.CommentId + "'";
                _cmd += ",@Seq =" + dis.Seq;
                _cmd += ",@Userdislikes  ='" + dis.Userdislikes + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCrmCommentFile(CRMComment_filesModel cf)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCRMComment_Files";
                _cmd += ",@CreateUser  ='" + cf.CreateUser + "'";
                _cmd += ",@CmpId ='" + cf.CmpId + "'";
                _cmd += ",@TaskId ='" + cf.TaskId + "'";
                _cmd += ",@CommentId ='" + cf.CommentId + "'";
                _cmd += ",@Seq =" + cf.Seq;
                _cmd += ",@FilePath =" + cf.FilePath;

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult delReqCrm([FromQuery] string TicketId, [FromQuery] string CmpId)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setReqOtherFromGoAlong_Delete";
                _cmd += " @CmpId ='" + CmpId + "'";
                _cmd += ",@TicketId ='" + TicketId + "'";

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
                    return NotFound(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }
    }
}
