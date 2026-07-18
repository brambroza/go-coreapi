using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class ListDataController : ControllerBase
    { 
        [HttpGet("[action]/{listid}/{cmpid}")]
        public IActionResult getlistdata( string listid , string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getlistdata] @ListName=" + listid + ", @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }

        [HttpPost("[action]")]
        public void setlistData(ListData listData) // string listname , int id ,
        {

            string _cmd = "";

            _cmd = "update SystemList  ";
            _cmd += " set  ListDescription ='" + Tool.Tool.validateStr(listData.ListDescription) + "' ";
            _cmd += "where Id =" + listData.Id + "  ";
            _cmd += "and ListName='" + listData.ListName + "'";

            _cmd += "insert into SystemList (Id, ListName, ListDescription, StateActive )";
            _cmd += " select " + listData.Id + ", '" + listData.ListName + "','" + Tool.Tool.validateStr(listData.ListDescription) + "' ,'1'";

            DB.DBConn.ExecuteOnly(_cmd);

        }


        [HttpDelete("[action]")]
        public void DeleteList([FromQuery] int id, [FromQuery] string listname)
        {
            string _cmd = "";
            _cmd = "delete from  SystemList where Id =" + id + " and  ListName ='" + listname + "'";


            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
