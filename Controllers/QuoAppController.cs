using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using goalongapi.Models;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuoAppController : ControllerBase
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
                    return NotFound(new { Message = "No records found for the provided ID." });
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
                _cmd += $"@Description='{ma.Description}',";
                _cmd += $"@PurchaseNo='{ma.PurchaseNo}',";
                _cmd += $"@ReferCode='{ma.ReferCode}',";
                _cmd += $"@QuotationNo='{ma.QuotationNo}',";
                _cmd += $"@StateActive='{ma.StateActive}',";
                _cmd += $"@CmpId={ma.CmpId}";

                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while saving data.", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] string value)
        {
            return Ok(new { Message = $"PUT method called for ID {id}.", Value = value });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return Ok(new { Message = $"DELETE method called for ID {id}." });
        }
    }
}
