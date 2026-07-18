using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class MsgReturn
    {
        public string ReturnCode { get; set; }
        public string Msg { get; set; }
        public string CmpId { get; set; }
    }
}
