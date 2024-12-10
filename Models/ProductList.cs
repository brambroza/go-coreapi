using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class fileinfo
    {
        public string filename { get; set; }
        public string pathto { get; set; }
    }

    public class ProductList
    {
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductDescripton { get; set; }

        public string UnitCode { get; set; }

        public string ProductType { get; set; }
        public string ProductTypeSub { get; set; }

        public string BarcodeNo { get; set; }

        public decimal PriceSale { get; set; }

        public decimal PricePur { get; set; }

        public int VatType { get; set; }

        public string AccountCodeAR { get; set; }

        public string AccountCodeAP { get; set; }

        public string ProdCateCode { get; set; }

        public int ProductStateActive { get; set; }

        public string CmpId { get; set; }
        public string UpdUser { get; set; }

        public string BrandName { get; set; }

        public string Warranty { get; set; }

        public string ProductTypeName { get; set; }
        public string ShowReport { get; set; }
        public string imgpath { get; set; }

        public string ProductCodeRef { get; set; }
        public string AccountCode { get; set; }
    }

    public class ProductMasterList
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductDescripton { get; set; }
        public string UnitCode { get; set; }
        public string ProductType { get; set; }
        public string ProductTypeSub { get; set; }
        public string BarcodeNo { get; set; }
        public decimal PriceSale { get; set; }
        public decimal PricePur { get; set; }
        public int VatType { get; set; }
        public string AccountCodeAR { get; set; }
        public string AccountCodeAP { get; set; }
        public string ProdCateCode { get; set; }
        public int ProductStateActive { get; set; }
        public string Warranty { get; set; }
        public string BrandName { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductTypeSubName { get; set; }
        public string ShowReport { get; set; }
        public string ImgPath { get; set; }
        public DateTime UpdDate { get; set; }
        public string CmpId { get; set; }
        public bool StateActive { get; set; }
        public string UpdUser { get; set; }
        public string ProductCodeRef { get; set; }
        public string AccountCode { get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
        public string InventoryType { get; set; }
        public string ProductNameSearch { get; set; }
        public int Id { get; set; }
    }

    public class ListData
    {
        public int Id { get; set; }
        public string ListDescription { get; set; }

        public string ListName { get; set; }
    }
}
