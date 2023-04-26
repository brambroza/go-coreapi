using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{
    
    public class VendorsController : ApiController
    {
        // GET: api/Vendors
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Vendors/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getVendors] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);

        }


        // POST: api/Vendors
        public void Post(Vendor vendor)
        {
            string _cmd = "";
            _cmd = "exec  dbo.mSupplier_Trans";
            _cmd += "  @UpdUser  ='" + vendor.UpdUser + "'";
            _cmd += ",@SupplierCode  ='" + vendor.SupplierCode + "'";
            _cmd += ",@SupplierName  ='" + vendor.SupplierName + "'";
            _cmd += ",@SupplierAddress  ='" + Tool.Tool.validateStr(vendor.SupplierAddress )+ "'";
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
            _cmd += ",@AddressShip  ='" + Tool.Tool.validateStr(vendor.AddressShip )+ "'";
            _cmd += ",@Remark  ='" + vendor.Remark + "'";
            _cmd += ",@BankCode  ='" + vendor.BankCode + "'";
            _cmd += ",@BankAccNo  ='" + vendor.BankAccNo + "'";
            _cmd += ",@BankBranchNo  ='" + vendor.BankBranchNo + "'";
            _cmd += ",@BankType  ='" + vendor.BankType + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        // PUT: api/Vendors/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Vendors/5
        public void Delete(string id)
        {

            string _cmd = "";
            _cmd = "delete from msb.mSupplier where  SupplierCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
