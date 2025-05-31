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
                project.ProjectDueDate = r["ProjectDueDate"].ToString() ;
                project.ProjectDate =  r["ProjectDate"].ToString() ;
                project.SaleOrderNo = r["SaleOrderNo"].ToString();
                project.Title = r["Title"].ToString();
                project.Priority = r["Priority"].ToString();
                project.RouteId  = r["RouteId"].ToString();
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
                project.ProjectState =   r["ProjectState"].ToString();

                project.MaintenanceServiceNumberOfTime = Convert.ToInt32( r["MaintenanceServiceNumberOfTime"] );
                project.MaintenanceRemoteNumberOfTime = Convert.ToInt32(r["MaintenanceRemoteNumberOfTime"] );
                
                project.MaintenanceServiceReport = r["MaintenanceServiceReport"].ToString()  == "1"  ;
                project.PreventiveServiceNumberOfTime =Convert.ToInt32( r["PreventiveServiceNumberOfTime"] );
                project.PreventiveRemoteNumberOfTime =Convert.ToInt32( r["PreventiveRemoteNumberOfTime"] );
                project.PreventiveServiceReport = r["PreventiveServiceReport"].ToString()  == "1" ;
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
                            CmpId = d["CmpId"].ToString(),
                            type = d["type"].ToString(),
                            QuotationNo = d["QuotationNo"].ToString(),
                            SaleOrderNo = d["SaleOrderNo"].ToString(),
                            Status = d["Status"].ToString(),
                            SupplierCode = d["SupplierCode"].ToString(), 
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
                    hist.ProjectTime =  d["ProjectTime"].ToString();
                    hist.PaymentTime = d["PaymentTime"].ToString();
                    hist.DeliveryTime =  d["DeliveryTime"].ToString();
                    hist.CompletionTime =  d["CompletionTime"].ToString();
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
                    project.Description = r["Description"].ToString();
                    project.CmpId = r["CmpId"].ToString();
                    project.PurchaseNo = r["PurchaseNo"].ToString();
                    project.QuotationNo = r["QuotationNo"].ToString();
                    project.ReferCode = r["ReferCode"].ToString();
                    project.StateActive = r["StateActive"].ToString();
                    project.ProjectDueDate =  r["ProjectDueDate"].ToString();
                    project.ProjectDate =  r["ProjectDate"].ToString();
                    project.SaleOrderNo = r["SaleOrderNo"].ToString();
                    project.Title = r["Title"].ToString();
                    project.Priority = r["Priority"].ToString();
                    project.RouteId = r["RouteId"].ToString();
                    project.Labels = r["Labels"].ToString();
                    project.RouteName = r["RouteName"].ToString();

                    project.items = new List<Project_Detail>();
                    project.TotalQty = dtItem
                        .Select("ProjectNo='" + project.ProjectNo + "'")
                        .Length;
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

                    projects.Add(project);

                    tasks[route.RouteId].Add(project);
                }
            }

            var response = new { board = new { tasks, columns } };
            return Ok(response);
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
                    + ( project.ProjectDate.Length <= 10 ? project.ProjectDate.ToString() :  DateTime
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
                    + (project.CustomerPODate.Length <=0 ? project.CustomerPODate.ToString() :  DateTime
                        .Parse(project.CustomerPODate.ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture))
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
                
                _cmd += " , @MaintenanceServiceReport=" + (project.MaintenanceServiceReport ? '1' : '0') ;
                _cmd += " , @PreventiveServiceNumberOfTime='" + project.PreventiveServiceNumberOfTime.ToString() + "'";
                _cmd += " , @PreventiveRemoteNumberOfTime='" + project.PreventiveRemoteNumberOfTime.ToString() + "'";
                _cmd += " , @PreventiveServiceReport=" + (project.PreventiveServiceReport ? '1' : '0') ;

                _cmd += " , @ServiceSLA='" + project.ServiceSLA + "'";
                _cmd += " , @ServiceReplacement='" + project.ServiceReplacement + "'";
                _cmd += " , @ServiceBackupConfig='" + project.ServiceBackupConfig + "'";
                _cmd += " , @DescriptionShipping='" +  project.DescriptionShipping + "'";
              
                _cmd += " , @CreditDate='" + project.CreditDate + "'";
                _cmd += " , @ShipOfDay='" +  project.ShipOfDay + "'";
              

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
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
            /*    var url = await UploadFilesAsyn(formFiles); */
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

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                }
                ;

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
    }
}
