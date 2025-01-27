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
