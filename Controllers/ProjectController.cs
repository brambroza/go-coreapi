using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        public DataSet _ds { get; set; }

        [HttpGet("[action]")]
        public IActionResult getProject([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectItemAll @CmpId='" + CmpId + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskAll @CmpId='" + CmpId + "' ";
            DataTable dtTask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectInstalltaskAll @CmpId='" + CmpId + "'  ";
            DataTable dtInstall = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getProjectCostAll @CmpId='" + CmpId + "'";
            DataTable dtCost = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttask_resourceAll @CmpId='" + CmpId + "'";
            DataTable dtResource = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectFileAll @CmpId='" + CmpId + "'";
            DataTable dtfiles = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectHistoryAll @CmpId='" + CmpId + "'";
            DataTable dtHis = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectTimelineAll @CmpId='" + CmpId + "'";
            DataTable dtTime = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getCustomer @CmpId='" + CmpId + "' , @Type='0'";
            DataTable dtcust = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getContact @CmpId='" + CmpId + "'";
            DataTable dtContact = DB.DBConn.GetDataTable(_cmd);

            string _DocType = "customer";

            List<Project> projects = new List<Project>();

            foreach (DataRow r in dt.Rows)
            {
                var project = new Project();
                project.UpdUser = r["UpdUser"].ToString();
                project.ProjectNo = r["ProjectNo"].ToString();
                project.CustCode = r["CustCode"].ToString();
                project.Description = r["Description"].ToString();
                project.CmpId = r["CmpId"].ToString();
                project.PurchaseNo = r["PurchaseNo"].ToString();
                project.QuotationNo = r["QuotationNo"].ToString();
                project.ReferCode = r["ReferCode"].ToString();
                project.StateActive = r["StateActive"].ToString();
                project.ProjectDueDate = DateTime.Parse(r["ProjectDueDate"].ToString());
                project.ProjectDate = DateTime.Parse(r["ProjectDate"].ToString());
                project.SaleOrderNo = r["SaleOrderNo"].ToString();

                project.items = new List<Project_Detail>();
                project.TotalQty = dtItem.Select("ProjectNo='" + project.ProjectNo + "'").Length;
                foreach (DataRow d in dtItem.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    project.items.Add(
                        new Project_Detail
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            ProdCode = d["ProdCode"].ToString(),
                            ProdDescription = d["ProdDescription"].ToString(),
                            Qty = Convert.ToDecimal(d["Qty"]),
                            UnitCode = d["UnitCode"].ToString(),
                            UnitPrice = Convert.ToDecimal(d["UnitPrice"]),
                            Amt = Convert.ToDecimal(d["Amt"]),
                            DisPer = Convert.ToDecimal(d["DisPer"]),
                            DisAmt = Convert.ToDecimal(d["DisAmt"]),
                            NetAmt = Convert.ToDecimal(d["NetAmt"]),
                            PricePur = Convert.ToDecimal(d["PricePur"]),
                            CostAmt = Convert.ToDecimal(d["CostAmt"]),
                            ProfitAmt = Convert.ToDecimal(d["ProfitAmt"]),
                            GroupCaption1 = d["GroupCaption1"].ToString(),
                            GroupCaption2 = d["GroupCaption2"].ToString(),
                            GroupCaption3 = d["GroupCaption3"].ToString(),
                            PurchaseNo = d["PurchaseNo"].ToString(),
                            DeliveryDate = d["DeliveryDate"].ToString(),
                            RevNo = Convert.ToInt32(d["RevNo"]),
                            imgpath = d["imgpath"].ToString(),
                        }
                    );
                }

                project.tasks = new List<Project_Task>();
                foreach (DataRow d in dtTask.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    project.tasks.Add(
                        new Project_Task
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            Description = d["Description"].ToString(),
                            Qty = Convert.ToDecimal(d["Qty"]),
                            UnitCode = d["UnitCode"].ToString(),
                            UnitPrice = Convert.ToDecimal(d["UnitPrice"]),
                            Amt = Convert.ToDecimal(d["Amt"]),
                            DayQty = Convert.ToDecimal(d["DayQty"]),
                            Time = Convert.ToDecimal(d["Time"]),
                            StartDate = d["StartDate"].ToString(),
                            StartTime = d["StartTime"].ToString(),
                            EndDate = d["EndDate"].ToString(),
                            EndTime = d["EndTime"].ToString(),
                            InstallDescription = d["InstallDescription"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                            Resource = d["Resource"]
                                .ToString()
                                .Split(',')
                                .ToList() // Assuming resources are comma-separated
                            ,
                        }
                    );
                }

                project.installs = new List<Project_TaskInstall>();
                foreach (DataRow d in dtInstall.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    project.installs.Add(
                        new Project_TaskInstall
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            InstallResource = d["InstallResource"].ToString().Split(',').ToList(), // Assuming resources are comma-separated
                            InstallQty = Convert.ToDecimal(d["InstallQty"]),
                            InstallStartDate = d["InstallStartDate"].ToString(),
                            InstallStartTime = d["InstallStartTime"].ToString(),
                            InstallEndDate = d["InstallEndDate"].ToString(),
                            InstallEndTime = d["InstallEndTime"].ToString(),
                            InstallDescription = d["InstallDescription"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                        }
                    );
                }

                project.attachfile = new List<Project_File>();
                foreach (DataRow d in dtfiles.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    project.attachfile.Add(
                        new Project_File
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            FileName = d["FileName"].ToString(),
                            FilePath = d["FilePath"].ToString(),
                            Files =
                                d["Files"]
                                as byte[] // Assuming this is binary data
                            ,
                        }
                    );
                }

                project.costs = new List<ProjectCost>();
                foreach (DataRow d in dtCost.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    project.costs.Add(
                        new ProjectCost
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            CostDescription = d["CostDescription"].ToString(),
                            CostAmt = Convert.ToDecimal(d["CostAmt"]),
                            AttachFile = d["AttachFile"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                        }
                    );
                }

                project.history = new ProjectHistory();
                foreach (DataRow d in dtHis.Select("ProjectNo='" + project.ProjectNo + "'"))
                {
                    var hist = new ProjectHistory();
                    hist.ProjectNo = d["ProjectNo"].ToString();
                    hist.ProjectTime = DateTime.Parse(d["ProjectTime"].ToString());
                    hist.PaymentTime = DateTime.Parse(d["PaymentTime"].ToString());
                    hist.DeliveryTime = DateTime.Parse(d["DeliveryTime"].ToString());
                    hist.CompletionTime = DateTime.Parse(d["CompletionTime"].ToString());
                    hist.CmpId = d["CmpId"].ToString();

                    hist.timeline = new List<ProjectTimeline>();
                    foreach (DataRow x in dtTime.Select("ProjectNo='" + project.ProjectNo + "'"))
                    {
                        hist.timeline.Add(
                            new ProjectTimeline
                            {
                                ProjectNo = x["ProjectNo"].ToString(),
                                Title = x["Title"].ToString(),
                                Time = DateTime.Parse(x["Time"].ToString()),
                                CmpId = x["CmpId"].ToString(),
                            }
                        );
                    }

                    project.history = hist;
                }

                project.customer = new CustomerList();

                foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustCode + "'"))
                {
                    var customer = new CustomerList();

                    customer.UpdUser = d["UpdUser"].ToString();
                    customer.CustomerCode = d["CustomerCode"].ToString();
                    customer.CustomerName = d["CustomerName"].ToString();
                    customer.CustomerAddress = d["CustomerAddress"].ToString();
                    customer.CustomerTaxNo = d["CustomerTaxNo"].ToString();
                    customer.CustomerBranch = d["CustomerBranch"].ToString();
                    customer.CustomerBranchCode = d["CustomerBranchCode"].ToString();
                    customer.CustomerBranchName = d["CustomerBranchName"].ToString();
                    customer.ContactName = d["ContactName"].ToString();
                    customer.ContactEmail = d["ContactEmail"].ToString();
                    customer.ContactPhone = d["ContactPhone"].ToString();
                    customer.ContactName1 = d["ContactName1"].ToString();
                    customer.ContactEmail1 = d["ContactEmail1"].ToString();
                    customer.ContactPhone1 = d["ContactPhone1"].ToString();
                    customer.CreditDay = Convert.ToInt32(d["CreditDay"]);
                    customer.PhoneOffice = d["PhoneOffice"].ToString();
                    customer.FaxOffice = d["FaxOffice"].ToString();
                    customer.Website = d["Website"].ToString();
                    customer.AddressShip = d["AddressShip"].ToString();
                    customer.Remark = d["Remark"].ToString();
                    customer.CmpId = d["CmpId"].ToString();
                    customer.ContactName2 = d["ContactName2"].ToString();
                    customer.ContactEmail2 = d["ContactEmail2"].ToString();
                    customer.ContactPhone2 = d["ContactPhone2"].ToString();
                    customer.ContactPosition2 = d["ContactPosition2"].ToString();
                    customer.ContactPosition1 = d["ContactPosition1"].ToString();
                    customer.ContactPosition = d["ContactPosition"].ToString();
                    customer.AddrSubDistrict = d["AddrSubDistrict"].ToString();
                    customer.AddrDistrict = d["AddrDistrict"].ToString();
                    customer.AddrProvince = d["AddrProvince"].ToString();
                    customer.AddrPostCode = d["AddrPostCode"].ToString();
                    customer.ImgPath = d["ImgPath"].ToString();
                    customer.CreditAccId = Convert.ToInt32(d["CreditAccId"]);
                    customer.DebitAccId = Convert.ToInt32(d["DebitAccId"]);
                    customer.BusinessGrpCode = d["BusinessGrpCode"].ToString();
                    customer.StateCustomer = d["StateCustomer"].ToString();
                    customer.StateVendor = d["StateVendor"].ToString();

                    customer.contacts = new List<ContactList>();

                    foreach (
                        DataRow c in dtContact.Select(
                            "DocType='" + _DocType + "' and DocNo='" + customer.CustomerCode + "'"
                        )
                    )
                    {
                        var item = new ContactList();
                        item.UpdUser = c["UpdUser"].ToString();
                        item.ContactName = c["ContactName"].ToString();
                        item.ContactPhone = c["ContactPhone"].ToString();
                        item.ContactEmail = c["ContactEmail"].ToString();
                        item.ContactPosition = c["ContactPosition"].ToString();
                        item.ContactLineId = c["ContactLineId"].ToString();
                        item.Remark = c["Remark"].ToString();
                        item.CmpId = c["CmpId"].ToString();
                        item.ContactId = c["ContactId"].ToString();
                        item.ImgPath = c["ImgPath"].ToString();
                        item.DocNo = c["DocNo"].ToString();
                        item.DocType = c["DocType"].ToString();

                        customer.contacts.Add(item);
                    }

                    project.customer = customer;
                }

                projects.Add(project);
            }

            return Ok(projects);
        }

        [HttpGet("[action]")]
        public IActionResult getProjectView(
            [FromQuery] string CmpId,
            [FromQuery] string user,
            [FromQuery] string docno
        )
        {
            string _cmd;
            _cmd =
                "exec dbo.GetProjectAllView @CmpId='"
                + CmpId
                + "' , @User='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
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
        public IActionResult getProjectInstallTask(
            [FromQuery] string CmpId,
            [FromQuery] string docno
        )
        {
            string _cmd;
            _cmd =
                "exec dbo.GetProjectInstalltask @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";

            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getProjecttaskres([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;

            _cmd =
                "exec dbo.GetProjecttask_resource @CmpId="
                + Convert.ToInt16(CmpId)
                + " , @DocNo='"
                + docno
                + "'";
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
                _cmd =
                    "exec dbo.setQuotationAppToPO @CmpId='"
                    + apppo.cmpid
                    + "' , @DocNo='"
                    + apppo.docno
                    + "' , @RevNo ="
                    + apppo.revno
                    + ",@User='"
                    + apppo.user
                    + "',@state='"
                    + apppo.state
                    + "'";

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
                _cmd += ",@QuotationNo  ='" + project.QuotationNo + "'";
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
                _cmd += ",@QuotationNo  ='" + project.QuotationNo + "'";
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
                        _cmd =
                            "Delete From dbo.Project_Detail where ProjectNo='"
                            + project[0].ProjectNo
                            + "'";

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
                        }
                        ;
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
                        _cmd =
                            "Delete From dbo.Project_Task where ProjectNo='"
                            + project[0].ProjectNo
                            + "'";
                        _cmd +=
                            "  delete from dbo.[Project_Task_Resource]  where ProjectNo  ='"
                            + project[0].ProjectNo
                            + "'";
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
                        }
                        ;

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
                        catch { }
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
                        _cmd =
                            "  delete from dbo.[Project_Task_InstallResource]  where ProjectNo  ='"
                            + project[0].ProjectNo
                            + "'";
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
                        }
                        ;

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
                        catch { }
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
        public IActionResult getProjectCost(
            [FromQuery] string CmpId,
            [FromQuery] string user,
            [FromQuery] string docno
        )
        {
            string _cmd;
            _cmd =
                "exec dbo.getProjectCost @CmpId='"
                + CmpId
                + "' , @User='"
                + user
                + "' , @DocNo='"
                + docno
                + "'";
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
                        _cmd =
                            "Delete From dbo.Project_JobCard_Cost where ProjectNo='"
                            + project[0].ProjectNo
                            + "'";

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
                        }
                        ;
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
