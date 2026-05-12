using Grand.Data;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     System account service implementation
/// </summary>
public class SystemAccountService(
    IRepository<SystemAccount> systemAccountRepository,
    IRepository<CreditRecord> creditRecordRepository)
    : ISystemAccountService
{
    public async Task<SystemAccount> GetOrCreateSystemAccountAsync()
    {
        var account = await systemAccountRepository.GetOneAsync(x => x.IsActive);
        if (account != null) return account;

        account = new SystemAccount { IsActive = true };
        return await systemAccountRepository.InsertAsync(account);
    }

    public async Task<SystemAccount> CollectFeeAsync(decimal amount, string orderId, string description)
    {
        var account = await GetOrCreateSystemAccountAsync();

        await systemAccountRepository.IncField(account.Id, x => x.Balance, amount);

        account.Balance += amount;

        // Record the fee transaction (no customer associated)
        await creditRecordRepository.InsertAsync(new CreditRecord
        {
            CustomerId = "SYSTEM",
            OrderId = orderId,
            TransactionType = CreditTransactionType.TransactionFee,
            Amount = amount,
            BalanceAfter = account.Balance,
            ShoppingCreditsAfter = 0,
            SalesCreditsAfter = 0,
            ActivityCreditsAfter = 0,
            Description = description
        });

        return account;
    }
}
