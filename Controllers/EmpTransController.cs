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
    public class EmpTransController : ApiController
    {
        // GET: api/EmpTrans
        [Route("api/TimeCard")]
        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            string _cmd = "";
            _cmd = "exec dbo.get_TimeCard  @UserName='" + id + "'";
            DataTable dt;
            dt = DB.DBConn.GetDataTable(_cmd);
           

            return Ok(dt);
        }

        [Route("api/TimeCard")]
        [HttpPost]
        public IHttpActionResult postTimeCard(timecard _timecard)
        {
            string _cmd = "";
            _cmd = "exec dbo.set_TimeCard  @UserName='" + _timecard.UserName + "', @TransDate='" + _timecard.TransDate + "',@TransTime='" + _timecard. TransTime + "',@latitude='" + _timecard.latitude + "',@longitude='" + _timecard.longitude + "' , @Status='" + _timecard.status + "'";
            DataTable dt;
            dt = DB.DBConn.GetDataTable(_cmd);

            return Ok(dt);
        }


     

    }
}
