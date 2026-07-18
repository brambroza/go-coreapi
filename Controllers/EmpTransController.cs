using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpTransController : ControllerBase
    {
        [HttpGet("TimeCard")]
        public ActionResult<string> Get(string id)
        {
            try
            {
                string _cmd = $"exec dbo.get_TimeCard @UserName='{id}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                string jsonString = JsonConvert.SerializeObject(dt);
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        } 

        [HttpPost("TimeCard")]
        public ActionResult<string> PostTimeCard([FromBody] TimeCard _timecard)
        {
            try
            {
                string _cmd = $"exec dbo.set_TimeCard @UserName='{_timecard.UserName}', @TransDate='{_timecard.TransDate}', @TransTime='{_timecard.TransTime}', @latitude='{_timecard.latitude}', @longitude='{_timecard.longitude}', @Status='{_timecard.status}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

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
