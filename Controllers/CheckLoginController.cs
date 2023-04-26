using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 
using System.Data;


namespace coreapi.Controllers
{
 
    public class CheckLoginController : ApiController
    {
        // GET: api/CheckLogin


        [Route("api/CheckLogin")]
        [HttpGet]
        public IHttpActionResult Get(string id , string Password , int CmpId)
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
             
            return Ok(dt);
        }

        [Route("api/login")]
        [HttpPost]
        public IHttpActionResult Checklogin(string id, string Password, int CmpId)
        {
            string _cmd = "";
            _cmd = "exec dbo.CheckLogin  @UserName='" + id + "', @PassWord='" + Password + "',@CmpId=" + CmpId;
            DataTable dt;
            dt = DB.DBConn.GetDataTable(_cmd);

            return Ok(dt);
        }

        // POST: api/CheckLogin
        //[Route("api/CheckLogin")]
        //[HttpPost]
        //public void Post(string UserName , string Password , int CmpId)
        //{
        //    string _cmd = "";
        //    _cmd = "exec dbo.Changepass  @UserName='" + UserName + "', @PassWord='" + Password + "',@CmpId=" + CmpId;

        //    DB.DBConn.ExecuteOnly(_cmd);

        //}


        //[Route("api/checklogin")]
        //[HttpGet]
        //public IHttpActionResult getmember(string user, string pass)
        //{
        //    string _cmd = "";

        //    DataTable dt;
        //    dt = new DataTable("DataTable11");
        //    dt.Columns.Add("UserName" , typeof(String));
        //    dt.Columns.Add("Detail", typeof(String));
        //    dt.Columns.Add("Status", typeof(String));
        //    dt.Columns.Add("UserTypeID", typeof(String));
        //    dt.Columns.Add("Name", typeof(String));
        //    DataRow dr = dt.NewRow();
        //    dr["UserName"] = "999";
        //    dr["Detail"] = "Admin";
        //    dr["Status"] = "Y";
        //    dr["UserTypeID"] = "A";
        //    dr["Name"] = "Admin";
        //    dt.Rows.Add(dr);


        //    return Ok(dt);
        //}



        [HttpGet]
        [Route("api/member")]
        public IHttpActionResult GetMember(string username, string ip)
        {
            DataTable dt;
            dt = new DataTable();

            dt = DB.DBConn.GetDataTable("exec  dbo.get_member @username='" + username + "', @ip='" + ip + "'");
            return Ok(dt);

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


        [HttpGet]
        [Route("api/getcartype")]
        public IHttpActionResult Get()
        {
            DataTable dt;
            string str = "";
            str = "exec sp_getCartype";
            dt = new DataTable();
            dt = DB.DBConn.GetDataTable(str);
            return Ok(dt);
        }


        
        public IHttpActionResult getcartype()
        {
            string _cmd = "";

            DataTable dt;
            dt = new DataTable("DataTable11");
            dt.Columns.Add("CarTypeID", typeof(String));
            dt.Columns.Add("CarTypeName", typeof(String));
            //dt.Columns.Add("Status", typeof(String));
            //dt.Columns.Add("UserTypeID", typeof(String));
            //dt.Columns.Add("Name", typeof(String));
            DataRow dr = dt.NewRow();
            dr["CarTypeID"] = "01";
             dr["CarTypeName"] = "รถยนต์ ส่วนบุคคล";
            //dr["Status"] = "Y";
            //dr["UserTypeID"] = "A";
            //dr["Name"] = "Admin";
            dt.Rows.Add(dr);
              dr = dt.NewRow();
            dr["CarTypeID"] = "02";
            dr["CarTypeName"] = "รถยนต์ รับจ้าง";
            //dr["Status"] = "Y";
            //dr["UserTypeID"] = "A";
            //dr["Name"] = "Admin";
            dt.Rows.Add(dr);

            return Ok(dt);
        }



        [HttpGet]
        [Route("api/getmember")]
        public IHttpActionResult Getmember(string License, string Province)
        {
            DataTable dt;
            string str = "";
            str = "exec [sp_checkmember] @License='" + License + "' , @Province='" + Province + "'";
            dt = new DataTable();
            dt = DB.DBConn.GetDataTable(str);
            return Ok(dt);
        }



    }
}
