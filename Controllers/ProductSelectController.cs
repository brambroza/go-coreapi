using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

 

namespace coreapi.Controllers
{
 
    public class ProductSelectController : ApiController
    {
        // GET: api/ProductSelect
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ProductSelect/5
        public IHttpActionResult Get(int id)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMaster  ";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }


        // POST: api/ProductSelect
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ProductSelect/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProductSelect/5
        public void Delete(int id)
        {
        }
    }
}
