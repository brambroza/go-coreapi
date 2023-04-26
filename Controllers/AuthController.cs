using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{
   
    public class AuthController : ApiController
    {
        // GET: api/Auth
        [HttpPost]
        [Route("api/systemlog")]
        public IHttpActionResult Post(UserActionLog actionLog)
        {
            //if (Request.Headers.Contains("authToken")){
            //    if (Request.Headers.GetValues("authToken").First() != "XXX")
            //        return   Ok(HttpStatusCode.Unauthorized); 
            //}



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
