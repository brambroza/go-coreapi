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
            DataTable dta = new System.Data.DataTable();
            DataTable dtf = new System.Data.DataTable();

            string _cmd;
            _cmd = "exec dbo.[getProblem] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_Assign] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            dta = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_File] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            dtf = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblemActions_All] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            DataTable dtba = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Actions_All] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            DataTable dtbb = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Files_All] @CmpId='" + cmpid + "' ,  @User='" + username + "'";
            DataTable dtbf = DB.DBConn.GetDataTable(_cmd);


            List<STProblem> problems = new List<STProblem>();

            foreach (DataRow b in dt.Rows)
            {
                var stproblem = new STProblem();
                stproblem.UpdUser = b["UpdUser"].ToString();
                stproblem.ProblemId = b["ProblemId"].ToString();
                stproblem.ReceiveDate = b["ReceiveDate"].ToString();
                stproblem.CustomerCode = b["CustomerCode"].ToString();
                stproblem.ContactName = b["ContactName"].ToString();
                stproblem.ProblemDetails = b["ProblemDetails"].ToString();
                stproblem.ReceiveTime = b["ReceiveTime"].ToString();
                stproblem.ProblemType = b["ProblemType"].ToString();
                stproblem.CustBranchName = b["CustBranchName"].ToString();
                stproblem.CmpId = b["CmpId"].ToString();
                stproblem.CustomerName = b["CustomerName"].ToString();
                stproblem.RequestBy = b["RequestBy"].ToString();

                stproblem.ProvinceId = b["ProvinceId"].ToString();
                stproblem.Status = b["Status"].ToString();
                stproblem.Priority = b["Priority"].ToString();
                stproblem.GrpId = b["GrpId"].ToString();
                stproblem.TaskNo = b["TaskNo"].ToString();
                stproblem.TaskId = b["TaskId"].ToString();
                stproblem.UserLineId = b["UserLineId"].ToString();
                stproblem.OALineId = b["OALineId"].ToString();
                stproblem.StartDate = b["StartDate"].ToString();
                stproblem.StartTime = b["StartTime"].ToString();

                stproblem.FeedbackRating = Convert.ToInt32(b["FeedbackRating"].ToString());
                stproblem.FeedbackDate = b["FeedbackDate"].ToString();
                stproblem.IsUnReadMsgCount = Convert.ToInt32(b["IsUnReadMsgCount"].ToString());
                stproblem.FeedbackDescription = b["FeedbackDescription"].ToString();
                stproblem.requestEmail = b["RequestEmail"].ToString();
                stproblem.requestPhone = b["RequestPhone"].ToString();
                stproblem.requestPosition = b["RequestPosition"].ToString();
                stproblem.Remark = b["Remark"].ToString();
                stproblem.IsReadMenu = b["IsReadMenu"].ToString();



                stproblem.attachfile = new List<STProblem_File>();

                foreach (DataRow f in dtf.Select("ProblemId='" + stproblem.ProblemId + "'"))
                {
                    var attachfile = new STProblem_File();
                    attachfile.UpdUser = f["UpdUser"].ToString();
                    attachfile.ProblemId = f["ProblemId"].ToString();
                    attachfile.Seq = Convert.ToInt32(f["Seq"].ToString());
                    attachfile.FileName = f["FileName"].ToString();
                    attachfile.FilePath = f["FilePath"].ToString();
                    attachfile.CmpId = f["CmpId"].ToString();
                    stproblem.attachfile.Add(attachfile);


                }


                stproblem.assign = new List<STProblem_Assign>();
                foreach (DataRow f in dta.Select("ProblemId='" + stproblem.ProblemId + "'"))
                {
                    var assign = new STProblem_Assign();
                    assign.UpdUser = f["UpdUser"].ToString();
                    assign.ProblemId = f["ProblemId"].ToString();
                    assign.UserFullName = f["UserFullName"].ToString();
                    assign.ImgPath = f["ImgPath"].ToString();
                    assign.Permission = f["Permission"].ToString();
                    assign.RouteId = f["RouteId"].ToString();
                    assign.RemindId = f["RemindId"].ToString();
                    assign.UserId = f["UserId"].ToString();
                    assign.CmpId = f["CmpId"].ToString();
                    assign.StateOwner = f["StateOwner"].ToString();
                    stproblem.assign.Add(assign);


                }



                stproblem.action = new STServiceActions();
                foreach (DataRow f in dtba.Select("ProblemId='" + stproblem.ProblemId + "'"))
                {
                    var action = new STServiceActions();
                    action.UpdUser = f["UpdUser"].ToString();
                    action.ProblemId = f["ProblemId"].ToString();
                    action.ServiceActionId = f["ServiceActionId"].ToString();
                    action.ServiceType = Convert.ToInt32(f["ServiceType"].ToString());
                    action.ActionDetails = f["ActionDetails"].ToString();
                    action.FinishDate = f["FinishDate"].ToString();
                    action.FinishTime = f["FinishTime"].ToString();
                    action.CmpId = f["CmpId"].ToString();

                    action.ActionBy = new List<STServiceActions_Assign>();

                    foreach (DataRow ac in dtbb.Select("ServiceActionId='" + action.ServiceActionId + "'"))
                    {
                        var assign = new STServiceActions_Assign();
                        assign.UpdUser = ac["UpdUser"].ToString();
                        assign.ServiceActionId = ac["ServiceActionId"].ToString();
                        assign.UserFullName = ac["UserFullName"].ToString();
                        assign.ImgPath = ac["ImgPath"].ToString();
                        assign.Permission = ac["Permission"].ToString();
                        assign.RouteId = ac["RouteId"].ToString();
                        assign.RemindId = ac["RemindId"].ToString();
                        assign.UserId = ac["UserId"].ToString();
                        assign.CmpId = ac["CmpId"].ToString();
                        action.ActionBy.Add(assign);

                    }


                    action.Attachfile = new List<STServiceActions_File>();

                    foreach (DataRow fa in dtbf.Select("ServiceActionId='" + action.ServiceActionId + "'"))
                    {
                        var attachfile = new STServiceActions_File();
                        attachfile.UpdUser = fa["UpdUser"].ToString();
                        attachfile.ServiceActionId = fa["ServiceActionId"].ToString();
                        attachfile.Seq = Convert.ToInt32(fa["Seq"].ToString());
                        attachfile.FileName = fa["FileName"].ToString();
                        attachfile.FilePath = fa["FilePath"].ToString();
                        attachfile.CmpId = fa["CmpId"].ToString();
                        action.Attachfile.Add(attachfile);


                    }


                    stproblem.action = action;
                }
                problems.Add(stproblem);

            }
            return Ok(problems);

        }

        [HttpPost("[action]")]
        public ActionResult settaskservice(STProblem pr)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            MsgReturn msgretrun = new MsgReturn();
            try
            {




                string _cmd = "";
                _cmd = "exec  dbo.STProblem_Trans";
                _cmd += "  @UpdUser  ='" + pr.UpdUser + "'";
                _cmd += ",@ProblemId ='" + pr.ProblemId + "'";
                _cmd += ",@ReceiveDate ='" + pr.ReceiveDate + "'";
                _cmd += ",@CustCode  ='" + pr.CustomerCode + "'";
                _cmd += ",@RequestBy  ='" + pr.ContactName + "'";
                _cmd += ",@ProblemDetails  ='" + Tool.Tool.validateStr(pr.ProblemDetails) + "'";
                _cmd += ",@ProblemType  ='" + pr.ProblemType + "'";
                _cmd += " ,@ReceiveTime  ='" + pr.ReceiveTime + "'";
                _cmd += " ,@CustBranchName='" + pr.CustBranchName + "'";
                _cmd += " ,@ProvinceId='" + pr.ProvinceId + "'";
                _cmd += " ,@Cmpid='" + pr.CmpId + "'";
                _cmd += " ,@Status='" + pr.Status + "'";
                _cmd += " ,@Priority='" + pr.Priority + "'";
                _cmd += " ,@TaskNo='" + pr.TaskNo + "'";
                _cmd += " ,@TaskId='" + pr.TaskId + "'";
                _cmd += " , @StartDate='" + pr.StartDate + "'";
                _cmd += " , @StartTime='" + pr.StartTime + "'";
                _cmd += " , @Remark='" + pr.Remark + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return BadRequest(msgretrun);
                }


                if (pr.assign.Count > 0)
                {
                    for (int i = 0; i < pr.assign.Count; i++)
                    {
                        _cmd = " exec dbo.STProblem_Trans_Assign ";
                        _cmd += " @ProblemId='" + pr.ProblemId + "'";
                        _cmd += " ,@UserId='" + pr.assign[i].UserId + "'";
                        _cmd += " ,@CmpId='" + pr.assign[i].CmpId + "'";
                        _cmd += " ,@StateOwner='" + pr.assign[i].StateOwner + "'";
                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return BadRequest(msgretrun);
                        }

                    }


                }


        

                if (pr.attachfile.Count > 0)
                {
                    for (int i = 0; i < pr.attachfile.Count; i++)
                    {
                        _cmd = " exec dbo.STProblem_Trans_File ";
                        _cmd += " @ProblemId='" + pr.ProblemId + "'";
                        _cmd += " ,@UpdUser='" + pr.attachfile[i].UpdUser + "'";
                        _cmd += " ,@Seq='" + pr.attachfile[i].Seq + "'";
                        _cmd += " ,@FilePath='" + pr.attachfile[i].FilePath + "'";
                        _cmd += " ,@FileName='" + pr.attachfile[i].FileName + "'";
                        _cmd += " ,@CmpId='" + pr.attachfile[i].CmpId + "'";
                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return BadRequest(msgretrun);
                        }

                    }


                }






                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);


            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }





        }


        [HttpGet("[action]")]
        public IActionResult getProblemServiceReport([FromQuery] string cmpId
        , [FromQuery] string customerCode, [FromQuery] string serviceType
        , [FromQuery] string startDate, [FromQuery] string endDate
        , [FromQuery] string stateFinish, [FromQuery] string stateWait)
        {
            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd = "exec dbo.[getProblemServiceReport] @CmpId='" + cmpId + "' ";
                _cmd += " , @customerCode='" + customerCode + "'";
                _cmd += " , @startDate='" + startDate + "'";
                _cmd += " , @endDate='" + endDate + "'";
                _cmd += " , @serviceType='" + serviceType + "'";
                _cmd += " , @stateFinish='" + stateFinish + "'";
                _cmd += " , @stateWait='" + stateWait + "'";
                dt = DB.DBConn.GetDataTable(_cmd);



                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(dt);

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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching products.", Details = ex.Message });
            }
        }




        [HttpDelete("[action]")]
        public IActionResult DeleteService([FromQuery] string cmpid, [FromQuery] string docno)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STProblem where  ProblemId='" + docno + "'  and cmpid='" + cmpid + "'";

            _cmd += "delete from dbo.STProblem_File where  ProblemId='" + docno + "'  and cmpid='" + cmpid + "'";
            _cmd += "delete from dbo.STProblem_Assign where  ProblemId='" + docno + "'  and cmpid='" + cmpid + "'";

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
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.STServiceActions_Trans";
                _cmd += " @UpdUser  ='" + pr.UpdUser + "'";
                _cmd += ",@ServiceActionId ='" + pr.ServiceActionId + "'";
                _cmd += ",@ProblemId ='" + pr.ProblemId + "'";
                _cmd += ",@ServiceType =" + pr.ServiceType;
                _cmd += ",@ActionDetails  ='" + Tool.Tool.validateStr(pr.ActionDetails) + "'";
                _cmd += ",@FinishDate  ='" + pr.FinishDate + "'";
                _cmd += ",@FinishTime  ='" + pr.FinishTime + "'";
                _cmd += ",@CmpId  ='" + pr.CmpId + "'";


                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return BadRequest(msgretrun);
                }


                if (pr.ActionBy.Count > 0)
                {
                    for (int i = 0; i < pr.ActionBy.Count; i++)
                    {
                        _cmd = " exec dbo.STServiceActions_Trans_Assign ";
                        _cmd += " @ServiceActionId='" + pr.ServiceActionId + "'";
                        _cmd += " ,@UserId='" + pr.ActionBy[i].UserId + "'";
                        _cmd += " ,@CmpId='" + pr.ActionBy[i].CmpId + "'";
                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return BadRequest(msgretrun);
                        }

                    }


                }

                if (pr.Attachfile.Count > 0)
                {
                    for (int i = 0; i < pr.Attachfile.Count; i++)
                    {
                        _cmd = " exec dbo.STServiceActions_Trans_File ";
                        _cmd += " @ServiceActionId='" + pr.ServiceActionId + "'";
                        _cmd += " ,@UpdUser='" + pr.Attachfile[i].UpdUser + "'";
                        _cmd += " ,@Seq='" + pr.Attachfile[i].Seq + "'";
                        _cmd += " ,@FilePath='" + pr.Attachfile[i].FilePath + "'";
                        _cmd += " ,@FileName='" + pr.Attachfile[i].FileName + "'";
                        _cmd += " ,@CmpId='" + pr.Attachfile[i].CmpId + "'";
                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return BadRequest(msgretrun);
                        }

                    }


                }






                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);


            }
            catch
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }


        [HttpDelete("[action]")]
        public IActionResult deleteactionservice([FromQuery] string cmpid, [FromQuery] int id)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STServiceActions where  ServiceActionId='" + id + "' and CmpId='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }


        [HttpDelete("[action]")]
        public IActionResult removeattachfile([FromQuery] string cmpid, [FromQuery] string docno, [FromQuery] string fileName)
        {
            string _cmd = "";
            _cmd = "delete from dbo.STProblem_Files where  ProblemId='" + docno + "' and FileName='" + fileName + "' and CmpId='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }
    }
}
