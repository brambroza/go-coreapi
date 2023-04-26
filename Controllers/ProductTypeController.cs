using Newtonsoft.Json;
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

    public class ProductTypeController : ApiController
    {
        // GET: api/ProductType
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/ProductType/5
        public IHttpActionResult Get(string id)
        {
            string _QuatationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getProdTypeMaster] @CmpId=" + _QuatationNo + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        // POST: api/ProductType
        public void Post(Prodtype prodtype)
        {

            string _cmd = "";
            _cmd = "exec  dbo.mProductCategory_Trans";
            _cmd +=  " @UpdUser  ='" + prodtype.UpdUser + "'";
            _cmd += ",@ProdCateCode  ='" + prodtype.ProdCateCode + "'"; 
            _cmd += ",@ProdCateDescripton  ='" + Tool.Tool.validateStr(prodtype.ProdCateDescripton) + "'";
            DB.DBConn.ExecuteOnly(_cmd);

        }

        // PUT: api/ProductType/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProductType/5
        public void Delete(string id)
        {
            string _cmd = "";
            _cmd = "delete from msb.mProductCategory where  ProdCateCode='" + id + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }
    }
}
