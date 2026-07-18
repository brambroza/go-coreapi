using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace NohWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotController : ControllerBase
    {
        static bool mailSent = false;

        // GET: api/Forgot
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            string[] smtpHost = { "smtp.gmail.com" };
            Main(smtpHost);
            return Ok(new string[] { "value1", "value2" });
        }

        // GET: api/Forgot/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return Ok("value");
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine($"[{token}] Send canceled.");
            }
            if (e.Error != null)
            {
                Console.WriteLine($"[{token}] {e.Error}");
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("SMTP host is required.");
                return;
            }

            try
            {
                // Configure SMTP client
                SmtpClient client = new SmtpClient(args[0])
                {
                    Port = 587,
                    Credentials = new NetworkCredential("brambroza@gmail.com", "your-app-password"),
                    EnableSsl = true
                };

                // Configure email
                MailAddress from = new MailAddress("brambroza@gmail.com", "Admin N.", System.Text.Encoding.UTF8);
                MailAddress to = new MailAddress("amnart.k@wisdom-erp.com");
                MailMessage message = new MailMessage(from, to)
                {
                    Body = "This is a test email message sent by an application.",
                    BodyEncoding = System.Text.Encoding.UTF8,
                    Subject = "Test Message",
                    SubjectEncoding = System.Text.Encoding.UTF8
                };

                client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

                string userState = "test message1";
                client.SendAsync(message, userState);
                Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
                string answer = Console.ReadLine();

                if (answer.StartsWith("c") && mailSent == false)
                {
                    client.SendAsyncCancel();
                }

                message.Dispose();
                Console.WriteLine("Goodbye.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
} 
