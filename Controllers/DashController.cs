using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashController : ControllerBase
    {
        [HttpGet("{CmpId}/{username}/{dtype}")]
        public ActionResult<string> Get(int CmpId, string username, int dtype)
        {
            try
            {
                DataTable dt = new DataTable();
                string _cmd = dtype switch
                {
                    0 => $"exec dbo.getsaledaily @CmpId={CmpId}, @Username='{username}'",
                    1 => $"exec dbo.getsalemonth @CmpId={CmpId}, @Username='{username}'",
                    2 => $"exec dbo.getTop10SaleProduct @CmpId={CmpId}, @Username='{username}'",
                    3 => $"exec dbo.getsaleyear @CmpId={CmpId}, @Username='{username}'",
                    _ => $"exec dbo.getsaledaily @CmpId={CmpId}, @Username='{username}'",
                };

                dt = DB.DBConn.GetDataTable(_cmd);
                string jsonString = JsonConvert.SerializeObject(dt);

                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
