using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using coreapi.Models;
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

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]

    public class ProjectController : ControllerBase
    {


        private readonly IWebHostEnvironment webHostEnvironment;


        public DataSet _ds { get; set; }

        [HttpGet("[action]")]
        public IActionResult getProject([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getProjectView([FromQuery] string CmpId, [FromQuery] string user, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectAllView @CmpId='" + CmpId + "' , @User='" + user + "' , @DocNo='" + docno + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }




        [HttpGet("[action]")]
        public IActionResult getProjectDetail([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectDetail @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }



        [HttpGet("[action]")]
        public IActionResult getProjectTask([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjecttask @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";

            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }



        [HttpGet("[action]")]
        public IActionResult getProjectInstallTask([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectInstalltask @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";

            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }





        [HttpGet("[action]")]
        public IActionResult getProjecttaskres([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;

            _cmd = "exec dbo.GetProjecttask_resource @CmpId=" + Convert.ToInt16(CmpId) + " , @DocNo='" + docno + "'";
            DataTable datatable2 = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable2);
            return Ok(JSONString);
        }



        [HttpGet("[action]")]
        public IActionResult getProjectDemand([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;

            _cmd = "exec dbo.getOnhandDemand @CmpId='" + CmpId + "' , @DocNo='" + docno + "'";
            DataTable datatable2 = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable2);
            return Ok(JSONString);
        }


        // POST: api/Project

        [HttpPost("[action]")]
        public IActionResult QuaHApptoPo(QuoHApprovetoPo apppo)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setQuatationAppToPO @CmpId='" + apppo.cmpid + "' , @DocNo='" + apppo.docno + "' , @RevNo =" + apppo.revno + ",@User='" + apppo.user + "',@state='" + apppo.state + "'";

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





        [HttpPost("[action]")]
        public IActionResult apppo(Apppo project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd = "";
                _cmd = "exec  dbo.SetProjectAppByPO";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += ",@ProjectNo  ='" + project.ProjectNo + "'";
                _cmd += ",@CustCode  ='" + project.CustCode + "'";
                _cmd += ",@Description  ='" + project.Description + "'";
                _cmd += ",@CmpId =" + project.CmpId;
                _cmd += ",@PurchaseNo  ='" + project.PurchaseNo + "'";
                _cmd += ",@QuatationNo  ='" + project.QuatationNo + "'";
                _cmd += ",@ReferCode  ='" + project.ReferCode + "'";
                _cmd += ",@StateActive =" + project.StateActive;
                _cmd += ",@SaleOrderNo  ='" + project.SaleOrderNo + "'";


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



        [HttpPost("[action]")]
        public IActionResult apptoinvoice(AppInvoice project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd = "";
                _cmd = "exec  dbo.setSaleOrderToInvoice";
                _cmd += " @User  ='" + project.UpdUser + "'";
                _cmd += ",@SaleOrderNo  ='" + project.SaleOrderNo + "'";
                _cmd += ",@CmpId =" + project.CmpId;

                _cmd += ",@State =" + project.State;


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






        /*    [HttpPost("[action]")]
           public IActionResult uploadfilePO(IFormCollection files)
           {
               MsgReturn msgretrun = new MsgReturn();


               // Check if the request contains multipart/form-data.
               if (!Request.Content.IsMimeMultipartContent())
               {
                   throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
               }

               string root = $"{webHostEnvironment.WebRootPath}/Image/Signature";
               var provider = new MultipartFormDataStreamProvider(root);

               try
               {
                   // Read the form data.
                   //await Request.Content.ReadAsMultipartAsync(provider);

                   //// This illustrates how to get the file names.
                   //int x = 0;
                   //foreach (MultipartFileData file in provider.FileData)
                   //{
                   //    x += +1;
                   //    var newname = DateTime.Now.ToString("yyyyMMddmmsss");
                   //    string pdfpath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Image/Signature"), newname + x + ".png");
                   //    File.Move(file.LocalFileName, pdfpath);

                   //    var orname = file.Headers.ContentDisposition.Name.ToString();
                   //    string[] subs = orname.Split('|');

                   //    //foreach (var sub in subs)
                   //    //{
                   //    //    Console.WriteLine($"Substring: {sub}");
                   //    //}

                   //    string cmd = "";
                   //    cmd = "exec  dbo.sp_savefileSignature @filename='" + newname + x + "' , @name='" + subs[0].Replace("\"", "") + "', @id=" + subs[1].Replace("\"", "");
                   //    DB.DBConn.ExecuteOnly(cmd);
                   //}
                   // return Request.CreateResponse(HttpStatusCode.OK);
                   msgretrun.ReturnCode = "200";
                   msgretrun.Msg = "Save Success !!";
                   return Ok(msgretrun);
               }
               catch (System.Exception e)
               {
                   msgretrun.ReturnCode = "400";
                   msgretrun.Msg = "Error !!";
                   return Ok(msgretrun);
                   //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
               }
           }


    */

        [HttpPost("[action]")]
        public IActionResult setProject(Project project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd = "";
                _cmd = "exec  dbo.SetProject";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += ",@ProjectNo  ='" + project.ProjectNo + "'";
                _cmd += ",@CustCode  ='" + project.CustCode + "'";
                _cmd += ",@Description  ='" + project.Description + "'";
                _cmd += ",@CmpId ='" + project.CmpId + "'";
                _cmd += ",@PurchaseNo  ='" + project.PurchaseNo + "'";
                _cmd += ",@QuatationNo  ='" + project.QuatationNo + "'";
                _cmd += ",@ReferCode  ='" + project.ReferCode + "'";
                _cmd += ",@StateActive ='" + project.StateActive + "'";
                _cmd += ",@ProjectDueDate  ='" + project.ProjectDueDate + "'";
                _cmd += " , @ProjectDate='" + project.ProjectDate + "'";
                _cmd += " , @SaleOrderNo='" + project.SaleOrderNo + "'";
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



        [HttpPost("[action]")]
        public IActionResult setProjectDetail(List<Project_Detail> project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {

                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                try
                {

                    string _cmd;
                    if (project.Count > 0)
                    {
                        _cmd = "Delete From dbo.Project_Detail where ProjectNo='" + project[0].ProjectNo + "'";

                        DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                    }
                    int il = 0;
                    for (int i = 0; i < project.Count; i++)
                    {
                        il++;

                        _cmd = "exec  dbo.SetProject_Detail";
                        _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                        _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                        _cmd += ",@Seq =" + project[i].Seq;
                        _cmd += ",@ProdCode  ='" + project[i].ProdCode + "'";
                        _cmd += ",@ProdDescription  ='" + project[i].ProdDescription + "'";
                        _cmd += ",@Qty =" + project[i].Qty;
                        _cmd += ",@UnitCode ='" + project[i].UnitCode + "'";
                        _cmd += ",@UnitPrice =" + project[i].UnitPrice;
                        _cmd += ",@Amt =" + project[i].Amt;
                        _cmd += ",@DisPer =" + project[i].DisPer;
                        _cmd += ",@DisAmt =" + project[i].DisAmt;
                        _cmd += ",@NetAmt =" + project[i].NetAmt;
                        _cmd += ",@PricePur =" + project[i].PricePur;
                        _cmd += ",@CostAmt =" + project[i].CostAmt;
                        _cmd += ",@ProfitAmt =" + project[i].ProfitAmt;
                        _cmd += ",@GroupCaption1 ='" + project[i].GroupCaption1 + "'";
                        _cmd += ",@GroupCaption2 ='" + project[i].GroupCaption2 + "'";
                        _cmd += ",@GroupCaption3 ='" + project[i].GroupCaption3 + "'";

                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return Ok(msgretrun);
                        };

                    }

                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                catch (Exception ex)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
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


        [HttpPost("[action]")]
        public IActionResult setProjectTask(List<Project_Task> project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {


                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                try
                {

                    string _cmd;
                    if (project.Count > 0)
                    {
                        _cmd = "Delete From dbo.Project_Task where ProjectNo='" + project[0].ProjectNo + "'";
                        _cmd += "  delete from dbo.[Project_Task_Resource]  where ProjectNo  ='" + project[0].ProjectNo + "'";
                        DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                    }
                    int il = 0;
                    for (int i = 0; i < project.Count; i++)
                    {
                        il++;

                        _cmd = "exec  dbo.SetProject_Task";
                        _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                        _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                        _cmd += ",@Seq =" + il;
                        _cmd += ",@Description  ='" + project[i].Description + "'";
                        _cmd += ",@Qty =" + project[i].Qty;
                        _cmd += ",@UnitCode ='" + project[i].UnitCode + "'";
                        _cmd += ",@UnitPrice =" + project[i].UnitPrice;
                        _cmd += ",@Amt =" + project[i].Amt;
                        _cmd += ",@Resource =''";
                        _cmd += ",@DayQty =" + project[i].DayQty;
                        _cmd += ",@Time =" + project[i].Time;
                        _cmd += ",@StartDate ='" + project[i].StartDate + "'";
                        _cmd += ",@StartTime ='" + project[i].StartTime + "'";
                        _cmd += ",@EndDate ='" + project[i].EndDate + "'";
                        _cmd += ",@EndTime ='" + project[i].EndTime + "'";
                        _cmd += ",@InstallDescription  ='" + project[i].InstallDescription + "'";
                        _cmd += ",@CmpId='" + project[i].CmpId + "'";


                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return Ok(msgretrun);
                        };




                        int x = 1;

                        try
                        {


                            for (int r = 0; r < project[i].Resource.Count; r++)
                            {

                                _cmd = "exec  dbo.setProject_Task_Resource";
                                _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                                _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                                _cmd += " ,@Username ='" + project[i].Resource[r] + "'";
                                _cmd += " ,@Seq =" + x;
                                _cmd += " ,@TaskSeq =" + il;

                                DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                                x++;

                            }



                        }
                        catch
                        {

                        }





                    }

                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                catch (Exception ex)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
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



        [HttpPost("[action]")]
        public IActionResult setProjectTaskInstall(List<Project_TaskInstall> project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                try
                {

                    string _cmd;
                    if (project.Count > 0)
                    {
                        _cmd = "  delete from dbo.[Project_Task_InstallResource]  where ProjectNo  ='" + project[0].ProjectNo + "'";
                        DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                    }
                    int il = 0;
                    for (int i = 0; i < project.Count; i++)
                    {
                        il++;

                        _cmd = "exec  dbo.SetProject_TaskInstall";
                        _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                        _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                        _cmd += ",@Seq =" + il;

                        _cmd += ",@InstallResource =''";
                        _cmd += ",@Qty =" + project[i].InstallQty;
                        _cmd += ",@InstallStartDate ='" + project[i].InstallStartDate + "'";
                        _cmd += ",@InstallStartTime ='" + project[i].InstallStartTime + "'";
                        _cmd += ",@InstallEndDate ='" + project[i].InstallEndDate + "'";
                        _cmd += ",@InstallEndTime ='" + project[i].InstallEndTime + "'";
                        _cmd += ",@InstallDescription  ='" + project[i].InstallDescription + "'";
                        _cmd += ",@CmpId='" + project[i].CmpId + "'";


                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return Ok(msgretrun);
                        };

                        int x = 1;

                        try
                        {


                            for (int r = 0; r < project[i].InstallResource.Count; r++)
                            {

                                _cmd = "exec  dbo.setProject_Task_InstallResource";
                                _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                                _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                                _cmd += " ,@Username ='" + project[i].InstallResource[r] + "'";
                                _cmd += " ,@Seq =" + x;
                                _cmd += " ,@TaskSeq =" + il;

                                DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                                x++;

                            }
                        }
                        catch
                        {

                        }

                    }

                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                catch (Exception ex)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
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


        [HttpGet("[action]")]
        public IActionResult getProjectCost([FromQuery] string CmpId, [FromQuery] string user, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.getProjectCost @CmpId='" + CmpId + "' , @User='" + user + "' , @DocNo='" + docno + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }



        [HttpPost("[action]")]
        public IActionResult setProjectCost(List<ProjectCost> project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {


                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                try
                {

                    string _cmd;
                    if (project.Count > 0)
                    {
                        _cmd = "Delete From dbo.Project_JobCard_Cost where ProjectNo='" + project[0].ProjectNo + "'";

                        DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                    }

                    for (int i = 0; i < project.Count; i++)
                    {


                        _cmd = "exec  dbo.SetProject_JobCard_Cost";
                        _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                        _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                        _cmd += ",@Seq =" + project[i].Seq;
                        _cmd += ",@CostDescription  ='" + project[i].CostDescription + "'";
                        _cmd += ",@CostAmt =" + project[i].CostAmt;
                        _cmd += ",@CmpId ='" + project[i].CmpId + "'";
                        _cmd += ",@AttachFile ='" + project[i].AttachFile + "'";

                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return Ok(msgretrun);
                        };



                    }

                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                catch (Exception ex)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
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
        public IActionResult deleteProjectCost([FromQuery] string docno)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {


                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                try
                {

                    string _cmd;

                    _cmd = "Delete From dbo.Project_JobCard_Cost where ProjectNo='" + docno + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);




                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Del Success !!";
                    return Ok(msgretrun);
                }
                catch (Exception ex)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
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



    }
}
