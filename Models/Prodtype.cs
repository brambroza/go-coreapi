using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models
{
    public class Prodtype
    {
        public string UpdUser { get; set; }
        public string ProdCateCode { get; set; }
        public string ProdCateName { get; set; }
        public string CmpId { get; set; }

    }

    public class ProdTypeSub
    {
        public string UpdUser { get; set; }
        public string ProdCateSubCode { get; set; }
        public string ProdCateSubName { get; set; }
        public string ProdCateCode { get; set; }
        public string CmpId { get; set; }

    }
}