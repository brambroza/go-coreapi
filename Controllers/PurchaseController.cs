using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization; 
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace coreapi.Controllers
{
    [ApiController] 
    [Authorize]


    public class PurchaseController : ControllerBase
    {


        
        [HttpGet("[action]")]
        public IActionResult getPurchaselist([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseAll @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

           
        [HttpGet("[action]")]
        public IActionResult getPurchasercvlist([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchasercv @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


          
        [HttpGet("[action]")]
        public IActionResult getPurchaseRcvDetail([FromQuery] string id , [FromQuery] int RevNo  , [FromQuery] string cmpid )
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseRcvDetail @PurchaseNo='" +  (id) + "', @RevNo=" + RevNo +", @CmpId='" + cmpid + "'";
             DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }





       
        [HttpGet("[action]")]
        public IActionResult getPurchaseSelect([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseSelect  @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        
        [HttpGet("[action]")]
        public IActionResult getPurchaseDetail([FromQuery] string id , [FromQuery] int RevNo )
        {
            string _cmd;
            _cmd = "exec dbo.getPurchaseDetail @PurchaseNo='" +  (id) + "', @RevNo=" + RevNo;
             DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }


        
        [HttpGet("[action]")]
        public IActionResult getPurchaseTracking([FromQuery] string cmpid )
        {
            string _cmd;
           _cmd = "exec dbo.getPurchaseTracking @CmpId='" + cmpid + "'";
             DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }




        
        [HttpGet("[action]")]
        public IActionResult getPurchaseforRcv([FromQuery] string id, [FromQuery] int RevNo)
        {
            string _cmd;
            _cmd = "exec dbo.[getPurchaseDetailforRcv] @PurchaseNo='" + (id) + "', @RevNo=" + RevNo;
             DataTable dt = DB.DBConn.GetDataTable(_cmd);
              string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }



        
        [HttpPost("[action]")]
        public IActionResult setPurchaseApp(QuoHApprove purApp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec dbo.setPurchaseApp @CmpId='" +  purApp.cmpid + "' , @DocNo='" + purApp.docno + "' , @RevNo =" + purApp.revno + ",@User='" + purApp.user + "'";

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
        public IActionResult setPurchase(Purchase po)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setPurchase"; 
                _cmd += " @UpdUser  ='" + po.UpdUser + "'"; 
                _cmd += " ,@PurchaseNo  ='" + po.PurchaseNo + "'";
                _cmd += " ,@PurchaseDate  ='" + po.PurchaseDate + "'";
                _cmd += " ,@PurchaseBy  ='" + po.PurchaseBy + "'"; 
                _cmd += " ,@PurchaseState =" + po.PurchaseState; 
                _cmd += " ,@SupplierCode  ='" + po.SupplierCode + "'"; 
                _cmd += " ,@CreditType =" + po.CreditType; 
                _cmd += " ,@CreditDate =" + po.CreditDate; 
                _cmd += " ,@ProjectName  ='" + po.ProjectName + "'"; 
                _cmd += " ,@ReferCode  ='" + po.ReferCode + "'"; 
                _cmd += " ,@VatType =" + po.VatType; 
                _cmd += " ,@Remark  ='" + po.Remark + "'"; 
                _cmd += " ,@Note  ='" + po.Note + "'"; 
                _cmd += " ,@PurchaseAmt =" + po.PurchaseAmt; 
                _cmd += " ,@PurchaseDisPer =" + po.PurchaseDisPer; 
                _cmd += " ,@PurchaseDisAmt =" + po.PurchaseDisAmt; 
                _cmd += " ,@PurchaseNetAmt =" + po.PurchaseNetAmt; 
                _cmd += " ,@PurchaseVatAmt =" + po.PurchaseVatAmt; 
                _cmd += " ,@PurchaseGrandAmt =" + po.PurchaseGrandAmt; 
                _cmd += " ,@PurchaseGrandAmtTHB  ='" + po.PurchaseGrandAmtTHB + "'"; 
                _cmd += " ,@PurchaseGrandAmtENB  ='" + po.PurchaseGrandAmtENB + "'"; 
                _cmd += " ,@WithholdingTaxState =" + po.WithholdingTaxState; 
                _cmd += " ,@ShowSignatureState =" + po.ShowSignatureState; 
                _cmd += "  ,@CmpId ='" + po.CmpId + "'";
                _cmd += " ,@DocState =" + po.DocState; 
                _cmd += " ,@PriceStand  ='" + po.PriceStand + "'"; 
                _cmd += " ,@PaymentDue  ='" + po.PaymentDue + "'"; 
                _cmd += " ,@Shipping  ='" + po.Shipping + "'"; 
                _cmd += " ,@RevNo =" + po.RevNo;
                _cmd += " ,@ProjectNo  ='" + po.ProjectNo + "'";



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

      

       
        [HttpDelete("[action]")]
        public IActionResult DeletePurchase(int id , string DocNo , int RevNo)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd;
                _cmd = "exec dbo.removePurchase @CmpId=" + Convert.ToInt16(id) + " , @DocNo='" + DocNo + "' , @RevNo =" + RevNo;
                DataTable datatable = DB.DBConn.GetDataTable(_cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Delete Success !!";
                return Ok(msgretrun);
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }


         
        [HttpPost("[action]")]
        public IActionResult setPurchaseDetail(List<Purchase_Detail> po)
        {
            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (po.Count > 0)
                {
                    _cmd = "Delete From pur.Purchase_Detail where PurchaseNo='" + po[0].PurchaseNo + "'";
                    _cmd += " and  RevNo=" + po[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < po.Count; i++)
                {
                    
                    _cmd = "exec  dbo.setPurchaseDetail";
                    _cmd += " @PurchaseNo  ='" + po[i].PurchaseNo + "'";
                    _cmd += ",@Seq =" + po[i].Seq;
                    _cmd += ",@ProdCode  ='" + po[i].ProdCode + "'";
                    _cmd += ",@ProdDesc  ='" + po[i].ProdDescription + "'";
                    _cmd += ",@Qty =" + po[i].Qty;
                    _cmd += ",@UnitPrice =" + po[i].UnitPrice;
                    _cmd += ",@UnitCode  ='" + po[i].UnitCode + "'";
                    _cmd += ",@Amt =" + po[i].Amt;
                    _cmd += ",@PricePur =" + po[i].PricePur;
                    _cmd += ",@CostAmt =" + po[i].CostAmt;
                    _cmd += ",@ProfitAmt =" + po[i].ProfitAmt;
                    _cmd += ",@RevNo =" + po[i].RevNo;
                    _cmd += ",@GroupCaption1  ='" + po[i].GroupCaption1 + "'";
                    _cmd += ",@GroupCaption2  ='" + po[i].GroupCaption2 + "'";
                    _cmd += ",@GroupCaption3  ='" + po[i].GroupCaption3 + "'";
                     _cmd += ",@CmpId  ='" + po[i].CmpId + "'";



                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return Ok(msgretrun);
                    }
                }

             

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch  
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }

        }





    }
}
