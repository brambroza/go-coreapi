using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class Project
    {
        public string UpdUser { get; set; }
        public string ProjectNo { get; set; }
        public string Description { get; set; }
        public string CmpId { get; set; }
        public string PurchaseNo { get; set; }
        public string QuotationNo { get; set; }
        public string ReferCode { get; set; }
        public string StateActive { get; set; }
        public string ProjectDueDate { get; set; }
        public string ProjectDate { get; set; }
        public string SaleOrderNo { get; set; }
        public int CreditDate { get; set; }
        public int ShipOfDay { get; set; }
        public int? TotalQty { get; set; } = 0;
        public CustomerList? customer { get; set; } = null;

        public List<Project_Detail> items { get; set; }
        public List<Project_Task> tasks { get; set; }
        public List<Project_TaskInstall> installs { get; set; }
        public List<Project_File> attachfile { get; set; }
        public List<ProjectCost> costs { get; set; }

        public ProjectHistory? history { get; set; } = null;

        public string Title { get; set; }
        public string? Priority { get; set; } = null;
        public string? RouteId { get; set; } = null;
        public string? RouteName { get; set; } = null;
        public string? Labels { get; set; } = null;
        public string CustomerCode { get; set; }
        public int CreditType { get; set; }
        
        public string ProjectName { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public decimal SaleOrderAmt { get; set; }
        public string Note { get; set; }
        public string CustomerName { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int RevNo { get; set; }
        public int StateApprove { get; set; }
        public string CustomerContactName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTaxNo { get; set; }
        public string CustomerBranchCode { get; set; }
        public string JobType { get; set; }
        public string CustomerPONo { get; set; }
        public int StateCreateInvoice { get; set; }
        public int StateCreateProject { get; set; }
        public string TicketId { get; set; }
        public string StateShipAddr { get; set; }
        public string Shiptoother { get; set; }
       
        public string CustomerPODate { get; set; }

        public string ShippingMethod { get; set; }
        public string ServiceTerms { get; set; }
        public string ServiceOfTerms { get; set; }
        public string DeliveryTerms { get; set; }
        public string ProjectState { get; set; }

        public List<string>? ServiceOfTermsSelect { get; set; }
        public int MaintenanceServiceNumberOfTime { get; set; }
        public int MaintenanceRemoteNumberOfTime { get; set; }
        public bool MaintenanceServiceReport { get; set; }

        public int PreventiveServiceNumberOfTime { get; set; }
        public int PreventiveRemoteNumberOfTime { get; set; }
        public bool PreventiveServiceReport { get; set; }

        public string ServiceSLA { get; set; }
        public string ServiceReplacement { get; set; }
        public string ServiceBackupConfig { get; set; }
        public string DescriptionShipping { get; set; }
      
        public string ServiceTermsReport { get; set; }
      


    }

    public enum MessageType
    {
        Image,
        Text,
    }

    public class Apppo
    {
        public string UpdUser { get; set; }
        public string ProjectNo { get; set; }
        public string CustCode { get; set; }
        public string Description { get; set; }
        public string CmpId { get; set; }
        public string PurchaseNo { get; set; }
        public string QuotationNo { get; set; }
        public string ReferCode { get; set; }
        public string StateActive { get; set; }
        public string SaleOrderNo { get; set; }
        public string TicketId { get; set; }
    }

    public class AppInvoice
    {
        public string UpdUser { get; set; }
        public string SaleOrderNo { get; set; }
        public string CmpId { get; set; }
        public string State { get; set; }
        public string InvoiceNo { get; set; }
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

        public string imgpath { get; set; }

        public string type { get; set; }
        public string CmpId { get; set; }
        public string QuotationNo { get; set; }
        public string SaleOrderNo { get; set; }
        public string? Status { get; set; } = null;
         
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
        public string StartDate { get; set; } = null;
        public string StartTime { get; set; } = null;
        public string EndDate { get; set; } = null;
        public string EndTime { get; set; } = null;
        public string InstallDescription { get; set; }
        public string CmpId { get; set; }
    }

    public class Project_TaskInstall
    {
        public string UpdUser { get; set; }
        public string ProjectNo { get; set; }
        public int Seq { get; set; }

        public List<String> InstallResource { get; set; }
        public decimal InstallQty { get; set; }
        public string InstallStartDate { get; set; }
        public string InstallStartTime { get; set; }
        public string InstallEndDate { get; set; }
        public string InstallEndTime { get; set; }
        public string InstallDescription { get; set; }

        public string CmpId { get; set; }
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

    public class ProjectCost
    {
        public string UpdUser { get; set; }
        public string ProjectNo { get; set; }
        public int Seq { get; set; }
        public string CostDescription { get; set; }
        public decimal CostAmt { get; set; }
        public string AttachFile { get; set; }
        public string CmpId { get; set; }
    }

    public class ProjectHistory
    {
        public string ProjectNo { get; set; }
        public string? ProjectTime { get; set; } = null;
        public string? PaymentTime { get; set; } = null;
        public string? DeliveryTime { get; set; } = null;
        public string? CompletionTime { get; set; } = null;
        public string CmpId { get; set; }
        public List<ProjectTimeline> timeline { get; set; }   
    }

    public class ProjectTimeline
    {
        public string ProjectNo { get; set; }
        public string Title { get; set; }
        public DateTime Time { get; set; }
        public string CmpId { get; set; }
    }
}
