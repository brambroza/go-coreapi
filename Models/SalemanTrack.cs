using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class SalemanTrack
    {
        public string UpdUser { get; set; }
        public string SalemanTrackNo { get; set; }
        public string TransDate { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Contact { get; set; }
        public string BusinessType { get; set; }
        public string ReferOrigin { get; set; }
        public int Seq { get; set; }
        public string ContactStatus { get; set; }
        public string Description { get; set; }
        public DateTime ActionDate { get; set; }
        public string SaleStatus { get; set; }

        public string DocRef { get; set; }
        public string CustNickName { get; set; }
        public List<SalemanTask> salemanTasks {get;set;}
        public List<SalemanAsign> salemanAsigns { get; set; }
    }

    public class SalemanTask
    {
        public string UpdUser { get; set; }
        public string SalemanTrackNo { get; set; }
        public int Seq { get; set; }
        public string Description { get; set; }
        public string ActionDate { get; set; }
        public int Status { get; set; }
    }

    public class SalemanAsign
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
    }

    public class SalemanApp
    {
        public string UpdUser { get; set; }
        public string SalemanTrackNo { get; set; }
        public int SaleStatus { get; set; }
    }

}