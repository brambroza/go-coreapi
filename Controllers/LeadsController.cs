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
    public class LeadsController : ApiController
    {
        // GET: api/Leads
        [HttpGet]
        [Route("api/Leads")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Leads/5
        [HttpGet]
        [Route("api/Leads")]
        public IHttpActionResult Get(int cmpid , string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getLeadsTrans]   @CmpId =" + cmpid + ",@user ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }

        [HttpGet]
        [Route("api/customerleads")]
        public IHttpActionResult GetCust(int cmpid, string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getCustomerLeads]   @CmpId =" + cmpid + ",@UserLogin ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }






        [Route("api/UserSaleAsgin")]
        [HttpGet]
        public IHttpActionResult getUserSaleAsgin(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getUserSaleAsgin] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }



        // get: api/ LeadsTask
        [HttpGet]
        [Route("api/LeadsTask")]
        public IHttpActionResult getLeadsTask(int cmpid, string user)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getLeadsTaskTrans]   @CmpId =" + cmpid + ",@user ='" + user + "' ";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }



        // POST: api/ Leads 
        [HttpPost]
        [Route("api/Leadsnew")]
        public IHttpActionResult Post(Leadsnew leads)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.LeadsTrans_new";
                _cmd += " @UpdUser  ='" + leads.UpdUser + "'";
                _cmd += ",@CustCodeNo  ='" + leads.CustCodeNo + "'";
                _cmd += ",@TransDate ='" +  leads.TransDate  + "'"; 
                _cmd += ",@CustRefTypeId =" + leads.CustRefTypeId;
                _cmd += ",@Topic ='" + leads.Topic + "'";
                _cmd += ",@ContactName  ='" + leads.ContactName + "'";
                _cmd += ",@ContactEmail  ='" + leads.ContactEmail + "'";
                _cmd += ",@ContactPhone  ='" + leads.ContactPhone + "'";
                _cmd += ",@ContactPosition  ='" + leads.ContactPosition + "'";
                _cmd += ",@LeadDescription  ='" + leads.LeadDescription + "'";
                _cmd += ",@CmpId  ='" + leads.CmpId + "'";
                _cmd += ",@Seq  ='" + leads.Seq + "'";



                if ( DB.DBConn.ExecuteOnly(_cmd) == false  )
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
        public IHttpActionResult Post(Leads leads)
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
                            _cmd += ",@Description  ='" + Tool.Tool.validateStr(leads.LeadsTasks[i].Description )+ "'";
                            _cmd += ",@TransDate ='" + Tool.Tool.validatestring(leads.LeadsTasks[i].TransDate) + "'";
                            if (!DB.DBConn.ExecuteOnly(_cmd )   )
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
        public IHttpActionResult setLeadsTask (List<Leads_Task> ls)
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
        public IHttpActionResult setLeadsQualify(LeadsQualify leads)
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
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Leads/5
        [HttpDelete]
        [Route("api/Leads")]
        public IHttpActionResult Delete(int id)
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
