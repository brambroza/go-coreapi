using System;
using System.Collections.Generic;
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
        public IActionResult getSocialChannel([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getCompanySocailChannel @CmpId='" + cmpid + "' , @User='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
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
        public IActionResult setSocialChannel(cmpSocialChannel cmp)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";

                _cmd = "exec  dbo.set_company";
                _cmd += " @UpdUser  ='" + cmp.UpdUser + "'";
                _cmd += ",@CmpId  ='" + cmp.CmpId + "'";
                _cmd += ",@AccountHook  ='" + cmp.AccountHook + "'";
                _cmd += ",@SocialType  ='" + cmp.SocialType + "'";
                _cmd += ",@Seq =" + cmp.Seq;
                _cmd += ",@AccountName='" + cmp.AccountName + "'";

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
