using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreapi.Models
{
    public class Customer
    {
        public string UpdUser { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTaxNo { get; set; }
        public string CustomerBranch { get; set; }
        public string CustomerBranchCode { get; set; }
        public string CustomerBranchName { get; set; }
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
        public string CmpId { get; set; }
        public string ContactName2 { get; set; }
        public string ContactEmail2 { get; set; }
        public string ContactPhone2 { get; set; }
        public string ContactPosition2 { get; set; }
        public string ContactPosition1 { get; set; }
        public string ContactPosition { get; set; }
        public string AddrSubDistrict { get; set; }
        public string AddrDistrict { get; set; }
        public string AddrProvince { get; set; }
        public string AddrPostCode { get; set; }
        public string ImgPath { get; set; }
        public int CreditAccId { get; set; }
        public int DebitAccId { get; set; }
        public string BusinessGrpCode { get; set; }
        public string StateCustomer { get; set; }
        public string StateVendor { get; set; }
    }

    public class CustomerList
    {
        public string UpdUser { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerTaxNo { get; set; }
        public string CustomerBranch { get; set; }
        public string CustomerBranchCode { get; set; }
        public string CustomerBranchName { get; set; }
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
        public string CmpId { get; set; }
        public string ContactName2 { get; set; }
        public string ContactEmail2 { get; set; }
        public string ContactPhone2 { get; set; }
        public string ContactPosition2 { get; set; }
        public string ContactPosition1 { get; set; }
        public string ContactPosition { get; set; }
        public string AddrSubDistrict { get; set; }
        public string AddrDistrict { get; set; }
        public string AddrProvince { get; set; }
        public string AddrPostCode { get; set; }
        public string ImgPath { get; set; }
        public int CreditAccId { get; set; }
        public int DebitAccId { get; set; }
        public string BusinessGrpCode { get; set; }

        public List<Contact> contacts { get; set; }

        public string StateCustomer { get; set; }
        public string StateVendor { get; set; }
    }

    public class Contact
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
    }

    public class CustCodeFill
    {
        public string CustomerCode { get; set; }
    }

    public class datefill
    {
        string sdate { get; set; }
    }

    public class CustomerDBD
    {
        public string UserLogin { get; set; }
        public string CmpId { get; set; }
        public string juristicID { get; set; }
        public string juristicNameTH { get; set; }
        public string juristicNameEN { get; set; }
        public string juristicType { get; set; }
        public string registerDate { get; set; }
        public string juristicStatus { get; set; }
        public string registerCapital { get; set; }
        public string standardObjective { get; set; }
        public standardObjectiveDetail standardObjectiveDetail { get; set; }
        public addressDetail addressDetail { get; set; }
    }

    public class standardObjectiveDetail
    {
        public string objectiveDescription { get; set; }
    }

    public class addressDetail
    {
        public string addressName { get; set; }
        public string buildingName { get; set; }
        public string roomNo { get; set; }
        public string floor { get; set; }
        public string villageName { get; set; }
        public string houseNumber { get; set; }
        public string moo { get; set; }
        public string soi { get; set; }
        public string street { get; set; }
        public string subDistrict { get; set; }
        public string district { get; set; }
        public string province { get; set; }
    }
}
