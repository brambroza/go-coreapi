using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace goalongapi.Controllers.Master
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

                var prodlist = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var eventObj = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        string lowercaseColumnName =
                            char.ToLowerInvariant(column.ColumnName[0])
                            + column.ColumnName.Substring(1);

                        eventObj[lowercaseColumnName] = row[column];
                    }

                    prodlist.Add(eventObj);
                }


                return Ok(prodlist);
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
                 _cmd += ",@Severity  ='" + jt.Severity + "'";


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
                    return BadRequest(msgretrun);
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
