using Member_App.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.Data.SqlClient;



namespace Member_App.Services
{
    public class AccountTransactionService : IAccountTransactionService
    {
        private readonly string _connectionString;

        public AccountTransactionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SelectListItem>> GetAccountsForDropdownAsync()
        {
            var accounts = new List<SelectListItem>();
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT AccountNumber FROM AccountOpen", con);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string accNumber = reader["AccountNumber"].ToString();
                        accounts.Add(new SelectListItem { Value = accNumber, Text = accNumber });
                    }
                }
            }
            return accounts;
        }

        public async Task<string> GetAccountHolderNameAsync(string accountNumber)
        {
            string accountHolder = "";
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT AccountHolderName FROM AccountOpen WHERE AccountNumber = @accNo", con);
                cmd.Parameters.AddWithValue("@accNo", accountNumber);
                await con.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                    accountHolder = result.ToString();
            }
            return accountHolder;
        }

        public async Task<decimal> GetBalanceAsync(string accountNumber)
        {
            decimal balance = 0;
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT Balance FROM AccountOpen WHERE AccountNumber = @accNo", con);
                cmd.Parameters.AddWithValue("@accNo", accountNumber);
                await con.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                    balance = Convert.ToDecimal(result);
            }
            return balance;
        }

        public async Task<(bool Success, string Message)> SaveTransactionAsync(AccountTransactionModel model)
        {
            if (model.TransType == "DR" && (model.BalanceBefore == 0 || model.NewBalance <= 0))
            {
                return (false, "❌ Debit not allowed. Balance is 0 or would become negative.");
            }

            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                // Start a database transaction to ensure both commands succeed or fail together.
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand(@"
                            INSERT INTO AccountTransaction (AccountNumber, AccountHolderName, BalanceBefore, TransType, Description,TransAmount, NewBalance,TransactionDate,IsApproved)
                            VALUES (@AccountNumber, @AccountHolderName, @BalanceBefore, @TransType,@Description, @TransAmount, @NewBalance, @TransactionDate,0);

                            ", con, transaction);

                        cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber);
                        cmd.Parameters.AddWithValue("@AccountHolderName", model.AccountHolderName);
                        cmd.Parameters.AddWithValue("@BalanceBefore", model.BalanceBefore);
                        cmd.Parameters.AddWithValue("@TransType", model.TransType);
                        cmd.Parameters.AddWithValue("@Description", model.Description);
                        cmd.Parameters.AddWithValue("@TransAmount", model.TransAmount);
                        cmd.Parameters.AddWithValue("@NewBalance", model.NewBalance);
                        cmd.Parameters.AddWithValue("@TransactionDate", model.TransactionDate);

                        await cmd.ExecuteNonQueryAsync();

                        // If everything is successful, commit the transaction.
                        transaction.Commit();
                        return (true, "Transaction saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        // If an error occurs, roll back all changes.
                        transaction.Rollback();
                        return (false, $"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public async Task<AccountTransactionModel?> GetTransactionByIdAsync(int transactionId)
        {

            AccountTransactionModel? trans = null;
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT * FROM AccountTransaction WHERE TransId = @TransId", con);
                cmd.Parameters.AddWithValue("@TransId", transactionId);

                await con.OpenAsync();
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    if (await rdr.ReadAsync())
                    {
                        trans = new AccountTransactionModel
                        {
                            TransId = Convert.ToInt32(rdr["TransId"]),
                            AccountNumber = rdr["AccountNumber"].ToString(),
                            AccountHolderName = rdr["AccountHolderName"].ToString(),
                            BalanceBefore = Convert.ToDecimal(rdr["BalanceBefore"]),
                            TransType = rdr["TransType"].ToString(),
                            Description = rdr["Description"].ToString(),
                            TransAmount = Convert.ToDecimal(rdr["TransAmount"]),
                            NewBalance = Convert.ToDecimal(rdr["NewBalance"])
                        };
                    }
                }
            }
            return trans;
        }

        public async Task<(bool Success, string Message)> UpdateTransactionAsync(AccountTransactionModel model)
        {

            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand(@"UPDATE AccountTransaction
                                           SET AccountNumber = @AccountNumber,
                                               AccountHolderName = @AccountHolderName,
                                               BalanceBefore = @BalanceBefore,
                                               TransType = @TransType,
                                               Description=@Description,
                                               TransAmount = @TransAmount,
                                               NewBalance = @NewBalance
                                           WHERE TransId = @TransId; 
                                           
                                           UPDATE AccountOpen SET Balance = @NewBalance WHERE AccountNumber = @AccountNumber;", con, transaction);

                        cmd.Parameters.AddWithValue("@TransId", model.TransId);
                        cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber);
                        cmd.Parameters.AddWithValue("@AccountHolderName", model.AccountHolderName);
                        cmd.Parameters.AddWithValue("@BalanceBefore", model.BalanceBefore);
                        cmd.Parameters.AddWithValue("@TransType", model.TransType);
                        cmd.Parameters.AddWithValue("@Description", model.Description);
                        cmd.Parameters.AddWithValue("@TransAmount", model.TransAmount);
                        cmd.Parameters.AddWithValue("@NewBalance", model.NewBalance);

                        await cmd.ExecuteNonQueryAsync();
                        transaction.Commit();
                        return (true, "Transaction updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Error updating transaction: {ex.Message}");
                    }
                }
            }
        }

        public async Task<(bool Success, string Message)> DeleteTransactionAsync(int transactionId)
        {

            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("DELETE FROM AccountTransaction WHERE TransId = @TransId", con);
                cmd.Parameters.AddWithValue("@TransId", transactionId);

                await con.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return (true, "Transaction deleted successfully.");
                }
                return (false, "Could not find the transaction to delete.");
            }
        }


        public async Task<List<AccountTransactionModel>> GetPendingTransactionsAsync()
        {
            var transactions = new List<AccountTransactionModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                string query = @"SELECT TransId, AccountNumber, AccountHolderName, TransType, Description,
                                TransAmount, BalanceBefore, NewBalance, TransactionDate,
                                IsApproved, ApprovedBy, ApprovedDate
                         FROM AccountTransaction
                         WHERE IsApproved = 0";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transactions.Add(new AccountTransactionModel
                        {
                            TransId = reader.GetInt32(0),
                            AccountNumber = reader.GetString(1),
                            AccountHolderName = reader.GetString(2),
                            TransType = reader.GetString(3),
                            Description = reader.GetString(4),
                            TransAmount = reader.GetDecimal(5),
                            BalanceBefore = reader.GetDecimal(6),
                            NewBalance = reader.GetDecimal(7),
                            TransactionDate = reader.GetDateTime(8),
                            IsApproved = reader.GetBoolean(9),
                            ApprovedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                            ApprovedDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                        });
                    }
                }
            }

            return transactions;
        }


        public async Task<(bool Success, string Message)> ApproveTransactionAsync(int transactionId, string approvedBy)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Get transaction details
                        var getCmd = new SqlCommand(@"
                    SELECT AccountNumber, TransType, TransAmount, BalanceBefore
                    FROM AccountTransaction
                    WHERE TransId = @TransId AND IsApproved = 0", con, transaction);
                        getCmd.Parameters.AddWithValue("@TransId", transactionId);

                        string accountNumber = null;
                        string transType = null;
                        decimal transAmount = 0, balanceBefore = 0;

                        using (var reader = await getCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                accountNumber = reader["AccountNumber"].ToString();
                                transType = reader["TransType"].ToString();
                                transAmount = reader.GetDecimal(reader.GetOrdinal("TransAmount"));
                                balanceBefore = reader.GetDecimal(reader.GetOrdinal("BalanceBefore"));
                            }
                            else
                            {
                                return (false, "Transaction not found or already approved.");
                            }
                        }

                        // 2️⃣ Calculate new balance
                        decimal newBalance = (transType == "CR")
                            ? balanceBefore + transAmount
                            : balanceBefore - transAmount;

                        // 3️⃣ Update AccountOpen balance
                        var updateBalanceCmd = new SqlCommand(@"
                    UPDATE AccountOpen SET Balance = @NewBalance
                    WHERE AccountNumber = @AccountNumber", con, transaction);
                        updateBalanceCmd.Parameters.AddWithValue("@NewBalance", newBalance);
                        updateBalanceCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        await updateBalanceCmd.ExecuteNonQueryAsync();

                        // 4️⃣ Mark transaction approved and store new balance
                        var approveCmd = new SqlCommand(@"
                    UPDATE AccountTransaction
                    SET IsApproved = 1, ApprovedBy = @ApprovedBy, ApprovedDate = GETDATE(), NewBalance = @NewBalance
                    WHERE TransId = @TransId AND IsApproved = 0", con, transaction);
                        approveCmd.Parameters.AddWithValue("@ApprovedBy", approvedBy);
                        approveCmd.Parameters.AddWithValue("@NewBalance", newBalance);
                        approveCmd.Parameters.AddWithValue("@TransId", transactionId);
                        await approveCmd.ExecuteNonQueryAsync();

                        transaction.Commit();
                        return (true, "Transaction approved and balance updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Error approving transaction: {ex.Message}");
                    }
                }
            }
        }
    }
}



















    