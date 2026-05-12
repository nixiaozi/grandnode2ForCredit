using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     User monetary account service
/// </summary>
public interface IUserAccountService
{
    /// <summary>
    ///     Get user account by customer id, creates one if not exists
    /// </summary>
    Task<UserAccount> GetOrCreateAccountAsync(string customerId);

    /// <summary>
    ///     Get user account by customer id, returns null if not exists
    /// </summary>
    Task<UserAccount?> GetAccountAsync(string customerId);

    /// <summary>
    ///     Check if user has sufficient balance
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(string customerId, decimal amount);

    /// <summary>
    ///     Deduct balance (atomic, for payment)
    ///     Throws if insufficient balance
    /// </summary>
    Task<UserAccount> DeductBalanceAsync(string customerId, decimal amount, string orderId, string description);

    /// <summary>
    ///     Add balance (atomic, for receiving payment or recharge)
    /// </summary>
    Task<UserAccount> AddBalanceAsync(string customerId, decimal amount, string? orderId, string? rechargeOrderId, string description, CreditTransactionType transactionType);

    /// <summary>
    ///     Add shopping credits (atomic)
    /// </summary>
    Task<UserAccount> AddShoppingCreditsAsync(string customerId, decimal amount, string? orderId, string description);

    /// <summary>
    ///     Add sales credits (atomic)
    /// </summary>
    Task<UserAccount> AddSalesCreditsAsync(string customerId, decimal amount, string? orderId, string description);

    /// <summary>
    ///     Add activity credits (atomic)
    /// </summary>
    Task<UserAccount> AddActivityCreditsAsync(string customerId, decimal amount, string? orderId, string description);

    /// <summary>
    ///     Get transaction records for a user
    /// </summary>
    Task<IList<CreditRecord>> GetTransactionRecordsAsync(string customerId, int pageIndex = 0, int pageSize = 20);
}
