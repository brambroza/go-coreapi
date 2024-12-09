using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class ContactList
    {
        public string UpdUser { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactPosition { get; set; }
        public string ContactLineId { get; set; }
        public string Remark { get; set; }
        public string ImgPath { get; set; }
        public string CmpId { get; set; }
        public string DocType {get;set; }
        public string DocNo {get;set;}
    }
}
