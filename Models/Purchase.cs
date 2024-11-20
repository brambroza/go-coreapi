using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Purchase
    {
        public string UpdUser { get; set; }
        public string PurchaseNo { get; set; }
        public string PurchaseDate { get; set; }
        public string PurchaseBy { get; set; }
        public string PurchaseState { get; set; }
        public string SupplierCode { get; set; }
        public int CreditType { get; set; }
        public int CreditDate { get; set; }
        public string ProjectName { get; set; }
        public string ReferCode { get; set; }
        public int VatType { get; set; }
        public string Remark { get; set; }
        public string Note { get; set; }
        public decimal PurchaseAmt { get; set; }
        public decimal PurchaseDisPer { get; set; }
        public decimal PurchaseDisAmt { get; set; }
        public decimal PurchaseNetAmt { get; set; }
        public decimal PurchaseVatAmt { get; set; }
        public decimal PurchaseVatPer  { get; set; }
        public decimal PurchaseGrandAmt { get; set; }
        public string PurchaseGrandAmtTHB { get; set; }
        public string PurchaseGrandAmtENB { get; set; }
        public int WithholdingTaxState { get; set; }
        public int ShowSignatureState { get; set; }
        public string CmpId { get; set; }
        public int DocState { get; set; }
        public string PriceStand { get; set; }
        public DateTime PaymentDue { get; set; }
        public DateTime Shipping { get; set; }
        public int RevNo { get; set; }
        public string ProjectNo { get; set; }

        public string SupplierName {get;set;}
        public string ContactName {get;set;}

        public List<Purchase_Detail> items {get;set;}


    }

    public class Purchase_Detail
    {
        public string UpdUser { get; set; }
        public string PurchaseNo { get; set; }
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
        public int RevNo { get; set; }
        public string GroupCaption1 { get; set; }
        public string GroupCaption2 { get; set; }
        public string GroupCaption3 { get; set; }

        public string CmpId { get; set; }
    }


}