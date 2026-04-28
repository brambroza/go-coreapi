using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using goalongapi.Models;
using goalongapi.Models.Trial;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using goalongapi.Data;
using System.Security.Claims;
using System.Security.Cryptography;


namespace goalongapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {

        private readonly IAccountService accountService;
        private readonly DatabaseContext databaseContext;
        private readonly EmailSettingRepository _repo;
        private readonly AesCrypto _crypto;
 


        public AccountController(IAccountService accountService, DatabaseContext databaseContext, EmailSettingRepository repo, AesCrypto crypto)
        {
            this.accountService = accountService;
            this.databaseContext = databaseContext;
            this._repo = repo;
            this._crypto = crypto; 
        }



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
        public async Task<ActionResult> RegisterFormMobile(RegisterFromMobileRequest registerRequest)
        {
            var account = registerRequest.Adapt<Account>();
            await accountService.Register(account);
            var tokenregis = accountService.GenerateTokenRegister(account.Username);

            /*  MailConfirm.main(account.Username, account.FullName, tokenregis, registerRequest.Url); */

            DataTable dt = new System.Data.DataTable();
            string _cmd;
            _cmd =
                "exec dbo.[setNewAccountFormMobile] @Email='"
                + registerRequest.Username
                + "' , @FullName='"
                + registerRequest.FullName
                + "'";

            _cmd += " , @TaxId='" + registerRequest.TaxId + "'";
            _cmd += " , @Tel ='" + registerRequest.Tel + "'";
            _cmd += " , @CompanyName='" + registerRequest.CompanyName + "'";
            _cmd += " , @CompanyAddress='" + registerRequest.CompanyAddress + "'";
            dt = goalongapi.DB.DBConn.GetDataTable(_cmd);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Ok(JSONString);
        }




        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword request)
        {

            try
            {
                var account = await accountService.ForgotPassword(request.Username);

                var encodedToken = WebUtility.UrlEncode(account.ResetToken);

                var resetLink = $"{request.Url}/auth/go-portal/reset-password?token={encodedToken}";
                MailConfirm.ReplyEmail(account.Username, account.FullName, resetLink);


                return Ok(new { message = "Reset token generated. Check your email.", token = account.ResetToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {

            var account = await accountService.ResetPassword(request.ResetToken, request.NewPassword);
            if (!account)
            {
                return BadRequest();
            }
            return Ok("Password reset successful.");
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

        /*   [HttpPost("[action]")]
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
                      refreshToken = accountService.GenerateRefreshToken(account),
                      CmpId = account.CmpId,
                      imgurl = account.imgPath,
                  }
              );
          } */


        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            var account = await accountService.Login(loginRequest.Username, loginRequest.Password);
            if (account == null) return Unauthorized();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            var result = await accountService.IssueSessionTokens(
                account,
                loginRequest.DeviceId,
                loginRequest.DeviceName,
                ua,
                ip,
                loginRequest.Force
            );

            if (result.Status == "ALREADY_LOGGED_IN")
            {
                return Conflict(new
                {
                    code = "ALREADY_LOGGED_IN",
                    activeSession = result.ActiveSession,
                    canForce = true
                });
            }

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                sessionId = result.SessionId,
                CmpId = account.CmpId,
                imgurl = account.imgPath,
                userId = account.AccountId
            });
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var sidStr = User.FindFirst("sid")?.Value;

            // account id หาได้หลาย key (เลือกที่ token คุณมีจริง)
            var aidStr =
                User.FindFirst("aid")?.Value ??
                User.FindFirst("AccountID")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(sidStr))
                return Ok(); // ไม่มี sid ก็ถือว่า logout สำเร็จในเชิง client

            if (!Guid.TryParse(sidStr, out var sid))
                return Ok();

            // ปิด session ใน DB
            var session = await databaseContext.AccountSessions
                .SingleOrDefaultAsync(x => x.SessionId == sid && x.IsActive);

            if (session != null)
            {
                // (optional) เช็ค aid ให้ตรงกัน
                if (long.TryParse(aidStr, out var aid) && session.AccountID != aid)
                    return Forbid();

                session.IsActive = false;
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = "LOGOUT";
                await databaseContext.SaveChangesAsync();
            }

            return Ok();
        }




        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel model)
        {
            Account account = await accountService.GetAccount(model.AccessToken);
            if (account == null) return Unauthorized();

            if (account.refreshToken != model.RefreshToken || account.refreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized();

            var newAccessToken = accountService.GenerateToken(account);
            var newRefreshToken = accountService.GenerateRefreshToken(account);

            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken,
                CmpId = account.CmpId,
                imgurl = account.imgPath
            });
        }





        [HttpPost("validate")]
        public IActionResult ValidateToken([FromHeader] string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization) || !Authorization.StartsWith("Bearer "))
                return Unauthorized(new { message = "Token is missing or invalid" });

            var token = Authorization.Substring(7);

            if (accountService.ValidateToken(token, out SecurityToken validatedToken))
                return Ok(new { message = "Token is valid" });

            return Unauthorized(new { message = "Token is invalid" });
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


        [HttpGet("emailsettings")]
        public async Task<IActionResult> GetEmailSettings([FromQuery] string? cmpId, [FromQuery] string settingName = "default")
        {
            var setting = await _repo.GetActiveAsync(cmpId, settingName);
            if (setting == null) return NotFound(new { ok = false, message = "Email setting not found." });

            var view = new EmailSmtpSettingViewDto
            {
                CmpId = setting.CmpId,
                SettingName = setting.SettingName,
                FromEmail = setting.FromEmail,
                FromName = setting.FromName,
                SmtpHost = setting.SmtpHost,
                SmtpPort = setting.SmtpPort,
                EnableSsl = setting.EnableSsl,
                Username = setting.Username,
                UpdatedAt = setting.UpdatedAt,
                IsActive = setting.IsActive,

            };

            return Ok(new { ok = true, data = view });
        }


        // /api/settings/emailsettings
        [HttpPost("emailsettings")]
        public async Task<IActionResult> UpsertEmailSettings([FromBody] EmailSmtpSettingUpsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FromEmail))
                return BadRequest(new { ok = false, message = "FromEmail is required." });

            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest(new { ok = false, message = "Username is required." });

            var updatePassword = !string.IsNullOrWhiteSpace(dto.AppPasswordPlain);

            // ถ้ามี password ก็เข้ารหัส
            byte[] cipher = Array.Empty<byte>();
            byte[] iv = Array.Empty<byte>();

            if (updatePassword)
            {
                (cipher, iv) = _crypto.Encrypt(dto.AppPasswordPlain!.Trim().Replace(" ", ""));
            }

            var entity = new EmailSmtpSetting
            {
                CmpId = dto.CmpId,
                SettingName = string.IsNullOrWhiteSpace(dto.SettingName) ? "default" : dto.SettingName.Trim(),
                FromEmail = dto.FromEmail.Trim(),
                FromName = dto.FromName?.Trim(),
                SmtpHost = dto.SmtpHost.Trim(),
                SmtpPort = dto.SmtpPort,
                EnableSsl = dto.EnableSsl,
                Username = dto.Username.Trim(),
                IsActive = true
            };

            // ใส่ password เฉพาะตอนมีการอัปเดต
            if (updatePassword)
            {
                entity.PasswordEnc = cipher;
                entity.PasswordIv = iv;
            }

            try
            {
                await _repo.UpsertAsync(entity, updatePassword);
                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, message = ex.Message });
            }
        }


        [HttpPost("testsendmailsmtp")]
        public async Task<IActionResult> TestSendmailSmtp([FromBody] ForgotPassword request)
        {

            try
            {
                var account = await accountService.ForgotPassword(request.Username);

                var encodedToken = WebUtility.UrlEncode(account.ResetToken);

                var resetLink = $"{request.Url}/auth/go-portal/reset-password?token={encodedToken}";
                MailConfirm.ReplyEmailAsync(_repo, _crypto, account.Username, account.FullName, resetLink , "230015");


                return Ok(new { message = "Reset token generated. Check your email.", token = account.ResetToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

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

        private readonly EmailSettingRepository _repo;
        private readonly AesCrypto _crypto;

        public MailConfirm(EmailSettingRepository repo, AesCrypto crypto)
        {
            _repo = repo;
            _crypto = crypto;
        }


        public static void SendEmail(string from, string toEmail, string fullname, string subject, string body)
        {
            var smtpHost = "smtp-relay.gmail.com"; // ใช้ SMTP Relay
            var smtpPort = 587;
            var fromEmail = "info@goalong.co.th"; // อีเมลที่ได้รับอนุญาตให้ส่งผ่าน SMTP Relay

            var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
            };

            try
            {
                var fromAddress = new MailAddress(fromEmail, fullname);
                var message = new MailMessage(fromAddress, new MailAddress(toEmail));

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                message.Bcc.Add(new MailAddress(from));

                smtpClient.Send(message);
                Console.WriteLine("✅ Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed to send email: " + ex.Message);
            }
        }


        public static void main(string emailto, string fullname, string token, string url)
        {
            var smtpHost = "smtp-relay.gmail.com"; // ใช้ SMTP Relay
            var smtpPort = 587;
            var fromEmail = "info@goalong.co.th"; // อีเมลที่ได้รับอนุญาตให้ส่งผ่าน SMTP Relay

            var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                //  Credentials = new NetworkCredential(fromEmail, "your-app-password") // ใช้ App Password ถ้าต้องการ
            };

            try
            {
                var fromAddress = new MailAddress(fromEmail, "Go Along Support");
                var message = new MailMessage(fromAddress, new MailAddress(emailto));
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

        public static void ReplyEmail(string toEmail, string fullname, string url)
        {
            var smtpHost = "smtp-relay.gmail.com"; // ใช้ SMTP Relay
            var smtpPort = 587;
            var fromEmail = "info@goalong.co.th"; // อีเมลที่ได้รับอนุญาตให้ส่งผ่าน SMTP Relay
            // var appPassword = "your-app-password"; // ใช้ App Password ถ้าต้องการ

            var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                /*    Credentials = new NetworkCredential(fromEmail, appPassword) ,
                   DeliveryMethod = SmtpDeliveryMethod.Network,
                   Timeout = 100000 */

            };

            try
            {
                var fromAddress = new MailAddress(fromEmail, "Go Along Support");
                var message = new MailMessage(fromAddress, new MailAddress(toEmail));

                message.Subject = " Go Along System Reset Password";
                message.Body = mailbodyReset(fullname, url);
                message.IsBodyHtml = true;

                message.Bcc.Add(new MailAddress(fromEmail));

                smtpClient.Send(message);
                Console.WriteLine("✅ Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed to send email: " + ex.Message);
            }
        }


        public static async Task ReplyEmailAsync(
        EmailSettingRepository repo,
        AesCrypto crypto,string toEmail, string fullname, string url, string? cmpId = null)
        {
            var setting = await repo.GetActiveAsync(cmpId, "nis");
            if (object.ReferenceEquals(setting, null))
                throw new Exception("Email SMTP Setting not found.");

            var appPass = crypto.Decrypt(setting.PasswordEnc, setting.PasswordIv);

            using var smtpClient = new SmtpClient(setting.SmtpHost, setting.SmtpPort)
            {
                EnableSsl = setting.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(setting.Username, appPass),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 100000
            };

            var fromAddress = new MailAddress(setting.FromEmail, setting.FromName ?? "Support");
            using var message = new MailMessage(fromAddress, new MailAddress(toEmail))
            {
                Subject = "System Reset Password",
                Body = mailbodyReset(fullname, url),
                IsBodyHtml = true
            };

            message.Bcc.Add(new MailAddress("brambroza@gmail.com"));

            smtpClient.Send(message);
        }

        public static void ResetPassword(string emailto, string fullname, string url)
        {
            // SMTP settings for Gmail
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUsername = "brambroza@gmail.com";
            var smtpPassword = "berz vwob qnfe uedt";

            // Sender and recipient email addresses
            var fromEmail = "info@goalong.co.th";
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
                message.Subject = " GoAlong System Reset Password";
                message.Body = mailbodyReset(fullname, url);
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



        public static string mailbodyReset(string toname, string linkconfirm)
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
            _str += "  background-color:rgb(255, 3, 3);";
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
            _str += "  <h1>Reset Your Password</h1>";
            _str += "  <p>Hi " + toname + ",We received a request to reset your password! </p>";
            _str +=
                "  <p>Use the link below to set a new password for your account. If you did not request to reset your password, ignore this email and the link will expire on its own.</p>";
            _str += " <p><a class=\"button\" href=\"" + linkconfirm + "\">SET NEW PASSWORD</a></p>";
            _str +=
                " <p>Link is valid until </p>";

            _str += "</body>";
            _str += "</html>";

            return _str;
        }


    }
}
