using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class SystemEventLog
    {
        public string UpdUser { get; set; }
        public string Id { get; set; }
        public string RepeatEveryId { get; set; }
        public string DocNo { get; set; }
        public string DocType { get; set; }
        public string ExpiresType { get; set; }
        public string EveryDay { get; set; }
        public string CmpId { get; set; }
        public string EventName { get; set; }
        public string CustomerName { get; set; }
        public string ImgPath { get; set; }
        public int Status { get; set; }
        public string Msg { get; set; }
        public string ModifyDate { get; set; }
        public string ModifyBy { get; set; }
        public string DocNoNew { get; set; }
    }
}
