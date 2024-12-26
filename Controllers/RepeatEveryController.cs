using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
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
    public class RepeatEveryController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getRepeatEvery([FromQuery] string cmpid, [FromQuery] string docno)
        {
            string _cmd;
            _cmd = "exec dbo.getRepeatEveryDocNo @CmpId='" + cmpid + "' , @DocNo='" + docno + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            List<RepeatEvery> repeats = new List<RepeatEvery>();

            foreach (DataRow r in dt.Rows)
            {
                var repeat = new RepeatEvery()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    DocNo = r["DocNo"].ToString(),
                    DocType = r["DocType"].ToString(),
                    EveryDay = DateTime.Parse(r["EveryDay"].ToString()),
                    ExpiresDate = DateTime.Parse(r["ExpiresDate"].ToString()),
                    RepeatEveryId = r["RepeatEveryId"].ToString(),
                    ExpiresType = r["ExpiresType"].ToString(),
                    RecurringEvery = Convert.ToInt32(r["RecurringEvery"]),
                    IntervalType = Convert.ToInt32(r["IntervalType"]),
                    ExpiresCount = Convert.ToInt32(r["ExpiresCount"]),
                    RevNo = Convert.ToInt32(r["RevNo"]),
                };

                repeats.Add(repeat);
            }

            return Ok(repeats);
        }

        [HttpPost("[action]")]
        public IActionResult setRepeatEvery(RepeatEvery po)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setRepeatEvery";
                _cmd += " @UpdUser  ='" + po.UpdUser + "'";
                _cmd += " ,@RepeatEveryId  ='" + po.RepeatEveryId + "'";
                _cmd += " ,@DocNo  ='" + po.DocNo + "'";
                _cmd += " ,@DocType ='" + po.DocType + "'";
                _cmd +=
                    " ,@ExpiresDate  ='" + po.ExpiresDate.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += " ,@EveryDay  ='" + po.EveryDay.ToString("yyyy-MM-dd", thaiCulture) + "'";
                _cmd += " ,@ExpiresCount =" + po.ExpiresCount;
                _cmd += " ,@IntervalType =" + po.IntervalType;
                _cmd += " ,@RecurringEvery =" + po.RecurringEvery;
                _cmd += " ,@ExpiresType  ='" + po.ExpiresType + "'";
                _cmd += " , @CmpId='" + po.CmpId + "'";
                _cmd += " ,@RevNo =" + po.RevNo;
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
    }
}
