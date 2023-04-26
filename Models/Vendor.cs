using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Vendor
    {
        public string UpdUser { get; set; } 
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierTaxNo { get; set; }
        public string SupplierBranch { get; set; }
        public string SupplierBranchCode { get; set; }
        public string SupplierBranchName { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactName1 { get; set; }
        public string ContactEmail1 { get; set; }
        public string ContactPhone1 { get; set; }
        public int CreditDay { get; set; }
        public string PhoneOffice { get; set; }
        public string FaxOffice { get; set; }
        public string Website { get; set; }
        public string AddressShip { get; set; }
        public string Remark { get; set; }
        public string BankCode { get; set; }
        public string BankAccNo { get; set; }
        public string BankBranchNo { get; set; }
        public string BankType { get; set; }

    }
}