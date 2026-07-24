using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Controllers.Master
{
    [ApiController]
    [Authorize]
    public class JobtypeController : ControllerBase
    {
        // GET: api/Jobtype
        [HttpGet("[action]")]
        public IActionResult GetJobtype([FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.getJobtypelist @CmpId=" + cmpid + "";
            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        } 

        // POST: api/Jobtype
        [HttpPost("[action]")]
        public IActionResult setJobtype(Jobtype jt)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setJobtype";
                _cmd += " @UpdUser  ='" + jt.UpdUser + "'";
                _cmd += ",@JobTypeCode  ='" + jt.JobTypeCode + "'";
                _cmd += ",@JobTypeName  ='" + jt.JobTypeName + "'";
                _cmd += ",@JobTypeDescripton  ='" + jt.JobTypeDescripton + "'";
                _cmd += ",@JobTypeStateActive =" + jt.JobTypeStateActive;
                _cmd += ",@CmpId  ='" + jt.CmpId + "'";

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
        public void DeleteJobtype([FromQuery] string jobid)
        {
            string _cmd = "";
            _cmd = "delete from msb.mJobtype where  JobTypeCode='" + jobid + "' ";
            DB.DBConn.ExecuteOnly(_cmd);
        }

        // ── NIS service-scoped endpoints ─────────────────────────────────────
        // แยกออกจาก SP กลาง (getJobtypelist/setJobtype) เพื่อรองรับคอลัมน์ JobTypeGroup
        // และไม่กระทบโมดูลอื่น (quotation/purchasetracking). ใช้ parameterized query ทุกจุด.

        /// <summary>ดึง job type ตามหมวด (เช่น group=service สำหรับงานบริการ NIS).</summary>
        [HttpGet("[action]")]
        public IActionResult GetJobtypeByGroup([FromQuery] string cmpid, [FromQuery] string group)
        {
            string _cmd =
                "SELECT UpdUser, JobTypeCode, JobTypeName, JobTypeDescripton, " +
                "JobTypeStateActive, CmpId, JobTypeGroup " +
                "FROM msb.mJobType " +
                "WHERE CmpId = @CmpId AND JobTypeGroup = @JobTypeGroup " +
                "ORDER BY JobTypeCode";

            DataTable dt = DB.DBConn.GetDataTableParam(_cmd, new[]
            {
                new SqlParameter("@CmpId", (object)cmpid ?? DBNull.Value),
                new SqlParameter("@JobTypeGroup", (object)group ?? DBNull.Value),
            });

            return Ok(JsonConvert.SerializeObject(dt));
        }

        /// <summary>Upsert job type พร้อมหมวด (JobTypeGroup) — insert ถ้ายังไม่มี, update ถ้ามีแล้ว.</summary>
        [HttpPost("[action]")]
        public IActionResult setJobtypeGroup(Jobtype jt)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd =
                    "IF EXISTS (SELECT 1 FROM msb.mJobType WHERE JobTypeCode = @JobTypeCode AND CmpId = @CmpId) " +
                    "UPDATE msb.mJobType SET " +
                    "  JobTypeName = @JobTypeName, " +
                    "  JobTypeDescripton = @JobTypeDescripton, " +
                    "  JobTypeStateActive = @JobTypeStateActive, " +
                    "  JobTypeGroup = @JobTypeGroup, " +
                    "  UpdUser = @UpdUser, UpdDate = CAST(GETDATE() AS date), UpdTime = CAST(GETDATE() AS time) " +
                    "WHERE JobTypeCode = @JobTypeCode AND CmpId = @CmpId " +
                    "ELSE " +
                    "INSERT INTO msb.mJobType " +
                    "  (JobTypeCode, JobTypeName, JobTypeDescripton, JobTypeStateActive, JobTypeGroup, CmpId, UpdUser, UpdDate, UpdTime) " +
                    "VALUES " +
                    "  (@JobTypeCode, @JobTypeName, @JobTypeDescripton, @JobTypeStateActive, @JobTypeGroup, @CmpId, @UpdUser, CAST(GETDATE() AS date), CAST(GETDATE() AS time))";

                var ok = DB.DBConn.ExecuteOnlyParam(_cmd,
                    new SqlParameter("@JobTypeCode", (object)jt.JobTypeCode ?? DBNull.Value),
                    new SqlParameter("@JobTypeName", (object)jt.JobTypeName ?? DBNull.Value),
                    new SqlParameter("@JobTypeDescripton", (object)jt.JobTypeDescripton ?? DBNull.Value),
                    new SqlParameter("@JobTypeStateActive", jt.JobTypeStateActive),
                    new SqlParameter("@JobTypeGroup", (object)jt.JobTypeGroup ?? DBNull.Value),
                    new SqlParameter("@CmpId", (object)jt.CmpId ?? DBNull.Value),
                    new SqlParameter("@UpdUser", (object)jt.UpdUser ?? DBNull.Value));

                if (ok)
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Save Success !!";
                    return Ok(msgretrun);
                }

                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        /// <summary>ลบ job type ตาม code — scope ด้วย CmpId เพื่อไม่ให้ลบข้ามบริษัท.</summary>
        [HttpDelete("[action]")]
        public IActionResult DeleteJobtypeByCode([FromQuery] string jobid, [FromQuery] string cmpid)
        {
            MsgReturn msgretrun = new MsgReturn();
            string _cmd = "DELETE FROM msb.mJobType WHERE JobTypeCode = @JobTypeCode AND CmpId = @CmpId";

            var ok = DB.DBConn.ExecuteOnlyParam(_cmd,
                new SqlParameter("@JobTypeCode", (object)jobid ?? DBNull.Value),
                new SqlParameter("@CmpId", (object)cmpid ?? DBNull.Value));

            msgretrun.ReturnCode = ok ? "200" : "400";
            msgretrun.Msg = ok ? "Delete Success !!" : "Error !!";
            return Ok(msgretrun);
        }
    }
}
