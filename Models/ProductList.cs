using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class ProductList
    {
        public string ProdductCode { get; set; }

        public string ProdductName { get; set; }

        public string ProdductDescripton { get; set; }

        public string UnitCode { get; set; }

        public int ProductType { get; set; }

        public string BarcodeNo { get; set; }

        public decimal PriceSale { get; set; }

        public decimal PricePur { get; set; }

        public int VatType { get; set; }

        public string AccountCodeAR { get; set; }

        public string AccountCodeAP { get; set; }

        public string ProdCateCode { get; set; }

        public int ProdductStateActive { get; set; }

        public int CmpId { get; set; }
        public string UpdUser { get; set; }

        public string BrandName { get; set; }

        public string Warranty { get; set; }

        public string ProductTypeName { get; set; }
        public string ShowReport { get; set; }


    }

    public class ListData
    {
        public int Id { get; set; }
        public string ListDescription { get; set; }

        public string ListName { get; set; }
    }
}