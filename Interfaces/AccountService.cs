using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using goalongapi.Data;
using goalongapi.Entities;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static goalongapi.Installers.JwtInstaller;
using goalongapi.Models;
using goalongapi.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace goalongapi.Interfaces
{
    public class AccountService : IAccountService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSettings;
        private readonly IHubContext<SessionHub> hub;

        public AccountService(DatabaseContext databaseContext, JwtSettings jwtSettings, IHubContext<SessionHub> hub)
        {
            this.jwtSettings = jwtSettings;
            this.databaseContext = databaseContext;
            this.hub = hub;
        }

        public async Task Register(Account account)
        {
            var existingAccount = await databaseContext.Accounts.SingleOrDefaultAsync(a =>
                a.Username == account.Username
            );
            if (existingAccount != null)
            {
                throw new Exception("Existing Account");
            }

            account.Password = CreatePasswordHash(account.Password);
            databaseContext.Accounts.Add(account);
            await databaseContext.SaveChangesAsync();
        }

        public async Task ChangePassword(string username, string newPassword)
        {
            var account = await databaseContext.Accounts.SingleOrDefaultAsync(a =>
                a.Username == username
            );

            if (account == null)
            {
                throw new Exception("Account not found");
            }
            // Hash and update the new password
            account.Password = CreatePasswordHash(newPassword);
            databaseContext.Accounts.Update(account);
            await databaseContext.SaveChangesAsync();
        }

        public async Task RegisterGoogle(AccountGoogle account)
        {
            var existingAccount = await databaseContext.AccountsGoogle.SingleOrDefaultAsync(a =>
                a.Email == account.Email
            );
            if (existingAccount != null)
            {
                throw new Exception("Existing Account");
            }

            databaseContext.AccountsGoogle.Add(account);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<Account?> Login(string username, string password)
        {
            /* var account = await databaseContext.Accounts.Include(a => a.Role)
            .SingleOrDefaultAsync(a => a.Username == username); */




            var account = await databaseContext
                .Accounts.Include(a => a.Role)
                .SingleOrDefaultAsync(a =>
                    a.Username == username
                    && a.CmpId != null
                    && a.CmpId != "0"
                    && a.stateEmailConfirm == 1
                ); //

            if (account != null && VerifyPassword(account.Password, password))
            {
                return account;
            }


            return null;
        }




        public async Task<Account?> LoginNewUser(string username, string password)
        {
            /* var account = await databaseContext.Accounts.Include(a => a.Role)
            .SingleOrDefaultAsync(a => a.Username == username); */




            var account = await databaseContext
                .Accounts.Include(a => a.Role)
                .SingleOrDefaultAsync(a =>
                    a.Username == username
                    && a.CmpId != null
                    && a.CmpId != "0"
                    && a.stateEmailConfirm == 1
                ); //

            if (account != null && VerifyPassword(account.Password, password))
            {
                return account;
            }

            return null;
        }

        public async Task<AccountGoogle?> LoginGoogle(long Id, string Email)
        {
            var account = await databaseContext
                .AccountsGoogle.Include(a => a.Role)
                .SingleOrDefaultAsync(a =>
                    a.Id == Id && a.Email == Email && a.CmpId != null && a.CmpId != "0"
                );

            return account;
        }

        private string CreatePasswordHash(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 258 / 8
                )
            );

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            var parts = hashedPassword.Split('.', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var passwordHashed = parts[1];

            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 258 / 8
                )
            );

            return passwordHashed == hashed;
        }

        public string GenerateToken(Account account)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim("role", account.Role.Name),
                new Claim("additional", "todo"),
            };

            return BuildToken(claims);
        }

        public async Task<string> GenerateNisTokenAsync(Account account)
        {
            var role = await GetNisRoleAsync(account.AccountId);
            return BuildToken([
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim("role", role),
                new Claim("additional", "todo"),
            ]);
        }


        private string GenerateTokenSession(Account account, AccountSession session, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                // NIS role is derived from SystemRole.StateManager via
                // SystemPermission, not from the display name of the role.
                new Claim("role", role),
                new Claim("additional", "todo"),
                new Claim("aid", account.AccountId.ToString()),
                new Claim("sid", session.SessionId.ToString()),
            };

            return BuildToken(claims);
        }

        private async Task<string> GetNisRoleAsync(long accountId)
        {
            const string sql = @"
SELECT CASE WHEN MAX(CASE WHEN ISNULL(role.StateManager, 0) = 1 THEN 1 ELSE 0 END) = 1
            THEN 'mng' ELSE 'staff' END
FROM Accounts AS userlist
LEFT JOIN SystemPermission AS per ON userlist.AccountID = per.AccountID
LEFT JOIN SystemRole AS role ON per.RoleId = role.RoleId
WHERE userlist.AccountID = @AccountId;";

            var connection = databaseContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@AccountId";
                parameter.Value = accountId;
                command.Parameters.Add(parameter);
                var value = await command.ExecuteScalarAsync();
                return string.Equals(value?.ToString(), "mng", StringComparison.OrdinalIgnoreCase) ? "mng" : "staff";
            }
            finally
            {
                if (shouldClose) await connection.CloseAsync();
            }
        }


        public string GenerateRefreshToken(Account account)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            account.refreshToken = refreshToken;
            account.refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            databaseContext.Entry(account).State = EntityState.Modified;
            databaseContext.SaveChanges();
            return refreshToken;

        }



        public string GenerateTokenGoogle(AccountGoogle account)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Email),
                new Claim("role", account.Role.Name),
                new Claim("additional", "todo"),
            };
            return BuildToken(claims);
        }

        public string GenerateTokenRegister(string Username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Username),
                new Claim("role", "admin"),
                new Claim("additional", "todo"),
            };
            return BuildToken(claims);
        }

        public bool UpdateConfirmEmail(string Username)
        {
            var account = databaseContext.Accounts.FirstOrDefault(a => a.Username == Username);
            if (account != null)
            {
                account.stateEmailConfirm = 1;
                databaseContext.Entry(account).State = EntityState.Modified;
                databaseContext.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Account> ForgotPassword(string Username)
        {
            var account = databaseContext.Accounts.FirstOrDefault(a => a.Username == Username);
            if (account == null)
                throw new Exception("User not found");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            account.ResetToken = token;
            account.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await databaseContext.SaveChangesAsync();

            return account;
        }

        public async Task<bool> ResetPassword(string token, string newPassword)
        {
            var account = await databaseContext.Accounts.FirstOrDefaultAsync(a => a.ResetToken == token);
            if (account == null || account.ResetTokenExpiry < DateTime.UtcNow)
                throw new Exception("Invalid or expired token");

            // Hash the new password (use BCrypt or another hashing algorithm)

            account.ResetToken = null;
            account.ResetTokenExpiry = null;
            account.Password = CreatePasswordHash(newPassword);
            databaseContext.Accounts.Update(account);
            await databaseContext.SaveChangesAsync();

            return true;
        }

        public async Task<Account?> GetAccount(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);

            var username = token.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(username)) return null;

            var account = await databaseContext.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Username == username);

            return account;
        }


        public Account GetInfo(string accessToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var username = token
                .Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub)
                .Value;
            var role = token.Claims.First(claim => claim.Type == "role").Value;

            var account = new Account
            {
                Username = username,
                Role = new Role { Name = role },

            };

            return account;
        }

        public bool validateEmails(string Username)
        {
            var account = databaseContext.Accounts.FirstOrDefault(a => a.Username == Username);
            if (account == null)
            {
                return false;
            }
            return true;
        }

        public bool removeUser(string Username)
        {
            var account = databaseContext.Accounts.FirstOrDefault(a => a.Username == Username);
            if (account != null)
            {
                databaseContext.Remove(account);
                databaseContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }



        private string BuildToken(Claim[] claims)
        {
            var expires = DateTime.Now.AddDays(Convert.ToDouble(jwtSettings.Expire));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token, out SecurityToken validatedToken)
        {
            validatedToken = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] HashToken(string token)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(token));
        }



        public async Task<IssueTokenResult> IssueSessionTokens(
        Account account,
        string deviceId,
        string? deviceName,
        string? userAgent,
        string? ipAddress,
        bool force
        )
        {
            // หา active session เดิม
            var active = await databaseContext.AccountSessions
                .Where(x => x.AccountID == account.AccountId && x.IsActive)
                .SingleOrDefaultAsync();

            var thaiTz = TimeZoneInfo.FindSystemTimeZoneById(
                      OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok"
                  );

            var atThai = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thaiTz);

            if (active != null && !force)
            {
                // แจ้งเครื่องเดิมว่า "มีการพยายาม login"
                await hub.Clients.Group($"session:{active.SessionId}")
                    .SendAsync("login_attempted", new
                    {
                        at = atThai,
                        fromDeviceName = deviceName,
                        fromIp = ipAddress,
                        fromUserAgent = userAgent
                    });

                return new IssueTokenResult
                {
                    Status = "ALREADY_LOGGED_IN",
                    ActiveSession = new
                    {
                        deviceName = active.DeviceName,
                        ipAddress = active.IpAddress,
                        lastSeenAt = active.LastSeenAt
                    }
                };
            }

            // force takeover: revoke session เดิม
            if (active != null && force)
            {
                active.IsActive = false;
                active.RevokedAt = atThai;
                active.RevokedReason = "REPLACED";
            }

            var newSessionId = Guid.NewGuid();

            // refresh token ใหม่ + เก็บ hash
            var refreshToken = GenerateRefreshToken(account);






            var session = new AccountSession
            {
                SessionId = newSessionId,
                AccountID = account.AccountId,
                DeviceId = deviceId,
                DeviceName = deviceName,
                UserAgent = userAgent,
                IpAddress = ipAddress,

                CreatedAt = atThai,
                LastSeenAt = atThai,

                ExpiresAt = atThai.AddDays(7),
                RefreshTokenHash = HashToken(refreshToken),
                RefreshTokenExpiry = atThai.AddDays(30),

                IsActive = true
            };

            databaseContext.AccountSessions.Add(session);
            await databaseContext.SaveChangesAsync();

            // ถ้า force takeover แจ้งเครื่องเดิมว่าโดน sign out
            if (active != null && force)
            {
                active.ReplacedBySessionId = newSessionId;
                await databaseContext.SaveChangesAsync();

                await hub.Clients.Group($"session:{active.SessionId}")
                    .SendAsync("session_revoked", new
                    {
                        reason = "REPLACED",
                        byDeviceName = deviceName,
                        at = atThai
                    });
            }

            // ออก JWT ที่มี sid=sessionId
            var role = await GetNisRoleAsync(account.AccountId);
            var token = GenerateTokenSession(account, session, role);

            return new IssueTokenResult
            {
                Status = "OK",
                Token = token,
                RefreshToken = refreshToken,
                SessionId = newSessionId,
                Role = role,
            };
        }




    }
}
