using goalongapi.Entities;

namespace goalongapi.Interfaces
{
    public interface IAccountService
    {
         Task Register(Account account);
         Task RegisterGoogle(AccountGoogle accountGoogle);
         Task<Account> Login(string username, string password);
         Task<AccountGoogle> LoginGoogle(long Id  , string Email);
         string GenerateToken(Account account);
         string GenerateTokenGoogle(AccountGoogle account);
         Account GetInfo(string accessToken);
         string GenerateTokenRegister(string Username);
         bool UpdateConfirmEmail (string Username);
         bool validateEmails(string Username);
         bool removeUser(string Username);
         

    }
}