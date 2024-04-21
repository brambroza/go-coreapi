using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace goalongapi.Datatools.Account
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }


    }


    public class LoginRequestGoogle
    {
        [Required]
        public long Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}