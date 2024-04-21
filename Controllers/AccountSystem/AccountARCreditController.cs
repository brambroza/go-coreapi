using System.ComponentModel;
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

    public class AccountARCreditController : ControllerBase
    {


        [HttpGet("[action]")]
        public IActionResult getTARTCreditNote_H([FromQuery] string cmpid, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTCreditNote_H @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getTARTCreditNote_D([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTCreditNote_D @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        [HttpPost("[action]")]
        public IActionResult setTARTCreditNote_H(TARTCreditNote_H acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.set_TARTCreditNote_H";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@CnhDocNo  ='" + acc.CnhDocNo + "'";
                _cmd += ",@CustomerCode  ='" + acc.CustomerCode + "'";
                _cmd += ",@CustomerContact  ='" + acc.CustomerContact + "'";
                _cmd += ",@CmpId ='" + acc.CmpId+ "'";
                _cmd += ",@CnhCurCode  ='" + acc.CnhCurCode + "'";
                _cmd += ",@CnhCurExcRate =" + acc.CnhCurExcRate;
                _cmd += ",@CnhVatType =" + acc.CnhVatType;
                _cmd += ",@CnhVatRate  ='" + acc.CnhVatRate + "'";
                _cmd += ",@CnhDocNote  ='" + acc.CnhDocNote + "'";
                _cmd += ",@CnhAmtTotal =" + acc.CnhAmtTotal;
                _cmd += ",@CnhAmtDis =" + acc.CnhAmtDis;
                _cmd += ",@CnhAmtchg =" + acc.CnhAmtchg;
                _cmd += ",@CnhAmtGross =" + acc.CnhAmtGross;
                _cmd += ",@CnhGndTextEN  ='" + acc.CnhGndTextEN + "'";
                _cmd += ",@CnhGndTextTH  ='" + acc.CnhGndTextTH + "'";
                _cmd += ",@CnhAmtVat =" + acc.CnhAmtVat;
                _cmd += ",@CnhAmtVatEx =" + acc.CnhAmtVatEx;
                _cmd += ",@CnhAmtNet =" + acc.CnhAmtNet;
                _cmd += ",@CnhRefARDocNo  ='" + acc.CnhRefARDocNo + "'";
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
        public IActionResult setTARTCreditNote_D(List<TARTCreditNote_D> acc)
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
                    _cmd = "Delete From acc.TARTCreditNote_D   where CnhDocNo='" + acc[0].CnhDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {

                    _cmd = "exec  dbo.set_TARTCreditNote_D";
                    _cmd += " @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += ",@CnhDocNo  ='" + acc[i].CnhDocNo + "'";
                    _cmd += ",@CndSeqNo =" + acc[i].CndSeqNo;
                    _cmd += ",@CndCode  ='" + acc[i].CndCode + "'";
                    _cmd += ",@CndDesc  ='" + acc[i].CndDesc + "'";
                    _cmd += ",@InvoiceNo  ='" + acc[i].InvoiceNo + "'";
                    _cmd += ",@CndQty =" + acc[i].CndQty;
                    _cmd += ",@CndUnit  ='" + acc[i].CndUnit + "'";
                    _cmd += ",@CndAmtTotal =" + acc[i].CndAmtTotal;
                    _cmd += ",@CndAmtGross =" + acc[i].CndAmtGross;
                    _cmd += ",@CmpId ='" + acc[i].CmpId+ "'";
                    _cmd += " , @CndUnitPrice=" + acc[i].CndUnitPrice + "" ; 
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
        public IActionResult setTARTCreditNote_H_Approve(TARTCreditNote_H_Approve acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.set_TARTCreditNote_H_Approve";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@CnhDocNo  ='" + acc.CnhDocNo + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";
                _cmd += ",@CnhStaApprove ='" + acc.CnhStaApprove + "'";
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
