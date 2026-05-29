using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     A credit award that is held pending until the return window expires.
///     Created when an order is paid; released when the window passes or cancelled when the order is returned/cancelled.
/// </summary>
public class PendingCreditRecord : BaseEntity
{
    /// <summary>Customer (buyer or seller) who will receive the credit</summary>
    public string CustomerId { get; set; }

    /// <summary>The order that triggered this credit</summary>
    public string OrderId { get; set; }

    /// <summary>Type of credit to be issued (ShoppingCreditEarned or SalesCreditEarned)</summary>
    public CreditTransactionType CreditType { get; set; }

    /// <summary>Amount of credit to award (in Yuan equivalent)</summary>
    public decimal Amount { get; set; }

    /// <summary>UTC time after which this credit can be released (CreatedOnUtc + ReturnWindowDays)</summary>
    public DateTime ReleasableAfterUtc { get; set; }

    /// <summary>Current state of this pending credit</summary>
    public PendingCreditStatus Status { get; set; } = PendingCreditStatus.Pending;

    /// <summary>UTC time when this record was released or cancelled</summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>Human-readable note for the credit record created on release</summary>
    public string Description { get; set; }
}

/// <summary>State machine for a pending credit record</summary>
public enum PendingCreditStatus
{
    /// <summary>Holding — return window has not yet expired</summary>
    Pending = 0,

    /// <summary>Credit has been issued to the user account</summary>
    Released = 1,

    /// <summary>Cancelled because the order was returned or cancelled within the window</summary>
    Cancelled = 2
}
