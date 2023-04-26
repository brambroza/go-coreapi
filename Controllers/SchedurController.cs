using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{
    
    public class SchedurController : ApiController
    {
        // GET: api/Schedur
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Schedur/5
        public IHttpActionResult Get(DateTime SDate , DateTime EDate)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getCalendar '" + SDate.Year.ToString() +"-"+ SDate.Month.ToString() + "-" + SDate.Day.ToString() + "','"+ EDate.Year.ToString() + "-" + EDate.Month.ToString() + "-" + EDate.Day.ToString() + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        // POST: api/Schedur
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Schedur/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Schedur/5
        public void Delete(int id)
        {
        }
    }
}
