using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using coreapi.Models;
using goalongapi.Models;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coreapi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CommentTicketController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> getChatConvertsations(
            [FromQuery] string cmpid,
            [FromQuery] string endpoint
        )
        {
            string _cmd;
            _cmd = "exec dbo.ticket_getCommentConversation @CmpId='" + cmpid + "'  ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getCommentMessage @CmpId='" + cmpid + "'  ";
            DataTable dtc = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getCommentMessage_Attachment @CmpId='" + cmpid + "'  ";
            DataTable dtf = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getParticipant @CmpId='" + cmpid + "'  ";
            DataTable dtp = DB.DBConn.GetDataTable(_cmd);

            var res = new List<TicketCommnetConversation>();

            foreach (DataRow r in dt.Rows)
            {
                var rd = new TicketCommnetConversation();
                rd.cmpId = r["CmpId"].ToString();
                rd.id = r["TicketId"].ToString();
                rd.type = "commnet";
                rd.ticketId = r["TicketId"].ToString();

                rd.unreadCount = 0;

                rd.messages = new List<TicketCommentMessage>();

                foreach (DataRow d in dtc.Select("ticketId='" + rd.ticketId + "'"))
                {
                    var msg = new TicketCommentMessage();
                    msg.id = d["id"].ToString();
                    msg.body = d["body"].ToString();
                    msg.senderId = d["senderId"].ToString();
                    msg.contentType = d["contentType"].ToString();
                    msg.ticketId = d["ticketId"].ToString();
                    msg.cmpId = d["cmpId"].ToString();
                    msg.createdAt = Convert.ToDateTime(d["createdAt"]);

                    msg.attachments = new List<TicketCommentAttachment>();

                    foreach (
                        DataRow x in dtf.Select(
                            "ticketId='" + rd.ticketId + "' and messageId='" + msg.id + "'"
                        )
                    )
                    {
                        var msgf = new TicketCommentAttachment()
                        {
                            id = x["id"].ToString(),
                            name = x["name"].ToString(),
                            size = int.Parse(x["size"].ToString()),
                            type = x["type"].ToString(),
                            path = x["path"].ToString(),
                            ticketId = x["ticketId"].ToString(), 
                            cmpId = x["cmpId"].ToString(),
                            createdAt = Convert.ToDateTime(x["createdAt"].ToString()),
                            modifiedAt = Convert.ToDateTime(x["modifiedAt"].ToString()),
                        };

                        msg.attachments.Add(msgf);
                    }

                    rd.messages.Add(msg);
                }

                rd.participants = new List<TicketCommentParticipant>();

                foreach (DataRow d in dtp.Select("ticketId='" + rd.ticketId + "'"))
                {
                    var par = new TicketCommentParticipant()
                    {
                        id = d["id"].ToString(),
                        name = d["name"].ToString(),
                        role = "",
                        email = "",
                        address = "",
                        avatarUrl = d["avatarUrl"].ToString(),
                        phoneNumber = "",
                        cmpId = d["cmpId"].ToString(),
                        ticketId = d["ticketId"].ToString(),
                        status = "",
                        lastActivity = DateTime.Now,
                    };

                    rd.participants.Add(par);
                }

                res.Add(rd);
            }

            return Ok(new { Conversations = res });
        }

        [HttpGet("conversation")]
        public async Task<IActionResult> getChatConvertsation(
            [FromQuery] string cmpid,
            [FromQuery] string conversationId,
            [FromQuery] string ticketId,
            [FromQuery] string endpoint
        )
        {
            string _cmd;
            _cmd = "exec dbo.ticket_getCommentConversation @CmpId='" + cmpid + "'  ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getCommentMessage @CmpId='" + cmpid + "'  ";
            DataTable dtc = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getCommentMessage_Attachment @CmpId='" + cmpid + "'  ";
            DataTable dtf = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.ticket_getParticipant @CmpId='" + cmpid + "'  ";
            DataTable dtp = DB.DBConn.GetDataTable(_cmd);

            var rd = new TicketCommnetConversation();

            foreach (DataRow r in dt.Select("TicketId='" + ticketId + "'"))
            {
                rd.cmpId = r["CmpId"].ToString();
                rd.id = r["TicketId"].ToString();
                rd.type = "commnet";
                rd.ticketId = r["TicketId"].ToString();

                rd.unreadCount = 0;

                rd.messages = new List<TicketCommentMessage>();

                foreach (DataRow d in dtc.Select("ticketId='" + rd.ticketId + "'"))
                {
                    var msg = new TicketCommentMessage();
                    msg.id = d["id"].ToString();
                    msg.body = d["body"].ToString();
                    msg.senderId = d["senderId"].ToString();
                    msg.contentType = d["contentType"].ToString();
                    msg.ticketId = d["ticketId"].ToString();
                    msg.cmpId = d["cmpId"].ToString();
                    msg.createdAt = Convert.ToDateTime(d["createdAt"]);

                    msg.attachments = new List<TicketCommentAttachment>();

                    foreach (
                        DataRow x in dtf.Select(
                            "ticketId='" + rd.ticketId + "' and messageId='" + msg.id + "'"
                        )
                    )
                    {
                        var msgf = new TicketCommentAttachment()
                        {
                            id = x["id"].ToString(),
                            name = x["name"].ToString(),
                            size = int.Parse(x["size"].ToString()),
                            type = x["type"].ToString(),
                            path = x["path"].ToString(),
                            ticketId = x["ticketId"].ToString(),
                             
                            cmpId = x["cmpId"].ToString(),
                            createdAt = Convert.ToDateTime(x["createdAt"].ToString()),
                            modifiedAt = Convert.ToDateTime(x["modifiedAt"].ToString()),
                        };

                        msg.attachments.Add(msgf);
                    }

                    rd.messages.Add(msg);
                }

                rd.participants = new List<TicketCommentParticipant>();

                foreach (DataRow d in dtp.Select("ticketId='" + rd.ticketId + "'"))
                {
                    var par = new TicketCommentParticipant()
                    {
                        id = d["id"].ToString(),
                        name = d["name"].ToString(),
                        role = "",
                        email = "",
                        address = "",
                        avatarUrl = d["avatarUrl"].ToString(),
                        phoneNumber = "",
                        cmpId = d["cmpId"].ToString(),
                        ticketId = d["ticketId"].ToString(),
                        status = "",
                        lastActivity = DateTime.Now,
                    };

                    rd.participants.Add(par);
                }
            }

            return Ok(new { Conversation = rd });
        }

        [HttpPut("conversation")]
        public IActionResult setMessageComment(TicketCommentMessage mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.ticket_setComment";
                _cmd += " @id  ='" + mt.id + "'";
                _cmd += ",@cmpId ='" + mt.cmpId + "'";
                _cmd += ",@body ='" + mt.body + "'";
                _cmd += ",@senderId  ='" + mt.senderId + "'";
                _cmd += ",@ticketId  ='" + mt.ticketId + "'";
                _cmd += ",@contentType  ='" + mt.contentType + "'";
                _cmd +=
                    ",@createdAt  ='"
                    + mt.createdAt.ToString("yyyy-MM-dd HH:mm:ss", thaiCulture)
                    + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
                }

                for (int i = 0; i < mt.attachments.Count; i++)
                {
                    _cmd = "exec  dbo.ticket_setComment_attachment";

                    _cmd += "  @id  ='" + mt.attachments[i].id + "'";
                    _cmd += ",@name  ='" + mt.attachments[i].name + "'";
                    _cmd += ",@size =" + mt.attachments[i].size;
                    _cmd += ",@path  ='" + mt.attachments[i].path + "'";
                    _cmd += ",@type  ='" + mt.attachments[i].type + "'";
                    _cmd +=
                        ",@createdAt  ='"
                        + mt.attachments[i].createdAt.ToString("yyyy-MM-dd HH:mm:ss", thaiCulture)
                        + "'";
                    _cmd +=
                        ",@modifiedAt  ='"
                        + mt.attachments[i].modifiedAt.ToString("yyyy-MM-dd HH:mm:ss", thaiCulture)
                        + "'";
                    _cmd += ",@ticketId  ='" + mt.attachments[i].ticketId + "'";
                    _cmd += ",@cmpId =" + mt.attachments[i].cmpId;
                    _cmd += ",@messageId  ='" + mt.id + "'";

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
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest(ex.Message);
            }
        }
    }
}
