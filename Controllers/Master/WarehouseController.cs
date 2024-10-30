
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

namespace coreapi.Controllers.Master
{

    [ApiController]
    [Authorize]

    public class WarehouseController : ControllerBase
    {
        // GET: api/Warehouse


        [HttpGet("[action]")]
        public IActionResult getWarehouse([FromQuery] string CmpId)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getWareHouseAll @CmpId='" + CmpId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<WareHouse> whs = new List<WareHouse>();
            foreach (DataRow r in dt.Rows)
            {

                var wh = new WareHouse();
                wh.CmpId = r["CmpId"].ToString();
                wh.UpdUser = r["UpdUser"].ToString();
                wh.WareHouseId = int.Parse(r["WareHouseId"].ToString());
                wh.StateActive = int.Parse(r["StateActive"].ToString());
                wh.WareHouseDescription = r["WareHouseDescription"].ToString();
                wh.WareHouseName = r["WareHouseName"].ToString();

                whs.Add(wh);

            }



            return Ok(whs);
        }


        [HttpPost("[action]")]
        public IActionResult setWarehouse(WareHouse wh)
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
                _cmd += ",@CmpId ='" + wh.CmpId + "'";
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



        [HttpGet("[action]")]
        public IActionResult getLocationByWH([FromQuery] string CmpId, [FromQuery] string WareHouseId)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getWareHouseLocationByWH @CmpId='" + CmpId + "' , @WH='" + WareHouseId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);

            List<WareHouseLocation> whs = new List<WareHouseLocation>();
            foreach (DataRow r in dt.Rows)
            {

                var wh = new WareHouseLocation();
                wh.CmpId = r["CmpId"].ToString();
                wh.UpdUser = r["UpdUser"].ToString();
                wh.WareHouseId = int.Parse(r["WareHouseId"].ToString());
                wh.StateActive = int.Parse(r["StateActive"].ToString());
                wh.WareHouseLocId = int.Parse(r["WareHouseLocId"].ToString());
                wh.WareHouseLocDescription = r["WareHouseLocDescription"].ToString();
                wh.WareHouseLocName = r["WareHouseLocName"].ToString();

                whs.Add(wh);

            }



            return Ok(whs);
        }





        /// api location
        /// 


        [HttpGet("[action]")]
        public IActionResult getLocation([FromQuery] string CmpId)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getWareHouseLocationAll @CmpId='" + CmpId + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
           List<WareHouseLocation> whs = new List<WareHouseLocation>();
            foreach (DataRow r in dt.Rows)
            {

                var wh = new WareHouseLocation();
                wh.CmpId = r["CmpId"].ToString();
                wh.UpdUser = r["UpdUser"].ToString();
                wh.WareHouseId = int.Parse(r["WareHouseId"].ToString());
                wh.StateActive = int.Parse(r["StateActive"].ToString());
                wh.WareHouseLocId = int.Parse(r["WareHouseLocId"].ToString());
                wh.WareHouseLocDescription = r["WareHouseLocDescription"].ToString();
                wh.WareHouseLocName = r["WareHouseLocName"].ToString();

                whs.Add(wh);

            }



            return Ok(whs);
        }


        [HttpPost("[action]")]
        public IActionResult setLocation(WareHouseLocation loc)
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
                _cmd += ",@CmpId ='" + loc.CmpId + "'";
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
