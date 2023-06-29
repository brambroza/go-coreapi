using System.Net;
using goalongapi.Datatools.Account;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

 


namespace goalongapi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class AccountController : ControllerBase
    {
        static bool mailSent = false;
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService) => this.accountService = accountService;

        [HttpPost("[action]")]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            var account = registerRequest.Adapt<Account>();
            await accountService.Register(account);
            return StatusCode((int)HttpStatusCode.Created);
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            var account = await accountService.Login(loginRequest.Username, loginRequest.Password);
            if (account == null)
            {
                return Unauthorized();
            }

            return Ok(new { token = accountService.GenerateToken(account) });

        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Info()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            return Ok(new
            {
                username = account.Username,
                role = account.Role.Name
            });
        }

         



       /*  public void SendEmail()
        {
            // Create a new MailMessage object
            MailMessage mail = new MailMessage();

            // Set the sender and recipient email addresses
            mail.From = new MailAddress("info@go-along.co");
            mail.To.Add("brambroza@gmail.com");

            // Set the subject and body of the email message
            mail.Subject = "Test email";
            mail.Body = "This is a test email.";

            // Create a new SmtpClient object and set its properties
            SmtpClient smtpClient = new SmtpClient("smtp.example.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("username", "password");
            smtpClient.EnableSsl = true;

            // Send the email message
            smtpClient.Send(mail);
        }

 */

    }
}