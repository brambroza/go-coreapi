namespace goalongapi.Models
{
    public class TicketCommnetConversation
    {
        public string id { get; set; }
        public string type { get; set; }
        public string cmpId { get; set; }
        public string ticketId { get; set; }
        public int unreadCount { get; set; }
        public List<TicketCommentMessage> messages { get; set; }
        public List<TicketCommentParticipant> participants { get; set; }
    }

    public class TicketCommentMessage
    {
        public string id { get; set; }
        public string body { get; set; }
        public string senderId { get; set; }
        public string contentType { get; set; }
        public DateTime createdAt { get; set; }
        public string ticketId { get; set; }
        public string cmpId { get; set; }
        public List<TicketCommentAttachment> attachments { get; set; }
    }

    public class TicketCommentAttachment
    {
        public string id { get; set; }
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string ticketId { get; set; }
        public string cmpId { get; set; }
    }

    public class TicketCommentParticipant
    {
        public string id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string avatarUrl { get; set; }
        public string phoneNumber { get; set; }
        public DateTime lastActivity { get; set; }
        public string ticketId { get; set; }
        public string cmpId { get; set; }
        public string status { get; set; }
    }
}
