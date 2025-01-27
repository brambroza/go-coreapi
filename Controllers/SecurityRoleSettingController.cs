using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityRoleSettingController : ControllerBase
    {
        // GET: api/SecurityRoleSetting
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        // GET: api/SecurityRoleSetting/5
        [HttpGet("{id}")]
        public ActionResult<List<Rolegroup>> Get(string id)
        {
            DataTable dt;
            DataTable sdt;
            string _cmd = "";

            _cmd = $"exec dbo.getMenuRule '{id}'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = $"exec dbo.getMenuChidrenRule '{id}'";
            sdt = DB.DBConn.GetDataTable(_cmd);

            List<Rolegroup> listmenu = new List<Rolegroup>();
            foreach (DataRow r in dt.Rows)
            {
                var menus = new Rolegroup
                {
                    children = new List<RoleMenu>(),
                    id = Convert.ToInt32(r["MenuId"]),
                    name = r["title"].ToString()
                };

                foreach (DataRow xr in sdt.Select($"MenuMainId={Convert.ToInt32(r["MenuId"])}"))
                {
                    menus.children.Add(new RoleMenu
                    {
                        name = xr["title"].ToString(),
                        id = Convert.ToInt32(xr["MenuId"])
                    });
                }
                listmenu.Add(menus);
            }

            return Ok(listmenu);
        }

        // POST: api/SecurityRoleSetting
        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            // Implement logic here
            return Ok();
        }

        // PUT: api/SecurityRoleSetting/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] string value)
        {
            // Implement logic here
            return Ok();
        }

        // DELETE: api/SecurityRoleSetting/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            // Implement delete logic here
            return Ok();
        }
    }
}
