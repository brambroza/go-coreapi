using System.ComponentModel;
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

    public class AccountARBillingController : ControllerBase
    {
 

        [HttpGet("[action]")]
        public IActionResult getTARTBillingSlips_H([FromQuery] string cmpid, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTBillingSlips_H @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getTARTBillingSlips_D([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTBillingSlips_D @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        [HttpPost("[action]")]
        public IActionResult setTARTBillingSlips_H(TARTBillingSlips_H acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.set_TARTBillingSlips_H";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@BlhDocNo  ='" + acc.BlhDocNo + "'";
                    _cmd += ",@BlhDocDate  ='" + acc.BlhDocDate + "'";
                _cmd += ",@BlhDocType  ='" + acc.BlhDocType + "'";
                _cmd += ",@BlhDeptCode  ='" + acc.BlhDeptCode + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";
                _cmd += ",@CustomerCode  ='" + acc.CustomerCode + "'";
                _cmd += ",@BlhCustCrTerm =" + acc.BlhCustCrTerm;
                _cmd += ",@BlhDueDate ='" + acc.BlhDueDate+ "'";
                _cmd += ",@BlhDateOfBill ='" + acc.BlhDateOfBill+ "'";
                _cmd += ",@BlhDocNote  ='" + acc.BlhDocNote + "'";
                _cmd += ",@BlhGndTextEN  ='" + acc.BlhGndTextEN + "'";
                _cmd += ",@BlhGndTextTH  ='" + acc.BlhGndTextTH + "'";
                _cmd += ",@BlhCmpCode  ='" + acc.BlhCmpCode + "'";
                _cmd += ",@BlhCustCode  ='" + acc.BlhCustCode + "'";
                _cmd += ",@BlhReceiptDocDate ='" + acc.BlhReceiptDocDate+"'";
                _cmd += " , @BlhGndAmt=" + acc.BlhGndAmt  ; 
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




        [HttpPost("[action]")]
        public IActionResult setTARTBillingSlips_D(List<TARTBillingSlips_D> acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {
                string _cmd = "";
                if (acc.Count > 0)
                {
                    _cmd = "Delete From acc.TARTBillingSlips_D   where BlhDocNo='" + acc[0].BlhDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {

                    _cmd ="exec  dbo.set_TARTBillingSlips_D"; 
                    _cmd += " @UpdUser  ='"+ acc[i].UpdUser  +"'"; 
                    _cmd += ",@BlhDocNo  ='"+ acc[i].BlhDocNo  +"'"; 
                    _cmd += ",@BldSeqNo ="+ acc[i].BldSeqNo;
                    _cmd += ",@InvoiceNo  ='"+ acc[i].InvoiceNo  +"'"; 
                    _cmd += ",@InvoiceDate ='"+ acc[i].InvoiceDate+ "'";
                    _cmd += ",@InvoiceDueDate ='"+ acc[i].InvoiceDueDate+"'";
                    _cmd += ",@InvDocType  ='"+ acc[i].InvDocType  +"'"; 
                    _cmd += ",@BldCurCode  ='"+ acc[i].BldCurCode  +"'"; 
                    _cmd += ",@BldCurExcRate ="+ acc[i].BldCurExcRate; 
                    _cmd += ",@BldAmtBill ="+ acc[i].BldAmtBill; 
                    _cmd += ",@BldRcvAmtBill ="+ acc[i].BldRcvAmtBill; 
                    _cmd += ",@BldNetAmt ="+ acc[i].BldNetAmt; 
                    _cmd += ",@CmpId ='"+ acc[i].CmpId+"'"; 
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);
                    };

                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);


                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);


            }
            catch
            {

                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);


                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }


        }


        [HttpPost("[action]")]
        public IActionResult setTARTBillingSlips_H_Approve(TARTBillingSlips_H_Approve acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.set_TARTBillingSlips_H_Approve";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@BlhDocNo  ='" + acc.BlhDocNo + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";
                _cmd += ",@BlhStaApprove ='" + acc.BlhStaApprove + "'";
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



    }

}
