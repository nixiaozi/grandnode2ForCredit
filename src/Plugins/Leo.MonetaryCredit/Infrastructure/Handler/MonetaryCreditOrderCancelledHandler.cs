using Grand.Business.Core.Events.Checkout.Orders;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Services;
using MediatR;

namespace Leo.MonetaryCredit.Infrastructure.Handler;

/// <summary>
///     Handles OrderCancelledEvent for Monetary Credit orders.
///
///     When an order paid with Monetary Credit is cancelled (including within the 
///     seven-day no-reason return window), this handler:
///     1. Cancels any pending shopping/sales credit records for this order.
///     2. Optionally refunds the buyer's balance (if the original payment was made 
///        with monetary credit and a refund is appropriate — currently a no-op placeholder
///        since balance refund may already be handled by the payment provider).
/// </summary>
public class MonetaryCreditOrderCancelledHandler(
    IPendingCreditService pendingCreditService)
    : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        // Only care about orders paid with Monetary Credit
        if (order.PaymentMethodSystemName != MonetaryCreditDefaults.ProviderSystemName)
            return;

        // Cancel all pending credit records for this order.
        // This covers both buyer's shopping credits and all sellers' sales credits.
        await pendingCreditService.CancelAllForOrderAsync(order.Id);
    }
}
