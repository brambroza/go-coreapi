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

            _cmd = "exec dbo.getInvoice_Detail_All   @CmpId='" + CmpId + "', @UpdUser='" + User + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);


            List<Invoice> invoices = new List<Invoice>();

            foreach (DataRow r in datatable.Rows)
            {
                var invoice = new Invoice
                {
                    UpdUser = r["UpdUser"].ToString(),
                    InvoiceNo = r["InvoiceNo"].ToString(),
                    InvoiceDate = r["InvoiceDate"].ToString(),
                    InvoiceBy = r["InvoiceBy"].ToString(),
                    InvoiceState = r["InvoiceState"].ToString(),
                    CustomerCode = r["CustomerCode"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    CreditType = Convert.ToInt32(r["CreditType"]),
                    CreditDate = Convert.ToInt32(r["CreditDate"]),
                    ProjectName = r["ProjectName"].ToString(),
                    ReferCode = r["ReferCode"].ToString(),
                    VatType = Convert.ToInt32(r["VatType"]),
                    Remark = r["Remark"].ToString(),
                    Note = r["Note"].ToString(),
                    InvoiceAmt = Convert.ToDecimal(r["InvoiceAmt"]),
                    InvoiceDisPer = Convert.ToDecimal(r["InvoiceDisPer"]),
                    InvoiceDisAmt = Convert.ToDecimal(r["InvoiceDisAmt"]),
                    InvoiceNetAmt = Convert.ToDecimal(r["InvoiceNetAmt"]),
                    InvoiceVatAmt = Convert.ToDecimal(r["InvoiceVatAmt"]),
                    InvoiceVatPer = Convert.ToDecimal(r["InvoiceVatPer"]),
                    InvoiceGrandAmt = Convert.ToDecimal(r["InvoiceGrandAmt"]),
                    InvoiceGrandAmtTHB = r["InvoiceGrandAmtTHB"].ToString(),
                    InvoiceGrandAmtENB = r["InvoiceGrandAmtENB"].ToString(),
                    WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]),
                    ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]),
                    CmpId = r["CmpId"].ToString(),
                    DocState = Convert.ToInt32(r["DocState"]),
                    PriceStand = r["PriceStand"].ToString(),
                    PaymentDue = r["PaymentDue"].ToString(),
                    Shipping = r["Shipping"].ToString(),
                    StateApprove = Convert.ToInt32(r["StateApprove"]),
                    CustomerContactName = r["CustomerContactName"].ToString(),
                    JobType = Convert.ToInt32(r["JobType"]),
                    StateSendApprove = Convert.ToInt32(r["StateSendApprove"]),
                    QuotationNo = r["QuotationNo"].ToString(),
                    CustomerPONo = r["CustomerPONo"].ToString(),
                    SaleOrderNo = r["SaleOrderNo"].ToString(),
                    RevNo = Convert.ToInt32(r["RevNo"]),
                    TicketId = r["TicketId"].ToString()
                };

                invoice.items = new List<Invoice_detail>();


                foreach (DataRow x in dtItem.Select("InvoiceNo='" + r["InvoiceNo"].ToString() + "'"))
                {

                    var item = new Invoice_detail
                    {
                        UpdUser = x["UpdUser"].ToString(),
                        InvoiceNo = x["InvoiceNo"].ToString(),
                        Seq = Convert.ToInt32(x["Seq"]),
                        ProdCode = x["ProdCode"].ToString(),
                        ProdDescription = x["ProdDescription"].ToString(),
                        Qty = Convert.ToDecimal(x["Qty"]),
                        UnitCode = x["UnitCode"].ToString(),
                        UnitPrice = Convert.ToDecimal(x["UnitPrice"]),
                        Amt = Convert.ToDecimal(x["Amt"]),
                        DisPer = Convert.ToDecimal(x["DisPer"]),
                        DisAmt = Convert.ToDecimal(x["DisAmt"]),
                        NetAmt = Convert.ToDecimal(x["NetAmt"]),
                        PricePur = Convert.ToDecimal(x["PricePur"]),
                        CostAmt = Convert.ToDecimal(x["CostAmt"]),
                        ProfitAmt = Convert.ToDecimal(x["ProfitAmt"]),
                        GroupCaption1 = x["GroupCaption1"].ToString(),
                        GroupCaption2 = x["GroupCaption2"].ToString(),
                        GroupCaption3 = x["GroupCaption3"].ToString(),
                        CmpId = x["CmpId"].ToString(),
                        RevNo = Convert.ToInt32(r["RevNo"])
                    };

                    invoice.items.Add(item);

                }


                invoices.Add(invoice);
            }


            return Ok(invoices);
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
                _cmd += ",@InvoiceState ='" + inv.InvoiceState + "'";
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
                _cmd += ",@InvoiceVatPer =" + inv.InvoiceVatPer;
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
                _cmd += ",@TicketId ='" + inv.TicketId + "'";
                _cmd += ",@RevNo =" + inv.RevNo;


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
        public IActionResult setInvoiceApp([FromBody] Invoice inv)
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
                _cmd += ",@InvoiceState ='" + inv.InvoiceState + "'";
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
                _cmd += ",@TicketId ='" + inv.TicketId + "'";
                _cmd += ",@RevNo =" + inv.RevNo;


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
        public IActionResult setSendInvoiceApp([FromBody] Invoice inv)
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
                _cmd += ",@InvoiceState ='" + inv.InvoiceState + "'";
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
                _cmd += ",@TicketId ='" + inv.TicketId + "'";
                _cmd += ",@RevNo =" + inv.RevNo;


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
        public IActionResult setCopy([FromBody] InvoiceCopy inv)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.setInvoiceCopy";
                _cmd += "  @InvoiceNo  ='" + inv.InvoiceNo + "'";
                _cmd += ",@InvoiceNoNew ='" + inv.InvoiceNoNew + "'";
                _cmd += ",@CustomerCode  ='" + inv.CustomerCode + "'";
                _cmd += ",@CmpId ='" + inv.CmpId + "'";
                _cmd += ",@CustomerCode  ='" + inv.CustomerCode + "'";

                _cmd += ",@RevNo =" + inv.RevNo;


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
        public ActionResult setTicketFromInv(TicketFromQuo Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCreateTicketFromInv @CmpId='" + Quotation.CmpId + "'";
                _cmd += ", @CustomerName ='" + Quotation.CustomerName + "'";
                _cmd += " ,@ContactName='" + Quotation.ContactName + "'";
                _cmd += " , @ContactPhone='" + Quotation.ContactPhone + "'";
                _cmd += " , @ContactEmail='" + Quotation.ContactEmail + "'";
                _cmd += " , @Address='" + Quotation.Address + "'";
                _cmd += " , @TicketId='" + Quotation.TicketId + "'";
                _cmd += " , @AdditionalDetail='" + Quotation.AdditionalDetail + "'";
                _cmd += " , @UpdUser='" + Quotation.UpdUser + "'";

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
                    _cmd += ",@RevNo =" + inv[i].RevNo;


                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);

                    }
                    ;

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