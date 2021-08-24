using System.ComponentModel.DataAnnotations;

namespace IdentityFramework.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage ="Email is required.")]
        [EmailAddress(ErrorMessage ="Invalid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
