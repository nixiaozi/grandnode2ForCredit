using Grand.Data;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Implementation of IPendingCreditService.
///     Credits are stored in MongoDB as PendingCreditRecord documents.
///     A background job (or webhook/event) should call ReleaseAsync periodically for expired records.
/// </summary>
public class PendingCreditService(
    IRepository<PendingCreditRecord> repository,
    IUserAccountService userAccountService)
    : IPendingCreditService
{
    public async Task<PendingCreditRecord> CreateAsync(
        string customerId,
        string orderId,
        CreditTransactionType creditType,
        decimal amount,
        int returnWindowDays,
        string description)
    {
        var record = new PendingCreditRecord
        {
            CustomerId = customerId,
            OrderId = orderId,
            CreditType = creditType,
            Amount = amount,
            ReleasableAfterUtc = DateTime.UtcNow.AddDays(returnWindowDays),
            Status = PendingCreditStatus.Pending,
            Description = description,
            CreatedOnUtc = DateTime.UtcNow
        };

        await repository.InsertAsync(record);
        return record;
    }

    public Task<IList<PendingCreditRecord>> GetReleasableAsync()
    {
        var now = DateTime.UtcNow;
        IList<PendingCreditRecord> result = repository.Table
            .Where(r => r.Status == PendingCreditStatus.Pending && r.ReleasableAfterUtc <= now)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IList<PendingCreditRecord>> GetPendingByOrderAsync(string orderId)
    {
        IList<PendingCreditRecord> result = repository.Table
            .Where(r => r.OrderId == orderId && r.Status == PendingCreditStatus.Pending)
            .ToList();
        return Task.FromResult(result);
    }

    public async Task ReleaseAsync(PendingCreditRecord record)
    {
        if (record.Status != PendingCreditStatus.Pending)
            return;

        // Issue the actual credit
        switch (record.CreditType)
        {
            case CreditTransactionType.ShoppingCreditEarned:
                await userAccountService.AddShoppingCreditsAsync(
                    record.CustomerId, record.Amount, record.OrderId, record.Description);
                break;

            case CreditTransactionType.SalesCreditEarned:
                await userAccountService.AddSalesCreditsAsync(
                    record.CustomerId, record.Amount, record.OrderId, record.Description);
                break;

            default:
                // Other types not expected here, but handle gracefully
                break;
        }

        // Mark as released
        record.Status = PendingCreditStatus.Released;
        record.ProcessedOnUtc = DateTime.UtcNow;
        await repository.UpdateAsync(record);
    }

    public async Task CancelAsync(PendingCreditRecord record)
    {
        if (record.Status != PendingCreditStatus.Pending)
            return;

        record.Status = PendingCreditStatus.Cancelled;
        record.ProcessedOnUtc = DateTime.UtcNow;
        await repository.UpdateAsync(record);
    }

    public async Task CancelAllForOrderAsync(string orderId)
    {
        var pending = await GetPendingByOrderAsync(orderId);
        foreach (var record in pending)
        {
            await CancelAsync(record);
        }
    }
}
