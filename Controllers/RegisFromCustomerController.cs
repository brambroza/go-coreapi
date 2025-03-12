using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using goalongapi.Models;
using goalongapi.Interfaces;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    public class RegisFromCustomerController : ControllerBase
    {
        private readonly IProductService productService;

        public RegisFromCustomerController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<RegisFromCustomerController> logger,
            IProductService productService
        )
        {
            this.productService = productService;
        }

        [HttpPost("sendMAFortigate")]
        public async Task<IActionResult> MAFortigate(
            List<IFormFile> formFiles,
            [FromForm] MAFortigate request
        )
        {
            var url = await UploadFilesAsyn(formFiles);

            string _cmd;
            _cmd = "exec  dbo.setMAFortigate";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @ModelName  ='" + request.model + "'";
            _cmd += ", @SerialNo  ='" + request.serial + "'";
            _cmd += ", @Forticloud  ='" + request.forticloud + "'";
            _cmd += ", @MADuration  ='" + request.maDuration + "'";
            _cmd += ", @AdvanceReplacement  ='" + request.advanceReplacement + "'";
            _cmd += ", @SLA  ='" + request.sla + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
            _cmd += ", @docno='" + request.docno + "'";
            if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            {
                _cmd += ", @FileUrl  =''";
            }

            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpPost("sendMASiscoServer")]
        public async Task<IActionResult> MACiscoServer(
            List<IFormFile> formFiles,
            [FromForm] MACiscoServer request
        )
        {
            var url = await UploadFilesAsyn(formFiles);

            string _cmd;
            _cmd = "exec  dbo.setMACiscoServer";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @ModelName  ='" + request.model + "'";
            _cmd += ", @SerialNo  ='" + request.serial + "'";
            _cmd += ", @PartNo  ='" + request.partNumber + "'";
            _cmd += ", @MABy  ='" + request.maBy + "'";
            _cmd += ", @MADuration  ='" + request.maDuration + "'";
            _cmd += ", @AdvanceReplacement  ='" + request.advanceReplacement + "'";
            _cmd += ", @SLA  ='" + request.sla + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
            _cmd += ", @docno='" + request.docno + "'";

            if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            { 
                _cmd += ", @FileUrl  =''";
            }

            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpPost("sendReqOther")]
        public async Task<IActionResult> ReqOther(
            List<IFormFile> formFiles,
            [FromForm] MAOther request
        )
        {
            var url = await UploadFilesAsyn(formFiles);

            string _cmd;
            _cmd = "exec  dbo.setReqOther";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @DesiredService ='" + request.desiredService + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
            _cmd += ", @docno='" + request.docno + "'";
            if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            {
                _cmd += ", @FileUrl  =''";
            }

            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpPost("sendReqOtherFromGoalongFile")]
        public async Task<IActionResult> ReqOtherFromGoalongFiles(List<IFormFile> formFiles)
        {
            var url = await UploadFilesAsyn(formFiles);
            return Ok(url);
        }

        [HttpPost("sendReqOtherFromGoalong")]
        public async Task<IActionResult> ReqOtherFromGoalong(ReqFromCustList request)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                /*    var url = await UploadFilesAsyn(formFiles); */

                string _cmd;
                string JSONString = string.Empty;
                string ticketIdRef = "";

                for (int i = 0; i < request.ReqItem.Count; i++)
                {
                    if (ticketIdRef != "")
                    {
                        request.ticketIdRef = ticketIdRef;
                    }

                    _cmd = "exec  dbo.setReqOtherFromGoAlong";
                    _cmd += " @CustomerName  ='" + request.CustomerName + "'";
                    _cmd += ", @ContactName  ='" + request.ContactName + "'";
                    _cmd += ", @ContactPhone  ='" + request.ContactPhone + "'";
                    _cmd += ", @ContactEmail  ='" + request.ContactEmail + "'";
                    _cmd += ", @Address  ='" + request.Address + "'";


                    _cmd += ", @ServiceType  ='" + request.ReqItem[i].ServiceType + "'";
                    _cmd += ", @FromApp  ='" + request.FromApp + "'";
                    _cmd += ", @CmpId='" + request.CmpId + "'";
                    _cmd += ", @TicketId='" + request.TicketId + "'";
                    _cmd += ",@DesiredService='" + request.ReqItem[i].DesiredService + "'";
                    _cmd += ", @ModelName='" + request.ReqItem[i].ModelName + "'";
                    _cmd += ", @SerialNo='" + request.ReqItem[i].SerialNo + "'";
                    _cmd += ", @Forticloud='" + request.ReqItem[i].Forticloud + "'";
                    _cmd += ", @MADuration='" + request.ReqItem[i].MADuration + "'";
                    _cmd += ", @TicketIdRef='" + request.ticketIdRef + "'";
                    _cmd += " , @Seq=" + request.ReqItem[i].Seq + "";
                    _cmd += ", @PartNo='" + request.ReqItem[i].PartNo + "'";
                    _cmd += ", @MABy='" + request.ReqItem[i].MABy + "'";
                    _cmd += ", @Priority='" + request.Priority + "'";
                    _cmd += ", @Status='" + request.Status + "'";

                    _cmd += ", @AdvanceReplacement='" + request.ReqItem[i].AdvanceReplacement + "'";

                    _cmd += ", @SLA='" + request.ReqItem[i].SLA + "'";
                    _cmd += " , @updUser='" + request.UpdUser + "'";

                    _cmd += ", @AdditionalDetail  ='" + request.ReqItem[i].AdditionalDetail + "'";
                    _cmd += ", @AdditionalDetail2  ='" + request.ReqItem[i].AdditionalDetail2 + "'";
                    if (request.ReqItem[i].FileUrl != null)
                    {
                        _cmd +=
                            ", @FileUrl  ='" + string.Join(",", request.ReqItem[i].FileUrl) + "'";
                    }
                    else
                    {
                        _cmd += ", @FileUrl  =''";
                    }

                    if (request.ReqItem[i].FileUrl1 != null)
                    {
                        _cmd +=
                            ", @FileUrl1  ='" + string.Join(",", request.ReqItem[i].FileUrl1) + "'";
                    }
                    else
                    {
                        _cmd += ", @FileUrl1  =''";
                    }

                    DataTable dt = DB.DBConn.GetDataTable(_cmd);
                    JSONString = JsonConvert.SerializeObject(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ticketIdRef = dt.Rows[0][0].ToString();
                    }
                    else
                    {
                        ticketIdRef = "";
                    }

                    for (int r = 0; r < request.ReqRoute.Count; r++)
                    {
                        _cmd = "exec dbo.[setReqOtherFromGoAlong_Route]";
                        _cmd += "  @updUser='" + request.UpdUser + "'";
                        _cmd += " , @CmpId='" + request.CmpId + "'";
                        _cmd += " ,  @dateFinish='" + request.ReqRoute[r].DateFinish + "'";
                        _cmd += " ,  @department='sales'";
                        _cmd +=
                            " ,  @duedate='"
                            + request.ReqRoute[r].DueDate?.ToString("yyyy-MM-dd HH:mm", thaiCulture)
                            + "'";
                        _cmd +=
                            " ,  @RemindDescription='"
                            + request.ReqRoute[r].RemideDescription
                            + "'";
                        _cmd += " ,  @remindId='" + request.ReqRoute[r].RemindId + "'";
                        _cmd += " ,  @RouteId='" + request.ReqRoute[r].RouteId + "'";
                        _cmd += " ,  @RouteIdBefore='" + request.ReqRoute[r].RouteIdBefore + "'";
                        _cmd += " ,  @routeName='" + request.ReqRoute[r].RouteName + "'";
                        _cmd += " ,  @Seq=" + request.ReqRoute[r].Seq + "";
                        _cmd += " ,  @TicketId='" + request.ReqRoute[r].TicketId + "'";
                        _cmd += " ,  @UserFinish='" + request.ReqRoute[r].UserFinish + "'";

                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                            return BadRequest();
                        }
                        ;

                        for (int a = 0; a < request.ReqRoute[r].reqAssign.Count; a++)
                        {
                            _cmd = " exec dbo.[setReqOtherFromGoAlong_Route_Assign]";
                            _cmd += "  @CmpId='" + request.ReqRoute[r].reqAssign[a].CmpId + "'";
                            _cmd +=
                                " , @TicketId='" + request.ReqRoute[r].reqAssign[a].TicketId + "'";
                            _cmd += " , @RouteId='" + request.ReqRoute[r].RouteId + "'";
                            _cmd += "  ,@RemindId='" + request.ReqRoute[r].RemindId + "'";
                            _cmd += "  ,@UserId='" + request.ReqRoute[r].reqAssign[a].UserId + "'";
                            _cmd +=
                                "  ,@Permission='"
                                + request.ReqRoute[r].reqAssign[a].Permission
                                + "'";
                            if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                            {
                                DB.DBConn.Tran.Rollback();
                                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                                return BadRequest();
                            }
                            ;
                        }
                    }
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return Ok(JSONString);
            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return BadRequest();
            }
        }

        [HttpPost("setReqRoute")]
        public async Task<IActionResult> ReqRoute(CustomerReqTicketRoute request)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                /*    var url = await UploadFilesAsyn(formFiles); */

                string _cmd;
                string JSONString = string.Empty;
                string ticketIdRef = "";

                _cmd = "exec dbo.[setReqOtherFromGoAlong_Route]";
                _cmd += "  @updUser='" + request.UpdUser + "'";
                _cmd += " , @CmpId='" + request.CmpId + "'";
                _cmd += " ,  @dateFinish='" + request.DateFinish + "'";
                _cmd += " ,  @department='sales'";
                _cmd +=
                    " ,  @duedate='"
                    + request.DueDate?.ToString("yyyy-MM-dd HH:mm", thaiCulture)
                    + "'";
                _cmd += " ,  @RemindDescription='" + request.RemideDescription + "'";
                _cmd += " ,  @remindId='" + request.RemindId + "'";
                _cmd += " ,  @RouteId='" + request.RouteId + "'";
                _cmd += " ,  @RouteIdBefore='" + request.RouteIdBefore + "'";
                _cmd += " ,  @routeName='" + request.RouteName + "'";
                _cmd += " ,  @Seq=" + request.Seq + "";
                _cmd += " ,  @TicketId='" + request.TicketId + "'";
                _cmd += " ,  @UserFinish='" + request.UserFinish + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                    return BadRequest();
                }


                _cmd = " delete from dbo.CustomerReqAssign  where ";
                _cmd += "  CmpId='" + request.CmpId + "'";
                _cmd += "  and TicketId='" + request.TicketId + "'";
                _cmd += "  and RouteId='" + request.RouteId + "'";
                _cmd += "  and RemindId='" + request.RemindId + "'";
                DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);

                for (int a = 0; a < request.reqAssign.Count; a++)
                {
                    _cmd = " exec dbo.[setReqOtherFromGoAlong_Route_Assign]";
                    _cmd += "  @CmpId='" + request.reqAssign[a].CmpId + "'";
                    _cmd += " , @TicketId='" + request.reqAssign[a].TicketId + "'";
                    _cmd += " , @RouteId='" + request.RouteId + "'";
                    _cmd += "  ,@RemindId='" + request.RemindId + "'";
                    _cmd += "  ,@UserId='" + request.reqAssign[a].UserId + "'";
                    _cmd += "  ,@Permission='" + request.reqAssign[a].Permission + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                        return BadRequest();
                    }
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return Ok();
            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return BadRequest();
            }
        }

        [HttpPost("setReqAssign")]
        public async Task<IActionResult> setReqAssign(ReqFromCustAssign assign)
        {
            string _cmd;
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                _cmd = " exec dbo.[setReqOtherFromGoAlong_Route_Assign]";
                _cmd += "  @CmpId='" + assign.CmpId + "'";
                _cmd += " , @TicketId='" + assign.TicketId + "'";
                _cmd += " , @RouteId='" + assign.RouteId + "'";
                _cmd += "  ,@RemindId='" + assign.RemindId + "'";
                _cmd += "  ,@UserId='" + assign.UserId + "'";
                _cmd += "  ,@Permission='" + assign.Permission + "'";
                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                    return BadRequest();
                }
                ;

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return Ok("");
            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return BadRequest();
            }
        }

        [HttpPost("setReqComment")]
        public async Task<IActionResult> setReqComment(ReqComment comment)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
            /*    var url = await UploadFilesAsyn(formFiles); */
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.setReqComment";
                _cmd += " @CmpId  ='" + comment.CmpId + "'";
                _cmd += ", @TicketId  ='" + comment.TicketId + "'";
                _cmd += ", @Id  ='" + comment.Id + "'";
                _cmd += ", @name  ='" + comment.Name + "'";
                _cmd += ", @message  ='" + comment.Message + "'";
                _cmd += " ,@avatarUrl  =''";
                _cmd +=
                    ", @postedAt ='"
                    + comment.PostedAt.ToString("yyyy-MM-dd HH:mm", thaiCulture)
                    + "'";

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

                for (int i = 0; i < comment.replyComment.Count; i++)
                {
                    _cmd = "exec  dbo.setReqReplyComment";
                    _cmd += " @CmpId  ='" + comment.replyComment[i].CmpId + "'";
                    _cmd += " ,@TicketId  ='" + comment.replyComment[i].TicketId + "'";
                    _cmd += ", @Id  ='" + comment.replyComment[i].Id + "'";
                    _cmd += " ,@name  ='" + comment.replyComment[i].UserId + "'";
                    _cmd += " ,@message  ='" + comment.replyComment[i].Message + "'";
                    _cmd += " ,@avatarUrl  =''";
                    _cmd +=
                        ", @postedAt ='"
                        + comment.replyComment[i].PostedAt.ToString("yyyy-MM-dd HH:mm", thaiCulture)
                        + "'";
                    _cmd += " ,@CommentId ='" + comment.replyComment[i].CommentId + "'";

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
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("setReqRouteFinish")]
        public async Task<IActionResult> setReqRouteFinish(CustomerReqTicketRoute comment)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
            /*    var url = await UploadFilesAsyn(formFiles); */
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.setReqRouteFinish";
                _cmd += " @CmpId  ='" + comment.CmpId + "'";
                _cmd += ", @TicketId  ='" + comment.TicketId + "'";
                _cmd += ", @RouteId  ='" + comment.RouteId + "'";
                _cmd += ", @RemindId  ='" + comment.RemindId + "'";
                _cmd += ", @StatusFinish  =" + Int16.Parse(comment.StatusFinish.ToString());
                _cmd += ", @UserFinish  ='" + comment.UserFinish + "'";

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

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun); 
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost("setReqUpdateTask")]
        public async Task<IActionResult> setUpdateTask(TaskUpdate comment)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
            /*    var url = await UploadFilesAsyn(formFiles); */
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.setReqUpdateColumn";
                _cmd += " @CmpId  ='" + comment.CmpId + "'";
                _cmd += ", @TicketId  ='" + comment.TicketId + "'";
                _cmd += ", @RouteId  ='" + comment.RouteId + "'";
                _cmd += ", @updUser  ='" + comment.updUser + "'";

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

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        private async Task<List<string>> UploadFilesAsyn(List<IFormFile> formFiles)
        {
            if (formFiles == null || formFiles.Count == 0)
            {
                return null;
            }

            try
            {
                (string errorMessage, List<string> imageName) =
                    await productService.UploadMultiFilesReq(formFiles);
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    return null;
                }

                return imageName;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("setReqStatus")]
        public async Task<IActionResult> ReqUpdateStatus(ReqUpdateStatus data)
        {
            string _cmd;
            _cmd = "exec  dbo.setReqStatus";
            _cmd += " @CmpId  ='" + data.cmpid + "'";
            _cmd += ", @status  ='" + data.status + "'";
            _cmd += ", @ticketId  ='" + data.ticketId + "'";
            if (DB.DBConn.ExecuteOnly(_cmd))
            {
                return StatusCode((int)HttpStatusCode.OK);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost("setReqPriority")]
        public async Task<IActionResult> setPriority(ReqPriority data)
        {
            string _cmd;
            _cmd = "exec  dbo.setReqPriority";
            _cmd += " @CmpId  ='" + data.cmpid + "'";
            _cmd += ", @priority  ='" + data.priority + "'";
            _cmd += ", @ticketId  ='" + data.ticketId + "'";
            if (DB.DBConn.ExecuteOnly(_cmd))
            {
                return StatusCode((int)HttpStatusCode.OK);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost("setChangeOwner")]
        public async Task<IActionResult> setChangeOwner(List<ReqFromCustOwner> data)
        {
            MsgReturn msgretrun = new MsgReturn();

            string _cmd;

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                if (data.Count > 0)
                {
                    _cmd = " delete from  dbo.CustomerReqOwner  ";
                    _cmd += " where  CmpId  ='" + data[0].CmpId + "'";
                    _cmd += " and  TicketId  ='" + data[0].TicketId + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < data.Count; i++)
                {
                    _cmd = "exec  dbo.[setChangeOwner]";
                    _cmd += " @CmpId  ='" + data[i].CmpId + "'";
                    _cmd += ", @AccountID  =" + data[i].UserId + "";
                    _cmd += ", @TicketId  ='" + data[i].TicketId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return NotFound(msgretrun);
                    }
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }
    }
}
