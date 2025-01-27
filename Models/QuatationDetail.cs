using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class QuotationDetail
    {
        public string QuotationNo { get; set; }
        public int Seq { get; set; }
        public string ProdCode { get; set; }
        public string ProdDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitCode { get; set; }
        public decimal Amt { get; set; }

        public decimal DisPer { get; set; }
        public decimal DisAmt { get; set; }
        public decimal NetAmt { get; set; }

        public decimal PricePur { get; set; }
        public decimal CostAmt { get; set; }
        public decimal ProfitAmt { get; set; }
        public decimal GrossProfitPer { get; set; }
        public int RevNo { get; set; }
        public string GroupCaption1 { get; set; }
        public string GroupCaption2 { get; set; }
        public string GroupCaption3 { get; set; }
        public string UpdUser { get; set; }
        public string CmpId { get; set; }

        public string MainProdCode { get; set; }
        public int MainSeq { get; set; }
        public int SeqSort { get; set; }
    }

    public class saleorderDetail
    {
        public string SaleOrderNo { get; set; }
        public int Seq { get; set; }
        public string ProdCode { get; set; }
        public string ProdDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitCode { get; set; }
        public decimal Amt { get; set; }

        public decimal DisPer { get; set; }
        public decimal DisAmt { get; set; }
        public decimal NetAmt { get; set; }

        public decimal PricePur { get; set; }
        public decimal CostAmt { get; set; }
        public decimal ProfitAmt { get; set; }
        public decimal GrossProfitPer { get; set; }
        public int RevNo { get; set; }
        public string GroupCaption1 { get; set; }
        public string GroupCaption2 { get; set; }
        public string GroupCaption3 { get; set; }
        public string UpdUser { get; set; }

        public string CmpId { get; set; }
    }
}
