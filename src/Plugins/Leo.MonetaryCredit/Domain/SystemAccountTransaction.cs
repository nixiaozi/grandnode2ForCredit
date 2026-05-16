using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     System account transaction record — audit log for platform fee collection / withdrawal
/// </summary>
public class SystemAccountTransaction : BaseEntity
{
    /// <summary>
    ///     Amount changed (+ = inflow, - = outflow)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Balance after this transaction
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    ///     Related order Id (if any)
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    ///     Transaction type description (e.g. "TransactionFee", "Withdraw")
    /// </summary>
    public string TransactionType { get; set; } = "";

    /// <summary>
    ///     Memo / description
    /// </summary>
    public string Description { get; set; } = "";

}
