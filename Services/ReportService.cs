using Member_App.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Member_App.Services
{
    public class ReportService : IReportService
    {
        private readonly string _connectionString;

        public ReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // CORRECTED: Placeholder table name is replaced with "AccountOpen"
        public async Task<List<AccountOpen>> GetAccountOpenReportAsync(DateTime startDate, DateTime endDate)
        {
            var accounts = new List<AccountOpen>();
            const string query = @"
                SELECT AccountNumber, AccountHolderName, FathersName, MothersName, NationalID, ContactNo, Email, AccountOpeningDate, Balance 
                FROM dbo.AccountOpen
                WHERE AccountOpeningDate BETWEEN @StartDate AND @EndDate
                ORDER BY AccountOpeningDate;";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new AccountOpen
                            {
                                AccountNumber = reader["AccountNumber"].ToString(),
                                AccountHolderName = reader["AccountHolderName"].ToString(),
                                FathersName = reader["FathersName"].ToString(),
                                MothersName = reader["MothersName"].ToString(),
                                NationalID = reader["NationalID"].ToString(),
                                ContactNo = reader["ContactNo"] != DBNull.Value ? reader["ContactNo"].ToString() : null,
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                                AccountOpeningDate = reader["AccountOpeningDate"] != DBNull.Value ? Convert.ToDateTime(reader["AccountOpeningDate"]) : DateTime.MinValue,
                                Balance = reader["Balance"] != DBNull.Value ? Convert.ToDecimal(reader["Balance"]) : 0
                            });
                        }
                    }
                }
            }
            return accounts;
        }

        // CORRECTED: Placeholder table names are replaced with "AccountTransaction" and "AccountOpen"
        // Also removed the unneeded JOIN since AccountHolderName is already in AccountTransaction table.
        public async Task<List<AccountTransactionModel>> GetTransactionReportAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<AccountTransactionModel>();

            const string query = @"
                SELECT 
                    TransId, 
                    AccountNumber, 
                    AccountHolderName, 
                    TransType, 
                     Description,
                    TransAmount, 
                    NewBalance,
                    TransactionDate
                FROM 
                    dbo.AccountTransaction
                WHERE 
                    AccountNumber = @AccountNumber 
                    AND CONVERT(date, TransactionDate) BETWEEN @StartDate AND @EndDate
                ORDER BY 
                    TransactionDate, TransId;";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new AccountTransactionModel
                            {
                                TransId = Convert.ToInt32(reader["TransId"]),
                                AccountNumber = reader["AccountNumber"].ToString(),
                                AccountHolderName = reader["AccountHolderName"].ToString(),
                                TransType = reader["TransType"].ToString(),
                                Description= reader["Description"].ToString(),
                                TransAmount = Convert.ToDecimal(reader["TransAmount"]),
                                NewBalance = Convert.ToDecimal(reader["NewBalance"]),
                                TransactionDate = Convert.ToDateTime(reader["TransactionDate"])
                            });
                        }
                    }
                }
            }
            return transactions;
        }
    }
}