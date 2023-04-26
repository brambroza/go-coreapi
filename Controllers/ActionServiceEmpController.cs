using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using coreapi.Models;


namespace coreapi.Controllers
{
     
    
    public class ActionServiceEmpController : ApiController
    {


        // GET: api/ActionServiceEmp/5
       
        
        public IHttpActionResult Get(int cmpid, string username )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblemActions_emp] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }

 
 
    }
}
