
using coreapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace coreapi.Controllers.Master
{
    
    public class WarehouseController : ApiController
    {
        // GET: api/Warehouse



        // GET: api/Warehouse/5
        [Route("api/Warehouse")]
        [HttpGet]
        public IHttpActionResult Get(int CmpId)
        {
             
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getWareHouseAll @CmpId='" + CmpId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        // POST: api/Warehouse
        [Route("api/Warehouse")]
        [HttpPost]
        public IHttpActionResult App(WareHouse  wh)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmWareHouse_Trans"; 
                _cmd += "  @UpdUser  ='" + wh.UpdUser + "'";
                _cmd += ",@WareHouseId =" + wh.WareHouseId; 
                _cmd += ",@WareHouseName ='" + wh.WareHouseName + "'";
                _cmd += ",@WareHouseDescription  ='" + wh.WareHouseDescription + "'"; 
                _cmd += ",@StateActive =" + wh.StateActive;
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
        // PUT: api/Warehouse/5
        [Route("api/Warehouse")]
        [HttpPut]

        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Warehouse/5
        [Route("api/Warehouse")]
        [HttpDelete]
        public void Delete(int id)
        {
        }




        /// api location
        /// 

        [Route("api/location")]
        [HttpGet]
        public IHttpActionResult getLocation(int CmpId)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getWareHouseLocationAll @CmpId='" + CmpId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            return Ok(dt);
        }

        // POST: api/Warehouse
        [Route("api/location")]
        [HttpPost]
        public IHttpActionResult setLocation(WareHouseLocation loc)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmWareHouseLocation_Trans";
                _cmd += " @UpdUser  ='" + loc.UpdUser + "'";
                _cmd += ",@WareHouseLocId =" + loc.WareHouseLocId;
                _cmd += ",@WareHouseId =" + loc.WareHouseId; 
                _cmd += ",@WareHouseLocName ='" + loc.WareHouseLocName + "'";
                _cmd += ",@WareHouseLocDescription  ='" + loc.WareHouseLocDescription + "'"; 
                _cmd += ",@StateActive =" + loc.StateActive;
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


    }
}
