using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace goalongapi.Models
{

    public class LineMessageModel
    {
        public string type { get; set; } = "text";
        public string text { get; set; }
    }

    public class LinePushMessage
    {
        public string to { get; set; }
        public List<LineMessageModel> messages { get; set; }
    }

    public class LineMessageRequest
    {
        public string UserId { get; set; }
        public string Message { get; set; }

        public string ChanelLineToken { get; set; }

        public string type { get; set; }

        public string sendbyId { get; set; }

        public string id { get; set; }
        public string cmpid { get; set; }

    }
    public class LineContactProfile
    {
        public string CmpId { get; set; }
        public string userId { get; set; }
        public string displayName { get; set; }
        public string pictureUrl { get; set; }
        public string language { get; set; }

    }

    public class LineProfile
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("pictureUrl")]
        public string PictureUrl { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        public string status { get; set; }
        public DateTime lastActivity { get; set; }


    }


    public class AttachFile
    {
        public string name { get; set; }
        public decimal size { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public string preview { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }


    }


    public class AttachFileUrl
    {

        public string type { get; set; }
        public string Url { get; set; }
        public string id { get; set; }
        public DateTime createdAt { get; set; }

        public string stickerId { get; set; }
        public string stickerType { get; set; }


    }
    public class LineChatMessage
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string type { get; set; }
        public string replyToken { get; set; }
        public string quotaToken { get; set; }
        public string text { get; set; }
        public DateTime timestamp { get; set; }
        public List<AttachFileUrl> attachments { get; set; }
    }

    public class LineChatConvertsation
    {
        public string CmpId { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public int unreadCount { get; set; }
        public List<LineChatMessage> messages { get; set; }
        public List<LineProfile> participants { get; set; }

    }


    public class EventData
    {
        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("webhookEventId")]
        public string WebhookEventId { get; set; }

        [JsonPropertyName("source")]
        public Source Source { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("deliveryContext")]
        public DeliveryContext DeliveryContext { get; set; }

        [JsonPropertyName("replyToken")]
        public string ReplyToken { get; set; }
    }




    public class Source
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("quoteToken")]
        public string QuoteToken { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        // Additional properties for other message types (e.g., image, sticker)
        [JsonPropertyName("stickerId")]
        public string StickerId { get; set; }

        [JsonPropertyName("stickerResourceType")]
        public string StickerResourceType { get; set; }

        [JsonPropertyName("packageId")]
        public string PackageId { get; set; }
    }

    public class DeliveryContext
    {
        [JsonPropertyName("isRedelivery")]
        public bool IsRedelivery { get; set; }
    }




}