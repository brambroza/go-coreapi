using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getPurchaselist([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseAll @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getPurchaseItemAll @CmpId='" + cmpid + "'";
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
                purchase.PaymentDue = DateTime.Parse(r["PaymentDue"].ToString());
                purchase.Shipping = DateTime.Parse(r["Shipping"].ToString());
                purchase.RevNo = Convert.ToInt32(r["RevNo"]);
                purchase.ProjectNo = r["ProjectNo"].ToString();
                purchase.SupplierName = r["SupplierName"].ToString();
                purchase.ContactName = r["ContactName"].ToString();

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

                    purchase.items.Add(item);
                }

                purchases.Add(purchase);
            }

            return Ok(purchases);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchasercvlist([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchasercv @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseRcvDetail(
            [FromQuery] string id,
            [FromQuery] int RevNo,
            [FromQuery] string cmpid
        )
        {
            string _cmd;
            _cmd =
                "exec dbo.getPurchaseRcvDetail @PurchaseNo='"
                + (id)
                + "', @RevNo="
                + RevNo
                + ", @CmpId='"
                + cmpid
                + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseSelect([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseSelect  @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseDetail([FromQuery] string id, [FromQuery] int RevNo)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseDetail @PurchaseNo='" + (id) + "', @RevNo=" + RevNo;
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseTracking([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseTracking @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getPurchaseforRcv([FromQuery] string id, [FromQuery] int RevNo)
        {
            string _cmd;
            _cmd = "exec dbo.[getPurchaseDetailforRcv] @PurchaseNo='" + (id) + "', @RevNo=" + RevNo;
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public IActionResult setPurchaseApp(QuoHApprove purApp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.setPurchaseApp @CmpId='"
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
        public IActionResult setPurchase(Purchase po)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setPurchase";
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
                _cmd +=
                    " ,@PaymentDue  ='" + po.PaymentDue.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += " ,@Shipping  ='" + po.Shipping.ToString("yyyy-MM-dd", thaiCulture) + "'";
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
        public IActionResult DeletePurchase(int id, string DocNo, int RevNo)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd;
                _cmd =
                    "exec dbo.removePurchase @CmpId="
                    + Convert.ToInt16(id)
                    + " , @DocNo='"
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
        public IActionResult setPurchaseDetail(List<Purchase_Detail> po)
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
                        "Delete From pur.Purchase_Detail where PurchaseNo='"
                        + po[0].PurchaseNo
                        + "'";
                    _cmd += " and  RevNo=" + po[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < po.Count; i++)
                {
                    _cmd = "exec  dbo.setPurchaseDetail";
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
        public IActionResult setTicketPurchaseClose(TicketPurchaseList po)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTicketPurchase_Close";
                _cmd += " @User  ='" + po.DocBy + "'";
                _cmd += " ,@DocNo  ='" + po.DocNo + "'";
                _cmd += " ,@DocType  ='" + po.DocType + "'";
                _cmd += " ,@DocState  ='" + po.DocState + "'";
                _cmd += " ,@DocRemind ='" + po.DocRemind + "'";
                _cmd += "  ,@CmpId ='" + po.CmpId + "'";
                _cmd += " ,@RevNo =" + po.RevNo;
                _cmd += " , @TicketId='" + po.TicketId + "'";

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

        [HttpPost("[action]")]
        public IActionResult setTicketPurchaseCloseItem(List<TicketPurchase_Item> po)
        {
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                for (int i = 0; i < po.Count; i++)
                {
                    _cmd = "exec  dbo.[setTicketPurchase_Item_Close]";
                    _cmd += " @DocNo  ='" + po[i].DocNo + "'";
                    _cmd += ",@Seq =" + po[i].Seq;
                    _cmd += ",@ProdCode  ='" + po[i].ProdCode + "'";
                    _cmd += ",@RevNo =" + po[i].RevNo;
                    _cmd += ",@TicketId  ='" + po[i].TicketId + "'";
                    _cmd += ",@User  ='" + po[i].User + "'";
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
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setTicketPurchase(TicketPurchase po)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setTicketPurchase";
                _cmd += " @User  ='" + po.User + "'";
                _cmd += " ,@DocNo  ='" + po.DocNo + "'";
                _cmd += " ,@DocType  ='" + po.DocType + "'";
                _cmd += " ,@DocState  ='" + po.DocState + "'";
                _cmd += " ,@DocRemind ='" + po.DocRemind + "'";
                _cmd += "  ,@CmpId ='" + po.CmpId + "'";
                _cmd += " ,@RevNo =" + po.RevNo;
                _cmd += " , @TicketId='" + po.TicketId + "'";

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

        [HttpPost("[action]")]
        public IActionResult setTicketPurchaseItem(List<TicketPurchase_Item> po)
        {
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                for (int i = 0; i < po.Count; i++)
                {
                    _cmd = "exec  dbo.setTicketPurchase_Item";
                    _cmd += " @DocNo  ='" + po[i].DocNo + "'";
                    _cmd += ",@Seq =" + po[i].Seq;
                    _cmd += ",@ProdCode  ='" + po[i].ProdCode + "'";
                    _cmd += ",@RevNo =" + po[i].RevNo;
                    _cmd += ",@TicketId  ='" + po[i].TicketId + "'";
                    _cmd += ",@User  ='" + po[i].User + "'";
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
        public IActionResult setTicketPurchaseAssign(List<TicketPurchase_Assign> po)
        {
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                for (int i = 0; i < po.Count; i++)
                {
                    _cmd = "exec  dbo.setTicketPurchase_Assign";
                    _cmd += " @User  ='" + po[i].User + "'";
                    _cmd += ",@TicketId  ='" + po[i].TicketId + "'";
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

        [HttpGet("[action]")]
        public IActionResult getTicketPurchase([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getTicketPurchase @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getTicketPurchase_Item @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<TicketPurchaseList> purchases = new List<TicketPurchaseList>();

            foreach (DataRow r in dt.Rows)
            {
                var purchase = new TicketPurchaseList();
                purchase.DocBy = r["DocBy"].ToString();
                purchase.DocDate = DateTime.Parse(r["DocDate"].ToString());
                purchase.DocNo = r["DocNo"].ToString();
                purchase.DocType = r["DocType"].ToString();
                purchase.DocState = r["DocState"].ToString();
                purchase.DocRemind = r["DocRemind"].ToString();
                purchase.RevNo = Convert.ToInt32(r["RevNo"]);
                purchase.CmpId = r["CmpId"].ToString();
                purchase.TicketId = r["TicketId"].ToString();
                purchase.Seq = Convert.ToInt32(r["Seq"]);
                purchase.StateClose = Convert.ToInt32(r["StateClose"]);
                purchase.items = new List<TicketPurchaseItemList>();
                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                            + r["DocNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                            + "  and TicketId='"
                            + r["TicketId"].ToString()
                            + "'"
                    )
                )
                {
                    var item = new TicketPurchaseItemList();
                    item.DocNo = d["DocNo"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.ProdCode = d["ProdCode"].ToString();
                    item.TicketId = d["TicketId"].ToString();

                    item.RevNo = Convert.ToInt32(d["RevNo"]);
                    item.CmpId = d["CmpId"].ToString();

                    purchase.items.Add(item);
                }

                purchases.Add(purchase);
            }

            return Ok(new { tickets = purchases });
        }

        [HttpGet("[action]")]
        public IActionResult getTicketPurchaseBom(
            [FromQuery] string cmpid,
            [FromQuery] string user,
            [FromQuery] string DocNo,
            [FromQuery] string TicketId
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtItemPrice = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getTicketPurchase_Bom] @CmpId='"
                + cmpid
                + "' , @user='"
                + user
                + "',@TicketId='"
                + TicketId
                + "' , @DocNo='"
                + DocNo
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getTicketPurchase_BomItem]  @CmpId='"
                + cmpid
                + "' , @user='"
                + user
                + "',@TicketId='"
                + TicketId
                + "' , @DocNo='"
                + DocNo
                + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[getTicketPurchase_BomItem_Price]  @CmpId='"
                + cmpid
                + "' , @user='"
                + user
                + "',@TicketId='"
                + TicketId
                + "' , @DocNo='"
                + DocNo
                + "'";
            dtItemPrice = DB.DBConn.GetDataTable(_cmd);

            List<SalesBom> bomList = new List<SalesBom>();

            foreach (DataRow r in dt.Rows)
            {
                var bom = new SalesBom();
                bom.BomNo = r["BomNo"].ToString();
                bom.BomBy = r["BomBy"].ToString();
                bom.BomDate = DateTime.Parse(r["BomDate"].ToString());
                bom.SaleName = r["SaleName"].ToString();
                bom.CustomerName = r["CustomerName"].ToString();
                bom.CustomerContactName = r["CustomerContactName"].ToString();
                bom.CustomerContactEmail = r["CustomerContactEmail"].ToString();
                bom.CustomerContactPhone = r["CustomerContactPhone"].ToString();
                bom.ProjectName = r["ProjectName"].ToString();
                bom.ProjectStatus = Convert.ToInt32(r["ProjectStatus"]);
                bom.Remark = r["Remark"].ToString();
                bom.CmpId = r["CmpId"].ToString();
                bom.UpdUser = r["UpdUser"].ToString();
                bom.BomState = r["BomState"].ToString();
                bom.TicketId = r["TicketId"].ToString();
                bom.StateApp = Convert.ToInt32(r["StateApp"]);
                bom.RevNoMax = Convert.ToInt32(r["RevNoMax"]);
                bom.RevNo = Convert.ToInt32(r["RevNo"]);
                bom.items = new List<SalesBom_Detail>();
                foreach (
                    DataRow x in dtItem.Select(
                        "BomNo='"
                            + bom.BomNo
                            + "' and RevNo="
                            + bom.RevNo
                            + " and CmpId='"
                            + bom.CmpId
                            + "'"
                    )
                )
                {
                    var item = new SalesBom_Detail();
                    item.BomNo = bom.BomNo;
                    item.UpdUser = x["UpdUser"].ToString();
                    item.RevNo = bom.RevNo;
                    item.Seq = Convert.ToInt32(x["Seq"]);
                    item.ProdCode = x["ProdCode"].ToString();
                    item.ProdDescription = x["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(x["Qty"]);
                    item.UnitPrice = Convert.ToDecimal(x["UnitPrice"]);
                    item.UnitCode = x["UnitCode"].ToString();
                    item.Amt = Convert.ToDecimal(x["Amt"]);
                    item.CmpId = x["CmpId"].ToString();
                    item.ReplaceStatus = Convert.ToInt32(x["ReplaceStatus"]);
                    item.Vendor = "";
                    item.VendorName = "";
                    item.Remark = x["Remark"].ToString();
                    item.OutofstockStatus = Convert.ToInt32(x["OutofstockStatus"]);
                    item.ReplaceProdCode = x["ReplaceProdCode"].ToString();
                    item.StatePriceReq = Convert.ToInt32(x["StatePriceReq"]);
                    item.StateUpdatePrice = Convert.ToInt32(x["StateUpdatePrice"]);

                    item.bomitemPrice = new List<SalesBom_Price_Item>();

                    foreach (
                        DataRow i in dtItemPrice.Select(
                            "BomNo='"
                                + bom.BomNo
                                + "' and RevNo="
                                + bom.RevNo
                                + " and CmpId='"
                                + bom.CmpId
                                + "' and ProdCode='"
                                + item.ProdCode
                                + "' and Seq="
                                + item.Seq
                        )
                    )
                    {
                        var itemprice = new SalesBom_Price_Item();

                        itemprice.BomNo = bom.BomNo;
                        itemprice.UpdUser = i["UpdUser"].ToString();
                        itemprice.RevNo = bom.RevNo;
                        itemprice.Seq = Convert.ToInt32(i["Seq"]);
                        itemprice.ProdCode = i["ProdCode"].ToString();
                        itemprice.SupplierCode = i["SupplierCode"].ToString();
                        itemprice.SupplierName = i["SupplierName"].ToString();
                        itemprice.DeliveryDate = DateTime.Parse(i["DeliveryDate"].ToString());
                        itemprice.Qty = Convert.ToDecimal(i["Qty"]);
                        itemprice.QtyBal = Convert.ToDecimal(i["QtyBal"]);
                        itemprice.UnitPrice = Convert.ToDecimal(i["UnitPrice"]);
                        itemprice.UnitCode = i["UnitCode"].ToString();
                        itemprice.Amt = Convert.ToDecimal(i["Amt"]);
                        itemprice.CmpId = i["CmpId"].ToString();
                        itemprice.Remark = i["Remark"].ToString();
                        itemprice.PriceSeq = Convert.ToInt32(i["PriceSeq"]);
                        itemprice.StateDelete = Convert.ToInt32(i["StateDelete"]);
                        itemprice.StateSelect = Convert.ToInt32(i["StateSelect"]);

                        item.bomitemPrice.Add(itemprice);
                    }

                    bom.items.Add(item);
                }

                bomList.Add(bom);
            }

            return Ok(bomList);
        }
    }
}
