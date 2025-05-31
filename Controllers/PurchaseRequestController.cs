using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
 
namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class PurchaseRequestController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getPurchaseRequestlist(
            [FromQuery] string cmpid,
            [FromQuery] string user
        )
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseRequestAll @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getPurchaseRequestItemAll @CmpId='" + cmpid + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<Purchase> purchases = new List<Purchase>();

            foreach (DataRow r in dt.Rows)
            {
                var purchase = new Purchase();
                purchase.UpdUser = r["UpdUser"].ToString();
                purchase.PurchaseNo = r["PurchaseNo"].ToString();
                purchase.PurchaseDate = r["PurchaseDate"].ToString();
                purchase.PurchaseBy = r["PurchaseBy"].ToString();
                purchase.PurchaseState = r["PurchaseState"].ToString();
                purchase.SupplierCode = r["SupplierCode"].ToString();
                purchase.CreditType = Convert.ToInt32(r["CreditType"]);
                purchase.CreditDate = Convert.ToInt32(r["CreditDate"]);
                purchase.ProjectName = r["ProjectName"].ToString();
                purchase.ReferCode = r["ReferCode"].ToString();
                purchase.VatType = Convert.ToInt32(r["VatType"]);
                purchase.Remark = r["Remark"].ToString();
                purchase.Note = r["Note"].ToString();
                purchase.PurchaseAmt = Convert.ToDecimal(r["PurchaseAmt"]);
                purchase.PurchaseDisPer = Convert.ToDecimal(r["PurchaseDisPer"]);
                purchase.PurchaseDisAmt = Convert.ToDecimal(r["PurchaseDisAmt"]);
                purchase.PurchaseNetAmt = Convert.ToDecimal(r["PurchaseNetAmt"]);
                purchase.PurchaseVatAmt = Convert.ToDecimal(r["PurchaseVatAmt"]);
                purchase.PurchaseVatPer = Convert.ToDecimal(r["PurchaseVatPer"]);
                purchase.PurchaseGrandAmt = Convert.ToDecimal(r["PurchaseGrandAmt"]);
                purchase.PurchaseGrandAmtTHB = r["PurchaseGrandAmtTHB"].ToString();
                purchase.PurchaseGrandAmtENB = r["PurchaseGrandAmtENB"].ToString();
                purchase.WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]);
                purchase.ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]);
                purchase.CmpId = r["CmpId"].ToString();
                purchase.DocState = Convert.ToInt32(r["DocState"]);
                purchase.PriceStand = r["PriceStand"].ToString();
                purchase.PaymentDue = r["PaymentDue"].ToString();
                purchase.Shipping = r["Shipping"].ToString();
                purchase.RevNo = Convert.ToInt32(r["RevNo"]);
                purchase.ProjectNo = r["ProjectNo"].ToString();
                purchase.SupplierName = r["SupplierName"].ToString();
                purchase.ContactName = r["ContactName"].ToString();
                purchase.SignaturePath = r["SignaturePath"].ToString();
                purchase.FullName = r["FullName"].ToString();

                purchase.items = new List<Purchase_Detail>();
                foreach (
                    DataRow d in dtItem.Select(
                        "PurchaseNo ='"
                            + r["PurchaseNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                    )
                )
                {
                    var item = new Purchase_Detail();
                    item.PurchaseNo = d["PurchaseNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
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
                    item.SupplierCode = d["SupplierCode"].ToString();
                    item.PurchaseNoRef = d["PurchaseNoRef"].ToString();

                    purchase.items.Add(item);
                }

                purchases.Add(purchase);
            }

            return Ok(purchases);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseRequestDetail([FromQuery] string id, [FromQuery] int RevNo)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseRequestDetail @PurchaseNo='" + (id) + "', @RevNo=" + RevNo;
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public IActionResult setPurchaseRequestApp(QuoHApprove purApp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.setPurchaseRequestApp @CmpId='"
                    + purApp.cmpid
                    + "' , @DocNo='"
                    + purApp.docno
                    + "' , @RevNo ="
                    + purApp.revno
                    + ",@User='"
                    + purApp.user
                    + "'";

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
        public IActionResult setPurchaseRequest(Purchase po)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setPurchaseRequest";
                _cmd += " @UpdUser  ='" + po.UpdUser + "'";
                _cmd += " ,@PurchaseNo  ='" + po.PurchaseNo + "'";
                _cmd += " ,@PurchaseDate  ='" + po.PurchaseDate + "'";
                _cmd += " ,@PurchaseBy  ='" + po.PurchaseBy + "'";
                _cmd += " ,@PurchaseState ='" + po.PurchaseState + "'";
                _cmd += " ,@SupplierCode  ='" + po.SupplierCode + "'";
                _cmd += " ,@CreditType =" + po.CreditType;
                _cmd += " ,@CreditDate =" + po.CreditDate;
                _cmd += " ,@ProjectName  ='" + po.ProjectName + "'";
                _cmd += " ,@ReferCode  ='" + po.ReferCode + "'";
                _cmd += " ,@VatType =" + po.VatType;
                _cmd += " ,@Remark  ='" + po.Remark + "'";
                _cmd += " ,@Note  ='" + po.Note + "'";
                _cmd += " ,@PurchaseAmt =" + po.PurchaseAmt;
                _cmd += " ,@PurchaseDisPer =" + po.PurchaseDisPer;
                _cmd += " ,@PurchaseDisAmt =" + po.PurchaseDisAmt;
                _cmd += " ,@PurchaseNetAmt =" + po.PurchaseNetAmt;
                _cmd += " ,@PurchaseVatAmt =" + po.PurchaseVatAmt;
                _cmd += " ,@PurchaseVatPer =" + po.PurchaseVatPer;
                _cmd += " ,@PurchaseGrandAmt =" + po.PurchaseGrandAmt;
                _cmd += " ,@PurchaseGrandAmtTHB  ='" + po.PurchaseGrandAmtTHB + "'";
                _cmd += " ,@PurchaseGrandAmtENB  ='" + po.PurchaseGrandAmtENB + "'";
                _cmd += " ,@WithholdingTaxState =" + po.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState =" + po.ShowSignatureState;
                _cmd += "  ,@CmpId ='" + po.CmpId + "'";
                _cmd += " ,@DocState =" + po.DocState;
                _cmd += " ,@PriceStand  ='" + po.PriceStand + "'";
                _cmd += " ,@PaymentDue  ='" + po.PaymentDue.ToString() + "'";
                _cmd += " ,@Shipping  ='" + po.Shipping.ToString() + "'";
                _cmd += " ,@RevNo =" + po.RevNo;
                _cmd += " ,@ProjectNo  ='" + po.ProjectNo + "'";
                _cmd += " , @ContactName='" + po.ContactName + "'";

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
                    return BadRequest(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult DeletePurchaseRequest(int id, string DocNo, int RevNo)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd;
                _cmd =
                    "exec dbo.removePurchaseRequest @CmpId='"
                    + id
                    + "', @DocNo='"
                    + DocNo
                    + "' , @RevNo ="
                    + RevNo;
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Delete Success !!";
                return Ok(msgretrun);
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setPurchaseRequestDetail(List<Purchase_Detail> po)
        {
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;
                if (po.Count > 0)
                {
                    _cmd =
                        "Delete From pur.PurchaseRequest_Detail where PurchaseNo='"
                        + po[0].PurchaseNo
                        + "'";
                    _cmd += " and  RevNo=" + po[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < po.Count; i++)
                {
                    _cmd = "exec  dbo.setPurchaseRequestDetail";
                    _cmd += " @PurchaseNo  ='" + po[i].PurchaseNo + "'";
                    _cmd += ",@Seq =" + po[i].Seq;
                    _cmd += ",@ProdCode  ='" + po[i].ProdCode + "'";
                    _cmd += ",@ProdDesc  ='" + po[i].ProdDescription + "'";
                    _cmd += ",@Qty =" + po[i].Qty;
                    _cmd += ",@UnitPrice =" + po[i].UnitPrice;
                    _cmd += ",@UnitCode  ='" + po[i].UnitCode + "'";
                    _cmd += ",@Amt =" + po[i].Amt;
                    _cmd += ",@PricePur =" + po[i].PricePur;
                    _cmd += ",@CostAmt =" + po[i].CostAmt;
                    _cmd += ",@ProfitAmt =" + po[i].ProfitAmt;
                    _cmd += ",@RevNo =" + po[i].RevNo;
                    _cmd += ",@GroupCaption1  ='" + po[i].GroupCaption1 + "'";
                    _cmd += ",@GroupCaption2  ='" + po[i].GroupCaption2 + "'";
                    _cmd += ",@GroupCaption3  ='" + po[i].GroupCaption3 + "'";
                    _cmd += ",@CmpId  ='" + po[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return BadRequest(msgretrun);
                    }
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
                return BadRequest(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setPurchaseRequestSendApp(QuoHApprove quoH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.setPurchaseRequestSendApp @CmpId='"
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
    }
}
