using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        [HttpPost("sendBase64")]

        public async Task<IActionResult> SendToEmail([FromForm] EmailRequest request,
        [FromForm] List<IFormFile>? files)
        {
            try
            {
                var smtpHost = "smtp-relay.gmail.com"; // ใช้ SMTP Relay
                var smtpPort = 587;
                var fromEmail = "info@goalong.co.th"; // ต้องเป็นอีเมลที่ได้รับอนุญาต
                var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };

                // ตั้งค่าผู้ส่งและผู้รับ
                var fromAddress = new MailAddress(fromEmail, request.Fullname);
                var message = new MailMessage(fromAddress, new MailAddress(request.To))
                {
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = true
                };

                // ✅ เพิ่ม BCC ส่งสำเนาให้ตัวเอง
                if (!string.IsNullOrEmpty(request.From))
                {
                    message.CC.Add(new MailAddress(request.From));
                }


                // ✅ แปลง Base64 หลายไฟล์กลับเป็นไฟล์แนบ
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        message.Attachments.Add(new Attachment(new MemoryStream(memoryStream.ToArray()), file.FileName));
                    }
                }

                // ✅ ส่งอีเมล
                smtpClient.Send(message);

                return Ok("✅ Email sent successfully with attachment.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "❌ Failed to send email: " + ex.Message);
            }
        }
    }


}