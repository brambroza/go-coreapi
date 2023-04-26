using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 
using coreapi.Models;


namespace coreapi.Controllers
{
 
    public class ProblemReceiveController : ApiController
    {
        // GET: api/ProblemReceive
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ProblemReceive/5
        public IHttpActionResult Get(int cmpid , string username)
        {
       
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblem] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);

        }

        // POST: api/ProblemReceive
        public void Post(STProblem pr)
        {
            string _cmd = "";
            _cmd = "exec  dbo.STProblem_Trans";
            _cmd += "  @UpdUser  ='" + pr.UpdUser + "'";
            _cmd += ",@ProblemId =" + pr.ProblemId;
            _cmd += ",@ReceiveDate ='" + pr.ReceiveDate + "'";
            _cmd += ",@CustCode  ='" + pr.CustCode + "'";
            _cmd += ",@RequestBy  ='" + pr.RequestBy + "'";
            _cmd += ",@ProblemDetails  ='" + Tool.Tool.validateStr(pr.ProblemDetails) + "'";
            _cmd += ",@ProblemType  ='" + pr.ProblemType + "'";
            _cmd += ",@ReceiveTime  ='" + pr.ReceiveTime + "'";
            _cmd += " ,@CustBranchName='" + pr.CustBranchName + "'";


            DB.DBConn.ExecuteOnly(_cmd);


        }
        // PUT: api/ProblemReceive/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProblemReceive/5
        public void Delete(int id)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STProblem where  ProblemId='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
