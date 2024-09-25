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


namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]
    public class LineMessageController : ControllerBase
    {

        private readonly IWebHostEnvironment webHostEnvironment;

        static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static readonly string ApplicationName = "GoogleSheetsWeb";
        static readonly string SpreadsheetId = "1wWXago6DcibLIzVwpRQhboyD6mVIuKnK5Ox6gJU0Hf0";  // Replace with your Google Sheet ID
        static readonly string SheetRange = "Sheet1!A2:A101";  // Adjust the range based on your data


        public LineMessageController(IWebHostEnvironment webHostEnvironment)
        {

            this.webHostEnvironment = webHostEnvironment;
        }



        [HttpGet("[action]")]

        public async Task<IActionResult> getLineFriend([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getLineFriend @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);


            var res = new List<LineContactProfile>();

            foreach (DataRow r in dt.Rows)
            {
                var rd = new LineContactProfile();
                rd.CmpId = r["CmpId"].ToString();
                rd.userId = r["UserId"].ToString();
                LineProfile datares = await getprofile(rd.userId, "zHOdhlkJkcfWa4Hzm4nFQORzqCogEKj9PDUttOurALA2KjMdl0l9cwhRVRdXhYSFlIVOmrP1vP7DCA3aIt5u4B6CtsrNSW3Gj1Ud8BX5BWKiq1MbJS9GpadBBFBjImJOslCyMGHihEcgq0deVVXmHQdB04t89/1O/w1cDnyilFU=");

                rd.displayName = datares.DisplayName.ToString();
                rd.pictureUrl = datares.PictureUrl.ToString();
                rd.language = datares.Language.ToString();


                res.Add(rd);

            }

            /*  string JSONString = string.Empty;
             JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
  */

            return Ok(res);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> getChatConvertsation([FromQuery] string cmpid)
        {


            string _cmd;
            _cmd = "exec dbo.getLineFriend @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);



            _cmd = "exec dbo.getLineChatConvertsatition @CmpId='" + cmpid + "'";
            DataTable dtc = DB.DBConn.GetDataTable(_cmd);

            var res = new List<LineChatConvertsation>();

            foreach (DataRow r in dt.Rows)
            {
                var rd = new LineChatConvertsation();
                rd.CmpId = r["CmpId"].ToString();
                rd.id = r["UserId"].ToString();
                rd.type = "text";
                rd.unreadCount = 0;

                rd.messages = new List<LineChatMessage>();


                foreach (DataRow d in dtc.Select("userId='" + rd.id + "'"))
                {
                    var msg = new LineChatMessage();
                    msg.id = d["Id"].ToString();
                    msg.userId = rd.id;
                    msg.replyToken = d["replyToken"].ToString();
                    msg.quotaToken = d["quotaToken"].ToString();
                    msg.text = d["text"].ToString();
                    msg.type = d["type"].ToString();
                    msg.timestamp = Convert.ToDateTime(d["TimeStamp"]);


                    rd.messages.Add(msg);
                }

                rd.participants = new List<LineProfile>();
                foreach (DataRow rx in dt.Select("UserId='" + rd.id + "'"))
                {
                    var rp = new LineProfile();

                    rp.UserId = rx["UserId"].ToString();
                    LineProfile datares = await getprofile(rp.UserId, "zHOdhlkJkcfWa4Hzm4nFQORzqCogEKj9PDUttOurALA2KjMdl0l9cwhRVRdXhYSFlIVOmrP1vP7DCA3aIt5u4B6CtsrNSW3Gj1Ud8BX5BWKiq1MbJS9GpadBBFBjImJOslCyMGHihEcgq0deVVXmHQdB04t89/1O/w1cDnyilFU=");

                    rp.DisplayName = datares.DisplayName.ToString();
                    rp.PictureUrl = datares.PictureUrl.ToString();
                    rp.Language = datares.Language.ToString();
                    rp.status = "online";
                    rp.lastActivity = DateTime.Now;



                    rd.participants.Add(rp);

                }




                res.Add(rd);

            }

            /*  string JSONString = string.Empty;
             JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
  */

            return Ok(res);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> getChatConvertsationUserId([FromQuery] string cmpid, string userId)
        {


            string _cmd;
            _cmd = "exec dbo.getLineFriendUserId @CmpId='" + cmpid + "' , @userid='" + userId + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);



            _cmd = "exec dbo.getLineChatConvertsatitionUserId @CmpId='" + cmpid + "', @userid='" + userId + "'";
            DataTable dtc = DB.DBConn.GetDataTable(_cmd);


            var rd = new LineChatConvertsation();
            foreach (DataRow r in dt.Rows)
            {

                rd.CmpId = r["CmpId"].ToString();
                rd.id = r["UserId"].ToString();
                rd.type = "text";
                rd.unreadCount = 0;

                rd.messages = new List<LineChatMessage>();


                foreach (DataRow d in dtc.Select("userId='" + rd.id + "'"))
                {
                    var msg = new LineChatMessage();
                    msg.id = d["Id"].ToString();
                    msg.userId = d["chatId"].ToString();
                    msg.replyToken = d["replyToken"].ToString();
                    msg.quotaToken = d["quotaToken"].ToString();
                    msg.text = d["text"].ToString();
                    msg.type = d["type"].ToString();
                    msg.timestamp = Convert.ToDateTime(d["TimeStamp"]);

                    if (msg.type == "image")
                    {
                        var imagePath = await DownloadImageAsync(msg.id, "zHOdhlkJkcfWa4Hzm4nFQORzqCogEKj9PDUttOurALA2KjMdl0l9cwhRVRdXhYSFlIVOmrP1vP7DCA3aIt5u4B6CtsrNSW3Gj1Ud8BX5BWKiq1MbJS9GpadBBFBjImJOslCyMGHihEcgq0deVVXmHQdB04t89/1O/w1cDnyilFU=");
                        /* Console.WriteLine($"Image saved at: {imagePath}"); */
                        msg.attachments = new List<AttachFileUrl>();
                        var att = new AttachFileUrl();
                        att.Url = imagePath;
                        att.id = msg.id;
                        att.createdAt = msg.timestamp;
                        att.type = "image";
                        msg.attachments.Add(att);


                    }
                    if (msg.type == "sticker")
                    {
                        msg.attachments = new List<AttachFileUrl>();
                        var att = new AttachFileUrl();
                        att.stickerId = d["stickerId"].ToString();
                        att.stickerType = d["stickerResourceType"].ToString();
                        att.type = "sticker";
                        att.Url = "";
                        att.id = msg.id;
                        att.createdAt = msg.timestamp;

                        msg.attachments.Add(att);
                    }


                    rd.messages.Add(msg);
                }




                rd.participants = new List<LineProfile>();
                foreach (DataRow rx in dt.Select("UserId='" + rd.id + "'"))
                {
                    var rp = new LineProfile();

                    rp.UserId = rx["UserId"].ToString();
                    LineProfile datares = await getprofile(rp.UserId, "zHOdhlkJkcfWa4Hzm4nFQORzqCogEKj9PDUttOurALA2KjMdl0l9cwhRVRdXhYSFlIVOmrP1vP7DCA3aIt5u4B6CtsrNSW3Gj1Ud8BX5BWKiq1MbJS9GpadBBFBjImJOslCyMGHihEcgq0deVVXmHQdB04t89/1O/w1cDnyilFU=");

                    rp.DisplayName = datares.DisplayName.ToString();
                    rp.PictureUrl = datares.PictureUrl.ToString();
                    rp.Language = datares.Language.ToString();
                    rp.status = "online";
                    rp.lastActivity = DateTime.Now;



                    rd.participants.Add(rp);

                }






            }

            /*  string JSONString = string.Empty;
             JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
  */

            return Ok(rd);
        }





        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(LineMessageRequest request)
        {
           await SendMessage(request.UserId, request.Message, request.ChanelLineToken);

            string _cmd;
            _cmd = "exec  dbo.setLineChatMessage";
            _cmd += " @CmpId  ='" + request.cmpid + "'";
            _cmd += ",@TimeStamp =0";
            _cmd += ",@id  ='" + request.id + "'";
            _cmd += ",@userId  ='" + request.UserId + "'";
            _cmd += ",@type ='" + request.type + "'";
            _cmd += ",@replyToken =''";
            _cmd += ",@quotaToken  =''";
            _cmd += ",@text  ='" + request.Message + "'";
            _cmd += ",@stickerId  =''";
            _cmd += ",@stickerResourceType  =''";
            _cmd += ",@sendbyId  ='" + request.sendbyId + "'";

            DB.DBConn.ExecuteOnly(_cmd);

            return Ok(new { message = "Message sent" });
        }


        [HttpGet("[action]")]

        public IActionResult getChatToken([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getSocailContact @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dt);


            return Ok(JSONString);
        }



        private async Task SendMessage(string userid, string msg, string token)
        {

            try
            {

                HttpClient _httpClient;
                _httpClient = new HttpClient();


                var lineMessage = new LinePushMessage
                {
                    to = userid,
                    messages = new List<LineMessage>
                        {
                            new LineMessage { text = msg }
                        }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(lineMessage),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.PostAsync("https://api.line.me/v2/bot/message/push", jsonContent);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private async Task<string> DownloadImageAsync(string messageId, string token)
        {
            HttpClient _httpClient;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.GetAsync($"https://api-data.line.me/v2/bot/message/{messageId}/content");

            response.EnsureSuccessStatusCode();

            // Save the image to a local path
            var pathto = $"{webHostEnvironment.WebRootPath}/images/chat";
            var filePath = Path.Combine(pathto, $"{messageId}.jpg");

            if (IsValidPaths(pathto))
            {
                if (!Directory.Exists(pathto))
                {
                    Directory.CreateDirectory(pathto);
                }

            }


            await using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }

            return $"images/chat/{messageId}.jpg";
        }


        private bool IsValidPaths(string path)
        {
            try
            {
                // This will check for any invalid characters in the path
                Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private async Task<LineProfile> getprofile(string userid, string token)
        {
            try
            {
                HttpClient _httpClient;
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync($"https://api.line.me/v2/bot/profile/{userid}");

                response.EnsureSuccessStatusCode();


                var jsonString = await response.Content.ReadAsStringAsync();
                // var profile = JsonSerializer.Deserialize<LineProfile>(jsonString);
                LineProfile profile = JsonSerializer.Deserialize<LineProfile>(jsonString);
                return profile;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }




        [HttpGet("getSheetData")]
        public IActionResult FetchGoogleSheetData()
        {
            var sheetData = GetSheetData();

            if (sheetData != null && sheetData.Count > 0)
            {
                return Ok(sheetData);  // Return data as JSON
            }

            return NotFound("No data found.");
        }



        private IList<IList<object>> GetSheetData()
        {
            UserCredential credential;
            GoogleCredential credentials;

            /*  using (var stream = new FileStream($"{webHostEnvironment.WebRootPath}/googlesheet.json", FileMode.Open, FileAccess.Read))
             {
                 credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                     GoogleClientSecrets.FromStream(stream).Secrets,
                     Scopes,
                     "user",
                     CancellationToken.None).Result;
             } */

            using (var stream = new FileStream($"{webHostEnvironment.WebRootPath}/.json", FileMode.Open, FileAccess.Read))
            {
                credentials = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }



            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = ApplicationName,
            });

            /*   SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, SheetRange);
              ValueRange response = request.Execute();
              return response.Values; */

            try
            {
                var request = service.Spreadsheets.Values.Get(SpreadsheetId, SheetRange);
                var response = request.Execute();


                // Define the table headers
                /*    Console.WriteLine("| {0,-35} | {1,-10} | {2,-10} | {3,-20} | {4,-10} | {5,-20} | {6,-20} | {7,-40} |",
                       "User ID", "Type", "Message Type", "Quote Token", "Text", "Message ID", "Reply Token", "Timestamp");
                   Console.WriteLine(new string('-', 200)); */

                // Iterate through the events and display the data
                string _cmd;

                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();



                foreach (var data in response.Values)
                {

                    var jsonData = $"{data[0]}";
                    var jsond = ConvertToValidJson(jsonData);


                    EventData eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<EventData>(jsond);

                    /* 

                                        Console.WriteLine("| {0,-35} | {1,-10} | {2,-12} | {3,-20} | {4,-10} | {5,-20} | {6,-20} | {7,-40} |",
                                            eventData.Source.UserId,
                                            eventData.Type,
                                            eventData.Message.Type,
                                            Truncate(eventData.Message.QuoteToken, 18),
                                            Truncate(eventData.Message.Text, 8),
                                            eventData.Message.Id,
                                            eventData.ReplyToken,
                                            ConvertTimestampToDateTime(eventData.Timestamp)); */


                    _cmd = "exec  dbo.setLineChatMessage";
                    _cmd += " @CmpId  ='230015'";
                    _cmd += ",@TimeStamp =" + eventData.Timestamp;
                    _cmd += ",@id  ='" + eventData.Message.Id + "'";
                    _cmd += ",@userId  ='" + eventData.Source.UserId + "'";
                    _cmd += ",@type ='" + eventData.Message.Type + "'";
                    _cmd += ",@replyToken ='" + eventData.ReplyToken + "'";
                    _cmd += ",@quotaToken  ='" + eventData.Message.QuoteToken + "'";
                    _cmd += ",@text  ='" + eventData.Message.Text + "'";
                    _cmd += ",@stickerId  ='" + eventData.Message.StickerId + "'";
                    _cmd += ",@stickerResourceType  ='" + eventData.Message.StickerResourceType + "'";


                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return null;
                    };



                }



                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return response.Values;
            }
            catch (GoogleApiException ex)
            {
                // Log the error message
                Console.WriteLine($"Google API Error: {ex.Message}");
                throw;
            }


        }


        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private string ConvertTimestampToDateTime(double timestamp)
        {
            // Convert timestamp in milliseconds to DateTime
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp);
            return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
        }





        private string ConvertToValidJson(string invalidJson)
        {
            // Step 1: Replace equal signs with colons
            string json = invalidJson.Replace("=", ":");
            json = json.Replace("[Ljava.lang.Object;", "");

            // Step 2: Add double quotes around keys
            json = System.Text.RegularExpressions.Regex.Replace(json, @"(?<={|,)\s*(\w+)\s*:", "\"$1\":");

            // Step 3: Add double quotes around string values
            json = System.Text.RegularExpressions.Regex.Replace(json, @":\s*([^{}\[\],]+)\s*(?=[,}])", m =>
            {
                var value = m.Groups[1].Value.Trim();
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    return ":" + value;
                if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^(-?\d+(\.\d+)?([eE][+-]?\d+)?)$")) // Number
                    return ":" + value;
                if (value == "true" || value == "false" || value == "null") // Boolean or null
                    return ":" + value;
                return ":\"" + value + "\""; // String
            });

            // Step 4: Remove any extra parentheses at the start and end
            json = json.Trim();
            if (json.StartsWith("(") && json.EndsWith(")"))
            {
                json = json.Substring(1, json.Length - 2);
            }

            // Step 5: Handle escaped quotes within the values (if any)
            json = json.Replace("\\\"", "\"");

            // Step 6: Return the valid JSON string
            return json;
        }





    }


}

