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
 
    public class InvenRcvController : ApiController
    {
        // GET: api/InvenRcv
        [Route("api/InvenRcv")]
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/InvenRcv/5
        [Route("api/InvenRcv")]
        [HttpGet]        
        public IHttpActionResult Get(string CmpId, string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getReceiveAll @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }

        // POST: api/InvenRcv
        [Route("api/InvenRcv")]
        [HttpPost]
        public IHttpActionResult Post(ReceiveModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReceiveTrans"; 
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@ReceiveNo  ='" + receive.ReceiveNo + "'"; 
                _cmd += ",@ReceiveDate  ='" + receive.ReceiveDate + "'"; 
                _cmd += ",@ReceiveBy  ='" + receive.ReceiveBy + "'"; 
                _cmd += ",@PurChaseNo  ='" + receive.PurChaseNo + "'";
                _cmd += ",@InvoiceNo  ='" + receive.InvoiceNo + "'"; 
                _cmd += ",@InvoiceDate  ='" + receive.InvoiceDate + "'"; 
                _cmd += ",@ReceiveType =" + receive.ReceiveType; 
                _cmd += ",@CmpId =" + receive.CmpId; 
                _cmd += ",@Remark  ='" + receive.Remark + "'";
                _cmd += ",@SupplierCode  ='" + receive.SupplierCode + "'";
                _cmd += ",@WHId =" + receive.SysWHId;
                _cmd += ",@WHLocId =" + receive.SysWHLocId;

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
        [Route("api/InvenRcv")]
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE: api/InvenRcv/5
        [Route("api/InvenRcv")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Receive where ReceiveNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
