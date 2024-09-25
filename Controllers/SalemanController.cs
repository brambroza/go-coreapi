using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers
{
    
    public class SalemanController : ApiController
    {
        // GET: api/Saleman
        [HttpGet]
        [Route("api/Saleman")]
        public IEnumerable<string> Get()
        {
             return new string[] { "value1", "value2" };
        }

        // GET: api/Saleman/5
        [HttpGet]
        [Route("api/Saleman")]
        public IHttpActionResult Get( int CmpId , string user)
        {
          
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getSalemantrackAll]  @User='" + user + "',@CmpId =" + CmpId ;
            dt = DB.DBConn.GetDataTable(_cmd); 
            return Ok(dt);
        }

        [Route("api/SalemanAsgin")]
        [HttpGet]
        public IHttpActionResult getUserSaleAsgin(string id)
        {
            string _QuotationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getSalemanAsgin] @CmpId=" + _QuotationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }



        // POST: api/Saleman
        [HttpPost]
        [Route("api/Saleman")]
        public IHttpActionResult Post(SalemanTrack sm)
        {
            MsgReturn msgReturn = new MsgReturn();
             

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.SalemanTrack_Trans";
                _cmd += " @UpdUser  ='" + sm.UpdUser + "'";
                _cmd += ",@SalemanTrackNo  ='" + sm.SalemanTrackNo + "'";
                _cmd += ",@TransDate ='"  + sm.TransDate + "'";
                _cmd += ",@CustomerName  ='" + sm.CustomerName + "'";
                _cmd += ",@Address  ='" + sm.Address + "'";
                _cmd += ",@Email  ='" + sm.Email + "'";
                _cmd += ",@MobileNo ='" + sm.MobileNo + "'";
                _cmd += ",@Contact ='" + sm.Contact + "'";
                _cmd += ",@BusinessType ='" + sm.BusinessType + "'";
                _cmd += ",@ReferOrigin ='" + sm.ReferOrigin + "'";
                _cmd += ",@Seq =" + sm.Seq;
                _cmd += ",@ContactStatus  ='" + sm.ContactStatus + "'";
                _cmd += ",@Description  ='" + sm.Description + "'";
                _cmd += ",@ActionDate ='" + sm.ActionDate + "'";
                _cmd += ",@SaleStatus =" + sm.SaleStatus;

                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                    msgReturn.ReturnCode = "404";
                    msgReturn.Msg = "บันทึกผิดพลาด";
                    return Ok(msgReturn);

                }
                else
                {

                    if (sm.salemanTasks.Count > 0)
                    {
                        for (int i = 0; i < sm.salemanTasks.Count; i++)
                        {
                            _cmd = "exec  dbo.SalemanTask_Trans";
                            _cmd += " @UpdUser  ='" + sm.salemanTasks[i].UpdUser + "'";
                            _cmd += ",@SalemanTrackNo  ='" + sm.salemanTasks[i].SalemanTrackNo + "'";
                            _cmd += ",@Seq =" + int.Parse(i.ToString());
                            _cmd += ",@Description  ='" + sm.salemanTasks[i].Description + "'";
                            _cmd += ",@ActionDate ='" + Tool.Tool.validatestring(sm.salemanTasks[i].ActionDate) + "'";
                            _cmd += ",@Status=" + sm.salemanTasks[i].Status;
                            if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                            {
                                DB.DBConn.Tran.Rollback();
                                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                                msgReturn.ReturnCode = "404";
                                msgReturn.Msg = "บันทึกผิดพลาด";
                                return Ok(msgReturn);

                            }


                        }

                    }

                    if (sm.salemanAsigns.Count > 0)
                    {

                        _cmd = "delete from mdb.Saleman_Asign where SalemanTrackNo ='" + sm.SalemanTrackNo + "'";
                        DB.DBConn.ExecuteOnly(_cmd);

                        for (int i = 0; i < sm.salemanAsigns.Count; i++)
                        {
                            _cmd = "exec  dbo.Saleman_AsignTrans";
                            _cmd += " @UpdUser  ='" + sm.UpdUser + "'";
                            _cmd += ",@SalemanTrackNo  ='" + sm.SalemanTrackNo + "'";
                            _cmd += ",@Seq =" + int.Parse(i.ToString());
                            _cmd += ",@UserName  ='" + sm.salemanAsigns[i].UserName + "'"; ;
                            if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                            {

                                msgReturn.ReturnCode = "404";
                                msgReturn.Msg = "บันทึกผิดพลาด";
                                return Ok(msgReturn);

                            }


                        }

                    }



                }





                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgReturn.ReturnCode = "200";
                msgReturn.Msg = "บันทึกสำเร็จ";
                return Ok(msgReturn);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgReturn.ReturnCode = "404";
                msgReturn.Msg = "บันทึกผิดพลาด";
                return Ok(msgReturn);
            }



        }



        [HttpPost]
        [Route("api/SalemanToQuotation")]
        public IHttpActionResult setSalemanToQuotation(SalemanApp salemanApp)
        {
            MsgReturn msgReturn = new MsgReturn();
            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.SalemanApp";
                _cmd += " @UpdUser  ='" + salemanApp.UpdUser + "'";
                _cmd += ",@SalemanTrackNo  ='" + salemanApp.SalemanTrackNo + "'";
                _cmd += ",@SaleStatus =" + salemanApp.SaleStatus;

                if (DB.DBConn.ExecuteOnly(_cmd))
                {

                    msgReturn.ReturnCode = "200";
                    msgReturn.Msg = "Qualify Success !!";
                    return Ok(msgReturn);
                }
                else
                {
                    msgReturn.ReturnCode = "400";
                    msgReturn.Msg = "Error !!";
                    return Ok(msgReturn);
                }

            }
            catch
            {
                msgReturn.ReturnCode = "400";
                msgReturn.Msg = "Error !!";
                return Ok(msgReturn);
            }
        }


        // PUT: api/Saleman/5
        [HttpPut]
        [Route("api/Saleman")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Saleman/5
        [HttpDelete]
        [Route("api/Saleman")]
        public void Delete(string id)
        {

            try
            {
                string _cmd = "";
                _cmd = "Delete from mdb.SalemanTrack where SalemanTrackNo='" + id + "'";
                _cmd += "Delete from mdb.Saleman_Asign where SalemanTrackNo='" + id + "'";
                _cmd += "Delete from mdb.SalemanTask where SalemanTrackNo='" + id + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
