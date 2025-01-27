using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        // GET: api/Auth
        [HttpPost("[action]")]
        
        public IActionResult systemlog(UserActionLog actionLog)
        { 

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.system_setSystemUserLog";
                _cmd += " @UserLogin  ='" + actionLog.UserLogin + "'";
                _cmd += ",@MenuName  ='" + actionLog.MenuName + "'";
                _cmd += ",@ActionsDescriptions  ='" + actionLog.ActionsDescriptions + "'";
                _cmd += ",@btnname  ='" + actionLog.btnname + "'";
                _cmd += ",@CmpId =" + actionLog.CmpId;


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
    }
}
