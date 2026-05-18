using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Domain.Orders;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Services;
using MediatR;

namespace Leo.MonetaryCredit.Infrastructure.Handler;

/// <summary>
///     Handles OrderPaidEvent for virtual recharge orders.
///     When the virtual order's payment completes, the actual RechargeOrder is completed
///     and the user's balance is credited.
/// </summary>
public class RechargeOrderPaidHandler(
    IRechargeService rechargeService)
    : INotificationHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        // Only process virtual recharge orders (identified by OrderTag)
        if (order.OrderTags == null)
            return;

        var rechargeTag = order.OrderTags
            .FirstOrDefault(t => t.StartsWith("RechargeOrder:", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(rechargeTag))
            return;

        var rechargeOrderId = rechargeTag.Substring("RechargeOrder:".Length);

        try
        {
            // Complete the recharge order (add balance)
            await rechargeService.CompleteRechargeAfterPaymentAsync(rechargeOrderId);
        }
        catch (Exception)
        {
            // Log but don't throw - the order payment event should not be blocked
            // by recharge processing failures
        }
    }
}
