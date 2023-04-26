using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class employee
    {
    }

    public class timecard
    {
        public string UserName { get; set; }
        public string TransDate { get; set; }
        public string TransTime { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string status { get; set; }
    }
}