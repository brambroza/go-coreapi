using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class DashboardServiceController : ControllerBase
    {
        [HttpGet]
        [Route("getTotalCase")]
        public IActionResult CaseTotal(
            [FromQuery] string cmpid,
            [FromQuery] string user,
            [FromQuery] string year
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_TotalCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet]
        [Route("getCaseInprogress")]
        public IActionResult CaseInprogress(
       [FromQuery] string cmpid,
       [FromQuery] string user,
       [FromQuery] string year
   )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_InprogressCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }


        [HttpGet]
        [Route("getCaseFinish")]
        public IActionResult CaseFinish(
             [FromQuery] string cmpid,
             [FromQuery] string user,
             [FromQuery] string year
         )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_FinishCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }



        [HttpGet]
        [Route("getTopcaseproblem")]
        public IActionResult TopProblem(
             [FromQuery] string cmpid,
             [FromQuery] string user,
             [FromQuery] string year
         )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_TopProblemCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }



        [HttpGet]
        [Route("getTopFixcase")]
        public IActionResult TopFixcase(
             [FromQuery] string cmpid,
             [FromQuery] string user,
             [FromQuery] string year
         )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_TopFixCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }


        [HttpGet]
        [Route("getTodoTask")]
        public IActionResult TodoTask(
             [FromQuery] string cmpid,
             [FromQuery] string user,
             [FromQuery] string year
         )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.DashboardService_TodoTaskCase @User='"
                + user
                + "'  ,@CmpId='"
                + cmpid
                + "' , @Year='"
                + year
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }




    }
}
