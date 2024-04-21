using coreapi.Models;
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

namespace coreapi.Controllers
{
      [ApiController]
    [Authorize]

    public class InvenTransController : ControllerBase
    {
     
        [HttpGet("[action]")]
        public IActionResult getInvenTrans([FromQuery] string  CmpId, [FromQuery] string user , [FromQuery] string TransNo)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getTransAll_ByDoc @CmpId='" + (CmpId) + "' , @User='" + user + "',@DocNo='" + TransNo +"'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
             string  JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


         
        [HttpGet("[action]")]
        public IActionResult getInvenOnhand([FromQuery] string CmpId, [FromQuery] string user, [FromQuery] string TransNo)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getOnhand @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'  ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string  JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        // POST: api/InvenTrans
      
        [HttpPost("[action]")]
        public void setInvenTrans( List<InvenTransModel> Inven)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (Inven.Count > 0)
                {
                    _cmd = "Delete From Inven.InvenTrans   where DocNo='" + Inven[0].DocNo + "'";
                    _cmd += "   and CmpId='" + Inven[0].CmpId + "'";
                     
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < Inven.Count; i++)
                {

                    _cmd = "exec  dbo.Inven_setInvenTrans"; 
                    _cmd += " @UpdUser  ='" + Inven[i].UpdUser + "'";
                    _cmd += ",@Seq =" + Inven[i].Seq;
                    _cmd += ",@DocNo  ='" + Inven[i].DocNo + "'";
                    _cmd += ",@TransDate ='" + Inven[i].TransDate + "'"; ;
                    _cmd += ",@SysWHId =" + Inven[i].SysWHId;
                    _cmd += ",@SysWHLocId =" + Inven[i].SysWHLocId; 
                    _cmd += ",@BarcodeNo  ='" + Inven[i].BarcodeNo + "'";
                    _cmd += ",@ProductCode  ='" + Inven[i].ProductCode + "'"; 
                    _cmd += ",@UnitPrice =" + Inven[i].UnitPrice; 
                    _cmd += ",@Qty =" + Inven[i].Qty; 
                    _cmd += ",@UnitCode ='" + Inven[i].UnitCode + "'"; ;
                    _cmd += ",@PurChaseNo  ='" + Inven[i].PurChaseNo + "'"; 
                    _cmd += ",@StateReserve ='" + Inven[i].StateReserve + "'"; ;
                    _cmd += ",@BatchNo ='" + Inven[i].BatchNo + "'"; ;
                    _cmd += ",@Grade ='" + Inven[i].Grade + "'"; ;
                    _cmd += ",@DateExpire ='" + Inven[i].DateExpire + "'"; ;
                    _cmd += ",@Type ='" + Inven[i].TransType + "'";
                    _cmd += ", @CmpId='" + Inven[i].CmpId + "'"; 
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


         
        [HttpPost("[action]")]
        public IActionResult setInvenAdjApp(AdjustModel adjust)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTransApp";
                _cmd += " @UpdUser  ='" + adjust.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adjust.AdjustNo + "'";
                _cmd += ",@StateApp  ='" + adjust.StateApp + "'"; 

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }


        }


        
        [HttpPost("[action]")]
        public IActionResult setInvenReserve(ReserveModel adjust)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReserveTrans";
                _cmd += " @UpdUser  ='" + adjust.UpdUser + "'";
                _cmd += ",@ReserveNo  ='" + adjust.ReserveNo + "'";
                _cmd += ",@ReserveDate  ='" + adjust.ReserveDate + "'";
                _cmd += ",@ReserveBy  ='" + adjust.ReserveBy + "'";
                _cmd += ",@ProjectNo  ='" + adjust.ProjectNo + "'";
                _cmd += ",@CmpId =" + adjust.CmpId;
                _cmd += ",@Remark  ='" + adjust.Remark + "'";
                _cmd += ",@WHId =" + adjust.WHId;
                _cmd += ",@WHLocId =" + adjust.WHLocId;
                _cmd += ", @ReserveType=" + adjust.ReserveType;



                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }


        }


         


         
        [HttpPost("[action]")]
        public IActionResult setInvenAppTrnas(invenAppModel invenApp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.[Inven_AppTrans]";
                _cmd += " @UpdUser  ='" + invenApp.AppBy + "'";
                _cmd += ",@DocNo  ='" + invenApp.DocNo + "'";
                _cmd += ",@StateApp  =" + invenApp.StateApp ;
                _cmd += ",@Type  ='" + invenApp.Type + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return Ok(msgretrun);
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }


        }


    }
}
