using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     System account - collects transaction fees
///     Singleton: only one document in collection
/// </summary>
public class SystemAccount : BaseEntity
{
    /// <summary>
    ///     System balance - accumulated transaction fees (in Yuan)
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Whether this is the active system account
    /// </summary>
    public bool IsActive { get; set; } = true;
}
