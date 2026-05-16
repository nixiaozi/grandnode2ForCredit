using Grand.Data;
using Grand.Domain;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     System account transaction service implementation
/// </summary>
public class SystemAccountTransactionService(IRepository<SystemAccountTransaction> transactionRepository)
    : ISystemAccountTransactionService
{
    public async Task InsertTransactionAsync(SystemAccountTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transactionRepository.InsertAsync(transaction);
    }

    public async Task<IPagedList<SystemAccountTransaction>> GetTransactionsAsync(int pageIndex, int pageSize = 20)
    {
        var query = transactionRepository.Table
            .OrderByDescending(x => x.CreatedOnUtc);
        return await PagedList<SystemAccountTransaction>.Create(query, pageIndex, pageSize);
    }
}