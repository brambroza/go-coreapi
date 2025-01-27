using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace goalongapi.Models.Trial
{
    public class Trial
    {
    }
    public class  CmpData 
    { 
        public int CmpId { get; set; } 
        public string CmpNameTH { get; set; }
        public string CmpAddressTH { get; set; } 
        public string TaxIdTH { get; set; } 
        public string CmpNameEN { get; set; } 
        public string CmpAddressEN { get; set; } 
        public string TaxIdEN { get; set; }
        public string TelNo { get; set; }
        public string Mobile { get; set; } 
        public string FaxNo { get; set; } 
        public string Website { get; set; } 
        public int VatType { get; set; } 
        public int StateActive { get; set; } 
        public int BarnchId { get; set; } 
        public string BranchCode { get; set; } 
        public string BranchName { get; set; } 
        public string Userlogin { get; set; }
    }
}