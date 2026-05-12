namespace Leo.MonetaryCredit.Models;

/// <summary>
///     Admin create recharge request model
/// </summary>
public class CreateRechargeModel
{
    /// <summary>
    ///     Customer Id to recharge
    /// </summary>
    public string CustomerId { get; set; }

    /// <summary>
    ///     Customer display name (for form display)
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    ///     Recharge amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Remark
    /// </summary>
    public string? Remark { get; set; }
}
