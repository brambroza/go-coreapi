namespace coreapi.Models
{
    public class Notification
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsUnRead { get; set; }
        public bool IsUnAlert { get; set; }
        public string urllink { get; set; }
    }
}
