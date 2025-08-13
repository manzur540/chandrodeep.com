using System.ComponentModel.DataAnnotations;

namespace Member_App.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^01\d{9}$", ErrorMessage = "Phone must start with 01 and be exactly 11 digits.")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }
    }
}
