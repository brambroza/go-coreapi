using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data;
using System;
using Line.Messaging.Webhooks;
using System.Text;
using goalongapi.Hubs;
using Microsoft.AspNetCore.SignalR;
using goalongapi.Models;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HookLineController : ControllerBase
    {

        private readonly IHubContext<ChatHub> _hubContext;

        public HookLineController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }


        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] LineWebhookRequests events)
        {
            if (events?.Events == null || events.Events.Count == 0)
            {
                return Ok(new { Message = "Webhook processed successfully." });
            }

            try
            {
                using (var connection = DB.DBConn.Cnn) // Use 'using' for connection
                {
                    string userfromid = "";
                    string msgtext = "";
                    DB.DBConn.SqlConnectionOpen();
                    DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                    DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                    foreach (var ev in events?.Events)
                    {
                        if (ev.Type == "message")
                        {
                            // Get event details
                            var replyToken = ev.ReplyToken;
                            var userId = ev.Source.UserId ?? string.Empty;
                            var messageId = ev.Message.Id ?? string.Empty;
                            var messageType = ev.Message.Type.ToString() ?? string.Empty;
                            var messageText = ev.Message.Text.ToString() ?? string.Empty;
                            var timestamp = ev.Timestamp;
                            userfromid = userId;
                            msgtext = messageText;

                            // SQL Command
                            string _cmd = "";


                            _cmd = "exec  dbo.setLineChatMessage";
                            _cmd += " @CmpId  ='230015'";
                            _cmd += ",@TimeStamp =" + timestamp;
                            _cmd += ",@id  ='" + messageId + "'";
                            _cmd += ",@userId  ='" + userId + "'";
                            _cmd += ",@type ='" + messageType + "'";
                            _cmd += ",@replyToken ='" + replyToken + "'";
                            _cmd += ",@quotaToken  =''";
                            _cmd += ",@text  ='" + messageText + "'";
                            _cmd += ",@stickerId  =''";
                            _cmd += ",@stickerResourceType  =''";


                            if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                            {
                                DB.DBConn.Tran.Rollback();
                                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                                return StatusCode(500, new { Message = "Failed to execute transaction." });
                            }
                            DateTime dateTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                            var msg = new LineChatMessage();
                            msg.id = messageId;
                            msg.userId = userId;
                            msg.replyToken = replyToken;
                            msg.quotaToken = "";
                            msg.text = messageText;
                            msg.type = messageType;
                            msg.timestamp = dateTimestamp;


                            await _hubContext.Clients.All.SendAsync($"ReceiveMessageChat230015{userfromid}", msg);



                        }
                    }

                    // Commit the transaction
                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);


                }

                return Ok(new { Message = "Webhook processed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while processing the webhook.", Details = ex.Message });
            }
        }
    }

    public class LineWebhookRequests
    {
        public List<LineEvent> Events { get; set; }
    }


    public class LineEvent
    {
        public string Type { get; set; }
        public LineMessageText Message { get; set; }
        public string ReplyToken { get; set; }
        public Source Source { get; set; }
        public long Timestamp { get; set; }
    }

    public class LineMessageText
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
    }

    public class Source
    {
        public string UserId { get; set; }
        public string Type { get; set; } // เช่น "user"
    }
}
