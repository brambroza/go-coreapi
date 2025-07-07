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

    public class InvenIssController : ControllerBase
    { 

        [HttpGet("[action]")]
        public IActionResult getInvenIss([FromQuery] string CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getIssAll @CmpId='" +  CmpId  + "' , @User='" + userlogin + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd); 
            _cmd = "exec dbo.[Inven_getTransAll] @CmpId='" +  CmpId  + "' , @User='" + userlogin + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<IssueModel> issues = new List<IssueModel>();

            foreach (DataRow r in dt.Rows)
            {
                var issue = new IssueModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    IssueNo = r["IssueNo"].ToString(),
                    IssueDate = r["IssueDate"].ToString(),
                    IssueBy = r["IssueBy"].ToString(),
                    DocRef = r["DocRef"].ToString(),
                    ProjectNo = r["ProjectNo"].ToString(),
                   
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),
                    
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),
                    
                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                   
                    CustomerName = r["CustomerName"].ToString(),
                    ReferCode = r["ReferCode"].ToString(),
                };

                issue.items = new List<InvenTransModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["IssueNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new InvenTransModel();
                    item.DocNo = d["DocNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.TransDate = d["TransDate"].ToString();
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);
                    item.BarcodeNo = d["BarcodeNo"].ToString();

                    item.ProductCode = d["ProductCode"].ToString();
                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.UnitCode = d["UnitCode"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.PurchaseNo = d["PurchaseNo"].ToString();

                    item.StateReserve = Convert.ToInt32(d["StateReserve"]);

                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.BatchNo = d["BatchNo"].ToString();
                    item.Grade = d["Grade"].ToString();
                    item.DateExpire = d["DateExpire"].ToString();

                    item.StateQC = Convert.ToInt32(d["StateQC"]);

                    item.QCBy = d["QCBy"].ToString();
                    item.TransType = d["TransType"].ToString();
                    item.CmpId = d["CmpId"].ToString();


 
                    issue.items.Add(item);
                }

                issues.Add(issue);
            }
            var response = new {   issues   };
            return Ok(response);

           


        }

        [HttpPost("[action]")]
        public IActionResult setInvenIss(IssueModel iss)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.Inven_setIssueTrans";
                _cmd += "  @UpdUser  ='" + iss.UpdUser + "'";
                _cmd += " ,@IssueNo  ='" + iss.IssueNo + "'";
                _cmd += " ,@IssueDate ='" + iss.IssueDate + "'";
                _cmd += " ,@IssueBy ='" + iss.IssueBy + "'";
                _cmd += " ,@CmpId ='" + iss.CmpId + "'";
                _cmd += " ,@Remark  ='" + iss.Remark + "'";
                _cmd += " ,@DocRef ='" + iss.DocRef + "'";
                _cmd += " ,@WHId =" + iss.SysWHId;
                _cmd += " ,@WHLocId =" + iss.SysWHLocId;
                _cmd += " ,@ProjectNo ='" + iss.ProjectNo + "'";
                _cmd += " ,@StateApp ='" + iss.StateApp + "'";


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
        public IActionResult setInvenIssApprove(IssueModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setIssueTrans_Approve";
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@IssueNo  ='" + receive.IssueNo + "'"; 
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@StateApp  ='" + receive.StateApp + "'"; 

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



        [HttpDelete("[action]")]
        public void DeleteInvenIss([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Issue where IssueNo='" + id + "' and   CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
