using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        // GET: api/Units

        // GET: api/Units/5
        [HttpGet("[action]")]
        public IActionResult GetUnitMaster([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUnitMaster] @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        // POST: api/Units
        [HttpPost("[action]")]
        public void SetUnitMaster(Units units)
        {
            string _cmd = "";
            _cmd = "exec  dbo.mUnit_Trans";
            _cmd += " @UpdUser  ='" + units.UpdUser + "'";
            _cmd += ",@UnitCode =" + units.UnitCode;
            _cmd += ",@UnitDescription  ='" + units.UnitDescription + "'";
            _cmd += ",@CmpId  ='" + units.CmpId + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        // DELETE: api/Units/5
        [HttpDelete("[action]")]
        public void DeleteUnitMaster([FromQuery] string unitcode, [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd =
                "delete from msb.mUnit where  UnitCode='"
                + unitcode
                + "' and CmpId ='"
                + cmpid
                + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
