using Grand.Data;
using Grand.Domain;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     System account transaction service — audit log for platform fee collection / withdrawal
/// </summary>
public interface ISystemAccountTransactionService
{
    /// <summary>
    ///     Insert a new transaction record
    /// </summary>
    Task InsertTransactionAsync(SystemAccountTransaction transaction);

    /// <summary>
    ///     Get paged transaction list (newest first)
    /// </summary>
    Task<IPagedList<SystemAccountTransaction>> GetTransactionsAsync(int pageIndex, int pageSize = 20);
}