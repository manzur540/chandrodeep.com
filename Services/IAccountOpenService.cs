using Member_App.Models;
using System.Security.Principal;

namespace Member_App.Services
{
    public interface IAccountOpenService
    {
        // --- CREATE ---
        Task<bool> AddAccountAsync(AccountOpen account);

        // --- READ ---
        Task<AccountOpen?> GetAccountByNumberAsync(string accountNumber);
        Task<IEnumerable<AccountOpen>> GetAllAccountsAsync(); // Not used in this workflow, but good to have

        // --- UPDATE ---
        Task<bool> UpdateAccountAsync(AccountOpen account);

        // --- DELETE ---
        Task<bool> DeleteAccountAsync(string accountNumber);
    }
}