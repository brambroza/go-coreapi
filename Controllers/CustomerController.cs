using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using goalongapi.Models;
using goalongapi.Data;
using goalongapi.Datatools.Product;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        [HttpGet]
        [Route("Customer")]
        public IActionResult Get([FromQuery] string cmpid, [FromQuery] string type)
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtContact = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getCustomer @CmpId='" + cmpid + "' , @Type='" + type + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
 
            _cmd = "exec dbo.getContact @CmpId='" + cmpid + "'";
            dtContact = DB.DBConn.GetDataTable(_cmd);

            string _DocType = "";
            if (type == "0")
            {
                _DocType = "customer";
            }
            else
            {
                _DocType = "vendor";
            }

            List<CustomerList> customerLists = new List<CustomerList>();

            foreach (DataRow r in dt.Rows)
            {
                var customer = new CustomerList();

                customer.UpdUser = r["UpdUser"].ToString();
                customer.CustomerCode = r["CustomerCode"].ToString();
                customer.CustomerName = r["CustomerName"].ToString();
                customer.CustomerAddress = r["CustomerAddress"].ToString();
                customer.CustomerTaxNo = r["CustomerTaxNo"].ToString();
                customer.CustomerBranch = r["CustomerBranch"].ToString();
                customer.CustomerBranchCode = r["CustomerBranchCode"].ToString();
                customer.CustomerBranchName = r["CustomerBranchName"].ToString();
                customer.ContactName = r["ContactName"].ToString();
                customer.ContactEmail = r["ContactEmail"].ToString();
                customer.ContactPhone = r["ContactPhone"].ToString();
                customer.ContactName1 = r["ContactName1"].ToString();
                customer.ContactEmail1 = r["ContactEmail1"].ToString();
                customer.ContactPhone1 = r["ContactPhone1"].ToString();
                customer.CreditDay = Convert.ToInt32(r["CreditDay"]);
                customer.PhoneOffice = r["PhoneOffice"].ToString();
                customer.FaxOffice = r["FaxOffice"].ToString();
                customer.Website = r["Website"].ToString();
                customer.AddressShip = r["AddressShip"].ToString();
                customer.Remark = r["Remark"].ToString();
                customer.CmpId = r["CmpId"].ToString();
                customer.ContactName2 = r["ContactName2"].ToString();
                customer.ContactEmail2 = r["ContactEmail2"].ToString();
                customer.ContactPhone2 = r["ContactPhone2"].ToString();
                customer.ContactPosition2 = r["ContactPosition2"].ToString();
                customer.ContactPosition1 = r["ContactPosition1"].ToString();
                customer.ContactPosition = r["ContactPosition"].ToString();
                customer.AddrSubDistrict = r["AddrSubDistrict"].ToString();
                customer.AddrDistrict = r["AddrDistrict"].ToString();
                customer.AddrProvince = r["AddrProvince"].ToString();
                customer.AddrPostCode = r["AddrPostCode"].ToString();
                customer.ImgPath = r["ImgPath"].ToString();
                customer.CreditAccId = Convert.ToInt32(r["CreditAccId"]);
                customer.DebitAccId = Convert.ToInt32(r["DebitAccId"]);
                customer.BusinessGrpCode = r["BusinessGrpCode"].ToString();
                customer.StateCustomer = r["StateCustomer"].ToString();
                customer.StateVendor = r["StateVendor"].ToString();
                customer.SourceCode = r["SourceCode"].ToString();
                customer.contacts = new List<ContactList>();

                foreach (
                    DataRow c in dtContact.Select(
                        "DocType='" + _DocType + "' and DocNo='" + customer.CustomerCode + "'"
                    )
                )
                {
                    var item = new ContactList();
                    item.UpdUser = c["UpdUser"].ToString();
                    item.ContactName = c["ContactName"].ToString();
                    item.ContactPhone = c["ContactPhone"].ToString();
                    item.ContactEmail = c["ContactEmail"].ToString();
                    item.ContactPosition = c["ContactPosition"].ToString();
                    item.ContactLineId = c["ContactLineId"].ToString();
                    item.Remark = c["Remark"].ToString();
                    item.CmpId = c["CmpId"].ToString();
                    item.ContactId = c["ContactId"].ToString();
                    item.ImgPath = c["ImgPath"].ToString();
                    item.DocNo = c["DocNo"].ToString();
                    item.DocType = c["DocType"].ToString();

                    customer.contacts.Add(item);
                }

                customerLists.Add(customer);
            }

            return Ok(customerLists);
        }

        [HttpGet]
        [Route("CustomerById")]
        public IActionResult getCustomerById(
            [FromQuery] string cmpid,
            [FromQuery] string customerCode
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.getCustomer_ById @CmpId='"
                + cmpid
                + "' , @CustomerCode='"
                + customerCode
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet]
        [Route("CustomerContact")]
        public IActionResult getContact([FromQuery] string CmpId, [FromQuery] string CustCode)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getCustContact @CmpId='" + CmpId + "' , @CustCode='" + CustCode + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost]
        [Route("Customer")]
        public IActionResult Post([FromBody] Customer customer)
        {
            //if (Request.Headers.Contains("authToken")){
            //    if (Request.Headers.GetValues("authToken").First() != "XXX")
            //        return   Ok(HttpStatusCode.Unauthorized);
            //}



            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.CustomerListTrans";
                _cmd += " @UpdUser  ='" + customer.UpdUser + "'";
                _cmd += ",@CustomerCode  ='" + customer.CustomerCode + "'";
                _cmd += ",@CustomerName  ='" + customer.CustomerName + "'";
                _cmd += ",@CustomerAddress  ='" + customer.CustomerAddress + "'";
                _cmd += ",@CustomerTaxNo  ='" + customer.CustomerTaxNo + "'";
                _cmd += ",@CustomerBranch  ='" + customer.CustomerBranch + "'";
                _cmd += ",@CustomerBranchCode  ='" + customer.CustomerBranchCode + "'";
                _cmd += ",@CustomerBranchName  ='" + customer.CustomerBranchName + "'";
                _cmd += ",@ContactName  ='" + customer.ContactName + "'";
                _cmd += ",@ContactEmail  ='" + customer.ContactEmail + "'";
                _cmd += ",@ContactPhone  ='" + customer.ContactPhone + "'";
                _cmd += ",@ContactName1  ='" + customer.ContactName1 + "'";
                _cmd += ",@ContactEmail1  ='" + customer.ContactEmail1 + "'";
                _cmd += ",@ContactPhone1  ='" + customer.ContactPhone1 + "'";
                _cmd += ",@ContactName2  ='" + customer.ContactName2 + "'";
                _cmd += ",@ContactEmail2  ='" + customer.ContactEmail2 + "'";
                _cmd += ",@ContactPhone2  ='" + customer.ContactPhone2 + "'";
                _cmd += ",@ContactPosition1  ='" + customer.ContactPosition1 + "'";
                _cmd += ",@ContactPosition2  ='" + customer.ContactPosition2 + "'";
                _cmd += ",@ContactPosition  ='" + customer.ContactPosition + "'";

                _cmd += ",@CreditDay =" + customer.CreditDay;
                _cmd += ",@PhoneOffice  ='" + customer.PhoneOffice + "'";
                _cmd += ",@FaxOffice  ='" + customer.FaxOffice + "'";
                _cmd += ",@Website  ='" + customer.Website + "'";
                _cmd += ",@AddressShip  ='" + customer.AddressShip + "'";
                _cmd += ",@Remark  ='" + customer.Remark + "'";
                _cmd += ",@CmpId ='" + customer.CmpId + "'";

                _cmd += ",@AddrSubDistrict  ='" + customer.AddrSubDistrict + "'";
                _cmd += ",@AddrDistrict  ='" + customer.AddrDistrict + "'";
                _cmd += ",@AddrProvince  ='" + customer.AddrProvince + "'";
                _cmd += ",@AddrPostCode  ='" + customer.AddrPostCode + "'";
                _cmd += ",@ImgPath  ='" + customer.ImgPath + "'";
                _cmd += ",@CreditAccId =" + customer.CreditAccId;
                _cmd += ",@DebitAccId =" + customer.DebitAccId;
                _cmd += " , @BusinessGrpCode='" + customer.BusinessGrpCode + "'";
                _cmd += " , @StateCustomer=" + customer.StateCustomer + "";
                _cmd += " , @StateVendor=" + customer.StateVendor + "";
                _cmd += ", @SourceCode='" + customer.SourceCode + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpDelete]
        [Route("Customer")]
        public void Delete([FromQuery] string customercode, [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd =
                "delete from msb.mCustomer where  CustomerCode='"
                + customercode
                + "' and cmpid='"
                + cmpid
                + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
