using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;



namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]
    [Route("[controller]")]



    public class SaleOrderController : ControllerBase
    {


        // GET: api/QuaH/5 
        [HttpGet("[action]")]

        public IActionResult getSaleOrder([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            _cmd = "exec dbo.getSaleOrderAll @CmpId='" + cmpid + "', @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getSaleOrderItem_All @CmpId='" + cmpid + "' , @user='" + user + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);



            List<saleorder> saleorderlist = new List<saleorder>();

            // Loop through SaleOrder DataTable and map to SaleOrder object
            foreach (DataRow r in dt.Rows)
            {
                var saleorder = new saleorder
                {
                    SaleOrderNo = r["SaleOrderNo"].ToString(),
                    SaleOrderDate = DateTime.Parse(r["SaleOrderDate"].ToString()),
                    SaleOrderBy = r["SaleOrderBy"].ToString(),
                    SaleOrderState = r["SaleOrderState"].ToString(),
                    CustomerCode = r["CustomerCode"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    CreditType = Convert.ToInt32(r["CreditType"]),
                    CreditDate = Convert.ToInt32(r["CreditDate"]),
                    ProjectName = r["ProjectName"].ToString(),
                    ReferCode = r["ReferCode"].ToString(),
                    VatType = Convert.ToInt32(r["VatType"]),
                    Remark = r["Remark"].ToString(),
                    Note = r["Note"].ToString(),
                    SaleOrderAmt = Convert.ToDecimal(r["SaleOrderAmt"]),
                    SaleOrderDisPer = Convert.ToDecimal(r["SaleOrderDisPer"]),
                    SaleOrderDisAmt = Convert.ToDecimal(r["SaleOrderDisAmt"]),
                    SaleOrderNetAmt = Convert.ToDecimal(r["SaleOrderNetAmt"]),
                    SaleOrderVatAmt = Convert.ToDecimal(r["SaleOrderVatAmt"]),
                    SaleOrderGrandAmt = Convert.ToDecimal(r["SaleOrderGrandAmt"]),
                    SaleOrderGrandAmtTHB = r["SaleOrderGrandAmtTHB"].ToString(),
                    SaleOrderGrandAmtENB = r["SaleOrderGrandAmtENB"].ToString(),
                    WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]),
                    ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]),
                    CmpId = r["CmpId"].ToString(),
                    PaymentDue = r["PaymentDue"].ToString(),
                    Shipping = r["Shipping"].ToString(),
                    RevNo = Convert.ToInt32(r["RevNo"]),
                    CustomerContactName = r["CustomerContactName"].ToString(),
                    JobType = r["JobType"].ToString(),
                    QuotationNo = r["QuotationNo"].ToString(),
                    CustomerPONo = r["CustomerPONo"].ToString(),
                    TicketId = r["TicketId"].ToString(),
                    items = new List<SaleOrderItem>()
                };

                // Find the corresponding SaleOrderItems for each SaleOrder
                foreach (DataRow itemRow in dtItem.Select("SaleOrderNo = '" + saleorder.SaleOrderNo + "'"))
                {
                    var saleOrderItem = new SaleOrderItem
                    {
                        SaleOrderNo = itemRow["SaleOrderNo"].ToString(),
                        Seq = Convert.ToInt32(itemRow["Seq"]),
                        RevNo = Convert.ToInt32(itemRow["Seq"]),
                        ProdCode = itemRow["ProdCode"].ToString(),
                        ProdDescription = itemRow["ProdDescription"].ToString(),
                        Qty = Convert.ToDecimal(itemRow["Qty"]),
                        UnitCode = itemRow["UnitCode"].ToString(),
                        UnitPrice = Convert.ToDecimal(itemRow["UnitPrice"]),
                        Amt = Convert.ToDecimal(itemRow["Amt"]),
                        DisPer = Convert.ToDecimal(itemRow["DisPer"]),
                        DisAmt = Convert.ToDecimal(itemRow["DisAmt"]),
                        NetAmt = Convert.ToDecimal(itemRow["NetAmt"]),
                        PricePur = Convert.ToDecimal(itemRow["PricePur"]),
                        CostAmt = Convert.ToDecimal(itemRow["CostAmt"]),
                        ProfitAmt = Convert.ToDecimal(itemRow["ProfitAmt"]),
                        GrossProfitPer = Convert.ToDecimal(itemRow["GrossProfitPer"]),
                        GroupCaption1 = itemRow["GroupCaption1"].ToString(),
                        GroupCaption2 = itemRow["GroupCaption2"].ToString(),
                        GroupCaption3 = itemRow["GroupCaption3"].ToString(),
                        CmpId = itemRow["CmpId"].ToString()

                    };
                    saleorder.items.Add(saleOrderItem);
                }

                saleorderlist.Add(saleorder);
            }

            return Ok(saleorderlist);

        }



        // POST: api/QuaH

        [HttpPost("[action]")]
        public IActionResult setSaleOrder([FromBody] saleorder Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {
                string _cmd = "";

                if (Quotation.items.Count > 0)
                {
                    _cmd = "Delete From mdb.SaleOrder_Detail where SaleOrderNo='" + Quotation.items[0].SaleOrderNo + "'";
                    _cmd += " and  RevNo=" + Quotation.items[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }


                _cmd = "exec  dbo.setSaleOrder @SaleOrderNo='" + Quotation.SaleOrderNo + "' ";
                _cmd += " ,@SaleOrderDate='" + Quotation.SaleOrderDate.ToString("yyyy-MM-dd HH:mm", thaiCulture) + "' ,@SaleOrderBy='" + Quotation.SaleOrderBy + "'";
                _cmd += " ,@SaleOrderState='" + Quotation.SaleOrderState + "'";
                _cmd += " ,@CustomerCode='" + Quotation.CustomerCode + "'";
                _cmd += " ,@CreditType=" + Quotation.CreditType;
                _cmd += " ,@CreditDate=" + Quotation.CreditDate;
                _cmd += " ,@ProjectName='" + Tool.Tool.validateStr(Quotation.ProjectName) + "'";
                _cmd += " ,@ReferCode='" + Tool.Tool.validateStr(Quotation.ReferCode) + "'";
                _cmd += " ,@VatType=" + Quotation.VatType;
                _cmd += " ,@Remark='" + Quotation.Remark + "'";
                _cmd += " ,@Note='" + Quotation.Note + "'";
                _cmd += " ,@SaleOrderAmt=" + Quotation.SaleOrderAmt;
                _cmd += " ,@SaleOrderDisPer=" + Quotation.SaleOrderDisPer;
                _cmd += " ,@SaleOrderDisAmt=" + Quotation.SaleOrderDisAmt;
                _cmd += " ,@SaleOrderNetAmt=" + Quotation.SaleOrderNetAmt;
                _cmd += " ,@SaleOrderVatAmt=" + Quotation.SaleOrderVatAmt;
                _cmd += " ,@SaleOrderGrandAmt=" + Quotation.SaleOrderGrandAmt;
                _cmd += " ,@WithholdingTaxState=" + Quotation.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState='" + Quotation.ShowSignatureState + "'";
                _cmd += " ,@CmpId=" + Quotation.CmpId;
                _cmd += " ,@PriceStand='" + Quotation.PriceStand + "'";
                _cmd += " ,@PaymentDue='" + Quotation.PaymentDue + "'";
                _cmd += " ,@Shipping='" + Quotation.Shipping + "'";
                _cmd += " ,@RevNo=" + Quotation.RevNo;
                _cmd += " ,@CustContact='" + Tool.Tool.validateStr(Quotation.CustomerContactName) + "'";
                _cmd += ", @Jobtype=" + Quotation.JobType;
                _cmd += " ,@QuotationNo='" + Quotation.QuotationNo + "'";
                _cmd += " ,@CustomerPONo='" + Quotation.CustomerPONo + "'";
                _cmd += ", @TicketId='" + Quotation.TicketId + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                };



                int il = 0;
                for (int i = 0; i < Quotation.items.Count; i++)
                {
                    il++;
                    _cmd = "Exec setSaleOrderDetail @SaleOrderNo='" + Quotation.items[i].SaleOrderNo + "'";
                    _cmd += ",@Seq=" + il;
                    _cmd += ",@ProdCode='" + Quotation.items[i].ProdCode + "'";
                    _cmd += ",@ProdDesc='" + Tool.Tool.validateStr(Quotation.items[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + Quotation.items[i].UnitPrice;
                    _cmd += ",@UnitCode='" + Quotation.items[i].UnitCode + "'";
                    _cmd += ",@Qty=" + Quotation.items[i].Qty;
                    _cmd += ",@Amt=" + Quotation.items[i].Amt;
                    _cmd += ",@PricePur=" + Quotation.items[i].PricePur;
                    _cmd += ",@CostAmt=" + Quotation.items[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + Quotation.items[i].ProfitAmt;
                    _cmd += ",@RevNo=" + Quotation.items[i].RevNo;
                    _cmd += " ,@GroupCaption1='" + Tool.Tool.validateStr(Quotation.items[i].GroupCaption1) + "'";
                    _cmd += " ,@GroupCaption2='" + Tool.Tool.validateStr(Quotation.items[i].GroupCaption2) + "'";
                    _cmd += " ,@GroupCaption3='" + Tool.Tool.validateStr(Quotation.items[i].GroupCaption3) + "'";
                    _cmd += ",@CmpId='" + Quotation.items[i].CmpId + "'";

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
        public IActionResult setsaleordercopy(SaleOrderCopy Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setSaleOrderCopy @SaleOrderNo='" + Quotation.SaleOrderNo + "'";
                _cmd += ", @SaleOrderNoNew ='" + Quotation.SaleOrderNoNew + "'";
                _cmd += " ,@RevNo=" + Quotation.RevNo;
                _cmd += ", @CmpId ='" + Quotation.CmpId + "'";
                _cmd += ", @userlogin ='" + Quotation.userlogin + "'";



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
        public IActionResult setSaleOrderApp(QuoHApprove quoHApprove)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setSaleOrderApp @CmpId=" + quoHApprove.cmpid + " , @DocNo='" + quoHApprove.docno + "' , @RevNo =" + quoHApprove.revno + ",@User='" + quoHApprove.user + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    // linenotiapp(quoHApprove.docno);
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





        // DELETE: api/QuaH/5

        [HttpDelete("[action]")]
        public IActionResult deletesaleorder([FromQuery] string id, [FromQuery] int RevNo, [FromQuery] string cmpid)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {

                string _cmd = "";
                _cmd = "delete from mdb.SaleOrder where  SaleOrderNo='" + id + "' and RevNo=" + RevNo + " and Cmpid='" + cmpid + "'";

                DB.DBConn.ExecuteOnly(_cmd);
                _cmd = "delete from mdb.SaleOrder_Detail where  SaleOrderNo='" + id + "'  and RevNo=" + RevNo + " and Cmpid='" + cmpid + "'";

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

        public IActionResult getsaleorderdetail([FromQuery] string sono, [FromQuery] int RevNo, [FromQuery] string cmpid, [FromQuery] string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getSaleOrderDetail @SaleOrderNo='" + sono + "' , @RevNo=" + RevNo + ", @CmpId='" + cmpid + "', @userlogin='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        // POST: api/Qua
        [HttpPost("[action]")]

        public void setsaleorderdetail([FromBody] List<saleorderDetail> saleorderD)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (saleorderD.Count > 0)
                {
                    _cmd = "Delete From mdb.SaleOrder_Detail where SaleOrderNo='" + saleorderD[0].SaleOrderNo + "'";
                    _cmd += " and  RevNo=" + saleorderD[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < saleorderD.Count; i++)
                {
                    il++;
                    _cmd = "Exec setSaleOrderDetail @SaleOrderNo='" + saleorderD[i].SaleOrderNo + "'";
                    _cmd += ",@Seq=" + il;
                    _cmd += ",@ProdCode='" + saleorderD[i].ProdCode + "'";
                    _cmd += ",@ProdDesc='" + Tool.Tool.validateStr(saleorderD[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + saleorderD[i].UnitPrice;
                    _cmd += ",@UnitCode='" + saleorderD[i].UnitCode + "'";
                    _cmd += ",@Qty=" + saleorderD[i].Qty;
                    _cmd += ",@Amt=" + saleorderD[i].Amt;
                    _cmd += ",@PricePur=" + saleorderD[i].PricePur;
                    _cmd += ",@CostAmt=" + saleorderD[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + saleorderD[i].ProfitAmt;
                    _cmd += ",@RevNo=" + saleorderD[i].RevNo;
                    _cmd += " ,@GroupCaption1='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption1) + "'";
                    _cmd += " ,@GroupCaption2='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption2) + "'";
                    _cmd += " ,@GroupCaption3='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption3) + "'";
                    _cmd += ",@CmpId='" + saleorderD[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    };

                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

            }




        }








    }
}
