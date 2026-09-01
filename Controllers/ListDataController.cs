using goalongapi.DB;
using goalongapi.Dtos;
using goalongapi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class ListDataController : ControllerBase
    {
        /// <summary>ความยาวสูงสุดของ ListDescription ที่ยอมให้บันทึก</summary>
        private const int MaxListDescriptionLength = 200;

        private readonly DbConnectionFactory _dbFactory;
        private readonly ILogger<ListDataController> _logger;

        public ListDataController(DbConnectionFactory dbFactory, ILogger<ListDataController> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        [HttpGet("[action]/{listid}/{cmpid}")]
        public IActionResult getlistdata( string listid , string cmpid)
        {

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getlistdata] @ListName=" + listid + ", @cmpid='" + cmpid + "'";
            dt = DB.DBConn.GetDataTable(_cmd);
            string qdetail = string.Empty;
            qdetail = JsonConvert.SerializeObject(dt);
            return Ok(qdetail);
        }

        [HttpPost("[action]")]
        public void setlistData(ListData listData) // string listname , int id ,
        {

            string _cmd = "";

            _cmd = "update SystemList  ";
            _cmd += " set  ListDescription ='" + Tool.Tool.validateStr(listData.ListDescription) + "' ";
            _cmd += "where Id =" + listData.Id + "  ";
            _cmd += "and ListName='" + listData.ListName + "'";

            _cmd += "insert into SystemList (Id, ListName, ListDescription, StateActive )";
            _cmd += " select " + listData.Id + ", '" + listData.ListName + "','" + Tool.Tool.validateStr(listData.ListDescription) + "' ,'1'";

            DB.DBConn.ExecuteOnly(_cmd);

        }


        [HttpDelete("[action]")]
        public void DeleteList([FromQuery] int id, [FromQuery] string listname)
        {
            string _cmd = "";
            _cmd = "delete from  SystemList where Id =" + id + " and  ListName ='" + listname + "'";


            DB.DBConn.ExecuteOnly(_cmd);
        }

        /// <summary>
        /// เพิ่มหรือแก้ไขรายการใน master แบบ list (dbo.SystemList) แยกตาม ListName + CmpId
        /// ส่ง Id &lt;= 0 = เพิ่มรายการใหม่ (ระบบออก Id ให้เอง), Id &gt; 0 = แก้ไขรายการเดิม
        /// </summary>
        /// <param name="listData">ข้อมูลรายการที่ต้องการบันทึก</param>
        /// <param name="cancellationToken">token สำหรับยกเลิกคำสั่ง</param>
        /// <returns>MsgReturn — 200 บันทึกสำเร็จ, 400 ข้อมูลไม่ถูกต้องหรือบันทึกไม่สำเร็จ</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> setsystemlistdata(
            [FromBody] SystemListDto listData,
            CancellationToken cancellationToken)
        {
            MsgReturn msgreturn = new MsgReturn();

            if (listData == null
                || string.IsNullOrWhiteSpace(listData.ListName)
                || string.IsNullOrWhiteSpace(listData.ListDescription)
                || string.IsNullOrWhiteSpace(listData.CmpId))
            {
                msgreturn.ReturnCode = "400";
                msgreturn.Msg = "ListName, ListDescription and CmpId are required.";
                return Ok(msgreturn);
            }

            string listName = listData.ListName.Trim();
            string listDescription = listData.ListDescription.Trim();
            string cmpId = listData.CmpId.Trim();

            if (listDescription.Length > MaxListDescriptionLength)
            {
                msgreturn.ReturnCode = "400";
                msgreturn.Msg = "ListDescription is too long.";
                return Ok(msgreturn);
            }

            try
            {
                using (SqlConnection conn = _dbFactory.CreateConnection())
                {
                    await conn.OpenAsync(cancellationToken);

                    if (listData.Id > 0)
                    {
                        // แก้ไข — ล็อกไว้เฉพาะ ListName + CmpId ของตัวเอง ห้ามข้ามบริษัท
                        const string updateSql = @"
UPDATE dbo.SystemList
   SET ListDescription = @ListDescription
 WHERE Id = @Id
   AND ListName = @ListName
   AND CmpId = @CmpId;";

                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = listData.Id });
                            cmd.Parameters.Add(new SqlParameter("@ListName", SqlDbType.NVarChar, 100) { Value = listName });
                            cmd.Parameters.Add(new SqlParameter("@ListDescription", SqlDbType.NVarChar, MaxListDescriptionLength) { Value = listDescription });
                            cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.NVarChar, 50) { Value = cmpId });

                            int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);

                            if (affected == 0)
                            {
                                msgreturn.ReturnCode = "400";
                                msgreturn.Msg = "Data not found !!";
                                msgreturn.CmpId = cmpId;
                                return Ok(msgreturn);
                            }
                        }
                    }
                    else
                    {
                        // เพิ่มใหม่ — ออก Id ต่อ ListName + CmpId เอง (UPDLOCK/HOLDLOCK กัน Id ชนกันตอนยิงพร้อมกัน)
                        const string insertSql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @NewId int;

SELECT @NewId = ISNULL(MAX(Id), 0) + 1
  FROM dbo.SystemList WITH (UPDLOCK, HOLDLOCK)
 WHERE ListName = @ListName
   AND CmpId = @CmpId;

INSERT INTO dbo.SystemList (Id, ListName, ListDescription, StateActive, CmpId)
VALUES (@NewId, @ListName, @ListDescription, '1', @CmpId);

COMMIT TRANSACTION;

SELECT @NewId;";

                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.Add(new SqlParameter("@ListName", SqlDbType.NVarChar, 100) { Value = listName });
                            cmd.Parameters.Add(new SqlParameter("@ListDescription", SqlDbType.NVarChar, MaxListDescriptionLength) { Value = listDescription });
                            cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.NVarChar, 50) { Value = cmpId });

                            await cmd.ExecuteScalarAsync(cancellationToken);
                        }
                    }
                }

                msgreturn.ReturnCode = "200";
                msgreturn.Msg = "Save Success !!";
                msgreturn.CmpId = cmpId;
                return Ok(msgreturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "setsystemlistdata failed for ListName {ListName}", listName);
                msgreturn.ReturnCode = "400";
                msgreturn.Msg = "Error !!";
                return Ok(msgreturn);
            }
        }

        /// <summary>
        /// ลบรายการใน master แบบ list (dbo.SystemList) เฉพาะของบริษัทตัวเอง
        /// </summary>
        /// <param name="id">Id ของรายการ</param>
        /// <param name="listname">ชื่อชุดรายการ เช่น "Warranty"</param>
        /// <param name="cmpid">รหัสบริษัท</param>
        /// <param name="cancellationToken">token สำหรับยกเลิกคำสั่ง</param>
        /// <returns>MsgReturn — 200 ลบสำเร็จ, 400 ไม่พบรายการหรือลบไม่สำเร็จ</returns>
        [HttpDelete("[action]")]
        public async Task<IActionResult> deletesystemlistdata(
            [FromQuery] int id,
            [FromQuery] string listname,
            [FromQuery] string cmpid,
            CancellationToken cancellationToken)
        {
            MsgReturn msgreturn = new MsgReturn();

            if (id <= 0 || string.IsNullOrWhiteSpace(listname) || string.IsNullOrWhiteSpace(cmpid))
            {
                msgreturn.ReturnCode = "400";
                msgreturn.Msg = "id, listname and cmpid are required.";
                return Ok(msgreturn);
            }

            try
            {
                const string deleteSql = @"
DELETE FROM dbo.SystemList
 WHERE Id = @Id
   AND ListName = @ListName
   AND CmpId = @CmpId;";

                using (SqlConnection conn = _dbFactory.CreateConnection())
                {
                    await conn.OpenAsync(cancellationToken);

                    using (SqlCommand cmd = new SqlCommand(deleteSql, conn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                        cmd.Parameters.Add(new SqlParameter("@ListName", SqlDbType.NVarChar, 100) { Value = listname.Trim() });
                        cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.NVarChar, 50) { Value = cmpid.Trim() });

                        int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);

                        if (affected == 0)
                        {
                            msgreturn.ReturnCode = "400";
                            msgreturn.Msg = "Data not found !!";
                            return Ok(msgreturn);
                        }
                    }
                }

                msgreturn.ReturnCode = "200";
                msgreturn.Msg = "Delete Success !!";
                msgreturn.CmpId = cmpid;
                return Ok(msgreturn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "deletesystemlistdata failed for ListName {ListName}", listname);
                msgreturn.ReturnCode = "400";
                msgreturn.Msg = "Error !!";
                return Ok(msgreturn);
            }
        }
    }
}
