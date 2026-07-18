using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class RevenueMobileController : ControllerBase
    {

        [HttpPost("[action]")]
        public ActionResult Qoutoinvoice(QuoToInvoice project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.[setQuotationToInvoice]";
                _cmd += " @User  ='" + project.UpdUser + "'";
                _cmd += ",@QuotationNo  ='" + project.QuotationNo + "'";
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
        public ActionResult InvoiceToReceive(QuoToInvoice project)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.[setInvoiceToReceive]";
                _cmd += " @User  ='" + project.UpdUser + "'";
                _cmd += ",@ReceiveNo  ='" + project.QuotationNo + "'";
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
        public ActionResult setQuoFormMobile([FromBody] QuotationMobile Quotation)
        {
            MsgReturn msgretrun = new MsgReturn();

            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
             "th-TH"
         );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();



            string _cmd = "";
            string _CustomerCode = "";
            DataTable dt = new System.Data.DataTable();
            try
            {
                if (Quotation.customer.CustomerCode == "")
                {

                    _cmd =
                             "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                             + Quotation.CmpId
                             + "-custrun]      select   @Runno   "; // + cmpid  ;
                    dt = DB.DBConn.GetDataTable(_cmd);

                    _CustomerCode = dt.Rows[0][0].ToString();

                }
            }
            catch
            {


            }


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {


                if (Quotation.customer.CustomerCode == "")
                {


                    Quotation.customer.CustomerCode = _CustomerCode;

                    _cmd = "exec  dbo.CustomerListTrans";
                    _cmd += " @UpdUser  ='" + Quotation.customer.UpdUser + "'";
                    _cmd += ",@CustomerCode  ='" + Quotation.customer.CustomerCode + "'";
                    _cmd += ",@CustomerName  ='" + Quotation.customer.CustomerName + "'";
                    _cmd += ",@CustomerAddress  ='" + Quotation.customer.CustomerAddress + "'";
                    _cmd += ",@CustomerTaxNo  ='" + Quotation.customer.CustomerTaxNo + "'";
                    _cmd += ",@CustomerBranch  ='" + Quotation.customer.CustomerBranch + "'";
                    _cmd += ",@CustomerBranchCode  ='" + Quotation.customer.CustomerBranchCode + "'";
                    _cmd += ",@CustomerBranchName  ='" + Quotation.customer.CustomerBranchName + "'";
                    _cmd += ",@ContactName  ='" + Quotation.customer.ContactName + "'";
                    _cmd += ",@ContactEmail  ='" + Quotation.customer.ContactEmail + "'";
                    _cmd += ",@ContactPhone  ='" + Quotation.customer.ContactPhone + "'";
                    _cmd += ",@ContactName1  ='" + Quotation.customer.ContactName1 + "'";
                    _cmd += ",@ContactEmail1  ='" + Quotation.customer.ContactEmail1 + "'";
                    _cmd += ",@ContactPhone1  ='" + Quotation.customer.ContactPhone1 + "'";
                    _cmd += ",@ContactName2  ='" + Quotation.customer.ContactName2 + "'";
                    _cmd += ",@ContactEmail2  ='" + Quotation.customer.ContactEmail2 + "'";
                    _cmd += ",@ContactPhone2  ='" + Quotation.customer.ContactPhone2 + "'";
                    _cmd += ",@ContactPosition1  ='" + Quotation.customer.ContactPosition1 + "'";
                    _cmd += ",@ContactPosition2  ='" + Quotation.customer.ContactPosition2 + "'";
                    _cmd += ",@ContactPosition  ='" + Quotation.customer.ContactPosition + "'";
                    _cmd += ",@CreditDay =" + Quotation.customer.CreditDay;
                    _cmd += ",@PhoneOffice  ='" + Quotation.customer.PhoneOffice + "'";
                    _cmd += ",@FaxOffice  ='" + Quotation.customer.FaxOffice + "'";
                    _cmd += ",@Website  ='" + Quotation.customer.Website + "'";
                    _cmd += ",@AddressShip  ='" + Quotation.customer.AddressShip + "'";
                    _cmd += ",@Remark  ='" + Quotation.customer.Remark + "'";
                    _cmd += ",@CmpId ='" + Quotation.customer.CmpId + "'";
                    _cmd += ",@AddrSubDistrict  ='" + Quotation.customer.AddrSubDistrict + "'";
                    _cmd += ",@AddrDistrict  ='" + Quotation.customer.AddrDistrict + "'";
                    _cmd += ",@AddrProvince  ='" + Quotation.customer.AddrProvince + "'";
                    _cmd += ",@AddrPostCode  ='" + Quotation.customer.AddrPostCode + "'";
                    _cmd += ",@ImgPath  ='" + Quotation.customer.ImgPath + "'";
                    _cmd += ",@CreditAccId =" + Quotation.customer.CreditAccId;
                    _cmd += ",@DebitAccId =" + Quotation.customer.DebitAccId;
                    _cmd += " , @BusinessGrpCode='" + Quotation.customer.BusinessGrpCode + "'";
                    _cmd += " , @StateCustomer=1";
                    _cmd += " , @StateVendor=0";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    }

                }



                if (Quotation.Items.Count > 0)
                {
                    _cmd =
                        "Delete From mdb.Quotation_Detail where QuotationNo='"
                        + Quotation.QuotationNo
                        + "'";
                    _cmd += " and  RevNo=" + Quotation.Items[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < Quotation.Items.Count; i++)
                {
                    il++;
                    _cmd =
                        "Exec setQuotationDetail @QuotationNo='" + Quotation.QuotationNo + "'";
                    _cmd += ",@Seq=" + Quotation.Items[i].Seq;
                    _cmd += ",@ProdCode='" + Quotation.Items[i].ProdCode + "'";
                    _cmd +=
                        ",@ProdDesc='" + Tool.Tool.validateStr(Quotation.Items[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + Quotation.Items[i].UnitPrice;
                    _cmd += ",@UnitCode='" + Quotation.Items[i].UnitCode + "'";
                    _cmd += ",@Qty=" + Quotation.Items[i].Qty;
                    _cmd += ",@Amt=" + Quotation.Items[i].Amt;
                    _cmd += ",@PricePur=" + Quotation.Items[i].PricePur;
                    _cmd += ",@CostAmt=" + Quotation.Items[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + Quotation.Items[i].ProfitAmt;
                    _cmd += ",@RevNo=" + Quotation.Items[i].RevNo;
                    _cmd +=
                        " ,@GroupCaption1='"
                        + Tool.Tool.validateStr(Quotation.Items[i].GroupCaption1)
                        + "'";
                    _cmd +=
                        " ,@GroupCaption2='"
                        + Tool.Tool.validateStr(Quotation.Items[i].GroupCaption2)
                        + "'";
                    _cmd +=
                        " ,@GroupCaption3='"
                        + Tool.Tool.validateStr(Quotation.Items[i].GroupCaption3)
                        + "'";
                    _cmd += " , @CmpId='" + Quotation.Items[i].CmpId + "'";
                    _cmd += ",@GrossProfitPer=" + Quotation.Items[i].GrossProfitPer;
                    _cmd += ",@UpdUser='" + Quotation.QuotationBy + "'";
                    _cmd += ",@MainProdCode='" + Quotation.Items[i].MainProdCode + "'";
                    _cmd += ",@MainSeq=" + Quotation.Items[i].MainSeq;
                    _cmd += ",@SeqSort=" + il;

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    }

                }






                _cmd =
                    "exec  dbo.setQuotation @QuotationNo='"
                    + Quotation.QuotationNo
                    + "' ,@QuotationDate='"
                    + Quotation.QuotationDate.Substring(6, 4) + "-" + Quotation.QuotationDate.Substring(3, 2) + "-" + Quotation.QuotationDate.Substring(0, 2)
                    + "' ,@QuotationBy='"
                    + Quotation.QuotationBy
                    + "'";
                _cmd += " ,@QuotationState='" + Quotation.QuotationState + "'";
                _cmd += " ,@CustomerCode='" + Quotation.customer.CustomerCode + "'";
                _cmd += " ,@CreditType=" + Quotation.CreditType;
                _cmd += " ,@CreditDate=" + Quotation.CreditDate;
                _cmd += " ,@ProjectName='" + Tool.Tool.validateStr(Quotation.ProjectName) + "'";
                _cmd += " ,@ReferCode='" + Tool.Tool.validateStr(Quotation.ReferCode) + "'";
                _cmd += " ,@VatType=" + Quotation.VatType;
                _cmd += " ,@Remark='" + Quotation.Remark + "'";
                _cmd += " ,@Note='" + Quotation.Note + "'";
                _cmd += " ,@QuotationAmt=" + Quotation.QuotationAmt;
                _cmd += " ,@QuotationDisPer=" + Quotation.QuotationDisPer;
                _cmd += " ,@QuotationDisAmt=" + Quotation.QuotationDisAmt;
                _cmd += " ,@QuotationNetAmt=" + Quotation.QuotationNetAmt;
                _cmd += " ,@QuotationVatAmt=" + Quotation.QuotationVatAmt;
                _cmd += " ,@QuotationGrandAmt=" + Quotation.QuotationGrandAmt;
                _cmd += " ,@WithholdingTaxState=" + Quotation.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState=" + Quotation.ShowSignatureState;
                _cmd += " ,@CmpId='" + Quotation.CmpId + "'";
                _cmd += " ,@PriceStand='" + Quotation.PriceStand + "'";
                _cmd += " ,@PaymentDue='" + Quotation.PaymentDue + "'";
                _cmd += " ,@Shipping='" + Quotation.Shipping + "'";
                _cmd += " ,@RevNo=" + Quotation.RevNo;
                _cmd +=
                    " ,@CustContact='" + Tool.Tool.validateStr(Quotation.CustomerContactName) + "'";
                _cmd += ", @Jobtype='" + Quotation.Jobtype + "'";
                _cmd += ", @TicketId ='" + Quotation.TicketId + "'";
                _cmd += ", @BomNo ='" + Quotation.BomNo + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
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


        [HttpGet("[action]")]
        public ActionResult getQuaHListForMobile([FromQuery] string id, [FromQuery] string user)
        {
            string _cmd;
            List<QuotationListMobile> quotationList = new List<QuotationListMobile>();

            _cmd = "exec dbo.getQuotationAll @CmpId='" + id + "', @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getQuotationItemAll @CmpId='" + id + "', @User='" + user + "'";
            DataTable datatableDetail = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getCustomer @CmpId='" + id + "' , @Type='0'";
            DataTable datacustomer = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getcmpinfo @CmpId='" + id + "'";
            DataTable datacompany = DB.DBConn.GetDataTable(_cmd);


            foreach (DataRow r in datatable.Rows)
            {
                var quotaion = new QuotationListMobile();

                quotaion.QuotationNo = r["QuotationNo"].ToString();
                quotaion.QuotationDate = r["QuotationDate"].ToString();
                quotaion.QuotationBy = r["QuotationBy"].ToString();
                quotaion.QuotationState = r["QuotationState"].ToString();
                quotaion.CustomerCode = r["CustomerCode"].ToString();
                quotaion.CustomerName = r["CustomerName"].ToString();
                quotaion.CreditType = Convert.ToInt32(r["CreditType"]);
                quotaion.CreditDate = Convert.ToInt32(r["CreditDate"]);
                quotaion.ProjectName = r["ProjectName"].ToString();
                quotaion.ReferCode = r["ReferCode"].ToString();
                quotaion.VatType = Convert.ToInt32(r["VatType"]);
                quotaion.Remark = r["Remark"].ToString();
                quotaion.Note = r["Note"].ToString();
                quotaion.QuotationAmt = Convert.ToDecimal(r["QuotationAmt"]);
                quotaion.QuotationDisPer = Convert.ToDecimal(r["QuotationDisPer"]);
                quotaion.QuotationDisAmt = Convert.ToDecimal(r["QuotationDisAmt"]);
                quotaion.QuotationNetAmt = Convert.ToDecimal(r["QuotationNetAmt"]);
                quotaion.QuotationVatAmt = Convert.ToDecimal(r["QuotationVatAmt"]);
                quotaion.QuotationGrandAmt = Convert.ToDecimal(r["QuotationGrandAmt"]);
                quotaion.QuotationGrandAmtTHB = r["QuotationGrandAmtTHB"].ToString();
                quotaion.QuotationGrandAmtENB = r["QuotationGrandAmtENB"].ToString();
                quotaion.WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]);
                quotaion.ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]);
                quotaion.CmpId = r["CmpId"].ToString();
                quotaion.DocState = r["DocState"].ToString();
                quotaion.PriceStand = r["PriceStand"].ToString();
                quotaion.PaymentDue = r["PaymentDue"].ToString();
                quotaion.Shipping = r["Shipping"].ToString();
                quotaion.RevNo = Convert.ToInt32(r["RevNo"]);
                quotaion.RevNoMax = Convert.ToInt32(r["RevNoMax"]);
                quotaion.StateApprove = Convert.ToInt32(r["StateApprove"]);

                quotaion.DateApprove = r["DateApprove"].ToString();
                quotaion.ApproveBy = r["ApproveBy"].ToString();
                quotaion.CustomerContactName = r["CustomerContactName"].ToString();
                quotaion.StateApproveToPO = Convert.ToInt32(r["StateApproveToPO"]);

                quotaion.DateApproveToPO = r["DateApproveToPO"].ToString();
                quotaion.ApproveToPOBy = r["ApproveToPOBy"].ToString();
                quotaion.JobType = r["JobType"].ToString();
                quotaion.StateSendApprove = Convert.ToInt32(r["StateSendApprove"]);
                quotaion.DateSendApprove = r["DateSendApprove"].ToString();
                quotaion.SendApproveBy = r["SendApproveBy"].ToString();
                quotaion.SignaturePath = r["SignaturePath"].ToString();
                quotaion.FullName = r["FullName"].ToString();
                quotaion.JobTypeFilter = r["JobTypeFilter"].ToString();
                quotaion.ImgPath = r["ImgPath"].ToString();
                quotaion.TicketId = r["TicketId"].ToString();
                quotaion.PhoneNo = r["PhoneNo"].ToString();
                quotaion.LineId = r["LineId"].ToString();
                quotaion.BomNo = r["BomNo"].ToString();
                quotaion.LineQRCodePath = r["LineQRCodePath"].ToString();

                quotaion.company = new cmpinfo();
                if (datacompany.Select("CmpId='" + id + "'").Length > 0)
                {

                    foreach (
                  DataRow d in datacompany.Select("CmpId='" + id + "'")
              )
                    {

                        var itemc = new cmpinfo()
                        {
                            CmpId = d["CmpId"].ToString(),
                            CmpName = d["CmpName"].ToString(),
                            CmpAddress = d["CmpAddress"].ToString(),
                            CmpTaxid = d["CmpTaxid"].ToString(),
                            CmpType = int.Parse(d["CmpType"].ToString()),
                            StateActive = int.Parse(d["StateActive"].ToString()),
                            Email = d["Email"].ToString(),
                            Fax = d["Fax"].ToString(),
                            Phone = d["Phone"].ToString(),
                            DateCreate = d["DateCreate"].ToString(),

                            DateExprie = d["DateExprie"].ToString(),
                            TelOffice = d["TelOffice"].ToString(),
                            CmpImg = d["CmpImg"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),

                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),

                            CmpBranchCode = d["CmpBranchCode"].ToString(),
                            CmpBranchName = d["CmpBranchName"].ToString(),
                            WebSite = d["WebSite"].ToString(),
                            Remark = d["Remark"].ToString(),

                            UpdUser = d["UpdUser"].ToString(),
                            DocPrefix = d["DocPrefix"].ToString(),
                            BankAccCode = d["BankAccCode"].ToString(),
                            BankAccName = d["BankAccName"].ToString(),

                            BankAccType = d["BankAccType"].ToString(),
                            BankCode = d["BankCode"].ToString(),
                            BankBranchCode = d["BankBranchCode"].ToString(),
                            LineId = d["LineId"].ToString(),

                            ColorThemeReport = d["ColorThemeReport"].ToString(),
                            FaviconUrl = d["FaviconUrl"].ToString(),
                            CmpNameEN = d["CmpNameEN"].ToString(),
                            CmpAddressEN = d["CmpAddressEN"].ToString(),

                        };
                        quotaion.company = itemc;


                    }



                }






                quotaion.customer = new Customer();
                if (datacustomer.Select("CustomerCode='" + quotaion.CustomerCode + "'").Length > 0)
                {
                    foreach (
                   DataRow d in datacustomer.Select("CustomerCode='" + quotaion.CustomerCode + "'")
               )
                    {
                        var itemc = new Customer()
                        {
                            CustomerCode = d["CustomerCode"].ToString(),
                            UpdUser = d["UpdUser"].ToString(),
                            CustomerName = d["CustomerName"].ToString(),
                            CustomerAddress = d["CustomerAddress"].ToString(),
                            CustomerTaxNo = d["CustomerTaxNo"].ToString(),
                            CustomerBranch = d["CustomerBranch"].ToString(),
                            CustomerBranchCode = d["CustomerBranchCode"].ToString(),
                            CustomerBranchName = d["CustomerBranchName"].ToString(),
                            ContactName = d["ContactName"].ToString(),
                            ContactEmail = d["ContactEmail"].ToString(),
                            ContactPhone = d["ContactPhone"].ToString(),
                            ContactName1 = d["ContactName1"].ToString(),
                            ContactEmail1 = d["ContactEmail1"].ToString(),
                            ContactPhone1 = d["ContactPhone1"].ToString(),
                            CreditDay = int.Parse(d["CreditDay"].ToString()),
                            PhoneOffice = d["PhoneOffice"].ToString(),
                            FaxOffice = d["FaxOffice"].ToString(),
                            Website = d["Website"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),
                            Remark = d["Remark"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                            ContactName2 = d["ContactName2"].ToString(),
                            ContactEmail2 = d["ContactEmail2"].ToString(),
                            ContactPhone2 = d["ContactPhone2"].ToString(),
                            ContactPosition2 = d["ContactPosition2"].ToString(),
                            ContactPosition1 = d["ContactPosition1"].ToString(),
                            ContactPosition = d["ContactPosition"].ToString(),

                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),
                            ImgPath = d["ImgPath"].ToString(),
                            CreditAccId = int.Parse(d["CreditAccId"].ToString()),
                            DebitAccId = int.Parse(d["DebitAccId"].ToString()),
                            BusinessGrpCode = d["BusinessGrpCode"].ToString(),
                            StateCustomer = d["StateCustomer"].ToString(),
                            StateVendor = d["StateVendor"].ToString(),
                        };
                        quotaion.customer = itemc;
                    }



                }

                if (
                    datatableDetail
                        .Select(
                            "QuotationNo ='"
                                + r["QuotationNo"].ToString()
                                + "'  and RevNo="
                                + Convert.ToInt32(r["RevNo"])
                        )
                        .Length > 0
                )
                {
                    quotaion.Items = new List<QuotationListItem>();
                }

                foreach (
                    DataRow d in datatableDetail.Select(
                        "QuotationNo ='"
                            + r["QuotationNo"].ToString()
                            + "'  and RevNo="
                            + Convert.ToInt32(r["RevNo"])
                    )
                )
                {
                    var item = new QuotationListItem();
                    item.QuotationNo = d["QuotationNo"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.ProdCode = d["ProdCode"].ToString();
                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.UnitCode = d["UnitCode"].ToString();

                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.Amt = Convert.ToDecimal(d["Amt"]);

                    item.DisPer = Convert.ToDecimal(d["DisPer"]);

                    item.DisAmt = Convert.ToDecimal(d["DisAmt"]);

                    item.NetAmt = Convert.ToDecimal(d["NetAmt"]);

                    item.PricePur = Convert.ToDecimal(d["PricePur"]);

                    item.CostAmt = Convert.ToDecimal(d["CostAmt"]);

                    item.ProfitAmt = Convert.ToDecimal(d["ProfitAmt"]);

                    item.RevNo = Convert.ToInt32(d["RevNo"]);

                    item.GroupCaption1 = d["GroupCaption1"].ToString();
                    item.GroupCaption2 = d["GroupCaption2"].ToString();
                    item.GroupCaption3 = d["GroupCaption3"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.GrossProfitPer = Convert.ToDecimal(d["GrossProfitPer"]);

                    item.MainProdCode = d["MainProdCode"].ToString();
                    item.MainSeq = Convert.ToInt32(d["MainSeq"]);
                    item.SeqSort = Convert.ToInt32(d["SeqSort"]);
                    quotaion.Items.Add(item);
                }

                quotationList.Add(quotaion);
            }

            return Ok(quotationList);
        }



        [HttpGet("[action]")]
        public IActionResult getInvoiceForMobile([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoice @CmpId='" + CmpId + "' , @UserName='" + User + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getInvoice_Detail_All   @CmpId='" + CmpId + "', @UpdUser='" + User + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getCustomer @CmpId='" + CmpId + "' , @Type='0'";
            DataTable datacustomer = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getcmpinfo @CmpId='" + CmpId + "'";
            DataTable datacompany = DB.DBConn.GetDataTable(_cmd);



            List<InvoiceForMobileModel> invoices = new List<InvoiceForMobileModel>();

            foreach (DataRow r in datatable.Rows)
            {
                var invoice = new InvoiceForMobileModel
                {
                    UpdUser = r["UpdUser"].ToString(),
                    InvoiceNo = r["InvoiceNo"].ToString(),
                    InvoiceDate = r["InvoiceDate"].ToString(),
                    InvoiceBy = r["InvoiceBy"].ToString(),
                    InvoiceState = r["InvoiceState"].ToString(),
                    CustomerCode = r["CustomerCode"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    CreditType = Convert.ToInt32(r["CreditType"]),
                    CreditDate = Convert.ToInt32(r["CreditDate"]),
                    ProjectName = r["ProjectName"].ToString(),
                    ReferCode = r["ReferCode"].ToString(),
                    VatType = Convert.ToInt32(r["VatType"]),
                    Remark = r["Remark"].ToString(),
                    Note = r["Note"].ToString(),
                    InvoiceAmt = Convert.ToDecimal(r["InvoiceAmt"]),
                    InvoiceDisPer = Convert.ToDecimal(r["InvoiceDisPer"]),
                    InvoiceDisAmt = Convert.ToDecimal(r["InvoiceDisAmt"]),
                    InvoiceNetAmt = Convert.ToDecimal(r["InvoiceNetAmt"]),
                    InvoiceVatAmt = Convert.ToDecimal(r["InvoiceVatAmt"]),
                    InvoiceVatPer = Convert.ToDecimal(r["InvoiceVatPer"]),
                    InvoiceGrandAmt = Convert.ToDecimal(r["InvoiceGrandAmt"]),
                    InvoiceGrandAmtTHB = r["InvoiceGrandAmtTHB"].ToString(),
                    InvoiceGrandAmtENB = r["InvoiceGrandAmtENB"].ToString(),
                    WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]),
                    ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]),
                    CmpId = r["CmpId"].ToString(),
                    DocState = Convert.ToInt32(r["DocState"]),
                    PriceStand = r["PriceStand"].ToString(),
                    PaymentDue = r["PaymentDue"].ToString(),
                    Shipping = r["Shipping"].ToString(),
                    StateApprove = Convert.ToInt32(r["StateApprove"]),
                    CustomerContactName = r["CustomerContactName"].ToString(),
                    JobType = r["JobType"].ToString(),
                    StateSendApprove = Convert.ToInt32(r["StateSendApprove"]),
                    QuotationNo = r["QuotationNo"].ToString(),
                    CustomerPONo = r["CustomerPONo"].ToString(),
                    SaleOrderNo = r["SaleOrderNo"].ToString(),
                    RevNo = Convert.ToInt32(r["RevNo"]),
                    TicketId = r["TicketId"].ToString()
                };

                invoice.company = new cmpinfo();
                if (datacompany.Select("CmpId='" + CmpId + "'").Length > 0)
                {

                    foreach (
                  DataRow d in datacompany.Select("CmpId='" + CmpId + "'")
              )
                    {

                        var itemc = new cmpinfo()
                        {
                            CmpId = d["CmpId"].ToString(),
                            CmpName = d["CmpName"].ToString(),
                            CmpAddress = d["CmpAddress"].ToString(),
                            CmpTaxid = d["CmpTaxid"].ToString(),
                            CmpType = int.Parse(d["CmpType"].ToString()),
                            StateActive = int.Parse(d["StateActive"].ToString()),
                            Email = d["Email"].ToString(),
                            Fax = d["Fax"].ToString(),
                            Phone = d["Phone"].ToString(),
                            DateCreate = d["DateCreate"].ToString(),

                            DateExprie = d["DateExprie"].ToString(),
                            TelOffice = d["TelOffice"].ToString(),
                            CmpImg = d["CmpImg"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),

                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),

                            CmpBranchCode = d["CmpBranchCode"].ToString(),
                            CmpBranchName = d["CmpBranchName"].ToString(),
                            WebSite = d["WebSite"].ToString(),
                            Remark = d["Remark"].ToString(),

                            UpdUser = d["UpdUser"].ToString(),
                            DocPrefix = d["DocPrefix"].ToString(),
                            BankAccCode = d["BankAccCode"].ToString(),
                            BankAccName = d["BankAccName"].ToString(),

                            BankAccType = d["BankAccType"].ToString(),
                            BankCode = d["BankCode"].ToString(),
                            BankBranchCode = d["BankBranchCode"].ToString(),
                            LineId = d["LineId"].ToString(),

                            ColorThemeReport = d["ColorThemeReport"].ToString(),
                            FaviconUrl = d["FaviconUrl"].ToString(),
                            CmpNameEN = d["CmpNameEN"].ToString(),
                            CmpAddressEN = d["CmpAddressEN"].ToString(),

                        };
                        invoice.company = itemc;


                    }



                }






                invoice.customer = new Customer();
                if (datacustomer.Select("CustomerCode='" + invoice.CustomerCode + "'").Length > 0)
                {
                    foreach (
                   DataRow d in datacustomer.Select("CustomerCode='" + invoice.CustomerCode + "'")
               )
                    {
                        var itemc = new Customer()
                        {
                            CustomerCode = d["CustomerCode"].ToString(),
                            UpdUser = d["UpdUser"].ToString(),
                            CustomerName = d["CustomerName"].ToString(),
                            CustomerAddress = d["CustomerAddress"].ToString(),
                            CustomerTaxNo = d["CustomerTaxNo"].ToString(),
                            CustomerBranch = d["CustomerBranch"].ToString(),
                            CustomerBranchCode = d["CustomerBranchCode"].ToString(),
                            CustomerBranchName = d["CustomerBranchName"].ToString(),
                            ContactName = d["ContactName"].ToString(),
                            ContactEmail = d["ContactEmail"].ToString(),
                            ContactPhone = d["ContactPhone"].ToString(),
                            ContactName1 = d["ContactName1"].ToString(),
                            ContactEmail1 = d["ContactEmail1"].ToString(),
                            ContactPhone1 = d["ContactPhone1"].ToString(),
                            CreditDay = int.Parse(d["CreditDay"].ToString()),
                            PhoneOffice = d["PhoneOffice"].ToString(),
                            FaxOffice = d["FaxOffice"].ToString(),
                            Website = d["Website"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),
                            Remark = d["Remark"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                            ContactName2 = d["ContactName2"].ToString(),
                            ContactEmail2 = d["ContactEmail2"].ToString(),
                            ContactPhone2 = d["ContactPhone2"].ToString(),
                            ContactPosition2 = d["ContactPosition2"].ToString(),
                            ContactPosition1 = d["ContactPosition1"].ToString(),
                            ContactPosition = d["ContactPosition"].ToString(),

                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),
                            ImgPath = d["ImgPath"].ToString(),
                            CreditAccId = int.Parse(d["CreditAccId"].ToString()),
                            DebitAccId = int.Parse(d["DebitAccId"].ToString()),
                            BusinessGrpCode = d["BusinessGrpCode"].ToString(),
                            StateCustomer = d["StateCustomer"].ToString(),
                            StateVendor = d["StateVendor"].ToString(),
                        };
                        invoice.customer = itemc;
                    }



                }



                invoice.items = new List<Invoice_detail>();


                foreach (DataRow x in dtItem.Select("InvoiceNo='" + r["InvoiceNo"].ToString() + "'"))
                {

                    var item = new Invoice_detail
                    {
                        UpdUser = x["UpdUser"].ToString(),
                        InvoiceNo = x["InvoiceNo"].ToString(),
                        Seq = Convert.ToInt32(x["Seq"]),
                        ProdCode = x["ProdCode"].ToString(),
                        ProdDescription = x["ProdDescription"].ToString(),
                        Qty = Convert.ToDecimal(x["Qty"]),
                        UnitCode = x["UnitCode"].ToString(),
                        UnitPrice = Convert.ToDecimal(x["UnitPrice"]),
                        Amt = Convert.ToDecimal(x["Amt"]),
                        DisPer = Convert.ToDecimal(x["DisPer"]),
                        DisAmt = Convert.ToDecimal(x["DisAmt"]),
                        NetAmt = Convert.ToDecimal(x["NetAmt"]),
                        PricePur = Convert.ToDecimal(x["PricePur"]),
                        CostAmt = Convert.ToDecimal(x["CostAmt"]),
                        ProfitAmt = Convert.ToDecimal(x["ProfitAmt"]),
                        GroupCaption1 = x["GroupCaption1"].ToString(),
                        GroupCaption2 = x["GroupCaption2"].ToString(),
                        GroupCaption3 = x["GroupCaption3"].ToString(),
                        CmpId = x["CmpId"].ToString(),
                        RevNo = Convert.ToInt32(r["RevNo"])
                    };

                    invoice.items.Add(item);

                }


                invoices.Add(invoice);
            }


            return Ok(invoices);
        }

   [HttpGet("[action]")]
        public IActionResult getReceiveForMobile([FromQuery] string CmpId, [FromQuery] string User)
        {
            string _cmd;
            _cmd = "exec dbo.getInvoiceReceive @CmpId='" + CmpId + "' , @UserName='" + User + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getInvoiceReceive_Detail_All   @CmpId='" + CmpId + "', @UpdUser='" + User + "'";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getCustomer @CmpId='" + CmpId + "' , @Type='0'";
            DataTable datacustomer = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.getcmpinfo @CmpId='" + CmpId + "'";
            DataTable datacompany = DB.DBConn.GetDataTable(_cmd);



            List<InvoiceReceiveForMobileModel> invoices = new List<InvoiceReceiveForMobileModel>();

            foreach (DataRow r in datatable.Rows)
            {
                var invoice = new InvoiceReceiveForMobileModel
                {
                    UpdUser = r["UpdUser"].ToString(),
                    InvoiceNo = r["InvoiceNo"].ToString(),
                    ReceiveNo = r["ReceiveNo"].ToString(),
                    ReceiveDate = r["ReceiveDate"].ToString(),
                    ReceiveBy = r["ReceiveBy"].ToString(),
                    ReceiveState = r["ReceiveState"].ToString(),
                    CustomerCode = r["CustomerCode"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    CreditType = Convert.ToInt32(r["CreditType"]),
                    CreditDate = Convert.ToInt32(r["CreditDate"]),
                    ProjectName = r["ProjectName"].ToString(),
                    ReferCode = r["ReferCode"].ToString(),
                    VatType = Convert.ToInt32(r["VatType"]),
                    Remark = r["Remark"].ToString(),
                    Note = r["Note"].ToString(),
                    ReceiveAmt = Convert.ToDecimal(r["ReceiveAmt"]),
                    ReceiveDisPer = Convert.ToDecimal(r["ReceiveDisPer"]),
                    ReceiveDisAmt = Convert.ToDecimal(r["ReceiveDisAmt"]),
                    ReceiveNetAmt = Convert.ToDecimal(r["ReceiveNetAmt"]),
                    ReceiveVatAmt = Convert.ToDecimal(r["ReceiveVatAmt"]),
                    ReceiveVatPer = Convert.ToDecimal(r["ReceiveVatPer"]),
                    ReceiveGrandAmt = Convert.ToDecimal(r["ReceiveGrandAmt"]),
                    ReceiveGrandAmtTHB = r["ReceiveGrandAmtTHB"].ToString(),
                    ReceiveGrandAmtENB = r["ReceiveGrandAmtENB"].ToString(),
                    WithholdingTaxState = Convert.ToInt32(r["WithholdingTaxState"]),
                    ShowSignatureState = Convert.ToInt32(r["ShowSignatureState"]),
                    CmpId = r["CmpId"].ToString(),
                    DocState = Convert.ToInt32(r["DocState"]),
                    PriceStand = r["PriceStand"].ToString(),
                    PaymentDue = r["PaymentDue"].ToString(),
                    Shipping = r["Shipping"].ToString(),
                    StateApprove = Convert.ToInt32(r["StateApprove"]),
                    CustomerContactName = r["CustomerContactName"].ToString(),
                    JobType = r["JobType"].ToString(),
                    StateSendApprove = Convert.ToInt32(r["StateSendApprove"]),
                    QuotationNo = r["QuotationNo"].ToString(),
                    CustomerPONo = r["CustomerPONo"].ToString(),
                    SaleOrderNo = r["SaleOrderNo"].ToString(),
                    RevNo = Convert.ToInt32(r["RevNo"]),
                    TicketId = r["TicketId"].ToString()
                };

                invoice.company = new cmpinfo();
                if (datacompany.Select("CmpId='" + CmpId + "'").Length > 0)
                {

                    foreach (
                  DataRow d in datacompany.Select("CmpId='" + CmpId + "'")
              )
                    {

                        var itemc = new cmpinfo()
                        {
                            CmpId = d["CmpId"].ToString(),
                            CmpName = d["CmpName"].ToString(),
                            CmpAddress = d["CmpAddress"].ToString(),
                            CmpTaxid = d["CmpTaxid"].ToString(),
                            CmpType = int.Parse(d["CmpType"].ToString()),
                            StateActive = int.Parse(d["StateActive"].ToString()),
                            Email = d["Email"].ToString(),
                            Fax = d["Fax"].ToString(),
                            Phone = d["Phone"].ToString(),
                            DateCreate = d["DateCreate"].ToString(),

                            DateExprie = d["DateExprie"].ToString(),
                            TelOffice = d["TelOffice"].ToString(),
                            CmpImg = d["CmpImg"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),

                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),

                            CmpBranchCode = d["CmpBranchCode"].ToString(),
                            CmpBranchName = d["CmpBranchName"].ToString(),
                            WebSite = d["WebSite"].ToString(),
                            Remark = d["Remark"].ToString(),

                            UpdUser = d["UpdUser"].ToString(),
                            DocPrefix = d["DocPrefix"].ToString(),
                            BankAccCode = d["BankAccCode"].ToString(),
                            BankAccName = d["BankAccName"].ToString(),

                            BankAccType = d["BankAccType"].ToString(),
                            BankCode = d["BankCode"].ToString(),
                            BankBranchCode = d["BankBranchCode"].ToString(),
                            LineId = d["LineId"].ToString(),

                            ColorThemeReport = d["ColorThemeReport"].ToString(),
                            FaviconUrl = d["FaviconUrl"].ToString(),
                            CmpNameEN = d["CmpNameEN"].ToString(),
                            CmpAddressEN = d["CmpAddressEN"].ToString(),

                        };
                        invoice.company = itemc;


                    }



                }






                invoice.customer = new Customer();
                if (datacustomer.Select("CustomerCode='" + invoice.CustomerCode + "'").Length > 0)
                {
                    foreach (
                   DataRow d in datacustomer.Select("CustomerCode='" + invoice.CustomerCode + "'")
               )
                    {
                        var itemc = new Customer()
                        {
                            CustomerCode = d["CustomerCode"].ToString(),
                            UpdUser = d["UpdUser"].ToString(),
                            CustomerName = d["CustomerName"].ToString(),
                            CustomerAddress = d["CustomerAddress"].ToString(),
                            CustomerTaxNo = d["CustomerTaxNo"].ToString(),
                            CustomerBranch = d["CustomerBranch"].ToString(),
                            CustomerBranchCode = d["CustomerBranchCode"].ToString(),
                            CustomerBranchName = d["CustomerBranchName"].ToString(),
                            ContactName = d["ContactName"].ToString(),
                            ContactEmail = d["ContactEmail"].ToString(),
                            ContactPhone = d["ContactPhone"].ToString(),
                            ContactName1 = d["ContactName1"].ToString(),
                            ContactEmail1 = d["ContactEmail1"].ToString(),
                            ContactPhone1 = d["ContactPhone1"].ToString(),
                            CreditDay = int.Parse(d["CreditDay"].ToString()),
                            PhoneOffice = d["PhoneOffice"].ToString(),
                            FaxOffice = d["FaxOffice"].ToString(),
                            Website = d["Website"].ToString(),
                            AddressShip = d["AddressShip"].ToString(),
                            Remark = d["Remark"].ToString(),
                            CmpId = d["CmpId"].ToString(),
                            ContactName2 = d["ContactName2"].ToString(),
                            ContactEmail2 = d["ContactEmail2"].ToString(),
                            ContactPhone2 = d["ContactPhone2"].ToString(),
                            ContactPosition2 = d["ContactPosition2"].ToString(),
                            ContactPosition1 = d["ContactPosition1"].ToString(),
                            ContactPosition = d["ContactPosition"].ToString(),

                            AddrSubDistrict = d["AddrSubDistrict"].ToString(),
                            AddrDistrict = d["AddrDistrict"].ToString(),
                            AddrProvince = d["AddrProvince"].ToString(),
                            AddrPostCode = d["AddrPostCode"].ToString(),
                            ImgPath = d["ImgPath"].ToString(),
                            CreditAccId = int.Parse(d["CreditAccId"].ToString()),
                            DebitAccId = int.Parse(d["DebitAccId"].ToString()),
                            BusinessGrpCode = d["BusinessGrpCode"].ToString(),
                            StateCustomer = d["StateCustomer"].ToString(),
                            StateVendor = d["StateVendor"].ToString(),
                        };
                        invoice.customer = itemc;
                    }



                }



                invoice.items = new List<InvoiceReceive_detail>();


                foreach (DataRow x in dtItem.Select("ReceiveNo='" + r["ReceiveNo"].ToString() + "'"))
                {

                    var item = new InvoiceReceive_detail
                    {
                        UpdUser = x["UpdUser"].ToString(),
                        ReceiveNo = x["ReceiveNo"].ToString(),
                        Seq = Convert.ToInt32(x["Seq"]),
                        ProdCode = x["ProdCode"].ToString(),
                        ProdDescription = x["ProdDescription"].ToString(),
                        Qty = Convert.ToDecimal(x["Qty"]),
                        UnitCode = x["UnitCode"].ToString(),
                        UnitPrice = Convert.ToDecimal(x["UnitPrice"]),
                        Amt = Convert.ToDecimal(x["Amt"]),
                        DisPer = Convert.ToDecimal(x["DisPer"]),
                        DisAmt = Convert.ToDecimal(x["DisAmt"]),
                        NetAmt = Convert.ToDecimal(x["NetAmt"]),
                        PricePur = Convert.ToDecimal(x["PricePur"]),
                        CostAmt = Convert.ToDecimal(x["CostAmt"]),
                        ProfitAmt = Convert.ToDecimal(x["ProfitAmt"]),
                        GroupCaption1 = x["GroupCaption1"].ToString(),
                        GroupCaption2 = x["GroupCaption2"].ToString(),
                        GroupCaption3 = x["GroupCaption3"].ToString(),
                        CmpId = x["CmpId"].ToString(),
                        RevNo = Convert.ToInt32(r["RevNo"])
                    };

                    invoice.items.Add(item);

                }


                invoices.Add(invoice);
            }


            return Ok(invoices);
        }




    }
}
