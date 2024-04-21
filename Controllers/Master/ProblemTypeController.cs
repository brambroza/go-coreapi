using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace coreapi.Controllers.Master
{

    [ApiController]
    [Authorize]
    public class ProblemTypeController : ControllerBase
    {
        // GET: api/Jobtype
        [HttpGet("[action]")]
        public IActionResult getProblemType([FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getmProblemType @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        // POST: api/Jobtype
        [HttpPost("[action]")]
        public IActionResult setProblemType(ProblemType jt)
        {

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmProblemType";
                _cmd += " @UpdUser  ='" + jt.UpdUser + "'";
                _cmd += ",@ProblemTypeId  ='" + jt.ProblemTypeId + "'";
                _cmd += ",@Descriptions  ='" + jt.Descriptions + "'";
                _cmd += ",@CmpId  ='" + jt.CmpId + "'";
                _cmd += ",@StateActive =" + jt.StateActive;


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

        public void DeleteProblemtype([FromQuery] string id , [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd = "delete from mProblemType where  ProblemTypeId='" + id + "' and CmpId='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
