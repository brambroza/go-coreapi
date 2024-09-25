using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class MAFortigate
    {
          public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string model { set; get; }
        public string serial { set; get; }
        public string forticloud { set; get; }
        public string maDuration { set; get; }
        public string advanceReplacement { set; get; }
        public string sla { set; get; }
        public string additionalDetail { set; get; }   
        public string fromApp {get;set;} 
    }

     public class MACiscoServer
    {
        public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string model { set; get; }
        public string serial { set; get; }
        public string partNumber { set; get; }
        public string maBy { set; get; }
        public string maDuration { set; get; }
        public string advanceReplacement { set; get; }
        public string sla { set; get; }
        public string additionalDetail { set; get; } 
        public string fromApp {get;set;}
    }



     public class MAOther
    {
        public string cmpName { set; get; }
        public string contactName { set; get; }
        public string contactPhone { set; get; }
        public string contactEmail { set; get; }
        public string address { set; get; }
        public string contactPosition { set; get; }
        public string serviceType { set; get; }
        public string additionalDetail { set; get; }
        public string desiredService { set; get; } 
 
        public string fromApp {get;set;}
    }

    

}