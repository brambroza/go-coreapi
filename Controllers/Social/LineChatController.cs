using goalongapi.Models;
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

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]


    public class LineChatController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getchatmsg([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getcmpinfo @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        [HttpGet("[action]")]
        public IActionResult getContactSocial([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.[getSocailContact] @cmpid='" + cmpid + "'";
            DataTable dtx = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dtx);



            DataTable dt = new System.Data.DataTable();
            DataTable dtContact = new System.Data.DataTable();

            string type = "0";
            _cmd = "exec dbo.getCustomer @CmpId='" + cmpid + "' , @Type='" + type + "' , @StateGenQRCode=1";
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

            List<CustomerListContactLine> customerLists = new List<CustomerListContactLine>();

            foreach (DataRow r in dt.Rows)
            {
                var customer = new CustomerListContactLine();

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
                customer.StateGenQRCode = Convert.ToInt32(r["StateGenQRCode"].ToString());


                var prodlist = new List<Dictionary<string, object>>();
                foreach (DataRow row in dtx.Select(
                        " CustomerCode='" + customer.CustomerCode + "'"
                    ))
                {
                    var eventObj = new Dictionary<string, object>();
                    foreach (DataColumn column in dtx.Columns)
                    {
                        string lowercaseColumnName =
                            char.ToLowerInvariant(column.ColumnName[0])
                            + column.ColumnName.Substring(1);

                        eventObj[lowercaseColumnName] = row[column];
                    }

                    prodlist.Add(eventObj);

                    customer.constactline = prodlist;

                }





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









        [HttpPost("[action]")]
        public IActionResult setchatmsg(cmpinfo cmp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.set_company";
                _cmd += " @UpdUser  ='" + cmp.UpdUser + "'";
                _cmd += ",@CmpId  ='" + cmp.CmpId + "'";
                _cmd += ",@CmpName  ='" + cmp.CmpName + "'";
                _cmd += ",@CmpAddress  ='" + cmp.CmpAddress + "'";
                _cmd += ",@CmpTaxid  ='" + cmp.CmpTaxid + "'";
                _cmd += ",@CmpType =" + cmp.CmpType;
                _cmd += ",@StateActive =" + cmp.StateActive;
                _cmd += ",@Email  ='" + cmp.Email + "'";
                _cmd += ",@Fax  ='" + cmp.Fax + "'";
                _cmd += ",@Phone  ='" + cmp.Phone + "'";
                _cmd += ",@DateCreate ='" + cmp.DateCreate + "'";
                _cmd += ",@DateExprie ='" + cmp.DateExprie + "'";
                _cmd += ",@TelOffice  ='" + cmp.TelOffice + "'";
                _cmd += ",@CmpImg  ='" + cmp.CmpImg + "'";
                _cmd += ",@AddressShip  ='" + cmp.AddressShip + "'";
                _cmd += ",@AddrProvince  ='" + cmp.AddrProvince + "'";
                _cmd += ",@AddrDistrict  ='" + cmp.AddrDistrict + "'";
                _cmd += ",@AddrSubDistrict  ='" + cmp.AddrSubDistrict + "'";
                _cmd += ",@AddrPostCode  ='" + cmp.AddrPostCode + "'";
                _cmd += ",@CmpBranchCode  ='" + cmp.CmpBranchCode + "'";
                _cmd += ",@CmpBranchName  ='" + cmp.CmpBranchName + "'";
                _cmd += ",@WebSite  ='" + cmp.WebSite + "'";
                _cmd += ",@Remark  ='" + cmp.Remark + "'";
                _cmd += ",@DocPrefix  ='" + cmp.DocPrefix + "'";
                _cmd += ",@BankAccCode  ='" + cmp.BankAccCode + "'";
                _cmd += ",@BankAccName  ='" + cmp.BankAccName + "'";
                _cmd += ",@BankAccType  ='" + cmp.BankAccType + "'";
                _cmd += ",@BankCode  ='" + cmp.BankCode + "'";
                _cmd += ",@BankBranchCode  ='" + cmp.BankBranchCode + "'";
                _cmd += ",@LineId  ='" + cmp.LineId + "'";
                _cmd += ",@ColorThemeReport  ='" + cmp.ColorThemeReport + "'";
                _cmd += ",@FaviconUrl  ='" + cmp.FaviconUrl + "'";


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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }



        }















    }
}
