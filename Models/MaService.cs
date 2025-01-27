using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class MaService
    {
        public string UpdUser { get; set; }
        public string MANo { get; set; }
        public int ServiceType { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public int Seq { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public string WarningTime { get; set; }
        public int WarningBeforExpireDay { get; set; }
        public int NotificationQtySet { get; set; }
        public int NotificationPeriodDay { get; set; }
        public int NotificationQty { get; set; }
        public int ServiceGrp { get; set; }
        public string ProjectName { get; set; }
        public string ReferNo { get; set; }
        public string QuotationNo { get; set; }
        public string PurchaseNo { get; set; }
        public decimal PriceSale { get; set; }
        public decimal PricePur { get; set; }

    }
}