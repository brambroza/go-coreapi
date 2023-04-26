using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 
namespace coreapi.Controllers
{
    
    public class UnitsController : ApiController
    {
        // GET: api/Units
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Units/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUnitMaster] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        

        // POST: api/Units
        public void Post(Units units)
        {

            string _cmd = "";
            _cmd = "exec  dbo.mUnit_Trans";
            _cmd += " @UpdUser  ='" + units.UpdUser + "'";
            _cmd += ",@UnitCode =" + units.UnitCode;
            _cmd += ",@UnitDescription  ='" + units.UnitDescription + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        // PUT: api/Units/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Units/5
        public void Delete(string id)
        {
            string _cmd = "";
            _cmd = "delete from msb.mUnit where  UnitCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
