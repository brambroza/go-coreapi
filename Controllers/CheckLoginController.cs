using goalongapi.Models;
using System.Net;
using System;
using goalongapi.Data;
using goalongapi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goalongapi.Datatools.Product;
using Mapster;
using goalongapi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.IdentityModel.Tokens.Jwt; 
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
 
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CheckLoginController : ControllerBase
    { 

        [Route("api/CheckLogin")]
        [HttpGet]
        public IActionResult CheckLogin(string id , string Password , int CmpId)
        {
            string _cmd = "";
            _cmd = "exec dbo.CheckLogin  @UserName='" + id + "', @PassWord='" + Password + "',@CmpId=" + CmpId;
            DataTable dt;
            dt = DB.DBConn.GetDataTable(_cmd);
            if (dt.Rows.Count > 0 )
            {
                if (dt.Rows[0]["statelogin"].ToString() == "0")
                {
                    _cmd = "exec dbo.CheckLogin  @UserName='" + id + "', @PassWord='" + Password + "',@CmpId=" + CmpId;
                     
                    dt = DB.DBConn.GetDataTableSystem(_cmd);
                }
            }
                string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
         
            return Ok(JSONString); 
        }

        [Route("api/login")]
        [HttpPost]
        public IActionResult login(string id, string Password, int CmpId)
        {
            string _cmd = "";
            _cmd = "exec dbo.CheckLogin  @UserName='" + id + "', @PassWord='" + Password + "',@CmpId=" + CmpId;
            DataTable dt;
            dt = DB.DBConn.GetDataTable(_cmd);

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
         
            return Ok(JSONString);
        }

         



        [HttpGet]
        [Route("api/member")]
        public IActionResult Member(string username, string ip)
        {
            DataTable dt;
            dt = new DataTable();

            dt = DB.DBConn.GetDataTable("exec  dbo.get_member @username='" + username + "', @ip='" + ip + "'");
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
         
            return Ok(JSONString);

        }



        //[Route("api/member")]
        //[HttpGet]
        //public IHttpActionResult getmembers(string username, string Ip)
        //{
        //    string _cmd = "";

        //    DataTable dt;
        //    dt = new DataTable("DataTable11");
        //    dt.Columns.Add("MemberAddress", typeof(String));
        //    //dt.Columns.Add("Detail", typeof(String));
        //    //dt.Columns.Add("Status", typeof(String));
        //    //dt.Columns.Add("UserTypeID", typeof(String));
        //    //dt.Columns.Add("Name", typeof(String));
        //    DataRow dr = dt.NewRow();
        //    dr["MemberAddress"] = "14/1";
        //    //dr["Detail"] = "Admin";
        //    //dr["Status"] = "Y";
        //    //dr["UserTypeID"] = "A";
        //    //dr["Name"] = "Admin";
        //    dt.Rows.Add(dr);


        //    return Ok(dt);
        //}

 



    }
}
