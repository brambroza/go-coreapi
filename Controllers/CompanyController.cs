using coreapi.Models;
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

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
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
        public IActionResult getfileall(string cmpid, string groupname, string foldername, string fileName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cmpid, groupname, foldername, fileName);
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
        public IActionResult DeleteFile([FromQuery] string fileUrl)
        {
            try
            {

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileUrl);  // Replace this with your logic to get the file path based on the URL
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
        public IActionResult deletefileall(string cmpid, string groupname, string foldername, string fileName)
        {

            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cmpid, groupname, foldername, fileName);
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
