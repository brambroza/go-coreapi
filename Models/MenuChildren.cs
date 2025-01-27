using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class MenuChildren
    {
        public int MenuId { get; set; }

        public int MenuMainId { get; set; }

        public string title { get; set; }

        public string icon { get; set; }

        public string to { get; set; }

        public string link { get; set; }
        public int StateActive { get; set; }

        public int SubOverViewSales {get;set;}
        public int SubOverViewCust { get;set;}
        public int SubOverViewVendor {get;set;}


    }
}