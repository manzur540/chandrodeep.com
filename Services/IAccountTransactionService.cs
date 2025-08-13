using Member_App.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Member_App.Services
{
    public interface IAccountTransactionService
    {
        Task<List<SelectListItem>> GetAccountsForDropdownAsync();
        Task<string> GetAccountHolderNameAsync(string accountNumber);
        Task<decimal> GetBalanceAsync(string accountNumber);
        Task<(bool Success, string Message)> SaveTransactionAsync(AccountTransactionModel model);
        Task<AccountTransactionModel?> GetTransactionByIdAsync(int transactionId);
        Task<(bool Success, string Message)> UpdateTransactionAsync(AccountTransactionModel model);
        Task<(bool Success, string Message)> DeleteTransactionAsync(int transactionId);

        Task<List<AccountTransactionModel>> GetPendingTransactionsAsync();
        Task<(bool Success, string Message)> ApproveTransactionAsync(int transactionId, string approvedBy);


    }
}

