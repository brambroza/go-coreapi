using System.ComponentModel;
using Newtonsoft.Json;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
 




namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class QuaController : ControllerBase
    {


        // GET: api/Qua/5
        [HttpGet("[action]")]
        
        public IActionResult GetQuoDetail([FromQuery] string id, [FromQuery] int RevNo , [FromQuery] string CmpId )
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getQuatationDetail @QuatationNo='" + _QuatationNo + "' , @RevNo=" + RevNo + ", @CmpId='" + CmpId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        // POST: api/Qua
        [HttpPost("[action]")]
         
        public void setQuoDetail( [FromBody] List<QuatationDetail> quatation)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (quatation.Count > 0)
                {
                    _cmd = "Delete From mdb.Quatation_Detail where QuatationNo='" + quatation[0].QuatationNo + "'";
                    _cmd += " and  RevNo=" + quatation[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < quatation.Count; i++)
                {
                    il++;
                    _cmd = "Exec setQuatationDetail @QuatationNo='" + quatation[i].QuatationNo + "'";
                    _cmd += ",@Seq=" + il;
                    _cmd += ",@ProdCode='" + quatation[i].ProdCode + "'";
                    _cmd += ",@ProdDesc='" + Tool.Tool.validateStr(quatation[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + quatation[i].UnitPrice;
                    _cmd += ",@UnitCode='" + quatation[i].UnitCode + "'";
                    _cmd += ",@Qty=" + quatation[i].Qty;
                    _cmd += ",@Amt=" + quatation[i].Amt;
                    _cmd += ",@PricePur=" + quatation[i].PricePur;
                    _cmd += ",@CostAmt=" + quatation[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + quatation[i].ProfitAmt;
                    _cmd += ",@RevNo=" + quatation[i].RevNo;
                    _cmd += " ,@GroupCaption1='" + Tool.Tool.validateStr(quatation[i].GroupCaption1) + "'";
                    _cmd += " ,@GroupCaption2='" + Tool.Tool.validateStr(quatation[i].GroupCaption2) + "'";
                    _cmd += " ,@GroupCaption3='" + Tool.Tool.validateStr(quatation[i].GroupCaption3) + "'";
                    _cmd += " , @CmpId='" + quatation[i].CmpId +"'" ; 
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

       
    }
}
