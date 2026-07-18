using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{



    public class ReasonTicket
    {
        public string updUser { get; set; }
        public string Reason { get; set; }
        public string CmpId { get; set; }
    }


    public class ReasonCloseTicket
    {
        public DateTime NotificationAgain { get; set; }
        public string type { get; set; }
        public string TicketId { get; set; }
        public string updUser { get; set; }
        public string Reason { get; set; }
        public string CmpId { get; set; }
         public int RecurringEvery { get; set; }
        public int IntervalType { get; set; }
        public DateTime EveryDay { get; set; }
        public string ExpiresType { get; set; }
        public DateTime ExpiresDate { get; set; }
        public int ExpiresCount { get; set; }




    }

}
