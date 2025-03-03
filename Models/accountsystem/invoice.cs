
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{

    public class Invoice
    {
        public string UpdUser { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceBy { get; set; }
        public string InvoiceState { get; set; }
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal InvoiceAmt { get; set; }
        public decimal InvoiceDisPer { get; set; }
        public decimal InvoiceDisAmt { get; set; }
        public decimal InvoiceNetAmt { get; set; }
        public decimal InvoiceVatAmt { get; set; }
        public decimal InvoiceVatPer { get; set; }
        public decimal InvoiceGrandAmt { get; set; }
        public string InvoiceGrandAmtTHB { get; set; }
        public string InvoiceGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public int DocState { get; set; }
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int StateApprove { get; set; }
        public string CustomerContactName { get; set; }
        public string JobType { get; set; }
        public int StateSendApprove { get; set; }
        public string QuotationNo { get; set; }
        public string CustomerPONo { get; set; }
        public string SaleOrderNo { get; set; }
        public string TicketId { get; set; }
        public int RevNo { get; set; }


        public List<Invoice_detail> items { get; set; }
    }

    public class Invoice_detail
    {
        public string UpdUser { get; set; }
        public string InvoiceNo { get; set; }
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
        public string CmpId { get; set; }
        public int RevNo { get; set; }
    }

    public class InvoiceCopy
    {
        public string InvoiceNo { get; set; }
        public string InvoiceNoNew { get; set; }
        public int RevNo { get; set; }
        public string CmpId { get; set; }

        public string CustomerCode { get; set; }
        public string TicketId { get; set; }
    }




    public class InvoiceForMobileModel
    {
        public string UpdUser { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceBy { get; set; }
        public string InvoiceState { get; set; }
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal InvoiceAmt { get; set; }
        public decimal InvoiceDisPer { get; set; }
        public decimal InvoiceDisAmt { get; set; }
        public decimal InvoiceNetAmt { get; set; }
        public decimal InvoiceVatAmt { get; set; }
        public decimal InvoiceVatPer { get; set; }
        public decimal InvoiceGrandAmt { get; set; }
        public string InvoiceGrandAmtTHB { get; set; }
        public string InvoiceGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public int DocState { get; set; }
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int StateApprove { get; set; }
        public string CustomerContactName { get; set; }
        public string JobType { get; set; }
        public int StateSendApprove { get; set; }
        public string QuotationNo { get; set; }
        public string CustomerPONo { get; set; }
        public string SaleOrderNo { get; set; }
        public string TicketId { get; set; }
        public int RevNo { get; set; }


        public List<Invoice_detail> items { get; set; }
        public cmpinfo company { get; set; }
        public Customer customer { get; set; }


    }



    public class InvoiceReceiveForMobileModel
    {
        public string UpdUser { get; set; }
        public string ReceiveNo { get; set; }
        public string ReceiveDate { get; set; }
        public string ReceiveBy { get; set; }
        public string ReceiveState { get; set; }
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal ReceiveAmt { get; set; }
        public decimal ReceiveDisPer { get; set; }
        public decimal ReceiveDisAmt { get; set; }
        public decimal ReceiveNetAmt { get; set; }
        public decimal ReceiveVatAmt { get; set; }
        public decimal ReceiveVatPer { get; set; }
        public decimal ReceiveGrandAmt { get; set; }
        public string ReceiveGrandAmtTHB { get; set; }
        public string ReceiveGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public int DocState { get; set; }
        public string PriceStand { get; set; }
        public string PaymentDue { get; set; }
        public string Shipping { get; set; }
        public int StateApprove { get; set; }
        public string CustomerContactName { get; set; }
        public string JobType { get; set; }
        public int StateSendApprove { get; set; }
        public string QuotationNo { get; set; }
        public string CustomerPONo { get; set; }
        public string SaleOrderNo { get; set; }
        public string TicketId { get; set; }
        public int RevNo { get; set; }
        public string InvoiceNo { get; set; }

        public List<InvoiceReceive_detail> items { get; set; }
        public cmpinfo company { get; set; }
        public Customer customer { get; set; }


    }



    public class InvoiceReceive_detail
    {
        public string UpdUser { get; set; }
        public string ReceiveNo { get; set; }
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
        public string CmpId { get; set; }
        public int RevNo { get; set; }
    }

}