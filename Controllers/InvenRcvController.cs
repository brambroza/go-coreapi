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
            _cmd = "exec dbo.Inven_getReceiveAll @CmpId='" +  CmpId  + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getTransAll] @CmpId='" +  CmpId  + "' , @User='" + user + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

             _cmd = "exec dbo.[Inven_getTransSerialAll] @CmpId='" +  CmpId  + "' , @User='" + user + "' ";
            DataTable dtItemSerial = DB.DBConn.GetDataTable(_cmd);

            List<ReceiveModel> receives = new List<ReceiveModel>();

            foreach (DataRow r in dt.Rows)
            {
                var receive = new ReceiveModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    ReceiveNo = r["ReceiveNo"].ToString(),
                    ReceiveDate = r["ReceiveDate"].ToString(),
                    ReceiveBy = r["ReceiveBy"].ToString(),
                    PurchaseNo = r["PurChaseNo"].ToString(),
                    InvoiceNo = r["InvoiceNo"].ToString(),
                    InvoiceDate = r["InvoiceDate"].ToString(),
                    ReceiveType = r["ReceiveType"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    StateApp = r["StateApp"].ToString(),
                    AppBy = r["AppBy"].ToString(),
                    SupplierCode = r["SupplierCode"].ToString(),
                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHLocId = int.Parse(r["SysWHLocId"].ToString()),
                    ImgPath = r["ImgPath"].ToString(),
                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    SupplierName = r["SupplierName"].ToString(),
                    PurchaseDate = r["PurchaseDate"].ToString(),
                    VatType = decimal.Parse(r["VatType"].ToString()) ,
                    ReceiveAmt = decimal.Parse(r["ReceiveAmt"].ToString()) ,
                    ReceiveDisPer = decimal.Parse(r["ReceiveDisPer"].ToString()) ,
                    ReceiveDisAmt = decimal.Parse(r["ReceiveDisAmt"].ToString()) ,
                    ReceiveNetAmt = decimal.Parse(r["ReceiveNetAmt"].ToString()) ,
                    ReceiveVatAmt = decimal.Parse(r["ReceiveVatAmt"].ToString()) ,
                    ReceiveGrandAmt = decimal.Parse(r["ReceiveGrandAmt"].ToString()) ,
                    ReceiveGrandAmtTHB = r["ReceiveGrandAmtTHB"].ToString(),
                    ReceiveGrandAmtENB = r["ReceiveGrandAmtENB"].ToString(),

                };

                receive.items = new List<InvenTransModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["ReceiveNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new InvenTransModel();
                    item.DocNo = d["DocNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.TransDate = d["TransDate"].ToString();
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);
                    item.BarcodeNo = d["BarcodeNo"].ToString();

                    item.ProductCode = d["ProductCode"].ToString();
                    item.UnitPrice = Convert.ToDecimal(d["UnitPrice"]);
                    item.UnitCode = d["UnitCode"].ToString();
                    item.Qty = Convert.ToDecimal(d["Qty"]);
                    item.PurchaseNo = d["PurchaseNo"].ToString();

                    item.StateReserve = Convert.ToInt32(d["StateReserve"]);

                    item.ProdDescription = d["ProdDescription"].ToString();
                    item.BatchNo = d["BatchNo"].ToString();
                    item.Grade = d["Grade"].ToString();
                    item.DateExpire = d["DateExpire"].ToString();

                    item.StateQC = Convert.ToInt32(d["StateQC"]);

                    item.QCBy = d["QCBy"].ToString();
                    item.TransType = d["TransType"].ToString();
                    item.CmpId = d["CmpId"].ToString();
                    item.serials = new List<InvenTransModelSerial>();

                    if (dtItemSerial.Rows.Count > 0)
                    {
                        foreach (
                              DataRow rd in dtItemSerial.Select(
                                  "DocNo ='"
                                       + d["DocNo"].ToString()
                                      + "'  and CmpId='"
                                      + d["CmpId"] + "'"
                                       + "  and ProductCode='"
                                      + d["ProductCode"] + "'"
                                       + "  and MainSeq="
                                      +  item.Seq + ""
                              )
                          )
                        {
                            var serial = new InvenTransModelSerial();

                            serial.DocNo = rd["DocNo"].ToString();
                            serial.UpdUser = rd["UpdUser"].ToString();
                            serial.Seq = Convert.ToInt32(rd["Seq"]);
                            serial.TransDate = rd["TransDate"].ToString();
                            serial.SysWHId = Convert.ToInt32(rd["SysWHId"]);
                            serial.SysWHLocId = Convert.ToInt32(rd["SysWHLocId"]);
                            serial.BarcodeNo = rd["BarcodeNo"].ToString();

                            serial.ProductCode = rd["ProductCode"].ToString();
                            serial.Qty = Convert.ToDecimal(rd["Qty"]);
                            serial.UnitCode = rd["UnitCode"].ToString();
                            serial.SerialNumber = rd["SerialNumber"].ToString();
                            serial.MACAddress = rd["MACAddress"].ToString();
                            serial.WarrantyStartDate = rd["WarrantyStartDate"].ToString();
                            serial.WarrantyEndDate = rd["WarrantyEndDate"].ToString();
                            serial.WarrantyPeriod = rd["WarrantyPeriod"].ToString();
                            serial.TransType = rd["TransType"].ToString();
                            serial.StatusInStock =  rd["StatusInStock"].ToString();
                            serial.MainSeq = Convert.ToInt32(rd["MainSeq"]);
                            serial.CmpId = rd["CmpId"].ToString();


                            item.serials.Add(serial);


                        }

                    }
 
                    receive.items.Add(item);
                }

                receives.Add(receive);
            }
            var response = new {   receives   };
            return Ok(response);
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
                _cmd += ",@PurChaseNo  ='" + receive.PurchaseNo + "'";
                _cmd += ",@InvoiceNo  ='" + receive.InvoiceNo + "'";
                _cmd += ",@InvoiceDate  ='" + receive.InvoiceDate + "'";
                _cmd += ",@ReceiveType ='" + receive.ReceiveType + "'";
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@Remark  ='" + receive.Remark + "'";
                _cmd += ",@SupplierCode  ='" + receive.SupplierCode + "'";
                _cmd += ",@WHId =" + receive.SysWHId;
                _cmd += ",@WHLocId =" + receive.SysWHLocId;
                _cmd += ",@VatType =" + receive.VatType;
                _cmd += ",@ReceiveAmt =" + receive.ReceiveAmt;
                _cmd += ",@ReceiveDisPer =" + receive.ReceiveDisPer;
                _cmd += ",@ReceiveDisAmt =" + receive.ReceiveDisAmt;
                _cmd += ",@ReceiveNetAmt =" + receive.ReceiveNetAmt;
                _cmd += ",@ReceiveVatAmt =" + receive.ReceiveVatAmt;
                _cmd += ",@ReceiveGrandAmt =" + receive.ReceiveGrandAmt;
                _cmd += ",@ReceiveGrandAmtTHB ='" + receive.ReceiveGrandAmtTHB + "'";
                _cmd += ",@ReceiveGrandAmtENB ='" + receive.ReceiveGrandAmtENB + "'";

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


        [HttpPost("[action]")]
        public IActionResult setInvenRcvApprove(ReceiveModel receive)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setReceiveTrans_Approve";
                _cmd += " @UpdUser  ='" + receive.UpdUser + "'";
                _cmd += ",@ReceiveNo  ='" + receive.ReceiveNo + "'"; 
                _cmd += ",@CmpId ='" + receive.CmpId + "'";
                _cmd += ",@StateApp  ='" + receive.StateApp + "'"; 

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
