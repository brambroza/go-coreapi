using Microsoft.AspNetCore.Mvc;
using System.Data;
using goalongapi.Models;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActionServiceEmpController : ControllerBase
    {
        [HttpGet("{cmpid}/{username}")]
        public ActionResult<DataTable> Get(int cmpid, string username)
        {
            try
            {
                string _cmd = $"exec dbo.[getProblemActions_emp] @CmpId={cmpid}, @User='{username}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count > 0)
                {
                    return Ok(dt);
                }
                else
                {
                    return NotFound(new { Message = "No records found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
 