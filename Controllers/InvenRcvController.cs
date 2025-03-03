using goalongapi.Models;
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

namespace goalongapi.Controllers
{

    [ApiController]
    [Authorize]


    public class InvenRcvController : ControllerBase
    {

        [HttpGet("[action]")]
        public IActionResult getInvenRcv([FromQuery] string CmpId, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.Inven_getReceiveAll @CmpId='" + (CmpId) + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            List<ReceiveModel> receiveModels = new List<ReceiveModel>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new ReceiveModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    ReceiveNo = r["ReceiveNo"].ToString(),
                    ReceiveDate = r["ReceiveDate"].ToString(),
                    ReceiveBy = r["ReceiveBy"].ToString(),
                    PurChaseNo = r["PurChaseNo"].ToString(),
                    InvoiceNo = r["InvoiceNo"].ToString(),
                    InvoiceDate = r["InvoiceDate"].ToString(),
                    ReceiveType = int.Parse(r["ReceiveType"].ToString()),
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    StateApp = int.Parse(r["StateApp"].ToString()),
                    AppBy = r["AppBy"].ToString(),
                    SupplierCode = r["SupplierCode"].ToString(),
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),
                    ImgPath = r["ImgPath"].ToString(),
                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    SupplierName = r["SupplierName"].ToString(),

                };



                receiveModels.Add(receive);
            }

            return Ok(receiveModels);
        }


        [HttpPost("[action]")]
        public IActionResult setInvenRcv(ReceiveModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReceiveTrans";
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@ReceiveNo  ='" + receive.ReceiveNo + "'";
                _cmd += ",@ReceiveDate  ='" + receive.ReceiveDate + "'";
                _cmd += ",@ReceiveBy  ='" + receive.ReceiveBy + "'";
                _cmd += ",@PurChaseNo  ='" + receive.PurChaseNo + "'";
                _cmd += ",@InvoiceNo  ='" + receive.InvoiceNo + "'";
                _cmd += ",@InvoiceDate  ='" + receive.InvoiceDate + "'";
                _cmd += ",@ReceiveType =" + receive.ReceiveType;
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@Remark  ='" + receive.Remark + "'";
                _cmd += ",@SupplierCode  ='" + receive.SupplierCode + "'";
                _cmd += ",@WHId =" + receive.SysWHId;
                _cmd += ",@WHLocId =" + receive.SysWHLocId;

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


        [HttpDelete("[action]")]
        public void DeleteRcv(string cmpid , string docno)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.Receive where ReceiveNo='" + docno + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
    }
}
