using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     System account service - manages the platform's fee collection account
/// </summary>
public interface ISystemAccountService
{
    /// <summary>
    ///     Get or create the system account
    /// </summary>
    Task<SystemAccount> GetOrCreateSystemAccountAsync();

    /// <summary>
    ///     Add transaction fee to system balance (atomic)
    /// </summary>
    Task<SystemAccount> CollectFeeAsync(decimal amount, string orderId, string description);
}
