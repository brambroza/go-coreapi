using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
 
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]

    public class VendorsController : ControllerBase
    {


        [HttpGet("[action]")]
        public IActionResult getVendor([FromQuery]  string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getVendors] @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);

        }


        [HttpGet("[action]")]
        public IActionResult getVendorById([FromQuery] string cmpid , [FromQuery] string Id)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getVendorsById] @CmpId='" + cmpid + "' , @Id='" + Id + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);

        }



        // POST: api/Vendors
        [HttpPost("[action]")]
        public void setVendor(Vendor vendor)
        {
            string _cmd = "";
            _cmd = "exec  dbo.mSupplier_Trans";
            _cmd += "  @UpdUser  ='" + vendor.UpdUser + "'";
            _cmd += ",@SupplierCode  ='" + vendor.SupplierCode + "'";
            _cmd += ",@SupplierName  ='" + vendor.SupplierName + "'";
            _cmd += ",@SupplierAddress  ='" + Tool.Tool.validateStr(vendor.SupplierAddress) + "'";
            _cmd += ",@SupplierTaxNo  ='" + vendor.SupplierTaxNo + "'";
            _cmd += ",@SupplierBranch  ='" + vendor.SupplierBranch + "'";
            _cmd += ",@SupplierBranchCode  ='" + vendor.SupplierBranchCode + "'";
            _cmd += ",@SupplierBranchName  ='" + vendor.SupplierBranchName + "'";
            _cmd += ",@ContactName  ='" + vendor.ContactName + "'";
            _cmd += ",@ContactEmail  ='" + vendor.ContactEmail + "'";
            _cmd += ",@ContactPhone  ='" + vendor.ContactPhone + "'";
            _cmd += ",@ContactName1  ='" + vendor.ContactName1 + "'";
            _cmd += ",@ContactEmail1  ='" + vendor.ContactEmail1 + "'";
            _cmd += ",@ContactPhone1  ='" + vendor.ContactPhone1 + "'";
            _cmd += ",@CreditDay =" + vendor.CreditDay;
            _cmd += ",@PhoneOffice  ='" + vendor.PhoneOffice + "'";
            _cmd += ",@FaxOffice  ='" + vendor.FaxOffice + "'";
            _cmd += ",@Website  ='" + vendor.Website + "'";
            _cmd += ",@AddressShip  ='" + Tool.Tool.validateStr(vendor.AddressShip) + "'";
            _cmd += ",@Remark  ='" + vendor.Remark + "'";
            _cmd += ",@BankCode  ='" + vendor.BankCode + "'";
            _cmd += ",@BankAccNo  ='" + vendor.BankAccNo + "'";
            _cmd += ",@BankBranchNo  ='" + vendor.BankBranchNo + "'";
            _cmd += ",@BankType  ='" + vendor.BankType + "'";
            _cmd += ",@CmpId='" + vendor.CmpId + "'";
            _cmd += ",@AddrSubDistrict  ='" + vendor.AddrSubDistrict + "'";
            _cmd += ",@AddrDistrict  ='" + vendor.AddrDistrict + "'";
            _cmd += ",@AddrProvince  ='" + vendor.AddrProvince + "'";
            _cmd += ",@AddrPostCode='" + vendor.AddrPostCode + "'";
             _cmd += ",@ImgPath='" + vendor.ImgPath + "'";

            DB.DBConn.ExecuteOnly(_cmd);
        }

        // DELETE: api/Vendors/5
        [HttpDelete("[action]/{id}")]
        public void Delete(string id)
        {

            string _cmd = "";
            _cmd = "delete from msb.mSupplier where  SupplierCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
