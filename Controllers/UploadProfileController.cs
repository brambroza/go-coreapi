using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace NohWebApi.Controllers
{
    
    public class UploadProfileController : ApiController
    {
        // GET: api/UploadProfile
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/UploadProfile/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/UploadProfile
        public void Post([FromBody]string value)
        {
            try
            {


            }
            catch
            {

            }
        }

        // PUT: api/UploadProfile/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/UploadProfile/5
        public void Delete(int id)
        {
        }
    }
}
