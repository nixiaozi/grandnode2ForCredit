using Grand.Domain.Orders;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Activity credit provider interface
///     Implemented by another plugin to calculate activity credits from order products
/// </summary>
public interface IActivityCreditProvider
{
    /// <summary>
    ///     Calculate activity credits for an order
    ///     Called after payment is completed
    /// </summary>
    /// <param name="order">The paid order</param>
    /// <returns>Activity credit amount (in Yuan equivalent)</returns>
    Task<decimal> CalculateActivityCreditsAsync(Order order);
}
