namespace Leo.MonetaryCredit.Models;

/// <summary>
///     User account view model
/// </summary>
public class UserAccountViewModel
{
    public decimal Balance { get; set; }
    public decimal ShoppingCredits { get; set; }
    public decimal SalesCredits { get; set; }
    public decimal ActivityCredits { get; set; }
    public decimal TotalRecharged { get; set; }
    public decimal MaxRechargeAmount { get; set; }
    public decimal RemainingRechargeQuota => MaxRechargeAmount > 0
        ? Math.Max(0, MaxRechargeAmount - TotalRecharged)
        : -1; // -1 means unlimited

    /// <summary>
    ///     Recent recharge orders for this customer
    /// </summary>
    public List<RechargeOrderSummaryModel> RechargeOrders { get; set; } = new();
}
