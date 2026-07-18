using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using goalongapi.Installers;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class SystemConfigController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getSystemRoute([FromQuery] string cmpid, [FromQuery] string system)
        {
            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.sp_getsystemroute @CmpId='" + cmpid + "', @System='" + system + "'";

            dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);
            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getSystemEventLog([FromQuery] string cmpid)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            string _cmd;
            DataTable dt = new System.Data.DataTable();
            _cmd = "exec dbo.sp_system_getSystemMarketingTickerEvent @CmpId='" + cmpid + "'";

            dt = DB.DBConn.GetDataTable(_cmd);

            List<SystemEventLog> systemlogs = new List<SystemEventLog>();

            foreach (DataRow r in dt.Rows)
            {
                var systemlog = new SystemEventLog()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    Id = r["Id"].ToString(),
                    RepeatEveryId = r["RepeatEveryId"].ToString(),
                    DocNo = r["DocNo"].ToString(),
                    DocType = r["DocType"].ToString(),
                    ExpiresType = r["ExpiresType"].ToString(),
                    EveryDay = DateTime
                        .Parse(r["EveryDay"].ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture),
                    CmpId = r["CmpId"].ToString(),
                    EventName = r["EventName"].ToString(),
                    CustomerName = r["CustomerName"].ToString(),
                    ImgPath = r["ImgPath"].ToString(),
                    Status = Convert.ToInt32(r["Status"].ToString()),

                    Msg = r["Msg"].ToString(),
                    ModifyDate = r["ModifyDate"].ToString(),
                    ModifyBy = r["ModifyBy"].ToString(),
                    DocNoNew = r["DocNoNew"].ToString(),
                };

                systemlogs.Add(systemlog);
            }
            return Ok(systemlogs);
        }

        [HttpPost("[action]")]
        public IActionResult setSystemEventLog([FromBody] SystemEventLog data)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_system_setSystemMarketingTickerEvent @Id='" + data.Id + "'  ";
                _cmd += " ,@CmpId='" + data.CmpId + "'";
                _cmd += " ,@User='" + data.UpdUser + "'";
                _cmd += " ,@DocNo='" + data.DocNo + "'";
                _cmd += " ,@DocType='" + data.DocType + "'";
                _cmd += " ,@Status=" + data.Status;

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
        public IActionResult setSystemEventLogDate([FromBody] SystemEventLog data)
        {
            MsgReturn msgretrun = new MsgReturn();
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string _cmd = "";
                _cmd =
                    "exec  dbo.sp_system_setSystemMarketingTickerEvent_UpdateDate @Id='"
                    + data.Id
                    + "'  ";
                _cmd += " ,@CmpId='" + data.CmpId + "'";
                _cmd += " ,@User='" + data.UpdUser + "'";
                _cmd += " ,@DocNo='" + data.DocNo + "'";
                _cmd += " ,@DocType='" + data.DocType + "'";
                _cmd +=
                    " ,@EventDay='"
                    + DateTime.Parse(data.EveryDay).ToString("yyyy-MM-dd HH:mm", thaiCulture)
                    + "'";

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
        public IActionResult delSystemEventLog([FromQuery] string CmpId, [FromQuery] string Id)
        {
            MsgReturn msgretrun = new MsgReturn();
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
                "th-TH"
            );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            try
            {
                string _cmd = "";
                _cmd = " delete dbo.SystemMarketingTickerEvent where  Id='" + Id + "'  ";
                _cmd += " and CmpId='" + CmpId + "'";

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



        [HttpGet("[action]/{endpointName}")]
        public IActionResult getConfigTerms([FromQuery] string cmpid, string endpointName)
        {
            string _cmd;
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo(
              "th-TH"
          );
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();


            switch (endpointName.ToLower())
            {
                case "shippingmethod":
                    _cmd = "exec dbo.get_ShippingMethod @CmpId='" + cmpid + "'";
                    break;
                case "serviceterms":
                    _cmd = "exec dbo.get_ServiceTerms @CmpId='" + cmpid + "'";
                    break;
                case "serviceofterms":
                    _cmd = "exec dbo.get_ServiceOfTerms @CmpId='" + cmpid + "'";
                    break;
                case "deliveryterms":
                    _cmd = "exec dbo.get_DeliveryTerms @CmpId='" + cmpid + "'";
                    break;
                case "adjustreason":
                    _cmd = "exec dbo.get_AdjustReason @CmpId='" + cmpid + "'";
                    break;

                 case "salaryreason":
                    _cmd = "exec dbo.get_SalaryReason @CmpId='" + cmpid + "'";
                    break;
                default:
                    _cmd = "exec dbo.get_ShippingMethod @CmpId='" + cmpid + "'";
                    break;
            }



            DataTable dt = DB.DBConn.GetDataTable(_cmd);




            var datas = new List<TermsService>();
            foreach (DataRow row in dt.Rows)
            {
                var data = new TermsService()
                {
                    Id = row["Id"].ToString(),
                    CmpId = row["CmpId"].ToString(),
                    Description = row["Description"].ToString()
                    ,
                    Name = row["Name"].ToString(),
                    UpdUser = row["UpdUser"].ToString()
                    ,
                    CreateAt = DateTime
                        .Parse(row["CreateAt"].ToString())
                        .ToString("yyyy-MM-dd HH:mm", thaiCulture)
                        ,
                    StateActive = int.Parse(row["StateActive"].ToString())
                };


                datas.Add(data);
            }

            return Ok(datas);
        }


        [HttpPost("[action]/{endpointName}")]
        public ActionResult setConfigTerms(TermsService data, string endpointName)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                switch (endpointName.ToLower())
                {
                    case "shippingmethod":
                        _cmd = "exec dbo.set_ShippingMethod  ";
                        break;
                    case "serviceterms":
                        _cmd = "exec dbo.set_ServiceTerms  ";
                        break;
                    case "serviceofterms":
                        _cmd = "exec dbo.set_ServiceOfTerms  ";
                        break;
                    case "deliveryterms":
                        _cmd = "exec dbo.set_DeliveryTerms  ";
                        break;
                     case "adjustreason":
                        _cmd = "exec dbo.set_AdjustReason  ";
                        break;
                    case "salaryreason":
                        _cmd = "exec dbo.set_SalaryReason  ";
                        break;
                    default:
                        _cmd = "exec dbo.set_ShippingMethod ";
                        break;
                }

                _cmd += "   @Id ='" + data.Id + "'";
                _cmd += " , @Name='" + data.Name + "'";
                _cmd += " , @Description='" + data.Description + "'";
                _cmd += " , @CmpId='" + data.CmpId + "'";
                _cmd += " , @UpdUser='" + data.UpdUser + "'";
                _cmd += " , @StateActive=" + data.StateActive;


                
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

        [HttpDelete("[action]/{endpointName}")]
        public ActionResult delConfigTerms([FromQuery] string cmpId, [FromQuery] string id, string endpointName)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                switch (endpointName.ToLower())
                {
                    case "shippingmethod":
                        _cmd = "delete from  dbo.ShippingMethod  ";
                        break;
                    case "serviceterms":
                        _cmd = "delete from dbo.ServiceTerms  ";
                        break;
                    case "serviceofterms":
                        _cmd = "delete from dbo.ServiceOfTerms  ";
                        break;
                    case "deliveryterms":
                        _cmd = "delete from  dbo.DeliveryTerms  ";
                        break;
                    case "adjustreason":
                        _cmd = "delete from [inven].[AdjustReason]  ";
                        break;
                    case "salaryreason":
                        _cmd = "delete from [hr].[Salary_Increase_Reason]  ";
                        break;
                    default:
                        _cmd = "delete from  dbo.ShippingMethod ";
                        break;
                }

                _cmd += "  where  Id ='" + id + "'";
                _cmd += "  and CmpId='" + cmpId + "'";


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




    }
}
