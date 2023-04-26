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
    
    public class ProfilesController : ApiController
    {

        [HttpGet]
        [Route("api/TaskDaily")]
        public IHttpActionResult GetTask(int cmpid, string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getTaskDaily] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
           List<ProfileTask> profile = new List<ProfileTask>();

            if (dt.Rows.Count > 0)
            {
               
                foreach( DataRow r in dt.Rows)
                {
                    var task = new ProfileTask();
                    task.time = r["time"].ToString();
                    task.task = r["task"].ToString();
                    task.color = r["color"].ToString();
                    task.done = r["done"].Equals(1);
                    

                    profile.Add(task);
                }

                
            }
            return Ok(profile);

        }

        [HttpGet]
        [Route("api/TaskWeek")]
        public IHttpActionResult GetTaskWeek(int cmpid, string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getTaskWeek] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            List<ProfileTask> profile = new List<ProfileTask>();

            if (dt.Rows.Count > 0)
            {

                foreach (DataRow r in dt.Rows)
                {
                    var task = new ProfileTask();
                    task.time = r["time"].ToString();
                    task.task = r["task"].ToString();
                    task.color = r["color"].ToString();
                    task.done = r["done"].Equals(1);


                    profile.Add(task);
                }


            }
            
            return Ok(profile);

        }

        [HttpGet]
        [Route("api/TaskMonth")]
        public IHttpActionResult GetTaskMonth(int cmpid, string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getTaskMonth] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            List<ProfileTask> profile = new List<ProfileTask>();

            if (dt.Rows.Count > 0)
            {

                foreach (DataRow r in dt.Rows)
                {
                    var task = new ProfileTask();
                    task.time = r["time"].ToString();
                    task.task = r["task"].ToString();
                    task.color = r["color"].ToString();
                    task.done = r["done"].Equals(1);


                    profile.Add(task);
                }


            }
            return Ok(profile);

        }




        // GET: api/Profiles
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Profiles/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Profiles
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Profiles/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Profiles/5
        //public void Delete(int id)
        //{
        //}
    }
}
