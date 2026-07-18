using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalemanTaskController : ControllerBase
    {
        [HttpGet] 
        public ActionResult<DataTable> Get(int CmpId, string user)
        {
            try
            {
                string _cmd = $"exec dbo.[getSalemantrackTaskAll] @User='{user}', @CmpId={CmpId}";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No tasks found for the given user and company ID." });
                }

                return Ok(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching data.", Details = ex.Message });
            }
        }
    }
}
