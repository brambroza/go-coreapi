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
    public class QuotationController : ApiController
    {

        QuotationDetail[] Quotations;
        public void getall()
        {
            string _cmd;
            _cmd = "exec dbo.getQuotationDetail @QuotationNo=''";
              DB.DBConn.GetDataTable(_cmd);

        }

        public IEnumerable<QuotationDetail> GetAllQuotationDetail()
        {
            return Quotations;
        }

        public IHttpActionResult GetQuotation(string  id)
        {
            string _cmd;
            _cmd = "exec dbo.getQuotationDetail @QuotationNo=''";
          DataTable datatable =   DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }
    }
}
