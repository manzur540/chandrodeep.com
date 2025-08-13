using System.ComponentModel.DataAnnotations;

namespace Member_App.Models
{
    public class AccountOpen
    {
        [Required(ErrorMessage = "Account Number is required")]
        [StringLength(7, ErrorMessage = "Account Number cannot exceed 7 digits")]
      
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Account Holder Name is required")]
        [StringLength(100)                   ]
        public string? AccountHolderName { get; set; }

        [Required(ErrorMessage = "Father's Name is required")]
        [StringLength(100)]
        public string? FathersName { get; set; }

        [Required(ErrorMessage = "Mother's Name is required")]
        [StringLength(100)]
        public string? MothersName { get; set; }

        [StringLength(17, ErrorMessage = "NID cannot exceed 17 digits")]
        [Required(ErrorMessage = "NID is required")]
        [RegularExpression(@"^\d{0,17}$", ErrorMessage = "NID must contain digits only")]
        public string? NationalID { get; set; }

        [Phone(ErrorMessage = "Invalid Contact Number")]
        [StringLength(11, ErrorMessage = "Contact Number cannot exceed 11 digits")]
        public string? ContactNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Account Opening Date is required")]
        [DataType(DataType.Date)]
        public DateTime? AccountOpeningDate { get; set; }

        [Required(ErrorMessage = "Balance is required")]
        
        [Range(typeof(decimal), "0", "792281625", ErrorMessage = "You Have cross Your Limit")]
        public decimal? Balance { get; set; }
    }
}

