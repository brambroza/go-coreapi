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
   
    public class ProductListController : ApiController
    {
        // GET: api/ProductList
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ProductList/5
        public IHttpActionResult Get(int id)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterAll @CmpId='" + id + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        // POST: api/ProductList
        public IHttpActionResult Post(ProductList productList  )
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.ProductListTrans";
                _cmd += " @UpdUser  ='" + productList.UpdUser + "'";
                _cmd += ",@ProdductCode  ='" + Tool.Tool.validateStr(productList.ProdductCode) + "'";
                _cmd += ",@ProdductName  ='" + Tool.Tool.validateStr(productList.ProdductName) + "'";
                _cmd += ",@ProdductDescripton  ='"  + Tool.Tool.validateStr( productList.ProdductDescripton ) + "'";
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
                _cmd += ",@CmpId =" + productList.CmpId;
                _cmd += ",@ShowReport='" + productList.ShowReport + "'";

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

        // PUT: api/ProductList/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProductList/5
        public void Delete(string id)
        {
            try
            {
                string cmd = "";
                cmd = "delete  from msb.mProductList where ProdductCode='" + id + "'";
                DB.DBConn.GetDataTable(cmd);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
