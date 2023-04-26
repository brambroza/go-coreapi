using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Project
    {
            public string UpdUser { get; set; } 
            public string ProjectNo { get; set; } 
            public string CustCode { get; set; } 
            public string Description { get; set; } 
            public int CmpId { get; set; } 
            public string PurchaseNo { get; set; } 
            public string QuatationNo { get; set; } 
            public string ReferCode { get; set; }
            public string StateActive { get; set; } 
            public string ProjectDueDate { get; set; }

    }

    public class Apppo
    {
        public string UpdUser { get; set; }
        public string ProjectNo { get; set; }
        public string CustCode { get; set; }
        public string Description { get; set; }
        public int CmpId { get; set; }
        public string PurchaseNo { get; set; }
        public string QuatationNo { get; set; }
        public string ReferCode { get; set; }
        public string StateActive { get; set; }
        

    }




    public class Project_Detail 
    { 
        public string UpdUser { get; set; }  
        public string ProjectNo { get; set; } 
        public int Seq { get; set; } 
        public string ProdCode { get; set; } 
        public string ProdDescription { get; set; } 
        public decimal Qty { get; set; } 
        public string UnitCode { get; set; } 
        public decimal UnitPrice { get; set; } 
        public decimal Amt { get; set; } 
        public decimal DisPer { get; set; } 
        public decimal DisAmt { get; set; } 
        public decimal NetAmt { get; set; } 
        public decimal PricePur { get; set; } 
        public decimal CostAmt { get; set; } 
        public decimal ProfitAmt { get; set; } 
        public string GroupCaption1 { get; set; } 
        public string GroupCaption2 { get; set; } 
        public string GroupCaption3 { get; set; } 

        public string PurchaseNo { get; set; }
        public string DeliveryDate { get; set; }
        public int RevNo { get; set; }
    }

    public class Project_Task 
    { 
        public string UpdUser { get; set; }  
        public string ProjectNo { get; set; } 
        public int Seq { get; set; } 
        public string Description { get; set; } 
        public decimal Qty { get; set; } 
        public string UnitCode { get; set; } 
        public decimal UnitPrice { get; set; } 
        public decimal Amt { get; set; }  
        public List<String> Resource { get; set; }
        public decimal DayQty { get; set; } 
        public decimal Time { get; set; } 
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string InstallDescription { get; set; }

    }


   

    public class Project_File 
    { 
        public string UpdUser { get; set; }  
        public string ProjectNo { get; set; } 
        public int Seq { get; set; } 
        public string FileName { get; set; } 
        public string FilePath { get; set; }
        public byte[] Files { get; set; } 
    }

}