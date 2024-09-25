using Newtonsoft.Json;
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



namespace coreapi.Controllers
{

    [ApiController]
    [Authorize]
    public class ProductTypeController : ControllerBase
    {


        // GET: api/ProductType/5
        [HttpGet("[action]")]
        public IActionResult getProductType([FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProdTypeMaster] @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }


        [HttpGet("[action]")]
        public IActionResult getProductTypeSub([FromQuery] string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProdTypeSubMaster] @CmpId='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }




        // POST: api/ProductType
        [HttpPost("[action]")]
        public void setProductType(Prodtype prodtype)
        {

            string _cmd = "";
            _cmd = "exec  dbo.mProductCategory_Trans";
            _cmd += " @UpdUser  ='" + prodtype.UpdUser + "'";
            _cmd += ",@ProdCateCode  ='" + prodtype.ProdCateCode + "'";
            _cmd += ",@ProdCateDescripton  ='" + Tool.Tool.validateStr(prodtype.ProdCateDescripton) + "'";
            DB.DBConn.ExecuteOnly(_cmd);

        }


        [HttpPost("[action]")]
        public void setProductTypeSub(ProdTypeSub prodtype)
        {

            string _cmd = "";
            _cmd = "exec  dbo.mProductCategorySub_Trans";
            _cmd += " @UpdUser  ='" + prodtype.UpdUser + "'";
            _cmd += ",@ProdCateCode  ='" + prodtype.ProdCateCode + "'";
            _cmd += ",@ProdCateSubCode  ='" + prodtype.ProdCateCode + "'";
            _cmd += ",@ProdCateSUbDescripton  ='" + Tool.Tool.validateStr(prodtype.ProdCateSubDescripton) + "'";
            DB.DBConn.ExecuteOnly(_cmd);

        }





        // DELETE: api/ProductType/5
        [HttpDelete("[action]")]
        public void DeleteProdtype(string id)
        {
            string _cmd = "";
            _cmd = "delete from msb.mProductCategory where  ProdCateCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
