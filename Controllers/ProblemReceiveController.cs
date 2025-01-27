using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]

    public class ProblemReceiveController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult gettaskservice([FromQuery] string cmpid, [FromQuery] string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblem] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);

        }

        [HttpPost("[action]")]
        public IActionResult settaskservice(STProblem pr)
        {


            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.STProblem_Trans";
                _cmd += "  @UpdUser  ='" + pr.UpdUser + "'";
                _cmd += ",@ProblemId =" + pr.ProblemId;
                _cmd += ",@ReceiveDate ='" + pr.ReceiveDate + "'";
                _cmd += ",@CustCode  ='" + pr.CustCode + "'";
                _cmd += ",@RequestBy  ='" + pr.RequestBy + "'";
                _cmd += ",@ProblemDetails  ='" + Tool.Tool.validateStr(pr.ProblemDetails) + "'";
                _cmd += ",@ProblemType  ='" + pr.ProblemType + "'";
                _cmd += ",@ReceiveTime  ='" + pr.ReceiveTime + "'";
                _cmd += " ,@CustBranchName='" + pr.CustBranchName + "'";
                _cmd += " ,@ProvinceId='" + pr.ProvinceId + "'";
                _cmd += " ,@Cmpid='" + pr.CmpId + "'";








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
        public IActionResult DeleteService([FromQuery] string cmpid, [FromQuery] int id)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STProblem where  ProblemId='" + id + "'  and cmpid='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }



        [HttpGet("[action]")]
        public IActionResult getactionservice([FromQuery] string cmpid, [FromQuery] string username)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblemActions] @CmpId=" + cmpid + " ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
             string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);

        }

     

        // POST: api/ActionService
        [HttpPost("[action]")]
        public IActionResult setactionservice(STServiceActions pr)
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
             _cmd += ",@CmpId  ='" + pr.CmpId + "'";

            DB.DBConn.ExecuteOnly(_cmd);

           

            return Ok();

        }
      

        [HttpDelete("[action]")]
        public IActionResult deleteactionservice( [FromQuery] string cmpid , [FromQuery] int id)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STServiceActions where  ServiceActionId='" + id + "' and CmpId='" + cmpid + "'" ;
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }
    }
}
