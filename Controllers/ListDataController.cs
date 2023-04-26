using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{
     
    public class ListDataController : ApiController
    {
        // GET: api/ListData
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ListData/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getlistdata] @ListName=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }
        // POST: api/ListData
        public void Post(ListData listData  ) // string listname , int id ,
        {

            string _cmd = "";

            _cmd = "update SystemList  ";
            _cmd += " set  ListDescription ='" + Tool.Tool.validateStr(listData.ListDescription) + "' ";
            _cmd += "where Id =" + listData.Id + "  ";
            _cmd += "and ListName='" + listData.ListName + "'";

            _cmd += "insert into SystemList (Id, ListName, ListDescription, StateActive )";
            _cmd += " select "+ listData.Id + ", '" + listData.ListName + "','" + Tool.Tool.validateStr(listData.ListDescription )+ "' ,'1'"; 
          
            DB.DBConn.ExecuteOnly(_cmd);

        }

        // PUT: api/ListData/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ListData/5
        public void Delete(int id , string listname)
        {
            string _cmd = "";
            _cmd = "delete from  SystemList where Id =" + id + " and  ListName ='" + listname + "'";
           

            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
