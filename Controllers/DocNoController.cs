using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{ 
    public class DocNoController : ApiController
    {
        // GET: api/DocNo
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/DocNo/5
        public IHttpActionResult Get(int cmpid, string DocNo , string type)
        {
            DataTable dt = new System.Data.DataTable();
            string _docnew = "";
            string _cmd;
            switch (type)
            {
                case "qua":
                    _cmd = "Select Top 1  QuatationNo  as FTDocNo FROM  mdb.Quatation  where   CmpId =" + cmpid + " and  QuatationNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.quarun   set @Runno  = 'QT-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }

                    break;
                case "saleman":
                    
                    _cmd = "Select Top 1  SalemanTrackNo  FROM mdb.[SalemanTrack]  where   SalemanTrackNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo. Salemanrun      select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }

                    break;
                case "leads":

                    _cmd = "Select Top 1  CustCodeNo  FROM mdb.[Leads]  where   CustCodeNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }

                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.LeadsRun      select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "adjust":

                    _cmd = "Select Top 1  AdjustNo  FROM Inven.[Adjust]  where  AdjustNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.adjrun      select 'AD-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "pur":

                    _cmd = "Select Top 1  PurchaseNo  FROM  pur.Purchase  where  PurchaseNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.porun      select 'PO-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;

                case "project":
                    _cmd = "Select Top 1  ProjectNo  FROM  dbo.Project  where  ProjectNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.projectrun      select 'NIS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;

                case "rcv":
                    _cmd = "Select Top 1  ReceiveNo  FROM  Inven.Receive  where  ReceiveNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rcvrun      select 'RC-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "iss":
                    _cmd = "Select Top 1  IssueNo  FROM  Inven.Issue  where  IssueNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.issrun      select 'IS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;

                case "rts":
                    _cmd = "Select Top 1  ReturnToSuplNo  FROM  Inven.ReturnToSupl  where  ReturnToSuplNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.rtsrun      select 'RS-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "rtc":
                    _cmd = "Select Top 1  ReturnToStockNo  FROM  Inven.ReturnToStock  where  ReturnToStockNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.Rtcrun      select 'RT-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "rsv":
                    _cmd = "Select Top 1  ReserveNo  FROM  Inven.Reserve where  ReserveNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[RsvRun]      select 'RV-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "trw":
                    _cmd = "Select Top 1  TransferWHNo  FROM  Inven.TransferWH where  TransferWHNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRun]      select 'TW-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "trwrcv":
                    _cmd = "Select Top 1  TransferWHNo  FROM  Inven.TransferWHRcv where  TransferWHNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[trwRunrcv]      select 'TWR-'+@Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "cust":
                    _cmd = "Select Top 1  CustomerCode  FROM  msb.mCustomer where  CustomerCode='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[custrun]      select   @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;
                case "jobtype":
                    _cmd = "Select Top 1  JobTypeCode  FROM  msb.mJobType where  JobTypeCode='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.[jobtyperun]      select   @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }
                    break;

                case "bom":
                    _cmd = "Select Top 1  BomNo  as FTDocNo FROM  dbo.salesbom  where   CmpId =" + cmpid + " and  BomNo='" + DocNo + "'";
                    dt = DB.DBConn.GetDataTable(_cmd);
                    if (dt.Rows.Count > 0)
                    {
                        try { _docnew = dt.Rows[0][0].ToString(); } catch { _docnew = ""; }
                    }


                    if ((_docnew.ToString() == "") || (_docnew.ToLower() == "null"))
                    {
                        _cmd = "declare @Runno varchar(30)  Select  @Runno =NEXT VALUE FOR  dbo.bomrun   set @Runno  = 'BM-'+@Runno  select @Runno   "; // + cmpid  ;
                        dt = DB.DBConn.GetDataTable(_cmd);

                    }

                    break;

                default:
                    break;
                    
            }
            

            return Ok(dt.Rows[0][0]);
        }
        // POST: api/DocNo
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/DocNo/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/DocNo/5
        public void Delete(int id)
        {
        }
    }
}
