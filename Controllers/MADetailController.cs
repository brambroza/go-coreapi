using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MADetailController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        [HttpGet("{id}")]
        public ActionResult<DataTable> Get(string id)
        {
            try
            {
                string _cmd = $"exec dbo.[getMADetail] @MANo='{id}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No data found for the given ID." });
                }

                return Ok(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("Detail/{docNo}/{cmpId}")]
        public ActionResult<DataTable> Get(string docNo, int cmpId)
        {
            try
            {
                string _cmd = $"exec dbo.[getMADetail_PODetail] @MANo='{docNo}'";
                DataTable dt = DB.DBConn.GetDataTable(_cmd);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { Message = "No data found for the given document number." });
                }

                return Ok(dt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody] List<MaDetail> maDetail)
        {
            try
            {
                if (maDetail == null || maDetail.Count == 0)
                {
                    return BadRequest(new { Message = "Invalid data provided." });
                }

                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                // Delete existing records
                string deleteCmd = $"Delete From dbo.MAService_Detail where MANo='{maDetail[0].MANo}'";
                DB.DBConn.ExecuteTran(deleteCmd, DB.DBConn.Cmd, DB.DBConn.Tran);

                // Insert new records
                foreach (var detail in maDetail)
                {
                    string insertCmd = "exec dbo.MAService_DetailTrans";
                    insertCmd += $" @UpdUser='{detail.UpdUser}',";
                    insertCmd += $"@MANo='{detail.MANo}',";
                    insertCmd += $"@Description='{Tool.Tool.validateStr(detail.Description)}',";
                    insertCmd += $"@ServiceType=1,";
                    insertCmd += $"@ProductCode='{detail.ProductCode}',";
                    insertCmd += $"@SerialNumber='{detail.SerialNumber}',";
                    insertCmd += $"@Model='{detail.Model}',";
                    insertCmd += $"@Seq={detail.Seq},";
                    insertCmd += $"@StartDate='{detail.StartDate}',";
                    insertCmd += $"@ExpireDate='{detail.ExpireDate}',";
                    insertCmd += $"@WarningTime='{detail.WarningTime}',";
                    insertCmd += $"@WarningBeforExpireDay={detail.WarningBeforExpireDay},";
                    insertCmd += $"@NotificationQtySet={detail.NotificationQtySet},";
                    insertCmd += $"@NotificationPeriodDay={detail.NotificationPeriodDay},";
                    insertCmd += $"@NotificationQty={detail.NotificationQty},";
                    insertCmd += $"@ServiceGrp={detail.ServiceGrp},";
                    insertCmd += $"@ProjectName='{detail.ProjectName}',";
                    insertCmd += $"@QuotationNo='{detail.QuotationNo}',";
                    insertCmd += $"@PurchaseNo='{detail.PurchaseNo}',";
                    insertCmd += $"@ReferNo='{detail.ReferNo}',";
                    insertCmd += $"@ProductType={detail.ProductType},";
                    insertCmd += $"@SerialNo='{detail.SerialNo}',";
                    insertCmd += $"@LicensNo='{detail.LicensNo}',";
                    insertCmd += $"@SuplName='{detail.SuplName}',";
                    insertCmd += $"@InvoiceNo='{detail.InvoiceNo}',";
                    insertCmd += $"@InvoiceDate='{detail.InvoiceDate}',";
                    insertCmd += $"@BrandName='{detail.BrandName}',";
                    insertCmd += $"@PriceSale={detail.PriceSale},";
                    insertCmd += $"@PricePur={detail.PricePur}";

                    if (DB.DBConn.ExecuteTran(insertCmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        throw new Exception("Failed to insert MA detail.");
                    }
                }

                DB.DBConn.Tran.Commit();
                return Ok(new { Message = "MA details saved successfully." });
            }
            catch (Exception ex)
            {
                DB.DBConn.Tran.Rollback();
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
            finally
            {
                DB.DBConn.DisposeSqlTransaction(DB.DBConn.Tran);
                DB.DBConn.DisposeSqlConnection(DB.DBConn.Cmd);
            }
        }

        [HttpDelete("{id}/{seq}")]
        public ActionResult Delete(string id, int seq)
        {
            try
            {
                string _cmd = $"delete from dbo.MAService_Detail where MANo='{id}' and Seq={seq}";
                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "MA detail deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
