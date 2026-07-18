using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers.Master
{
    [ApiController]
    [Authorize]
    public class JobtypeController : ControllerBase
    {
        // GET: api/Jobtype
        [HttpGet("[action]")]
        public IActionResult GetJobtype([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getJobtypelist @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        } 

        // POST: api/Jobtype
        [HttpPost("[action]")]
        public IActionResult setJobtype(Jobtype jt)
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
                _cmd += ",@CmpId  ='" + jt.CmpId + "'";

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

        [HttpDelete("[action]")]
        public void DeleteJobtype([FromQuery] string jobid)
        {
            string _cmd = "";
            _cmd = "delete from msb.mJobtype where  JobTypeCode='" + jobid + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
