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
    
    public class PremisstionlistController : ApiController
    {
        // GET: api/Premisstionlist
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Premisstionlist/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getPermissionlist] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);

        }


        // POST: api/Premisstionlist
        public void Post(List<UserMap> roldmap)
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
                    _cmd = "	 delete from  SystemPermission WHERE UserName ='" + roldmap[0].UserName + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }                

                for (int i = 0; i < roldmap.Count; i++)
                {
                    _cmd = "exec  dbo.RoleSet";
                    _cmd += "  @RoleId =" + roldmap[i].RoleId;
                    _cmd += ",@UserName ='" + roldmap[i].UserName + "'";
               
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


        // PUT: api/Premisstionlist/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Premisstionlist/5
        public void Delete(int id)
        {
        }
    }
}
