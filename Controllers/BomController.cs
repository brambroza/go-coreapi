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


    public class BomController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getSalesbom([FromQuery] string id, [FromQuery] string user)
        {

            DataTable dt = new System.Data.DataTable();
            DataTable dtItem = new System.Data.DataTable();
            DataTable dtItemPrice = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_All @CmpId='" + id + "' , @user='" + user + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.sp_getSaleBomItem_All @CmpId='" + id + "' , @user='" + user + "'";
            dtItem = DB.DBConn.GetDataTable(_cmd);


            _cmd = "exec dbo.sp_getSaleBomItem_Price_All @CmpId='" + id + "' , @user='" + user + "'";
            dtItemPrice = DB.DBConn.GetDataTable(_cmd);


            List<SalesBom> bomList = new List<SalesBom>();



            foreach (DataRow r in dt.Rows)
            {

                var bom = new SalesBom();
                bom.BomNo = r["BomNo"].ToString();
                bom.BomBy = r["BomBy"].ToString();
                bom.BomDate = r["BomDate"].ToString();
                bom.SaleName = r["SaleName"].ToString();
                bom.CustomerName = r["CustomerName"].ToString();
                bom.CustomerContactName = r["CustomerContactName"].ToString();
                bom.CustomerContactEmail = r["CustomerContactEmail"].ToString();
                bom.CustomerContactPhone = r["CustomerContactPhone"].ToString();
                bom.ProjectName = r["ProjectName"].ToString();
                bom.ProjectStatus = Convert.ToInt32(r["ProjectStatus"]);
                bom.Remark = r["Remark"].ToString();
                bom.CmpId = r["CmpId"].ToString();
                bom.UpdUser = r["UpdUser"].ToString();
                bom.BomState = r["BomState"].ToString();
                bom.TicketId = Convert.ToInt64(r["TicketId"]);
                bom.items = new List<SalesBom_Detail>();
                foreach (DataRow x in dtItem.Select("BomNo='" + bom.BomNo + "' and RevNo=" + bom.RevNo + " and CmpId='" + bom.CmpId + "'"))
                {
                    var item = new SalesBom_Detail();
                    item.BomNo = bom.BomNo;
                    item.UpdUser = x["UpdUser"].ToString();
                    item.RevNo = bom.RevNo;
                    item.Seq = Convert.ToInt32(x["Seq"]);
                    item.ProdCode = x["ProdCode"].ToString();
                    item.ProdDescription = x["ProdDescription"].ToString();
                    item.Qty = Convert.ToDecimal(x["Qty"]);
                    item.UnitPrice = Convert.ToDecimal(x["UnitPrice"]);
                    item.UnitCode = x["UnitCode"].ToString();
                    item.Amt = Convert.ToDecimal(x["Amt"]);
                    item.CmpId = x["CmpId"].ToString();
                    item.ReplaceStatus = Convert.ToInt32(x["ReplaceStatus"]);

                    item.bomitemPrice = new List<SalesBom_Price_Item>();

                    foreach (DataRow i in dtItemPrice.Select("BomNo='" + bom.BomNo + "' and RevNo=" + bom.RevNo + " and CmpId='" + bom.CmpId + "' and ProdCode='" + item.ProdCode + "' and Seq=" + item.Seq))
                    {
                        var itemprice = new SalesBom_Price_Item();

                        itemprice.BomNo = bom.BomNo;
                        itemprice.UpdUser = i["UpdUser"].ToString();
                        itemprice.RevNo = bom.RevNo;
                        itemprice.Seq = Convert.ToInt32(i["Seq"]);
                        itemprice.ProdCode = i["ProdCode"].ToString();
                        itemprice.SupplierCode = i["SupplierCode"].ToString();
                        itemprice.SupplierName = i["SupplierName"].ToString();
                        itemprice.DeliveryDate = DateTime.Parse(i["DeliveryDate"].ToString());
                        itemprice.Qty = Convert.ToDecimal(i["Qty"]);
                        itemprice.QtyBal = Convert.ToDecimal(i["QtyBal"]);
                        itemprice.UnitPrice = Convert.ToDecimal(i["UnitPrice"]);
                        itemprice.UnitCode = i["UnitCode"].ToString();
                        itemprice.Amt = Convert.ToDecimal(i["Amt"]);
                        itemprice.CmpId = i["CmpId"].ToString();
                        itemprice.Remark = i["Remark"].ToString();
                        itemprice.PriceSeq = Convert.ToInt32(i["PriceSeq"]);


                        item.bomitemPrice.Add(itemprice);


                    }

                    bom.items.Add(item);

                }

                bomList.Add(bom);




            }

            return Ok(bomList);


        }


        [HttpGet]
        [Route("salesbomRev")]
        public IActionResult salesbomGetR([FromQuery] string cmpid, [FromQuery] string bomno, [FromQuery] int RevNo, [FromQuery] string user)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_Rev  @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId='" + cmpid + "' , @user='" + user + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }


        [HttpGet]
        [Route("salesbomD")]
        public IActionResult salesbomDGet([FromQuery] string cmpid, [FromQuery] string bomno, [FromQuery] int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_D @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }


        [HttpGet]
        [Route("salesbomF")]
        public IActionResult salesbomFGet([FromQuery] string cmpid, [FromQuery] string bomno, [FromQuery] int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_F @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt);
        }


        [HttpGet]
        [Route("salesbomA")]
        public IActionResult salesbomAGet([FromQuery] string cmpid, [FromQuery] string bomno, [FromQuery] int RevNo)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.sp_getSaleBom_A @BomNo='" + bomno + "' , @Rev=" + RevNo + " ,@CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }



        [HttpPost]
        [Route("salesbom")]
        public IActionResult Post(SalesBom salebom)
        {



            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd = "";
                _cmd = "exec  dbo.sp_SetSalesBom";
                _cmd += "  @UpdUser  ='" + salebom.UpdUser + "'";
                _cmd += " ,@BomNo  ='" + salebom.BomNo + "'";
                _cmd += " ,@RevNo =" + salebom.RevNo;
                _cmd += " ,@BomBy  ='" + salebom.BomBy + "'";
                _cmd += " ,@SaleName  ='" + salebom.SaleName + "'";
                _cmd += " ,@CustomerName  ='" + salebom.CustomerName + "'";
                _cmd += " ,@CustomerContactName  ='" + salebom.CustomerContactName + "'";
                _cmd += " ,@CustomerContactPhone  ='" + salebom.CustomerContactPhone + "'";
                _cmd += " ,@CustomerContactEmail  ='" + salebom.CustomerContactEmail + "'";
                _cmd += " ,@ProjectName  ='" + salebom.ProjectName + "'";
                _cmd += " ,@ProjectStatus =" + salebom.ProjectStatus;
                _cmd += " ,@Remark ='" + salebom.Remark + "'";
                _cmd += " ,@CmpId ='" + salebom.CmpId + "'";
                _cmd += " ,@BomState ='" + salebom.BomState + "'";
                _cmd += " ,@BomDate ='" + salebom.BomDate + "'";
                _cmd += " ,@TicketId =" + salebom.TicketId;


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
        [Route("salesbomD")]
        public IActionResult postsalesbomD(List<SalesBom_Detail> salebomD)
        {

            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();
            MsgReturn msgretrun = new MsgReturn();

            try
            {

                string _cmd;
                if (salebomD.Count > 0)
                {
                    _cmd = "Delete From dbo.SalesBom_Detail where 	WHERE BomNo = '" + salebomD[0].BomNo + "' AND Rev = '" + salebomD[0].RevNo + "'";
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < salebomD.Count; i++)
                {

                    _cmd = "exec  dbo.sp_SetSalesBom_Detail";

                    _cmd += "  @UpdUser  ='" + salebomD[i].UpdUser + "'";
                    _cmd += ",@BomNo  ='" + salebomD[i].BomNo + "'";
                    _cmd += ",@RevNo =" + salebomD[i].RevNo;
                    _cmd += ",@Seq =" + salebomD[i].Seq;
                    _cmd += ",@ProdCode  ='" + salebomD[i].ProdCode + "'";
                    _cmd += ",@ProdDescription  ='" + salebomD[i].ProdDescription + "'";
                    _cmd += ",@Qty =" + salebomD[i].Qty;
                    _cmd += ",@UnitCode  ='" + salebomD[i].UnitCode + "'";
                    _cmd += ",@UnitPrice =" + salebomD[i].UnitPrice;
                    _cmd += ",@Amt =" + salebomD[i].Amt;
                    _cmd += ",@ReplaceStatus =" + salebomD[i].ReplaceStatus;
                    _cmd += ",@Remark  ='" + salebomD[i].Remark + "'";
                    _cmd += ",@CmpId  ='" + salebomD[i].CmpId + "'";


                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return BadRequest();
                    };

                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                return BadRequest(ex.Message);
            }

        }



        [HttpPost("[action]")]

        public IActionResult updateBomPrice([FromBody] SalesBom_Price_Version item)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;



                _cmd = "exec  dbo.sp_SetSalesBom_Detail_UpdatePrice";

                _cmd += "  @UpdUser  ='" + item.UpdUser + "'";
                _cmd += ",@BomNo  ='" + item.BomNo + "'";
                _cmd += ",@RevNo =" + item.RevNo;
                _cmd += ",@Seq =" + item.Seq;
                _cmd += ",@ProdCode  ='" + item.ProdCode + "'";
                _cmd += ",@SupplierCode  ='" + item.SupplierCode + "'";
                _cmd += ",@Qty =" + item.Qty;
                _cmd += ",@UnitCode  ='" + item.UnitCode + "'";
                _cmd += ",@UnitPrice =" + item.UnitPrice;
                _cmd += ",@Amt =" + item.Amt;
                _cmd += ",@Remark  ='" + item.Remark + "'";
                _cmd += ",@CmpId  ='" + item.CmpId + "'";
                _cmd += ",@DeliveryDate  ='" + item.DeliveryDate.ToString("yyyy-MM-dd HH:mm", thaiCulture)  + "'";


                if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                {
                    DB.DBConn.Tran.Rollback();
                    DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                    DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return NotFound(msgretrun);
                };



                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return NotFound(msgretrun);

            }




        }






        [HttpPost]
        [Route("salesbomapprove")]
        public IActionResult BomApp([FromQuery] string cmpid, [FromQuery] string DocNo, [FromQuery] int RevNo, [FromQuery] string user)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.sp_SetSalesBomApp @CmpId='" + cmpid + "' , @DocNo='" + DocNo + "' , @RevNo =" + RevNo + ",@User='" + user + "'";

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
        [Route("salesbomaction")]
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
        [Route("salesbomfile")]
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








    }

}