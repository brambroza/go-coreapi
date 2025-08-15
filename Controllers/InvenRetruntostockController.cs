using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvenReturntostockController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getInventReturnStock( [FromQuery] string CmpId, [FromQuery] string user)
        {
           string _cmd;
            _cmd = "exec dbo.[Inven_getRctAll] @CmpId='" +   CmpId  + "' , @User='" + user + "'";
           DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getTransAll] @CmpId='" +  CmpId  + "' , @User='" + user + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<ReturnToStock> receives = new List<ReturnToStock>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new ReturnToStock()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    ReturnToStockNo = r["ReturnToStockNo"].ToString(),
                    ReturnToStockDate = r["ReturnToStockDate"].ToString(),
                    ReturnToStockBy = r["ReturnToStockBy"].ToString(),
                    IssueNo = r["IssueNo"].ToString(), 
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),
                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    IssueDate = r["IssueDate"].ToString(), 
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),
                    
                };

                receive.items = new List<InvenTransModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["ReturnToStockNo"].ToString()
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
            var response = new { receives };
            return Ok(response);

           
        }

        [HttpPost("InvenRtc")]
        public ActionResult<MsgReturn> Post([FromBody] ReturnToStock rc)
        {
            var msgretrun = new MsgReturn();

            try
            {
                string _cmd = "exec dbo.Inven_setReturnToStockTrans";
                _cmd += $" @UpdUser='{rc.UpdUser}',";
                _cmd += $"@ReturnToStockNo='{rc.ReturnToStockNo}',";
                _cmd += $"@ReturnToStockDate='{rc.ReturnToStockDate}',";
                _cmd += $"@ReturnToStockBy='{rc.ReturnToStockBy}',";
                _cmd += $"@IssueNo='{rc.IssueNo}',";
                _cmd += $"@CmpId={rc.CmpId},";
                _cmd += $"@Remark='{rc.Remark}',";
                _cmd += $"@SysWHId={rc.SysWHId},";
                _cmd += $"@SysWHLocId={rc.SysWHLocId}";

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
            catch (Exception ex)
            {
                msgretrun.ReturnCode = "500";
                msgretrun.Msg = "An error occurred.";
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpDelete("InvenRtc/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                string _cmd = $"Delete from Inven.ReturnToStock where [ReturnToStockNo]='{id}'";
                DB.DBConn.ExecuteOnly(_cmd);
                return Ok(new { Message = "Delete successful." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
