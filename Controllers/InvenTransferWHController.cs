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
   
    public class InvenTransferWHController : ApiController
    {
        // GET: api/InvenTransferWH
        [Route("api/InvenTransferWH")]
        [HttpGet]
        public IHttpActionResult Get(int CmpId, string user )
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHAll] @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        [Route("api/InvenTransferWHRcvlist")]
        [HttpGet]
        public IHttpActionResult GetTransferWHRcvlist(int CmpId, string user)
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHRcvAll] @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }



        [Route("api/InvenTransferWHProdWaidRcv")]
        [HttpGet]
        public IHttpActionResult Get(int CmpId)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterforRcvTransferWH  @CmpId=" + Convert.ToInt16(CmpId) + " ";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }



        // POST: api/InvenTransferWH
        [Route("api/InvenTransferWH")]
        [HttpPost]
        public IHttpActionResult Post(TransferWHModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTrans"; 
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'"; 
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId; 
                _cmd += ",@Remark  ='" + TransWH.Remark + "'"; 
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.WHId; 
                _cmd += ",@WHLocId =" + TransWH.WHLocId;
                _cmd += ",@WHToId =" + TransWH.WHToId; 
                _cmd += ",@WHLocToId =" + TransWH.WHLocToId;



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

        // POST: api/InvenTransferWH
        [Route("api/InvenTransferWHRcv")]
        [HttpPost]
        public IHttpActionResult InvenTransferWHRcv(TransferWHRcvModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTransRcv";
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'";
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId;
                _cmd += ",@Remark  ='" + TransWH.Remark + "'";
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.WHId;
                _cmd += ",@WHLocId =" + TransWH.WHLocId; 



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



        // PUT: api/InvenTransferWH/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/InvenTransferWH/5
        [Route("api/InvenTransferWH")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.TrasferWH where TransferWHNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
