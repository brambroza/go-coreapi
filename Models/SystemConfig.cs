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


    public class TermsService  {
        public string Id {get;set;}
        public string Name {get;set;}
        public string Description {get;set;}
        public int StateActive {get;set;}
        public string CmpId {get;set;}
        public string UpdUser {get;set;}
        public string? CreateAt {get;set;  }
    }
}
