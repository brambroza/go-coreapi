using System.Dynamic;
using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MATaskControll : ControllerBase
    {



        [HttpGet("[action]")]
        public IActionResult getmataskfordashboard([FromQuery] string cmpid, [FromQuery] string userlogin)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProblemForDashBoard] @CmpId=" + cmpid + " ,  @User='" + userlogin + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);

        }




        [HttpGet("[action]")]
        public IActionResult getMATasklist([FromQuery] string userlogin, [FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            DataTable dttask = new System.Data.DataTable();
            DataTable dttaskaction = new System.Data.DataTable();

            string _cmd;
            _cmd = "exec dbo.[getMATaskGrp] @userlogin='" + userlogin + "', @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem] @User='" + userlogin + "', @cmpid='" + cmpid + "'";
            dttask = DB.DBConn.GetDataTable(_cmd);

            List<MAGrp> crm = new List<MAGrp>();

            foreach (DataRow r in dt.Rows)
            {
                var crms = new MAGrp();

                crms.items = new List<STProblemTask>();



                crms.grpid = r["grpid"].ToString();
                crms.grpname = r["grpname"].ToString();
                crms.grpdesciption = r["grpdescription"].ToString();

                foreach (DataRow ct in dttask.Select("grpid='" + r["grpid"].ToString() + "'"))
                {
                    var ctask = new STProblemTask();
                    ctask.ProblemId = Convert.ToInt32(ct["ProblemId"].ToString());
                    ctask.ReceiveDate = ct["ReceiveDateText"].ToString();
                    ctask.RequestBy = ct["RequestBy"].ToString();
                    ctask.ProblemDetails = ct["ProblemDetails"].ToString();
                    ctask.ReceiveTime = ct["ReceiveTime"].ToString();
                    ctask.CustCode = ct["CustCode"].ToString();
                    ctask.GrpId = ct["GrpId"].ToString();


                    ctask.ProblemType = ct["ProblemType"].ToString();
                    ctask.CustBranchName = ct["CustBranchName"].ToString();
                    ctask.CmpId = ct["CmpId"].ToString();
                    ctask.ProvinceId = ct["ProvinceId"].ToString();
                    ctask.CustomerName = ct["CustomerName"].ToString();
                    ctask.imgPath = ct["imgPath"].ToString();
                    ctask.Progress = Convert.ToInt32(ct["Progress"].ToString());




                    ctask.ServiceActionId = Convert.ToInt32(ct["ServiceActionId"].ToString());
                    ctask.ServiceActionBy = ct["ServiceActionBy"].ToString();
                    ctask.ServiceType = Convert.ToInt32(   ct["ServiceTypes"].ToString() );
                    ctask.ActionDetails = ct["ActionDetails"].ToString();
                    ctask.FinishDate = ct["FinishDateaction"].ToString();
                    ctask.FinishTime = ct["FinishTimeaction"].ToString();




                    crms.items.Add(ctask);
                }

                crm.Add(crms);


            }

            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(crm);
            return Ok(qdetail);
        }






        
        [HttpPost("[action]")]
        public IActionResult setMATaskGrp(CrmGrpModel grp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setMaTaskGrp";
                _cmd += " @CreateUser  ='" + grp.CreateUser + "'";
                _cmd += ",@CmpId ='" + grp.CmpId + "'";
                _cmd += ",@GrpId ='" + grp.GrpId + "'";
                _cmd += ",@GrpName  ='" + grp.GrpName + "'";
                _cmd += ",@GrpDescription  ='" + grp.GrpDescription + "'";

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
                    return NotFound(msgretrun);
                }

            }
            catch
            {

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }

        }


 



        [HttpPost("[action]")]
        public IActionResult setMaTaskMove(MaTaskMoveModel mt)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setMaTaskMove";
                _cmd += " @CreateUser  ='" + mt.CreateUser + "'";
                _cmd += ",@CmpId ='" + mt.CmpId + "'";
                _cmd += ",@GrpId ='" + mt.GrpId + "'";
                _cmd += ",@ProblemId  =" + mt.ProblemId + "";

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
                    return NotFound(msgretrun);
                }

            }
            catch
            {

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);
            }
        }









    }
}