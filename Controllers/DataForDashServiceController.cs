using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    public class DataForDashServiceController : ApiController
    {
        // GET: api/DataForDashService
        

        // GET: api/DataForDashService/5
        [Route("api/DashService")]
        [HttpGet]
        public IHttpActionResult Get(string CmpId, string OfDate)
        {
            string _cmd;
            _cmd = "exec dbo.getTop5Problem @CmpId=" + Convert.ToInt16(CmpId) + " , @DateOfMonth='" + OfDate +"'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
         
            return Ok(JSONString);
        }

        [Route("api/DashServiceActionPopular")]
        [HttpGet]
        public IHttpActionResult GetActionPop(string CmpId, string OfDate)
        {
            string _cmd;
            _cmd = "exec dbo.getTop5ProblemActions @CmpId=" + Convert.ToInt16(CmpId) + " , @DateOfMonth='" + OfDate + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
         
            return Ok(JSONString);
        }


        [Route("api/DashProblemChartPieSeries")]
        [HttpGet]
        public IHttpActionResult DashProblemChartPie(string CmpId, string OfDate)
        {
            string _cmd;
            _cmd = "exec dbo.dashboardProblemSeries @CmpId=" + Convert.ToInt16(CmpId) + " , @DateOfMonth='" + OfDate + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
         
            return Ok(JSONString);
        }

        [Route("api/DashProblemChartPielabels")]
        [HttpGet]
        public IHttpActionResult DashProblemChartPielabels(string CmpId, string OfDate)
        {
            string _cmd;
            _cmd = "exec dbo.dashboardProblemlabels @CmpId=" + Convert.ToInt16(CmpId) + " , @DateOfMonth='" + OfDate + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
         
            return Ok(JSONString);
        }

         
        // POST: api/DataForDashService
        [Route("api/DashService")]
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/DataForDashService/5
        [Route("api/DashService")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/DataForDashService/5
        [Route("api/DashService")]
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}
