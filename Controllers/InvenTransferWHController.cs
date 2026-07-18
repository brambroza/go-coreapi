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

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]


    public class InvenTransferWHController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getInvenTransferWH([FromQuery] int CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHAll] @CmpId='" + CmpId + "' , @User='" + userlogin + "' ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getTransferWHItemAll] @CmpId='" + CmpId + "' , @User='" + userlogin + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<TransferWHModel> issues = new List<TransferWHModel>();

            foreach (DataRow r in dt.Rows)
            {
                var issue = new TransferWHModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    TransferWHNo = r["TransferWHNo"].ToString(),
                    TransferWHDate = r["TransferWHDate"].ToString(),
                    TransferWHBy = r["TransferWHBy"].ToString(),

                    DocRef = r["DocRef"].ToString(),

                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    TransferWHApp = r["TransferWHApp"].ToString(),
                    TransferWHAppBy = r["TransferWHAppBy"].ToString(),

                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHToId = int.Parse(r["SysWHToId"].ToString()),


                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    ToWareHouseName = r["ToWareHouseName"].ToString(),
                    ToWareHouseLocName = r["ToWareHouseLocName"].ToString(),
                    Status = r["Status"].ToString(),

                };

                issue.items = new List<InvenTransItemModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["TransferWHNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new InvenTransItemModel();
                    item.DocNo = d["DocNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.TransDate = d["TransDate"].ToString();
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);

                    item.SysWHToId = Convert.ToInt32(d["SysWHToId"]);
                    item.SysWHLocToId = Convert.ToInt32(d["SysWHLocToId"]);


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

                    item.WareHouseName = d["WareHouseName"].ToString();
                    item.ToWareHouseName = d["ToWareHouseName"].ToString();
                    item.WareHouseLocName = d["WareHouseLocName"].ToString();
                    item.ToWareHouseLocName = d["ToWareHouseLocName"].ToString();



                    issue.items.Add(item);
                }

                issues.Add(issue);
            }
            var response = new { issues };
            return Ok(response);


        }


        [HttpGet("[action]")]
        public IActionResult getInvenTransferWHRcv([FromQuery] int CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHRcvAll] @CmpId='" + CmpId + "' , @User='" + userlogin + "' ";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            _cmd = "exec dbo.[Inven_getTransferWHItemAll] @CmpId='" + CmpId + "' , @User='" + userlogin + "' ";
            DataTable dtItem = DB.DBConn.GetDataTable(_cmd);

            List<TransferWHModel> issues = new List<TransferWHModel>();

            foreach (DataRow r in dt.Rows)
            {
                var issue = new TransferWHModel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    TransferWHNo = r["TransferWHNo"].ToString(),
                    TransferWHDate = r["TransferWHDate"].ToString(),
                    TransferWHBy = r["TransferWHBy"].ToString(),

                    DocRef = r["DocRef"].ToString(),

                    CmpId = r["CmpId"].ToString(),
                    Remark = r["Remark"].ToString(),
                    TransferWHApp = r["TransferWHApp"].ToString(),
                    TransferWHAppBy = r["TransferWHAppBy"].ToString(),

                    SysWHId = int.Parse(r["SysWHId"].ToString()),
                    SysWHToId = int.Parse(r["SysWHToId"].ToString()),


                    WareHouseName = r["WareHouseName"].ToString(),
                    WareHouseLocName = r["WareHouseLocName"].ToString(),
                    ToWareHouseName = r["ToWareHouseName"].ToString(),
                    ToWareHouseLocName = r["ToWareHouseLocName"].ToString(),
                    Status = r["Status"].ToString(),

                };

                issue.items = new List<InvenTransItemModel>();

                foreach (
                    DataRow d in dtItem.Select(
                        "DocNo ='"
                             + r["TransferWHNo"].ToString()
                            + "'  and CmpId='"
                            + r["CmpId"] + "'"
                    )
                )
                {
                    var item = new InvenTransItemModel();
                    item.DocNo = d["DocNo"].ToString();
                    item.UpdUser = d["UpdUser"].ToString();
                    item.Seq = Convert.ToInt32(d["Seq"]);
                    item.TransDate = d["TransDate"].ToString();
                    item.SysWHId = Convert.ToInt32(d["SysWHId"]);
                    item.SysWHLocId = Convert.ToInt32(d["SysWHLocId"]);

                    item.SysWHToId = Convert.ToInt32(d["SysWHToId"]);
                    item.SysWHLocToId = Convert.ToInt32(d["SysWHLocToId"]);


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

                    item.WareHouseName = d["WareHouseName"].ToString();
                    item.ToWareHouseName = d["ToWareHouseName"].ToString();
                    item.WareHouseLocName = d["WareHouseLocName"].ToString();
                    item.ToWareHouseLocName = d["ToWareHouseLocName"].ToString();



                    issue.items.Add(item);
                }

                issues.Add(issue);
            }
            var response = new { issues };
            return Ok(response);


        }





        [HttpGet("[action]")]
        public IActionResult getInvenTransferWHRcvlist([FromQuery] string CmpId, [FromQuery] string userlogin)
        {
            string _cmd;
            _cmd = "exec dbo.[Inven_getTransferWHRcvAll] @CmpId='" + (CmpId) + "' , @User='" + userlogin + "' ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string res = string.Empty;
            res = JsonConvert.SerializeObject(datatable);
            return Ok(res);
        }



        [HttpGet("[action]")]
        public IActionResult getInvenTransferWHProdWaidRcv([FromQuery] string CmpId)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getProdMasterforRcvTransferWH  @CmpId=" + Convert.ToInt16(CmpId) + " ";
            dt = DB.DBConn.GetDataTable(_cmd);
            string res = string.Empty;
            res = JsonConvert.SerializeObject(dt);
            return Ok(res);
        }



        [HttpPost("[action]")]
        public IActionResult setInvenTransferWH(TransferWHModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTrans";
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'";
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId;
                _cmd += ",@Remark  ='" + TransWH.Remark + "'";
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.SysWHId;

                _cmd += ",@WHToId =" + TransWH.SysWHToId;




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
                    return BadRequest(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }

        [HttpPost("[action]")]
        public IActionResult setInvenTransferWHItem(List<InvenTransItemModel> Inven)
        {

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                if (Inven.Count > 0)
                {
                    _cmd = "Delete From Inven.TransferWHItem   where DocNo='" + Inven[0].DocNo + "'";
                    _cmd += "   and CmpId='" + Inven[0].CmpId + "'";

                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }

                for (int i = 0; i < Inven.Count; i++)
                {

                    _cmd = "exec  dbo.Inven_setTransferWHItemTrans";
                    _cmd += " @UpdUser  ='" + Inven[i].UpdUser + "'";
                    _cmd += ",@Seq =" + Inven[i].Seq;
                    _cmd += ",@DocNo  ='" + Inven[i].DocNo + "'";
                    _cmd += ",@TransDate ='" + Inven[i].TransDate + "'"; ;
                    _cmd += ",@SysWHId =" + Inven[i].SysWHId;
                    _cmd += ",@SysWHLocId =" + Inven[i].SysWHLocId;
                    _cmd += ",@SysWHToId =" + Inven[i].SysWHToId;
                    _cmd += ",@SysWHLocToId =" + Inven[i].SysWHLocToId;
                    _cmd += ",@BarcodeNo  ='" + Inven[i].BarcodeNo + "'";
                    _cmd += ",@ProductCode  ='" + Inven[i].ProductCode + "'";
                    _cmd += ",@UnitPrice =" + Inven[i].UnitPrice;
                    _cmd += ",@Qty =" + Inven[i].Qty;
                    _cmd += ",@UnitCode ='" + Inven[i].UnitCode + "'"; ;
                    _cmd += ",@PurChaseNo  ='" + Inven[i].PurchaseNo + "'";
                    _cmd += ",@StateReserve ='" + Inven[i].StateReserve + "'"; ;
                    _cmd += ",@BatchNo ='" + Inven[i].BatchNo + "'"; ;
                    _cmd += ",@Grade ='" + Inven[i].Grade + "'"; ;
                    _cmd += ",@DateExpire ='" + Inven[i].DateExpire + "'"; ;
                    _cmd += ",@Type ='" + Inven[i].TransType + "'";
                    _cmd += ", @CmpId='" + Inven[i].CmpId + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return BadRequest(msgretrun);
                    }


                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);

            }




        }


        [HttpPost("[action]")]
        public IActionResult setInvenTransferWHRcvItem(List<InvenTransItemModel> Inven)
        {

            MsgReturn msgretrun = new MsgReturn();
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {

                string _cmd;
                
                for (int i = 0; i < Inven.Count; i++)
                {

                    _cmd = "exec  dbo.Inven_setTransferWHItemTrans";
                    _cmd += " @UpdUser  ='" + Inven[i].UpdUser + "'";
                    _cmd += ",@Seq =" + Inven[i].Seq;
                    _cmd += ",@DocNo  ='" + Inven[i].DocNo + "'";
                    _cmd += ",@TransDate ='" + Inven[i].TransDate + "'"; ;
                    _cmd += ",@SysWHId =" + Inven[i].SysWHId;
                    _cmd += ",@SysWHLocId =" + Inven[i].SysWHLocId;
                    _cmd += ",@SysWHToId =" + Inven[i].SysWHToId;
                    _cmd += ",@SysWHLocToId =" + Inven[i].SysWHLocToId;
                    _cmd += ",@BarcodeNo  ='" + Inven[i].BarcodeNo + "'";
                    _cmd += ",@ProductCode  ='" + Inven[i].ProductCode + "'";
                    _cmd += ",@UnitPrice =" + Inven[i].UnitPrice;
                    _cmd += ",@Qty =" + Inven[i].Qty;
                    _cmd += ",@UnitCode ='" + Inven[i].UnitCode + "'"; ;
                    _cmd += ",@PurChaseNo  ='" + Inven[i].PurchaseNo + "'";
                    _cmd += ",@StateReserve ='" + Inven[i].StateReserve + "'"; ;
                    _cmd += ",@BatchNo ='" + Inven[i].BatchNo + "'"; ;
                    _cmd += ",@Grade ='" + Inven[i].Grade + "'"; ;
                    _cmd += ",@DateExpire ='" + Inven[i].DateExpire + "'"; ;
                    _cmd += ",@Type ='" + Inven[i].TransType + "'";
                    _cmd += ", @CmpId='" + Inven[i].CmpId + "'";
                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        msgretrun.ReturnCode = "400";
                        msgretrun.Msg = "Error !!";
                        return BadRequest(msgretrun);
                    }


                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "200";
                msgretrun.Msg = "Save Success !!";
                return Ok(msgretrun);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);

            }




        }





        [Route("api/InvenTransferWHRcv")]
        [HttpPost]
        public IActionResult InvenTransferWHRcv(TransferWHRcvModel TransWH)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTransRcv";
                _cmd += " @UpdUser  ='" + TransWH.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + TransWH.TransferWHNo + "'";
                _cmd += ",@TransferWHDate ='" + TransWH.TransferWHDate + "'";
                _cmd += ",@TransferWHBy ='" + TransWH.TransferWHBy + "'";
                _cmd += ",@CmpId =" + TransWH.CmpId;
                _cmd += ",@Remark  ='" + TransWH.Remark + "'";
                _cmd += ",@DocRef ='" + TransWH.DocRef + "'";
                _cmd += ",@WHId =" + TransWH.WHId;
                _cmd += ",@WHLocId =" + TransWH.WHLocId;



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
        public IActionResult getCheckBarcodeNoForTransferWH([FromQuery] string CmpId)
        {
            try
            {
                DataTable dt = new System.Data.DataTable();
                string _cmd = "exec dbo.[getProdMaster_Onhand_ForTransferWH_All] @CmpId='" + CmpId + "' ";
                dt = DB.DBConn.GetDataTable(_cmd);



                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(dt);

                var prodlist = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var eventObj = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        string lowercaseColumnName =
                            char.ToLowerInvariant(column.ColumnName[0])
                            + column.ColumnName.Substring(1);

                        eventObj[lowercaseColumnName] = row[column];
                    }

                    prodlist.Add(eventObj);
                }


                return Ok(prodlist);



            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching products.", Details = ex.Message });
            }
        }


        [HttpPost("[action]")]
        public IActionResult setInvenTransferWHApprove(TransferWHModel trw)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHTrans_Approve";
                _cmd += " @UpdUser  ='" + trw.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + trw.TransferWHNo + "'"; 
                _cmd += ",@CmpId ='" + trw.CmpId + "'";
                _cmd += ",@TransferWHApp  ='" + trw.TransferWHApp + "'";
                

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
                    return BadRequest(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }


         [HttpPost("[action]")]
        public IActionResult setInvenTransferWHRcvApprove(TransferWHModel trw)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.Inven_setTransferWHRcvTrans_Approve";
                _cmd += " @UpdUser  ='" + trw.UpdUser + "'";
                _cmd += ",@TransferWHNo  ='" + trw.TransferWHNo + "'"; 
                _cmd += ",@CmpId ='" + trw.CmpId + "'";
                _cmd += ",@TransferWHApp  ='" + trw.TransferWHApp + "'";
                

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
                    return BadRequest(msgretrun);
                }

            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return BadRequest(msgretrun);
            }


        }






        [HttpDelete("[action]")]
        public void DeleteInvenTransferWH([FromQuery] string id, [FromQuery] string cmpid)
        {
            try
            {
                string _cmd = "";
                _cmd = "Delete from Inven.TrasferWH where TransferWHNo='" + id + "' and CmpId='" + cmpid + "'";
                DB.DBConn.ExecuteOnly(_cmd);
            }
            catch
            {

            }
        }
        





    }
}
