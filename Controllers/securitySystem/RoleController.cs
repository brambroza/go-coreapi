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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    { 

        [HttpGet("[action]")]
        public IActionResult getRole([FromQuery] string cmpid, [FromQuery] string User)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[security_Rolelist] @CmpId=" + cmpid + " , @user ='" + User + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<Rolelist> roleList = new List<Rolelist>();

            foreach (DataRow r in dt.Rows)
            {
                var role = new Rolelist()
                {
                    RoleId = Convert.ToInt32(r["RoleId"]),
                    RoleName = r["RoleName"].ToString(),
                    RoleDescription = r["RoleDescription"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    JobDesc = Convert.ToInt32(r["JobDesc"]),
                    StateManager = Convert.ToInt32(r["StateManager"]),
                    JobDescFilter = r["JobDescFilter"].ToString(),
                };

                roleList.Add(role);
            }

            return Ok(roleList);
        }

        [HttpGet("[action]")]
        public IActionResult getJobDesc([FromQuery] string cmpid, [FromQuery] string User)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[security_getJobdesc] @CmpId=" + cmpid + " , @user ='" + User + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            List<JobDesc> jobDescs = new List<JobDesc>();

            foreach (DataRow r in dt.Rows)
            {
                var job = new JobDesc()
                {
                    JobDescId = Convert.ToInt32(r["JobDescId"]),
                    JobDescName = r["JobDescName"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                };

                jobDescs.Add(job);
            }

            return Ok(jobDescs);
        }

        [HttpGet("[action]")]
        public IActionResult getMenulist(
            [FromQuery] string cmpid,
            [FromQuery] string User,
            [FromQuery] int RoleId
        )
        {
            DataTable dt = new System.Data.DataTable();
            DataTable dtObject = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[security_Menulist] @CmpId='"
                + cmpid
                + "' , @user ='"
                + User
                + "' , @RoleId="
                + RoleId;
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd =
                "exec dbo.[security_MenuObject] @CmpId='"
                + cmpid
                + "' , @user ='"
                + User
                + "' , @RoleId="
                + RoleId;
            dtObject = DB.DBConn.GetDataTable(_cmd);

            List<Menulist> roleList = new List<Menulist>();

            foreach (DataRow r in dt.Rows)
            {
                var role = new Menulist()
                {
                    MenuId = Convert.ToInt32(r["MenuId"]),
                    MenuMainId = Convert.ToInt32(r["MenuMainId"]),
                    title = r["title"].ToString(),
                    StateActive = Convert.ToInt32(r["StateActive"]),
                    Seq = Convert.ToInt32(r["Seq"]),
                    JobDesId = Convert.ToInt32(r["JobDesId"]),
                    StateSelect = Convert.ToInt32(r["StateSelect"]),
                    objects = new List<MenuButtonObject>(),
                };

                foreach (DataRow x in dtObject.Select("MenuId=" + role.MenuId))
                {
                    var obj = new MenuButtonObject()
                    {
                        MenuId = Convert.ToInt32(x["MenuId"]),
                        ObjectName = x["ObjectName"].ToString(),
                        StateSelect = Convert.ToInt32(x["StateSelect"]),
                        StateActive = Convert.ToInt32(x["StateActive"]),
                        StateManager = Convert.ToInt32(x["StateManager"]),
                        ObjectLable = x["ObjectLable"].ToString(),
                    };

                    role.objects.Add(obj);
                }

                roleList.Add(role);
            }

            return Ok(roleList);
        }

        [HttpPost("[action]")]
        public IActionResult Updaterole(Rolelist roleset)
        {
            string _cmd = "";

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                _cmd = "exec  dbo.security_Roleset";
                _cmd += "  @RoleId =" + roleset.RoleId;
                _cmd += ",@RoleName ='" + roleset.RoleName + "'";
                _cmd += ",@RoleDescription ='" + roleset.RoleDescription + "'";
                _cmd += ",@StateManager =" + roleset.StateManager;
                _cmd += " ,@CmpId='" + roleset.CmpId + "'";
                _cmd += ",@JobDesc =" + roleset.JobDesc;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
                }
                ;

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return Ok();
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public IActionResult UpdatePermissionMenu(PermissionMenu roleset)
        {
            string _cmd = "";

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                _cmd = "exec  dbo.[security_setPermissionMenu]";
                _cmd += "  @RoleId =" + roleset.RoleId;
                _cmd += ",@MenuId =" + roleset.MenuId;
                _cmd += " ,@CmpId='" + roleset.CmpId + "'";
                _cmd += ",@StateActive =" + roleset.StateActive;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
                }
                ;

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return Ok();
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public IActionResult UpdatePermissionMenuObject(PermissionMenuObject roleset)
        {
            string _cmd = "";

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                _cmd = "exec  dbo.[security_setPermissionMenuObject]";
                _cmd += "  @RoleId =" + roleset.RoleId;
                _cmd += ",@MenuId =" + roleset.MenuId;
                _cmd += " ,@CmpId='" + roleset.CmpId + "'";
                _cmd += ",@StateActive =" + roleset.StateActive;
                _cmd += " ,@ObjectName='" + roleset.ObjectName + "'";

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    return BadRequest();
                }
                ;

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return Ok();
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest();
            }
        }

        [HttpGet("[action]")]
        public IActionResult getPermissionObject([FromQuery] string cmpid, [FromQuery] string User)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[security_getPermissionObject] @CmpId="
                + cmpid
                + " , @user ='"
                + User
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<PermissionMenuObject> roleList = new List<PermissionMenuObject>();

            foreach (DataRow r in dt.Rows)
            {
                var role = new PermissionMenuObject()
                {
                    RoleId = Convert.ToInt32(r["RoleId"]),
                    ObjectName = r["ObjectName"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    StateActive = Convert.ToInt32(r["StateActive"]),
                    MenuId = Convert.ToInt32(r["MenuId"]),
                };

                roleList.Add(role);
            }

            return Ok(roleList);
        }

        [HttpGet("[action]")]
        public IActionResult getUserMapRole([FromQuery] string cmpid, [FromQuery] string User)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[security_getUserMapRole] @CmpId=" + cmpid + " , @user ='" + User + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<UserMapRole> roleList = new List<UserMapRole>();

            foreach (DataRow r in dt.Rows)
            {
                var role = new UserMapRole()
                {
                    RoleID = Convert.ToInt32(r["RoleId"]),
                    CmpId = r["CmpId"].ToString(),
                    AccountID = Convert.ToInt32(r["AccountID"]),
                };

                roleList.Add(role);
            }

            return Ok(roleList);
        }

        [HttpPost("[action]")]
        public IActionResult setUserMapRole(List<UserMapRole> roldmap)
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
                    _cmd = "exec  dbo.[security_setUserMapRole]";
                    _cmd += "  @RoleId =" + roldmap[i].RoleID;
                    _cmd += " ,@AccountID =" + roldmap[i].AccountID + "";
                    _cmd += " ,@CmpId='" + roldmap[i].CmpId + "'";

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return Ok();
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest();
            }
        }
    }
}
