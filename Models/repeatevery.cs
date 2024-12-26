namespace coreapi.Models
{
    public class RepeatEvery
    {
        public string UpdUser { get; set; }
        public string RepeatEveryId { get; set; }
        public string DocNo { get; set; }
        public string DocType { get; set; }
        public int RecurringEvery { get; set; }
        public int IntervalType { get; set; }
        public DateTime EveryDay { get; set; }
        public string ExpiresType { get; set; }
        public DateTime ExpiresDate { get; set; }
        public int ExpiresCount { get; set; }
        public string CmpId { get; set; }

        public int RevNo { get; set; }
    }
}
