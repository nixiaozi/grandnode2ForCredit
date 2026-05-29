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

    /// <summary>
    ///     Return window period in days.
    ///     Shopping credits and sales credits are held as "pending" for this many days after order payment.
    ///     They are released automatically once the window expires (no return happened),
    ///     or cancelled if the order is cancelled/refunded within the window.
    ///     Set to 0 to release credits immediately upon payment (no hold period).
    /// </summary>
    public int ReturnWindowDays { get; set; } = 7;

    /// <summary>
    ///     Shopping credit rate: credits earned per Yuan paid by buyer (e.g. 0.05 = 5%)
    ///     0 means no shopping credits are issued.
    /// </summary>
    public decimal ShoppingCreditRate { get; set; } = 0.05m;

    /// <summary>
    ///     Sales credit rate: credits earned per Yuan received by seller (e.g. 0.03 = 3%)
    ///     0 means no sales credits are issued.
    /// </summary>
    public decimal SalesCreditRate { get; set; } = 0.03m;
}
