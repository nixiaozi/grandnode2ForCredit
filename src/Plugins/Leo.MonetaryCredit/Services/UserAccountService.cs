using Grand.Data;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     User monetary account service implementation
/// </summary>
public class UserAccountService(
    IRepository<UserAccount> userAccountRepository,
    IRepository<CreditRecord> creditRecordRepository)
    : IUserAccountService
{
    public async Task<UserAccount> GetOrCreateAccountAsync(string customerId)
    {
        var account = await userAccountRepository.GetOneAsync(x => x.CustomerId == customerId);
        if (account != null) return account;

        account = new UserAccount { CustomerId = customerId };
        return await userAccountRepository.InsertAsync(account);
    }

    public async Task<UserAccount?> GetAccountAsync(string customerId)
    {
        return await userAccountRepository.GetOneAsync(x => x.CustomerId == customerId);
    }

    public async Task<bool> HasSufficientBalanceAsync(string customerId, decimal amount)
    {
        var account = await GetOrCreateAccountAsync(customerId);
        return account.Balance >= amount;
    }

    public async Task<UserAccount> DeductBalanceAsync(string customerId, decimal amount, string orderId, string description)
    {
        var account = await GetOrCreateAccountAsync(customerId);
        if (account.Balance < amount)
            throw new InvalidOperationException($"余额不足。当前余额: {account.Balance} 元, 需要: {amount} 元");

        // Atomic decrement
        await userAccountRepository.IncField(account.Id, x => x.Balance, -amount);

        var balanceAfter = account.Balance - amount;
        await InsertRecordAsync(new CreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            TransactionType = CreditTransactionType.PaymentDeduction,
            Amount = -amount,
            BalanceAfter = balanceAfter,
            ShoppingCreditsAfter = account.ShoppingCredits,
            SalesCreditsAfter = account.SalesCredits,
            ActivityCreditsAfter = account.ActivityCredits,
            Description = description
        });

        account.Balance = balanceAfter;
        return account;
    }

    public async Task<UserAccount> AddBalanceAsync(string customerId, decimal amount, string? orderId, string? rechargeOrderId, string description, CreditTransactionType transactionType)
    {
        var account = await GetOrCreateAccountAsync(customerId);

        await userAccountRepository.IncField(account.Id, x => x.Balance, amount);

        var balanceAfter = account.Balance + amount;
        await InsertRecordAsync(new CreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            RechargeOrderId = rechargeOrderId,
            TransactionType = transactionType,
            Amount = amount,
            BalanceAfter = balanceAfter,
            ShoppingCreditsAfter = account.ShoppingCredits,
            SalesCreditsAfter = account.SalesCredits,
            ActivityCreditsAfter = account.ActivityCredits,
            Description = description
        });

        account.Balance = balanceAfter;
        return account;
    }

    public async Task<UserAccount> AddShoppingCreditsAsync(string customerId, decimal amount, string? orderId, string description)
    {
        var account = await GetOrCreateAccountAsync(customerId);

        await userAccountRepository.IncField(account.Id, x => x.ShoppingCredits, amount);

        var creditsAfter = account.ShoppingCredits + amount;
        await InsertRecordAsync(new CreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            TransactionType = CreditTransactionType.ShoppingCreditEarned,
            Amount = amount,
            BalanceAfter = account.Balance,
            ShoppingCreditsAfter = creditsAfter,
            SalesCreditsAfter = account.SalesCredits,
            ActivityCreditsAfter = account.ActivityCredits,
            Description = description
        });

        account.ShoppingCredits = creditsAfter;
        return account;
    }

    public async Task<UserAccount> AddSalesCreditsAsync(string customerId, decimal amount, string? orderId, string description)
    {
        var account = await GetOrCreateAccountAsync(customerId);

        await userAccountRepository.IncField(account.Id, x => x.SalesCredits, amount);

        var creditsAfter = account.SalesCredits + amount;
        await InsertRecordAsync(new CreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            TransactionType = CreditTransactionType.SalesCreditEarned,
            Amount = amount,
            BalanceAfter = account.Balance,
            ShoppingCreditsAfter = account.ShoppingCredits,
            SalesCreditsAfter = creditsAfter,
            ActivityCreditsAfter = account.ActivityCredits,
            Description = description
        });

        account.SalesCredits = creditsAfter;
        return account;
    }

    public async Task<UserAccount> AddActivityCreditsAsync(string customerId, decimal amount, string? orderId, string description)
    {
        var account = await GetOrCreateAccountAsync(customerId);

        await userAccountRepository.IncField(account.Id, x => x.ActivityCredits, amount);

        var creditsAfter = account.ActivityCredits + amount;
        await InsertRecordAsync(new CreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            TransactionType = CreditTransactionType.ActivityCreditEarned,
            Amount = amount,
            BalanceAfter = account.Balance,
            ShoppingCreditsAfter = account.ShoppingCredits,
            SalesCreditsAfter = account.SalesCredits,
            ActivityCreditsAfter = creditsAfter,
            Description = description
        });

        account.ActivityCredits = creditsAfter;
        return account;
    }

    public async Task<IList<CreditRecord>> GetTransactionRecordsAsync(string customerId, int pageIndex = 0, int pageSize = 20)
    {
        var query = creditRecordRepository.Table
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedOnUtc);

        return query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
    }

    private async Task InsertRecordAsync(CreditRecord record)
    {
        record.CreatedOnUtc = DateTime.UtcNow;
        await creditRecordRepository.InsertAsync(record);
    }
}
