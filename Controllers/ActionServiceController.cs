 
using Microsoft.AspNetCore.Mvc;
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
    [System.Web.Http.Route("[controller]")]
    [Authorize]
    public class ActionServiceController : ApiController
    {
         

        // GET: api/ActionService/5
        public IHttpActionResult Get(int cmpid, string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblemActions] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);

        }

     

        // POST: api/ActionService
        public void Post(STServiceActions pr)
        {
            string _cmd = "";
            _cmd = "exec  dbo.STServiceActions_Trans";
            _cmd += " @UpdUser  ='" + pr.UpdUser + "'";
            _cmd += ",@ServiceActionId =" + pr.ServiceActionId;
            _cmd += ",@ProblemId =" + pr.ProblemId;
            _cmd += ",@ServiceActionBy  ='" + pr.ServiceActionBy + "'";
            _cmd += ",@ServiceType =" + pr.ServiceType;
            _cmd += ",@ActionDetails  ='" + Tool.Tool.validateStr(pr.ActionDetails )+ "'"; 
            _cmd += ",@FinishDate  ='" + pr.FinishDate + "'";
            _cmd += ",@FinishTime  ='" + pr.FinishTime + "'";

            DB.DBConn.ExecuteOnly(_cmd);

            _cmd = "delete from dbo.STServiceActions_Emp where  ServiceActionId='" + pr.ServiceActionId + "' ";
            DB.DBConn.ExecuteOnly(_cmd);

            for (int i = 0; i < pr.emp.Count; i++)
            { 
                 
                _cmd = "exec  dbo.STServiceActions_Emp_trans";
                _cmd += " @UpdUser  ='" + pr.UpdUser + "'";
                _cmd += " ,@ServiceActionId =" + pr.ServiceActionId;
                _cmd += " ,@Username ='" + pr.emp[i] + "'";

                DB.DBConn.ExecuteOnly(_cmd);

            }

        }
      

        // DELETE: api/ActionService/5
        public void Delete(int id)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STServiceActions where  ServiceActionId='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
