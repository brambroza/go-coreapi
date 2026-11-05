using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using goalongapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace goalongapi.Controllers.Master
{


    [ApiController]
    [Authorize]
    public class MasterController : ControllerBase
    {

        private const string RdVatServiceUrl = "https://rdws.rd.go.th/serviceRD3/vatserviceRD3.asmx";
        private static readonly XNamespace RdVatNs = "https://rdws.rd.go.th/serviceRD3/vatserviceRD3";

        private readonly IHttpClientFactory _httpClientFactory;

        public MasterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        [Route("province")]
        [HttpGet]
        public IActionResult getProvince()
        {
            string _cmd;
            _cmd = "exec dbo.getmProvince ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("districts")]
        [HttpGet]
        public IActionResult getDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("subdistricts")]
        [HttpGet]
        public IActionResult getSubDistricts()
        {
            string _cmd;
            _cmd = "exec dbo.getmSubDistricts ";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("setCustomerDBD")]
        [HttpPost]
        public IActionResult Post(CustomerDBD cusdb)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setCustomerFromDBD @juristicID='" + cusdb.juristicID + "'";
                _cmd += " ,@juristicNameTH='" + cusdb.juristicNameTH + "'";
                _cmd += " ,@juristicNameEN='" + cusdb.juristicNameEN + "'";
                _cmd += " ,@juristicType='" + cusdb.juristicType + "'";
                _cmd += " ,@registerDate='" + cusdb.registerDate + "'";
                _cmd += " ,@juristicStatus='" + cusdb.juristicStatus + "'";
                _cmd += " ,@registerCapital='" + cusdb.registerCapital + "'";
                _cmd += " ,@standardObjective='" + cusdb.standardObjective + "'";
                _cmd +=
                    " ,@objectiveDescription='"
                    + cusdb.standardObjectiveDetail.objectiveDescription
                    + "'";
                _cmd += " ,@addressName='" + cusdb.addressDetail.addressName + "'";
                _cmd += " ,@buildingName='" + cusdb.addressDetail.buildingName + "'";
                _cmd += " ,@roomNo='" + cusdb.addressDetail.roomNo + "'";
                _cmd += " ,@floor='" + cusdb.addressDetail.floor + "'";
                _cmd += " ,@moo='" + cusdb.addressDetail.moo + "'";
                _cmd += " ,@soi='" + cusdb.addressDetail.soi + "'";
                _cmd += " ,@street='" + cusdb.addressDetail.street + "'";
                _cmd += " ,@subDistrict='" + cusdb.addressDetail.subDistrict + "'";
                _cmd += " ,@district='" + cusdb.addressDetail.district + "'";
                _cmd += " ,@province='" + cusdb.addressDetail.province + "'";

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

        [HttpGet("[action]")]
        public IActionResult getBank([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getBank @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getBankType([FromQuery] string cmpid, [FromQuery] string user)
        {
            string _cmd;
            _cmd = "exec dbo.getBankType @CmpId='" + cmpid + "' , @user='" + user + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getBankBranch([FromQuery] string cmpid)
        {
            string _cmd;
            _cmd = "exec dbo.getBankBranch @CmpId='" + cmpid + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public IActionResult setBank(Bank bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmBank";
                _cmd += "  @UserName  ='" + bk.UserName + "'";
                _cmd += " ,@BankCode ='" + bk.BankCode + "'";
                _cmd += " ,@BankName ='" + bk.BankName + "'";
                _cmd += " ,@Remark  ='" + bk.Remark + "'";
                _cmd += " ,@StateActive =" + bk.StateActive;
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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
        public IActionResult delBank([FromQuery] string cmpid, [FromQuery] string id)
        {
            string _cmd;
            _cmd = " delete from dbo.mBank where  CmpId='" + cmpid + "' and BankCode='" + id + "'";
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult setBankBranch(BankBranch bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setmBankBranch";
                _cmd += "  @UserName  ='" + bk.UserName + "'";
                _cmd += " ,@BankCode ='" + bk.BankCode + "'";
                _cmd += " ,@BankBranchCode ='" + bk.BankBranchCode + "'";
                _cmd += " ,@BankBranchName ='" + bk.BankBranchName + "'";
                _cmd += " ,@Address ='" + bk.Address + "'";
                _cmd += " ,@AddrProvince ='" + bk.AddrProvince + "'";
                _cmd += " ,@AddrDistrict ='" + bk.AddrDistrict + "'";
                _cmd += " ,@AddrSubDistrict ='" + bk.AddrSubDistrict + "'";
                _cmd += " ,@AddrPostCode ='" + bk.AddrPostCode + "'";
                _cmd += " ,@Fax ='" + bk.Fax + "'";
                _cmd += " ,@Phone ='" + bk.Phone + "'";
                _cmd += " ,@Remark  ='" + bk.Remark + "'";
                _cmd += " ,@StateActive =" + bk.StateActive;
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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
        public IActionResult delBankBranch([FromQuery] string cmpid, [FromQuery] string id)
        {
            string _cmd;
            _cmd =
                " delete from dbo.mBankBranch where  CmpId='"
                + cmpid
                + "' and BankBranchCode='"
                + id
                + "'";
            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [Route("bussinetGrp")]
        [HttpGet]
        public IActionResult getBussinetGrp([FromQuery] string CmpId)
        {
            string _cmd;
            _cmd = "exec dbo.sp_getmBusinessGrp @CmpId='" + CmpId + "'";
            DataTable datatable = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(datatable);
            return Ok(JSONString);
        }

        [Route("delbusinessGrp")]
        [HttpDelete]
        public IActionResult delbusinessGrp([FromQuery] string CmpId, [FromQuery] string id)
        {
            string _cmd;
            _cmd =
                " delete from   msb.mBusinessGrp  where CmpId='"
                + CmpId
                + "' and BusinessGrpCode='"
                + id
                + "'";

            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult setBussinetGrp(mBussinetGrp bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetBusinessGrp";
                _cmd += "  @UpdUser  ='" + bk.UpdUser + "'";
                _cmd += " ,@BusinessGrpCode ='" + bk.BusinessGrpCode + "'";
                _cmd += " ,@BusinessGrpName ='" + bk.BusinessGrpName + "'";
                _cmd += " ,@BusinessGrpDescription ='" + bk.BusinessGrpDescription + "'";
                _cmd += " ,@StateActive =" + bk.StateActive + "";
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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



        /* source customer  */

        [Route("sourcelist")]
        [HttpGet]
        public IActionResult getsource([FromQuery] string CmpId)
        {
            string _cmd;
            _cmd = "exec dbo.sp_getmsource @CmpId='" + CmpId + "'";
            DataTable dt = DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            var prodlist = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var eventObj = new Dictionary<string, object>();
                foreach (DataColumn column in dt.Columns)
                {
                    string lowercaseColumnName =
                        char.ToLowerInvariant(column.ColumnName[0])
                        + column.ColumnName.Substring(1);

                    eventObj[lowercaseColumnName] = row[column];
                }

                prodlist.Add(eventObj);
            }


            return Ok(prodlist);
        }

        [Route("delsource")]
        [HttpDelete]
        public IActionResult delsource([FromQuery] string CmpId, [FromQuery] string id)
        {
            string _cmd;
            _cmd =
                " delete from   msb.mCustomerSource  where CmpId='"
                + CmpId
                + "' and SourceCode='"
                + id
                + "'";

            DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult setsource(mSource bk)
        {
            MsgReturn msgretrun = new MsgReturn();

            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_SetCustomerSource";
                _cmd += "  @UpdUser  ='" + bk.UpdUser + "'";
                _cmd += " ,@SourceCode ='" + bk.SourceCode + "'";
                _cmd += " ,@SourceName ='" + bk.SourceName + "'";
                _cmd += " ,@StateActive =" + bk.StateActive + "";
                _cmd += " ,@CmpId ='" + bk.CmpId + "'";

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

        [Route("rdvat/{taxno}")]
        [HttpGet]
        public async Task<IActionResult> GetRdVatInfo(string taxno, [FromQuery] int branch = 0)
        {
            if (string.IsNullOrEmpty(taxno) || !Regex.IsMatch(taxno, @"^\d{13}$"))
            {
                return BadRequest(new { Message = "taxno must be 13 digits" });
            }
            if (branch < 0 || branch > 99999)
            {
                return BadRequest(new { Message = "branch must be 0-99999" });
            }

            // taxno/branch are digits only (validated above), safe to place in the XML body.
            string soapBody =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>"
                + "<Service xmlns=\"https://rdws.rd.go.th/serviceRD3/vatserviceRD3\">"
                + "<username>anonymous</username><password>anonymous</password>"
                + "<TIN>" + taxno + "</TIN>"
                + "<ProvinceCode>0</ProvinceCode><BranchNumber>" + branch + "</BranchNumber><AmphurCode>0</AmphurCode>"
                + "</Service></soap:Body></soap:Envelope>";

            XDocument doc;
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                using var request = new HttpRequestMessage(HttpMethod.Post, RdVatServiceUrl)
                {
                    Content = new StringContent(soapBody, Encoding.UTF8, "text/xml"),
                };
                request.Headers.Add("SOAPAction", "\"https://rdws.rd.go.th/serviceRD3/vatserviceRD3/Service\"");

                using HttpResponseMessage response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(502, new { Message = "RD service error", Status = (int)response.StatusCode });
                }

                doc = XDocument.Parse(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[rdvat] {ex.Message}");
                return StatusCode(502, new { Message = "RD service unavailable" });
            }

            XElement? result = doc.Descendants(RdVatNs + "ServiceResult").FirstOrDefault();
            if (result == null)
            {
                return StatusCode(502, new { Message = "RD service returned unexpected response" });
            }

            string msgErr = ReadRdField(result, "vmsgerr");
            string name = ReadRdField(result, "vName");
            if (!string.IsNullOrEmpty(msgErr) || string.IsNullOrEmpty(name))
            {
                return NotFound(new { Message = "Taxpayer not found" });
            }

            string titleName = ReadRdField(result, "vtitleName");
            string surname = ReadRdField(result, "vSurname");
            string fullName = string.Join(" ", new[] { titleName, name, surname }.Where(x => !string.IsNullOrEmpty(x)));

            var addressParts = new List<string>();
            void AddPart(string prefix, string field)
            {
                string value = ReadRdField(result, field);
                if (!string.IsNullOrEmpty(value)) addressParts.Add(prefix + value);
            }
            AddPart("", "vBuildingName");
            AddPart("ชั้น ", "vFloorNumber");
            AddPart("ห้อง ", "vRoomNumber");
            AddPart("เลขที่ ", "vHouseNumber");
            AddPart("หมู่ ", "vMooNumber");
            AddPart("หมู่บ้าน", "vVillageName");
            AddPart("ซ.", "vSoiName");
            AddPart("แยก", "vYaek");
            AddPart("ถ.", "vStreetName");

            var info = new RdVatInfo
            {
                taxId = ReadRdField(result, "vNID"),
                titleName = titleName,
                name = fullName,
                branchNumber = ReadRdField(result, "vBranchNumber"),
                branchName = ReadRdField(result, "vBranchName"),
                address = string.Join(" ", addressParts),
                subDistrict = ReadRdField(result, "vThambol"),
                district = ReadRdField(result, "vAmphur"),
                province = ReadRdField(result, "vProvince"),
                postCode = ReadRdField(result, "vPostCode"),
            };

            return Ok(info);
        }

        /// <summary>
        /// Reads the first anyType value of an RD result field; "-" and blanks become "".
        /// </summary>
        private static string ReadRdField(XElement result, string field)
        {
            string value = result.Element(RdVatNs + field)?.Elements().FirstOrDefault()?.Value?.Trim() ?? "";
            value = Regex.Replace(value, @"\s{2,}", " ");
            return value == "-" ? "" : value;
        }

    }
}
