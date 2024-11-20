using System.Dynamic;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using coreapi.Models.Trial;

namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CrmController : ControllerBase
    {


        [HttpGet("[action]")]
        public IActionResult getreqfromcustlist([FromQuery] string userlogin, [FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtAssign = new System.Data.DataTable();
            DataTable dtComment = new DataTable();
            DataTable dtCommentReply = new DataTable();
            DataTable dtOwner = new DataTable();
            DataTable dtRoute = new DataTable();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();


            string _cmd;
            _cmd = "exec dbo.[getReqFromCustomer] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerItem] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getReqFromCustomerAssign] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtAssign = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerOwner] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtOwner = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerRoute] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);



            _cmd = "exec dbo.[sp_getManageReqComment] @Operation='COMMENT' ,   @cmpid='" + cmpid + "'";
            dtComment = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[sp_getManageReqComment] @Operation='REPLY' ,   @cmpid='" + cmpid + "'";
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
                crm.completepercent = decimal.Parse(r["completepercent"].ToString());
                crm.DueDate = DateTime.Parse(r["DueDate"].ToString()).ToString("yyyy-MM-dd HH:mm", thaiCulture);
                crm.UpdUser = r["updUser"].ToString();
                crm.ticketIdRef = r["TicketIdRef"].ToString();

                crm.ReqRoute = new List<CustomerReqTicketRoute>();
                foreach (DataRow i in dtRoute.Select(" TicketId='" + r["TicketId"].ToString() + "'  "))
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

                    item.reqAssign = new List<ReqFromCustAssign>();

                    foreach (DataRow a in dtAssign.Select("TicketId='" + item.TicketId + "' and RemindId='" + item.RemindId + "' and RouteId='" + item.RouteId + "'"))
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
                foreach (DataRow i in dtOwner.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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

                foreach (DataRow i in dtItem.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                    item.DesiredService = i["DesiredService"].ToString();
                    item.FileUrl = i["FIleUrl"].ToString();
                    item.Seq = int.Parse(i["Seq"].ToString());
                    crm.ReqItem.Add(item);

                }


                crm.ReqAssign = new List<ReqFromCustAssign>();
                foreach (DataRow i in dtAssign.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                foreach (DataRow i in dtComment.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                    foreach (DataRow x in dtCommentReply.Select("TicketId='" + i["TicketId"].ToString() + "' and CmpId='" + i["CmpId"].ToString() + "' and CommentId='" + i["CommentId"].ToString() + "'"))
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







            return Ok(crms);
        }



        [HttpGet("[action]")]
        public IActionResult getreqfromcustkanban([FromQuery] string userlogin, [FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtAssign = new System.Data.DataTable();
            DataTable dtComment = new DataTable();
            DataTable dtCommentReply = new DataTable();
            DataTable dtOwner = new DataTable();
            DataTable dtRoute = new DataTable();
            DataTable dtystemroute = new DataTable();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();


            string _cmd;

            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + cmpid + "', @System='Sales'";
            dtystemroute = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomer] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerItem] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getReqFromCustomerAssign] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtAssign = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerOwner] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtOwner = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getReqFromCustomerRoute] @user='" + userlogin + "', @cmpid='" + cmpid + "'";
            dtRoute = DB.DBConn.GetDataTable(_cmd);



            _cmd = "exec dbo.[sp_getManageReqComment] @Operation='COMMENT' ,   @cmpid='" + cmpid + "'";
            dtComment = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[sp_getManageReqComment] @Operation='REPLY' ,   @cmpid='" + cmpid + "'";
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
                    crm.todo = r["todo"].ToString();
                    crm.completepercent = decimal.Parse(r["completepercent"].ToString());
                    crm.DueDate = DateTime.Parse(r["DueDate"].ToString()).ToString("yyyy-MM-dd HH:mm", thaiCulture);
                    crm.UpdUser = r["updUser"].ToString();
                    crm.ticketIdRef = r["TicketIdRef"].ToString();

                    crm.ReqRoute = new List<CustomerReqTicketRoute>();
                    foreach (DataRow i in dtRoute.Select(" TicketId='" + r["TicketId"].ToString() + "'  "))
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

                        item.reqAssign = new List<ReqFromCustAssign>();

                        foreach (DataRow a in dtAssign.Select("TicketId='" + item.TicketId + "' and RemindId='" + item.RemindId + "' and RouteId='" + item.RouteId + "'"))
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
                    foreach (DataRow i in dtOwner.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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

                    foreach (DataRow i in dtItem.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                        item.DesiredService = i["DesiredService"].ToString();
                        item.FileUrl = i["FIleUrl"].ToString();
                        item.Seq = int.Parse(i["Seq"].ToString());
                        crm.ReqItem.Add(item);

                    }


                    crm.ReqAssign = new List<ReqFromCustAssign>();
                    foreach (DataRow i in dtAssign.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                    foreach (DataRow i in dtComment.Select(" TicketId='" + r["TicketId"].ToString() + "' and  CmpId='" + r["CmpId"].ToString() + "'"))
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
                        foreach (DataRow x in dtCommentReply.Select("TicketId='" + i["TicketId"].ToString() + "' and CmpId='" + i["CmpId"].ToString() + "' and CommentId='" + i["CommentId"].ToString() + "'"))
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

            var response = new
            {
                board = new
                {
                    tasks,
                    columns
                }
            };

            return Ok(response);
        }





        [HttpGet("[action]")]
        public IActionResult getreqfromroutesaledefault([FromQuery] string userlogin, [FromQuery] string cmpid, [FromQuery] string ticketId)
        {

            DataTable dtRoute = new DataTable();

            string _cmd;

            _cmd = "exec dbo.[getReqFromCustomerRoute_Sale_Default] @user='" + userlogin + "', @cmpid='" + cmpid + "' , @ticketId = '" + ticketId + "'";
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

            _cmd = "exec dbo.[getCrmTaskFile] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttaskfile = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTaskComment] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttaskcomment = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTaskAppointment] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
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



                    foreach (DataRow cf in dttaskfile.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
                    {
                        var crmf = new getCrmFile();

                        crmf.filePath = cf["FilePath"].ToString();
                        crmf.description = cf["Description"].ToString();
                        crmf.Seq = Convert.ToInt32(cf["Seq"].ToString());
                        ctask.files.Add(crmf);
                    }


                    ctask.comments = new List<getCRMComment>();




                    foreach (DataRow cm in dttaskcomment.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
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

                    foreach (DataRow ap in dtappointment.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
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
        public IActionResult getCrmlistByCust([FromQuery] string userlogin, [FromQuery] string cmpid, [FromQuery] string customername)
        {

            DataTable dt = new System.Data.DataTable();
            DataTable dttask = new System.Data.DataTable();
            DataTable dttaskfile = new System.Data.DataTable();
            DataTable dttaskcomment = new System.Data.DataTable();
            DataTable dtappointment = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCrmGrp] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getCrmTaskByCust] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' , @cust='" + customername + "'";
            dttask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTaskFile] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' ";
            dttaskfile = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTaskComment] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttaskcomment = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCrmTaskAppointment] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "' ";
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



                    foreach (DataRow cf in dttaskfile.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
                    {
                        var crmf = new getCrmFile();

                        crmf.filePath = cf["FilePath"].ToString();
                        crmf.description = cf["Description"].ToString();
                        crmf.Seq = Convert.ToInt32(cf["Seq"].ToString());
                        ctask.files.Add(crmf);
                    }


                    ctask.comments = new List<getCRMComment>();




                    foreach (DataRow cm in dttaskcomment.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
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

                    foreach (DataRow ap in dtappointment.Select("TaskId='" + ct["TaskId"].ToString() + "'"))
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
            _cmd = "exec dbo.[getCrmTaskTable] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);



            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }



        [HttpGet("[action]")]
        public IActionResult getCrmAppointment([FromQuery] string userlogin, [FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCrmTaskAppointment] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
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
                    _cmd = "Delete From CRMFile where TaskId='" + f[0].TaskId + "' and CmpId='" + f[0].CmpId + "'";

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
                    };

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
                    _cmd = "Delete From CRMAppointment where TaskId='" + appointment[0].TaskId + "' and CmpId='" + appointment[0].CmpId + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < appointment.Count; i++)
                {
                    _cmd = "exec  dbo.setCRMAppointment";
                    _cmd += " @CreateUser  ='" + appointment[i].CreateUser + "'";
                    _cmd += ",@CmpId ='" + appointment[i].CmpId + "'";
                    _cmd += ",@TaskId ='" + appointment[i].TaskId + "'";
                    _cmd += ",@Seq =" + appointment[i].Seq;
                    _cmd += ",@AppointmentDescription  ='" + appointment[i].AppointmentDescription + "'";
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
                    };

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





















    }
}