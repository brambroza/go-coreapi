using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{

    public class TimeCard
    {
        public string UserName { get; set; }
        public string TransDate { get; set; }
        public string TransTime { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string status { get; set; }
    }

    public class EmpWorkingOnsite
    {
        public string? UpdUser { get; set; }

        public string CmpId { get; set; } = null!;
        public int AccountId { get; set; }

        public string Customer { get; set; } = null!;
        public string? SiteName { get; set; }
        public string? Description { get; set; }

        public string TransDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string? EmployeeCode { get; set; }
    }
}