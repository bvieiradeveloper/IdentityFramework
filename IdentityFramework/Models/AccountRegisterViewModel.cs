using System.ComponentModel.DataAnnotations;

namespace IdentityFramework.Models
{
    public class AccountRegisterViewModel
    {
        [Required(ErrorMessage ="User name is required")]
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="Email in invald format")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
