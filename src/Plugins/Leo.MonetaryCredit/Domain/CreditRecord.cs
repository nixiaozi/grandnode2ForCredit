using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     Credit / balance transaction record (audit log)
/// </summary>
public class CreditRecord : BaseEntity
{
    /// <summary>
    ///     Associated Customer Id
    /// </summary>
    public string CustomerId { get; set; }

    /// <summary>
    ///     Related order Id (if any)
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    ///     Related recharge order Id (if any)
    /// </summary>
    public string? RechargeOrderId { get; set; }

    /// <summary>
    ///     Transaction type
    /// </summary>
    public CreditTransactionType TransactionType { get; set; }

    /// <summary>
    ///     Amount changed (positive = credit, negative = debit)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Balance after transaction
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    ///     Shopping credits after transaction
    /// </summary>
    public decimal ShoppingCreditsAfter { get; set; }

    /// <summary>
    ///     Sales credits after transaction
    /// </summary>
    public decimal SalesCreditsAfter { get; set; }

    /// <summary>
    ///     Activity credits after transaction
    /// </summary>
    public decimal ActivityCreditsAfter { get; set; }

    /// <summary>
    ///     Description / memo
    /// </summary>
    public string Description { get; set; }

}

/// <summary>
///     Types of credit/balance transactions
/// </summary>
public enum CreditTransactionType
{
    /// <summary>
    ///     Admin manual recharge (after approval)
    /// </summary>
    Recharge = 0,

    /// <summary>
    ///     Balance deduction for order payment (buyer)
    /// </summary>
    PaymentDeduction = 1,

    /// <summary>
    ///     Shopping credits earned from purchase (buyer)
    /// </summary>
    ShoppingCreditEarned = 2,

    /// <summary>
    ///     Balance received from order payment (seller)
    /// </summary>
    PaymentReceived = 3,

    /// <summary>
    ///     Sales credits earned from sale (seller)
    /// </summary>
    SalesCreditEarned = 4,

    /// <summary>
    ///     Activity credits earned
    /// </summary>
    ActivityCreditEarned = 5,

    /// <summary>
    ///     Refund - balance returned
    /// </summary>
    Refund = 6,

    /// <summary>
    ///     Transaction fee collected by system
    /// </summary>
    TransactionFee = 7
}
