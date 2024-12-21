using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class QuaHController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getQuaH([FromQuery] string id, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.getQuotationAll @CmpId='" + id + "', @User='" + user + "'";

            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getQuaHList([FromQuery] string id, [FromQuery] string user)
        {
            string _cmd;
            List<QuotationList> quotationList = new List<QuotationList>();

            _cmd = "exec dbo.getQuotationAll @CmpId='" + id + "', @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getQuotationItemAll @CmpId='" + id + "', @User='" + user + "'";
            DataTable datatableDetail = DB.DBConn.GetDataTable(_cmd);

            foreach (DataRow r in datatable.Rows)
            {
                var quotaion = new QuotationList();

                quotaion.QuotationNo = r["QuotationNo"].ToString();
                quotaion.QuotationDate = r["QuotationDate"].ToString();
                quotaion.QuotationBy = r["QuotationBy"].ToString();
                quotaion.QuotationState = r["QuotationState"].ToString();
                quotaion.CustomerCode = r["CustomerCode"].ToString();
                quotaion.CustomerName = r["CustomerName"].ToString();
                quotaion.CreditType = Convert.ToInt32(r["CreditType"]);
                quotaion.CreditDate = Convert.ToInt32(r["CreditDate"]);
                quotaion.ProjectName = r["ProjectName"].ToString();
                quotaion.ReferCode = r["ReferCode"].ToString();
                quotaion.VatType = Convert.ToInt32(r["VatType"]);
                quotaion.Remark = r["Remark"].ToString();
                quotaion.Note = r["Note"].ToString();
                quotaion.QuotationAmt = Convert.ToDecimal(r["QuotationAmt"]);
                quotaion.QuotationDisPer = Convert.ToDecimal(r["QuotationDisPer"]);
                quotaion.QuotationDisAmt = Convert.ToDecimal(r["QuotationDisAmt"]);
                quotaion.QuotationNetAmt = Convert.ToDecimal(r["QuotationNetAmt"]);
                quotaion.QuotationVatAmt = Convert.ToDecimal(r["QuotationVatAmt"]);
                quotaion.QuotationGrandAmt = Convert.ToDecimal(r["QuotationGrandAmt"]);
                quotaion.QuotationGrandAmtTHB = r["QuotationGrandAmtTHB"].ToString();
                quotaion.QuotationGrandAmtENB = r["QuotationGrandAmtENB"].ToString();
                quotaion.WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]);
                quotaion.ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]);
                quotaion.CmpId = r["CmpId"].ToString();
                quotaion.DocState = r["DocState"].ToString();
                quotaion.PriceStand = r["PriceStand"].ToString();
                quotaion.PaymentDue = r["PaymentDue"].ToString();
                quotaion.Shipping = r["Shipping"].ToString();
                quotaion.RevNo = Convert.ToInt32(r["RevNo"]);
                quotaion.RevNoMax = Convert.ToInt32(r["RevNoMax"]);
                quotaion.StateApprove = Convert.ToInt32(r["StateApprove"]);

                quotaion.DateApprove = r["DateApprove"].ToString();
                quotaion.ApproveBy = r["ApproveBy"].ToString();
                quotaion.CustomerContactName = r["CustomerContactName"].ToString();
                quotaion.StateApproveToPO = Convert.ToInt32(r["StateApproveToPO"]);

                quotaion.DateApproveToPO = r["DateApproveToPO"].ToString();
                quotaion.ApproveToPOBy = r["ApproveToPOBy"].ToString();
                quotaion.JobType = r["JobType"].ToString();
                quotaion.StateSendApprove = Convert.ToInt32(r["StateSendApprove"]);
                quotaion.DateSendApprove = r["DateSendApprove"].ToString();
                quotaion.SendApproveBy = r["SendApproveBy"].ToString();
                quotaion.SignaturePath = r["SignaturePath"].ToString();
                quotaion.FullName = r["FullName"].ToString();
                quotaion.JobTypeFilter = r["JobTypeFilter"].ToString();
                quotaion.ImgPath = r["ImgPath"].ToString();
                quotaion.TicketId = r["TicketId"].ToString();
                quotaion.PhoneNo = r["PhoneNo"].ToString();
                quotaion.LineId = r["LineId"].ToString();
                quotaion.BomNo = r["BomNo"].ToString();

                if (
                    datatableDetail
                        .Select(
                            "QuotationNo ='"
                                + r["QuotationNo"].ToString()
                                + "'  and RevNo="
                                + Convert.ToInt32(r["RevNo"])
                        )
                        .Length > 0
                )
                {
                    quotaion.Items = new List<QuotationListItem>();
                }

                foreach (
                    DataRow d in datatableDetail.Select(
                        "QuotationNo ='"
                            + r["QuotationNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                    )
                )
                {
                    var item = new QuotationListItem();
                    item.QuotationNo = d["QuotationNo"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.ProdCode = d["ProdCode"].ToString();
                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.UnitCode = d["UnitCode"].ToString();

                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.Amt = Convert.ToDecimal(d["Amt"]);

                    item.DisPer = Convert.ToDecimal(d["DisPer"]);

                    item.DisAmt = Convert.ToDecimal(d["DisAmt"]);

                    item.NetAmt = Convert.ToDecimal(d["NetAmt"]);

                    item.PricePur = Convert.ToDecimal(d["PricePur"]);

                    item.CostAmt = Convert.ToDecimal(d["CostAmt"]);

                    item.ProfitAmt = Convert.ToDecimal(d["ProfitAmt"]);

                    item.RevNo = Convert.ToInt32(d["RevNo"]);

                    item.GroupCaption1 = d["GroupCaption1"].ToString();
                    item.GroupCaption2 = d["GroupCaption2"].ToString();
                    item.GroupCaption3 = d["GroupCaption3"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.GrossProfitPer = Convert.ToDecimal(d["GrossProfitPer"]);

                    item.MainProdCode = d["MainProdCode"].ToString();
                    item.MainSeq = Convert.ToInt32(d["MainSeq"]);
                    item.SeqSort = Convert.ToInt32(d["SeqSort"]);
                    quotaion.Items.Add(item);
                }

                quotationList.Add(quotaion);
            }

            return Ok(quotationList);
        }

        [HttpGet("[action]")]
        public IActionResult getQuaHByDocno(
            [FromQuery] string id,
            [FromQuery] string user,
            [FromQuery] string docno
        )
        {
            string _cmd;
            List<QuotationList> quotationList = new List<QuotationList>();

            _cmd =
                "exec dbo.getQuotationByDocNoAll @CmpId='"
                + id
                + "', @User='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.getQuotationItemByDocNoAll @CmpId='"
                + id
                + "', @User='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
            DataTable datatableDetail = DB.DBConn.GetDataTable(_cmd);

            foreach (DataRow r in datatable.Rows)
            {
                var quotaion = new QuotationList();

                quotaion.QuotationNo = r["QuotationNo"].ToString();
                quotaion.QuotationDate = r["QuotationDate"].ToString();
                quotaion.QuotationBy = r["QuotationBy"].ToString();
                quotaion.QuotationState = r["QuotationState"].ToString();
                quotaion.CustomerCode = r["CustomerCode"].ToString();
                quotaion.CustomerName = r["CustomerName"].ToString();
                quotaion.CreditType = Convert.ToInt32(r["CreditType"]);
                quotaion.CreditDate = Convert.ToInt32(r["CreditDate"]);
                quotaion.ProjectName = r["ProjectName"].ToString();
                quotaion.ReferCode = r["ReferCode"].ToString();
                quotaion.VatType = Convert.ToInt32(r["VatType"]);
                quotaion.Remark = r["Remark"].ToString();
                quotaion.Note = r["Note"].ToString();
                quotaion.QuotationAmt = Convert.ToDecimal(r["QuotationAmt"]);
                quotaion.QuotationDisPer = Convert.ToDecimal(r["QuotationDisPer"]);
                quotaion.QuotationDisAmt = Convert.ToDecimal(r["QuotationDisAmt"]);
                quotaion.QuotationNetAmt = Convert.ToDecimal(r["QuotationNetAmt"]);
                quotaion.QuotationVatAmt = Convert.ToDecimal(r["QuotationVatAmt"]);
                quotaion.QuotationGrandAmt = Convert.ToDecimal(r["QuotationGrandAmt"]);
                quotaion.QuotationGrandAmtTHB = r["QuotationGrandAmtTHB"].ToString();
                quotaion.QuotationGrandAmtENB = r["QuotationGrandAmtENB"].ToString();
                quotaion.WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]);
                quotaion.ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]);
                quotaion.CmpId = r["CmpId"].ToString();
                quotaion.DocState = r["DocState"].ToString();
                quotaion.PriceStand = r["PriceStand"].ToString();
                quotaion.PaymentDue = r["PaymentDue"].ToString();
                quotaion.Shipping = r["Shipping"].ToString();
                quotaion.RevNo = Convert.ToInt32(r["RevNo"]);
                quotaion.RevNoMax = Convert.ToInt32(r["RevNoMax"]);
                quotaion.StateApprove = Convert.ToInt32(r["StateApprove"]);

                quotaion.DateApprove = r["DateApprove"].ToString();
                quotaion.ApproveBy = r["ApproveBy"].ToString();
                quotaion.CustomerContactName = r["CustomerContactName"].ToString();
                quotaion.StateApproveToPO = Convert.ToInt32(r["StateApproveToPO"]);

                quotaion.DateApproveToPO = r["DateApproveToPO"].ToString();
                quotaion.ApproveToPOBy = r["ApproveToPOBy"].ToString();
                quotaion.JobType = r["JobType"].ToString();
                quotaion.StateSendApprove = Convert.ToInt32(r["StateSendApprove"]);
                quotaion.DateSendApprove = r["DateSendApprove"].ToString();
                quotaion.SendApproveBy = r["SendApproveBy"].ToString();
                quotaion.SignaturePath = r["SignaturePath"].ToString();
                quotaion.FullName = r["FullName"].ToString();
                quotaion.JobTypeFilter = r["JobTypeFilter"].ToString();
                quotaion.ImgPath = r["ImgPath"].ToString();
                quotaion.TicketId = r["TicketId"].ToString();
                quotaion.PhoneNo = r["PhoneNo"].ToString();
                quotaion.LineId = r["LineId"].ToString();
                quotaion.BomNo = r["BomNo"].ToString();

                if (
                    datatableDetail
                        .Select(
                            "QuotationNo ='"
                                + r["QuotationNo"].ToString()
                                + "'  and RevNo="
                                + Convert.ToInt32(r["RevNo"])
                        )
                        .Length > 0
                )
                {
                    quotaion.Items = new List<QuotationListItem>();
                }

                foreach (
                    DataRow d in datatableDetail.Select(
                        "QuotationNo ='"
                            + r["QuotationNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                    )
                )
                {
                    var item = new QuotationListItem();
                    item.QuotationNo = d["QuotationNo"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.ProdCode = d["ProdCode"].ToString();
                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.UnitCode = d["UnitCode"].ToString();

                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.Amt = Convert.ToDecimal(d["Amt"]);

                    item.DisPer = Convert.ToDecimal(d["DisPer"]);

                    item.DisAmt = Convert.ToDecimal(d["DisAmt"]);

                    item.NetAmt = Convert.ToDecimal(d["NetAmt"]);

                    item.PricePur = Convert.ToDecimal(d["PricePur"]);

                    item.CostAmt = Convert.ToDecimal(d["CostAmt"]);

                    item.ProfitAmt = Convert.ToDecimal(d["ProfitAmt"]);

                    item.RevNo = Convert.ToInt32(d["RevNo"]);

                    item.GroupCaption1 = d["GroupCaption1"].ToString();
                    item.GroupCaption2 = d["GroupCaption2"].ToString();
                    item.GroupCaption3 = d["GroupCaption3"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.GrossProfitPer = Convert.ToDecimal(d["GrossProfitPer"]);

                    item.MainProdCode = d["MainProdCode"].ToString();
                    item.MainSeq = Convert.ToInt32(d["MainSeq"]);
                    item.SeqSort = Convert.ToInt32(d["SeqSort"]);
                    quotaion.Items.Add(item);
                }

                quotationList.Add(quotaion);
            }

            return Ok(quotationList);
        }

        [HttpGet("[action]")]
        public IActionResult getQuaHAccept([FromQuery] string id, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.getQuotationAccept @CmpId='" + id + "', @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getQuaHState([FromQuery] string cmpid, [FromQuery] string state)
        {
            string _cmd;
            _cmd = "exec dbo.getQuotationapprove @CmpId='" + (cmpid) + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        // POST: api/QuaH

        [HttpPost("[action]")]
        public IActionResult setQuoH([FromBody] Quotation Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec  dbo.setQuotation @QuotationNo='"
                    + Quotation.QuotationNo
                    + "' ,@QuotationDate='"
                    + Quotation.QuotationDate
                    + "' ,@QuotationBy='"
                    + Quotation.QuotationBy
                    + "'";
                _cmd += " ,@QuotationState='" + Quotation.QuotationState + "'";
                _cmd += " ,@CustomerCode='" + Quotation.CustomerCode + "'";
                _cmd += " ,@CreditType=" + Quotation.CreditType;
                _cmd += " ,@CreditDate=" + Quotation.CreditDate;
                _cmd += " ,@ProjectName='" + Tool.Tool.validateStr(Quotation.ProjectName) + "'";
                _cmd += " ,@ReferCode='" + Tool.Tool.validateStr(Quotation.ReferCode) + "'";
                _cmd += " ,@VatType=" + Quotation.VatType;
                _cmd += " ,@Remark='" + Quotation.Remark + "'";
                _cmd += " ,@Note='" + Quotation.Note + "'";
                _cmd += " ,@QuotationAmt=" + Quotation.QuotationAmt;
                _cmd += " ,@QuotationDisPer=" + Quotation.QuotationDisPer;
                _cmd += " ,@QuotationDisAmt=" + Quotation.QuotationDisAmt;
                _cmd += " ,@QuotationNetAmt=" + Quotation.QuotationNetAmt;
                _cmd += " ,@QuotationVatAmt=" + Quotation.QuotationVatAmt;
                _cmd += " ,@QuotationGrandAmt=" + Quotation.QuotationGrandAmt;
                _cmd += " ,@WithholdingTaxState=" + Quotation.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState=" + Quotation.ShowSignatureState;
                _cmd += " ,@CmpId='" + Quotation.CmpId + "'";
                _cmd += " ,@PriceStand='" + Quotation.PriceStand + "'";
                _cmd += " ,@PaymentDue='" + Quotation.PaymentDue + "'";
                _cmd += " ,@Shipping='" + Quotation.Shipping + "'";
                _cmd += " ,@RevNo=" + Quotation.RevNo;
                _cmd +=
                    " ,@CustContact='" + Tool.Tool.validateStr(Quotation.CustomerContactName) + "'";
                _cmd += ", @Jobtype='" + Quotation.Jobtype + "'";
                _cmd += ", @TicketId ='" + Quotation.TicketId + "'";
                _cmd += ", @BomNo ='" + Quotation.BomNo + "'";

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
        public IActionResult setTicketFromQuo(TicketFromQuo Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCreateTicketFromQuo @CmpId='" + Quotation.CmpId + "'";
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
        public IActionResult setTicketFromBom(TicketFromQuo Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCreateTicketFromBom @CmpId='" + Quotation.CmpId + "'";
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
        public IActionResult QuaHCopy(QuotationCopy Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setQuotationCopy @QuotationNo='" + Quotation.QuotationNo + "'";
                _cmd += ", @QuotationNoNew ='" + Quotation.QuotationNoNew + "'";
                _cmd += " ,@RevNo=" + Quotation.RevNo;
                _cmd += " , @CmpId='" + Quotation.CmpId + "'";
                _cmd += " , @CustomerCode='" + Quotation.CustomerCode + "'";
                _cmd += " , @TicketId='" + Quotation.TicketId + "'";

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
        public IActionResult QuaHApp(QuoHApprove quoHApprove)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.setQuotationApp @CmpId="
                    + quoHApprove.cmpid
                    + " , @DocNo='"
                    + quoHApprove.docno
                    + "' , @RevNo ="
                    + quoHApprove.revno
                    + ",@User='"
                    + quoHApprove.user
                    + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    //linenotiapp(quoHApprove.docno);
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
        public IActionResult QuaHSendApp(QuoHApprove quoH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.setQuotationSendApp @CmpId='"
                    + quoH.cmpid
                    + "' , @DocNo='"
                    + quoH.docno
                    + "' , @RevNo ="
                    + quoH.revno
                    + ",@User='"
                    + quoH.user
                    + "'";

                System.Data.DataTable dt = DB.DBConn.GetDataTable(_cmd);
                if (dt.Rows.Count > 0)
                {
                    /*  var x = linenotisendapp(quoH.docno); */


                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(new { approvedoc = dt.Rows[0][0] });
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

        // DELETE: api/QuaH/5

        [HttpDelete("[action]")]
        public IActionResult DeleteQuoH(string id, int RevNo)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd =
                    "delete from mdb.Quotation where  QuotationNo='" + id + "' and RevNo=" + RevNo;

                DB.DBConn.ExecuteOnly(_cmd);
                _cmd =
                    "delete from mdb.Quotation_Detail where  QuotationNo='"
                    + id
                    + "'  and RevNo="
                    + RevNo;

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
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

        [HttpGet("[action]")]
        public IActionResult GetQuaHRev(string cmpid, string DocNo, int RevNo)
        {
            string _cmd;

            QuotationList quotaion = new QuotationList();

            _cmd =
                "exec dbo.getQuotation @CmpId='"
                + cmpid
                + "', @DocNo='"
                + DocNo
                + "' , @RevNo ="
                + RevNo;
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.getQuotationDetail @QuotationNo='"
                + DocNo
                + "' , @RevNo="
                + RevNo
                + ", @CmpId='"
                + cmpid
                + "'";
            DataTable datatableDetail = DB.DBConn.GetDataTable(_cmd);

            foreach (DataRow r in datatable.Rows)
            {
                //  var quotaion = new QuotationList();

                quotaion.QuotationNo = r["QuotationNo"].ToString();
                quotaion.QuotationDate = r["QuotationDate"].ToString();
                quotaion.QuotationBy = r["QuotationBy"].ToString();
                quotaion.QuotationState = r["QuotationState"].ToString();
                quotaion.CustomerCode = r["CustomerCode"].ToString();
                quotaion.CustomerName = r["CustomerName"].ToString();
                quotaion.CreditType = Convert.ToInt32(r["CreditType"]);
                quotaion.CreditDate = Convert.ToInt32(r["CreditDate"]);
                quotaion.ProjectName = r["ProjectName"].ToString();
                quotaion.ReferCode = r["ReferCode"].ToString();
                quotaion.VatType = Convert.ToInt32(r["VatType"]);
                quotaion.Remark = r["Remark"].ToString();
                quotaion.Note = r["Note"].ToString();
                quotaion.QuotationAmt = Convert.ToDecimal(r["QuotationAmt"]);
                quotaion.QuotationDisPer = Convert.ToDecimal(r["QuotationDisPer"]);
                quotaion.QuotationDisAmt = Convert.ToDecimal(r["QuotationDisAmt"]);
                quotaion.QuotationNetAmt = Convert.ToDecimal(r["QuotationNetAmt"]);
                quotaion.QuotationVatAmt = Convert.ToDecimal(r["QuotationVatAmt"]);
                quotaion.QuotationGrandAmt = Convert.ToDecimal(r["QuotationGrandAmt"]);
                quotaion.QuotationGrandAmtTHB = r["QuotationGrandAmtTHB"].ToString();
                quotaion.QuotationGrandAmtENB = r["QuotationGrandAmtENB"].ToString();
                quotaion.WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]);
                quotaion.ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]);
                quotaion.CmpId = r["CmpId"].ToString();
                quotaion.DocState = r["DocState"].ToString();
                quotaion.PriceStand = r["PriceStand"].ToString();
                quotaion.PaymentDue = r["PaymentDue"].ToString();
                quotaion.Shipping = r["Shipping"].ToString();
                quotaion.RevNo = Convert.ToInt32(r["RevNo"]);
                quotaion.RevNoMax = Convert.ToInt32(r["RevNoMax"]);
                quotaion.StateApprove = Convert.ToInt32(r["StateApprove"]);

                quotaion.DateApprove = r["DateApprove"].ToString();
                quotaion.ApproveBy = r["ApproveBy"].ToString();
                quotaion.CustomerContactName = r["CustomerContactName"].ToString();
                quotaion.StateApproveToPO = Convert.ToInt32(r["StateApproveToPO"]);

                quotaion.DateApproveToPO = r["DateApproveToPO"].ToString();
                quotaion.ApproveToPOBy = r["ApproveToPOBy"].ToString();
                quotaion.JobType = r["JobType"].ToString();
                quotaion.StateSendApprove = Convert.ToInt32(r["StateSendApprove"]);
                quotaion.DateSendApprove = r["DateSendApprove"].ToString();
                quotaion.SendApproveBy = r["SendApproveBy"].ToString();
                quotaion.SignaturePath = r["SignaturePath"].ToString();
                quotaion.FullName = r["FullName"].ToString();
                quotaion.JobTypeFilter = r["JobTypeFilter"].ToString();
                quotaion.ImgPath = r["ImgPath"].ToString();
                quotaion.BomNo = r["BomNo"].ToString();

                if (
                    datatableDetail
                        .Select(
                            "QuotationNo ='"
                                + r["QuotationNo"].ToString()
                                + "'  and RevNo="
                                + Convert.ToInt32(r["RevNo"])
                        )
                        .Length > 0
                )
                {
                    quotaion.Items = new List<QuotationListItem>();
                }

                foreach (
                    DataRow d in datatableDetail.Select(
                        "QuotationNo ='"
                            + r["QuotationNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                    )
                )
                {
                    var item = new QuotationListItem();
                    item.QuotationNo = d["QuotationNo"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.ProdCode = d["ProdCode"].ToString();
                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.UnitCode = d["UnitCode"].ToString();

                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.Amt = Convert.ToDecimal(d["Amt"]);

                    item.DisPer = Convert.ToDecimal(d["DisPer"]);

                    item.DisAmt = Convert.ToDecimal(d["DisAmt"]);

                    item.NetAmt = Convert.ToDecimal(d["NetAmt"]);

                    item.PricePur = Convert.ToDecimal(d["PricePur"]);

                    item.CostAmt = Convert.ToDecimal(d["CostAmt"]);

                    item.ProfitAmt = Convert.ToDecimal(d["ProfitAmt"]);

                    item.RevNo = Convert.ToInt32(d["RevNo"]);

                    item.GroupCaption1 = d["GroupCaption1"].ToString();
                    item.GroupCaption2 = d["GroupCaption2"].ToString();
                    item.GroupCaption3 = d["GroupCaption3"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.GrossProfitPer = Convert.ToDecimal(d["GrossProfitPer"]);
                    item.MainSeq = Convert.ToInt32(d["MainSeq"]);
                    item.SeqSort = int.Parse(d["SeqSort"].ToString());
                    quotaion.Items.Add(item);
                }
            }

            return Ok(quotaion);
        }

        [HttpGet]
        [Route("api/linenotisendapp")]
        public string linenotisendapp(string qno)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getNotisendappqt '" + qno + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            StringBuilder _msg;

            foreach (DataRow r in dt.Rows)
            {
                _msg = new StringBuilder();
                _msg.Append(" ส่งอนุมัติ ใบเสนอราคา");
                _msg.AppendLine();
                _msg.Append("ชื่อลูกค้า : " + r["CustomerName"].ToString());
                _msg.AppendLine();
                _msg.Append("เลขใบเสนอราคา : " + r["QuotationNo"].ToString());
                _msg.AppendLine();
                _msg.Append("วันที่ : " + r["QuotationDate"].ToString());
                _msg.AppendLine();
                _msg.Append("ผู้สร้างใบเสนอรา : " + r["QuotationBy"].ToString());
                _msg.AppendLine();
                _msg.Append("โปรเจค : " + r["ProjectName"].ToString());
                _msg.AppendLine();
                _msg.Append("อ้างอิง : " + r["ReferCode"].ToString());
                _msg.AppendLine();
                _msg.Append("Note : " + r["Remark"].ToString());

                lineNotify(_msg.ToString());
            }

            return "value";
        }

        [HttpGet]
        [Route("api/linenotiapp")]
        public string linenotiapp(string qno)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getNotiappqt '" + qno + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            StringBuilder _msg;

            foreach (DataRow r in dt.Rows)
            {
                _msg = new StringBuilder();
                _msg.Append(" ใบเสนอราคาเลขที่ " + r["QuotationNo"].ToString() + " อนุมัติแล้ว");
                _msg.AppendLine();
                _msg.Append(" อนุมัติโดย : " + r["QuotationBy"].ToString());

                lineNotify(_msg.ToString());
            }

            return "value";
        }

        [HttpGet]
        [Route("api/lineNotify")]
        private void lineNotify(string msg)
        {
            string token = "8LtACGcDqZS6ZouELpfLZPc8Trl6LWgbEErI0pgjSeg";
            token = "pRCg56EkubWTcMhvkgC64GBZTTZkCG2e0bMAZ2g1JFg";
            try
            {
                var request = (HttpWebRequest)
                    WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", msg);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
