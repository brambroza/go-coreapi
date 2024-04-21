using System.ComponentModel;
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

    public class AccountSystemController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getAccountCode([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            DataTable dt = new DataTable();
            string _cmd;

            _cmd = "exec dbo.getAccountCode @userlogin='" + userlogin + "' , @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<AccountCode> accountcode = new List<AccountCode>();

            foreach (DataRow r in dt.Select("AccMainId = AccCode", "AccCode asc"))
            {
                var addcode = new AccountCode();
                addcode.AccId = Int32.Parse(r["AccId"].ToString());
                addcode.AccCode = r["AccCode"].ToString();
                addcode.AccName = r["AccName"].ToString();
                addcode.AccTypeId = (r["AccTypeId"].ToString());
                addcode.AccLevelId = (r["AccLevelId"].ToString());
                addcode.StateActive = r["StateActive"].ToString();
                addcode.AccMainId = r["AccMainId"].ToString();
                addcode.children = new List<AccountCode>();
                foreach (DataRow x in dt.Select("AccMainId <> AccCode and AccMainId='" + r["AccCode"].ToString() + "'", "AccCode asc"))
                {
                    var addChildcode = new AccountCode();
                    addChildcode.AccCode = x["AccCode"].ToString();
                    addChildcode.AccName = x["AccName"].ToString();
                    addChildcode.AccTypeId = (x["AccTypeId"].ToString());
                    addChildcode.AccLevelId = (x["AccLevelId"].ToString());
                    addChildcode.StateActive = x["StateActive"].ToString();
                    addChildcode.AccMainId = x["AccMainId"].ToString();
                    addChildcode.AccId = Int32.Parse(x["AccId"].ToString());
                    addcode.children.Add(addChildcode);
                }


                accountcode.Add(addcode);
            }

            string jsonReturn = string.Empty;
            jsonReturn = JsonConvert.SerializeObject(accountcode);
            return Ok(jsonReturn);

        }

        [HttpGet("[action]")]
        public IActionResult getAccountType([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getAccountType @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        [HttpGet("[action]")]
        public IActionResult getAccountLevel([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getAccountLevel @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        [HttpPost("[action]")]
        public IActionResult setAccountCode(setAccountCode acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setAccountCode";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@AccId =" + acc.AccId;
                _cmd += " ,@AccCode ='" + acc.AccCode + "'";
                _cmd += " ,@AccName  ='" + acc.AccName + "'";
                _cmd += " ,@StateActive =" + acc.StateActive;
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
                _cmd += " ,@AccTypeId=" + acc.AccTypeId;
                _cmd += " , @AccLevelId=" + acc.AccLevelId;
                _cmd += " , @AccMainId='" + acc.AccMainId + "'";
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
        public IActionResult setAccountType(setAccountType acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setAccountType";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@AccTypeId =" + acc.AccTypeId;
                _cmd += " ,@AccTypeCode ='" + acc.AccTypeCode + "'";
                _cmd += " ,@AccTypeName  ='" + acc.AccTypeName + "'";
                _cmd += " ,@StateActive =" + acc.StateActive;
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
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
        public IActionResult setAccountLevel(setAccountLevel acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setAccountLevel";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@AccLevelId =" + acc.AccLevelId;
                _cmd += " ,@AccLevelCode ='" + acc.AccLevelCode + "'";
                _cmd += " ,@AccLevelName  ='" + acc.AccLevelName + "'";
                _cmd += " ,@StateActive ='" + acc.StateActive + "'";
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
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
        public void deletAccountCode([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from acc.AccountCode where AccId='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }


        [HttpDelete("[action]")]
        public void deletAccountType([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from acc.AccountType where AccTypeId='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }



        [HttpDelete("[action]")]
        public void deletAccountLevel([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from acc.AccountLevel where AccLevelId='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }





        /// account receive book 
        /// 

        [HttpGet("[action]")]
        public IActionResult getAccountRcvBook([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            DataTable dt = new DataTable();
            string _cmd;

            _cmd = "exec dbo.getaccountrcvbook @user='" + userlogin + "' , @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            string jsonReturn = string.Empty;
            jsonReturn = JsonConvert.SerializeObject(dt);
            return Ok(jsonReturn);

        }


        [HttpPost("[action]")]
        public IActionResult setAccountRcvBook(AccountRcvBook acc)
        {

            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setAccountRcvBook";
                _cmd += "  @UpdUser  ='" + acc.UpdUser + "'";
                _cmd += " ,@BookRcvId =" + acc.BookRcvId;
                _cmd += " ,@BookRcvCode ='" + acc.BookRcvCode + "'";
                _cmd += " ,@BookRcvName  ='" + acc.BookRcvName + "'";
                _cmd += " ,@AccRcvType =" + acc.AccRcvType;
                _cmd += " ,@AccCode ='" + acc.AccCode + "'";
                _cmd += " ,@BankCode ='" + acc.BankCode + "'";
                _cmd += " ,@BankBranchCode ='" + acc.BankBranchCode + "'";
                _cmd += " ,@CmpId ='" + acc.CmpId + "'";
                _cmd += " ,@BankAccCode='" + acc.BankAccCode + "'";
                _cmd += " ,@Remark='" + acc.Remark + "'";
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
        public void deleteAccountRcvBook([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from acc.AccountRcvBook where BookRcvId='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }






        ////account rcv type 

        [HttpGet("[action]")]
        public IActionResult getAccountRcvType([FromQuery] string userlogin, [FromQuery] string cmpid)
        {
            DataTable dt = new DataTable();
            string _cmd;

            _cmd = "exec dbo.getaccountrcvtype @user='" + userlogin + "' , @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            string jsonReturn = string.Empty;
            jsonReturn = JsonConvert.SerializeObject(dt);
            return Ok(jsonReturn);

        }






    }
}
