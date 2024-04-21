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


    public class InvenTransferWHController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getInvenTransferWH([FromQuery] int CmpId, [FromQuery] string userlogin )
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHAll] @CmpId='" + (CmpId) + "' , @User='" + userlogin + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
               string res = string.Empty;
            res = JsonConvert.SerializeObject(datatable);
            return Ok(res);
        }


        
        [HttpGet("[action]")]
        public IActionResult getInvenTransferWHRcvlist( [FromQuery] string CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHRcvAll] @CmpId='" + (CmpId) + "' , @User='" + userlogin + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
               string res = string.Empty;
            res = JsonConvert.SerializeObject(datatable);
            return Ok(res);
        }



        [HttpGet("[action]")]
        public IActionResult getInvenTransferWHProdWaidRcv([FromQuery] string  CmpId)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterforRcvTransferWH  @CmpId=" + Convert.ToInt16(CmpId) + " ";
            dt = DB.DBConn.GetDataTable(_cmd);
             string res = string.Empty;
            res = JsonConvert.SerializeObject(dt);
            return Ok(res);
        }



         [HttpPost("[action]")]
        public IActionResult setInvenTransferWH(TransferWHModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTrans"; 
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'"; 
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId; 
                _cmd += ",@Remark  ='" + TransWH.Remark + "'"; 
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.WHId; 
                _cmd += ",@WHLocId =" + TransWH.WHLocId;
                _cmd += ",@WHToId =" + TransWH.WHToId; 
                _cmd += ",@WHLocToId =" + TransWH.WHLocToId;



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

       
        [Route("api/InvenTransferWHRcv")]
        [HttpPost]
        public IActionResult InvenTransferWHRcv(TransferWHRcvModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTransRcv";
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'";
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId;
                _cmd += ",@Remark  ='" + TransWH.Remark + "'";
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.WHId;
                _cmd += ",@WHLocId =" + TransWH.WHLocId; 



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
        public void DeleteInvenTransferWH( [FromQuery] string id , [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.TrasferWH where TransferWHNo='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
