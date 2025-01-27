using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using goalongapi.Models;
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


namespace goalongapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    { 
        [HttpGet("[action]")]
        public IActionResult getInvoice([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoice @CmpId='" + CmpId + "' , @UserName='" + User + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }




        [HttpPost("[action]")]
        public IActionResult setInvoice([FromBody] Invoice inv)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.setInvoice";
                _cmd += " @UpdUser  ='" + inv.UpdUser + "'";
                _cmd += ",@InvoiceNo  ='" + inv.InvoiceNo + "'";
                _cmd += ",@InvoiceDate ='" + inv.InvoiceDate + "'";
                _cmd += ",@InvoiceBy  ='" + inv.InvoiceBy + "'";
                _cmd += ",@InvoiceState =" + inv.InvoiceState;
                _cmd += ",@CustomerCode  ='" + inv.CustomerCode + "'";
                _cmd += ",@CreditType =" + inv.CreditType;
                _cmd += ",@CreditDate =" + inv.CreditDate;
                _cmd += ",@ProjectName  ='" + inv.ProjectName + "'";
                _cmd += ",@ReferCode  ='" + inv.ReferCode + "'";
                _cmd += ",@VatType =" + inv.VatType;
                _cmd += ",@Remark  ='" + inv.Remark + "'";
                _cmd += ",@Note  ='" + inv.Note + "'";
                _cmd += ",@InvoiceAmt =" + inv.InvoiceAmt;
                _cmd += ",@InvoiceDisPer =" + inv.InvoiceDisPer;
                _cmd += ",@InvoiceDisAmt =" + inv.InvoiceDisAmt;
                _cmd += ",@InvoiceNetAmt =" + inv.InvoiceNetAmt;
                _cmd += ",@InvoiceVatAmt =" + inv.InvoiceVatAmt;
                _cmd += ",@InvoiceGrandAmt =" + inv.InvoiceGrandAmt;
                _cmd += ",@InvoiceGrandAmtTHB  ='" + inv.InvoiceGrandAmtTHB + "'";
                _cmd += ",@InvoiceGrandAmtENB  ='" + inv.InvoiceGrandAmtENB + "'";
                _cmd += ",@WithholdingTaxState =" + inv.WithholdingTaxState;
                _cmd += ",@ShowSignatureState =" + inv.ShowSignatureState;
                _cmd += ",@CmpId ='" + inv.CmpId + "'";
                _cmd += ",@DocState =" + inv.DocState;
                _cmd += ",@PriceStand  ='" + inv.PriceStand + "'";
                _cmd += ",@PaymentDue  ='" + inv.PaymentDue + "'";
                _cmd += ",@Shipping  ='" + inv.Shipping + "'";
                _cmd += ",@StateApprove =" + inv.StateApprove;
              
                _cmd += ",@CustomerContactName ='" + inv.CustomerContactName + "'";
                _cmd += ",@JobType =" + inv.JobType;
                _cmd += ",@StateSendApprove =" + inv.StateSendApprove;
        
                _cmd += ",@QuotationNo ='" + inv.QuotationNo + "'";
                _cmd += ",@CustomerPONo ='" + inv.CustomerPONo + "'";
                _cmd += ",@SaleOrderNo ='" + inv.SaleOrderNo + "'";


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
        public IActionResult setInvoice_Detail([FromBody] List<Invoice_detail> inv)
        {
            MsgReturn msgretrun = new MsgReturn();


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                if (inv.Count > 0)
                {
                    _cmd = "Delete From dbo.Invoice_Detail where InvoiceNo='" + inv[0].InvoiceNo + "'";
                    _cmd += " and  CmpId='" + inv[0].CmpId + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
               
                for (int i = 0; i < inv.Count; i++)
                {


                    _cmd = "exec  dbo.setInvoiceDetail";
                    _cmd += " @InvoiceNo  ='" + inv[i].InvoiceNo + "'";
                    _cmd += ",@Seq =" + inv[i].Seq;
                    _cmd += ",@ProdCode  ='" + inv[i].ProdCode + "'";
                    _cmd += ",@ProdDesc  ='" + inv[i].ProdDescription + "'";
                    _cmd += ",@Qty =" + inv[i].Qty;
                    _cmd += ",@UnitPrice =" + inv[i].UnitPrice;
                    _cmd += ",@UnitCode  ='" + inv[i].UnitCode + "'";
                    _cmd += ",@Amt =" + inv[i].Amt;
                    _cmd += ",@PricePur =" + inv[i].PricePur;
                    _cmd += ",@CostAmt =" + inv[i].CostAmt;
                    _cmd += ",@ProfitAmt =" + inv[i].ProfitAmt;
                    _cmd += ",@GroupCaption1  ='" + inv[i].GroupCaption1 + "'";
                    _cmd += ",@GroupCaption2  ='" + inv[i].GroupCaption2 + "'";
                    _cmd += ",@GroupCaption3  ='" + inv[i].GroupCaption3 + "'";
                    _cmd += ",@CmpId ='" + inv[i].CmpId + "'";


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


        [HttpGet("[action]")]

        public IActionResult getInvoiceDetail([FromQuery] string invoiceNo, [FromQuery] string cmpid, [FromQuery] string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getInvoice_Detail @InvoiceNo='" + invoiceNo + "' ,   @CmpId='" + cmpid + "', @UpdUser='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }





    }
}