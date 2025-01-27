using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedurController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        [HttpGet("Calendar")]
        public ActionResult<DataTable> GetCalendar(DateTime SDate, DateTime EDate)
        {
            try
            {
                string _cmd = $"exec dbo.sp_getCalendar '{SDate:yyyy-MM-dd}', '{EDate:yyyy-MM-dd}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No data found for the specified date range." });
                }

                return Ok(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the data.", Details = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            return Ok(new { Message = "POST method called.", Value = value });
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
