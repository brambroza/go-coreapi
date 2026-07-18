using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class BomController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getSalesbom([FromQuery] string id, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtItemPrice = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_All @CmpId='" + id + "' , @user='" + user + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.sp_getSaleBomItem_All @CmpId='" + id + "' , @user='" + user + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.sp_getSaleBomItem_Price_All @CmpId='" + id + "' , @user='" + user + "'";
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
                bom.StateNotification = r["StateNotification"].ToString(); 
                bom.TicketIdRef = r["TicketIdRef"].ToString();
                bom.items = new List<SalesBom_Detail>();
                foreach (
                    DataRow x in dtItem.Select(
                        "BomNo='"
                            + bom.BomNo
                            + "' and RevNo="
                            + bom.RevNo
                            + " and CmpId='"
                            + bom.CmpId
                            + "'  "
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
                    item.VendorCode = "";
                    item.VendorName = "";
                    item.Remark = x["Remark"].ToString();
                    item.OutofstockStatus = Convert.ToInt32(x["OutofstockStatus"]);
                    item.ReplaceProdCode = x["ReplaceProdCode"].ToString();
                    item.StatePriceReq = Convert.ToInt32(x["StatePriceReq"]);
                    item.StateUpdatePrice = Convert.ToInt32(x["StateUpdatePrice"]);
                    item.SeqSort = Convert.ToInt32(x["SeqSort"]);

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
                    /* start bom item replace */
                    /* item.bomitemReplace = new List<SalesBom_Detail>();

                    foreach (
                        DataRow xr in dtItem.Select(
                            "BomNo='"
                                + bom.BomNo
                                + "' and RevNo="
                                + bom.RevNo
                                + " and CmpId='"
                                + bom.CmpId
                                + "' and ReplaceProdCode<>''"
                                + "  and ReplaceProdCode='"
                                + item.ProdCode
                                + "'"
                        )
                    )
                    {
                        var itemxr = new SalesBom_Detail();
                        itemxr.BomNo = bom.BomNo;
                        itemxr.UpdUser = xr["UpdUser"].ToString();
                        itemxr.RevNo = bom.RevNo;
                        itemxr.Seq = Convert.ToInt32(xr["Seq"]);
                        itemxr.ProdCode = xr["ProdCode"].ToString();
                        itemxr.ProdDescription = xr["ProdDescription"].ToString();
                        itemxr.Qty = Convert.ToDecimal(xr["Qty"]);
                        itemxr.UnitPrice = Convert.ToDecimal(xr["UnitPrice"]);
                        itemxr.UnitCode = xr["UnitCode"].ToString();
                        itemxr.Amt = Convert.ToDecimal(xr["Amt"]);
                        itemxr.CmpId = xr["CmpId"].ToString();
                        itemxr.ReplaceStatus = Convert.ToInt32(xr["ReplaceStatus"]);
                        itemxr.Vendor = "";
                        itemxr.VendorName = "";
                        itemxr.Remark = xr["Remark"].ToString();
                        itemxr.OutofstockStatus = Convert.ToInt32(xr["OutofstockStatus"]);
                        itemxr.ReplaceProdCode = xr["ReplaceProdCode"].ToString();
                        itemxr.StatePriceReq = Convert.ToInt32(xr["StatePriceReq"]);
                        itemxr.StateUpdatePrice = Convert.ToInt32(xr["StateUpdatePrice"]);

                        itemxr.bomitemPrice = new List<SalesBom_Price_Item>();

                        foreach (
                            DataRow ixr in dtItemPrice.Select(
                                "BomNo='"
                                    + bom.BomNo
                                    + "' and RevNo="
                                    + bom.RevNo
                                    + " and CmpId='"
                                    + bom.CmpId
                                    + "' and ProdCode='"
                                    + itemxr.ProdCode
                                    + "' and Seq="
                                    + itemxr.Seq
                            )
                        )
                        {
                            var itemprice = new SalesBom_Price_Item();

                            itemprice.BomNo = bom.BomNo;
                            itemprice.UpdUser = ixr["UpdUser"].ToString();
                            itemprice.RevNo = bom.RevNo;
                            itemprice.Seq = Convert.ToInt32(ixr["Seq"]);
                            itemprice.ProdCode = ixr["ProdCode"].ToString();
                            itemprice.SupplierCode = ixr["SupplierCode"].ToString();
                            itemprice.SupplierName = ixr["SupplierName"].ToString();
                            itemprice.DeliveryDate = DateTime.Parse(ixr["DeliveryDate"].ToString());
                            itemprice.Qty = Convert.ToDecimal(ixr["Qty"]);
                            itemprice.QtyBal = Convert.ToDecimal(ixr["QtyBal"]);
                            itemprice.UnitPrice = Convert.ToDecimal(ixr["UnitPrice"]);
                            itemprice.UnitCode = ixr["UnitCode"].ToString();
                            itemprice.Amt = Convert.ToDecimal(ixr["Amt"]);
                            itemprice.CmpId = ixr["CmpId"].ToString();
                            itemprice.Remark = ixr["Remark"].ToString();
                            itemprice.PriceSeq = Convert.ToInt32(ixr["PriceSeq"]);
                            itemprice.StateDelete = Convert.ToInt32(ixr["StateDelete"]);
                            itemprice.StateSelect = Convert.ToInt32(ixr["StateSelect"]);

                            itemxr.bomitemPrice.Add(itemprice);
                        }

                        item.bomitemReplace.Add(itemxr);
                    } */

                    /*  end item replace */

                    bom.items.Add(item);
                }

                bomList.Add(bom);
            }

            return Ok(bomList);
        }

        [HttpGet("[action]")]
        public IActionResult getsaleBomByDocNo(
            [FromQuery] string id,
            [FromQuery] string user,
            [FromQuery] string docno
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtItemPrice = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.sp_getSaleBom_ByDocNo @CmpId='"
                + id
                + "' , @user='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.sp_getSaleBomItem_ByDocNo @CmpId='"
                + id
                + "' , @user='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.sp_getSaleBomItem_Price_ByDocNo @CmpId='"
                + id
                + "' , @user='"
                + user
                + "' , @DocNo='"
                + docno
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
                    item.VendorCode = "";
                    item.VendorName = "";
                    item.Remark = x["Remark"].ToString();
                    item.OutofstockStatus = Convert.ToInt32(x["OutofstockStatus"]);
                    item.ReplaceProdCode = x["ReplaceProdCode"].ToString();
                    item.StatePriceReq = Convert.ToInt32(x["StatePriceReq"]);
                    item.StateUpdatePrice = Convert.ToInt32(x["StateUpdatePrice"]);
                    item.SeqSort = Convert.ToInt32(x["SeqSort"]);

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
                        itemprice.ContactName = i["ContactName"].ToString();

                        item.bomitemPrice.Add(itemprice);
                    }

                    /* start bom item replace */
                    /*  item.bomitemReplace = new List<SalesBom_Detail>();
 
                     foreach (
                         DataRow xr in dtItem.Select(
                             "BomNo='"
                                 + bom.BomNo
                                 + "' and RevNo="
                                 + bom.RevNo
                                 + " and CmpId='"
                                 + bom.CmpId
                                 + "' and ReplaceProdCode<>''"
                                 + "  and ReplaceProdCode='"
                                 + item.ProdCode
                                 + "'"
                         )
                     )
                     {
                         var itemxr = new SalesBom_Detail();
                         itemxr.BomNo = bom.BomNo;
                         itemxr.UpdUser = xr["UpdUser"].ToString();
                         itemxr.RevNo = bom.RevNo;
                         itemxr.Seq = Convert.ToInt32(xr["Seq"]);
                         itemxr.ProdCode = xr["ProdCode"].ToString();
                         itemxr.ProdDescription = xr["ProdDescription"].ToString();
                         itemxr.Qty = Convert.ToDecimal(xr["Qty"]);
                         itemxr.UnitPrice = Convert.ToDecimal(xr["UnitPrice"]);
                         itemxr.UnitCode = xr["UnitCode"].ToString();
                         itemxr.Amt = Convert.ToDecimal(xr["Amt"]);
                         itemxr.CmpId = xr["CmpId"].ToString();
                         itemxr.ReplaceStatus = Convert.ToInt32(xr["ReplaceStatus"]);
                         itemxr.Vendor = "";
                         itemxr.VendorName = "";
                         itemxr.Remark = xr["Remark"].ToString();
                         itemxr.OutofstockStatus = Convert.ToInt32(xr["OutofstockStatus"]);
                         itemxr.ReplaceProdCode = xr["ReplaceProdCode"].ToString();
                         itemxr.StatePriceReq = Convert.ToInt32(xr["StatePriceReq"]);
                         itemxr.StateUpdatePrice = Convert.ToInt32(xr["StateUpdatePrice"]);
 
                         itemxr.bomitemPrice = new List<SalesBom_Price_Item>();
 
                         foreach (
                             DataRow ixr in dtItemPrice.Select(
                                 "BomNo='"
                                     + bom.BomNo
                                     + "' and RevNo="
                                     + bom.RevNo
                                     + " and CmpId='"
                                     + bom.CmpId
                                     + "' and ProdCode='"
                                     + itemxr.ProdCode
                                     + "' and Seq="
                                     + itemxr.Seq
                             )
                         )
                         {
                             var itemprice = new SalesBom_Price_Item();
 
                             itemprice.BomNo = bom.BomNo;
                             itemprice.UpdUser = ixr["UpdUser"].ToString();
                             itemprice.RevNo = bom.RevNo;
                             itemprice.Seq = Convert.ToInt32(ixr["Seq"]);
                             itemprice.ProdCode = ixr["ProdCode"].ToString();
                             itemprice.SupplierCode = ixr["SupplierCode"].ToString();
                             itemprice.SupplierName = ixr["SupplierName"].ToString();
                             itemprice.DeliveryDate = DateTime.Parse(ixr["DeliveryDate"].ToString());
                             itemprice.Qty = Convert.ToDecimal(ixr["Qty"]);
                             itemprice.QtyBal = Convert.ToDecimal(ixr["QtyBal"]);
                             itemprice.UnitPrice = Convert.ToDecimal(ixr["UnitPrice"]);
                             itemprice.UnitCode = ixr["UnitCode"].ToString();
                             itemprice.Amt = Convert.ToDecimal(ixr["Amt"]);
                             itemprice.CmpId = ixr["CmpId"].ToString();
                             itemprice.Remark = ixr["Remark"].ToString();
                             itemprice.PriceSeq = Convert.ToInt32(ixr["PriceSeq"]);
                             itemprice.StateDelete = Convert.ToInt32(ixr["StateDelete"]);
                             itemprice.StateSelect = Convert.ToInt32(ixr["StateSelect"]);
 
                             itemxr.bomitemPrice.Add(itemprice);
                         }
 
                         item.bomitemReplace.Add(itemxr);
                     } */

                    /* end bomitemreplace */

                    bom.items.Add(item);
                }

                bomList.Add(bom);
            }

            return Ok(bomList);
        }

        [HttpGet]
        [Route("salesbomRev")]
        public IActionResult salesbomGetR(
            [FromQuery] string cmpid,
            [FromQuery] string bomno,
            [FromQuery] int RevNo,
            [FromQuery] string user
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.sp_getSaleBom_Rev  @BomNo='"
                + bomno
                + "' , @Rev="
                + RevNo
                + " ,@CmpId='"
                + cmpid
                + "' , @user='"
                + user
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("salesbomD")]
        public IActionResult salesbomDGet(
            [FromQuery] string cmpid,
            [FromQuery] string bomno,
            [FromQuery] int RevNo
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.sp_getSaleBom_D @BomNo='"
                + bomno
                + "' , @Rev="
                + RevNo
                + " ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("salesbomF")]
        public IActionResult salesbomFGet(
            [FromQuery] string cmpid,
            [FromQuery] string bomno,
            [FromQuery] int RevNo
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.sp_getSaleBom_F @BomNo='"
                + bomno
                + "' , @Rev="
                + RevNo
                + " ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }

        [HttpGet]
        [Route("salesbomA")]
        public IActionResult salesbomAGet(
            [FromQuery] string cmpid,
            [FromQuery] string bomno,
            [FromQuery] int RevNo
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.sp_getSaleBom_A @BomNo='"
                + bomno
                + "' , @Rev="
                + RevNo
                + " ,@CmpId='"
                + cmpid
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpPost]
        [Route("salesbom")]
        public IActionResult Post(SalesBom salebom)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBom";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@RevNo =" + salebom.RevNo;
                _cmd += " ,@BomBy  ='" + salebom.BomBy + "'";
                _cmd += " ,@SaleName  ='" + salebom.SaleName + "'";
                _cmd += " ,@CustomerName  ='" + salebom.CustomerName + "'";
                _cmd += " ,@CustomerContactName  ='" + salebom.CustomerContactName + "'";
                _cmd += " ,@CustomerContactPhone  ='" + salebom.CustomerContactPhone + "'";
                _cmd += " ,@CustomerContactEmail  ='" + salebom.CustomerContactEmail + "'";
                _cmd += " ,@ProjectName  ='" + salebom.ProjectName + "'";
                _cmd += " ,@ProjectStatus =" + salebom.ProjectStatus;
                _cmd += " ,@Remark ='" + salebom.Remark + "'";
                _cmd += " ,@CmpId ='" + salebom.CmpId + "'";
                _cmd += " ,@BomState ='" + salebom.BomState + "'";
                _cmd += " ,@BomDate ='" + salebom.BomDate.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += " ,@TicketId ='" + salebom.TicketId + "'";

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

        [HttpPost]
        [Route("setBomCopy")]
        public IActionResult setBomCopy(SalesBomCopy salebom)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBomCopy";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@BomNoNew  ='" + salebom.BomNoNew + "'";
                _cmd += " ,@RevNo =" + salebom.RevNo;
                _cmd += " ,@CustomerName  ='" + salebom.CustomerName + "'";
                _cmd += " ,@CustomerContactName  ='" + salebom.CustomerContactName + "'";
                _cmd += " ,@CustomerContactPhone  ='" + salebom.CustomerContactPhone + "'";
                _cmd += " ,@CustomerContactEmail  ='" + salebom.CustomerContactEmail + "'";
                _cmd += " ,@CmpId ='" + salebom.CmpId + "'";
                _cmd += " ,@TicketId ='" + salebom.TicketId + "'";

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
                return Ok(msgretrun);
            }
        }

        [HttpPost]
        [Route("salesbomD")]
        public IActionResult postsalesbomD(List<SalesBom_Detail> salebomD)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd;


                int il = 0;
                for (int i = 0; i < salebomD.Count; i++)
                {
                    il++;
                    _cmd = "exec  dbo.sp_SetSalesBom_Detail";

                    _cmd += "  @UpdUser  ='" + salebomD[i].UpdUser + "'";
                    _cmd += ",@BomNo  ='" + salebomD[i].BomNo + "'";
                    _cmd += ",@RevNo =" + salebomD[i].RevNo;
                    _cmd += ",@Seq =" + salebomD[i].Seq;
                    _cmd += ",@ProdCode  ='" + salebomD[i].ProdCode + "'";
                    _cmd += ",@ProdDescription  ='" + salebomD[i].ProdDescription + "'";
                    _cmd += ",@Qty =" + salebomD[i].Qty;
                    _cmd += ",@UnitCode  ='" + salebomD[i].UnitCode + "'";
                    _cmd += ",@UnitPrice =" + salebomD[i].UnitPrice;
                    _cmd += ",@Amt =" + salebomD[i].Amt;
                    _cmd += ",@ReplaceStatus =" + salebomD[i].ReplaceStatus;
                    _cmd += ",@Remark  ='" + salebomD[i].Remark + "'";
                    _cmd += ",@CmpId  ='" + salebomD[i].CmpId + "'";
                    _cmd += ",@OutofstockStatus =" + salebomD[i].OutofstockStatus;
                    _cmd += ",@ReplaceProdCode  ='" + salebomD[i].ReplaceProdCode + "'";
                    _cmd += ",@SeqSort =" + il;

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    }
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult updateBomPrice([FromBody] SalesBom_Price_Version item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.sp_SetSalesBom_Detail_UpdatePrice";

                _cmd += "  @UpdUser  ='" + item.UpdUser + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@Seq =" + item.Seq;
                _cmd += ",@ProdCode  ='" + item.ProdCode + "'";
                _cmd += ",@SupplierCode  ='" + item.SupplierCode + "'";
                _cmd += ",@Qty =" + item.Qty;
                _cmd += ",@UnitCode  ='" + item.UnitCode + "'";
                _cmd += ",@UnitPrice =" + item.UnitPrice;
                _cmd += ",@Amt =" + item.Amt;
                _cmd += ",@Remark  ='" + item.Remark + "'";
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd +=
                    ",@DeliveryDate  ='"
                    + item.DeliveryDate.ToString("yyyy-MM-dd HH:mm", thaiCulture)
                    + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
                ;

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
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult deleteBomPrice([FromBody] SalesBom_Price_Item item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.sp_SetSalesBom_Detail_DeletePrice";

                _cmd += "  @UpdUser  ='" + item.UpdUser + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@Seq =" + item.Seq;
                _cmd += ",@ProdCode  ='" + item.ProdCode + "'";
                _cmd += ",@PriceSeq =" + item.PriceSeq;
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd += ",@VenderCode='" + item.SupplierCode + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
                ;

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
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult selectBomPrice([FromBody] SalesBom_Price_Item item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.sp_SetSalesBom_Detail_SelectPrice";

                _cmd += "  @UpdUser  ='" + item.UpdUser + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@Seq =" + item.Seq;
                _cmd += ",@ProdCode  ='" + item.ProdCode + "'";
                _cmd += ",@PriceSeq =" + item.PriceSeq;
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd += ",@VenderCode='" + item.SupplierCode + "'";
                _cmd += ",@StateSelect=" + item.StateSelect;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
                ;

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
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult updateBomPriceDetail([FromBody] SalesBom_Price_Item item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.[sp_SetSalesBom_Detail_ReqPrice]";

                _cmd += "  @UpdUser  ='" + item.UpdUser + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@Seq =" + item.Seq;
                _cmd += ",@ProdCode  ='" + item.ProdCode + "'";
                _cmd += ",@PriceSeq =" + item.PriceSeq;
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd += ",@VenderCode='" + item.SupplierCode + "'";
                _cmd += ",@Qty =" + item.Qty;
                _cmd += ",@QtyBal =" + item.QtyBal;
                _cmd += ",@Amt =" + item.Amt;
                _cmd += ",@UnitPrice =" + item.UnitPrice;
                _cmd +=
                    " ,@DeliveryDate ='"
                    + item.DeliveryDate?.ToString("yyyy-MM-dd", thaiCulture)
                    + "'";
                _cmd +=
                    " ,@BalCheckDate ='"
                    + item.BalCheckDate?.ToString("yyyy-MM-dd", thaiCulture)
                    + "'";

                _cmd += ",@UnitCode='" + item.UnitCode + "'";
                _cmd += ",@Remark='" + item.Remark + "'";
                _cmd += ",@Contact='" + item.ContactName + "'";


                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
                ;

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost]
        [Route("salesbomapprove")]
        public IActionResult BomApp(
            [FromQuery] string cmpid,
            [FromQuery] string DocNo,
            [FromQuery] int RevNo,
            [FromQuery] string user
        )
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec dbo.sp_SetSalesBomApp @CmpId='"
                    + cmpid
                    + "' , @DocNo='"
                    + DocNo
                    + "' , @RevNo ="
                    + RevNo
                    + ",@User='"
                    + user
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
        public IActionResult setBomtoQuo([FromBody] SaleBomToQuo item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.setBomtoQuo";
                _cmd += "  @User  ='" + item.User + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@CustomerName  ='" + item.CustomerName + "'";
                _cmd += ",@ContactName ='" + item.ContactName + "'";
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd += ",@ContactMail='" + item.ContactMail + "'";
                _cmd += ",@ContactPhone='" + item.ContactPhone + "'";
                _cmd += ",@QuotationNo='" + item.QuotationNo + "'";
                _cmd += " , @QuoRevNo=" + item.QuoRevNo + "";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }

                for (int i = 0; i < item.items.Count; i++)
                {
                    _cmd = "exec  dbo.setBomtoQuo_Item";

                    _cmd += "  @User  ='" + item.User + "'";
                    _cmd += ",@BomNo  ='" + item.BomNo + "'";
                    _cmd += ",@RevNo =" + item.RevNo;
                    _cmd += ",@QuotationNo='" + item.QuotationNo + "'";
                    _cmd += ",@CmpId  ='" + item.CmpId + "'";
                    _cmd += " ,@Seq=" + item.items[i].Seq;
                    _cmd += ", @ProdCode='" + item.items[i].ProdCode + "'";
                    _cmd += " ,@QuoRevNo=" + item.QuoRevNo + "";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    }
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }

        [HttpPost]
        [Route("salesbomaction")]
        public void PostbomAction(List<SalesBom_Action> salebomA)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                if (salebomA.Count > 0)
                {
                    _cmd =
                        "Delete From dbo.SalesBom_Action where 	WHERE BomNo = '"
                        + salebomA[0].BomNo
                        + "' AND Rev = '"
                        + salebomA[0].Rev
                        + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomA.Count; i++)
                {
                    _cmd = "exec  dbo.sp_SetSalesBom_Action";
                    _cmd += " @UpdUser  ='" + salebomA[i].UpdUser + "'";
                    _cmd += ",@BomNo  ='" + salebomA[i].BomNo + "'";
                    _cmd += ",@Rev =" + salebomA[i].Rev;
                    _cmd += ",@Seq =" + salebomA[i].Seq;
                    _cmd += ",@DescActions  ='" + salebomA[i].DescActions + "'";
                    _cmd += ",@DateActions ='" + salebomA[i].DateActions + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
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

        [HttpPost]
        [Route("salesbomfile")]
        public void PostbomFile(List<SalesBom_File> salebomF)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                if (salebomF.Count > 0)
                {
                    _cmd =
                        "Delete From dbo.SalesBom_File where 	WHERE BomNo = '"
                        + salebomF[0].BomNo
                        + "' AND Rev = '"
                        + salebomF[0].Rev
                        + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomF.Count; i++)
                {
                    _cmd = "exec  dbo.sp_SetSalesBom_File";
                    _cmd += " @UpdUser  ='" + salebomF[i].UpdUser + "'";
                    _cmd += ",@BomNo  ='" + salebomF[i].BomNo + "'";
                    _cmd += ",@Rev =" + salebomF[i].Rev;
                    _cmd += ",@Seq =" + salebomF[i].Seq;
                    _cmd += ",@FileName  ='" + salebomF[i].FileName + "'";
                    _cmd += ",@FileType ='" + salebomF[i].FileType + "'";
                    _cmd += ",@FlieSize ='" + salebomF[i].FlieSize + "'";
                    _cmd += ",@Remark  ='" + salebomF[i].Remark + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
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

        [HttpPost]
        [Route("[action]")]
        public IActionResult setBomSendApprove(SalesBom salebom)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBom_SendApprove";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@RevNo =" + salebom.RevNo;
                _cmd += " ,@CmpId ='" + salebom.CmpId + "'";
                _cmd += " ,@TicketId ='" + salebom.TicketId + "'";
                _cmd += " ,@UserApprove ='" + salebom.UserApproveTo + "'";

                 System.Data.DataTable dt = DB.DBConn.GetDataTable(_cmd);
                if (dt.Rows.Count > 0)
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                   return Ok(new { approvedoc = dt.Rows[0][0] });
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

        [HttpPost]
        [Route("[action]")]
        public IActionResult setBomApprove(SalesBom salebom)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBom_Approve";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@RevNo =" + salebom.RevNo;
                _cmd += " ,@CmpId ='" + salebom.CmpId + "'";
                _cmd += " ,@TicketId ='" + salebom.TicketId + "'";

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

        [HttpPost]
        [Route("[action]")]
        public IActionResult setBomItemOutofStock(SalesBom_Detail salebomD)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.[sp_SetSalesBom_Detail_Outofstock]";

                _cmd += "  @UpdUser  ='" + salebomD.UpdUser + "'";
                _cmd += ",@BomNo  ='" + salebomD.BomNo + "'";
                _cmd += ",@RevNo =" + salebomD.RevNo;
                _cmd += ",@Seq =" + salebomD.Seq;
                _cmd += ",@ProdCode  ='" + salebomD.ProdCode + "'";
                _cmd += ",@OutofstockStatus =" + salebomD.OutofstockStatus;
                _cmd += ",@CmpId  ='" + salebomD.CmpId + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("[action]")]
        public IActionResult delBomItem(
            [FromQuery] string bomno,
            [FromQuery] int seq,
            [FromQuery] string cmpid,
            [FromQuery] int revno,
            [FromQuery] string prodcode
        )
        {
            string _cmd = "";
            _cmd = "exec dbo.[sp_SetSalesBom_Detail_Del] ";
            _cmd += " @BomNo='" + bomno + "'";
            _cmd += " , @RevNo=" + revno;
            _cmd += " , @CmpId='" + cmpid + "'";
            _cmd += " , @ProdCode='" + prodcode + "'";
            _cmd += " , @Seq=" + seq;
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }
    }
}
