using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http; 

namespace NohWebApi.Controllers
{
 
    public class InfoNameController : ApiController
    {
        // GET: api/InfoName
        public IEnumerable<string> Get()
        {
           

            return new string[] { "value1", "value2" };
        }

        public IHttpActionResult Get(int id )
        { 
            //DataTable dt = new System.Data.DataTable();
            //string _cmd;
            //_cmd = "exec dbo.[getPermissionlist] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            //dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok("");

        }


        // GET: api/InfoName/5
      
        // POST: api/InfoName
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/InfoName/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/InfoName/5
        public void Delete(int id)
        {
        }
    }
}
