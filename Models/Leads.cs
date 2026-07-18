using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class Leads  {      
    public string UpdUser { get; set; }
    public string CustCodeNo { get; set; }
    public string TransDate { get; set; }
    public string CustName { get; set; }
    public int CustRefTypeId { get; set; }
    public string Topic { get; set; }
    public string Phone { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
    public string CompanyAddr { get; set; }
    public string CustNickName { get; set; }
    public List<Leads_Task> LeadsTasks { get; set; }

    public List<LeadsAsign> leadsAsigns { get; set; }

    }

    public class Leads_Task 
    {      
    public string UpdUser { get; set; } 
    public string CustCodeNo { get; set; }
    public string TransDate { get; set; }
    public int Seq { get; set; }
    public string Description { get; set; }
    }

    public class LeadsQualify
    {
        public string UpdUser { get; set; }
        public string CustCodeNo { get; set; }
        public int QualifyState { get; set; }
    }

    public class LeadsAsign
    {
        public string FullName { get; set; } 
        public string UserName { get; set; }
    }


    public class Leadsnew
    {
        public string UpdUser { get; set; }
        public string CustCodeNo { get; set; }
        public string TransDate { get; set; } 
        public int CustRefTypeId { get; set; }
        public string Topic { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactPosition { get; set; }
        public string LeadDescription { get; set; }
        public int CmpId { get; set; }
        public int Seq { get; set; }

    }


}