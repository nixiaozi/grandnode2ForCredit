using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Manages credits that are held pending until the return window expires.
/// </summary>
public interface IPendingCreditService
{
    /// <summary>
    ///     Create a pending credit record for an order.
    /// </summary>
    Task<PendingCreditRecord> CreateAsync(
        string customerId,
        string orderId,
        CreditTransactionType creditType,
        decimal amount,
        int returnWindowDays,
        string description);

    /// <summary>
    ///     Get all Pending records whose ReleasableAfterUtc is in the past (ready to release).
    /// </summary>
    Task<IList<PendingCreditRecord>> GetReleasableAsync();

    /// <summary>
    ///     Get all Pending records for a specific order (used on cancellation).
    /// </summary>
    Task<IList<PendingCreditRecord>> GetPendingByOrderAsync(string orderId);

    /// <summary>
    ///     Release a pending credit: issue the credit to the user account and mark as Released.
    /// </summary>
    Task ReleaseAsync(PendingCreditRecord record);

    /// <summary>
    ///     Cancel a pending credit (order cancelled/returned within window). Marks as Cancelled without issuing credit.
    /// </summary>
    Task CancelAsync(PendingCreditRecord record);

    /// <summary>
    ///     Cancel all pending credits for an order (called when the order is cancelled).
    /// </summary>
    Task CancelAllForOrderAsync(string orderId);
}
