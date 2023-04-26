using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace coreapi.Controllers
{
    public class QuatationController : ApiController
    {

        QuatationDetail[] quatations;
        public void getall()
        {
            string _cmd;
            _cmd = "exec dbo.getQuatationDetail @QuatationNo=''";
              DB.DBConn.GetDataTable(_cmd);

        }

        public IEnumerable<QuatationDetail> GetAllQuatationDetail()
        {
            return quatations;
        }

        public IHttpActionResult GetQuatation(string  id)
        {
            string _cmd;
            _cmd = "exec dbo.getQuatationDetail @QuatationNo=''";
          DataTable datatable =   DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }
    }
}
