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
    public class invenRtsController : ControllerBase
    { 
        [HttpGet("[action]")]
        public IActionResult getInventReturnSupl( [FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getRtsAll @CmpId='" +   CmpId  + "' , @User='" + user + "'";
           DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getTransAll] @CmpId='" +  CmpId  + "' , @User='" + user + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<ReturnToSuplModel> receives = new List<ReturnToSuplModel>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new ReturnToSuplModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    ReturnToSuplNo = r["ReturnToSuplNo"].ToString(),
                    ReturnToSuplDate = r["ReturnToSuplDate"].ToString(),
                    ReturnToSuplBy = r["ReturnToSuplBy"].ToString(),
                    PurchaseNo = r["PurChaseNo"].ToString(),

                    ReturnType = r["ReturnType"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),

                    SupplierCode = r["SupplierCode"].ToString(),
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),

                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    SupplierName = r["SupplierName"].ToString(),
                    PurchaseDate = r["PurchaseDate"].ToString(),
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),




                };

                receive.items = new List<InvenTransModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["ReturnToSuplNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new InvenTransModel();
                    item.DocNo = d["DocNo"].ToString();
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
                    item.PurchaseNo = d["PurchaseNo"].ToString();

                    item.StateReserve = Convert.ToInt32(d["StateReserve"]);

                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.BatchNo = d["BatchNo"].ToString();
                    item.Grade = d["Grade"].ToString();
                    item.DateExpire = d["DateExpire"].ToString();

                    item.StateQC = Convert.ToInt32(d["StateQC"]);

                    item.QCBy = d["QCBy"].ToString();
                    item.TransType = d["TransType"].ToString();
                    item.CmpId = d["CmpId"].ToString();


 
                    receive.items.Add(item);
                }

                receives.Add(receive);
            }
            var response = new {   receives   };
            return Ok(response);

           
        }

        
        [HttpPost("[action]")]
        public IActionResult setReturnSupl(ReturnToSuplModel rts)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReturnToSuplTrans"; 
                _cmd += " @UpdUser  ='" + rts.UpdUser + "'"; 
                _cmd += ",@ReturnToSuplNo  ='" + rts.ReturnToSuplNo + "'"; 
                _cmd += ",@ReturnToSuplDate  ='" + rts.ReturnToSuplDate + "'"; 
                _cmd += ",@ReturnToSuplBy ='" + rts.ReturnToSuplBy + "'";
                _cmd += ",@PurChaseNo  ='" + rts.PurchaseNo + "'"; 
                _cmd += ",@CmpId ='" + rts.CmpId+ "'";
                _cmd += ",@Remark  ='" + rts.Remark + "'";
                _cmd += ",@ReturnType =" + rts.ReturnType; 
                _cmd += ",@SupplierCode ='" + rts.SupplierCode + "'";
                _cmd += ",@WHId =" + rts.SysWHId; 
                _cmd += ",@WHLocId =" + rts.SysWHLocId; 
              
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
        public IActionResult setInvenRTSApprove(ReturnToSuplModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.[Inven_setReturnToSuplTrans_Approve]";
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@ReturnToSuplNo  ='" + receive.ReturnToSuplNo + "'"; 
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@StateApp  ='" + receive.StateApp + "'"; 

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


    
        [HttpDelete("[action]")]
        public void DeleteInvenRts( [FromQuery]  string id , [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.ReturnToSupl where ReturnToSuplNo='" + id + "' and cmpid='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
