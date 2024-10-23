using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using goalongapi.Data;
using goalongapi.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using static goalongapi.Installers.JwtInstaller;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.CookiePolicy;

namespace goalongapi.Interfaces
{
    public class AccountService : IAccountService
    {
        private readonly DatabaseContext databaseContext;
        private readonly JwtSettings jwtSettings;
        public AccountService(DatabaseContext databaseContext, JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
            this.databaseContext = databaseContext;
        }

        public async Task Register(Account account)
        {
            var existingAccount = await databaseContext.Accounts.SingleOrDefaultAsync(a => a.Username == account.Username);
            if (existingAccount != null)
            {
                throw new Exception("Existing Account");
            }

            account.Password = CreatePasswordHash(account.Password);
            databaseContext.Accounts.Add(account);
            await databaseContext.SaveChangesAsync();
        }

        public async Task ChangePassword(string username,  string newPassword)
        {
            var account = await databaseContext.Accounts
                .SingleOrDefaultAsync(a => a.Username == username);

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
            var existingAccount = await databaseContext.AccountsGoogle.SingleOrDefaultAsync(a => a.Email == account.Email);
            if (existingAccount != null)
            {
                throw new Exception("Existing Account");
            }


            databaseContext.AccountsGoogle.Add(account);
            await databaseContext.SaveChangesAsync();
        }


        public async Task<Account?> Login(string username,
                                          string password)
        {
            /* var account = await databaseContext.Accounts.Include(a => a.Role)
            .SingleOrDefaultAsync(a => a.Username == username); */




            var account = await databaseContext.Accounts.Include(a => a.Role)
            .SingleOrDefaultAsync(a => a.Username == username && a.CmpId != null && a.CmpId != "0" && a.stateEmailConfirm == 1);

            if (account != null && VerifyPassword(account.Password, password))
            {
                return account;
            }



            return null;
        }


        public async Task<AccountGoogle?> LoginGoogle(long Id, string Email)
        {
            var account = await databaseContext.AccountsGoogle.Include(a => a.Role)
            .SingleOrDefaultAsync(a => a.Id == Id && a.Email == Email && a.CmpId != null && a.CmpId != "0");



            return account;



        }


        private string CreatePasswordHash(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));

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

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));

            return passwordHashed == hashed;
        }

        public string GenerateToken(Account account)
        {
            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim("role", account.Role.Name),
                new Claim("additional", "todo"),
            };

            return BuildToken(claims);
        }

        public string GenerateTokenGoogle(AccountGoogle account)
        {
            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, account.Email),
                new Claim("role" , account.Role.Name),
                new Claim("additional" , "todo"),
            };
            return BuildToken(claims);
        }

        public string GenerateTokenRegister(string Username)
        {
            var claims = new[] {
              new Claim(JwtRegisteredClaimNames.Sub , Username),
              new Claim("role" , "admin"),
              new Claim("additional" , "todo"),
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


        public Account GetInfo(string accessToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var username = token.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            var role = token.Claims.First(claim => claim.Type == "role").Value;

            var account = new Account
            {
                Username = username,
                Role = new Role
                {
                    Name = role
                }
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
    }
}