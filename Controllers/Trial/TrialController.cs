using goalongapi.Models;
using goalongapi.Models.Trial;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrialController : ControllerBase
    {
        // GET: api/Trial/CheckDupEmail
        [HttpGet("CheckDupEmail")]
        public ActionResult<MsgReturn> CheckEmail(string email)
        {
            MsgReturn msgReturn = new MsgReturn();

            try
            { 
                string _cmd = $"exec dbo.[getCheckEmail] @Email='{email}'";
                DataTable dt = DB.DBConn.GetDataTableSystem(_cmd);

                if (dt.Rows.Count > 0)
                {
                    msgReturn.ReturnCode = "201";
                    msgReturn.Msg = "Email นี้ถูกใช้งานไปแล้ว.";
                    return Ok(msgReturn);
                }
                else
                {
                    msgReturn.ReturnCode = "200";
                    msgReturn.Msg = "Email ใช้ได้.";
                    return Ok(msgReturn);
                }
            }
            catch
            {
                msgReturn.ReturnCode = "404";
                msgReturn.Msg = "เกิดข้อผิดพลาด.";
                return Ok(msgReturn);
            }
        }

        // POST: api/Trial/SetCmp
        [HttpPost("SetCmp")]
        public ActionResult<MsgReturn> UserSignUp([FromBody] CmpData cmp)
        {
            MsgReturn msgReturn = new MsgReturn();
            string _cmd = $"exec dbo.SetCmp @CmpId={cmp.CmpId}, @CmpNameTH='{cmp.CmpNameTH}', @Userlogin='{cmp.Userlogin}'";

            if (DB.DBConn.ExecuteOnlySystem(_cmd))
            {
                string fetchCmd = $"exec dbo.getCmpid '{cmp.Userlogin}'";
                DataTable dt = DB.DBConn.GetDataTableSystem(fetchCmd);

                if (dt.Rows.Count > 0)
                {
                    msgReturn.CmpId = dt.Rows[0][0].ToString();
                }

                msgReturn.ReturnCode = "200";
                msgReturn.Msg = "บันทึกสำเร็จ.";
                return Ok(msgReturn);
            }
            else
            {
                msgReturn.ReturnCode = "404";
                msgReturn.Msg = "บันทึกผิดพลาด.";
                return Ok(msgReturn);
            }
        }
    }
}
