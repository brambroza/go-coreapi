using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Quotation
    {
        public string QuotationNo { get; set; }
        public string QuotationDate { get; set; }
        public string QuotationBy { get; set; }
        public string QuotationState { get; set; }
        public string CustomerCode { get; set; }
        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal QuotationAmt { get; set; }
        public decimal QuotationDisPer { get; set; }
        public decimal QuotationDisAmt { get; set; }
        public decimal QuotationNetAmt { get; set; }
        public decimal QuotationVatAmt { get; set; }
        public decimal QuotationGrandAmt { get; set; }
        public string QuotationGrandAmtTHB { get; set; }
        public string QuotationGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int RevNo { get; set; }
        public string CustomerContactName { get; set; }

        public string Jobtype { get; set; }

    }

    public class QuotationCopy
    {
        public string QuotationNo { get; set; }
        public string QuotationNoNew { get; set; }
        public int RevNo { get; set; }
        public string CmpId { get; set; }




    }


    public class SaleOrderCopy
    {
        public string SaleOrderNo { get; set; }
        public string SaleOrderNoNew { get; set; }
        public int RevNo { get; set; }

        public string CmpId { get; set; }
        public string userlogin { get; set; }




    }




    public class QuoHApprove
    {
        public string cmpid { get; set; }
        public string docno { get; set; }
        public int revno { get; set; }
        public string user { get; set; }

    }

    public class QuoHApprovetoPo
    {
        public string cmpid { get; set; }
        public string docno { get; set; }
        public int revno { get; set; }
        public string state { get; set; }
        public string user { get; set; }

    }








    public class SalesBomApprove
    {
        public string UpdUser { get; set; }
        public string UpdDate { get; set; }
        public string UpdTime { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int StateApp { get; set; }
    }
    public class SalesBom
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int RevNo { get; set; }
        public string BomBy { get; set; }
        public string BomDate { get; set; }
        public string SaleName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerContactName { get; set; }
        public string CustomerContactPhone { get; set; }
        public string CustomerContactEmail { get; set; }
        public string ProjectName { get; set; }
        public int ProjectStatus { get; set; }
        public string Remark { get; set; }

        public string BomState { get; set; }
        public string CmpId { get; set; }

        public string TicketId { get; set; }

        public List<SalesBom_Detail> items { get; set; }
    }

    public class SalesBom_Detail
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int RevNo { get; set; }
        public int Seq { get; set; }
        public string ProdCode { get; set; }
        public string ProdDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitCode { get; set; }
        public decimal Amt { get; set; }
        public string CmpId { get; set; }
        public int ReplaceStatus { get; set; }
        public string Remark { get; set; }

        public string Vendor {get;set;} ="";
        public string VendorName {get;set;} = "";

        public List<SalesBom_Price_Item> bomitemPrice { get; set; }

    }


    public class SalesBom_Price_Version
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int RevNo { get; set; }
        public int Seq { get; set; }
        public string ProdCode { get; set; }
        public string SupplierCode { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitCode { get; set; }
        public decimal Amt { get; set; }
        public string CmpId { get; set; }
        public string Remark { get; set; }
        public DateTime DeliveryDate { get; set; }
    }

    public class SalesBom_Price_Item
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int RevNo { get; set; }
        public int Seq { get; set; }
        public int PriceSeq { get; set; }
        public string CmpId { get; set; }
        public string ProdCode { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public decimal Qty { get; set; }
        public decimal? QtyBal { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? BalCheckDate { get; set; }
        public string Remark { get; set; }
        public string UnitCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amt { get; set; }
    }




    public class SalesBom_Action
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int Seq { get; set; }
        public string DescActions { get; set; }
        public string DateActions { get; set; }
    }

    public class SalesBom_File
    {
        public string UpdUser { get; set; }
        public string BomNo { get; set; }
        public int Rev { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FlieSize { get; set; }
        public string Remark { get; set; }
    }


    public class saleorder
    {
        public string SaleOrderNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string SaleOrderBy { get; set; }
        public string SaleOrderState { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName  {get;set;}
        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal SaleOrderAmt { get; set; }
        public decimal SaleOrderDisPer { get; set; }
        public decimal SaleOrderDisAmt { get; set; }
        public decimal SaleOrderNetAmt { get; set; }
        public decimal SaleOrderVatAmt { get; set; }
        public decimal SaleOrderGrandAmt { get; set; }
        public string SaleOrderGrandAmtTHB { get; set; }
        public string SaleOrderGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public string PriceStand { get; set; } ="";
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int RevNo { get; set; }
        public string CustomerContactName { get; set; }
        public string JobType { get; set; }
        public string QuotationNo { get; set; }
        public string CustomerPONo { get; set; }

        public List<SaleOrderItem> items { get; set; }

        public string TicketId {get;set;}


    }


    public class SaleOrderItem
    {
        public string SaleOrderNo { get; set; }
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
        public decimal GrossProfitPer { get; set; }
        public string GroupCaption1 { get; set; }
        public string GroupCaption2 { get; set; }
        public string GroupCaption3 { get; set; }
        public string CmpId { get; set; }

        public int RevNo {get;  set;}
    }








    public class QuotationListItem
    {
        public string QuotationNo { get; set; }
        public int Seq { get; set; }
        public string ProdCode { get; set; } // Nullable
        public string ProdDescription { get; set; } // Nullable
        public decimal Qty { get; set; }
        public string UnitCode { get; set; } // Nullable
        public decimal UnitPrice { get; set; }
        public decimal Amt { get; set; }
        public decimal? DisPer { get; set; } // Nullable
        public decimal? DisAmt { get; set; } // Nullable
        public decimal? NetAmt { get; set; } // Nullable
        public decimal? PricePur { get; set; } // Nullable
        public decimal? CostAmt { get; set; } // Nullable
        public decimal? ProfitAmt { get; set; } // Nullable
        public int RevNo { get; set; }
        public string GroupCaption1 { get; set; } // Nullable
        public string GroupCaption2 { get; set; } // Nullable
        public string GroupCaption3 { get; set; } // Nullable
        public string CmpId { get; set; }

        public decimal GrossProfitPer { get; set; }
          public string MainProdCode {get;set;}
        public int MainSeq {get;set;}
        
    }

    public class QuotationList
    {
        public string QuotationNo { get; set; }
        public string QuotationDate { get; set; } // ISO Date
        public string QuotationBy { get; set; }
        public string QuotationState { get; set; } // Nullable
        public string CustomerCode { get; set; } // Nullable
        public string CustomerName { get; set; }
        public int CreditType { get; set; }
        public int? CreditDate { get; set; } // Nullable
        public string ProjectName { get; set; } // Nullable
        public string ReferCode { get; set; } // Nullable
        public int VatType { get; set; }
        public string Remark { get; set; } // Nullable
        public string Note { get; set; } // Nullable
        public decimal QuotationAmt { get; set; }
        public decimal QuotationDisPer { get; set; }
        public decimal QuotationDisAmt { get; set; }
        public decimal QuotationNetAmt { get; set; }
        public decimal QuotationVatAmt { get; set; }
        public decimal QuotationGrandAmt { get; set; }
        public string QuotationGrandAmtTHB { get; set; }
        public string QuotationGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public string? DocState { get; set; } // Nullable
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int RevNo { get; set; }
        public int RevNoMax { get; set; }
        public int? StateApprove { get; set; } // Nullable
        public string DateApprove { get; set; } // ISO Date Nullable
        public string ApproveBy { get; set; } // Nullable
        public string CustomerContactName { get; set; }
        public int? StateApproveToPO { get; set; } // Nullable
        public string DateApproveToPO { get; set; } // ISO Date Nullable
        public string ApproveToPOBy { get; set; } // Nullable
        public string JobType { get; set; }
        public int? StateSendApprove { get; set; } // Nullable
        public string DateSendApprove { get; set; } // ISO Date Nullable
        public string SendApproveBy { get; set; } // Nullable

        public string SignaturePath { get; set; }
        public string FullName { get; set; }

        public string JobTypeFilter { get; set; }
        public string ImgPath { get; set; }

        public string TicketId { get; set; }

        public List<QuotationListItem> Items { get; set; }
    }




}