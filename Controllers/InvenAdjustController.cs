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

     [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class InvenAdjustController : ControllerBase
    { 

        [HttpGet("[action]")]
        public IActionResult getInvenAdjustList( [FromQuery] string CmpId,  [FromQuery]  string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_GetAdjustAll @CmpId='" + (CmpId) + "' , @User='" + userlogin + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
             string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(datatable);
            return Ok(qdetail);
        }


         [HttpPost("[action]")]
        public IActionResult setInvenAdjust(AdjustModel adjust )
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTrans";
                _cmd += " @UpdUser  ='" + adjust.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adjust.AdjustNo + "'"; 
                _cmd += ",@AdjustDate  ='" + adjust.AdjustDate + "'"; 
                _cmd += ",@AdjustBy  ='" + adjust.AdjustBy + "'";
                _cmd += ",@PurChaseNo  ='" + adjust.PurChaseNo + "'";
                _cmd += ",@CmpId ='" + adjust.CmpId+ "'"; 
                _cmd += ",@Remark  ='" + adjust.Remark + "'";
                _cmd += ",@WHId =" + adjust.WHId;
                _cmd += ",@WHLocId =" + adjust.WHLocId;
                _cmd += ", @AdjustType=" + adjust.AdjustType;

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
        public void DeleteAdjust( [FromQuery] string id  , [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Adjust where AdjustNo='" + id + "' and CmpId='" + cmpid  + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
