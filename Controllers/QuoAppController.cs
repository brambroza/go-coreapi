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
  
    public class QuoAppController : ApiController
    {
        // GET: api/QuoApp
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/QuoApp/5
        public IHttpActionResult Get(int id)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getMA_All @CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }

        // POST: api/QuoApp
        public void Post(Ma ma)
        {

            string _cmd = "";
            _cmd = "exec  dbo.MAServiceTrans";
            _cmd += " @UpdUser  ='" + ma.UpdUser + "'";
            _cmd += ",@MANo  ='" + ma.MANo + "'";
            _cmd += ",@CustCode  ='" + ma.CustCode + "'";
            _cmd += ",@Description  ='" + ma.Description + "'";
            _cmd += ",@PurchaseNo  ='" + ma.PurchaseNo + "'";
            _cmd += ",@ReferCode  ='" + ma.ReferCode + "'";
            _cmd += ",@QuotationNo  ='" + ma.QuotationNo + "'";
            _cmd += ",@StateActive  ='" + ma.StateActive + "'";
            _cmd += ",@CmpId =" + ma.CmpId;

            DB.DBConn.ExecuteOnly(_cmd);


        }

        // PUT: api/QuoApp/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/QuoApp/5
        public void Delete(int id)
        {
        }
    }
}
