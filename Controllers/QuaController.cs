using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    public class QuaController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult GetQuoDetail(
            [FromQuery] string id,
            [FromQuery] int RevNo,
            [FromQuery] string CmpId
        )
        {
            string _QuotationNo = id;
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.getQuotationDetail @QuotationNo='"
                + _QuotationNo
                + "' , @RevNo="
                + RevNo
                + ", @CmpId='"
                + CmpId
                + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            //string qdetail = string.Empty;
            //qdetail = JsonConvert.SerializeObject(dt);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public void setQuoDetail([FromBody] List<QuotationDetail> Quotation)
        {
            DB.DBConn.SqlConnectionOpen();
            DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
            DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

            try
            {
                string _cmd;
                if (Quotation.Count > 0)
                {
                    _cmd =
                        "Delete From mdb.Quotation_Detail where QuotationNo='"
                        + Quotation[0].QuotationNo
                        + "'";
                    _cmd += " and  RevNo=" + Quotation[0].RevNo;
                    DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran);
                }
                int il = 0;
                for (int i = 0; i < Quotation.Count; i++)
                {
                    il++;
                    _cmd =
                        "Exec setQuotationDetail @QuotationNo='" + Quotation[i].QuotationNo + "'";
                    _cmd += ",@Seq=" + Quotation[i].Seq;
                    _cmd += ",@ProdCode='" + Quotation[i].ProdCode + "'";
                    _cmd +=
                        ",@ProdDesc='" + Tool.Tool.validateStr(Quotation[i].ProdDescription) + "'";
                    _cmd += ",@UnitPrice=" + Quotation[i].UnitPrice;
                    _cmd += ",@UnitCode='" + Quotation[i].UnitCode + "'";
                    _cmd += ",@Qty=" + Quotation[i].Qty;
                    _cmd += ",@Amt=" + Quotation[i].Amt;
                    _cmd += ",@PricePur=" + Quotation[i].PricePur;
                    _cmd += ",@CostAmt=" + Quotation[i].CostAmt;
                    _cmd += ",@ProfitAmt=" + Quotation[i].ProfitAmt;
                    _cmd += ",@RevNo=" + Quotation[i].RevNo;
                    _cmd +=
                        " ,@GroupCaption1='"
                        + Tool.Tool.validateStr(Quotation[i].GroupCaption1)
                        + "'";
                    _cmd +=
                        " ,@GroupCaption2='"
                        + Tool.Tool.validateStr(Quotation[i].GroupCaption2)
                        + "'";
                    _cmd +=
                        " ,@GroupCaption3='"
                        + Tool.Tool.validateStr(Quotation[i].GroupCaption3)
                        + "'";
                    _cmd += " , @CmpId='" + Quotation[i].CmpId + "'";
                    _cmd += ",@GrossProfitPer=" + Quotation[i].GrossProfitPer;
                    _cmd += ",@UpdUser='" + Quotation[i].UpdUser + "'";
                    _cmd += ",@MainProdCode='" + Quotation[i].MainProdCode + "'";
                    _cmd += ",@MainSeq=" + Quotation[i].MainSeq;
                    _cmd += ",@SeqSort=" + il;

                    if (DB.DBConn.ExecuteTran(_cmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        DB.DBConn.Tran.Rollback();
                        DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                        DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
                        return;
                    }
                    ;
                }

                DB.DBConn.Tran.Commit();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }
    }
}
