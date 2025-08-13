using System.ComponentModel.DataAnnotations;

namespace Member_App.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^01\d{9}$", ErrorMessage = "Phone must start with 01 and be exactly 11 digits.")]
        public string Phone { get; set; }
    }
}