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
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class invenRtsController : ControllerBase
    { 
        [HttpGet("[action]")]
        public IActionResult getInventReturnSupl( [FromQuery] string CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getRtsAll @CmpId='" +  (CmpId) + "' , @User='" + userlogin + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
              string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(datatable);
            return Ok(qdetail);

           
        }

        
        [HttpPost("[action]")]
        public IActionResult setReturnSupl(ReturnToSuplModel rts)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReturnToSuplTrans"; 
                _cmd += " @UpdUser  ='" + rts.UpdUser + "'"; 
                _cmd += ",@ReturnToSuplNo  ='" + rts.ReturnToSuplNo + "'"; 
                _cmd += ",@ReturnToSuplDate  ='" + rts.ReturnToSuplDate + "'"; 
                _cmd += ",@ReturnToSuplBy ='" + rts.ReturnToSuplBy + "'";
                _cmd += ",@PurChaseNo  ='" + rts.PurChaseNo + "'"; 
                _cmd += ",@CmpId ='" + rts.CmpId+ "'";
                _cmd += ",@Remark  ='" + rts.Remark + "'";
                _cmd += ",@ReturnType =" + rts.ReturnType; 
                _cmd += ",@SupplierCode ='" + rts.SupplierCode + "'";
                _cmd += ",@WHId =" + rts.WHId; 
                _cmd += ",@WHLocId =" + rts.WHLocId; 
              


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
        public void DeleteInvenRts( [FromQuery]  string id , [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.ReturnToSupl where ReturnToSuplNo='" + id + "' and cmpid='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
