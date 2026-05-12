namespace Leo.MonetaryCredit.Models;

/// <summary>
///     Credit record view model for user transaction history
/// </summary>
public class CreditRecordViewModel
{
    public DateTime CreatedOnUtc { get; set; }
    public string TransactionTypeName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public decimal BalanceAfter { get; set; }
}
