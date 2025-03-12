using Newtonsoft.Json;
using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



namespace goalongapi.Controllers
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
            _cmd += ",@CmpId  ='" + prodtype.CmpId + "'";
            _cmd += ",@ProdCateDescripton  ='" + Tool.Tool.validateStr(prodtype.ProdCateName) + "'";
            DB.DBConn.ExecuteOnly(_cmd);

        }


        [HttpPost("[action]")]
        public void setProductTypeSub(ProdTypeSub prodtype)
        {

            string _cmd = "";
            _cmd = "exec  dbo.mProductCategorySub_Trans";
            _cmd += " @UpdUser  ='" + prodtype.UpdUser + "'";
            _cmd += ",@ProdCateCode  ='" + prodtype.ProdCateCode + "'";
            _cmd += ",@ProdCateSubCode  ='" + prodtype.ProdCateSubCode + "'";
            _cmd += ",@CmpId  ='" + prodtype.CmpId + "'";
            _cmd += ",@ProdCateSUbDescripton  ='" + Tool.Tool.validateStr(prodtype.ProdCateSubName) + "'";
            DB.DBConn.ExecuteOnly(_cmd);

        }


        [HttpGet("[action]")]
        public IActionResult validateprodtype([FromQuery] string cmpid, [FromQuery] string prodtype)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[check_Use_ProdType] @CmpId='" + cmpid + "' , @ProdType='" + prodtype + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt.Rows.Count > 0);
        }

        [HttpGet("[action]")]
        public IActionResult validateprodsubtype([FromQuery] string cmpid, [FromQuery] string prodsubtype)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[check_Use_ProdTypeSub] @CmpId='" + cmpid + "' , @ProdTypeSub='" + prodsubtype + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            return Ok(dt.Rows.Count > 0);
        }







        // DELETE: api/ProductType/5
        [HttpDelete("[action]")]
        public void DeleteProdtype(string id, string cmpid)
        {
            string _cmd = "";
            _cmd = "delete from msb.mProductCategory where  ProdCateCode='" + id + "' and CmpId='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }


        [HttpDelete("[action]")]
        public void DeleteProdtypeSub(string id, string cmpid)
        {
            string _cmd = "";
            _cmd = "delete from msb.mProductCategorySub where  ProdCateSubCode='" + id + "' and CmpId='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }


    }
}
