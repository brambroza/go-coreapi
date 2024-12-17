using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Web;

namespace coreapi.Models
{
    public class Calendar
    {
        public string username { get; set; }
        public string calendarId { get; set; }
        public string cmpId { get; set; }
        public string color { get; set; }
        public bool allDay { get; set; }
        public string description { get; set; }
        public DateTime end { get; set; }
        public DateTime start { get; set; }
        public string location { get; set; }
        public string title { get; set; }
        public string customerName { get; set; }
        public string ticketId { get; set; }
        public List<Invite> invite { get; set; }
        public string ticketIdRef { get; set; }
    }

    public class Invite
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ImgPath { get; set; }
        public string FullName { get; set; }
    }
}
