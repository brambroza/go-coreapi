using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 
using System.Data;
using coreapi.Models;

namespace coreapi.Controllers
{
     
    public class RoleSetController : ApiController
    {
        // GET: api/RoleSet
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/RoleSet/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getRolelist] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        // POST: api/RoleSet
        public void Post(List<RoleSet> roleset)
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
                    if ( roleset[0].RoleId <= 0)
                    {
                        _cmd = "Select NEXT VALUE FOR dbo.roleid";
                        roleids = int.Parse( DB.DBConn.GetFieldOnBeginTrans(_cmd, 0));
                    }
                     
                }

                _cmd = "delete from  SystemPermissionMenu where RoleId=" + roleset[0].RoleId ;
                DB.DBConn.ExecuteOnly(_cmd);


                for (int i = 0; i < roleset.Count; i++)
                {

                    if  (roleset[i].RoleId == 0)
                    {
                        roleset[i].RoleId = roleids;
                      
                    }
                    _cmd = "exec  dbo.SystemRoleSet";
                    _cmd += "  @RoleId =" +  roleset[i].RoleId;
                    _cmd += ",@RoleName ='" + roleset[i].RoleName + "'";
                    _cmd += ",@RoleDescription ='" + roleset[i].RoleDescription + "'";
                    _cmd += ",@MenuId =" + roleset[i].MenuId;


                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;

                    };
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

        // PUT: api/RoleSet/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RoleSet/5
        public void Delete(int id)
        {
        }
    }
}
