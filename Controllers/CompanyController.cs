using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult getcmpinfo([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getcmpinfo @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        } 

        [HttpGet("[action]")]
        public IActionResult getPaymentMethod([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getPaymentmethod @CmpId='" + cmpid + "' , @userlogin='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            /*   string JSONString = string.Empty;
              JSONString = JsonConvert.SerializeObject(dt);
  
              return Ok(new { payments = JSONString}); */

            List<paymentmethod> paymentmethods = new List<paymentmethod>();

            foreach (DataRow r in dt.Rows)
            {
                var paymentmethod = new paymentmethod()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    BankAccCode = r["BankAccCode"].ToString(),
                    PaymentMethodId = int.Parse(r["PaymentMethodId"].ToString()),
                    BankAccName = r["BankAccName"].ToString(),
                    BankBranchCode = r["BankBranchCode"].ToString(),
                    BankCode = r["BankCode"].ToString(),
                    BankType = int.Parse(r["BankType"].ToString()),
                    BankTypeName = r["BankTypeName"].ToString(),
                };

                paymentmethods.Add(paymentmethod);
            }

            return Ok(new { payments = paymentmethods });
        }

        [HttpGet("[action]")]
        public IActionResult getSocialChannelLiffApp([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getCompanySocialChannel_LiffApp @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            List<cmpSocialChannel_LiffApp> cmpSocialChannels = new List<cmpSocialChannel_LiffApp>();

            foreach (DataRow r in dt.Rows)
            {
                var cmpSocialChannel = new cmpSocialChannel_LiffApp()
                { 
                    CmpId = r["CmpId"].ToString(), 
                    ChannelId = r["ChannelId"].ToString(),
                    LiffId = r["LiffId"].ToString(),
                    AppName = r["AppName"].ToString(),
                };

                cmpSocialChannels.Add(cmpSocialChannel);
            }

            return Ok(cmpSocialChannels);
        }


        [HttpGet("[action]")]
        public IActionResult getSocialChannel([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getCompanySocialChannel @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);

            List<cmpSocialChannel> cmpSocialChannels = new List<cmpSocialChannel>();

            foreach (DataRow r in dt.Rows)
            {
                var cmpSocialChannel = new cmpSocialChannel()
                {
                    UpdUser = r["UpdUser"].ToString(),
                    CmpId = r["CmpId"].ToString(),
                    Seq = int.Parse(r["Seq"].ToString()),
                    Platform = r["Platform"].ToString(),
                    ChannelId = r["ChannelId"].ToString(),
                    ApiKey = r["ApiKey"].ToString(),
                    WebhookUrl = r["WebhookUrl"].ToString(),
                    AccessToken = r["AccessToken"].ToString(),
                    PageId = r["PageId"].ToString(),
                    PhoneNumber = r["PhoneNumber"].ToString(),
                    Name = r["Name"].ToString(),
                };

                cmpSocialChannels.Add(cmpSocialChannel);
            }

            return Ok(cmpSocialChannels);
        }


        [HttpPost("[action]")]
        public IActionResult setLiffApp([FromBody] cmpSocialChannel_LiffApp cmp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.setLiffApp";
                _cmd += " @UpdUser  ='" + cmp.UpdUser + "'";
                _cmd += ",@CmpId  ='" + cmp.CmpId + "'"; 
                _cmd += ",@AppName  ='" + cmp.AppName + "'";
                _cmd += ",@ChannelId  ='" + cmp.ChannelId + "'";
                _cmd += ",@LiffId  ='" + cmp.LiffId + "'";
                _cmd += ",@ChannelId  ='" + cmp.ChannelId + "'"; 

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }



        [HttpPost("[action]")]
        public IActionResult setSocialChannel([FromBody] cmpSocialChannel cmp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.setCompanySocialChannel";
                _cmd += " @UpdUser  ='" + cmp.UpdUser + "'";
                _cmd += ",@CmpId  ='" + cmp.CmpId + "'";
                _cmd += ",@Seq =" + cmp.Seq;
                _cmd += ",@Platform  ='" + cmp.Platform + "'";
                _cmd += ",@ChannelId  ='" + cmp.ChannelId + "'";
                _cmd += ",@ApiKey  ='" + cmp.ApiKey + "'";
                _cmd += ",@WebhookUrl  ='" + cmp.WebhookUrl + "'";
                _cmd += ",@AccessToken  ='" + cmp.AccessToken + "'";
                _cmd += ",@PageId  ='" + cmp.PageId + "'";
                _cmd += ",@PhoneNumber  ='" + cmp.PhoneNumber + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpDelete("[action]")]
        public IActionResult delSocialChannel([FromQuery] string cmpid, [FromQuery] int id)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.delCompanySocialChannel";
                _cmd += " @CmpId  ='" + cmpid + "'";
                _cmd += ",@Seq =" + id;

                if (DB.DBConn.ExecuteOnly(_cmd))
                {
                    msgretrun.ReturnCode = "200";
                    msgretrun.Msg = "Delete Success !!";
                    return Ok(msgretrun);
                }
                else
                {
                    msgretrun.ReturnCode = "400";
                    msgretrun.Msg = "Error !!";
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var imagePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                fileName
            );
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, "image/jpeg");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("getfileall/{cmpid}/{groupname}/{foldername}/{fileName}")]
        public IActionResult getfileall(
            string cmpid,
            string groupname,
            string foldername,
            string fileName
        )
        {
            var imagePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                cmpid,
                groupname,
                foldername,
                fileName
            );
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, "image/jpeg");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("[action]")]
        public IActionResult setPaymentMethod(paymentmethod pm)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.set_PaymentMethod";
                _cmd += " @UpdUser  ='" + pm.UpdUser + "'";
                _cmd += ",@CmpId  ='" + pm.CmpId + "'";
                _cmd += ",@PaymentMethodId  =" + pm.PaymentMethodId + "";
                _cmd += ",@BankAccCode  ='" + pm.BankAccCode + "'";
                _cmd += ",@BankAccName  ='" + pm.BankAccName + "'";
                _cmd += ",@BankCode  ='" + pm.BankCode + "'";
                _cmd += ",@BankBranchCode  ='" + pm.BankBranchCode + "'";
                _cmd += ",@BankType  ='" + pm.BankType + "'";
                _cmd += ",@BankTypeName ='" + pm.BankTypeName + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpPost("[action]")]
        public IActionResult delPaymentMethod(paymentmethod pm)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.set_PaymentMethod_Del";
                _cmd += " @UpdUser  ='" + pm.UpdUser + "'";
                _cmd += ",@CmpId  ='" + pm.CmpId + "'";
                _cmd += ",@PaymentMethodId  =" + pm.PaymentMethodId + "";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCmpinfo(cmpinfo cmp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.set_company";
                _cmd += " @UpdUser  ='" + cmp.UpdUser + "'";
                _cmd += ",@CmpId  ='" + cmp.CmpId + "'";
                _cmd += ",@CmpName  ='" + cmp.CmpName + "'";
                _cmd += ",@CmpAddress  ='" + cmp.CmpAddress + "'";
                _cmd += ",@CmpTaxid  ='" + cmp.CmpTaxid + "'";
                _cmd += ",@CmpType =" + cmp.CmpType;
                _cmd += ",@StateActive =" + cmp.StateActive;
                _cmd += ",@Email  ='" + cmp.Email + "'";
                _cmd += ",@Fax  ='" + cmp.Fax + "'";
                _cmd += ",@Phone  ='" + cmp.Phone + "'";
                _cmd += ",@DateCreate ='" + cmp.DateCreate + "'";
                _cmd += ",@DateExprie ='" + cmp.DateExprie + "'";
                _cmd += ",@TelOffice  ='" + cmp.TelOffice + "'";
                _cmd += ",@CmpImg  ='" + cmp.CmpImg + "'";
                _cmd += ",@AddressShip  ='" + cmp.AddressShip + "'";
                _cmd += ",@AddrProvince  ='" + cmp.AddrProvince + "'";
                _cmd += ",@AddrDistrict  ='" + cmp.AddrDistrict + "'";
                _cmd += ",@AddrSubDistrict  ='" + cmp.AddrSubDistrict + "'";
                _cmd += ",@AddrPostCode  ='" + cmp.AddrPostCode + "'";
                _cmd += ",@CmpBranchCode  ='" + cmp.CmpBranchCode + "'";
                _cmd += ",@CmpBranchName  ='" + cmp.CmpBranchName + "'";
                _cmd += ",@WebSite  ='" + cmp.WebSite + "'";
                _cmd += ",@Remark  ='" + cmp.Remark + "'";
                _cmd += ",@DocPrefix  ='" + cmp.DocPrefix + "'";
                _cmd += ",@BankAccCode  ='" + cmp.BankAccCode + "'";
                _cmd += ",@BankAccName  ='" + cmp.BankAccName + "'";
                _cmd += ",@BankAccType  ='" + cmp.BankAccType + "'";
                _cmd += ",@BankCode  ='" + cmp.BankCode + "'";
                _cmd += ",@BankBranchCode  ='" + cmp.BankBranchCode + "'";
                _cmd += ",@LineId  ='" + cmp.LineId + "'";
                _cmd += ",@ColorThemeReport  ='" + cmp.ColorThemeReport + "'";
                _cmd += ",@FaviconUrl  ='" + cmp.FaviconUrl + "'";
                _cmd += ",@CmpNameEN  ='" + cmp.CmpNameEN + "'";
                _cmd += ",@CmpAddressEN  ='" + cmp.CmpAddressEN + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpPost("[action]")]
        public IActionResult setCmpImg(datacmpimg img)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_register_company_imgpath";
                _cmd += " @cmpid  ='" + img.cmpid + "'";
                _cmd += " ,@imgpath  ='" + img.imgpath + "'";

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
                    return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
                }
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return StatusCode(400, new { Message = msgretrun.Msg, Error = msgretrun.Msg });
            }
        }

        [HttpDelete("[action]")]
        public IActionResult DeleteFile([FromQuery] string fileUrl)
        {
            try
            {
                string filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    fileUrl
                ); // Replace this with your logic to get the file path based on the URL
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the deletion process
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("deletefileall/{cmpid}/{groupname}/{foldername}/{fileName}")]
        public IActionResult deletefileall(
            string cmpid,
            string groupname,
            string foldername,
            string fileName
        )
        {
            try
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    cmpid,
                    groupname,
                    foldername,
                    fileName
                );
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the deletion process
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
