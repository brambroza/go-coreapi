using goalongapi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MAServiceController : ControllerBase
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
                string _cmd = $"exec dbo.[getMAService] @MANo='{id}'";
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

        [HttpPost]
        public ActionResult Post([FromBody] List<MaService> maServices)
        {
            if (maServices == null || maServices.Count == 0)
            {
                return BadRequest(new { Message = "Invalid data provided." });
            }

            try
            {
                DB.DBConn.SqlConnectionOpen();
                DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
                DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();

                // Delete existing records
                string deleteCmd = $"Delete From dbo.MAService_Service where MANo='{maServices[0].MANo}'";
                DB.DBConn.ExecuteTran(deleteCmd, DB.DBConn.Cmd, DB.DBConn.Tran);

                // Insert new records
                foreach (var service in maServices)
                {
                    string insertCmd = "exec dbo.MAService_ServiceTrans";
                    insertCmd += $" @UpdUser='{service.UpdUser}',";
                    insertCmd += $"@MANo='{service.MANo}',";
                    insertCmd += $"@ServiceType={service.ServiceType},";
                    insertCmd += $"@Description='{service.Description}',";
                    insertCmd += $"@Model='{service.Model}',";
                    insertCmd += $"@Seq={service.Seq},";
                    insertCmd += $"@StartDate='{service.StartDate}',";
                    insertCmd += $"@ExpireDate='{service.ExpireDate}',";
                    insertCmd += $"@WarningTime='{service.WarningTime}',";
                    insertCmd += $"@WarningBeforExpireDay={service.WarningBeforExpireDay},";
                    insertCmd += $"@NotificationQtySet={service.NotificationQtySet},";
                    insertCmd += $"@NotificationPeriodDay={service.NotificationPeriodDay},";
                    insertCmd += $"@NotificationQty={service.NotificationQty},";
                    insertCmd += $"@ServiceGrp={service.ServiceGrp},";
                    insertCmd += $"@PurchaseNo='{service.PurchaseNo}',";
                    insertCmd += $"@ReferNo='{service.ReferNo}',";
                    insertCmd += $"@ProjectName='{service.ProjectName}',";
                    insertCmd += $"@QuotationNo='{service.QuotationNo}',";
                    insertCmd += $"@PricePur={service.PricePur},";
                    insertCmd += $"@PriceSale={service.PriceSale}";

                    if (DB.DBConn.ExecuteTran(insertCmd, DB.DBConn.Cmd, DB.DBConn.Tran) <= 0)
                    {
                        throw new Exception("Failed to insert MAService record.");
                    }
                }

                DB.DBConn.Tran.Commit();
                return Ok(new { Message = "MAService records saved successfully." });
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
                string _cmd = $"delete from dbo.MAService_Service where MANo='{id}' and Seq={seq}";
                DB.DBConn.ExecuteOnly(_cmd);

                return Ok(new { Message = "MAService record deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
