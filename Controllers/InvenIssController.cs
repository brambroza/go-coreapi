using coreapi.Models;
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


namespace coreapi.Controllers
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
            _cmd = "exec dbo.Inven_getIssAll @CmpId='" + (CmpId) + "' , @User='" + userlogin + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);

            string res = string.Empty;
            res = JsonConvert.SerializeObject(datatable);
            return Ok(res);


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
                _cmd += " ,@WHId =" + iss.WHId;
                _cmd += " ,@WHLocId =" + iss.WHLocId;
                _cmd += " ,@ProjectNo ='" + iss.ProjectNo + "'";


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
