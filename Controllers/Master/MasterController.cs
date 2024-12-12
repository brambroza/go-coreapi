using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers.Master
{
    [ApiController]
    [Authorize]
    public class MasterController : ControllerBase
    {
        [Route("province")]
        [HttpGet]
        public IActionResult getProvince()
        {
            string _cmd;
            _cmd = "exec dbo.getmProvince ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("districts")]
        [HttpGet]
        public IActionResult getDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("subdistricts")]
        [HttpGet]
        public IActionResult getSubDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmSubDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("setCustomerDBD")]
        [HttpPost]
        public IActionResult Post(CustomerDBD cusdb)
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
                _cmd +=
                    " ,@objectiveDescription='"
                    + cusdb.standardObjectiveDetail.objectiveDescription
                    + "'";
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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpGet("[action]")]
        public IActionResult getBank([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getBank @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getBankType([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getBankType @CmpId='" + cmpid + "' , @user='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getBankBranch([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getBankBranch @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public IActionResult setBank(Bank bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmBank";
                _cmd += "  @UserName  ='" + bk.UserName + "'";
                _cmd += " ,@BankCode ='" + bk.BankCode + "'";
                _cmd += " ,@BankName ='" + bk.BankName + "'";
                _cmd += " ,@Remark  ='" + bk.Remark + "'";
                _cmd += " ,@StateActive =" + bk.StateActive;
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpPost("[action]")]
        public IActionResult setBankBranch(BankBranch bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmBankBranch";
                _cmd += "  @UserName  ='" + bk.UserName + "'";
                _cmd += " ,@BankCode ='" + bk.BankCode + "'";
                _cmd += " ,@BankBranchCode ='" + bk.BankBranchCode + "'";
                _cmd += " ,@BankBranchName ='" + bk.BankBranchName + "'";
                _cmd += " ,@Address ='" + bk.Address + "'";
                _cmd += " ,@AddrProvince ='" + bk.AddrProvince + "'";
                _cmd += " ,@AddrDistrict ='" + bk.AddrDistrict + "'";
                _cmd += " ,@AddrSubDistrict ='" + bk.AddrSubDistrict + "'";
                _cmd += " ,@AddrPostCode ='" + bk.AddrPostCode + "'";
                _cmd += " ,@Fax ='" + bk.Fax + "'";
                _cmd += " ,@Phone ='" + bk.Phone + "'";
                _cmd += " ,@Remark  ='" + bk.Remark + "'";
                _cmd += " ,@StateActive =" + bk.StateActive;
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpDelete("[action]")]
        public void DeleteBank([FromQuery] string key, [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd = "delete from dbo.mBank where  BankCode='" + key + "' and CmpId ='" + cmpid + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        [HttpDelete("[action]")]
        public void DeleteBankBranch([FromQuery] string key, [FromQuery] string cmpid)
        {
            string _cmd = "";
            _cmd =
                "delete from dbo.mBankBranch where  BankBranchCode='"
                + key
                + "' and CmpId ='"
                + cmpid
                + "'";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        [Route("bussinetGrp")]
        [HttpGet]
        public IActionResult getBussinetGrp([FromQuery] string CmpId)
        {
            string _cmd;
            _cmd = "exec dbo.sp_getmBusinessGrp @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("delbusinessGrp")]
        [HttpDelete]
        public IActionResult delbusinessGrp([FromQuery] string CmpId, [FromQuery] string id)
        {
            string _cmd;
            _cmd =
                " delete from   msb.mBusinessGrp  where CmpId='"
                + CmpId
                + "' and BusinessGrpCode='"
                + id
                + "'";

            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult setBussinetGrp(mBussinetGrp bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetBusinessGrp";
                _cmd += "  @UpdUser  ='" + bk.UpdUser + "'";
                _cmd += " ,@BusinessGrpCode ='" + bk.BusinessGrpCode + "'";
                _cmd += " ,@BusinessGrpName ='" + bk.BusinessGrpName + "'";
                _cmd += " ,@BusinessGrpDescription ='" + bk.BusinessGrpDescription + "'";
                _cmd += " ,@StateActive =" + bk.StateActive + "";
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }
    }
}
