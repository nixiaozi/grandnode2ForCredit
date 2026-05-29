using System.ComponentModel.DataAnnotations;

namespace Leo.MonetaryCredit.Models;

/// <summary>
///     Admin configuration model
/// </summary>
public class ConfigurationModel
{
    public int DisplayOrder { get; set; }
    public string DescriptionText { get; set; }
    public decimal MaxRechargeAmount { get; set; }
    public decimal TransactionFeeRate { get; set; }
    public bool SkipPaymentInfo { get; set; }

    /// <summary>
    ///     Return window in days. Credits are held for this many days before being released.
    ///     0 = release immediately.
    /// </summary>
    [Range(0, 365)]
    public int ReturnWindowDays { get; set; }

    /// <summary>Shopping credit rate (e.g. 0.05 = 5%)</summary>
    [Range(0, 1)]
    public decimal ShoppingCreditRate { get; set; }

    /// <summary>Sales credit rate (e.g. 0.03 = 3%)</summary>
    [Range(0, 1)]
    public decimal SalesCreditRate { get; set; }
}
