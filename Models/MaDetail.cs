using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class MaDetail
    {
        public string UpdUser { get; set; } 
        public string MANo { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public int Seq { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public string WarningTime { get; set; }
        public string WarningBeforExpireDay { get; set; }
        public int NotificationQtySet { get; set; }
        public string NotificationPeriodDay { get; set; }
        public string NotificationQty { get; set; }
        public int ServiceGrp { get; set; }
        public string ProjectName { get; set; }
        public string QuotationNo { get; set; }
        public string PurchaseNo { get; set; }
        public string ReferNo { get; set; }
        public int ProductType { get; set; }
        public string SerialNo { get; set; }
        public string LicensNo { get; set; }
        public string SuplName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string BrandName { get; set; }

        public decimal PriceSale { get; set; }
        public decimal PricePur { get; set; }
         



    }
}