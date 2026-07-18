using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
namespace goalongapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class CostController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getCostAdvance([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getCostAdvance @CmpId='" + CmpId + "' , @User='" + User + "' ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);


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


        [HttpGet("[action]")]
        public IActionResult getCostExpense([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getCostExpense @CmpId='" + CmpId + "' , @User='" + User + "' ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCostExpense_File] @CmpId=" + CmpId + " ,  @User='" + User + "'";
           DataTable  dtf = DB.DBConn.GetDataTable(_cmd);


            var filesLookup = new Dictionary<string, List<Dictionary<string, object>>>();

                foreach (DataRow row in dtf.Rows)
                {
                    var filesItem = new Dictionary<string, object>();
                    foreach (DataColumn column in dtf.Columns)
                    {
                        string lower = char.ToLowerInvariant(column.ColumnName[0]) + column.ColumnName.Substring(1);
                        filesItem[lower] = row[column];
                    }


                    string expenseNo = row["ExpenseNo"].ToString();
                    if (!filesLookup.ContainsKey(expenseNo))
                    {
                        filesLookup[expenseNo] = new List<Dictionary<string, object>>();
                    }
                    filesLookup[expenseNo].Add(filesItem);
                }

                
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

                  string expenseNo = row["ExpenseNo"].ToString();
                    if (filesLookup.TryGetValue(expenseNo, out var stockserial))
                    {
                        eventObj["attachments"] = stockserial;
                    }
                    else
                    {
                        eventObj["attachments"] = new List<Dictionary<string, object>>();
                    }


                prodlist.Add(eventObj);
            }




            return Ok(prodlist);
        }


        [HttpGet("[action]")]
        public IActionResult getCostCenter([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getCostCenter @CmpId='" + CmpId + "' , @User='" + User + "' ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);


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




        [HttpPost("[action]")]
        public IActionResult setCostAdvance(CostAdvance acc)
        {

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostAdvance ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@AdvanceNo  ='" + acc.AdvanceNo + "'";
                _cmd += ",@UserId ='" + acc.UserId + "'";
                _cmd += ",@AmountRequested =" + acc.AmountRequested;
               /*  _cmd += ",@AmountApproved =" + acc.AmountApproved; */
              /*   _cmd += ",@Status  ='" + acc.Status + "'"; */
                _cmd += ",@ProjectNo  ='" + acc.ProjectNo + "'";
                _cmd += ",@CostCenterNo  ='" + acc.CostCenterNo + "'";
                _cmd += ",@Purpose  ='" + acc.Purpose + "'";
               /*  _cmd += ",@PaymentDate  ='" + acc.PaymentDate + "'"; */
                _cmd += ",@CmpId ='" + acc.CmpId + "'";


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
                return BadRequest(msgretrun);
            }


        }

        [HttpPost("[action]")]
        public IActionResult setCostAdvanceSendApprove([FromBody] CostAdvance acc)
        {

            MsgReturn msgretrun = new MsgReturn();  
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostAdvanceSendApprove ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@AdvanceNo  ='" + acc.AdvanceNo + "'";
                _cmd += ",@Status  ='" + acc.Status + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";     
                _cmd += ", @UserTo='" + acc.UserTo + "'";
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
                return BadRequest(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCostAdvanceApprove(CostAdvance acc)
        {

            MsgReturn msgretrun = new MsgReturn();  
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostAdvanceApprove ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@AdvanceNo  ='" + acc.AdvanceNo + "'";
                _cmd += ",@AmountApproved =" + acc.AmountApproved;
                _cmd += ",@Status  ='" + acc.Status + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";     
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
                return BadRequest(msgretrun);
            }
        }



        [HttpPost("[action]")]
        public IActionResult setCostExpense(CostExpense acc)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostExpense ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@ExpenseNo  ='" + acc.ExpenseNo + "'";
                _cmd += ",@AdvanceNo  ='" + acc.AdvanceNo + "'";
                _cmd += ",@UserId ='" + acc.UserId + "'";
                _cmd += ",@AmountSpent =" + acc.AmountSpent;
                _cmd += ",@Status  ='" + acc.Status + "'";
                _cmd += ",@ProjectNo  ='" + acc.ProjectNo + "'";
                _cmd += ",@CostCenterNo  ='" + acc.CostCenterNo + "'";
                _cmd += ",@Description  ='" + acc.Description + "'";
                _cmd += ",@ExpenseDate  ='" + acc.ExpenseDate + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";
                _cmd += ",@Attachments  ='" + acc.Attachments + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return BadRequest(msgretrun);
                }
                    
                 if (acc.Attachments.Count > 0)
                {
                    for (int i = 0; i < acc.Attachments.Count; i++)
                    {
                        _cmd = " exec dbo.setCostExpense_File ";
                        _cmd += " @ExpenseNo='" + acc.ExpenseNo + "'";
                        _cmd += " ,@UpdUser='" + acc.Attachments[i].UpdUser + "'";
                        _cmd += " ,@Seq='" + acc.Attachments[i].Seq + "'";
                        _cmd += " ,@FilePath='" + acc.Attachments[i].FilePath + "'";
                        _cmd += " ,@FileName='" + acc.Attachments[i].FileName + "'";
                        _cmd += " ,@CmpId='" + acc.Attachments[i].CmpId + "'";
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
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }

    [HttpPost("[action]")]

     public IActionResult setCostExpenseSendApprove([FromBody] CostExpense acc)
        {

            MsgReturn msgretrun = new MsgReturn();  
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostExpenseSendApprove ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@ExpenseNo  ='" + acc.ExpenseNo + "'";
                _cmd += ",@Status  ='" + acc.Status + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";     
                _cmd += ", @UserTo='" + acc.UserTo + "'";
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
                return BadRequest(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCostExpenseApprove(CostExpense acc)
        {

            MsgReturn msgretrun = new MsgReturn();  
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostExpenseApprove ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@ExpenseNo  ='" + acc.ExpenseNo + "'"; 
                _cmd += ",@Status  ='" + acc.Status + "'";
                _cmd += ",@CmpId ='" + acc.CmpId + "'";     
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
                return BadRequest(msgretrun);
            }
        }




        [HttpPost("[action]")]
        public IActionResult setCostCenter(CostCenter acc)
        {

            MsgReturn msgretrun = new MsgReturn();


            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCostCenter ";
                _cmd += " @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += ",@CostCenterNo  ='" + acc.CostCenterNo + "'";
                _cmd += ",@CostCenterName  ='" + acc.CostCenterName + "'";
                _cmd += ",@DepartmentNo  ='" + acc.DepartmentNo + "'";
                _cmd += ",@IsActive =" + acc.IsActive;
                _cmd += ",@BudgetAmount =" + acc.BudgetAmount;
                _cmd += ",@CmpId ='" + acc.CmpId + "'";


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
                return BadRequest(msgretrun);
            }


        }

        [HttpDelete("[action]")]
        public IActionResult deleteCostAdvance([FromQuery] string CmpId, [FromQuery] string id)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "exec dbo.deleteCostAdvance @CmpId='" + CmpId + "', @AdvanceNo='" + id + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
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
                return BadRequest(msgretrun);
            }
        }

        [HttpDelete("[action]")]
        public IActionResult deleteCostExpense([FromQuery] string CmpId, [FromQuery] string id)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "exec dbo.deleteCostExpense @CmpId='" + CmpId + "', @ExpenseNo='" + id + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
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
        public IActionResult deleteCostCenter([FromQuery] string CmpId, [FromQuery] string id)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "exec dbo.deleteCostCenter @CmpId='" + CmpId + "', @CostCenterNo='" + id + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
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
                return BadRequest(msgretrun);
            }
        }
        

 

    









    }


}