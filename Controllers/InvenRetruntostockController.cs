using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvenRetruntostockController : ControllerBase
    {
        [HttpGet("InvenRtc")]
        public ActionResult<DataTable> Get(string CmpId, string user)
        {
            try 
            {
                string _cmd = $"exec dbo.Inven_getRctAll @CmpId={Convert.ToInt16(CmpId)}, @User='{user}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);

                if (datatable.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No records found." });
                }

                return Ok(datatable);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
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
