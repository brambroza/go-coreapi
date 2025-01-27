using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataForDashServiceController : ControllerBase
    {
        [HttpGet("DashService")]
        public ActionResult<string> Get(string CmpId, string OfDate)
        {
            try
            {
                string _cmd = $"exec dbo.getTop5Problem @CmpId={Convert.ToInt16(CmpId)}, @DateOfMonth='{OfDate}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                string jsonString = JsonConvert.SerializeObject(datatable);
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        } 

        [HttpGet("DashServiceActionPopular")]
        public ActionResult<string> GetActionPop(string CmpId, string OfDate)
        {
            try
            {
                string _cmd = $"exec dbo.getTop5ProblemActions @CmpId={Convert.ToInt16(CmpId)}, @DateOfMonth='{OfDate}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                string jsonString = JsonConvert.SerializeObject(datatable);
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("DashProblemChartPieSeries")]
        public ActionResult<string> DashProblemChartPie(string CmpId, string OfDate)
        {
            try
            {
                string _cmd = $"exec dbo.dashboardProblemSeries @CmpId={Convert.ToInt16(CmpId)}, @DateOfMonth='{OfDate}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                string jsonString = JsonConvert.SerializeObject(datatable);
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("DashProblemChartPielabels")]
        public ActionResult<string> DashProblemChartPielabels(string CmpId, string OfDate)
        {
            try
            {
                string _cmd = $"exec dbo.dashboardProblemlabels @CmpId={Convert.ToInt16(CmpId)}, @DateOfMonth='{OfDate}'";
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                string jsonString = JsonConvert.SerializeObject(datatable);
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpPost("DashService")]
        public ActionResult Post([FromBody] string value)
        {
            return Ok(new { Message = "Post method called.", Value = value });
        }

        [HttpPut("DashService/{id}")]
        public ActionResult Put(int id, [FromBody] string value)
        {
            return Ok(new { Message = $"Put method called for ID {id}.", Value = value });
        }

        [HttpDelete("DashService/{id}")]
        public ActionResult Delete(int id)
        {
            return Ok(new { Message = $"Delete method called for ID {id}." });
        }
    }
}
