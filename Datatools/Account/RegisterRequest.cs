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

    public class RegisterFromMobileRequest
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

        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Tel { get; set; }
        public string TaxId { get; set; }

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

    public class ForgotPassword
    {

        [Required]
        [EmailAddress]
        public string Username { get; set; }
 

        public string Url { get; set; }

    }

      public class ResetPassword
    {
 
        public string ResetToken { get; set; }

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