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
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {

        [HttpGet("[action]")]
        public ActionResult getProject([FromQuery] string CmpId, [FromQuery] string user)
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
                project.CustomerCode = r["CustCode"].ToString();
                project.Description = r["Description"].ToString();
                project.CmpId = r["CmpId"].ToString();
                project.PurchaseNo = r["PurchaseNo"].ToString();
                project.QuotationNo = r["QuotationNo"].ToString();
                project.ReferCode = r["ReferCode"].ToString();
                project.StateActive = r["StateActive"].ToString();
                project.ProjectDueDate = r["ProjectDueDate"].ToString();
                project.ProjectDate = r["ProjectDate"].ToString();
                project.SaleOrderNo = r["SaleOrderNo"].ToString();
                project.Title = r["Title"].ToString();
                project.Priority = r["Priority"].ToString();
                project.RouteId = r["RouteId"].ToString();
                project.Labels = r["Labels"].ToString();
                project.ShippingMethod = r["ShippingMethod"].ToString();
                project.ServiceTerms = r["ServiceTerms"].ToString();
                project.ServiceOfTerms = r["ServiceOfTerms"].ToString();
                project.DeliveryTerms = r["DeliveryTerms"].ToString();
                project.JobType = r["JobType"].ToString();
                project.Shipping = r["Shipping"].ToString();
                project.CustomerPONo = r["CustomerPONo"].ToString();
                project.CustomerPODate = r["CustomerPODate"].ToString();
                project.StateShipAddr = r["StateShipAddr"].ToString();
                project.Shiptoother = r["Shiptoother"].ToString();
                project.TicketId = r["TicketId"].ToString();
                project.ProjectState = r["ProjectState"].ToString();

                project.MaintenanceServiceNumberOfTime = Convert.ToInt32(r["MaintenanceServiceNumberOfTime"]);
                project.MaintenanceRemoteNumberOfTime = Convert.ToInt32(r["MaintenanceRemoteNumberOfTime"]);

                project.MaintenanceServiceReport = r["MaintenanceServiceReport"].ToString() == "1";
                project.PreventiveServiceNumberOfTime = Convert.ToInt32(r["PreventiveServiceNumberOfTime"]);
                project.PreventiveRemoteNumberOfTime = Convert.ToInt32(r["PreventiveRemoteNumberOfTime"]);
                project.PreventiveServiceReport = r["PreventiveServiceReport"].ToString() == "1";
                project.ServiceSLA = r["ServiceSLA"].ToString();
                project.ServiceReplacement = r["ServiceReplacement"].ToString();
                project.ServiceBackupConfig = r["ServiceBackupConfig"].ToString();
                project.DescriptionShipping = r["DescriptionShipping"].ToString();
                project.ServiceOfTermsSelect = r["ServiceOfTermsSelect"]
                    .ToString()
                    .Split(',')
                    .ToList(); // Assuming ServiceOfTermsSelect is a comma-separated string
                project.ServiceTermsReport = r["ServiceTermsReport"].ToString();

                project.CreditDate = Convert.ToInt32(r["CreditDate"]);
                project.ShipOfDay = Convert.ToInt32(r["ShipOfDay"]);

                project.StateApproveType = r["StateApproveType"].ToString();
                project.StateApprove = Convert.ToInt32(r["StateApprove"]);
                project.StateSendApprove = Convert.ToInt32(r["StateSendApprove"]);

                project.CustomerContactName = r["CustomerContactName"].ToString();

                project.items = new List<Project_Detail>();
                project.TotalQty = dtItem.Select("ProjectNo='" + project.ProjectNo + "'").Length;
                project.ProjectStatusType =  r["ProjectStatusType"].ToString();
                
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
                            CmpId = d["CmpId"].ToString(),
                            type = d["type"].ToString(),
                            QuotationNo = d["QuotationNo"].ToString(),
                            SaleOrderNo = d["SaleOrderNo"].ToString(),
                            Status = d["Status"].ToString(),
                            SupplierCode = d["SupplierCode"].ToString(),
                            BarcodeNo = d["BarcodeNo"].ToString(),
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
                            StatusJob = d["StatusJob"].ToString(),
                            Resource = d["Resource"]
                                .ToString()
                                .Split(',')
                                .ToList() // Assuming resources are comma-separated
                            ,
                            TaskNo = d["TaskNo"].ToString(),
                            TaskId = d["TaskId"].ToString(),
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
                            TaskId = d["TaskId"].ToString(),
                            CmpId = d["CmpId"].ToString(),
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
                    hist.ProjectTime = d["ProjectTime"].ToString();
                    hist.PaymentTime = d["PaymentTime"].ToString();
                    hist.DeliveryTime = d["DeliveryTime"].ToString();
                    hist.CompletionTime = d["CompletionTime"].ToString();
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

                foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustomerCode + "'"))
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
        public ActionResult getProjectKanban([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            DataTable dtystemroute = new DataTable();
            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + CmpId + "', @System='Project'";
            dtystemroute = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectKanbanAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectItemAll @CmpId='" + CmpId + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskAll @CmpId='" + CmpId + "' ";
            DataTable dtTask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskSubAll @CmpId='" + CmpId + "' ";
            DataTable dtTaskSub = DB.DBConn.GetDataTable(_cmd);

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

            List<Models.Route> routes = new List<Models.Route>();
            var columns = new List<object>();
            var tasks = new Dictionary<string, List<object>>();
            foreach (DataRow rt in dtystemroute.Rows)
            {
                var route = new Models.Route();
                route.CmpId = rt["CmpId"].ToString();
                route.RouteId = rt["RouteId"].ToString();
                route.RouteName = rt["RouteName"].ToString();
                route.Department = rt["Department"].ToString();
                route.completepercent = double.Parse(rt["completepercent"].ToString());
                route.Seq = int.Parse(rt["Seq"].ToString());

                columns.Add(route);

                tasks[route.RouteId] = new List<object>();

                foreach (DataRow r in dt.Select("RouteId='" + route.RouteId + "'"))
                {
                    var project = new Project();
                    project.UpdUser = r["UpdUser"].ToString();
                    project.ProjectNo = r["ProjectNo"].ToString();
                    project.CustomerCode = r["CustCode"].ToString();
                    project.CustomerName = r["CustomerName"].ToString();
                    project.Description = r["Description"].ToString();
                    project.CmpId = r["CmpId"].ToString();
                    project.PurchaseNo = r["PurchaseNo"].ToString();
                    project.QuotationNo = r["QuotationNo"].ToString();
                    project.ReferCode = r["ReferCode"].ToString();
                    project.StateActive = r["StateActive"].ToString();
                    project.ProjectDueDate = r["ProjectDueDate"].ToString();
                    project.ProjectDate = r["ProjectDate"].ToString();
                    project.SaleOrderNo = r["SaleOrderNo"].ToString();
                    project.Title = r["Title"].ToString();
                    project.Priority = r["Priority"].ToString();
                    project.RouteId = r["RouteId"].ToString();
                    project.Labels = r["Labels"].ToString();
                    project.RouteName = r["RouteName"].ToString();
                    project.TaskName = r["ProdDescription"].ToString();
                    project.TaskNo = r["TaskNo"].ToString();
                    project.TaskId = r["TaskId"].ToString();
                    project.CustomerContactName = r["CustomerContactName"].ToString();

                    project.items = new List<Project_Detail>();
                    project.Seq = int.Parse(r["Seq"].ToString());
                    project.TotalQty = dtItem
                        .Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq + "")
                        .Length;



                    foreach (DataRow d in dtItem.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                CmpId = d["CmpId"].ToString(),
                                type = d["type"].ToString(),
                                QuotationNo = d["QuotationNo"].ToString(),
                                SaleOrderNo = d["SaleOrderNo"].ToString(),
                                BarcodeNo = d["BarcodeNo"].ToString(),
                            }
                        );
                    }

                    project.tasks = new List<Project_Task>();
                    foreach (DataRow d in dtTask.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                TaskNo = d["TaskNo"].ToString(),
                                TaskId = d["TaskId"].ToString(),
                                Resource = d["Resource"]
                                    .ToString()
                                    .Split(',')
                                    .ToList() // Assuming resources are comma-separated
                                ,
                            }
                        );
                    }

                    project.installs = new List<Project_TaskInstall>();
                    foreach (DataRow d in dtInstall.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
                    {
                        project.installs.Add(
                            new Project_TaskInstall
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                ProjectNo = d["ProjectNo"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),
                                InstallResource = d["InstallResource"]
                                    .ToString()
                                    .Split(',')
                                    .ToList(), // Assuming resources are comma-separated
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
                                TaskId = d["TaskId"].ToString(),
                                CmpId = d["CmpId"].ToString()
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
                        hist.ProjectTime = d["ProjectTime"].ToString();
                        hist.PaymentTime = d["PaymentTime"].ToString();
                        hist.DeliveryTime = d["DeliveryTime"].ToString();
                        hist.CompletionTime = d["CompletionTime"].ToString();
                        hist.CmpId = d["CmpId"].ToString();

                        hist.timeline = new List<ProjectTimeline>();
                        foreach (
                            DataRow x in dtTime.Select("ProjectNo='" + project.ProjectNo + "'")
                        )
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

                    foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustomerCode + "'"))
                    {
                        var customer = new CustomerList();

                        customer.UpdUser = d["UpdUser"].ToString() ?? string.Empty;
                        customer.CustomerCode = d["CustomerCode"].ToString() ?? string.Empty;
                        customer.CustomerName = d["CustomerName"].ToString() ?? string.Empty;
                        customer.CustomerAddress = d["CustomerAddress"].ToString() ?? string.Empty;
                        customer.CustomerTaxNo = d["CustomerTaxNo"].ToString() ?? string.Empty;
                        customer.CustomerBranch = d["CustomerBranch"].ToString() ?? string.Empty;
                        customer.CustomerBranchCode = d["CustomerBranchCode"].ToString() ?? string.Empty;
                        customer.CustomerBranchName = d["CustomerBranchName"].ToString() ?? string.Empty;
                        customer.ContactName = d["ContactName"].ToString() ?? string.Empty;
                        customer.ContactEmail = d["ContactEmail"].ToString() ?? string.Empty;
                        customer.ContactPhone = d["ContactPhone"].ToString() ?? string.Empty;
                        customer.ContactName1 = d["ContactName1"].ToString() ?? string.Empty;
                        customer.ContactEmail1 = d["ContactEmail1"].ToString() ?? string.Empty;
                        customer.ContactPhone1 = d["ContactPhone1"].ToString() ?? string.Empty;
                        customer.CreditDay = Convert.ToInt32(d["CreditDay"]);
                        customer.PhoneOffice = d["PhoneOffice"].ToString() ?? string.Empty;
                        customer.FaxOffice = d["FaxOffice"].ToString() ?? string.Empty;
                        customer.Website = d["Website"].ToString() ?? string.Empty;
                        customer.AddressShip = d["AddressShip"].ToString() ?? string.Empty;
                        customer.Remark = d["Remark"].ToString() ?? string.Empty;
                        customer.CmpId = d["CmpId"].ToString() ?? string.Empty;
                        customer.ContactName2 = d["ContactName2"].ToString() ?? string.Empty;
                        customer.ContactEmail2 = d["ContactEmail2"].ToString() ?? string.Empty;
                        customer.ContactPhone2 = d["ContactPhone2"].ToString() ?? string.Empty;
                        customer.ContactPosition2 = d["ContactPosition2"].ToString() ?? string.Empty;
                        customer.ContactPosition1 = d["ContactPosition1"].ToString() ?? string.Empty;
                        customer.ContactPosition = d["ContactPosition"].ToString() ?? string.Empty;
                        customer.AddrSubDistrict = d["AddrSubDistrict"].ToString() ?? string.Empty;
                        customer.AddrDistrict = d["AddrDistrict"].ToString() ?? string.Empty;
                        customer.AddrProvince = d["AddrProvince"].ToString() ?? string.Empty;
                        customer.AddrPostCode = d["AddrPostCode"].ToString() ?? string.Empty;
                        customer.ImgPath = d["ImgPath"].ToString() ?? string.Empty;
                        customer.CreditAccId = Convert.ToInt32(d["CreditAccId"]);
                        customer.DebitAccId = Convert.ToInt32(d["DebitAccId"]);
                        customer.BusinessGrpCode = d["BusinessGrpCode"].ToString() ?? string.Empty;
                        customer.StateCustomer = d["StateCustomer"].ToString() ?? string.Empty;
                        customer.StateVendor = d["StateVendor"].ToString() ?? string.Empty;

                        customer.contacts = new List<ContactList>();

                        foreach (
                            DataRow c in dtContact.Select(
                                "DocType='"
                                    + _DocType
                                    + "' and DocNo='"
                                    + customer.CustomerCode
                                    + "'"
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
                            item.CmpId = c["CmpId"].ToString() ?? string.Empty;
                            item.ContactId = c["ContactId"].ToString();
                            item.ImgPath = c["ImgPath"].ToString();
                            item.DocNo = c["DocNo"].ToString();
                            item.DocType = c["DocType"].ToString();

                            customer.contacts.Add(item);
                        }

                        project.customer = customer;
                    }



                    project.Assign = new List<Project_Assign>();

                    foreach (DataRow d in dtResource.Select("TaskId='" + project.TaskId + "'"))
                    {
                        project.Assign.Add(
                            new Project_Assign
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                ProjectNo = d["ProjectNo"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),

                                CmpId = d["CmpId"].ToString(),
                                UserId = d["UserId"].ToString(),
                                UserFullName = d["UserFullName"].ToString(),
                                ImgPath = d["ImgPath"].ToString(),
                                Permission = d["Permission"].ToString(),
                                RouteId = d["RouteId"].ToString(),
                                RemindId = d["RemindId"].ToString(),
                            }
                        );
                    }


                    project.SubTaskItem = new List<ServiceTaskItem>();
                    foreach (DataRow d in dtTaskSub.Select("TaskId='" + project.TaskId + "'"))
                    {
                        project.SubTaskItem.Add(
                            new ServiceTaskItem
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                TaskId = d["TaskId"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),
                                StateFinish = d["StateFinish"].ToString(),

                                CmpId = d["CmpId"].ToString(),
                                Description = d["Description"].ToString(),

                            }
                        );
                    }



                    projects.Add(project);

                    tasks[route.RouteId].Add(project);
                }
            }

            var response = new { board = new { tasks, columns } };
            return Ok(response);
        }


        [HttpGet("[action]")]
        public ActionResult getProjectKanbanlist([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;


            _cmd = "exec dbo.GetProjectKanbanAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectItemAll @CmpId='" + CmpId + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskAll @CmpId='" + CmpId + "' ";
            DataTable dtTask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskSubAll @CmpId='" + CmpId + "' ";
            DataTable dtTaskSub = DB.DBConn.GetDataTable(_cmd);

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

            List<Models.Route> routes = new List<Models.Route>();




            foreach (DataRow r in dt.Rows)
            {
                var project = new Project();
                project.UpdUser = r["UpdUser"].ToString();
                project.ProjectNo = r["ProjectNo"].ToString();
                project.CustomerCode = r["CustCode"].ToString();
                project.CustomerName = r["CustomerName"].ToString();
                project.Description = r["Description"].ToString();
                project.CmpId = r["CmpId"].ToString();
                project.PurchaseNo = r["PurchaseNo"].ToString();
                project.QuotationNo = r["QuotationNo"].ToString();
                project.ReferCode = r["ReferCode"].ToString();
                project.StateActive = r["StateActive"].ToString();
                project.ProjectDueDate = r["ProjectDueDate"].ToString();
                project.ProjectDate = r["ProjectDate"].ToString();
                project.SaleOrderNo = r["SaleOrderNo"].ToString();
                project.Title = r["Title"].ToString();
                project.Priority = r["Priority"].ToString();
                project.RouteId = r["RouteId"].ToString();
                project.Labels = r["Labels"].ToString();
                project.RouteName = r["RouteName"].ToString();
                project.TaskName = r["ProdDescription"].ToString();
                project.TaskNo = r["TaskNo"].ToString();
                project.TaskId = r["TaskId"].ToString();

                project.items = new List<Project_Detail>();
                project.Seq = int.Parse(r["Seq"].ToString());
                project.TotalQty = dtItem
                    .Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq + "")
                    .Length;

                foreach (DataRow d in dtItem.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                            CmpId = d["CmpId"].ToString(),
                            type = d["type"].ToString(),
                            QuotationNo = d["QuotationNo"].ToString(),
                            SaleOrderNo = d["SaleOrderNo"].ToString(),
                        }
                    );
                }

                project.tasks = new List<Project_Task>();
                foreach (DataRow d in dtTask.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                            TaskNo = d["TaskNo"].ToString(),
                            TaskId = d["TaskId"].ToString(),
                            Resource = d["Resource"]
                                .ToString()
                                .Split(',')
                                .ToList() // Assuming resources are comma-separated
                            ,
                        }
                    );
                }

                project.installs = new List<Project_TaskInstall>();
                foreach (DataRow d in dtInstall.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
                {
                    project.installs.Add(
                        new Project_TaskInstall
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            InstallResource = d["InstallResource"]
                                .ToString()
                                .Split(',')
                                .ToList(), // Assuming resources are comma-separated
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
                foreach (DataRow d in dtfiles.Select("TaskId='" + project.TaskId + "'"))
                {
                    project.attachfile.Add(
                        new Project_File
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            FileName = d["FileName"].ToString(),
                            FilePath = d["FilePath"].ToString(),
                            TaskId = d["TaskId"].ToString(),
                            CmpId = d["CmpId"].ToString(),

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
                    hist.ProjectTime = d["ProjectTime"].ToString();
                    hist.PaymentTime = d["PaymentTime"].ToString();
                    hist.DeliveryTime = d["DeliveryTime"].ToString();
                    hist.CompletionTime = d["CompletionTime"].ToString();
                    hist.CmpId = d["CmpId"].ToString();

                    hist.timeline = new List<ProjectTimeline>();
                    foreach (
                        DataRow x in dtTime.Select("ProjectNo='" + project.ProjectNo + "'")
                    )
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

                foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustomerCode + "'"))
                {
                    var customer = new CustomerList();

                    customer.UpdUser = d["UpdUser"].ToString() ?? string.Empty;
                    customer.CustomerCode = d["CustomerCode"].ToString() ?? string.Empty;
                    customer.CustomerName = d["CustomerName"].ToString() ?? string.Empty;
                    customer.CustomerAddress = d["CustomerAddress"].ToString() ?? string.Empty;
                    customer.CustomerTaxNo = d["CustomerTaxNo"].ToString() ?? string.Empty;
                    customer.CustomerBranch = d["CustomerBranch"].ToString() ?? string.Empty;
                    customer.CustomerBranchCode = d["CustomerBranchCode"].ToString() ?? string.Empty;
                    customer.CustomerBranchName = d["CustomerBranchName"].ToString() ?? string.Empty;
                    customer.ContactName = d["ContactName"].ToString() ?? string.Empty;
                    customer.ContactEmail = d["ContactEmail"].ToString() ?? string.Empty;
                    customer.ContactPhone = d["ContactPhone"].ToString() ?? string.Empty;
                    customer.ContactName1 = d["ContactName1"].ToString() ?? string.Empty;
                    customer.ContactEmail1 = d["ContactEmail1"].ToString() ?? string.Empty;
                    customer.ContactPhone1 = d["ContactPhone1"].ToString() ?? string.Empty;
                    customer.CreditDay = Convert.ToInt32(d["CreditDay"]);
                    customer.PhoneOffice = d["PhoneOffice"].ToString() ?? string.Empty;
                    customer.FaxOffice = d["FaxOffice"].ToString() ?? string.Empty;
                    customer.Website = d["Website"].ToString() ?? string.Empty;
                    customer.AddressShip = d["AddressShip"].ToString() ?? string.Empty;
                    customer.Remark = d["Remark"].ToString() ?? string.Empty;
                    customer.CmpId = d["CmpId"].ToString() ?? string.Empty;
                    customer.ContactName2 = d["ContactName2"].ToString() ?? string.Empty;
                    customer.ContactEmail2 = d["ContactEmail2"].ToString() ?? string.Empty;
                    customer.ContactPhone2 = d["ContactPhone2"].ToString() ?? string.Empty;
                    customer.ContactPosition2 = d["ContactPosition2"].ToString() ?? string.Empty;
                    customer.ContactPosition1 = d["ContactPosition1"].ToString() ?? string.Empty;
                    customer.ContactPosition = d["ContactPosition"].ToString() ?? string.Empty;
                    customer.AddrSubDistrict = d["AddrSubDistrict"].ToString() ?? string.Empty;
                    customer.AddrDistrict = d["AddrDistrict"].ToString() ?? string.Empty;
                    customer.AddrProvince = d["AddrProvince"].ToString() ?? string.Empty;
                    customer.AddrPostCode = d["AddrPostCode"].ToString() ?? string.Empty;
                    customer.ImgPath = d["ImgPath"].ToString() ?? string.Empty;
                    customer.CreditAccId = Convert.ToInt32(d["CreditAccId"]);
                    customer.DebitAccId = Convert.ToInt32(d["DebitAccId"]);
                    customer.BusinessGrpCode = d["BusinessGrpCode"].ToString() ?? string.Empty;
                    customer.StateCustomer = d["StateCustomer"].ToString() ?? string.Empty;
                    customer.StateVendor = d["StateVendor"].ToString() ?? string.Empty;

                    customer.contacts = new List<ContactList>();

                    foreach (
                        DataRow c in dtContact.Select(
                            "DocType='"
                                + _DocType
                                + "' and DocNo='"
                                + customer.CustomerCode
                                + "'"
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
                        item.CmpId = c["CmpId"].ToString() ?? string.Empty;
                        item.ContactId = c["ContactId"].ToString();
                        item.ImgPath = c["ImgPath"].ToString();
                        item.DocNo = c["DocNo"].ToString();
                        item.DocType = c["DocType"].ToString();

                        customer.contacts.Add(item);
                    }

                    project.customer = customer;
                }



                project.Assign = new List<Project_Assign>();

                foreach (DataRow d in dtResource.Select("TaskId='" + project.TaskId + "'"))
                {
                    project.Assign.Add(
                        new Project_Assign
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            ProjectNo = d["ProjectNo"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),

                            CmpId = d["CmpId"].ToString(),
                            UserId = d["UserId"].ToString(),
                            UserFullName = d["UserFullName"].ToString(),
                            ImgPath = d["ImgPath"].ToString(),
                            Permission = d["Permission"].ToString(),
                            RouteId = d["RouteId"].ToString(),
                            RemindId = d["RemindId"].ToString(),
                        }
                    );
                }


                project.SubTaskItem = new List<ServiceTaskItem>();
                foreach (DataRow d in dtTaskSub.Select("TaskId='" + project.TaskId + "'"))
                {
                    project.SubTaskItem.Add(
                        new ServiceTaskItem
                        {
                            UpdUser = d["UpdUser"].ToString(),
                            TaskId = d["TaskId"].ToString(),
                            Seq = Convert.ToInt32(d["Seq"]),
                            StateFinish = d["StateFinish"].ToString(),

                            CmpId = d["CmpId"].ToString(),
                            Description = d["Description"].ToString(),

                        }
                    );
                }




                projects.Add(project);


            }



            return Ok(projects);
        }

        [HttpGet("[action]")]
        public ActionResult getProjectView(
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

        private Dictionary<string, object> RowToDictionary(DataRow row)
        {
            return row.Table.Columns.Cast<DataColumn>()
                      .ToDictionary(
                          col => char.ToLowerInvariant(col.ColumnName[0]) + col.ColumnName.Substring(1),
                          col => row[col] == DBNull.Value ? null : row[col]
                      );
        }


        [HttpGet("[action]")]
        public ActionResult getProjectDetail([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjectDetail @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public ActionResult getProjectTask([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.GetProjecttask @CmpId='" + (CmpId) + "' , @DocNo='" + docno + "'";

            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public ActionResult getProjectInstallTask(
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
        public ActionResult getProjecttaskres([FromQuery] string CmpId, [FromQuery] string docno)
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
        public ActionResult getProjectDemand([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;

            _cmd = "exec dbo.getOnhandDemand @CmpId='" + CmpId + "' , @DocNo='" + docno + "'";
            DataTable datatable2 = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable2);
            return Ok(JSONString);
        }





        [HttpGet("[action]")]
        public ActionResult getServiceTasklist([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;

            _cmd = "exec dbo.GetServiceTaskAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dtTaskAll = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectKanbanAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectItemAll @CmpId='" + CmpId + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskAll @CmpId='" + CmpId + "' ";
            DataTable dtTask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskSubAll @CmpId='" + CmpId + "' ";
            DataTable dtTaskSub = DB.DBConn.GetDataTable(_cmd);

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


            _cmd = "exec dbo.[getProblem] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtb = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_Assign] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dta = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_File] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtf = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtba = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Actions_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtbb = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Files_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtbf = DB.DBConn.GetDataTable(_cmd);




            string _DocType = "customer";

            List<Project> projects = new List<Project>();

            List<ServiceTask> serviceTasks = new List<ServiceTask>();


            _cmd = "exec dbo.getCostExpense @CmpId='" + CmpId + "' , @User='" + User + "' ";
            DataTable dtce = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getCostExpense_File] @CmpId=" + CmpId + " ,  @User='" + User + "'";
            DataTable dtecf = DB.DBConn.GetDataTable(_cmd);




            var filesLookup = dtecf.AsEnumerable()
            .GroupBy(r => r["ExpenseNo"].ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Select(RowToDictionary).ToList()
            );


            var prodlist = dtce.AsEnumerable()
           .GroupBy(r => r["ProjectNo"].ToString())
           .ToDictionary(
               g => g.Key,
               g => g.Select(row =>
               {
                   var dict = RowToDictionary(row); // ใช้ helper method ที่แปลง DataRow → Dictionary
                   var expenseNo = row["ExpenseNo"].ToString();
                   dict["attachments"] = filesLookup.ContainsKey(expenseNo)
                       ? filesLookup[expenseNo]
                       : new List<Dictionary<string, object>>();
                   return dict;
               }).ToList()
           );



            foreach (DataRow tx in dtTaskAll.Rows)
            {
                var serviceTask = new ServiceTask();
                serviceTask.UpdUser = tx["UpdUser"].ToString();
                serviceTask.TaskNo = tx["TaskNo"].ToString();
                serviceTask.TaskId = tx["TaskId"].ToString();
                serviceTask.CmpId = tx["CmpId"].ToString();
                serviceTask.CustCode = tx["CustCode"].ToString();
                serviceTask.Priority = tx["Priority"].ToString();
                serviceTask.DueDate = tx["DueDate"].ToString();
                serviceTask.RouteId = tx["RouteId"].ToString();
                serviceTask.TaskStatus = tx["TaskStatus"].ToString();
                serviceTask.DocRef = tx["DocRef"].ToString();
                serviceTask.TaskDate = tx["TaskDate"].ToString();
                serviceTask.TaskTime = tx["TaskTime"].ToString();
                serviceTask.RequestBy = tx["RequestBy"].ToString();
                serviceTask.Type = tx["Type"].ToString();
                serviceTask.customer = new CustomerList();

                foreach (DataRow d in dtcust.Select("CustomerCode='" + serviceTask.CustCode + "'"))
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

                    serviceTask.customer = customer;
                }



                serviceTask.project = new Project();

                foreach (DataRow r in dt.Select("TaskId='" + serviceTask.TaskId + "'"))
                {
                    var project = new Project();
                    project.UpdUser = r["UpdUser"].ToString();
                    project.ProjectNo = r["ProjectNo"].ToString();
                    project.CustomerCode = r["CustCode"].ToString();
                    project.CustomerName = r["CustomerName"].ToString();
                    project.Description = r["Description"].ToString();
                    project.CmpId = r["CmpId"].ToString();
                    project.PurchaseNo = r["PurchaseNo"].ToString();
                    project.QuotationNo = r["QuotationNo"].ToString();
                    project.ReferCode = r["ReferCode"].ToString();
                    project.StateActive = r["StateActive"].ToString();
                    project.ProjectDueDate = r["ProjectDueDate"].ToString();
                    project.ProjectDate = r["ProjectDate"].ToString();
                    project.SaleOrderNo = r["SaleOrderNo"].ToString();
                    project.Title = r["Title"].ToString();
                    project.Priority = r["Priority"].ToString();
                    project.RouteId = r["RouteId"].ToString();
                    project.Labels = r["Labels"].ToString();
                    project.RouteName = r["RouteName"].ToString();
                    project.TaskName = r["ProdDescription"].ToString();
                    project.TaskNo = r["TaskNo"].ToString();
                    project.TaskId = r["TaskId"].ToString();

                    project.items = new List<Project_Detail>();
                    project.Seq = int.Parse(r["Seq"].ToString());
                    project.TotalQty = dtItem
                        .Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq + "")
                        .Length;

                    foreach (DataRow d in dtItem.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                CmpId = d["CmpId"].ToString(),
                                type = d["type"].ToString(),
                                QuotationNo = d["QuotationNo"].ToString(),
                                SaleOrderNo = d["SaleOrderNo"].ToString(),
                            }
                        );
                    }

                    project.tasks = new List<Project_Task>();
                    foreach (DataRow d in dtTask.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                TaskNo = d["TaskNo"].ToString(),
                                TaskId = d["TaskId"].ToString(),
                                Resource = d["Resource"]
                                    .ToString()
                                    .Split(',')
                                    .ToList() // Assuming resources are comma-separated
                                ,
                            }
                        );
                    }

                    project.installs = new List<Project_TaskInstall>();
                    foreach (DataRow d in dtInstall.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
                    {
                        project.installs.Add(
                            new Project_TaskInstall
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                ProjectNo = d["ProjectNo"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),
                                InstallResource = d["InstallResource"]
                                    .ToString()
                                    .Split(',')
                                    .ToList(), // Assuming resources are comma-separated
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
                    foreach (DataRow d in dtfiles.Select("TaskId='" + project.TaskId + "'"))
                    {
                        project.attachfile.Add(
                            new Project_File
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                ProjectNo = d["ProjectNo"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),
                                FileName = d["FileName"].ToString(),
                                FilePath = d["FilePath"].ToString(),
                                TaskId = d["TaskId"].ToString(),
                                CmpId = d["CmpId"].ToString(),

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
                        hist.ProjectTime = d["ProjectTime"].ToString();
                        hist.PaymentTime = d["PaymentTime"].ToString();
                        hist.DeliveryTime = d["DeliveryTime"].ToString();
                        hist.CompletionTime = d["CompletionTime"].ToString();
                        hist.CmpId = d["CmpId"].ToString();

                        hist.timeline = new List<ProjectTimeline>();
                        foreach (
                            DataRow x in dtTime.Select("ProjectNo='" + project.ProjectNo + "'")
                        )
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

                    foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustomerCode + "'"))
                    {
                        var customer = new CustomerList();

                        customer.UpdUser = d["UpdUser"].ToString() ?? string.Empty;
                        customer.CustomerCode = d["CustomerCode"].ToString() ?? string.Empty;
                        customer.CustomerName = d["CustomerName"].ToString() ?? string.Empty;
                        customer.CustomerAddress = d["CustomerAddress"].ToString() ?? string.Empty;
                        customer.CustomerTaxNo = d["CustomerTaxNo"].ToString() ?? string.Empty;
                        customer.CustomerBranch = d["CustomerBranch"].ToString() ?? string.Empty;
                        customer.CustomerBranchCode = d["CustomerBranchCode"].ToString() ?? string.Empty;
                        customer.CustomerBranchName = d["CustomerBranchName"].ToString() ?? string.Empty;
                        customer.ContactName = d["ContactName"].ToString() ?? string.Empty;
                        customer.ContactEmail = d["ContactEmail"].ToString() ?? string.Empty;
                        customer.ContactPhone = d["ContactPhone"].ToString() ?? string.Empty;
                        customer.ContactName1 = d["ContactName1"].ToString() ?? string.Empty;
                        customer.ContactEmail1 = d["ContactEmail1"].ToString() ?? string.Empty;
                        customer.ContactPhone1 = d["ContactPhone1"].ToString() ?? string.Empty;
                        customer.CreditDay = Convert.ToInt32(d["CreditDay"]);
                        customer.PhoneOffice = d["PhoneOffice"].ToString() ?? string.Empty;
                        customer.FaxOffice = d["FaxOffice"].ToString() ?? string.Empty;
                        customer.Website = d["Website"].ToString() ?? string.Empty;
                        customer.AddressShip = d["AddressShip"].ToString() ?? string.Empty;
                        customer.Remark = d["Remark"].ToString() ?? string.Empty;
                        customer.CmpId = d["CmpId"].ToString() ?? string.Empty;
                        customer.ContactName2 = d["ContactName2"].ToString() ?? string.Empty;
                        customer.ContactEmail2 = d["ContactEmail2"].ToString() ?? string.Empty;
                        customer.ContactPhone2 = d["ContactPhone2"].ToString() ?? string.Empty;
                        customer.ContactPosition2 = d["ContactPosition2"].ToString() ?? string.Empty;
                        customer.ContactPosition1 = d["ContactPosition1"].ToString() ?? string.Empty;
                        customer.ContactPosition = d["ContactPosition"].ToString() ?? string.Empty;
                        customer.AddrSubDistrict = d["AddrSubDistrict"].ToString() ?? string.Empty;
                        customer.AddrDistrict = d["AddrDistrict"].ToString() ?? string.Empty;
                        customer.AddrProvince = d["AddrProvince"].ToString() ?? string.Empty;
                        customer.AddrPostCode = d["AddrPostCode"].ToString() ?? string.Empty;
                        customer.ImgPath = d["ImgPath"].ToString() ?? string.Empty;
                        customer.CreditAccId = Convert.ToInt32(d["CreditAccId"]);
                        customer.DebitAccId = Convert.ToInt32(d["DebitAccId"]);
                        customer.BusinessGrpCode = d["BusinessGrpCode"].ToString() ?? string.Empty;
                        customer.StateCustomer = d["StateCustomer"].ToString() ?? string.Empty;
                        customer.StateVendor = d["StateVendor"].ToString() ?? string.Empty;

                        customer.contacts = new List<ContactList>();

                        foreach (
                            DataRow c in dtContact.Select(
                                "DocType='"
                                    + _DocType
                                    + "' and DocNo='"
                                    + customer.CustomerCode
                                    + "'"
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
                            item.CmpId = c["CmpId"].ToString() ?? string.Empty;
                            item.ContactId = c["ContactId"].ToString();
                            item.ImgPath = c["ImgPath"].ToString();
                            item.DocNo = c["DocNo"].ToString();
                            item.DocType = c["DocType"].ToString();

                            customer.contacts.Add(item);
                        }

                        project.customer = customer;
                    }



                    project.Assign = new List<Project_Assign>();

                    foreach (DataRow d in dtResource.Select("TaskId='" + project.TaskId + "'"))
                    {
                        project.Assign.Add(
                            new Project_Assign
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                ProjectNo = d["ProjectNo"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),

                                CmpId = d["CmpId"].ToString(),
                                UserId = d["UserId"].ToString(),
                                UserFullName = d["UserFullName"].ToString(),
                                ImgPath = d["ImgPath"].ToString(),
                                Permission = d["Permission"].ToString(),
                                RouteId = d["RouteId"].ToString(),
                                RemindId = d["RemindId"].ToString(),
                            }
                        );
                    }


                    project.SubTaskItem = new List<ServiceTaskItem>();
                    foreach (DataRow d in dtTaskSub.Select("TaskId='" + project.TaskId + "'"))
                    {
                        project.SubTaskItem.Add(
                            new ServiceTaskItem
                            {
                                UpdUser = d["UpdUser"].ToString(),
                                TaskId = d["TaskId"].ToString(),
                                Seq = Convert.ToInt32(d["Seq"]),
                                StateFinish = d["StateFinish"].ToString(),

                                CmpId = d["CmpId"].ToString(),
                                Description = d["Description"].ToString(),

                            }
                        );
                    }




                    string projectNo = r["ProjectNo"].ToString();
                    if (prodlist.TryGetValue(projectNo, out var costExpenses))
                    {
                        // ✅ Dynamic Attachments inside costExpense
                        project.costExpense = costExpenses[0];
                    }
                    else
                    {
                        project.costExpense = new Dictionary<string, object>();
                    }


                    projects.Add(project);
                    serviceTask.project = project;


                }



                serviceTask.problem = new STProblem();
                foreach (DataRow b in dtb.Select("TaskId='" + serviceTask.TaskId + "'"))
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
                    stproblem.FeedbackRating = Convert.ToInt32(b["FeedbackRating"].ToString());
                    stproblem.FeedbackDate = b["FeedbackDate"].ToString();

                    stproblem.StartDate = b["StartDate"].ToString();
                    stproblem.StartTime = b["StartTime"].ToString();
                    stproblem.IsUnReadMsgCount = Convert.ToInt32(b["IsUnReadMsgCount"].ToString());

                    stproblem.Remark = b["Remark"].ToString();
                    stproblem.FeedbackDescription = b["FeedbackDescription"].ToString();
                    stproblem.requestEmail = b["RequestEmail"].ToString();
                    stproblem.requestPhone = b["RequestPhone"].ToString();
                    stproblem.requestPosition = b["RequestPosition"].ToString();

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

                    serviceTask.problem = stproblem;

                }






                serviceTasks.Add(serviceTask);





            }

            return Ok(serviceTasks);
        }



        [HttpGet("[action]")]
        public ActionResult getServiceTaskKanban([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            DataTable dtystemroute = new DataTable();
            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + CmpId + "', @System='Project'";
            dtystemroute = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.GetServiceTaskAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dtTaskAll = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectKanbanAll @CmpId='" + CmpId + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjectItemAll @CmpId='" + CmpId + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskAll @CmpId='" + CmpId + "' ";
            DataTable dtTask = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.GetProjecttaskSubAll @CmpId='" + CmpId + "' ";
            DataTable dtTaskSub = DB.DBConn.GetDataTable(_cmd);

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


            _cmd = "exec dbo.[getProblem] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtb = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_Assign] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dta = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.[getProblem_File] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtf = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtba = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Actions_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtbb = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[getProblemActions_Files_All] @CmpId=" + CmpId + " ,  @User='" + user + "'";
            DataTable dtbf = DB.DBConn.GetDataTable(_cmd);



            string _DocType = "customer";

            List<Project> projects = new List<Project>();

            List<Models.Route> routes = new List<Models.Route>();
            var columns = new List<object>();
            var tasks = new Dictionary<string, List<object>>();
            foreach (DataRow rt in dtystemroute.Rows)
            {
                var route = new Models.Route();
                route.CmpId = rt["CmpId"].ToString();
                route.RouteId = rt["RouteId"].ToString();
                route.RouteName = rt["RouteName"].ToString();
                route.Department = rt["Department"].ToString();
                route.completepercent = double.Parse(rt["completepercent"].ToString());
                route.Seq = int.Parse(rt["Seq"].ToString());

                columns.Add(route);

                tasks[route.RouteId] = new List<object>();

                foreach (DataRow tx in dtTaskAll.Select("RouteId='" + route.RouteId + "'"))
                {
                    var serviceTask = new ServiceTask();
                    serviceTask.UpdUser = tx["UpdUser"].ToString();
                    serviceTask.TaskNo = tx["TaskNo"].ToString();
                    serviceTask.TaskId = tx["TaskId"].ToString();
                    serviceTask.CmpId = tx["CmpId"].ToString();
                    serviceTask.CustCode = tx["CustCode"].ToString();
                    serviceTask.Priority = tx["Priority"].ToString();
                    serviceTask.DueDate = tx["DueDate"].ToString();
                    serviceTask.RouteId = tx["RouteId"].ToString();
                    serviceTask.TaskStatus = tx["TaskStatus"].ToString();
                    serviceTask.DocRef = tx["DocRef"].ToString();
                    serviceTask.TaskDate = tx["TaskDate"].ToString();
                    serviceTask.TaskTime = tx["TaskTime"].ToString();
                    serviceTask.RequestBy = tx["RequestBy"].ToString();
                    serviceTask.Type = tx["Type"].ToString();
                    serviceTask.Title = tx["Title"].ToString();
                    serviceTask.customer = new CustomerList();

                    foreach (DataRow d in dtcust.Select("CustomerCode='" + serviceTask.CustCode + "'"))
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

                        serviceTask.customer = customer;
                    }



                    serviceTask.project = new Project();


                    foreach (DataRow r in dt.Select("TaskId='" + serviceTask.TaskId + "'"))
                    {
                        var project = new Project();
                        project.UpdUser = r["UpdUser"].ToString();
                        project.ProjectNo = r["ProjectNo"].ToString();
                        project.CustomerCode = r["CustCode"].ToString();
                        project.CustomerName = r["CustomerName"].ToString();
                        project.Description = r["Description"].ToString();
                        project.CmpId = r["CmpId"].ToString();
                        project.PurchaseNo = r["PurchaseNo"].ToString();
                        project.QuotationNo = r["QuotationNo"].ToString();
                        project.ReferCode = r["ReferCode"].ToString();
                        project.StateActive = r["StateActive"].ToString();
                        project.ProjectDueDate = r["ProjectDueDate"].ToString();
                        project.ProjectDate = r["ProjectDate"].ToString();
                        project.SaleOrderNo = r["SaleOrderNo"].ToString();
                        project.Title = r["Title"].ToString();
                        project.Priority = r["Priority"].ToString();
                        project.RouteId = r["RouteId"].ToString();
                        project.Labels = r["Labels"].ToString();
                        project.RouteName = r["RouteName"].ToString();
                        project.TaskName = r["ProdDescription"].ToString();
                        project.TaskNo = r["TaskNo"].ToString();
                        project.TaskId = r["TaskId"].ToString();
                        project.CustomerContactName = r["CustomerContactName"].ToString();

                        project.items = new List<Project_Detail>();
                        project.Seq = int.Parse(r["Seq"].ToString());
                        project.TotalQty = dtItem
                            .Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq + "")
                            .Length;



                        foreach (DataRow d in dtItem.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                    CmpId = d["CmpId"].ToString(),
                                    type = d["type"].ToString(),
                                    QuotationNo = d["QuotationNo"].ToString(),
                                    SaleOrderNo = d["SaleOrderNo"].ToString(),
                                    BarcodeNo = d["BarcodeNo"].ToString(),
                                }
                            );
                        }

                        project.tasks = new List<Project_Task>();
                        foreach (DataRow d in dtTask.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
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
                                    TaskNo = d["TaskNo"].ToString(),
                                    TaskId = d["TaskId"].ToString(),
                                    Resource = d["Resource"]
                                        .ToString()
                                        .Split(',')
                                        .ToList() // Assuming resources are comma-separated
                                    ,
                                }
                            );
                        }

                        project.installs = new List<Project_TaskInstall>();
                        foreach (DataRow d in dtInstall.Select("ProjectNo='" + project.ProjectNo + "' and Seq=" + project.Seq))
                        {
                            project.installs.Add(
                                new Project_TaskInstall
                                {
                                    UpdUser = d["UpdUser"].ToString(),
                                    ProjectNo = d["ProjectNo"].ToString(),
                                    Seq = Convert.ToInt32(d["Seq"]),
                                    InstallResource = d["InstallResource"]
                                        .ToString()
                                        .Split(',')
                                        .ToList(), // Assuming resources are comma-separated
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
                                    TaskId = d["TaskId"].ToString(),
                                    CmpId = d["CmpId"].ToString()
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
                            hist.ProjectTime = d["ProjectTime"].ToString();
                            hist.PaymentTime = d["PaymentTime"].ToString();
                            hist.DeliveryTime = d["DeliveryTime"].ToString();
                            hist.CompletionTime = d["CompletionTime"].ToString();
                            hist.CmpId = d["CmpId"].ToString();

                            hist.timeline = new List<ProjectTimeline>();
                            foreach (
                                DataRow x in dtTime.Select("ProjectNo='" + project.ProjectNo + "'")
                            )
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

                        foreach (DataRow d in dtcust.Select("CustomerCode='" + project.CustomerCode + "'"))
                        {
                            var customer = new CustomerList();

                            customer.UpdUser = d["UpdUser"].ToString() ?? string.Empty;
                            customer.CustomerCode = d["CustomerCode"].ToString() ?? string.Empty;
                            customer.CustomerName = d["CustomerName"].ToString() ?? string.Empty;
                            customer.CustomerAddress = d["CustomerAddress"].ToString() ?? string.Empty;
                            customer.CustomerTaxNo = d["CustomerTaxNo"].ToString() ?? string.Empty;
                            customer.CustomerBranch = d["CustomerBranch"].ToString() ?? string.Empty;
                            customer.CustomerBranchCode = d["CustomerBranchCode"].ToString() ?? string.Empty;
                            customer.CustomerBranchName = d["CustomerBranchName"].ToString() ?? string.Empty;
                            customer.ContactName = d["ContactName"].ToString() ?? string.Empty;
                            customer.ContactEmail = d["ContactEmail"].ToString() ?? string.Empty;
                            customer.ContactPhone = d["ContactPhone"].ToString() ?? string.Empty;
                            customer.ContactName1 = d["ContactName1"].ToString() ?? string.Empty;
                            customer.ContactEmail1 = d["ContactEmail1"].ToString() ?? string.Empty;
                            customer.ContactPhone1 = d["ContactPhone1"].ToString() ?? string.Empty;
                            customer.CreditDay = Convert.ToInt32(d["CreditDay"]);
                            customer.PhoneOffice = d["PhoneOffice"].ToString() ?? string.Empty;
                            customer.FaxOffice = d["FaxOffice"].ToString() ?? string.Empty;
                            customer.Website = d["Website"].ToString() ?? string.Empty;
                            customer.AddressShip = d["AddressShip"].ToString() ?? string.Empty;
                            customer.Remark = d["Remark"].ToString() ?? string.Empty;
                            customer.CmpId = d["CmpId"].ToString() ?? string.Empty;
                            customer.ContactName2 = d["ContactName2"].ToString() ?? string.Empty;
                            customer.ContactEmail2 = d["ContactEmail2"].ToString() ?? string.Empty;
                            customer.ContactPhone2 = d["ContactPhone2"].ToString() ?? string.Empty;
                            customer.ContactPosition2 = d["ContactPosition2"].ToString() ?? string.Empty;
                            customer.ContactPosition1 = d["ContactPosition1"].ToString() ?? string.Empty;
                            customer.ContactPosition = d["ContactPosition"].ToString() ?? string.Empty;
                            customer.AddrSubDistrict = d["AddrSubDistrict"].ToString() ?? string.Empty;
                            customer.AddrDistrict = d["AddrDistrict"].ToString() ?? string.Empty;
                            customer.AddrProvince = d["AddrProvince"].ToString() ?? string.Empty;
                            customer.AddrPostCode = d["AddrPostCode"].ToString() ?? string.Empty;
                            customer.ImgPath = d["ImgPath"].ToString() ?? string.Empty;
                            customer.CreditAccId = Convert.ToInt32(d["CreditAccId"]);
                            customer.DebitAccId = Convert.ToInt32(d["DebitAccId"]);
                            customer.BusinessGrpCode = d["BusinessGrpCode"].ToString() ?? string.Empty;
                            customer.StateCustomer = d["StateCustomer"].ToString() ?? string.Empty;
                            customer.StateVendor = d["StateVendor"].ToString() ?? string.Empty;

                            customer.contacts = new List<ContactList>();

                            foreach (
                                DataRow c in dtContact.Select(
                                    "DocType='"
                                        + _DocType
                                        + "' and DocNo='"
                                        + customer.CustomerCode
                                        + "'"
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
                                item.CmpId = c["CmpId"].ToString() ?? string.Empty;
                                item.ContactId = c["ContactId"].ToString();
                                item.ImgPath = c["ImgPath"].ToString();
                                item.DocNo = c["DocNo"].ToString();
                                item.DocType = c["DocType"].ToString();

                                customer.contacts.Add(item);
                            }

                            project.customer = customer;
                        }

                        project.Assign = new List<Project_Assign>();

                        foreach (DataRow d in dtResource.Select("TaskId='" + project.TaskId + "'"))
                        {
                            project.Assign.Add(
                                new Project_Assign
                                {
                                    UpdUser = d["UpdUser"].ToString(),
                                    ProjectNo = d["ProjectNo"].ToString(),
                                    Seq = Convert.ToInt32(d["Seq"]),

                                    CmpId = d["CmpId"].ToString(),
                                    UserId = d["UserId"].ToString(),
                                    UserFullName = d["UserFullName"].ToString(),
                                    ImgPath = d["ImgPath"].ToString(),
                                    Permission = d["Permission"].ToString(),
                                    RouteId = d["RouteId"].ToString(),
                                    RemindId = d["RemindId"].ToString(),
                                }
                            );
                        }


                        project.SubTaskItem = new List<ServiceTaskItem>();
                        foreach (DataRow d in dtTaskSub.Select("TaskId='" + project.TaskId + "'"))
                        {
                            project.SubTaskItem.Add(
                                new ServiceTaskItem
                                {
                                    UpdUser = d["UpdUser"].ToString(),
                                    TaskId = d["TaskId"].ToString(),
                                    Seq = Convert.ToInt32(d["Seq"]),
                                    StateFinish = d["StateFinish"].ToString(),

                                    CmpId = d["CmpId"].ToString(),
                                    Description = d["Description"].ToString(),

                                }
                            );
                        }

                        projects.Add(project);

                        serviceTask.project = project;
                    }



                    serviceTask.problem = new STProblem();
                    foreach (DataRow b in dtb.Select("TaskId='" + serviceTask.TaskId + "'"))
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

                        stproblem.FeedbackRating = Convert.ToInt32(b["FeedbackRating"].ToString());
                        stproblem.FeedbackDate = b["FeedbackDate"].ToString();


                        stproblem.StartDate = b["StartDate"].ToString();
                        stproblem.StartTime = b["StartTime"].ToString();
                        stproblem.IsUnReadMsgCount = Convert.ToInt32(b["IsUnReadMsgCount"].ToString());

                        stproblem.Remark = b["Remark"].ToString();
                        stproblem.FeedbackDescription = b["FeedbackDescription"].ToString();
                        stproblem.requestEmail = b["RequestEmail"].ToString();
                        stproblem.requestPhone = b["RequestPhone"].ToString();
                        stproblem.requestPosition = b["RequestPosition"].ToString();

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


                        serviceTask.problem = stproblem;

                    }




                    tasks[route.RouteId].Add(serviceTask);

                }
            }

            var response = new { board = new { tasks, columns } };
            return Ok(response);
        }





        // POST: api/Project

        [HttpPost("[action]")]
        public ActionResult QuoAppToSaleOrder(QuoHApprovetoPo apppo)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {
                string _cmd = "";
                for (int i = 0; i < apppo.items.Count; i++)
                {
                    _cmd =
                                       "exec dbo.setQuotationAppToPO_Item @CmpId='"
                                       + apppo.cmpid
                                       + "' , @DocNo='"
                                       + apppo.docno
                                       + "' , @RevNo ="
                                       + apppo.revno
                                       + ",@User='"
                                       + apppo.user
                                       + "',@state='"
                                       + apppo.state
                                       + "' , @SaleOrderNo='"
                                       + apppo.saleorderno
                                       + "'"
                                       + " , @Remark='"
                                       + apppo.remark
                                       + "'";
                    _cmd += " , @ProdCode='" + apppo.items[i].ProdCode + "'";
                    _cmd += " , @Seq=" + apppo.items[i].Seq + "";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);
                    }



                }



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
                    + "' , @SaleOrderNo='"
                    + apppo.saleorderno
                    + "'"
                    + " , @Remark='"
                    + apppo.remark
                    + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public ActionResult setSoToProject(Apppo project)
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
                _cmd += ",@CmpId ='" + project.CmpId + "'";
                _cmd += ",@PurchaseNo  ='" + project.PurchaseNo + "'";
                _cmd += ",@QuotationNo  ='" + project.QuotationNo + "'";
                _cmd += ",@ReferCode  ='" + project.ReferCode + "'";
                _cmd += ",@StateActive =" + project.StateActive;
                _cmd += ",@SaleOrderNo  ='" + project.SaleOrderNo + "'";
                _cmd += ",@TicketId  ='" + project.TicketId + "'";


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
        public ActionResult apptoinvoice(AppInvoice project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setSaleOrderToInvoice";
                _cmd += " @User  ='" + project.UpdUser + "'";
                _cmd += ",@SaleOrderNo  ='" + project.SaleOrderNo + "'";
                _cmd += ",@CmpId ='" + project.CmpId + "'";

                _cmd += ",@State =" + project.State;
                _cmd += ",@InvoiceNo='" + project.InvoiceNo + "'";

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
        public ActionResult setProjectGenerate(ProjectGenerate project)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setProject_TaskGenerate";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += ",@ProjectNo  ='" + project.ProjectNo + "'";
                _cmd += ",@CmpId  ='" + project.CmpId + "'";

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
        [HttpPost("[action]")]
        public ActionResult setProject(Project project)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.SetProject";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += ",@ProjectNo  ='" + project.ProjectNo + "'";
                _cmd += ",@CustCode  ='" + project.CustomerCode + "'";
                _cmd += ",@Description  ='" + project.Description + "'";
                _cmd += ",@CmpId ='" + project.CmpId + "'";
                _cmd += ",@PurchaseNo  ='" + project.PurchaseNo + "'";
                _cmd += ",@QuotationNo  ='" + project.QuotationNo + "'";
                _cmd += ",@ReferCode  ='" + project.ReferCode + "'";
                _cmd += ",@StateActive ='" + project.StateActive + "'";
                _cmd += ",@TicketId ='" + project.TicketId + "'";

                _cmd +=
                    ",@ProjectDate  ='"
                    + (project.ProjectDate.Length <= 10 ? project.ProjectDate.ToString() : DateTime
                        .Parse(project.ProjectDate.ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture))
                    + "'";

                _cmd += " , @SaleOrderNo='" + project.SaleOrderNo + "'";
                _cmd += " , @Title='" + project.Title + "'";
                _cmd += " , @Priority='" + project.Priority + "'";
                _cmd += " , @RouteId='" + project.RouteId + "'";
                _cmd += " , @ShippingMethod='" + project.ShippingMethod + "'";
                _cmd += " , @ServiceTerms='" + project.ServiceTerms + "'";
                _cmd += " , @ServiceOfTerms='" + project.ServiceOfTerms + "'";
                _cmd += " , @DeliveryTerms='" + project.DeliveryTerms + "'";
                _cmd += " , @StateShipAddr='" + project.StateShipAddr + "'";
                _cmd += " , @Shiptoother='" + project.Shiptoother + "'";
                _cmd += " , @JobType='" + project.JobType + "'";
                _cmd += " , @CustomerPONo='" + project.CustomerPONo + "'";
                _cmd +=
                    ",@CustomerPODate  ='"
                    + (project.CustomerPODate.Length <= 0 ? project.CustomerPODate.ToString() : DateTime
                        .Parse(project.CustomerPODate.ToString())
                        .ToString("yyyy-MM-dd", thaiCulture))
                    + "'";
                _cmd += " , @Shipping='" + project.Shipping + "'";

                _cmd += " , @ProjectState='" + project.ProjectState + "'";
                _cmd +=
                    ",@ProjectDueDate  ='"
                    + (project.ProjectDueDate.Length <= 10 ? project.ProjectDueDate.ToString() : DateTime
                        .Parse(project.ProjectDueDate.ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture))
                    + "'";

                _cmd += " , @MaintenanceServiceNumberOfTime='" + project.MaintenanceServiceNumberOfTime.ToString() + "'";
                _cmd += " , @MaintenanceRemoteNumberOfTime='" + project.MaintenanceRemoteNumberOfTime.ToString() + "'";

                _cmd += " , @MaintenanceServiceReport=" + (project.MaintenanceServiceReport ? '1' : '0');
                _cmd += " , @PreventiveServiceNumberOfTime='" + project.PreventiveServiceNumberOfTime.ToString() + "'";
                _cmd += " , @PreventiveRemoteNumberOfTime='" + project.PreventiveRemoteNumberOfTime.ToString() + "'";
                _cmd += " , @PreventiveServiceReport=" + (project.PreventiveServiceReport ? '1' : '0');

                _cmd += " , @ServiceSLA='" + project.ServiceSLA + "'";
                _cmd += " , @ServiceReplacement='" + project.ServiceReplacement + "'";
                _cmd += " , @ServiceBackupConfig='" + project.ServiceBackupConfig + "'";
                _cmd += " , @DescriptionShipping='" + project.DescriptionShipping + "'";

                _cmd += " , @CreditDate='" + project.CreditDate + "'";
                _cmd += " , @ShipOfDay='" + project.ShipOfDay + "'";

                /*      _cmd += " , @TaskNo='" + project.TaskNo + "'";
                     _cmd += " , @TaskId='" +  project.TaskId + "'"; */

                if (project.ServiceOfTermsSelect != null)
                {
                    _cmd += " , @ServiceOfTermsSelect='"
                            + string.Join(",", project.ServiceOfTermsSelect)
                            + "'";
                }
                else
                {
                    _cmd += " , @ServiceOfTermsSelect=''";
                }

                _cmd += ", @ServiceTermsReport='" + project.ServiceTermsReport + "'";
                _cmd += ", @CustomerContactName='" + project.CustomerContactName + "'";

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

        [HttpPost("[action]")]
        public ActionResult setProjectDetail(List<Project_Detail> project)
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
                        _cmd += ",@ProdDescription  ='" + Tool.Tool.validateStr(project[i].ProdDescription) + "'";
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
                        _cmd += " , @Type='" + project[i].type + "'";
                        _cmd += " , @cmpId='" + project[i].CmpId + "'";
                        _cmd += " , @QuotationNo='" + project[i].QuotationNo + "'";
                        _cmd += " , @SaleOrderNo='" + project[i].SaleOrderNo + "'";
                        _cmd += " ,@SupplierCode='" + project[i].SupplierCode + "'";
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
                catch
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
        public ActionResult setProjectMoveRoute(TaskUpdate comment)
        {

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;

                _cmd = "exec  dbo.SetProjectUpdateColumn";
                _cmd += " @CmpId  ='" + comment.CmpId + "'";
                _cmd += ", @ProjectNo  ='" + comment.TicketId + "'";
                _cmd += ", @RouteId  ='" + comment.RouteId + "'";
                _cmd += ", @updUser  ='" + comment.updUser + "'";
                _cmd += ", @TaskId  ='" + comment.TaskId + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
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
                return NotFound(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public ActionResult setProjectTask(List<Project_Task> project)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
               "th-TH"
           );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();


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
                            "  delete from dbo.[Project_Task_Resource]  where ProjectNo  ='"
                            + project[0].ProjectNo
                            + "' and ";
                        _cmd += " TaskId ='" + project[0].TaskId + "'";
                        _cmd += " and CmpId ='" + project[0].CmpId + "'";
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
                        _cmd += ",@Resource ='" + string.Join(",", project[i].Resource) + "'";
                        _cmd += ",@DayQty =" + project[i].DayQty;
                        _cmd += ",@Time =" + project[i].Time;
                        _cmd += ",@StartDate ='" + project[i].StartDate + "'";
                        _cmd += ",@StartTime ='" + project[i].StartTime + "'";
                        _cmd += ",@EndDate ='" + project[i].EndDate + "'";
                        _cmd += ",@EndTime ='" + (project[i].EndTime.Length >= 8 ? project[i].EndTime.Substring(project[i].EndTime.Length - 8) : project[i].EndTime) + "'";
                        _cmd += ",@InstallDescription  ='" + project[i].InstallDescription + "'";
                        _cmd += ",@CmpId='" + project[i].CmpId + "'";
                        _cmd += ",@TaskNo ='" + project[i].TaskNo + "'";
                        _cmd += ",@TaskId ='" + project[i].TaskId + "'";

                        if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                        {
                            DB.DBConn.Tran.Rollback();
                            DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                            DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                            msgretrun.ReturnCode = "400";
                            msgretrun.Msg = "Error !!";
                            return Ok(msgretrun);
                        }


                        int x = 1;

                        try
                        {
                            for (int r = 0; r < project[i].Resource.Count; r++)
                            {
                                if (project[i].Resource[r] != "")
                                {
                                    _cmd = "exec  dbo.setProject_Task_Resource";
                                    _cmd += " @UpdUser  ='" + project[i].UpdUser + "'";
                                    _cmd += ",@ProjectNo  ='" + project[i].ProjectNo + "'";
                                    _cmd += " ,@Username ='" + project[i].Resource[r] + "'";
                                    _cmd += " ,@Seq =" + x;
                                    _cmd += " ,@TaskSeq =" + il;
                                    _cmd += " ,@TaskId ='" + project[i].TaskId + "'";
                                    _cmd += ", @CmpId='" + project[i].CmpId + "'";

                                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                                }


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
                catch
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
        public ActionResult setProjectTaskInstall(List<Project_TaskInstall> project)
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
                catch
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
        public ActionResult getProjectCost(
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
        public ActionResult setProjectCost(List<ProjectCost> project)
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
                catch
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
        public ActionResult deleteProjectCost([FromQuery] string docno)
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
                catch
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
        public ActionResult setServiceTask(ServiceTask project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setServiceTask";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += " ,@TaskNo  ='" + project.TaskNo + "'";
                _cmd += " ,@TaskId  ='" + project.TaskId + "'";
                _cmd += " ,@CmpId ='" + project.CmpId + "'";
                _cmd += " ,@CustCode='" + project.CustCode + "'";
                _cmd += " ,@Priority='" + project.Priority + "'";
                _cmd += " ,@Title='" + project.Title + "'";
                _cmd += " ,@DueDate='" + project.DueDate + "'";
                _cmd += " ,@RouteId='" + project.RouteId + "'";
                _cmd += " ,@TaskStatus='" + project.TaskStatus + "'";

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
        public ActionResult setServiceTaskItem(ServiceTaskItem project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setServiceTask_Item";
                _cmd += " @UpdUser  ='" + project.UpdUser + "'";
                _cmd += " ,@TaskId  ='" + project.TaskId + "'";
                _cmd += " ,@CmpId ='" + project.CmpId + "'";
                _cmd += " ,@Description='" + project.Description + "'";


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
        public IActionResult setServiceTaskItemFinish([FromBody] ServiceTaskItem task)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                _cmd =
                    "Exec dbo.setServiceTask_Item_Approve @TaskId='"
                    + task.TaskId
                    + "'";
                _cmd += " ,@StateFinish='" + task.StateFinish + "'";
                _cmd += " , @CmpId='" + task.CmpId + "'";
                _cmd += " , @UpdUser='" + task.UpdUser + "'";
                _cmd += " , @Seq =" + task.Seq;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }



        [HttpDelete("[action]")]
        public IActionResult delServiceTaskItem([FromQuery] string TaskId,
            [FromQuery] string cmpid, [FromQuery] string Seq)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                _cmd =
                    " delete  dbo.ServiceTask_Item where TaskId='"
                    + TaskId
                    + "'";
                _cmd += "  and Seq='" + Seq + "'";
                _cmd += "  and CmpId='" + cmpid + "'";


                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }







        [HttpPost("[action]")]
        public IActionResult setProjectFile([FromBody] List<Project_File> Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";



                for (int i = 0; i < Quotation.Count; i++)
                {
                    _cmd =
                        "Exec dbo.SetProject_File @ProjectNo='"
                        + Quotation[i].ProjectNo
                        + "'";
                    _cmd += " ,@Seq=" + i + 1;
                    _cmd += " , @CmpId='" + Quotation[i].CmpId + "'";
                    _cmd += " , @FileName='" + Quotation[i].FileName + "'";
                    _cmd += " , @UpdUser='" + Quotation[i].UpdUser + "'";
                    _cmd += " , @TaskId='" + Quotation[i].TaskId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }


        [HttpDelete("[action]")]
        public IActionResult deleteProjectfile(
            [FromQuery] string TaskId,
            [FromQuery] string cmpid,
            [FromQuery] string filename
        )
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd =
                    "delete from dbo.Project_File where  TaskId='"
                    + TaskId

                    + "' and Cmpid='"
                    + cmpid
                    + "'"
                    + " and FileName='"
                    + filename
                    + "'";
                ;

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
                return Ok(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setTaskSplit([FromBody] ProjectSplit task)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                _cmd =
                    "Exec dbo.setProjectTask_Split @TaskId='"
                    + task.TaskId
                    + "'";
                _cmd += " ,@TaskIdNew='" + task.TaskIdNew + "'";
                _cmd += " , @CmpId='" + task.CmpId + "'";
                _cmd += " , @TaskNoNew='" + task.TaskNoNew + "'";
                _cmd += " , @UpdUser='" + task.UpdUser + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }


        [HttpPost("[action]")]
        public IActionResult setProjectApprove([FromBody] ProjectApprove task)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                _cmd =
                    "Exec dbo.SetProject_Approve @ProjectNo='"
                    + task.ProjectNo
                    + "'";
                _cmd += " ,@StateApp='" + task.State + "'";
                _cmd += " , @CmpId='" + task.CmpId + "'";
                _cmd += " , @UpdUser='" + task.UpdUser + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }

        [HttpPost("[action]")]
        public IActionResult setProjectSendApprove([FromBody] ProjectApprove task)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                _cmd =
                    "Exec dbo.SetProject_SendApprove @ProjectNo='"
                    + task.ProjectNo
                    + "'";
                _cmd += " ,@StateApp='" + task.State + "'";
                _cmd += " , @CmpId='" + task.CmpId + "'";
                _cmd += " , @UpdUser='" + task.UpdUser + "'";
                _cmd += " , @UserTo='" + task.UserTo + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }



        [HttpPost("[action]")]
        public IActionResult setProjectLogs([FromBody] ProjectLogs task)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";

                _cmd =
                    "Exec dbo.setProjectLogs @ProjectNo='"
                    + task.ProjectNo
                    + "'";
                _cmd += " ,@DocNo='" + task.DocNo + "'";
                _cmd += " , @CmpId='" + task.CmpId + "'";
                _cmd += " , @User='" + task.UpdUser + "'";
                _cmd += " , @Type='" + task.LogType + "'";
                _cmd += " , @Description='" + task.Description + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
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
                return Ok(msgretrun);
            }
        }


        [HttpGet("[action]")]
        public ActionResult getProjectLogs([FromQuery] string CmpId, [FromQuery] string docno)
        {
            string _cmd;

            _cmd = "exec dbo.getProjectLogs @CmpId='" + CmpId + "' , @ProjectNo='" + docno + "'";
            DataTable datatable2 = DB.DBConn.GetDataTable(_cmd);


            var rows = datatable2.AsEnumerable()
                .Select(RowToDictionary)
                .ToList();

            return Ok(rows);
        }




    }
}
