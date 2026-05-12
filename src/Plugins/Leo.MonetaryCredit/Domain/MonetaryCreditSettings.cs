using Grand.Domain.Configuration;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     Settings for Monetary Credit payment plugin
/// </summary>
public class MonetaryCreditSettings : ISettings
{
    /// <summary>
    ///     Maximum cumulative recharge amount per user (in Yuan)
    ///     0 means unlimited
    /// </summary>
    public decimal MaxRechargeAmount { get; set; } = 0;

    /// <summary>
    ///     Transaction fee rate (e.g., 0.2 means 0.2%)
    ///     Applied when seller receives payment
    /// </summary>
    public decimal TransactionFeeRate { get; set; } = 0.2m;

    /// <summary>
    ///     Display order in payment method list
    /// </summary>
    public int DisplayOrder { get; set; } = 100;

    /// <summary>
    ///     Whether to skip the payment info page during checkout
    /// </summary>
    public bool SkipPaymentInfo { get; set; } = true;

    /// <summary>
    ///     Description text displayed on checkout page
    /// </summary>
    public string DescriptionText { get; set; } = "使用余额支付";
}
