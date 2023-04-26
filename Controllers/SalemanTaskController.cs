using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace coreapi.Controllers
{
    public class SalemanTaskController : ApiController
    {
        // GET: api/SalemanTask
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SalemanTask/5
        public IHttpActionResult Get(int CmpId, string user)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getSalemantrackTaskAll]  @User='" + user + "',@CmpId =" + CmpId;
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }

        // POST: api/SalemanTask
        public void Post([FromBody]string value)
        {

        }

        // PUT: api/SalemanTask/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SalemanTask/5
        public void Delete(int id)
        {
        }
    }
}
