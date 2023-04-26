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
    public class MasterController : ApiController
    {

        [Route("api/province")]
        [HttpGet]
        public IHttpActionResult getProvince()
        {
            string _cmd;
            _cmd = "exec dbo.getmProvince ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        [Route("api/districts")]
        [HttpGet]
        public IHttpActionResult getDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        [Route("api/subdistricts")]
        [HttpGet]
        public IHttpActionResult getSubDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmSubDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            return Ok(datatable);
        }


        [Route("api/setCustomerDBD")]
        [HttpPost]
        public IHttpActionResult Post(CustomerDBD cusdb)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCustomerFromDBD @juristicID='" + cusdb.juristicID + "'";
                _cmd += " ,@juristicNameTH='" + cusdb.juristicNameTH + "'";
                _cmd += " ,@juristicNameEN='" + cusdb.juristicNameEN + "'";
                _cmd += " ,@juristicType='" + cusdb.juristicType + "'";
                _cmd += " ,@registerDate='" + cusdb.registerDate + "'";
                _cmd += " ,@juristicStatus='" + cusdb.juristicStatus + "'";
                _cmd += " ,@registerCapital='" + cusdb.registerCapital + "'";
                _cmd += " ,@standardObjective='" + cusdb.standardObjective + "'";
                _cmd += " ,@objectiveDescription='" + cusdb.standardObjectiveDetail.objectiveDescription + "'";
                _cmd += " ,@addressName='" + cusdb.addressDetail.addressName + "'";
                _cmd += " ,@buildingName='" + cusdb.addressDetail.buildingName + "'";
                _cmd += " ,@roomNo='" + cusdb.addressDetail.roomNo + "'";
                _cmd += " ,@floor='" + cusdb.addressDetail.floor + "'";
                _cmd += " ,@moo='" + cusdb.addressDetail.moo + "'";
                _cmd += " ,@soi='" + cusdb.addressDetail.soi + "'";
                _cmd += " ,@street='" + cusdb.addressDetail.street + "'";
                _cmd += " ,@subDistrict='" + cusdb.addressDetail.subDistrict + "'";
                _cmd += " ,@district='" + cusdb.addressDetail.district + "'";
                _cmd += " ,@province='" + cusdb.addressDetail.province + "'"; 

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
