using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
 
using System.Data;
using coreapi.Models;

namespace coreapi.Controllers
{
   
    public class UserPermisstionController : ApiController
    {
        // GET: api/UserPermisstion
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/UserPermisstion/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUserPermisstion] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);

        }

        // POST: api/UserPermisstion
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/UserPermisstion/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/UserPermisstion/5
        public void Delete(int id)
        {
        }
    }
}
