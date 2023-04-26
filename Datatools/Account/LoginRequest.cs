using System.ComponentModel.DataAnnotations;

namespace goalongapi.Datatools.Account
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Username {get; set;}

        [Required]
        [MinLength(8)]
        public string Password {get; set;}


    }
}