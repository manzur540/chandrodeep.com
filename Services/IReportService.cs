using Member_App.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Member_App.Services
{
    /// <summary>
    /// Defines the contract for report generation services.
    /// It outlines the methods that any report service implementation must provide.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Gets a list of transactions for a specific account within a date range.
        /// </summary>
        /// <param name="accountNumber">The account number to fetch transactions for.</param>
        /// <param name="startDate">The start date of the report period.</param>
        /// <param name="endDate">The end date of the report period.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains a list of AccountTransactionModel.</returns>
        Task<List<AccountTransactionModel>> GetTransactionReportAsync(string accountNumber, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a list of accounts opened within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the report period.</param>
        /// <param name="endDate">The end date of the report period.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains a list of AccountOpen models.</returns>
        Task<List<AccountOpen>> GetAccountOpenReportAsync(DateTime startDate, DateTime endDate);
    }
}