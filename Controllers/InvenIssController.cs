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
    public class InvenIssController : ApiController
    {
      

        // GET: api/InvenRcv/5
        [Route("api/InvenIss")]
        [HttpGet]
        public IHttpActionResult Get(string CmpId, string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getIssAll @CmpId=" + Convert.ToInt16(CmpId) + " , @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }

        // POST: api/InvenRcv
        [Route("api/InvenIss")]
        [HttpPost]
        public IHttpActionResult Post(IssueModel iss)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
              
                _cmd = "exec  dbo.Inven_setIssueTrans"; 
                _cmd += "  @UpdUser  ='" + iss.UpdUser + "'"; 
                _cmd += " ,@IssueNo  ='" + iss.IssueNo + "'";
                _cmd += " ,@IssueDate ='" + iss.IssueDate + "'";
                _cmd += " ,@IssueBy ='" + iss.IssueBy + "'";
                _cmd += " ,@CmpId =" + iss.CmpId; 
                _cmd += " ,@Remark  ='" + iss.Remark + "'"; 
                _cmd += " ,@DocRef =" + iss.DocRef;
                 _cmd += " ,@WHId =" + iss.WHId; 
                _cmd += " ,@WHLocId =" + iss.WHLocId; 
                _cmd += " ,@ProjectNo ='" + iss.ProjectNo + "'";
                //_cmd += ",@StateApp =" + iss.StateApp;
                //_cmd += ",@AppDate =" + iss.AppDate; 
                //_cmd += ",@AppTime =" + iss.AppTime; 
                //_cmd += ",@AppBy ='" + iss.AppBy + "'";


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
        [Route("api/InvenIss")]
        [HttpPut]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE: api/InvenRcv/5
        [Route("api/InvenIss")]
        [HttpDelete]
        public void Delete(string id)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Issue where IssueNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
