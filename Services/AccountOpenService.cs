using static Member_App.Services.AccountOpenService;
using Microsoft.Data.SqlClient;
using System.Security.Principal;
using Member_App.Models;

namespace Member_App.Services
{
    public class AccountOpenService : IAccountOpenService
    {
        private readonly string _connectionString;

        public AccountOpenService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // --- CREATE ---
        public async Task<bool> AddAccountAsync(AccountOpen account)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "INSERT INTO [dbo].[AccountOpen] (AccountNumber, AccountHolderName, FathersName, MothersName, NationalID, ContactNo, Email, AccountOpeningDate, Balance) " +
                    "VALUES (@AccountNumber, @AccountHolderName, @FathersName, @MothersName, @NationalID, @ContactNo, @Email, @AccountOpeningDate, @Balance)", connection);

                AddAccountParameters(command, account);
                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        // --- READ ---
        public async Task<AccountOpen?> GetAccountByNumberAsync(string accountNumber)
        {
            AccountOpen? account = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM [dbo].[AccountOpen] WHERE AccountNumber = @AccountNumber", connection);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        account = MapToAccount(reader);
                    }
                }
            }
            return account;
        }

        // --- UPDATE ---
        public async Task<bool> UpdateAccountAsync(AccountOpen account)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    @"UPDATE [dbo].[AccountOpen] SET 
                        AccountHolderName = @AccountHolderName, 
                        FathersName = @FathersName, 
                        MothersName = @MothersName, 
                        NationalID = @NationalID, 
                        ContactNo = @ContactNo, 
                        Email = @Email, 
                        AccountOpeningDate = @AccountOpeningDate, 
                        Balance = @Balance
                      WHERE AccountNumber = @AccountNumber", connection);

                AddAccountParameters(command, account);
                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        // --- DELETE ---
        public async Task<bool> DeleteAccountAsync(string accountNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "DELETE FROM [dbo].[AccountOpen] WHERE AccountNumber = @AccountNumber", connection);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public Task<IEnumerable<AccountOpen>> GetAllAccountsAsync()
        {
            // This method is not used in the UI workflow but can be implemented if needed later
            throw new NotImplementedException();
        }

        // --- HELPER METHODS (No changes needed) ---
        private void AddAccountParameters(SqlCommand command, AccountOpen account)
        {
            command.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
            command.Parameters.AddWithValue("@AccountHolderName", (object)account.AccountHolderName ?? DBNull.Value);
            command.Parameters.AddWithValue("@FathersName", (object)account.FathersName ?? DBNull.Value);
            command.Parameters.AddWithValue("@MothersName", (object)account.MothersName ?? DBNull.Value);
            command.Parameters.AddWithValue("@NationalID", (object)account.NationalID ?? DBNull.Value);
            command.Parameters.AddWithValue("@ContactNo", (object)account.ContactNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)account.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@AccountOpeningDate", (object)account.AccountOpeningDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Balance", (object)account.Balance ?? DBNull.Value);
        }

        private AccountOpen MapToAccount(SqlDataReader reader)
        {
            return new AccountOpen
            {
                AccountNumber = reader["AccountNumber"].ToString()!,
                AccountHolderName = reader["AccountHolderName"] == DBNull.Value ? null : reader["AccountHolderName"].ToString(),
                FathersName = reader["FathersName"] == DBNull.Value ? null : reader["FathersName"].ToString(),
                MothersName = reader["MothersName"] == DBNull.Value ? null : reader["MothersName"].ToString(),
                NationalID = reader["NationalID"] == DBNull.Value ? null : reader["NationalID"].ToString(),
                ContactNo = reader["ContactNo"] == DBNull.Value ? null : reader["ContactNo"].ToString(),
                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                AccountOpeningDate = reader["AccountOpeningDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["AccountOpeningDate"]),
                Balance = reader["Balance"] == DBNull.Value ? null : Convert.ToDecimal(reader["Balance"])
            };
        }
    }
}