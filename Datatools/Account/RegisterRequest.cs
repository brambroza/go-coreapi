using System.ComponentModel.DataAnnotations;

namespace goalongapi.Datatools.Account
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required] 
        [MinLength(8)]
        public string Password { get; set; }
        public int RoleId { get; set; }

        [Required]
        public string FullName { get; set; }
        public string CmpId { get; set; }

        public string Url { get; set; }


    }


    public class PasswordChange
    {

        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }



    }

    public class RegisterGoogle
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
    }
}