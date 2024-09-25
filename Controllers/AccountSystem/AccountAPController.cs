using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class AccountAPController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getInvoiceAP([FromQuery] string CmpId, [FromQuery] string User, [FromQuery] string DocNo, [FromQuery] string VendorCode)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoiceForAPEntry @CmpId='" + CmpId + "' , @Userlogin='" + User + "' , @VendorCode='" + VendorCode + "' , @DocNo='" + DocNo + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getAPEntryD([FromQuery] string CmpId, [FromQuery] string User, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getAPEntry_D @CmpId='" + CmpId + "' , @Userlogin='" + User + "'  , @DocNo='" + DocNo + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getAPEntryList([FromQuery] string CmpId)
        {
            string _cmd;
            _cmd = "exec dbo.getAPEntrylist @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }




        [HttpPost("[action]")]
        public IActionResult setAPPayableH(TAPPayablesH acc)
        {

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTAPPayables_H";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@PayableNo  ='" + acc.PayableNo + "'";
                _cmd += ",@PayableDate ='" + acc.PayableDate + "'";
                _cmd += ",@SupplierCode  ='" + acc.SupplierCode + "'";
                _cmd += ",@SupplierDesc  ='" + acc.SupplierDesc + "'";
                _cmd += ",@CurCode  ='" + acc.CurCode + "'";
                _cmd += ",@Credit =" + acc.Credit;
                _cmd += ",@BuyType  ='" + acc.BuyType + "'";
                _cmd += ",@VatType  ='" + acc.VatType + "'";
                _cmd += ",@VatAmt =" + acc.VatAmt;
                _cmd += ",@Remark ='" + acc.Remark + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";


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
        public IActionResult setTAPPayableD(List<TAPPayablesD> acc)
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
                    _cmd = "Delete From acc.TAPPayables_D   where RchDocNo='" + acc[0].PayableNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {
                    _cmd = "exec  dbo.setTAPPayables_D";
                    _cmd += " @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += ",@PayableNo ='" + acc[i].PayableNo + "'";
                    _cmd += ",@Seq =" + acc[i].Seq;
                    _cmd += ",@InvoiceNo  ='" + acc[i].InvoiceNo + "'";
                    _cmd += ",@InvoiceDate ='" + acc[i].InvoiceDate + "'";
                    _cmd += ",@InvoiceAmt =" + acc[i].InvoiceAmt;
                    _cmd += ",@DueDate ='" + acc[i].DueDate + "'";
                    _cmd += ",@Discount =" + acc[i].Discount;
                    _cmd += ",@Cost =" + acc[i].Cost;
                    _cmd += ",@VatAmt =" + acc[i].VatAmt;
                    _cmd += ",@TotalAmt =" + acc[i].TotalAmt;
                    _cmd += ",@AccCode  ='" + acc[i].AccCode + "'";
                    _cmd += ",@Remark ='" + acc[i].Remark + "'";
                    _cmd += ",@RcvDate ='" + acc[i].RcvDate + "'";
                    _cmd += ",@DocrefNo  ='" + acc[i].DocrefNo + "'";
                    _cmd += ",@CmpId ='" + acc[i].CmpId + "'";
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






        #region "APCreditNote"


        [HttpGet("[action]")]
        public IActionResult getAPCredit_D([FromQuery] string CmpId, [FromQuery] string User, [FromQuery] string DocNo)
        {
            string _cmd;
            _cmd = "exec dbo.getTAPCreditNode_D @CmpId='" + CmpId + "' , @Userlogin='" + User + "'  , @DocNo='" + DocNo + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getAPCredit_H([FromQuery] string CmpId, [FromQuery] string Userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getTAPCreditNode_H @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getAPDebit_H([FromQuery] string CmpId, [FromQuery] string Userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getTAPDebitNode_H @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }



        [HttpPost("[action]")]
        public IActionResult setAPCreditH(TAPCreditNoteH acc)
        {

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTAPTCreditNote_H";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@CnhDocNo  ='" + acc.CnhDocNo + "'";
                _cmd += ",@CnhDocDate ='" + acc.CnhDocDate + "'";
                _cmd += ",@CnhBy  ='" + acc.CnhBy + "'";
                _cmd += ",@CmpId  ='" + acc.CmpId + "'";
                _cmd += ",@SupplierCode  ='" + acc.SupplierCode + "'";
                _cmd += ",@Remark  ='" + acc.Remark + "'";
                _cmd += ",@StateApprove ='" + acc.StateApprove + "'";
                _cmd += ",@VatType ='" + acc.VatType + "'";
                _cmd += ",@Vat =" + acc.Vat;
                _cmd += ",@Amt =" + acc.Amt;
                _cmd += ",@AmtTHB  ='" + acc.AmtTHB + "'";
                _cmd += ",@AmtEN  ='" + acc.AmtEN + "'";
                _cmd += ",@CNType =" + acc.CNType;
                _cmd += " ,@CnhCurCode='" + acc.CnhCurCode + "'";


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
        public IActionResult setTAPCreditD(List<TAPCreditNoteD> acc)
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
                    _cmd = "Delete From acc.TAPCreditNote_D   where CnhDocNo='" + acc[0].CnhDocNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {
                    _cmd = "exec  dbo.setTAPTCreditNote_D";
                    _cmd += " @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += ",@CnhDocNo  ='" + acc[i].CnhDocNo + "'";
                    _cmd += ",@Seq =" + acc[i].Seq;
                    _cmd += ",@PurchaseNo  ='" + acc[i].PurchaseNo + "'";
                    _cmd += ",@InvoiceNo  ='" + acc[i].InvoiceNo + "'";
                    _cmd += ",@InvoiceDate ='" + acc[i].InvoiceDate + "'";
                    _cmd += ",@Description  ='" + acc[i].Description + "'";
                    _cmd += ",@Quantity =" + acc[i].Quantity;
                    _cmd += ",@UnitCode ='" + acc[i].UnitCode + "'";
                    _cmd += ",@UnitPrice =" + acc[i].UnitPrice;
                    _cmd += ",@VatAmt =" + acc[i].VatAmt;
                    _cmd += ",@GrandAmt =" + acc[i].GrandAmt;
                    _cmd += ",@DocRefNo  ='" + acc[i].DocRefNo + "'";
                    _cmd += ",@CmpId ='" + acc[i].CmpId + "'";
                    _cmd += ",@Amount =" + acc[i].Amount;

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


        #endregion




        #region "Apbilling"

        [HttpGet("[action]")]
        public IActionResult getAPBilling_H([FromQuery] string CmpId, [FromQuery] string Userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getAPBilling_H @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getAPBilling_D([FromQuery] string CmpId, [FromQuery] string DocNo, [FromQuery] string Userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getAPBilling_D @CmpId='" + CmpId + "' , @DocNo='" + DocNo + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getAPPayblesForBilling([FromQuery] string CmpId, [FromQuery] string SupplierCode, [FromQuery] string DocNo, [FromQuery] string Userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.getAPPayblesForBilling @CmpId='" + CmpId + "'";
            _cmd += " , @SupplierCode='" + SupplierCode + "' , @DocNo='" + DocNo + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);

        }



        [HttpPost("[action]")]
        public IActionResult setAPBilling_H(APBiling_H acc)
        {

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTAPBilling_H  ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@BillNo ='" + acc.BillNo + "'";
                _cmd += ",@BillDate ='" + acc.BillDate + "'";
                _cmd += ",@BillBy  ='" + acc.BillBy + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";
                _cmd += ",@SupplierCode ='" + acc.SupplierCode + "'";
                _cmd += ",@CurCode ='" + acc.CurCode + "'";
                _cmd += ",@CreditDate ='" + acc.CreditDate + "'";
                _cmd += ",@PaymentDate ='" + acc.PaymentDate + "'";
                _cmd += ",@Remark  ='" + acc.Remark + "'";
                _cmd += ",@TotalAmt =" + acc.TotalAmt;
                _cmd += ",@TotalAmtTH  ='" + acc.TotalAmtTH + "'";
                _cmd += ",@TotalAmtEN  ='" + acc.TotalAmtEN + "'";
                _cmd += ",@StateApprove =" + acc.StateApprove;
                _cmd += ",@DateApprove ='" + acc.DateApprove + "'";
                _cmd += ",@UserApprove  ='" + acc.UserApprove + "'";
                _cmd += ",@TimeApprove ='" + acc.TimeApprove + "'";



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
        public IActionResult setAPBilling_D(List<APBilling_D> acc)
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
                    _cmd = "Delete From acc.TAPBilling_D   where BillNo='" + acc[0].BillNo + "'";
                    _cmd += "   and CmpId='" + acc[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < acc.Count; i++)
                {
                    _cmd = "exec  dbo.setTAPBilling_D";
                    _cmd += " @UpdUser  ='" + acc[i].UpdUser + "'";
                    _cmd += ",@BillNo ='" + acc[i].BillNo + "'";
                    _cmd += ",@Seq =" + acc[i].Seq;
                    _cmd += ",@DocRefNo ='" + acc[i].DocRefNo + "'";
                    _cmd += ",@InvoiceNo ='" + acc[i].InvoiceNo + "'";
                    _cmd += ",@InvoiceDate ='" + acc[i].InvoiceDate + "'";
                    _cmd += ",@DueDate ='" + acc[i].DueDate + "'";
                    _cmd += ",@InvoiceAmt =" + acc[i].InvoiceAmt;
                    _cmd += ",@BalAmt =" + acc[i].BalAmt;
                    _cmd += ",@PaidAmt =" + acc[i].PaidAmt;
                    _cmd += ",@DocRefType =" + acc[i].DocRefType + "'";
                    _cmd += ",@SeqDocRef =" + acc[i].SeqDocRef;
                    _cmd += ",@PaymentAmt =" + acc[i].PaymentAmt;
                    _cmd += ",@CmpId ='" + acc[i].CmpId + "'";

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








        #endregion









    }


}