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

    public class AccountARController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getTARTReciveInv_H([FromQuery] string cmpid, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTReciveInv_H @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getInvoiceForAccRcv([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string CustomerCode, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoiceForAccRcv @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @CustomerCode='" + CustomerCode + "' , @DocNo ='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


    [HttpGet("[action]")]
        public IActionResult getInvoiceForAccBilling([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string CustomerCode, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoiceForAccBilling @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @CustomerCode='" + CustomerCode + "' , @DocNo ='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }





        [HttpGet("[action]")]
        public IActionResult getInvoiceForAccCredit([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string CustomerCode, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoiceForAccCredit @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @CustomerCode='" + CustomerCode + "' , @DocNo ='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }





        [HttpGet("[action]")]
        public IActionResult getARRciveInvReport([FromQuery] string cmpid, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getARRciveInvReport @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "'  ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }





        [HttpGet("[action]")]
        public IActionResult getTARTReciveInv_D([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTReciveInv_D @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getTARTReciveInv_I([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTReciveInv_I @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getTARTReciveInv_R([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTReciveInv_R @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getarvoucher([FromQuery] string cmpid, [FromQuery] string userlogin, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTARTReciveInv_Voucher @CmpId='" + cmpid + "' ,@Userlogin='" + userlogin + "' , @DocNo='" + DocNo + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }







        [HttpPost("[action]")]
        public IActionResult setTARTReciveInv_H(TARTReciveInv_H acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTARTReciveInv_H";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@RchDocNo ='" + acc.RchDocNo + "'";
                _cmd += " ,@RchDocDate  ='" + acc.RchDocDate + "'";
                _cmd += " ,@RchDocType ='" + acc.RchDocType + "'";
                _cmd += " ,@RchDeptCode ='" + acc.RchDeptCode + "'";
                _cmd += " ,@RchUsrCode  ='" + acc.RchUsrCode + "'";
                _cmd += " ,@RchType ='" + acc.RchType + "'";
                _cmd += " ,@RchCustCode ='" + acc.RchCustCode + "'";
                _cmd += " ,@RchCurCode  ='" + acc.RchCurCode + "'";
                _cmd += " ,@RchCurExcRate =" + acc.RchCurExcRate;
                _cmd += " ,@RchAmtTotal =" + acc.RchAmtTotal;
                _cmd += " ,@RchAmtDis =" + acc.RchAmtDis;
                _cmd += " ,@RchAmtChg =" + acc.RchAmtChg;
                _cmd += " ,@RchAmtGross =" + acc.RchAmtGross;
                _cmd += " ,@RchAmtVatEx =" + acc.RchAmtVatEx;
                _cmd += " ,@RchAmtVat =" + acc.RchAmtVat;
                _cmd += " ,@RchAmtNet =" + acc.RchAmtNet;
                _cmd += " ,@RchAmtDiffExcRate =" + acc.RchAmtDiffExcRate;
                _cmd += " ,@RchGndTextEN ='" + acc.RchGndTextEN + "'";
                _cmd += " ,@RchGndTextTH  ='" + acc.RchGndTextTH + "'";
                _cmd += " ,@RchDocNote ='" + acc.RchDocNote + "'";
                _cmd += " ,@RchRefGLDocNo ='" + acc.RchRefGLDocNo + "'";
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
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
        public IActionResult setTARTReciveInv_D(List<TARTReciveInv_D> acc)
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
                    _cmd = "Delete From acc.TARTReciveInv_D   where RchDocNo='" + acc[0].RchDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {
                    _cmd = "exec  dbo.setTARTReciveInv_D";
                    _cmd += "  @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += " ,@RchDocNo ='" + acc[i].RchDocNo + "'";
                    _cmd += " ,@RcdSeqNo =" + acc[i].RcdSeqNo;
                    _cmd += " ,@RcdType  ='" + acc[i].RcdType + "'";
                    _cmd += " ,@RcdStaVat ='" + acc[i].RcdStaVat + "'";
                    _cmd += " ,@RcdAccCode ='" + acc[i].RcdAccCode + "'";
                    _cmd += " ,@RcdDeptActivity  ='" + acc[i].RcdDeptActivity + "'";
                    _cmd += " ,@RcdDesc ='" + acc[i].RcdDesc + "'";
                    _cmd += " ,@RcdCurCode  ='" + acc[i].RcdCurCode + "'";
                    _cmd += " ,@RcdCurExcRate =" + acc[i].RcdCurExcRate;
                    _cmd += " ,@RcdCurAmt =" + acc[i].RcdCurAmt;
                    _cmd += " ,@RcdAmt =" + acc[i].RcdAmt;
                    _cmd += " ,@RcdNetAmt =" + acc[i].RcdNetAmt;
                    _cmd += " ,@RcdStaAuto ='" + acc[i].RcdStaAuto + "'";
                    _cmd += " ,@CmpId ='" + acc[i].CmpId + "'";
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
        public IActionResult setTARTReciveInv_I(List<TARTReciveInv_I> acc)
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
                    _cmd = "Delete From acc.TARTReciveInv_I   where RchDocNo='" + acc[0].RchDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {
                    _cmd = "exec  dbo.setTARTReciveInv_I";
                    _cmd += "  @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += " ,@RchDocNo ='" + acc[i].RchDocNo + "'";
                    _cmd += " ,@RciSeqNo =" + acc[i].RciSeqNo;
                    _cmd += " ,@RciDocNo  ='" + acc[i].RciDocNo + "'";
                    _cmd += " ,@RciDocType ='" + acc[i].RciDocType + "'";
                    _cmd += " ,@RciTypeDoc ='" + acc[i].RciTypeDoc + "'";
                    _cmd += " ,@RciRcvAmtVat =" + acc[i].RciRcvAmtVat;
                    _cmd += " ,@RciRcvAmtVatEx =" + acc[i].RciRcvAmtVatEx;
                    _cmd += " ,@RciRcvAmtNet =" + acc[i].RciRcvAmtNet;
                    _cmd += " ,@RciWhhDocNo  ='" + acc[i].RciWhhDocNo + "'";
                    _cmd += " ,@RciWhhAmtNet =" + acc[i].RciWhhAmtNet;
                    _cmd += " ,@RciDphAmtNet =" + acc[i].RciDphAmtNet;
                    _cmd += " ,@RciStaTaxInv ='" + acc[i].RciStaTaxInv + "'";
                    _cmd += " ,@RciTaxInvNo ='" + acc[i].RciTaxInvNo + "'";
                    _cmd += " ,@RciTaxInvDate ='" + acc[i].RciTaxInvDate + "'";
                    _cmd += " ,@RciARDocNo ='" + acc[i].RciARDocNo + "'";
                    _cmd += " ,@RciBLDocNo ='" + acc[i].RciBLDocNo + "'";
                    _cmd += " ,@CmpId ='" + acc[i].CmpId + "'";
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
        public IActionResult setTARTReciveInv_R(List<TARTReciveInv_R> acc)
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
                    _cmd = "Delete From acc.TARTReciveInv_R   where RchDocNo='" + acc[0].RchDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {

                    _cmd = "exec  dbo.setTARTReciveInv_R";
                    _cmd += "  @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += " ,@RchDocNo ='" + acc[i].RchDocNo + "'";
                    _cmd += " ,@RcrSeqNo =" + acc[i].RcrSeqNo;
                    _cmd += " ,@RcrRcvTypeCode  ='" + acc[i].RcrRcvTypeCode + "'";
                    _cmd += " ,@RcrBookCode ='" + acc[i].RcrBookCode + "'";
                    _cmd += " ,@RcrBnkCode ='" + acc[i].RcrBnkCode + "'";
                    _cmd += " ,@RcrBnkBchCode ='" + acc[i].RcrBnkBchCode + "'";
                    _cmd += " ,@RcrChequeNo ='" + acc[i].RcrChequeNo + "'";
                    _cmd += " ,@RcrChequeDate ='" + acc[i].RcrChequeDate + "'";
                    _cmd += " ,@RcrAmtFee =" + acc[i].RcrAmtFee;
                    _cmd += " ,@RcrAmt =" + acc[i].RcrAmt;
                    _cmd += " ,@RcrBnkAccCode ='" + acc[i].RcrBnkAccCode + "'";
                    _cmd += " ,@RcrAccCode ='" + acc[i].RcrAccCode + "'";
                    _cmd += " ,@RcrNote ='" + acc[i].RcrNote + "'";
                    _cmd += " ,@CmpId ='" + acc[i].CmpId + "'";
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
        public IActionResult setTARTReciveInvApprove(TARTReciveInvApprove acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTARTReciveInv_Approve";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@RchDocNo ='" + acc.RchDocNo + "'";
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
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