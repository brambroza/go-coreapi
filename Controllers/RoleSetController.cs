using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{ 
    [ApiController]
    [Authorize]
    public class RoleSetController : ControllerBase
    {
        // GET: api/RoleSet

        [HttpGet("[action]")]
        public IActionResult getRolelist([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getRolelist] @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getRolelistById([FromQuery] string cmpid, [FromQuery] string RoleId)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getRolelistByid] @CmpId='"
                + cmpid
                + "'  ,  @RoleId='"
                + RoleId.ToString()
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getMenuForRoleSet(
            [FromQuery] string cmpid,
            [FromQuery] string userlogin
        )
        {
            DataTable dt;
            DataTable sdt;
            string _cmd = "";
            _cmd = "exec dbo.getMenuRule @cmpid= '" + cmpid + "' , @user='" + userlogin + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.getMenuChidrenRule @cmpid= '" + cmpid + "' , @user='" + userlogin + "'";
            sdt = DB.DBConn.GetDataTable(_cmd);

            List<setRoleGroup> listmenu = new List<setRoleGroup>();
            foreach (DataRow r in dt.Rows)
            {
                var menus = new setRoleGroup();
                menus.children = new List<setRoleGroup>();
                menus.key = Convert.ToInt32(r["MenuId"]);
                menus.title = r["title"].ToString();
                menus.icon = r["icon"].ToString();
                foreach (DataRow xr in sdt.Select("MenuMainId=" + Convert.ToInt32(r["MenuId"])))
                {
                    var sub = new setRoleGroup();
                    sub.title = xr["title"].ToString();
                    sub.key = Convert.ToInt32(xr["MenuId"]);
                    sub.icon = r["icon"].ToString();
                    menus.children.Add(sub);
                }
                listmenu.Add(menus);
            }

            return Ok(listmenu);
        }

        // POST: api/RoleSet
        [HttpPost("[action]")]
        public void Updaterole(List<RoleSet> roleset)
        {
            string _cmd = "";

            int roleids = 0;

            DB.DBConn.ExecuteOnly(_cmd);

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                if (roleset.Count > 0)
                {
                    if (roleset[0].RoleId <= 0)
                    {
                        _cmd = "Select NEXT VALUE FOR dbo.roleid";
                        roleids = int.Parse(DB.DBConn.GetFieldOnBeginTrans(_cmd, 0));
                    }
                }

                _cmd =
                    "delete from  SystemPermissionMenu where RoleId="
                    + roleset[0].RoleId
                    + " and CmpId ='"
                    + roleset[0].CmpId
                    + "'";
                DB.DBConn.ExecuteOnly(_cmd);

                for (int i = 0; i < roleset.Count; i++)
                {
                    if (roleset[i].RoleId == 0)
                    {
                        roleset[i].RoleId = roleids;
                    }
                    _cmd = "exec  dbo.SystemRoleSet";
                    _cmd += "  @RoleId =" + roleset[i].RoleId;
                    _cmd += ",@RoleName ='" + roleset[i].RoleName + "'";
                    _cmd += ",@RoleDescription ='" + roleset[i].RoleDescription + "'";
                    _cmd += ",@MenuId =" + roleset[i].MenuId;
                    _cmd += " , @CmpId='" + roleset[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }

        [HttpGet("[action]")]
        public IActionResult getSaleTeam([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getSystemSaleTeam] @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getSaleTeamById([FromQuery] string cmpid, [FromQuery] int SaleTeamId)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getSystemSaleTeamById] @CmpId="
                + cmpid
                + " , @SaleTeamId="
                + SaleTeamId.ToString();
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public void setSystemSaleTeam(List<SaleTeam> saleTeams)
        {
            string _cmd = "";

            DB.DBConn.ExecuteOnly(_cmd);

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                _cmd =
                    "delete from  SystemSaleTeam where SaleTeamId="
                    + saleTeams[0].SaleTeamId
                    + " and CmpId='"
                    + saleTeams[0].CmpId
                    + "'";
                DB.DBConn.ExecuteOnly(_cmd);

                for (int i = 0; i < saleTeams.Count; i++)
                {
                    _cmd = "exec  dbo.setSystemSaleTeam";
                    _cmd += "  @SaleTeamId =" + saleTeams[i].SaleTeamId;
                    _cmd += ",@SaleTeamName ='" + saleTeams[i].SaleTeamName + "'";
                    _cmd += ",@CmpId ='" + saleTeams[i].CmpId + "'";
                    _cmd += ",@AccountID =" + saleTeams[i].AccountID;

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
            catch 
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }

        [HttpPost("[action]")]
        public void RoleSet(List<UserMap> roldmap)
        {
            string _cmd = "";

            DB.DBConn.ExecuteOnly(_cmd);

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                if (roldmap.Count > 0)
                {
                    _cmd =
                        "	 delete from  SystemPermission WHERE AccountID ='"
                        + roldmap[0].AccountID
                        + "' and CmpId='"
                        + roldmap[0].CmpId
                        + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < roldmap.Count; i++)
                {
                    _cmd = "exec  dbo.RoleSet";
                    _cmd += "  @RoleId =" + roldmap[i].RoleId;
                    _cmd += " ,@AccountID =" + roldmap[i].AccountID + "";
                    _cmd += " ,@CmpId='" + roldmap[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
            catch  
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }

        [HttpPost("[action]")]
        public void SaleTeamSet(List<SaleTeamMap> roldmap)
        {
            string _cmd = "";

            DB.DBConn.ExecuteOnly(_cmd);

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                if (roldmap.Count > 0)
                {
                    _cmd =
                        "	 delete from  SystemPermissionSaleTeam WHERE AccountID ='"
                        + roldmap[0].AccountID
                        + "' and CmpId='"
                        + roldmap[0].CmpId
                        + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < roldmap.Count; i++)
                {
                    _cmd = "exec  dbo.SaleTeamSet";
                    _cmd += "  @SaleTeamId =" + roldmap[i].SaleTeamId;
                    _cmd += " ,@AccountID =" + roldmap[i].AccountID + "";
                    _cmd += " ,@CmpId='" + roldmap[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
            catch 
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }

        [HttpGet("[action]")]
        public IActionResult getRoleByUserID([FromQuery] string cmpid, [FromQuery] string AccountID)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getPermissionUseByAccountID] @CmpId='"
                + cmpid
                + "'  ,  @AccountID='"
                + AccountID.ToString()
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getSaleTeamByUserID(
            [FromQuery] string cmpid,
            [FromQuery] string AccountID
        )
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getPermissionSaleTeamUseByAccountID] @CmpId='"
                + cmpid
                + "'  ,  @AccountID='"
                + AccountID.ToString()
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }
    }
}
