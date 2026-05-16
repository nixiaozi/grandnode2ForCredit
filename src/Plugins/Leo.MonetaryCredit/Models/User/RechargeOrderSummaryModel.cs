using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Models;

/// <summary>
///     Recharge order summary for display on user account page
/// </summary>
public class RechargeOrderSummaryModel
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public RechargeType RechargeType { get; set; }
    public RechargeStatus Status { get; set; }
    public string StatusName { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public string? RejectionReason { get; set; }
}
