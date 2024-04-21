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


    public class ProductListController : ControllerBase
    {
        // GET: api/ProductList


        // GET: api/ProductList/5
        [HttpGet("[action]")]
        public IActionResult getProductlist([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterAll @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        // POST: api/ProductList
        [HttpPost("[action]")]
        public IActionResult UpdateProductList([FromBody] ProductList productList)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.ProductListTrans";
                _cmd += " @UpdUser  ='" + productList.UpdUser + "'";
                _cmd += ",@ProdductCode  ='" + Tool.Tool.validateStr(productList.ProdductCode) + "'";
                _cmd += ",@ProdductName  ='" + Tool.Tool.validateStr(productList.ProdductName) + "'";
                _cmd += ",@ProdductDescripton  ='" + Tool.Tool.validateStr(productList.ProdductDescripton) + "'";
                _cmd += ",@UnitCode  ='" + productList.UnitCode + "'";
                _cmd += ",@ProductType =" + productList.ProductType;
                _cmd += ",@BarcodeNo  ='" + Tool.Tool.validateStr(productList.BarcodeNo) + "'";
                _cmd += ",@PriceSale =" + productList.PriceSale;
                _cmd += ",@PricePur =" + productList.PricePur;
                _cmd += ",@VatType =" + productList.VatType;
                _cmd += ",@AccountCodeAR  ='" + productList.AccountCodeAR + "'";
                _cmd += ",@AccountCodeAP  ='" + productList.AccountCodeAP + "'";
                _cmd += ",@ProdCateCode  ='" + productList.ProdCateCode + "'";
                _cmd += ",@Warrantry  ='" + productList.Warranty + "'";
                _cmd += ",@BrandName  ='" + productList.BrandName + "'";
                _cmd += ",@ProdductStateActive =" + productList.ProdductStateActive;
                _cmd += ",@CmpId = '" + productList.CmpId+"'";
                _cmd += ",@ShowReport='" + productList.ShowReport + "'";
                _cmd += ",@imgpath='" + productList.imgpath + "'";

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


        // DELETE: api/ProductList/5
        [HttpDelete("[action]")]
        public void DeleteProduct([FromQuery] string cmpid , [FromQuery] string prodcode )
        {
            try
            {
                string cmd = "";
                cmd = "delete  from msb.mProductList where ProdductCode='" + prodcode + "' and cmpid='" + cmpid + "'";
                DB.DBConn.GetDataTable(cmd);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
