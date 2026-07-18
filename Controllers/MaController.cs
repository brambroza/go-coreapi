using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        [HttpGet("{id}")]
        public ActionResult<DataTable> Get(int id)
        {
            try
            {
                string _cmd = $"exec dbo.getMA_All @CmpId={id}";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No data found for the provided ID." });
                }

                return Ok(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody] Ma ma)
        {
            try
            {
                string _cmd = "exec dbo.MAServiceTrans";
                _cmd += $" @UpdUser='{ma.UpdUser}',";
                _cmd += $"@MANo='{ma.MANo}',";
                _cmd += $"@CustCode='{ma.CustCode}',";
                _cmd += $"@Description='{Tool.Tool.validateStr(ma.Description)}',";
                _cmd += $"@PurchaseNo='{ma.PurchaseNo}',";
                _cmd += $"@ReferCode='{ma.ReferCode}',";
                _cmd += $"@QuotationNo='{ma.QuotationNo}',";
                _cmd += $"@StateActive='{ma.StateActive}',";
                _cmd += $"@CmpId={ma.CmpId}";

                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "MA record created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                string _cmd = $"delete from dbo.MAService where MANo='{id}'";
                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "MA record deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
