using System.Net;
using goalongapi.Datatools.Account;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Mail;
using goalongapi.Models;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController] 

    public class RegisController : ControllerBase
    {  

        [HttpPost("[action]")]
        public IActionResult setup(Company cmp)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_register_company";
                _cmd += " @cmpid  ='" + cmp.CmpId + "'";
                _cmd += " ,@CmpAddress  ='" + cmp.CmpAddress + "'";
                _cmd += " ,@CmpName  ='" + cmp.CmpName + "'";
                _cmd += " ,@Phone  ='" + cmp.Phone + "'";
                _cmd += " ,@fax  ='" + cmp.Fax + "'";
                _cmd += " ,@email  ='" + cmp.Email + "'";
                _cmd += " ,@teloffice  ='" + cmp.teloffice + "'";


                DataTable dt = goalongapi.DB.DBConn.GetDataTable(_cmd);

                return Ok(dt.Rows[0][0]);



            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }

        }




        [HttpPost("[action]")]
        public IActionResult setUpMapUser(MapUser cmp)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_mapregister_user";
                _cmd += " @cmpid  ='" + cmp.cmpid + "'";
                _cmd += " ,@email  ='" + cmp.email + "'";




                if (goalongapi.DB.DBConn.ExecuteOnly(_cmd))
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