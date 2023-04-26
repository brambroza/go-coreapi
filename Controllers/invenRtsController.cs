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
    public class invenRtsController : ApiController
    {
        // GET: api/invenRts
        // GET: api/InvenRcv/5
        [Route("api/InvenRts")]
        [HttpGet]
        public IHttpActionResult Get(string CmpId, string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getRtsAll @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }

        // POST: api/InvenRcv
        [Route("api/InvenRts")]
        [HttpPost]
        public IHttpActionResult Post(ReturnToSuplModel rts)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReturnToSuplTrans"; 
                _cmd += " @UpdUser  ='" + rts.UpdUser + "'"; 
                _cmd += ",@ReturnToSuplNo  ='" + rts.ReturnToSuplNo + "'"; 
                _cmd += ",@ReturnToSuplDate  ='" + rts.ReturnToSuplDate + "'"; 
                _cmd += ",@ReturnToSuplBy ='" + rts.ReturnToSuplBy + "'";
                _cmd += ",@PurChaseNo  ='" + rts.PurChaseNo + "'"; 
                _cmd += ",@CmpId =" + rts.CmpId; 
                _cmd += ",@Remark  ='" + rts.Remark + "'";
                _cmd += ",@ReturnType =" + rts.ReturnType; 
                _cmd += ",@SupplierCode ='" + rts.SupplierCode + "'";
                _cmd += ",@WHId =" + rts.WHId; 
                _cmd += ",@WHLocId =" + rts.WHLocId; 
              


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
        [Route("api/InvenRts")]
        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE: api/InvenRcv/5
        [Route("api/InvenRts")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.ReturnToSupl where ReturnToSuplNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
