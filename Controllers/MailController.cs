using System.Net.Http;
using System.Security.AccessControl;
using System;
using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MimeKit;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace goalongapi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MailController : ControllerBase
    {
        private readonly GmailServiceHelper _gmailService;

        public MailController(IWebHostEnvironment env)
        {
            _gmailService = new GmailServiceHelper(env);
        }

        [HttpGet("emails")]
        public async Task<ActionResult<IEnumerable<Message>>> GetEmails()
        {
            var emails = await _gmailService.GetEmailsAsync();
            return Ok(emails);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendToEmail([FromForm] string from,
            [FromForm] string to,
            [FromForm] string fullname,
            [FromForm] string subject,
            [FromForm] string body, [FromForm] List<IFormFile>? files)
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
                var fromAddress = new MailAddress(fromEmail, fullname);
                var message = new MailMessage(fromAddress, new MailAddress(to))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                // ✅ เพิ่ม BCC ส่งสำเนาให้ตัวเอง
                if (!string.IsNullOrEmpty(from))
                {
                    message.Bcc.Add(new MailAddress(from));
                }

                // ✅ แนบไฟล์ถ้ามีการอัปโหลด
                if (files != null && files.Count > 0)
                {
                    foreach (var formFile in files)
                    {
                        using var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        message.Attachments.Add(new Attachment(new MemoryStream(fileBytes), formFile.FileName));

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

        private async Task<MimeMessage> CreateMessageAsync(EmailRequest emailRequest, List<IFormFile> files)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Name", "your-email@example.com"));
            message.To.Add(new MailboxAddress("", emailRequest.To));
            message.Subject = emailRequest.Subject;

            // Create the body and attachment
            var body = new TextPart("plain")
            {
                Text = emailRequest.Body
            };

            var multipart = new Multipart("mixed") { body };

            if (files != null && files.Any())
            {
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        // ใช้ OpenReadStream() เพื่อเปิด stream โดยไม่ต้องใช้ MemoryStream
                        var stream = formFile.OpenReadStream();

                        var attachment = new MimeKit.MimePart(formFile.ContentType)
                        {
                            Content = new MimeKit.MimeContent(stream, MimeKit.ContentEncoding.Default),
                            ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                            ContentTransferEncoding = MimeKit.ContentEncoding.Base64,
                            FileName = formFile.FileName
                        };

                        multipart.Add(attachment);

                        // stream จะถูกปิดเมื่อ attachment ถูก dispose ถ้าจำเป็นต้องปิดเพิ่มเติมให้ใช้การจัดการที่เหมาะสม
                    }
                }
            }



            message.Body = multipart;

            return message;
        }



        [HttpGet("getmaildetail")]
        public async Task<ActionResult<IMail>> GetMailDetail([FromQuery] string mailId)
        {
            if (string.IsNullOrEmpty(mailId))
            {
                return BadRequest("Mail ID is required.");
            }

            var message = await _gmailService.GetMailByIdAsync(mailId); // Assuming you have a method in GmailServiceHelper to get mail by ID

            if (message == null)
            {
                return NotFound("No mail found with the specified ID.");
            }

            var mailDetail = new IMail
            {
                Id = message.Id,
                Folder = message.LabelIds[0], // You can customize this as needed
                Subject = message.Snippet, // Adjust for actual subject mapping
                Message = message.Snippet, // Use message body or other details as needed
                IsUnread = !message.LabelIds.Contains("READ"),
                From = new IMailSender
                {
                    Name = GetSenderName(message), // Replace with actual sender's name
                    Email = GetSenderEmail(message), // Replace with actual sender's email
                    AvatarUrl = null // Optional if available
                },
                To = message.Payload.Headers
                    .Where(h => h.Name == "To") // Look for 'To' header(s)
                    .SelectMany(h => h.Value.Split(',')) // In case of multiple recipients, split by comma
                    .Select(toAddress => new IMailSender
                    {
                        Name = ExtractNameFromAddress(toAddress), // Function to extract name if available
                        Email = ExtractEmailFromAddress(toAddress), // Function to extract email
                        AvatarUrl = null // Optionally add logic for avatars if available
                    })
                    .ToList(),
                LabelIds = message.LabelIds.ToList(),
                IsStarred = message.LabelIds.Contains("STARRED"),
                IsImportant = message.LabelIds.Contains("IMPORTANT"),
                CreatedAt = GetSentTimeFromHeaders(message), // Adjust as per Gmail's DateTime
                Attachments = new List<IMailAttachment>() // Map attachments if available
            };

            return Ok(mailDetail);
        }



        [HttpGet("labels")]
        public async Task<ActionResult<IEnumerable<Label>>> GetLabels()
        {
            var labels = await _gmailService.GetLabelsAsync();
            return Ok(labels);
        }
        [HttpGet("emailsWithLabel")]
        public async Task<ActionResult<IEnumerable<Message>>> GetEmailsWithLabel([FromQuery] string labelId)
        {
            if (string.IsNullOrEmpty(labelId))
            {
                return BadRequest("LabelId is required.");
            }

            var messages = await _gmailService.GetEmailsByLabelAsync(labelId);

            if (messages == null || messages.Count == 0)
            {
                return NotFound("No emails found with the specified label.");
            }

            var emails = messages.Select(message => new IMail
            {
                Id = message.Id,
                Folder = labelId, // You can customize this as needed
                Subject = message.Snippet, // Adjust for actual subject mapping
                Message = message.Snippet, // Use message body or other details as needed
                IsUnread = !message.LabelIds.Contains("READ"),
                From = new IMailSender
                {
                    Name = GetSenderName(message), // Replace with actual sender's name
                    Email = GetSenderEmail(message), // Replace with actual sender's email
                    AvatarUrl = null // Optional if available
                },
                To = message.Payload.Headers
                    .Where(h => h.Name == "To") // Look for 'To' header(s)
                    .SelectMany(h => h.Value.Split(',')) // In case of multiple recipients, split by comma
                    .Select(toAddress => new IMailSender
                    {
                        Name = ExtractNameFromAddress(toAddress), // Function to extract name if available
                        Email = ExtractEmailFromAddress(toAddress), // Function to extract email
                        AvatarUrl = null // Optionally add logic for avatars if available
                    })
                    .ToList(),
                LabelIds = message.LabelIds.ToList(),
                IsStarred = message.LabelIds.Contains("STARRED"),
                IsImportant = message.LabelIds.Contains("IMPORTANT"),
                CreatedAt = GetSentTimeFromHeaders(message), // Adjust as per Gmail's DateTime
                Attachments = new List<IMailAttachment>() // Map attachments if available
            });

            return Ok(emails);

        }

        String GetSentTimeFromHeaders(Message message)
        {
            System.Globalization.CultureInfo thaiCulture = new System.Globalization.CultureInfo("th-TH");
            thaiCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

            string format = "ddd, dd MMM yyyy HH:mm:ss";
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;


            var dateHeader = message.Payload.Headers.FirstOrDefault(h => h.Name == "Date");
            if (dateHeader != null)
            {


                string datevalue = dateHeader.Value;

                return datevalue;
                /*   if (DateTime.TryParseExact(dateHeader.Value, format, provider, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime sentTime))
                  {

                      return sentTime;
                  }
       */
            }
            return DateTime.MinValue.ToString();
        }


        string ExtractNameFromAddress(string address)
        {
            var match = Regex.Match(address, @"^(.*?)(<.*>)?$");
            return match.Success ? match.Groups[1].Value.Trim() : address;
        }

        string ExtractEmailFromAddress(string address)
        {
            var match = Regex.Match(address, @"<(.+?)>");
            return match.Success ? match.Groups[1].Value.Trim() : address;
        }

        string GetSenderEmail(Message message)
        {
            var fromHeader = message.Payload.Headers.FirstOrDefault(h => h.Name == "From");
            if (fromHeader != null)
            {
                var fromValue = fromHeader.Value;
                // ใช้ Regex เพื่อจับเฉพาะส่วนอีเมล
                var match = Regex.Match(fromValue, @"<(.+?)>");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim(); // ดึงเฉพาะส่วนอีเมลที่อยู่ภายใน <>
                }
                else
                {
                    // กรณีที่ไม่มี <> ล้อมรอบ สามารถใช้การตรวจสอบแบบทั่วไป
                    if (fromValue.Contains("@"))
                    {
                        return fromValue; // คืนค่าเต็มในกรณีไม่มี <> และมี @
                    }
                }
            }
            return "Unknown Email"; // ค่าพื้นฐานในกรณีที่ไม่พบอีเมล
        }

        string GetSenderName(Message message)
        {
            var fromHeader = message.Payload.Headers.FirstOrDefault(h => h.Name == "From");
            if (fromHeader != null)
            {

                var fromValue = fromHeader.Value;
                var match = Regex.Match(fromValue, @"^(.*?)(<.*>)?$");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim(); // ดึงเฉพาะส่วนชื่อ
                }
            }
            return "Unknown Sender"; // ค่าพื้นฐานในกรณีที่ไม่พบชื่อ
        }


    }



    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string Fullname { get; set; }
    }


    public class MailLabel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int? UnreadCount { get; set; }
    }

    public class IMailSender
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class IMailAttachment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }  // Use long for file sizes in bytes
        public string Type { get; set; }
        public string Path { get; set; }
        public string Preview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class IMail
    {
        public string Id { get; set; }
        public string Folder { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsUnread { get; set; }
        public IMailSender From { get; set; }
        public List<IMailSender> To { get; set; }
        public List<string> LabelIds { get; set; }
        public bool IsStarred { get; set; }
        public bool IsImportant { get; set; }
        public string CreatedAt { get; set; }
        public List<IMailAttachment> Attachments { get; set; }
    }

    public class MailsResponse

    {
        public Dictionary<string, IMail> ById { get; set; }
        public List<string> AllIds { get; set; }
    }

}