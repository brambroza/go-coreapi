using coreapi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Net;
using System.Text;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google;
using System.Text.Json;
using Line.Messaging.Webhooks;


namespace coreapi.Controllers
{

    [ApiController]
    public class HookLineController : ControllerBase
    {

 
       [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] WebhookEvent[] events)
        {
 
            try
            {

                string _cmd;

                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();



                foreach (var ev in events)
                {
                    // Handle each event type (e.g., MessageEvent, FollowEvent, etc.)
                    switch (ev)
                    {
                        case MessageEvent messageEvent:
                            if (messageEvent.Message is TextEventMessage textMessage)
                            {
                                // Respond to a text message 
                                var replyToken = messageEvent.ReplyToken;
                                var userId = messageEvent.Source.UserId ?? string.Empty;
                                var messageId = messageEvent.Message.Id ?? string.Empty;
                                var messageType = messageEvent.Message.Type.ToString() ?? string.Empty;
                                
                                var timestamp = messageEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

                                // Use a parameterized query to prevent SQL injection
                                _cmd = "exec dbo.setLineChatMessage @CmpId, @TimeStamp, @Id, @UserId, @Type, @ReplyToken, @QuotaToken, @Text, @StickerId, @StickerResourceType";

                                DB.DBConn.Cmd.CommandText = _cmd;

                                // Add SQL parameters
                                DB.DBConn.Cmd.Parameters.AddWithValue("@CmpId", "230015");
                                DB.DBConn.Cmd.Parameters.AddWithValue("@TimeStamp", timestamp);
                                DB.DBConn.Cmd.Parameters.AddWithValue("@Id", messageId);
                                DB.DBConn.Cmd.Parameters.AddWithValue("@UserId", userId);
                                DB.DBConn.Cmd.Parameters.AddWithValue("@Type", messageType);
                                DB.DBConn.Cmd.Parameters.AddWithValue("@ReplyToken", replyToken);
                                DB.DBConn.Cmd.Parameters.AddWithValue("@QuotaToken", "");
                                DB.DBConn.Cmd.Parameters.AddWithValue("@Text", textMessage.Text);

                                // Handle stickers if they exist (replace or remove these lines based on actual use)
                                DB.DBConn.Cmd.Parameters.AddWithValue("@StickerId", DBNull.Value); // Assuming no sticker
                                DB.DBConn.Cmd.Parameters.AddWithValue("@StickerResourceType", DBNull.Value); // Assuming no sticker resource type


                                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                                {
                                    DB.DBConn.Tran.Rollback();
                                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                                    return BadRequest();
                                };


                            }
                            break;
                    }
                }






                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return Ok();
            }
            catch (GoogleApiException ex)
            {
                // Log the error message
                Console.WriteLine($"Google API Error: {ex.Message}");
                throw;
            }


        }

    }

}