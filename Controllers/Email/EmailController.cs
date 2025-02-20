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
    [Authorize]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        [HttpPost("sendBase64")]

        public async Task<IActionResult> SendToEmail([FromBody] EmailRequest request)
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
                    message.Bcc.Add(new MailAddress(request.From));
                }


                // ✅ แปลง Base64 หลายไฟล์กลับเป็นไฟล์แนบ
                if (request.Files != null && request.Files.Count > 0)
                {
                    foreach (var file in request.Files)
                    {
                        if (!string.IsNullOrEmpty(file.FileData))
                        {
                            byte[] fileBytes = Convert.FromBase64String(file.FileData);
                            var memoryStream = new MemoryStream(fileBytes);
                            message.Attachments.Add(new Attachment(memoryStream, file.Filename));
                        }
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