using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{

    public class TAPCreditNoteH
    {
        public string UpdUser { get; set; }
        public string CnhDocNo { get; set; }
         public string CnhDocDate { get; set; }
        public string CnhBy { get; set; }
        public string CmpId { get; set; }
        public string SupplierCode { get; set; }
        public string Remark { get; set; }
        public string StateApprove { get; set; }
        public string VatType { get; set; }
        public double Vat { get; set; }
        public double Amt { get; set; }
        public string AmtTHB { get; set; }
        public string AmtEN { get; set; }
        public int CNType { get; set; }
        public string CnhCurCode {get;set;}

    }

    public class TAPCreditNoteD
    {
        public string UpdUser { get; set; }
        public string CnhDocNo { get; set; }
        public int Seq { get; set; }
        public string PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string UnitCode { get; set; }
        public double UnitPrice {get;set;}
        public double Amount {get;set;}
        public double VatAmt { get; set; }
        public double GrandAmt { get; set; }
        public string DocRefNo { get; set; }
        public string CmpId { get; set; }
    }

    public class TAPPayablesH
    {
        public string UpdUser { get; set; }
        public string PayableNo { get; set; }
        public string PayableDate { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierDesc { get; set; }
        public string CurCode { get; set; }
        public int Credit { get; set; }
        public string BuyType { get; set; }
        public string VatType { get; set; }
        public double VatAmt { get; set; }
        public string Remark { get; set; }
        public string CmpId { get; set; }

    }


    public class TAPPayablesD
    {
        public string UpdUser { get; set; }
        public string PayableNo { get; set; }
        public int Seq { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public double InvoiceAmt { get; set; }
        public string DueDate { get; set; }
        public double Discount { get; set; }
        public double Cost { get; set; }
        public double VatAmt { get; set; }
        public double TotalAmt { get; set; }
        public string AccCode { get; set; }
        public string Remark { get; set; }
        public string RcvDate { get; set; }
        public string DocrefNo { get; set; }
        public string CmpId { get; set; }
    }



    public class APBiling_H {
        public string UpdUser {get;set;}
        public string BillNo {get;set;}
        public string BillDate {get;set;}
        public string BillBy {get;set;}
        public string CmpId {get;set;}
        public string SupplierCode {get;set;}
        public string CurCode {get;set;}
        public int CreditDate {get;set;}
        public string PaymentDate {get;set;}
        public string Remark {get;set;}
        public double TotalAmt {get;set;  }
        public string TotalAmtTH {get;set;}
        public string TotalAmtEN {get;set;}
        public int StateApprove {get;set;}
        public string DateApprove {get;set;}
        public string UserApprove {get;set;}
        public string TimeApprove {get;set;}
    }


    public class APBilling_D {
        public string UpdUser {get;set;}
        public string BillNo {get;set;}
        public int Seq {get;set;}
        public string DocRefNo {get;set;}
        public string InvoiceNo {get;set;}
        public string InvoiceDate {get;set; }
        public string DueDate {get;set;}
        public double InvoiceAmt {get;set;}
        public double BalAmt {get;set;}
        public double PaidAmt { get;set;}
        public int DocRefType {get;set;}
        public int SeqDocRef {get;set;}
        public double PaymentAmt {get;set;}
        public string CmpId {get;set;}
    }

}