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
    public class SecurityRoleSettingController : ApiController
    {
        // GET: api/SecurityRoleSetting
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SecurityRoleSetting/5
        public IHttpActionResult Get(string id)
        {
            DataTable dt;
            DataTable sdt;
            string _cmd = "";
            _cmd = "exec dbo.getMenuRule '" + id + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.getMenuChidrenRule '" + id + "' ";
            sdt = DB.DBConn.GetDataTable(_cmd);

            List<Rolegroup> listmenu = new List<Rolegroup>();
            foreach (DataRow r in dt.Rows)
            {
                var menus = new Rolegroup();
                menus.children = new List<RoleMenu>();
                menus.id = Convert.ToInt32(r["MenuId"]);
                menus.name = r["title"].ToString(); 
                foreach (DataRow xr in sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])))
                {
                    var sub = new RoleMenu();
                    sub.name = xr["title"].ToString(); 
                    sub.id = Convert.ToInt32(xr["MenuId"]);
                    menus.children.Add(sub);
                }
                listmenu.Add(menus);
            }

            return Ok(listmenu);

        }

        // POST: api/SecurityRoleSetting
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SecurityRoleSetting/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SecurityRoleSetting/5
        public void Delete(int id)
        {
        }
    }
}
