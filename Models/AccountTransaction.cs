using System.ComponentModel.DataAnnotations;
using System;
namespace Member_App.Models
{
    
    public class AccountTransactionModel
    {
        public int? TransId { get; set; }
        [Required]
        public string AccountNumber { get; set; }
      
        public string AccountHolderName { get; set; }
    
        public decimal BalanceBefore { get; set; }

        [Required]
        public string TransType { get; set; } // DR or CR
        [Required]
        public string Description { get; set; }
        [Required(ErrorMessage = "Balance is required")]
        [Range(typeof(decimal), "0", "792281625", ErrorMessage = "You Have cross Your Limit")]
        public decimal TransAmount { get; set; }


      
        public decimal NewBalance { get; set; }

        
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

}
