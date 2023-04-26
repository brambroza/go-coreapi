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
    
    public class InvenAdjustController : ApiController
    { 

        // GET: api/InvenAdjust/5
        [Route("api/InvenAdjust")]
        [HttpGet]
        public IHttpActionResult Get(string CmpId, string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_GetAdjustAll @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        // POST: api/InvenAdjust
        [Route("api/InvenAdjust")]
        [HttpPost]
        public IHttpActionResult Post(AdjustModel adjust )
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setAdjustTrans";
                _cmd += " @UpdUser  ='" + adjust.UpdUser + "'";
                _cmd += ",@AdjustNo  ='" + adjust.AdjustNo + "'"; 
                _cmd += ",@AdjustDate  ='" + adjust.AdjustDate + "'"; 
                _cmd += ",@AdjustBy  ='" + adjust.AdjustBy + "'";
                _cmd += ",@PurChaseNo  ='" + adjust.PurChaseNo + "'";
                _cmd += ",@CmpId =" + adjust.CmpId; 
                _cmd += ",@Remark  ='" + adjust.Remark + "'";
                _cmd += ",@WHId =" + adjust.WHId;
                _cmd += ",@WHLocId =" + adjust.WHLocId;
                _cmd += ", @AdjustType=" + adjust.AdjustType;

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



        // PUT: api/InvenAdjust/5
        [Route("api/InvenAdjust")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/InvenAdjust/5
        [Route("api/InvenAdjust")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Adjust where AdjustNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
