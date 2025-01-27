using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace goalongapi.Controllers
{
 
    [ApiController]
    [Authorize]

    public class RouteController : ControllerBase
    {
        // GET: api/RoleSet



        [HttpPost("[action]")]
        public IActionResult setRemindSystem(Remind remind)
        {

            string _cmd = "";

            DB.DBConn.ExecuteOnly(_cmd);


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            { 
                _cmd = "exec  dbo.setRemindSystem";
                _cmd += "  @RemindId ='" + remind.RemindId+ "'";
                _cmd += " ,@RouteId ='" + remind.RouteId + "'";
                _cmd += " ,@CmpId='" + remind.CmpId + "'";
                _cmd += " ,@Seq=" + remind.Seq + "";
                _cmd += " ,@RemideDescription='" + remind.RemindDescription + "'";
                _cmd += " ,@Manday=" + remind.Manday + "";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return StatusCode((int)HttpStatusCode.BadRequest);
                };


                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return StatusCode((int)HttpStatusCode.BadRequest);
            }



        }

 




    }
}
