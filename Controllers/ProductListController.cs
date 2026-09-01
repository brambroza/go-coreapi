using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using goalongapi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
 
namespace goalongapi.Controllers
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

            var prodlist = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var eventObj = new Dictionary<string, object>();
                foreach (DataColumn column in dt.Columns)
                {
                    string lowercaseColumnName =
                        char.ToLowerInvariant(column.ColumnName[0])
                        + column.ColumnName.Substring(1);

                    eventObj[lowercaseColumnName] = row[column];
                }

                prodlist.Add(eventObj);
            }

            return Ok(prodlist);
        }

        [HttpGet("[action]")]
        public IActionResult getActionProductlist([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterAll @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<ProductMasterList> productMasters = new List<ProductMasterList>();

            foreach (DataRow r in dt.Rows)
            {
                var product = new ProductMasterList()
                {
                    Id = int.Parse(r["Id"].ToString()),
                    ProductCode = r["ProductCode"]?.ToString(),
                    ProductName = r["ProductName"]?.ToString(),
                    ProductDescripton = r["ProductDescripton"]?.ToString(),
                    UnitCode = r["UnitCode"]?.ToString(),
                    ProductType = r["ProductType"]?.ToString(),
                    ProductTypeSub = r["ProductTypeSub"]?.ToString(),
                    BarcodeNo = r["BarcodeNo"]?.ToString(),
                    PriceSale =
                        r["PriceSale"] != DBNull.Value ? Convert.ToDecimal(r["PriceSale"]) : 0,
                    PricePur = r["PricePur"] != DBNull.Value ? Convert.ToDecimal(r["PricePur"]) : 0,
                    VatType = r["VatType"] != DBNull.Value ? Convert.ToInt32(r["VatType"]) : 0,
                    AccountCodeAR = r["AccountCodeAR"]?.ToString(),
                    AccountCodeAP = r["AccountCodeAP"]?.ToString(),
                    ProdCateCode = r["ProdCateCode"]?.ToString(),
                    ProductStateActive =
                        r["ProductStateActive"] != DBNull.Value
                            ? Convert.ToInt32(r["ProductStateActive"])
                            : 0,
                    Warranty = r["Warranty"]?.ToString(),
                    BrandName = r["BrandName"]?.ToString(),
                    ProductTypeName = r["ProductTypeName"]?.ToString(),
                    ProductTypeSubName = r["ProductTypeSubName"]?.ToString(),
                    ShowReport = r["ShowReport"]?.ToString(),
                    ImgPath = r["imgpath"]?.ToString(),
                    UpdDate = DateTime.Parse(r["UpdDate"]?.ToString()),
                    CmpId = r["CmpId"]?.ToString(),
                    StateActive =
                        r["ProductStateActive"] != DBNull.Value
                            ? Convert.ToBoolean(r["ProductStateActive"])
                            : false,
                    UpdUser = r["UpdUser"]?.ToString(),
                    ProductCodeRef = r["ProductCodeRef"]?.ToString(),
                    AccountCode = r["AccountCode"]?.ToString(),
                    Quantity = r["quantity"] != DBNull.Value ? Convert.ToInt32(r["quantity"]) : 0,
                    Available =
                        r["available"] != DBNull.Value ? Convert.ToInt32(r["available"]) : 0,
                    InventoryType = r["inventoryType"]?.ToString(),
                    ProductNameSearch = r["ProductNameSearch"]?.ToString(),
                };
                productMasters.Add(product);
            }

            return Ok(new { products = productMasters });
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
                _cmd += ",@ProductCode  ='" + Tool.Tool.validateStr(productList.ProductCode) + "'";
                _cmd += ",@ProductName  ='" + Tool.Tool.validateStr(productList.ProductName) + "'";
                _cmd +=
                    ",@ProductDescripton  ='"
                    + Tool.Tool.validateStr(productList.ProductDescripton)
                    + "'";
                _cmd += ",@UnitCode  ='" + productList.UnitCode + "'";
                _cmd += ",@ProductType ='" + productList.ProductType + "'";
                _cmd += ",@ProductTypeSub ='" + productList.ProductTypeSub + "'";
                _cmd += ",@BarcodeNo  ='" + Tool.Tool.validateStr(productList.BarcodeNo) + "'";
                _cmd += ",@PriceSale =" + productList.PriceSale;
                _cmd += ",@PricePur =" + productList.PricePur;
                _cmd += ",@VatType =" + productList.VatType;
                _cmd += ",@AccountCodeAR  ='" + productList.AccountCodeAR + "'";
                _cmd += ",@AccountCodeAP  ='" + productList.AccountCodeAP + "'";
                _cmd += ",@ProdCateCode  ='" + productList.ProdCateCode + "'";
                _cmd += ",@Warrantry  ='" + Tool.Tool.validateStr(productList.Warranty) + "'";
                _cmd += ",@BrandName  ='" + productList.BrandName + "'";
                _cmd += ",@ProductStateActive =" + productList.ProductStateActive;
                _cmd += ",@CmpId = '" + productList.CmpId + "'";
                _cmd += ",@ShowReport='" + productList.ShowReport + "'";
                _cmd += ",@imgpath='" + productList.imgpath + "'";
                _cmd += ",@ProductCodeRef='" + productList.ShowReport + "'";
                _cmd += ",@AccountCode='" + productList.AccountCode + "'";

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
        public void DeleteProduct([FromQuery] string cmpid, [FromQuery] string prodcode)
        {
            try
            {
                string cmd = "";
                cmd =
                    "delete  from msb.mProductList where ProductCode='"
                    + prodcode
                    + "' and cmpid='"
                    + cmpid
                    + "'";
                DB.DBConn.GetDataTable(cmd);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
