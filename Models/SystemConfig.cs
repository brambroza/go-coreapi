namespace goalongapi.Installers
{
    public static class SystemConfig
    {
        public static string _ConnectionString { get; set; }
    }

    public class setNotitfication
    {
        public string cmpid { get; set; }
        public string userlogin { get; set; }
    }

    public class setReadNotification
    {
        public string cmpid { get; set; }
        public int userId { get; set; }
    }
}
