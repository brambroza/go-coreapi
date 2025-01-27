using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using goalongapi.Models;
using goalongapi.Datatools.Account;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {  

        private readonly IAccountService accountService;

        public AccountController(IAccountService accountService) =>
            this.accountService = accountService;

        [HttpPost("[action]")]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            var account = registerRequest.Adapt<Account>();
            await accountService.Register(account);
            var tokenregis = accountService.GenerateTokenRegister(account.Username);

            MailConfirm.main(account.Username, account.FullName, tokenregis, registerRequest.Url);

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[getAccountInfo] @CmpId='"
                + registerRequest.CmpId
                + "' , @User='"
                + registerRequest.Username
                + "'";
            dt = goalongapi.DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> ChangePassword(PasswordChange registerRequest)
        {
            var account = await accountService.Login(
                registerRequest.Username,
                registerRequest.OldPassword
            );
            if (account == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            await accountService.ChangePassword(
                registerRequest.Username,
                registerRequest.NewPassword
            );

            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> RegisterGoogle(RegisterGoogle registerRequestGoogle)
        {
            var account = registerRequestGoogle.Adapt<AccountGoogle>();
            await accountService.RegisterGoogle(account);
            var tokenregis = accountService.GenerateTokenRegister(account.Email);

            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> LoginGoogle(LoginRequestGoogle loginRequest)
        {
            var account = await accountService.LoginGoogle(loginRequest.Id, loginRequest.Email);
            if (account == null)
            {
                return Unauthorized();
            }

            return Ok(
                new
                {
                    token = accountService.GenerateTokenGoogle(account),
                    CmpId = account.CmpId,
                    imgurl = account.imgPath,
                }
            );
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            var account = await accountService.Login(loginRequest.Username, loginRequest.Password);
            if (account == null)
            {
                return Unauthorized();
            }

            return Ok(
                new
                {
                    token = accountService.GenerateToken(account),
                    CmpId = account.CmpId,
                    imgurl = account.imgPath,
                }
            );
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> LoginState(LoginState loginRequest)
        {
            var _cmd = "";
            _cmd = " Exec dbo.setLoginState @username='" + loginRequest.Username + "'";
            _cmd += " , @devicename='" + loginRequest.DeviceName + "'";
            _cmd += " , @ip ='" + loginRequest.Ip + "'";
            _cmd += " , @os ='" + loginRequest.OS + "'";

            goalongapi.DB.DBConn.ExecuteOnly(_cmd);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> TestConnect()
        {
            var connectionString =
                "Server=PCLDK\\PCLDKERP;Database=NSDBs;User ID=sa;Password=1234@pass;Encrypt=False;TrustServerCertificate=True;";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return Ok("Connection successful!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Info()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Ok("40441"); // Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            return Ok(new { username = account.Username, role = account.Role.Name });
        }

        [HttpGet("[action]/{token}")]
        public IActionResult ConfirmEmailEnjoy(string token)
        {
            // Verify the token and perform email confirmation
            Account account = accountService.GetInfo(token);

            if (account.Username != "")
            {
                if (accountService.UpdateConfirmEmail(account.Username))
                {
                    return Ok("Email confirmed successfully.");
                }
                else
                {
                    return BadRequest("Invalid token or email already confirmed.");
                }
            }
            else
            {
                return BadRequest("Invalid token or email already confirmed.");
            }
        }

        [HttpGet("[action]/{token}")]
        public IActionResult GetUserInfo(string token)
        {
            Account account = accountService.GetInfo(token);
            if (account.Username != "")
            {
                var _cmd = "";
                _cmd = "exec dbo.sp_getUserInfo @Username ='" + account.Username + "'";
                var dt = goalongapi.DB.DBConn.GetDataTable(_cmd);
                if (dt.Rows.Count <= 0)
                {
                    return BadRequest("Invalid token or email already confirmed.");
                }
                else
                {
                    return Ok(
                        new
                        {
                            fullname = dt.Rows[0]["fullname"].ToString(),
                            cmpname = dt.Rows[0]["cmpname"].ToString(),
                        }
                    );
                }
            }
            else
            {
                return BadRequest("Invalid token or email already confirmed.");
            }
        }

        [HttpGet("[action]/{email}")]
        public bool validateEmail(string email)
        {
            var res = accountService.validateEmails(email);
            return res;
        }

        [HttpPost("[action]")]
        public IActionResult setup(Company cmp)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_register_company";
                _cmd += " @cmpid  ='" + cmp.CmpId + "'";
                _cmd += " ,@CmpAddress  ='" + cmp.CmpAddress + "'";
                _cmd += " ,@CmpName  ='" + cmp.CmpName + "'";
                _cmd += " ,@Phone  ='" + cmp.Phone + "'";
                _cmd += " ,@fax  ='" + cmp.Fax + "'";
                _cmd += " ,@email  ='" + cmp.Email + "'";
                _cmd += " ,@teloffice  ='" + cmp.teloffice + "'";

                DataTable dt = goalongapi.DB.DBConn.GetDataTable(_cmd);

                return Ok(dt.Rows[0][0]);
            }
            catch
            {
                msgretrun.ReturnCode = "400";
                msgretrun.Msg = "Error !!";
                return Ok(msgretrun);
            }
        }

        [HttpGet("[action]")]
        public IActionResult getAccountInfo([FromQuery] string user, [FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getAccountInfo] @CmpId='" + cmpid + "' , @User='" + user + "'";
            dt = goalongapi.DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpGet("[action]")]
        public IActionResult getAccountInfoList([FromQuery] string user, [FromQuery] string cmpid)
        {
            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd = "exec dbo.[getAccountInfoList] @CmpId='" + cmpid + "' , @User='" + user + "'";
            dt = goalongapi.DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }

        [HttpPost("[action]")]
        public IActionResult setAccountInfo(UserAccouter user)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setAccountInfo";
                _cmd += " @AccountID  =" + user.AccountID + "";
                _cmd += " ,@CmpId  ='" + user.CmpId + "'";
                _cmd += " ,@FullName  ='" + user.FullName + "'";
                _cmd += " ,@Username  ='" + user.Username + "'";
                _cmd += " ,@imgPath  ='" + user.imgPath + "'";
                _cmd += " ,@SignaturePath  ='" + user.SignaturePath + "'";
                _cmd += " ,@LineQRCodePath  ='" + user.LineQRCodePath + "'";
                _cmd += " ,@MobileNo  ='" + user.MobileNo + "'";
                _cmd += " ,@LineId  ='" + user.LineId + "'";
                _cmd += " ,@Address  ='" + user.Address + "'";
                _cmd += " ,@AddrProvince  ='" + user.AddrProvince + "'";
                _cmd += " ,@AddrDistrict  ='" + user.AddrDistrict + "'";
                _cmd += " ,@AddrSubDistrict  ='" + user.AddrSubDistrict + "'";
                _cmd += " ,@AddrPostCode  ='" + user.AddrPostCode + "'";
                _cmd += " ,@RoleID  =" + user.RoleID + "";

                if (goalongapi.DB.DBConn.ExecuteOnly(_cmd))
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
        public IActionResult delAccountInfo(UserAccouter user)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.setDeleteAccountInfo";
                _cmd += " @AccountID  =" + user.AccountID + "";
                _cmd += " ,@CmpId  ='" + user.CmpId + "'";

                if (goalongapi.DB.DBConn.ExecuteOnly(_cmd))
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
        public IActionResult setUpMapUser(MapUser cmp)
        {
            MsgReturn msgretrun = new MsgReturn();
            try
            {
                string _cmd = "";
                _cmd = "exec  dbo.sp_mapregister_user";
                _cmd += " @cmpid  ='" + cmp.cmpid + "'";
                _cmd += " ,@email  ='" + cmp.email + "'";

                if (goalongapi.DB.DBConn.ExecuteOnly(_cmd))
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
        public IActionResult testSendmail(
            string mailfrom,
            string mailto,
            string smtphost,
            string smtpuser,
            string smtppass,
            string subject,
            string msg
        )
        {
            MailConfirm.testmail(mailfrom, mailto, smtphost, smtpuser, smtppass, subject, msg);
            return StatusCode((int)HttpStatusCode.Created);
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

    public class MailConfirm
    {
        public static void main(string emailto, string fullname, string token, string url)
        {
            // SMTP settings for Gmail
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUsername = "amnart.gl@gmail.com";
            var smtpPassword = "zsdwjtbgnouxrnvb";

            // Sender and recipient email addresses
            var fromEmail = "info@goalong.co";
            var toEmail = emailto;

            // Create a new SMTP client
            var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            try
            {
                // Create a new email message
                var message = new MailMessage(fromEmail, toEmail);
                message.Subject = "Welcome to GoAlong System!";
                message.Body = mailbody(fullname, url + "/#/confirmemail?id=" + token);
                message.IsBodyHtml = true;

                // Send the email
                smtpClient.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email: " + ex.Message);
            }
        }

        public static void testmail(
            string emailfrom,
            string emailto,
            string smtphost,
            string smtpuser,
            string smtppass,
            string subject,
            string msg
        )
        {
            // SMTP settings for Gmail
            var smtpHost = smtphost;
            var smtpPort = 587;
            var smtpUsername = smtpuser;
            var smtpPassword = smtppass;

            // Sender and recipient email addresses
            var fromEmail = emailfrom;
            var toEmail = emailto;

            // Create a new SMTP client
            var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            try
            {
                // Create a new email message
                var message = new MailMessage(fromEmail, toEmail);
                message.Subject = subject;
                message.Body = mailbodyTest(msg);
                message.IsBodyHtml = true;

                // Send the email
                smtpClient.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email: " + ex.Message);
            }
        }

        public static string mailbodyTest(string msg)
        {
            var _str = "";
            _str = @"<!DOCTYPE html>  <html> <head> <style>";
            _str += " body {  font-family: Arial, sans-serif;   background-color: #F1F9F4;  }";
            _str += " .container {";
            _str += "   max-width: 600px;";
            _str += "   margin: 0 auto;";
            _str += "   padding: 20px;";
            _str += "   background-color: #FFFFFF;";
            _str += "  border-radius: 5px;";
            _str += "   box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);";
            _str += "  }";
            _str += " h1 {";
            _str += "     color: #2E8B57;";
            _str += "    margin-bottom: 20px;";
            _str += " }";
            _str += " p {";
            _str += " color: #333333;";
            _str += "  line-height: 1.6;";
            _str += "}";
            _str += " .button {";
            _str += "    display: inline-block;";
            _str += "    padding: 10px 20px;";
            _str += "  background-color: #2E8B57;";
            _str += "  color: #FFFFFF;";
            _str += "  text-decoration: none;";
            _str += "  border-radius: 5px;";
            _str += " }";
            _str += ".button:hover {";
            _str += "    background-color: #228B22;";
            _str += " }";
            _str += "</style>";
            _str += "</head>";
            _str += "<body>";
            _str += " <div class=\"container\">";
            _str += "  <h1>Welcome  System!</h1>";
            _str += "  <p>" + msg + "</p>";

            _str += "</div>";
            _str += "</body>";
            _str += "</html>";

            return _str;
        }

        public static string mailbody(string toname, string linkconfirm)
        {
            var _str = "";
            _str = @"<!DOCTYPE html>  <html> <head> <style>";
            _str += " body {  font-family: Arial, sans-serif;   background-color: #F1F9F4;  }";
            _str += " .container {";
            _str += "   max-width: 600px;";
            _str += "   margin: 0 auto;";
            _str += "   padding: 20px;";
            _str += "   background-color: #FFFFFF;";
            _str += "  border-radius: 5px;";
            _str += "   box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);";
            _str += "  }";
            _str += " h1 {";
            _str += "     color: #2E8B57;";
            _str += "    margin-bottom: 20px;";
            _str += " }";
            _str += " p {";
            _str += " color: #333333;";
            _str += "  line-height: 1.6;";
            _str += "}";
            _str += " .button {";
            _str += "    display: inline-block;";
            _str += "    padding: 10px 20px;";
            _str += "  background-color: #2E8B57;";
            _str += "  color: #FFFFFF;";
            _str += "  text-decoration: none;";
            _str += "  border-radius: 5px;";
            _str += " }";
            _str += ".button:hover {";
            _str += "    background-color: #228B22;";
            _str += " }";
            _str += "</style>";
            _str += "</head>";
            _str += "<body>";
            _str += " <div class=\"container\">";
            _str += "  <h1>Welcome to GoAlong System!</h1>";
            _str += "  <p>Dear " + toname + ",</p>";
            _str +=
                "  <p>Thank you for choosing GoAlong System for your registration. We are excited to have you on board! To get started and unlock all the amazing features, we kindly request you to verify your email address.</p>";
            _str +=
                "  <p><strong>To proceed with email verification, please click the button below:</strong></p>";
            _str += " <p><a class=\"button\" href=\"" + linkconfirm + "\">Verify Email</a></p>";
            _str +=
                " <p>By verifying your email, you will embark on an incredible journey with GoAlong System, where you can connect with like-minded individuals, discover exciting events, and make lasting memories.</p>";
            _str +=
                " <p>If you have any questions or need assistance during the registration process, our dedicated support team is ready to help. Feel free to reach out to us at support team support@goalong.co.</p>";
            _str +=
                " <p>Thank you for joining the GoAlong System community! Let's illuminate the path to unforgettable experiences together!</p>";
            _str += " <p>Best regards,</p>";
            _str += " <p>Go Along Support Team <br> Goalong ltd <br> 085-608-3298</p>";
            _str += "</div>";
            _str += "</body>";
            _str += "</html>";

            return _str;
        }
    }
}
