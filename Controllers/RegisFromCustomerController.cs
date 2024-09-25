using coreapi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Net;
using System.Text;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google;
using System.Text.Json;
using goalongapi.Interfaces;


namespace coreapi.Controllers
{

    [ApiController]
    public class RegisFromCustomerController : ControllerBase
    {


        private readonly IProductService productService;


        public RegisFromCustomerController(IWebHostEnvironment webHostEnvironment, ILogger<RegisFromCustomerController> logger, IProductService productService)
        {

            this.productService = productService;

        } 




        [HttpPost("sendMAFortigate")]
        public async Task<IActionResult> MAFortigate(List<IFormFile> formFiles, [FromForm] MAFortigate request)
        {


            var url = await UploadFilesAsyn(formFiles);

            string _cmd;
            _cmd = "exec  dbo.setMAFortigate";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @ModelName  ='" + request.model + "'";
            _cmd += ", @SerialNo  ='" + request.serial + "'";
            _cmd += ", @Forticloud  ='" + request.forticloud + "'";
            _cmd += ", @MADuration  ='" + request.maDuration + "'";
            _cmd += ", @AdvanceReplacement  ='" + request.advanceReplacement + "'";
            _cmd += ", @SLA  ='" + request.sla + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
               if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            {
                _cmd += ", @FileUrl  =''";
            }


            DB.DBConn.ExecuteOnly(_cmd);

            return Ok(new { message = "Message sent" });
        }


        [HttpPost("sendMASiscoServer")]
        public async Task<IActionResult> MACiscoServer(List<IFormFile> formFiles, [FromForm] MACiscoServer request)
        {


            var url = await UploadFilesAsyn(formFiles);



            string _cmd;
            _cmd = "exec  dbo.setMACiscoServer";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @ModelName  ='" + request.model + "'";
            _cmd += ", @SerialNo  ='" + request.serial + "'";
            _cmd += ", @PartNo  ='" + request.partNumber + "'";
            _cmd += ", @MABy  ='" + request.maBy + "'";
            _cmd += ", @MADuration  ='" + request.maDuration + "'";
            _cmd += ", @AdvanceReplacement  ='" + request.advanceReplacement + "'";
            _cmd += ", @SLA  ='" + request.sla + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
            if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            {
                _cmd += ", @FileUrl  =''";
            }


            DB.DBConn.ExecuteOnly(_cmd);

            return Ok(new { message = "Message sent" });
        }



        [HttpPost("sendReqOther")]
        public async Task<IActionResult> ReqOther(List<IFormFile> formFiles, [FromForm] MAOther request)
        {


            var url = await UploadFilesAsyn(formFiles);

            string _cmd;
            _cmd = "exec  dbo.setReqOther";
            _cmd += " @CustomerName  ='" + request.cmpName + "'";
            _cmd += ", @ContactName  ='" + request.contactName + "'";
            _cmd += ", @ContactPhone  ='" + request.contactPhone + "'";
            _cmd += ", @ContactEmail  ='" + request.contactEmail + "'";
            _cmd += ", @Address  ='" + request.address + "'";
            _cmd += ", @ServiceType  ='" + request.serviceType + "'";
            _cmd += ", @DesiredService ='" + request.desiredService + "'";
            _cmd += ", @AdditionalDetail  ='" + request.additionalDetail + "'";
            _cmd += ", @FromApp  ='" + request.fromApp + "'";
              if (url != null)
            {
                _cmd += ", @FileUrl  ='" + string.Join(",", url) + "'";
            }
            else
            {
                _cmd += ", @FileUrl  =''";
            }


            DB.DBConn.ExecuteOnly(_cmd);

            return Ok(new { message = "Message sent" });
        }






        private async Task<List<string>> UploadFilesAsyn(List<IFormFile> formFiles)
        {
            if (formFiles == null || formFiles.Count == 0)
            {

                return null;
            }

            try
            {
                (string errorMessage, List<string> imageName) = await productService.UploadMultiFiles(formFiles);
                if (!String.IsNullOrEmpty(errorMessage))
                {

                    return null;
                }


                return imageName;
            }
            catch (Exception ex)
            {

                return null;
            }
        }




    }

}