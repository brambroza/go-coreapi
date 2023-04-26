using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers.Master
{
 
    public class JobtypeController : ApiController
    {
        // GET: api/Jobtype
        [HttpGet]
        [Route("api/Jobtype")]
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getJobtypelist @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }


        // POST: api/Jobtype
        [HttpPost]
        [Route("api/Jobtype")] 
        public IHttpActionResult Post(Jobtype jt)
        { 

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = ""; 
                _cmd = "exec  dbo.setJobtype"; 
                _cmd += " @UpdUser  ='" + jt.UpdUser + "'";
                _cmd += ",@JobTypeCode  ='" + jt.JobTypeCode + "'"; 
                _cmd += ",@JobTypeName  ='" + jt.JobTypeName + "'"; 
                _cmd += ",@JobTypeDescripton  ='" + jt.JobTypeDescripton + "'";
                _cmd += ",@JobTypeStateActive =" + jt.JobTypeStateActive;


                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }



        }
        // PUT: api/Jobtype/5
       

        // DELETE: api/Jobtype/5
        [HttpDelete]
        [Route("api/Jobtype")]
        public void Delete(string id)
        {
            string _cmd = "";
            _cmd = "delete from msb.mJobtype where  JobTypeCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
