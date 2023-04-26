
using coreapi.Models;
using System.Net;
using System;
using goalongapi.Data;
using goalongapi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Datatools.Product;
using Mapster;
using goalongapi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.IdentityModel.Tokens.Jwt; 
using Newtonsoft.Json;


namespace coreapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
   

    public class CustomerController : ControllerBase
    {
       

        // GET: api/QuaH/5
        
        [HttpGet]
        [Route("api/Customer")]
        public  IActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getCustomer @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
         
            return Ok(JSONString);
        }


        [HttpGet]
        [Route("api/CustomerContact")]

        public IActionResult getContact(string CmpId  , string CustCode)
        {
            string _QuatationNo = CmpId;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getCustContact @CmpId=" + _QuatationNo + " , @CustCode='" + CustCode + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }
        // POST: api/QuaH

        [HttpPost]
        [Route("api/Customer")]
        public IActionResult Post(Customer customer   )
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
                _cmd += ",@CmpId =" + customer.CmpId;

                _cmd += ",@AddrSubDistrict  ='" + customer.AddrSubDistrict + "'";
                _cmd += ",@AddrDistrict  ='" + customer.AddrDistrict + "'";
                _cmd += ",@AddrProvince  ='" + customer.AddrProvince + "'";
                _cmd += ",@AddrPostCode  ='" + customer.AddrPostCode + "'";


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

        

        // DELETE: api/QuaH/5
        [HttpDelete]
        [Route("api/Customer")]
        public void Delete(string id)
        {
            string _cmd = "";
            _cmd = "delete from msb.mCustomer where  CustomerCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }

    }
}
