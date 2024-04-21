using coreapi.Models;
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
using System.Reflection;

namespace coreapi.Controllers
{
    [ApiController] 
    [Authorize]
    public class MenuController : ControllerBase
    {
        // GET: api/Menu
        [Route("Menu")]
        [HttpGet] 
        public IActionResult Get([FromQuery] string cmpcode , [FromQuery] string user)
        {
            DataTable dt;
            DataTable sdt;
            string _cmd = "";
            _cmd = "exec dbo.getMenuRule '" + cmpcode + "' , '" + user +"'";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.getMenuChidrenRule '" + cmpcode + "', '" + user + "' ";
            sdt = DB.DBConn.GetDataTable(_cmd);

            List<Menuslist> listmenu = new List<Menuslist>();
            foreach (DataRow r in dt.Rows)
            {

                var menus = new Menuslist();
                if (sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])).Length > 0)
                {
                    menus.children = new List<MenuChildren>();
                }                
                menus.MenuId = Convert.ToInt32(r["MenuId"]);
                menus.title = r["title"].ToString();
                menus.to = r["to"].ToString();
                menus.link = r["to"].ToString();
                menus.icon = r["icon"].ToString();
                foreach (DataRow xr in sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])))
                {
                    
                    var sub = new MenuChildren();
                    
                    sub.title = xr["title"].ToString();
                    sub.to = xr["to"].ToString();
                    sub.link = xr["to"].ToString();
                    sub.icon = xr["icon"].ToString();
                    sub.MenuId = Convert.ToInt32(xr["MenuId"]);
                    menus.children.Add(sub);
                   
                }
              if (sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])).Length > 0 || r["to"].ToString() != "")
                {
                    listmenu.Add(menus);
                }
              

            }

            return Ok(listmenu);

        }



        [Route("menusub")]
        [HttpGet]
        public IActionResult getsub([FromQuery]  string cmpcode,[FromQuery]  string user , [FromQuery] int menuid )
        {
            DataTable dt;
            DataTable sdt;
            string _cmd = "";
            _cmd = "exec dbo.getMenuRule '" + cmpcode + "' , '" + user + "'";
            dt = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.getMenuChidrenRule '" + cmpcode + "', '" + user + "' ";
            sdt = DB.DBConn.GetDataTable(_cmd);

            List<Menuslist> listmenu = new List<Menuslist>();
            foreach (DataRow r in dt.Select("MenuId=" + menuid))
            {

                var menus = new Menuslist();
                if (sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])).Length > 0)
                {
                    menus.children = new List<MenuChildren>();
                }
                menus.MenuId = Convert.ToInt32(r["MenuId"]);
                menus.title = r["title"].ToString();
                menus.to = r["to"].ToString();
                menus.link = r["to"].ToString();
                menus.icon = r["icon"].ToString();


                foreach (DataRow xr in sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])))
                {

                    var sub = new MenuChildren();

                    sub.title = xr["title"].ToString();
                    sub.to = xr["to"].ToString();
                    sub.link = xr["to"].ToString();
                    sub.icon = xr["icon"].ToString();
                    sub.MenuId = Convert.ToInt32(xr["MenuId"]);
                    sub.SubOverViewSales = Convert.ToInt16(xr["SubOverViewSales"]);
                    sub.SubOverViewCust = Convert.ToInt16(xr["SubOverViewCust"]);
                    sub.SubOverViewVendor = Convert.ToInt16(xr["SubOverViewVendor"]);
                    menus.children.Add(sub);

                }
                if (sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])).Length > 0 || r["to"].ToString() != "")
                {
                    listmenu.Add(menus);
                }


            }

            return Ok(listmenu);

        }





        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

    }
}
