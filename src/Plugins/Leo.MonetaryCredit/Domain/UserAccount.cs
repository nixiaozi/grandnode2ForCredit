using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     User monetary account - balance and credits
///     Stored in separate MongoDB collection, decoupled from Customer entity
/// </summary>
public class UserAccount : BaseEntity
{
    /// <summary>
    ///     Associated Customer Id
    /// </summary>
    public string CustomerId { get; set; }

    /// <summary>
    ///     Current balance (in Yuan)
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Shopping credits - earned when purchasing (in Yuan equivalent)
    /// </summary>
    public decimal ShoppingCredits { get; set; }

    /// <summary>
    ///     Sales credits - earned when selling (in Yuan equivalent)
    /// </summary>
    public decimal SalesCredits { get; set; }

    /// <summary>
    ///     Activity credits - earned from product activities (in Yuan equivalent)
    /// </summary>
    public decimal ActivityCredits { get; set; }

    /// <summary>
    ///     Total cumulative recharge amount (in Yuan)
    ///     Used to enforce max recharge limit
    /// </summary>
    public decimal TotalRecharged { get; set; }
}
