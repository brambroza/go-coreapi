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
    public class InvenRetruntostockController : ApiController
    {
        [Route("api/InvenRtc")]
        [HttpGet]
        public IHttpActionResult Get(string CmpId, string user)
        {
            string _cmd;
             _cmd = "exec dbo.Inven_getRctAll @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }

        // POST: api/InvenRcv
        [Route("api/InvenRtc")]
        [HttpPost]
        public IHttpActionResult Post(ReturnToStock rc)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.Inven_setReturnToStockTrans"; 
                _cmd += " @UpdUser  ='" + rc.UpdUser + "'"; 
                _cmd += ",@ReturnToStockNo  ='" + rc.ReturnToStockNo + "'"; 
                _cmd += ",@ReturnToStockDate ='" + rc.ReturnToStockDate + "'";
                _cmd += ",@ReturnToStockBy ='" + rc.ReturnToStockBy + "'";
                _cmd += ",@IssueNo  ='" + rc.IssueNo + "'";
                _cmd += ",@CmpId =" + rc.CmpId; 
                _cmd += ",@Remark  ='" + rc.Remark + "'"; 
                _cmd += ",@SysWHId =" + rc.SysWHId; 
                _cmd += ",@SysWHLocId =" + rc.SysWHLocId;

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

        // PUT: api/InvenRcv/5
        [Route("api/InvenRtc")]
        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE: api/InvenRcv/5
        [Route("api/InvenRtc")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.ReturnToStock where [ReturnToStockNo]='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
