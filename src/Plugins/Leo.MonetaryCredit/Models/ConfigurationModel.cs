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
}
