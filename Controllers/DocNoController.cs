using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class DocNoController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult GetDocNo(
            [FromQuery] string cmpid,
            [FromQuery] string DocNo,
            [FromQuery] string type
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _docnew = "";
            string _cmd;
 
            if (cmpid == "230015")
            {
                switch (type)
                {
                    case "quo":
                        _cmd =
                            "Select Top 1  QuotationNo  as FTDocNo FROM  mdb.Quotation  where   CmpId ="
                            + cmpid
                            + " and  QuotationNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.quorun   set @Runno  = 'QT-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;
                    case "saleman":

                        _cmd =
                            "Select Top 1  SalemanTrackNo  FROM mdb.[SalemanTrack]  where   SalemanTrackNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo. Salemanrun      select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;
                    case "leads":

                        _cmd =
                            "Select Top 1  CustCodeNo  FROM mdb.[Leads]  where   CustCodeNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.LeadsRun      select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "adjust":

                        _cmd =
                            "Select Top 1  AdjustNo  FROM Inven.[Adjust]  where  AdjustNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.adjrun      select 'AD-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "pur":

                        _cmd =
                            "Select Top 1  PurchaseNo  FROM  pur.Purchase  where  PurchaseNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.porun      select 'PO-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "project":
                        _cmd =
                            "Select Top 1  ProjectNo  FROM  dbo.Project  where  ProjectNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.projectrun      select 'NIS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "rcv":
                        _cmd =
                            "Select Top 1  ReceiveNo  FROM  Inven.Receive  where  ReceiveNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rcvrun      select 'RC-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "iss":
                        _cmd =
                            "Select Top 1  IssueNo  FROM  Inven.Issue  where  IssueNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.issrun      select 'IS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "rts":
                        _cmd =
                            "Select Top 1  ReturnToSuplNo  FROM  Inven.ReturnToSupl  where  ReturnToSuplNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rtsrun      select 'RS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "rtc":
                        _cmd =
                            "Select Top 1  ReturnToStockNo  FROM  Inven.ReturnToStock  where  ReturnToStockNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.Rtcrun      select 'RT-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "rsv":
                        _cmd =
                            "Select Top 1  ReserveNo  FROM  Inven.Reserve where  ReserveNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[RsvRun]      select 'RV-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "trw":
                        _cmd =
                            "Select Top 1  TransferWHNo  FROM  Inven.TransferWH where  TransferWHNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRun]      select 'TW-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "trwrcv":
                        _cmd =
                            "Select Top 1  TransferWHNo  FROM  Inven.TransferWHRcv where  TransferWHNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRunrcv]      select 'TWR-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "cust":
                        _cmd =
                            "Select Top 1  CustomerCode  FROM  msb.mCustomer where  CustomerCode='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[custrun]      select   @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "jobtype":
                        _cmd =
                            "Select Top 1  JobTypeCode  FROM  msb.mJobType where  JobTypeCode='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[jobtyperun]      select   @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "bom":
                        _cmd =
                            "Select Top 1  BomNo  as FTDocNo FROM  dbo.salesbom  where   CmpId ="
                            + cmpid
                            + " and  BomNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.bomrun   set @Runno  = 'BM-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "vendor":
                        _cmd =
                            "Select Top 1  SupplierCode  as FTDocNo FROM  [msb].[mSupplier]  where   CmpId ="
                            + cmpid
                            + " and  SupplierCode='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.vendorrun   set @Runno  = 'VD-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "saleorder":
                        _cmd =
                            "Select Top 1  SaleOrderNo  as FTDocNo FROM  mdb.SaleOrder  where   CmpId ="
                            + cmpid
                            + " and  SaleOrderNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.saleorderrun   set @Runno  = 'SO-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmgrp":
                        _cmd =
                            "Select Top 1  grpid  as FTDocNo FROM  dbo.crmgrp  where   CmpId ='"
                            + cmpid
                            + "' and  grpid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmgrprun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmtask":
                        _cmd =
                            "Select Top 1  taskid  as FTDocNo FROM  dbo.crmtask  where   CmpId ='"
                            + cmpid
                            + "' and  taskid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmtaskrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmcomment":
                        _cmd =
                            "Select Top 1  CommentId  as FTDocNo FROM  dbo.CRMComment  where   CmpId ='"
                            + cmpid
                            + "' and  CommentId='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmcommentrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "invoice":
                        _cmd =
                            "Select Top 1  InvoiceNo  as FTDocNo FROM  dbo.Invoice  where   CmpId ='"
                            + cmpid
                            + "' and  InvoiceNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.invoicerun   set @Runno  = 'IV-'+@Runno  select @Runno    "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "araccrcv":
                        _cmd =
                            "Select Top 1  RchDocNo  as FTDocNo FROM  acc.TARTReciveInv_H  where   CmpId ='"
                            + cmpid
                            + "' and  RchDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accaracvrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "saleteamrun":
                        _cmd =
                            "Select Top 1  SaleTeamId  as FTDocNo FROM  dbo.SystemSaleTeam  where   CmpId ='"
                            + cmpid
                            + "' and  SaleTeamId='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.systemsaleteamrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "aracccredit":
                        _cmd =
                            "Select Top 1  CnhDocNo  as FTDocNo FROM  acc.TARTCreditNote_H  where   CmpId ='"
                            + cmpid
                            + "' and  CnhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accarcreditrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "araccbilling":
                        _cmd =
                            "Select Top 1  BlhDocNo  as FTDocNo FROM  acc.TARTBillingSlips_H  where   CmpId ='"
                            + cmpid
                            + "' and  BlhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accarbillingrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "magrp":
                        _cmd =
                            "Select Top 1  grpid  as FTDocNo FROM  dbo.mataskgrp  where   CmpId ='"
                            + cmpid
                            + "' and  grpid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.mataskgrprun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "apentry":

                        _cmd =
                            "Select Top 1  PayableNo  as FTDocNo FROM  acc.TAPPayables_H  where   CmpId ='"
                            + cmpid
                            + "' and  PayableNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accentry   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "apcredit":

                        _cmd =
                            "Select Top 1  CnhDocNo  as FTDocNo FROM  acc.TAPCreditNote_H  where   CmpId ='"
                            + cmpid
                            + "' and  CnhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.apcredit   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "roleid":

                        _cmd =
                            "declare @Runno int  Select  @Runno =NEXT VALUE FOR  dbo.roleid   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case "quo":
                        _cmd =
                            "Select Top 1  QuotationNo  as FTDocNo FROM  mdb.Quotation  where   CmpId ="
                            + cmpid
                            + " and  QuotationNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-quorun]   set @Runno  = 'QT-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;
                    case "saleman":

                        _cmd =
                            "Select Top 1  SalemanTrackNo  FROM mdb.[SalemanTrack]  where   SalemanTrackNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-Salemanrun]      select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;
                    case "leads":

                        _cmd =
                            "Select Top 1  CustCodeNo  FROM mdb.[Leads]  where   CustCodeNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-LeadsRun]     select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "adjust":

                        _cmd =
                            "Select Top 1  AdjustNo  FROM Inven.[Adjust]  where  AdjustNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-adjrun]      select 'AD-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "pur":

                        _cmd =
                            "Select Top 1  PurchaseNo  FROM  pur.Purchase  where  PurchaseNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-porun]     select 'PO-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "project":
                        _cmd =
                            "Select Top 1  ProjectNo  FROM  dbo.Project  where  ProjectNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo."
                                + cmpid
                                + "-projectrun      select 'NIS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "rcv":
                        _cmd =
                            "Select Top 1  ReceiveNo  FROM  Inven.Receive  where  ReceiveNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo."
                                + cmpid
                                + "-rcvrun      select 'RC-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "iss":
                        _cmd =
                            "Select Top 1  IssueNo  FROM  Inven.Issue  where  IssueNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-issrun]      select 'IS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "rts":
                        _cmd =
                            "Select Top 1  ReturnToSuplNo  FROM  Inven.ReturnToSupl  where  ReturnToSuplNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-rtsrun]      select 'RS-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "rtc":
                        _cmd =
                            "Select Top 1  ReturnToStockNo  FROM  Inven.ReturnToStock  where  ReturnToStockNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-Rtcrun]     select 'RT-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "rsv":
                        _cmd =
                            "Select Top 1  ReserveNo  FROM  Inven.Reserve where  ReserveNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-RsvRun]     select 'RV-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "trw":
                        _cmd =
                            "Select Top 1  TransferWHNo  FROM  Inven.TransferWH where  TransferWHNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-trwRun]     select 'TW-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "trwrcv":
                        _cmd =
                            "Select Top 1  TransferWHNo  FROM  Inven.TransferWHRcv where  TransferWHNo='"
                            + DocNo
                            + "' and  CmpId ='"
                            + cmpid
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-trwRunrcv]     select 'TWR-'+@Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "cust":
                        _cmd =
                            "Select Top 1  CustomerCode  FROM  msb.mCustomer where  CustomerCode='"
                            + DocNo
                            + "'  and   CmpId ='"
                            + cmpid
                            + "' ";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-custrun]      select   @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;
                    case "jobtype":
                        _cmd =
                            "Select Top 1  JobTypeCode  FROM  msb.mJobType where  JobTypeCode='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-jobtyperun]     select   @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }
                        break;

                    case "bom":
                        _cmd =
                            "Select Top 1  BomNo  as FTDocNo FROM  dbo.salesbom  where   CmpId ='"
                            + cmpid
                            + "' and  BomNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-bomrun]   set @Runno  = 'BM-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "vendor":
                        _cmd =
                            "Select Top 1  SupplierCode  as FTDocNo FROM  [msb].[mSupplier]  where   CmpId ="
                            + cmpid
                            + " and  SupplierCode='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-vendorrun]   set @Runno  = 'VD-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "saleorder":
                        _cmd =
                            "Select Top 1  SaleOrderNo  as FTDocNo FROM  mdb.SaleOrder  where   CmpId ="
                            + cmpid
                            + " and  SaleOrderNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-saleorderrun]   set @Runno  = 'SO-'+@Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmgrp":
                        _cmd =
                            "Select Top 1  grpid  as FTDocNo FROM  dbo.crmgrp  where   CmpId ='"
                            + cmpid
                            + "' and  grpid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-crmgrprun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmtask":
                        _cmd =
                            "Select Top 1  taskid  as FTDocNo FROM  dbo.crmtask  where   CmpId ='"
                            + cmpid
                            + "' and  taskid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-crmtaskrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "crmcomment":
                        _cmd =
                            "Select Top 1  CommentId  as FTDocNo FROM  dbo.CRMComment  where   CmpId ='"
                            + cmpid
                            + "' and  CommentId='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-crmcommentrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "invoice":
                        _cmd =
                            "Select Top 1  InvoiceNo  as FTDocNo FROM  dbo.Invoice  where   CmpId ='"
                            + cmpid
                            + "' and  InvoiceNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-invoicerun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "araccrcv":
                        _cmd =
                            "Select Top 1  RchDocNo  as FTDocNo FROM  acc.TARTReciveInv_H  where   CmpId ='"
                            + cmpid
                            + "' and  RchDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-accaracvrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "saleteamrun":
                        _cmd =
                            "Select Top 1  SaleTeamId  as FTDocNo FROM  dbo.SystemSaleTeam  where   CmpId ='"
                            + cmpid
                            + "' and  SaleTeamId='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-systemsaleteamrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "aracccredit":
                        _cmd =
                            "Select Top 1  CnhDocNo  as FTDocNo FROM  acc.TARTCreditNote_H  where   CmpId ='"
                            + cmpid
                            + "' and  CnhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-accarcreditrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "araccbilling":
                        _cmd =
                            "Select Top 1  BlhDocNo  as FTDocNo FROM  acc.TARTBillingSlips_H  where   CmpId ='"
                            + cmpid
                            + "' and  BlhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-accarbillingrun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "magrp":
                        _cmd =
                            "Select Top 1  grpid  as FTDocNo FROM  dbo.mataskgrp  where   CmpId ='"
                            + cmpid
                            + "' and  grpid='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-mataskgrprun]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "apentry":

                        _cmd =
                            "Select Top 1  PayableNo  as FTDocNo FROM  acc.TAPPayables_H  where   CmpId ='"
                            + cmpid
                            + "' and  PayableNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.["
                                + cmpid
                                + "-accentry]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;

                    case "apcredit":

                        _cmd =
                            "Select Top 1  CnhDocNo  as FTDocNo FROM  acc.TAPCreditNote_H  where   CmpId ='"
                            + cmpid
                            + "' and  CnhDocNo='"
                            + DocNo
                            + "'";
                        dt = DB.DBConn.GetDataTable(_cmd);
                        if (dt.Rows.Count > 0)
                        {
                            try
                            {
                                _docnew = dt.Rows[0][0].ToString();
                            }
                            catch
                            {
                                _docnew = "";
                            }
                        }

                        if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                        {
                            _cmd =
                                "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR dbo.["
                                + cmpid
                                + "-apcredit]   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                            dt = DB.DBConn.GetDataTable(_cmd);
                        }

                        break;
                    case "roleid":

                        _cmd =
                            "declare @Runno int  Select  @Runno =NEXT VALUE FOR  dbo.roleid   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                        break;
                    default:
                        break;
                }
            }

            return Ok(dt.Rows[0][0]);
        }

        [HttpGet("[action]")]
        public IActionResult GetDocNo_Stl(
            [FromQuery] string cmpid,
            [FromQuery] string DocNo,
            [FromQuery] string type
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _docnew = "";
            string _cmd;
            switch (type)
            {
                case "quo":
                    _cmd =
                        "Select Top 1  QuotationNo  as FTDocNo FROM  mdb.Quotation  where   CmpId ="
                        + cmpid
                        + " and  QuotationNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.quorun   set @Runno  = 'QT-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;
                case "saleman":

                    _cmd =
                        "Select Top 1  SalemanTrackNo  FROM mdb.[SalemanTrack]  where   SalemanTrackNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo. Salemanrun      select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;
                case "leads":

                    _cmd =
                        "Select Top 1  CustCodeNo  FROM mdb.[Leads]  where   CustCodeNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.LeadsRun      select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "adjust":

                    _cmd =
                        "Select Top 1  AdjustNo  FROM Inven.[Adjust]  where  AdjustNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.adjrun      select 'AD-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "pur":

                    _cmd =
                        "Select Top 1  PurchaseNo  FROM  pur.Purchase  where  PurchaseNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.porun      select 'PO-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;

                case "project":
                    _cmd =
                        "Select Top 1  ProjectNo  FROM  dbo.Project  where  ProjectNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.projectrun      select 'NIS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;

                case "rcv":
                    _cmd =
                        "Select Top 1  ReceiveNo  FROM  Inven.Receive  where  ReceiveNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rcvrun      select 'RC-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "iss":
                    _cmd =
                        "Select Top 1  IssueNo  FROM  Inven.Issue  where  IssueNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.issrun      select 'IS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;

                case "rts":
                    _cmd =
                        "Select Top 1  ReturnToSuplNo  FROM  Inven.ReturnToSupl  where  ReturnToSuplNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rtsrun      select 'RS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "rtc":
                    _cmd =
                        "Select Top 1  ReturnToStockNo  FROM  Inven.ReturnToStock  where  ReturnToStockNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.Rtcrun      select 'RT-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "rsv":
                    _cmd =
                        "Select Top 1  ReserveNo  FROM  Inven.Reserve where  ReserveNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[RsvRun]      select 'RV-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "trw":
                    _cmd =
                        "Select Top 1  TransferWHNo  FROM  Inven.TransferWH where  TransferWHNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRun]      select 'TW-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "trwrcv":
                    _cmd =
                        "Select Top 1  TransferWHNo  FROM  Inven.TransferWHRcv where  TransferWHNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRunrcv]      select 'TWR-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "cust":
                    _cmd =
                        "Select Top 1  CustomerCode  FROM  msb.mCustomer where  CustomerCode='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[custrun]      select   @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;
                case "jobtype":
                    _cmd =
                        "Select Top 1  JobTypeCode  FROM  msb.mJobType where  JobTypeCode='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[jobtyperun]      select   @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }
                    break;

                case "bom":
                    _cmd =
                        "Select Top 1  BomNo  as FTDocNo FROM  dbo.salesbom  where   CmpId ="
                        + cmpid
                        + " and  BomNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.bomrun   set @Runno  = 'BM-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "vendor":
                    _cmd =
                        "Select Top 1  SupplierCode  as FTDocNo FROM  [msb].[mSupplier]  where   CmpId ="
                        + cmpid
                        + " and  SupplierCode='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.vendorrun   set @Runno  = 'VD-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "saleorder":
                    _cmd =
                        "Select Top 1  SaleOrderNo  as FTDocNo FROM  mdb.SaleOrder  where   CmpId ="
                        + cmpid
                        + " and  SaleOrderNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.saleorderrun   set @Runno  = 'SO-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "crmgrp":
                    _cmd =
                        "Select Top 1  grpid  as FTDocNo FROM  dbo.crmgrp  where   CmpId ='"
                        + cmpid
                        + "' and  grpid='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmgrprun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "crmtask":
                    _cmd =
                        "Select Top 1  taskid  as FTDocNo FROM  dbo.crmtask  where   CmpId ='"
                        + cmpid
                        + "' and  taskid='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmtaskrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "crmcomment":
                    _cmd =
                        "Select Top 1  CommentId  as FTDocNo FROM  dbo.CRMComment  where   CmpId ='"
                        + cmpid
                        + "' and  CommentId='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.crmcommentrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "invoice":
                    _cmd =
                        "Select Top 1  InvoiceNo  as FTDocNo FROM  dbo.Invoice  where   CmpId ='"
                        + cmpid
                        + "' and  InvoiceNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.invoicerun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "araccrcv":
                    _cmd =
                        "Select Top 1  RchDocNo  as FTDocNo FROM  acc.TARTReciveInv_H  where   CmpId ='"
                        + cmpid
                        + "' and  RchDocNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accaracvrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "saleteamrun":
                    _cmd =
                        "Select Top 1  SaleTeamId  as FTDocNo FROM  dbo.SystemSaleTeam  where   CmpId ='"
                        + cmpid
                        + "' and  SaleTeamId='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.systemsaleteamrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "aracccredit":
                    _cmd =
                        "Select Top 1  CnhDocNo  as FTDocNo FROM  acc.TARTCreditNote_H  where   CmpId ='"
                        + cmpid
                        + "' and  CnhDocNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accarcreditrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "araccbilling":
                    _cmd =
                        "Select Top 1  BlhDocNo  as FTDocNo FROM  acc.TARTBillingSlips_H  where   CmpId ='"
                        + cmpid
                        + "' and  BlhDocNo='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.accarbillingrun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                case "magrp":
                    _cmd =
                        "Select Top 1  grpid  as FTDocNo FROM  dbo.mataskgrp  where   CmpId ='"
                        + cmpid
                        + "' and  grpid='"
                        + DocNo
                        + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try
                        {
                            _docnew = dt.Rows[0][0].ToString();
                        }
                        catch
                        {
                            _docnew = "";
                        }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd =
                            "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.mataskgrprun   set @Runno  =  @Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);
                    }

                    break;

                default:
                    break;
            }

            return Ok(dt.Rows[0][0]);
        }
    }
}
