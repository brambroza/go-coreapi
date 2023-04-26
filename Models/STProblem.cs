using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class STProblem
    {
        public string UpdUser { get; set; }      
        public int ProblemId { get; set; }
        public string ReceiveDate { get; set; }
        public string CustCode { get; set; }
        public string RequestBy { get; set; }
        public string ProblemDetails { get; set; }

        public string ReceiveTime { get; set; }

        public int ProblemType { get; set; }

        public string CustBranchName { get; set; }


    }
}