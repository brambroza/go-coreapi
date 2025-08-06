using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{

    public class CostAdvance
    {
        public string UpdUser { get; set; }
        public string AdvanceNo { get; set; } // NOT NULL
        public Int64 UserId { get; set; } // NOT NULL 
        public decimal AmountRequested { get; set; } // NOT NULL
        public decimal? AmountApproved { get; set; }
        public string Status { get; set; }
        public string ProjectNo { get; set; }
        public string CostCenterNo { get; set; }
        public string Purpose { get; set; }
        public string? PaymentDate { get; set; }
        public string CmpId { get; set; } // NOT NULL
        public string? UserTo { get; set; } // NOT NULL
    }


    public class CostCenter
    {
        public string CostCenterNo { get; set; } // NOT NULL
        public string CostCenterName { get; set; } // NOT NULL
        public string DepartmentNo { get; set; }
        public decimal? BudgetAmount { get; set; }
        public bool? IsActive { get; set; }
        public string CmpId { get; set; } // NOT NULL
        public string UpdUser { get; set; }
    }


    public class CostExpense
    {
        public string UpdUser { get; set; }
        public string ExpenseNo { get; set; } // NOT NULL
        public long UserId { get; set; } // NOT NULL
        public string AdvanceNo { get; set; }
        public string? ExpenseDate { get; set; }
        public decimal AmountSpent { get; set; } // NOT NULL
        public string Description { get; set; }
        public string Status { get; set; }
        public string ProjectNo { get; set; }
        public string CostCenterNo { get; set; } // NOT NULL
        public List<CostExpense_File>? Attachments { get; set; }
        public string CmpId { get; set; } // NOT NULL
        public string? UserTo { get; set; }
    }

    public class CostExpense_File
    {
        public string UpdUser { get; set; }
        public string ExpenseNo { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } 
        public string CmpId { get; set; }

    }




}