using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace goalongapi.Models
{
    public class Company
    {
        public string CmpId { get; set; }
        public string CmpName { get; set; }
        public string CmpAddress { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string teloffice { get; set; }
    }


    public class Department
    {
        public string CmpId { get; set; }
        public string DepartmentNo { get; set; }
        public string DepartmentName { get; set; }
        public string StateActive { get; set; }
        public string UpdUser { get; set; }
    
    }
  public class Position
    {
        public string CmpId { get; set; }
        public string PositionNo { get; set; }
        public string PositionName { get; set; }
        public string? StateActive { get; set; }
        public string? UpdUser { get; set; }
    
    }



 

    public class UserAccouter
    {
        public Int64 AccountID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string CmpId { get; set; }
        public string imgPath { get; set; }
        public string SignaturePath { get; set; }
        public string LineQRCodePath { get; set; }
        public string MobileNo { get; set; }
        public string LineId { get; set; }
        public string Address { get; set; }
        public string AddrProvince { get; set; }
        public string AddrDistrict { get; set; }
        public string AddrSubDistrict { get; set; }
        public string AddrPostCode { get; set; }
        public int RoleID { get; set; }
    }

    public class MapUser
    {
        public string email { get; set; }
        public string cmpid { get; set; }
    }

    public class datacmpimg
    {
        public string imgpath { get; set; }
        public string cmpid { get; set; }
    }

    public class cmpSocialChannel
    {
        public string CmpId { get; set; }
        public string UpdUser { get; set; }
        public int Seq { get; set; }
        public string Platform { get; set; }
        public string ApiKey { get; set; }
        public string WebhookUrl { get; set; }
        public string AccessToken { get; set; }
        public string PageId { get; set; }
        public string PhoneNumber { get; set; }
        public string ChannelId { get; set; }
        public string LineId { get; set; }
        public string? Name { get; set; }
    }

    public class cmpSocialChannel_LiffApp
    {
        public string CmpId { get; set; }
        public string UpdUser { get; set; }
        public int? Seq { get; set; }
        public string AppName { get; set; }
        public string LiffId { get; set; }
        public string ChannelId { get; set; }
        public string? LineOAName { get; set; }
        public string? LineId { get; set; }
        public string? AccessToken { get; set; }
    }

      public class cmpSocialChannel_LiffAppUrl
    {
        public string CmpId { get; set; }
        public string UpdUser { get; set; }
        public int? Seq { get; set; }
        public string AppName { get; set; }        
         public string Url { get; set; } 
        public string? Description { get; set; }
    }


    public class cmpinfo
    {
        public string CmpId { get; set; }
        public string CmpName { get; set; }
        public string CmpAddress { get; set; }
        public string CmpTaxid { get; set; }
        public int CmpType { get; set; }
        public int StateActive { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string DateCreate { get; set; }
        public string DateExprie { get; set; }
        public string TelOffice { get; set; }
        public string CmpImg { get; set; }
        public string AddressShip { get; set; }
        public string AddrProvince { get; set; }
        public string AddrDistrict { get; set; }
        public string AddrSubDistrict { get; set; }
        public string AddrPostCode { get; set; }
        public string CmpBranchCode { get; set; }
        public string CmpBranchName { get; set; }
        public string WebSite { get; set; }
        public string Remark { get; set; }
        public string UpdUser { get; set; }
        public string DocPrefix { get; set; }
        public string BankAccCode { get; set; }
        public string BankAccName { get; set; }
        public string BankAccType { get; set; }
        public string BankCode { get; set; }
        public string BankBranchCode { get; set; }
        public string LineId { get; set; }
        public string ColorThemeReport { get; set; }
        public string FaviconUrl { get; set; }
        public string CmpNameEN { get; set; }
        public string CmpAddressEN { get; set; }
    }

    public class Bank
    {
        public string UserName { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string Remark { get; set; }
        public int StateActive { get; set; }
        public string CmpId { get; set; }
    }

    public class BankBranch
    {
        public string UserName { get; set; }
        public string BankCode { get; set; }
        public string BankBranchCode { get; set; }
        public string BankBranchName { get; set; }
        public string Address { get; set; }
        public string AddrProvince { get; set; }
        public string AddrDistrict { get; set; }
        public string AddrSubDistrict { get; set; }
        public string AddrPostCode { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string Remark { get; set; }
        public int StateActive { get; set; }

        public string CmpId { get; set; }
    }

    public class mBussinetGrp
    {
        public string UpdUser { get; set; }
        public string BusinessGrpCode { get; set; }
        public string BusinessGrpName { get; set; }
        public string BusinessGrpDescription { get; set; }
        public int StateActive { get; set; }
        public string CmpId { get; set; }
    }

     public class mSource
    {
        public string UpdUser { get; set; }
        public string SourceCode { get; set; }
        public string SourceName { get; set; } 
        public int StateActive { get; set; }
        public string CmpId { get; set; }
    }


    public class paymentmethod
    {
        public string UpdUser { get; set; }
        public string CmpId { get; set; }
        public int PaymentMethodId { get; set; }
        public string BankAccCode { get; set; }
        public string BankAccName { get; set; }
        public string BankCode { get; set; }
        public string BankBranchCode { get; set; }
        public int BankType { get; set; }
        public string BankTypeName { get; set; }
    }
}
