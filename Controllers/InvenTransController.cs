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
   
    public class InvenTransController : ApiController
    {
        // GET: api/InvenTrans
         

        // GET: api/InvenTrans/5
        [Route("api/InvenTrans")]
        [HttpGet]
        public IHttpActionResult Get(int CmpId, string user , string TransNo)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getTransAll_ByDoc @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "',@DocNo='" + TransNo +"'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        [Route("api/InvenOnhand")]
        [HttpGet]
        public IHttpActionResult GetOnhand(int CmpId, string user, string TransNo)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getOnhand @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'  ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        // POST: api/InvenTrans
        [Route("api/InvenTrans")]
        [HttpPost]
        public void Post(List<InvenTransModel> Inven)
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


        [Route("api/InvenApp")]
        [HttpPost]
        public IHttpActionResult App(AdjustModel adjust)
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


        [Route("api/InvenReserve")]
        [HttpPost]
        public IHttpActionResult Post(ReserveModel adjust)
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


        // PUT: api/InvenTrans/5
        [Route("api/InvenTrans")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/InvenTrans/5
        [Route("api/InvenTrans")]
        [HttpDelete]
        public void Delete(int id)
        {
            try
            {

            }
            catch
            {

            }

        }


        [Route("api/InvenAppTrans")]
        [HttpPost]
        public IHttpActionResult AppTrans(invenAppModel invenApp)
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
