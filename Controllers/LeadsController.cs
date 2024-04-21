using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]
    public class LeadsController : ControllerBase
    {


        // GET: api/Leads/5
        [HttpGet]
        [Route("api/Leads")]
        public IActionResult Get([FromQuery] int cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getLeadsTrans]   @CmpId =" + cmpid + ",@user ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet]
        [Route("api/customerleads")]
        public IActionResult GetCust([FromQuery] int cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCustomerLeads]   @CmpId =" + cmpid + ",@UserLogin ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }






        [Route("api/UserSaleAsgin")]
        [HttpGet]
        public IActionResult getUserSaleAsgin([FromQuery] string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUserSaleAsgin] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        // get: api/ LeadsTask
        [HttpGet]
        [Route("api/LeadsTask")]
        public IActionResult getLeadsTask([FromQuery] int cmpid, [FromQuery] string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getLeadsTaskTrans]   @CmpId =" + cmpid + ",@user ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        // POST: api/ Leads 
        [HttpPost("[action]")]
        
        public IActionResult Leadsnew(Leadsnew leads)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.LeadsTrans_new";
                _cmd += " @UpdUser  ='" + leads.UpdUser + "'";
                _cmd += ",@CustCodeNo  ='" + leads.CustCodeNo + "'";
                _cmd += ",@TransDate ='" + leads.TransDate + "'";
                _cmd += ",@CustRefTypeId =" + leads.CustRefTypeId;
                _cmd += ",@Topic ='" + leads.Topic + "'";
                _cmd += ",@ContactName  ='" + leads.ContactName + "'";
                _cmd += ",@ContactEmail  ='" + leads.ContactEmail + "'";
                _cmd += ",@ContactPhone  ='" + leads.ContactPhone + "'";
                _cmd += ",@ContactPosition  ='" + leads.ContactPosition + "'";
                _cmd += ",@LeadDescription  ='" + leads.LeadDescription + "'";
                _cmd += ",@CmpId  ='" + leads.CmpId + "'";
                _cmd += ",@Seq  ='" + leads.Seq + "'";



                if (DB.DBConn.ExecuteOnly(_cmd) == false)
                {



                    msgretrun.ReturnCode = "404";
                    msgretrun.Msg = "บันทึกผิดพลาด";
                    return Ok(msgretrun);



                }







                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);


            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }




        // POST: api/ Leads 
        [HttpPost]
        [Route("api/Leads")]
        public IActionResult Post(Leads leads)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.LeadsTrans";
                _cmd += " @UpdUser  ='" + leads.UpdUser + "'";
                _cmd += ",@CustCodeNo  ='" + leads.CustCodeNo + "'";
                _cmd += ",@TransDate ='" + Tool.Tool.validatestring(leads.TransDate) + "'";
                _cmd += ",@CustName  ='" + leads.CustName + "'";
                _cmd += ",@CustRefTypeId =" + leads.CustRefTypeId;
                _cmd += ",@Topic ='" + leads.Topic + "'";
                _cmd += ",@Phone  ='" + leads.Phone + "'";
                _cmd += ",@Mobile  ='" + leads.Mobile + "'";
                _cmd += ",@Email  ='" + leads.Email + "'";
                _cmd += ",@CompanyName  ='" + leads.CompanyName + "'";
                _cmd += ",@CompanyAddr  ='" + leads.CompanyAddr + "'";
                _cmd += ",@CustNickName  ='" + leads.CustNickName + "'";



                if (DB.DBConn.ExecuteOnly(_cmd))
                {

                    if (leads.LeadsTasks.Count > 0)
                    {

                        _cmd = "delete from mdb.Leads_Task where CustCodeNo ='" + leads.CustCodeNo + "'";
                        DB.DBConn.ExecuteOnly(_cmd);

                        for (int i = 0; i < leads.LeadsTasks.Count; i++)
                        {
                            _cmd = "exec  dbo.Leads_TaskTrans";
                            _cmd += " @UpdUser  ='" + leads.LeadsTasks[i].UpdUser + "'";
                            _cmd += ",@CustCodeNo  ='" + leads.LeadsTasks[i].CustCodeNo + "'";
                            _cmd += ",@Seq =" + int.Parse(i.ToString());
                            _cmd += ",@Description  ='" + Tool.Tool.validateStr(leads.LeadsTasks[i].Description) + "'";
                            _cmd += ",@TransDate ='" + Tool.Tool.validatestring(leads.LeadsTasks[i].TransDate) + "'";
                            if (!DB.DBConn.ExecuteOnly(_cmd))
                            {

                                msgretrun.ReturnCode = "404";
                                msgretrun.Msg = "บันทึกผิดพลาด";
                                return Ok(msgretrun);

                            }


                        }

                    }


                    if (leads.leadsAsigns.Count > 0)
                    {

                        _cmd = "delete from mdb.Leads_Asign where CustCodeNo ='" + leads.CustCodeNo + "'";
                        DB.DBConn.ExecuteOnly(_cmd);

                        for (int i = 0; i < leads.leadsAsigns.Count; i++)
                        {
                            _cmd = "exec  dbo.Leads_AsignTrans";
                            _cmd += " @UpdUser  ='" + leads.UpdUser + "'";
                            _cmd += ",@CustCodeNo  ='" + leads.CustCodeNo + "'";
                            _cmd += ",@Seq =" + int.Parse(i.ToString());
                            _cmd += ",@UserName  ='" + leads.leadsAsigns[i].UserName + "'"; ;
                            if (!DB.DBConn.ExecuteOnly(_cmd))
                            {

                                msgretrun.ReturnCode = "404";
                                msgretrun.Msg = "บันทึกผิดพลาด";
                                return Ok(msgretrun);

                            }


                        }

                    }







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


        [HttpPost]
        [Route("api/LeadsTask")]
        public IActionResult setLeadsTask(List<Leads_Task> ls)
        {
            MsgReturn msgretrun = new MsgReturn();

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {

                string _cmd = "";

                if (ls.Count > 0)
                {
                    _cmd = "Delete From mdb.Leads_Task where CustCodeNo='" + ls[0].CustCodeNo + "'";
                    _cmd += " and  Seq=" + ls[0].Seq;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < ls.Count; i++)
                {


                    _cmd = "exec  dbo.Leads_TaskTrans";
                    _cmd += ",@UpdUser  ='" + ls[i].UpdUser + "'";
                    _cmd += ",@CustCodeNo  ='" + ls[i].CustCodeNo + "'";
                    _cmd += ",@TransDate =" + ls[i].TransDate;
                    _cmd += ",@Seq =" + ls[i].Seq;
                    _cmd += ",@Description  ='" + ls[i].Description + "'";

                }

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    DB.DBConn.Tran.Commit();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                    return Ok(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                return Ok(msgretrun);
            }
        }



        [HttpPost]
        [Route("api/LeadsQualify")]
        public IActionResult setLeadsQualify(LeadsQualify leads)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.LeadsQualify";
                _cmd += " @UpdUser  ='" + leads.UpdUser + "'";
                _cmd += ",@CustCodeNo  ='" + leads.CustCodeNo + "'";
                _cmd += ",@QualifyState =" + leads.QualifyState;

                if (DB.DBConn.ExecuteOnly(_cmd))
                {

                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Qualify Success !!";
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



        // PUT: api/Leads/5
        [HttpPut]
        [Route("api/Leads")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Leads/5
        [HttpDelete]
        [Route("api/Leads")]
        public IActionResult Delete(int id)
        {

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "delete from mdb.Leads  where CustCodeNo ='" + id + "'";
                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
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
