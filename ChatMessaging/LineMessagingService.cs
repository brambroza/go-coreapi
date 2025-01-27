using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using goalongapi.Models;


namespace goalongapi
{
    public class LineMessagingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _channelAccessToken;

        public LineMessagingService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _channelAccessToken = "";
        }

        public async Task SendMessage(string userId, string messageText, string channelAccessToken)
        {
            var lineMessage = new LinePushMessage
            {
                to = userId,
                messages = new List<LineMessageModel>
            {
                new LineMessageModel { text = messageText }
            }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(lineMessage),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {channelAccessToken}");

            var response = await _httpClient.PostAsync("https://api.line.me/v2/bot/message/push", jsonContent);

            response.EnsureSuccessStatusCode();
        }
    }

}