using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalemanController : ControllerBase
    {
        // GET: api/Saleman
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        // GET: api/Saleman/5
        [HttpGet("{CmpId}/{user}")]
        public ActionResult<DataTable> Get(int CmpId, string user)
        {
            string _cmd = $"exec dbo.[getSalemantrackAll] @User='{user}',@CmpId={CmpId}";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        { 
            try
            {
                string _cmd = $"Delete from mdb.SalemanTrack where SalemanTrackNo='{id}'";
                _cmd += $" Delete from mdb.Saleman_Asign where SalemanTrackNo='{id}'";
                _cmd += $" Delete from mdb.SalemanTask where SalemanTrackNo='{id}'";
                DB.DBConn.ExecuteOnly(_cmd);
                return Ok(new { Msg = "ลบข้อมูลสำเร็จ" });
            }
            catch
            {
                return StatusCode(500, "เกิดข้อผิดพลาด");
            }
        }
    }
}
