using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using goalongapi.Interfaces;
  
namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class UserPermisstionController : ControllerBase
    {
        private readonly IAccountService accountService;
         public UserPermisstionController(IAccountService accountService) => this.accountService = accountService;


        // GET: api/UserPermisstion/5

        [HttpPost]
        [Route("[action]/{cmpid}")]

        public IActionResult GetPermission(string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUserPermisstion] @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);

        }

        [HttpPost]
        [Route("[action]/{cmpid}")]
        public IActionResult GetUserList( string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUserlist] @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpDelete]
        [Route("[action]")]
        public bool DeleteUser(string Username)
        {
            bool res  = accountService.removeUser(Username);
            return res;
        }



    }
}
