using goalongapi.Models;
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

namespace goalongapi.Controllers
{
    [ApiController]
    

    public class LineExternalController : ControllerBase
    { 
  
        [HttpGet("[action]")]
        public IActionResult getContactSocialById([FromQuery] string userId)
        {
            string _cmd;
            _cmd = "exec dbo.[getSocailContactById] @userId='" + userId + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(dt);

                var prodlist = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var eventObj = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        string lowercaseColumnName =
                            char.ToLowerInvariant(column.ColumnName[0])
                            + column.ColumnName.Substring(1);

                        eventObj[lowercaseColumnName] = row[column];
                    }

                    prodlist.Add(eventObj);
                }


                return Ok(prodlist);
        }


 

    }
}
