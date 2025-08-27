using goalongapi.Models;
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

namespace goalongapi.Controllers
{

    [ApiController]
    [Authorize]


    public class InvenDeliveryController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getDeliveryNote([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getDeliveryNoteAll @CmpId='" +  CmpId  + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getDeliveryNoteItemAll] @CmpId='" +  CmpId  + "' , @User='" + user + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            

            List<DeliveryNote> receives = new List<DeliveryNote>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new DeliveryNote()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    DeliveryNodeNo = r["DeliveryNodeNo"].ToString(),
                    DeliveryNodeDate = r["DeliveryNodeDate"].ToString(),
                    DeliveryNodeBy = r["DeliveryNodeBy"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    ShipDate = r["ShipDate"].ToString(),
                    CustomerCode = r["CustomerCode"].ToString(), 
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),
                    AppDate = r["AppDate"].ToString(),
                    AppTime = r["AppTime"].ToString(), 
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),
                    SaleOrderNo = r["SaleOrderNo"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    
                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                   

                };

                receive.Items = new List<DeliveryNoteItem>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DeliveryNodeNo ='"
                             + r["DeliveryNodeNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new DeliveryNoteItem();
                    item.DeliveryNodeNo = d["DeliveryNodeNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.TransDate = d["TransDate"].ToString();
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);
                    item.BarcodeNo = d["BarcodeNo"].ToString();

                    item.ProductCode = d["ProductCode"].ToString();
                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.UnitCode = d["UnitCode"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]); 

                    item.TransType = d["TransType"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                  
                    
  
                }

                receives.Add(receive);
            }
           /*  var response = new {   receives   }; */
            return Ok(receives);
        }


        [HttpPost("[action]")]
        public void setDeliveryNote(DeliveryNote receive)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


           

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setDeliveryNote";
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@DeliveryNodeNo  ='" + receive.DeliveryNodeNo + "'";
                _cmd += ",@DeliveryNodeDate  ='" + receive.DeliveryNodeDate + "'";
                _cmd += ",@DeliveryNodeBy  ='" + receive.DeliveryNodeBy + "'";
                _cmd += ",@ShipDate  ='" + receive.ShipDate + "'";
                _cmd += ",@CustomerCode  ='" + receive.CustomerCode + "'";
                _cmd += ",@SaleOrderNo  ='" + receive.SaleOrderNo + "'";
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@Remark  ='" + receive.Remark + "'";
                _cmd += ",@WHId =" + receive.SysWHId;
                _cmd += ",@WHLocId =" + receive.SysWHLocId;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return;
                }


                for (int i = 0; i < receive.Items.Count; i++)
                {

                    _cmd = "exec  dbo.Inven_setDeliveryNoteItem";
                    _cmd += " @UpdUser  ='" + receive.Items[i].UpdUser + "'";
                    _cmd += ",@Seq =" + receive.Items[i].Seq;
                    _cmd += ",@DeliveryNodeNo  ='" + receive.Items[i].DeliveryNodeNo + "'";
                    _cmd += ",@TransDate ='" + receive.Items[i].TransDate + "'"; ;
                    _cmd += ",@SysWHId =" + receive.Items[i].SysWHId;
                    _cmd += ",@SysWHLocId =" + receive.Items[i].SysWHLocId;
                    _cmd += ",@BarcodeNo  ='" + receive.Items[i].BarcodeNo + "'";
                    _cmd += ",@ProductCode  ='" + receive.Items[i].ProductCode + "'";
                    _cmd += ",@UnitPrice =" + receive.Items[i].UnitPrice;
                    _cmd += ",@Qty =" + receive.Items[i].Qty;
                    _cmd += ",@UnitCode ='" + receive.Items[i].UnitCode + "'"; ;  
                    _cmd += ", @CmpId='" + receive.Items[i].CmpId + "'";
                     _cmd += ", @TransType='DN'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }

                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return;

            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return;
            }


        }
 

        [HttpDelete("[action]")]
        public void DeleteDeliveryNote(string cmpid , string docno)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Receive where ReceiveNo='" + docno + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
