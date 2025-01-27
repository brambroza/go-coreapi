using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class STServiceActions
    {
        public string UpdUser { get; set; }
        public int ServiceActionId { get; set; }
        public int ProblemId { get; set; }
        public string ServiceActionBy { get; set; }
        public int ServiceType { get; set; }
        public string ActionDetails { get; set; }
        public string FinishDate { get; set; }
        public string FinishTime { get; set; }
        public string CmpId {get;set;}

      
    }

    public class STServiceAction_Emp
    { 
        public string Username { get; set; }
    }
}