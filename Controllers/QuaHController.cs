using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;



namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]


    public class QuaHController : ControllerBase
    {


        // GET: api/QuaH/5 
        [HttpGet]
        [Route("api/QuaH")]

        public IActionResult Get([FromQuery] string id, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.getQuatationAll @CmpId=" + Convert.ToInt16(id) + ", @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }



        [HttpGet]
        [Route("api/QuaHState")]
        public IActionResult GetApp([FromQuery] string id, [FromQuery] string state)
        {
            string _cmd;
            _cmd = "exec dbo.getQuatationapprove @CmpId=" + Convert.ToInt16(id);
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        // POST: api/QuaH
        [Route("api/QuaH")]
        [HttpPost]
        public IActionResult Post([FromQuery] Quatation quatation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setQuatation @QuatationNo='" + quatation.QuatationNo + "' ,@QuatationDate='" + quatation.QuatationDate + "' ,@QuatationBy='" + quatation.QuatationBy + "'";
                _cmd += " ,@QuatationState=" + quatation.QuatationState;
                _cmd += " ,@CustomerCode='" + quatation.CustomerCode + "'";
                _cmd += " ,@CreditType=" + quatation.CreditType;
                _cmd += " ,@CreditDate=" + quatation.CreditDate;
                _cmd += " ,@ProjectName='" + Tool.Tool.validateStr(quatation.ProjectName) + "'";
                _cmd += " ,@ReferCode='" + Tool.Tool.validateStr(quatation.ReferCode) + "'";
                _cmd += " ,@VatType=" + quatation.VatType;
                _cmd += " ,@Remark='" + quatation.Remark + "'";
                _cmd += " ,@Note='" + quatation.Note + "'";
                _cmd += " ,@QuatationAmt=" + quatation.QuatationAmt;
                _cmd += " ,@QuatationDisPer=" + quatation.QuatationDisPer;
                _cmd += " ,@QuatationDisAmt=" + quatation.QuatationDisAmt;
                _cmd += " ,@QuatationNetAmt=" + quatation.QuatationNetAmt;
                _cmd += " ,@QuatationVatAmt=" + quatation.QuatationVatAmt;
                _cmd += " ,@QuatationGrandAmt=" + quatation.QuatationGrandAmt;
                _cmd += " ,@WithholdingTaxState=" + quatation.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState=" + quatation.ShowSignatureState;
                _cmd += " ,@CmpId=" + quatation.CmpId;
                _cmd += " ,@PriceStand='" + quatation.PriceStand + "'";
                _cmd += " ,@PaymentDue='" + quatation.PaymentDue + "'";
                _cmd += " ,@Shipping='" + quatation.Shipping + "'";
                _cmd += " ,@RevNo=" + quatation.RevNo;
                _cmd += " ,@CustContact='" + Tool.Tool.validateStr(quatation.CustomerContactName) + "'";
                _cmd += ", @Jobtype=" + quatation.Jobtype;

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


        [Route("api/QuaHCopy")]
        [HttpPost]
        public IActionResult QuaHCopy(QuatationCopy quatation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setQuatationCopy @QuatationNo='" + quatation.QuatationNo + "'";
                _cmd += ", @QuatationNoNew ='" + quatation.QuatationNoNew + "'";
                _cmd += " ,@RevNo=" + quatation.RevNo;


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



        // PUT: api/QuaH/5
        [Route("api/QuaHApp")]
        [HttpGet]
        public IActionResult QuaHApp(string id, string DocNo, int RevNo, string user)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setQuatationApp @CmpId=" + Convert.ToInt16(id) + " , @DocNo='" + DocNo + "' , @RevNo =" + RevNo + ",@User='" + user + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    linenotiapp(DocNo);
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


        [Route("api/QuaHSendApp")]
        [HttpGet]
        public IActionResult QuaHSendApp([FromQuery] string id, [FromQuery] string DocNo, [FromQuery] int RevNo, [FromQuery] string user)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setQuatationSendApp @CmpId=" + Convert.ToInt16(id) + " , @DocNo='" + DocNo + "' , @RevNo =" + RevNo + ",@User='" + user + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    var x = linenotisendapp(DocNo);

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




        // DELETE: api/QuaH/5
        [Route("api/QuaH")]
        [HttpDelete]
        public IActionResult Delete(string id, int RevNo)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {

                string _cmd = "";
                _cmd = "delete from mdb.Quatation where  QuatationNo='" + id + "' and RevNo=" + RevNo;

                DB.DBConn.ExecuteOnly(_cmd);
                _cmd = "delete from mdb.Quatation_Detail where  QuatationNo='" + id + "'  and RevNo=" + RevNo;

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


        [Route("api/QuaHRev")]
        [HttpGet]
        public IActionResult Get(string id, string DocNo, int RevNo)
        {
            string _cmd;
            _cmd = "exec dbo.getQuatation @CmpId=" + Convert.ToInt16(id) + " , @DocNo='" + DocNo + "' , @RevNo =" + RevNo;
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }





        [HttpGet]
        [Route("api/salesbom")]
        public IActionResult salesbomGet([FromQuery] int id)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_All @CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpGet]
        [Route("api/salesbomRev")]
        public IActionResult salesbomGetR(int id, string bomno, int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_Rev  @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpGet]
        [Route("api/salesbomD")]
        public IActionResult salesbomDGet(int id, string bomno, int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_D @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpGet]
        [Route("api/salesbomF")]
        public IActionResult salesbomFGet([FromQuery] int id, [FromQuery] string bomno, [FromQuery] int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_F @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpGet]
        [Route("api/salesbomA")]
        public IActionResult salesbomAGet([FromQuery] int id, [FromQuery] string bomno, [FromQuery] int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_A @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId=" + id + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }








        [HttpPost]
        [Route("api/salesbom")]
        public IActionResult Post(SalesBom salebom)
        {



            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBom";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@UpdDate ='" + salebom.UpdDate + "'";
                _cmd += " ,@UpdTime ='" + salebom.UpdTime + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@Rev =" + salebom.Rev;
                _cmd += " ,@BomBy  ='" + salebom.BomBy + "'";
                _cmd += " ,@SaleName  ='" + salebom.SaleName + "'";
                _cmd += " ,@CustomerName  ='" + salebom.CustomerName + "'";
                _cmd += " ,@CustomerContactName  ='" + salebom.CustomerContactName + "'";
                _cmd += " ,@CustomerContactPhone  ='" + salebom.CustomerContactPhone + "'";
                _cmd += " ,@CustomerContactEmail  ='" + salebom.CustomerContactEmail + "'";
                _cmd += " ,@ProjectName  ='" + salebom.ProjectName + "'";
                _cmd += " ,@ProjectStatus =" + salebom.ProjectStatus;
                _cmd += " ,@Remark ='" + salebom.Remark + "'";
                _cmd += " ,@CmpId =" + salebom.CmpId;


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




        [HttpPost]
        [Route("api/salesbomapprove")]
        public IActionResult BomApp(string id, string DocNo, int RevNo, string user)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.sp_SetSalesBomApp @CmpId=" + Convert.ToInt16(id) + " , @DocNo='" + DocNo + "' , @RevNo =" + RevNo + ",@User='" + user + "'";

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





        [HttpPost]
        [Route("api/salesbomaction")]
        public void PostbomAction(List<SalesBom_Action> salebomA)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {


                string _cmd = "";
                if (salebomA.Count > 0)
                {
                    _cmd = "Delete From dbo.SalesBom_Action where 	WHERE BomNo = '" + salebomA[0].BomNo + "' AND Rev = '" + salebomA[0].Rev + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomA.Count; i++)
                {

                    _cmd = "exec  dbo.sp_SetSalesBom_Action";
                    _cmd += " @UpdUser  ='" + salebomA[i].UpdUser + "'";
                    _cmd += ",@UpdDate ='" + salebomA[i].UpdDate + "'";
                    _cmd += ",@UpdTime ='" + salebomA[i].UpdTime + "'";
                    _cmd += ",@BomNo  ='" + salebomA[i].BomNo + "'";
                    _cmd += ",@Rev =" + salebomA[i].Rev;
                    _cmd += ",@Seq =" + salebomA[i].Seq;
                    _cmd += ",@DescActions  ='" + salebomA[i].DescActions + "'";
                    _cmd += ",@DateActions ='" + salebomA[i].DateActions + "'";


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




        [HttpPost]
        [Route("api/salesbomfile")]
        public void PostbomFile(List<SalesBom_File> salebomF)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {


                string _cmd = "";
                if (salebomF.Count > 0)
                {
                    _cmd = "Delete From dbo.SalesBom_File where 	WHERE BomNo = '" + salebomF[0].BomNo + "' AND Rev = '" + salebomF[0].Rev + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomF.Count; i++)
                {

                    _cmd = "exec  dbo.sp_SetSalesBom_File";
                    _cmd += " @UpdUser  ='" + salebomF[i].UpdUser + "'";
                    _cmd += ",@UpdDate ='" + salebomF[i].UpdDate + "'";
                    _cmd += ",@UpdTime ='" + salebomF[i].UpdTime + "'";
                    _cmd += ",@BomNo  ='" + salebomF[i].BomNo + "'";
                    _cmd += ",@Rev =" + salebomF[i].Rev;
                    _cmd += ",@Seq =" + salebomF[i].Seq;
                    _cmd += ",@FileName  ='" + salebomF[i].FileName + "'";
                    _cmd += ",@FileType ='" + salebomF[i].FileType + "'";
                    _cmd += ",@FlieSize ='" + salebomF[i].FlieSize + "'";
                    _cmd += ",@Remark  ='" + salebomF[i].Remark + "'";


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




        [HttpPost]
        [Route("api/salesbomD")]
        public void postsalesbomD(List<SalesBom_Detail> salebomD)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();


            try
            {

                string _cmd;
                if (salebomD.Count > 0)
                {
                    _cmd = "Delete From dbo.SalesBom_Detail where 	WHERE BomNo = '" + salebomD[0].BomNo + "' AND Rev = '" + salebomD[0].Rev + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomD.Count; i++)
                {

                    _cmd = "exec  dbo.sp_SetSalesBom_Detail";

                    _cmd += "  @UpdUser  ='" + salebomD[i].UpdUser + "'";
                    _cmd += ",@UpdDate ='" + salebomD[i].UpdDate + "'";
                    _cmd += ",@UpdTime ='" + salebomD[i].UpdTime + "'";
                    _cmd += ",@BomNo  ='" + salebomD[i].BomNo + "'";
                    _cmd += ",@Rev =" + salebomD[i].Rev;
                    _cmd += ",@Seq =" + salebomD[i].Seq;
                    _cmd += ",@PartNo  ='" + salebomD[i].PartNo + "'";
                    _cmd += ",@Descriptions  ='" + salebomD[i].Descriptions + "'";
                    _cmd += ",@Qty =" + salebomD[i].Qty;
                    _cmd += ",@QtyBal =" + salebomD[i].QtyBal;
                    _cmd += ",@DeliveryDate ='" + salebomD[i].DeliveryDate + "'";
                    _cmd += ",@BalCheckDate ='" + salebomD[i].BalCheckDate + "'";
                    _cmd += ",@Remark  ='" + salebomD[i].Remark + "'";
                    _cmd += ",@UnitCode  ='" + salebomD[i].UnitCode + "'";
                    _cmd += ",@UnitPrice =" + salebomD[i].UnitPrice;
                    _cmd += ",@Amount =" + salebomD[i].Amount;
                    _cmd += ",@ReplaceStatus =" + salebomD[i].ReplaceStatus;


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


        //[HttpDelete]
        //[Route("api/SaleBomD")]


        [HttpGet]
        [Route("api/linenotisendapp")]
        public string linenotisendapp(string qno)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getNotisendappqt '" + qno + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            StringBuilder _msg;

            foreach (DataRow r in dt.Rows)
            {
                _msg = new StringBuilder();
                _msg.Append(" ส่งอนุมัติ ใบเสนอราคา");
                _msg.AppendLine();
                _msg.Append("ชื่อลูกค้า : " + r["CustomerName"].ToString());
                _msg.AppendLine();
                _msg.Append("เลขใบเสนอราคา : " + r["QuatationNo"].ToString());
                _msg.AppendLine();
                _msg.Append("วันที่ : " + r["QuatationDate"].ToString());
                _msg.AppendLine();
                _msg.Append("ผู้สร้างใบเสนอรา : " + r["QuatationBy"].ToString());
                _msg.AppendLine();
                _msg.Append("โปรเจค : " + r["ProjectName"].ToString());
                _msg.AppendLine();
                _msg.Append("อ้างอิง : " + r["ReferCode"].ToString());
                _msg.AppendLine();
                _msg.Append("Note : " + r["Remark"].ToString());

                lineNotify(_msg.ToString());
            }





            return "value";
        }


        [HttpGet]
        [Route("api/linenotiapp")]
        public string linenotiapp(string qno)
        {
            string _cmd = "";
            _cmd = "exec  dbo.sp_getNotiappqt '" + qno + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            StringBuilder _msg;

            foreach (DataRow r in dt.Rows)
            {
                _msg = new StringBuilder();
                _msg.Append(" ใบเสนอราคาเลขที่ " + r["QuatationNo"].ToString() + " อนุมัติแล้ว");
                _msg.AppendLine();
                _msg.Append(" อนุมัติโดย : " + r["QuatationBy"].ToString());


                lineNotify(_msg.ToString());
            }





            return "value";
        }



        [HttpGet]
        [Route("api/lineNotify")]
        private void lineNotify(string msg)
        {
            string token = "8LtACGcDqZS6ZouELpfLZPc8Trl6LWgbEErI0pgjSeg";
            token = "pRCg56EkubWTcMhvkgC64GBZTTZkCG2e0bMAZ2g1JFg";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", msg);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


    }
}
