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


    public class InvenRcvController : ControllerBase
    { 
        
        [HttpGet("[action]")]        
        public IActionResult getInvenRcv([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getReceiveAll @CmpId='" +  (CmpId) + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

      
        [HttpPost("[action]")]
        public IActionResult setInvenRcv(ReceiveModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReceiveTrans"; 
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@ReceiveNo  ='" + receive.ReceiveNo + "'"; 
                _cmd += ",@ReceiveDate  ='" + receive.ReceiveDate + "'"; 
                _cmd += ",@ReceiveBy  ='" + receive.ReceiveBy + "'"; 
                _cmd += ",@PurChaseNo  ='" + receive.PurChaseNo + "'";
                _cmd += ",@InvoiceNo  ='" + receive.InvoiceNo + "'"; 
                _cmd += ",@InvoiceDate  ='" + receive.InvoiceDate + "'"; 
                _cmd += ",@ReceiveType =" + receive.ReceiveType; 
                _cmd += ",@CmpId ='" + receive.CmpId +"'";
                _cmd += ",@Remark  ='" + receive.Remark + "'";
                _cmd += ",@SupplierCode  ='" + receive.SupplierCode + "'";
                _cmd += ",@WHId =" + receive.SysWHId;
                _cmd += ",@WHLocId =" + receive.SysWHLocId;

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

       
        [HttpDelete("[action]")]
        public void DeleteRcv(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Receive where ReceiveNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
