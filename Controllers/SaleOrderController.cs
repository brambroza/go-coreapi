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
    [Route("[controller]")]
     


    public class SaleOrderController : ControllerBase
    {


        // GET: api/QuaH/5 
        [HttpGet("[action]")] 

        public IActionResult getSaleOrder([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.getSaleOrderAll @CmpId='" + cmpid + "', @User='" + user + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        
 
        // POST: api/QuaH
        
        [HttpPost("[action]")]
        public IActionResult setSaleOrder([FromBody]  saleorder quatation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setSaleOrder @SaleOrderNo='" + quatation.SaleOrderNo + "' ,@SaleOrderDate='" + quatation.SaleOrderDate + "' ,@SaleOrderBy='" + quatation.SaleOrderBy + "'";
                _cmd += " ,@SaleOrderState=" + quatation.SaleOrderState;
                _cmd += " ,@CustomerCode='" + quatation.CustomerCode + "'";
                _cmd += " ,@CreditType=" + quatation.CreditType;
                _cmd += " ,@CreditDate=" + quatation.CreditDate;
                _cmd += " ,@ProjectName='" + Tool.Tool.validateStr(quatation.ProjectName) + "'";
                _cmd += " ,@ReferCode='" + Tool.Tool.validateStr(quatation.ReferCode) + "'";
                _cmd += " ,@VatType=" + quatation.VatType;
                _cmd += " ,@Remark='" + quatation.Remark + "'";
                _cmd += " ,@Note='" + quatation.Note + "'";
                _cmd += " ,@SaleOrderAmt=" + quatation.SaleOrderAmt;
                _cmd += " ,@SaleOrderDisPer=" + quatation.SaleOrderDisPer;
                _cmd += " ,@SaleOrderDisAmt=" + quatation.SaleOrderDisAmt;
                _cmd += " ,@SaleOrderNetAmt=" + quatation.SaleOrderNetAmt;
                _cmd += " ,@SaleOrderVatAmt=" + quatation.SaleOrderVatAmt;
                _cmd += " ,@SaleOrderGrandAmt=" + quatation.SaleOrderGrandAmt;
                _cmd += " ,@WithholdingTaxState=" + quatation.WithholdingTaxState;
                _cmd += " ,@ShowSignatureState=" + quatation.ShowSignatureState;
                _cmd += " ,@CmpId=" + quatation.CmpId;
                _cmd += " ,@PriceStand='" + quatation.PriceStand + "'";
                _cmd += " ,@PaymentDue='" + quatation.PaymentDue + "'";
                _cmd += " ,@Shipping='" + quatation.Shipping + "'";
                _cmd += " ,@RevNo=" + quatation.RevNo;
                _cmd += " ,@CustContact='" + Tool.Tool.validateStr(quatation.CustomerContactName) + "'";
                _cmd += ", @Jobtype=" + quatation.Jobtype;
                _cmd += " ,@QuatationNo='" + quatation.QuatationNo + "'";
                _cmd += " ,@CustomerPONo='" + quatation.CustomerPONo + "'";
                 

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


        
        [HttpPost("[action]")]
        public IActionResult setsaleordercopy(SaleOrderCopy quatation)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setSaleOrderCopy @SaleOrderNo='" + quatation.SaleOrderNo + "'";
                _cmd += ", @SaleOrderNoNew ='" + quatation.SaleOrderNoNew + "'";
                _cmd += " ,@RevNo=" + quatation.RevNo;
                _cmd += ", @CmpId ='" + quatation.CmpId + "'";
                 _cmd += ", @userlogin ='" + quatation.userlogin + "'";



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



       
        [HttpPost("[action]")]
        public IActionResult setSaleOrderApp(  QuoHApprove quoHApprove)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setSaleOrderApp @CmpId=" + quoHApprove.cmpid + " , @DocNo='" + quoHApprove.docno + "' , @RevNo =" + quoHApprove.revno + ",@User='" + quoHApprove.user + "'";

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                   // linenotiapp(quoHApprove.docno);
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
         
        [HttpDelete("[action]")]
        public IActionResult deletesaleorder([FromQuery] string id, [FromQuery] int RevNo ,[FromQuery] string cmpid)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {

                string _cmd = "";
                _cmd = "delete from mdb.SaleOrder where  SaleOrderNo='" + id + "' and RevNo=" + RevNo+ " and Cmpid='" + cmpid +"'";

                DB.DBConn.ExecuteOnly(_cmd);
                _cmd = "delete from mdb.SaleOrder_Detail where  SaleOrderNo='" + id + "'  and RevNo=" + RevNo + " and Cmpid='" + cmpid +"'";

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


    
          [HttpGet("[action]")]
        
        public IActionResult getsaleorderdetail([FromQuery] string sono, [FromQuery] int RevNo , [FromQuery] string cmpid  , [FromQuery] string username)
        {
          
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getSaleOrderDetail @SaleOrderNo='" + sono + "' , @RevNo=" + RevNo + ", @CmpId='" + cmpid + "', @userlogin='" + username + "'" ;
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        // POST: api/Qua
        [HttpPost("[action]")]
         
        public void setsaleorderdetail( [FromBody] List<saleorderDetail> saleorderD)
        {


            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (saleorderD.Count > 0)
                {
                    _cmd = "Delete From mdb.SaleOrder_Detail where SaleOrderNo='" + saleorderD[0].SaleOrderNo + "'";
                    _cmd += " and  RevNo=" + saleorderD[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < saleorderD.Count; i++)
                {
                    il++;
                    _cmd = "Exec setSaleOrderDetail @SaleOrderNo='" + saleorderD[i].SaleOrderNo + "'";
                    _cmd += ",@Seq=" + il;
                    _cmd += ",@ProdCode='" + saleorderD[i].ProdCode + "'";
                    _cmd += ",@ProdDesc='" + Tool.Tool.validateStr(saleorderD[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + saleorderD[i].UnitPrice;
                    _cmd += ",@UnitCode='" + saleorderD[i].UnitCode + "'";
                    _cmd += ",@Qty=" + saleorderD[i].Qty;
                    _cmd += ",@Amt=" + saleorderD[i].Amt;
                    _cmd += ",@PricePur=" + saleorderD[i].PricePur;
                    _cmd += ",@CostAmt=" + saleorderD[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + saleorderD[i].ProfitAmt;
                    _cmd += ",@RevNo=" + saleorderD[i].RevNo;
                    _cmd += " ,@GroupCaption1='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption1) + "'";
                    _cmd += " ,@GroupCaption2='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption2) + "'";
                    _cmd += " ,@GroupCaption3='" + Tool.Tool.validateStr(saleorderD[i].GroupCaption3) + "'";
                     _cmd += ",@CmpId='" + saleorderD[i].CmpId + "'";

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
