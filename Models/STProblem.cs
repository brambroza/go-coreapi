using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
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
        public string CmpId { get; set; }
        public string ProvinceId { get; set; }

        public string GrpId { get; set; } = "2300005";
    }

    public class MAGrp
    {
        public string grpid { get; set; }
        public string grpname { get; set; }
        public string grpdesciption { get; set; }
        public List<STProblemTask> items { get; set; }
    }

    public class STProblemTask
    {
        public string UpdUser { get; set; }
        public int ProblemId { get; set; }
        public string ReceiveDate { get; set; }
        public string CustCode { get; set; }
        public string RequestBy { get; set; }
        public string ProblemDetails { get; set; }

        public string ReceiveTime { get; set; }

        public string ProblemType { get; set; }

        public string CustBranchName { get; set; }
        public string CmpId { get; set; }
        public string ProvinceId { get; set; }

        public string GrpId { get; set; } = "2300005";

        public string CustomerName { get; set; }
        public string imgPath { get; set; }
        public int Progress { get; set; }

        public int ServiceActionId { get; set; }
        public string ServiceActionBy { get; set; }
        public int ServiceType { get; set; }
        public string ActionDetails { get; set; }
        public string FinishDate { get; set; }
        public string FinishTime { get; set; }
    }

    public class MaTaskMoveModel
    {
        public string CreateUser { get; set; }
        public string CmpId { get; set; }
        public string GrpId { get; set; }
        public int ProblemId { get; set; }
    }
}
