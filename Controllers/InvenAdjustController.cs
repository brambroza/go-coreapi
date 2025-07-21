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

     [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class InvenAdjustController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getInvenAdjustList([FromQuery] string CmpId, [FromQuery] string user , [FromQuery] string type )
        {
 


            string _cmd;
            _cmd = "exec dbo.Inven_GetAdjustAll @CmpId='" + CmpId + "' , @User='" + user + "' , @Type='" + type + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getAdjustItemAll] @CmpId='" + CmpId + "' , @User='" + user + "' , @Type='" + type + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<AdjustModel> receives = new List<AdjustModel>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new AdjustModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    AdjustNo = r["AdjustNo"].ToString(),
                    AdjustDate = r["AdjustDate"].ToString(),
                    AdjustBy = r["AdjustBy"].ToString(),

                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),
                    StateSend = r["StateSend"].ToString(),
                    SendAppBy = r["SendAppBy"].ToString(),

                    AdjustType = int.Parse(r["AdjustType"].ToString()),
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),

                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    Status = r["Status"].ToString(),
                    
                    Type = r["Type"].ToString(),
                    Reason = r["Reason"].ToString(),
                    RefDocNo = r["RefDocNo"].ToString(),

                };

                receive.items = new List<AdjustItem>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["AdjustNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new AdjustItem();
                    item.DocNo = d["DocNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
               
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);
                    item.BarcodeNo = d["BarcodeNo"].ToString();

                    item.ProductCode = d["ProductCode"].ToString();
                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.UnitCode = d["UnitCode"].ToString();
                    item.QtySystem = Convert.ToDecimal(d["QtySystem"]);
                    item.QtyCounted = Convert.ToDecimal(d["QtyCounted"]);
                    item.AdjustQty = Convert.ToDecimal(d["AdjustQty"]);
                    item.QtyAfter = Convert.ToDecimal(d["QtyAfter"]);
                  

                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.BatchNo = d["BatchNo"].ToString();
                    item.Grade = d["Grade"].ToString();
                    item.DateExpire = d["DateExpire"].ToString();

                    item.StateQC = Convert.ToInt32(d["StateQC"]);

            
                    item.TransType = d["TransType"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.Imgpath = d["Imgpath"].ToString();


                    receive.items.Add(item);
                }

                receives.Add(receive);
            }
            var response = new { receives };
            return Ok(response);
            



            


        }


         [HttpPost("[action]")]
        public IActionResult setInvenAdjust(AdjustModel adjust )
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTrans";
                _cmd += " @UpdUser  ='" + adjust.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adjust.AdjustNo + "'"; 
                _cmd += ",@AdjustDate  ='" + adjust.AdjustDate + "'"; 
                _cmd += ",@AdjustBy  ='" + adjust.AdjustBy + "'";
                _cmd += ",@PurChaseNo  =''";
                _cmd += ",@CmpId ='" + adjust.CmpId+ "'"; 
                _cmd += ",@Remark  ='" + adjust.Remark + "'";
                _cmd += ",@WHId =" + adjust.SysWHId;
                _cmd += ",@WHLocId =" + adjust.SysWHLocId;
                _cmd += ", @AdjustType=" + adjust.AdjustType;
                _cmd += ",@Type ='" + adjust.Type+ "'"; 
                _cmd += ",@Reason  ='" + adjust.Reason + "'";
                _cmd += " ,@RefDocNo='" + adjust.RefDocNo + "'";
                 

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
        public void setInvenAdjustItem(List<AdjustItem> Inven)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (Inven.Count > 0)
                {
                    _cmd = "Delete From Inven.AdjustItem   where DocNo='" + Inven[0].DocNo + "'";
                    _cmd += "   and CmpId='" + Inven[0].CmpId + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < Inven.Count; i++)
                {

                    _cmd = "exec  dbo.Inven_setInvenAdjustItem";
                    _cmd += " @UpdUser  ='" + Inven[i].UpdUser + "'";
                    _cmd += ",@Seq =" + Inven[i].Seq;
                    _cmd += ",@DocNo  ='" + Inven[i].DocNo + "'"; 
                    _cmd += ",@SysWHId =" + Inven[i].SysWHId;
                    _cmd += ",@SysWHLocId =" + Inven[i].SysWHLocId;
                    _cmd += ",@BarcodeNo  ='" + Inven[i].BarcodeNo + "'";
                    _cmd += ",@ProductCode  ='" + Inven[i].ProductCode + "'";
                    _cmd += ",@UnitPrice =" + Inven[i].UnitPrice;
                    _cmd += ",@QtySystem =" + Inven[i].QtySystem;
                    _cmd += ",@QtyCounted =" + Inven[i].QtyCounted;
                    _cmd += ",@AdjustQty =" + Inven[i].AdjustQty;
                    _cmd += ",@QtyAfter =" + Inven[i].QtyAfter;
                    _cmd += ",@UnitCode ='" + Inven[i].UnitCode + "'"; ; 
                    _cmd += ",@BatchNo ='" + Inven[i].BatchNo + "'"; ;
                    _cmd += ",@Grade ='" + Inven[i].Grade + "'"; ;
                    _cmd += ",@DateExpire ='" + Inven[i].DateExpire + "'"; ;
                    _cmd += ",@Type ='" + Inven[i].TransType + "'";
                    _cmd += ", @CmpId='" + Inven[i].CmpId + "'";


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

 
       [HttpPost("[action]")]
        public IActionResult setInvenAdjustApprove(AdjustModel adj)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTrans_Approve";
                _cmd += " @UpdUser  ='" + adj.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adj.AdjustNo + "'"; 
                _cmd += ",@CmpId ='" + adj.CmpId + "'";
                _cmd += ",@StateApp  ='" + adj.StateApp + "'";
                _cmd += ",@Type='" + adj.Type + "'"; 

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
        public IActionResult setInvenAdjustSendApprove(AdjustModel adj)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTrans_SendApprove";
                _cmd += " @UpdUser  ='" + adj.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adj.AdjustNo + "'"; 
                _cmd += ",@CmpId ='" + adj.CmpId + "'";
                _cmd += ",@StateApp  ='" + adj.StateApp + "'"; 
                _cmd += ",@SendTo  ='" + adj.SendTo + "'"; 
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
  
        [HttpGet("[action]")]
        public IActionResult getCheckBarcodeNoForAdjust([FromQuery] string CmpId )
        {
            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd = "exec dbo.[getProdMaster_Onhand_All] @CmpId='" + CmpId + "' ";
                dt = DB.DBConn.GetDataTable(_cmd);



                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(dt);

                var prodlist = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var eventObj = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        string lowercaseColumnName =
                            char.ToLowerInvariant(column.ColumnName[0])
                            + column.ColumnName.Substring(1);

                        eventObj[lowercaseColumnName] = row[column];
                    }

                    prodlist.Add(eventObj);
                }


                return Ok(prodlist);



            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching products.", Details = ex.Message });
            }
        }



 
        [HttpDelete("[action]")]
        public void DeleteAdjust( [FromQuery] string id  , [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Adjust where AdjustNo='" + id + "' and CmpId='" + cmpid  + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
