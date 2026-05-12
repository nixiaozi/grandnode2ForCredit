using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Domain.Orders;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Services;
using MediatR;

namespace Leo.MonetaryCredit.Infrastructure.Handler;

/// <summary>
///     Handles OrderPaidEvent - distributes balance and credits after payment
///     
///     Payment flow:
///     1. Buyer's balance -= order amount
///     2. Buyer's shopping credits += order amount
///     3. For each order item:
///        a. Seller's balance += item amount × (1 - fee rate)
///        b. Seller's sales credits += item amount × (1 - fee rate)
///        c. System account balance += item amount × fee rate
///     4. Calculate activity credits (via IActivityCreditProvider if available)
/// </summary>
public class MonetaryCreditOrderPaidHandler(
    IUserAccountService userAccountService,
    ISystemAccountService systemAccountService,
    IMonetaryCreditSettingsService settingsService,
    IEnumerable<IActivityCreditProvider> activityCreditProviders,
    Grand.Data.IRepository<Grand.Domain.Orders.Order> orderRepository)
    : INotificationHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        // Only process orders paid with Monetary Credit
        if (order.PaymentMethodSystemName != MonetaryCreditDefaults.ProviderSystemName)
            return;

        var settings = await settingsService.GetSettingsAsync();
        var feeRate = settings.TransactionFeeRate / 100m; // e.g., 0.2% → 0.002

        var buyerId = order.CustomerId;
        var orderId = order.Id;

        // Step 1: Deduct buyer's balance (already done in ProcessPayment, but let's verify)
        // Actually, balance deduction should happen in ProcessPayment before order is placed.
        // Here we only handle credit distribution.

        // Step 2: Add buyer's shopping credits
        var orderAmount = (decimal)order.OrderTotal;
        await userAccountService.AddShoppingCreditsAsync(
            buyerId,
            orderAmount,
            orderId,
            $"订单支付获得购物积分，金额: {orderAmount} 元");

        // Step 3: Process seller payments for each order item
        foreach (var item in order.OrderItems)
        {
            if (string.IsNullOrEmpty(item.VendorId))
                continue; // Skip items without a vendor (platform's own products)

            var itemAmount = (decimal)item.PriceInclTax;
            var feeAmount = itemAmount * feeRate;
            var sellerAmount = itemAmount - feeAmount;

            // Add seller's balance
            await userAccountService.AddBalanceAsync(
                item.VendorId,
                sellerAmount,
                orderId,
                null,
                $"订单商品销售收款，金额: {sellerAmount} 元 (订单: {order.Code})",
                CreditTransactionType.PaymentReceived);

            // Add seller's sales credits
            await userAccountService.AddSalesCreditsAsync(
                item.VendorId,
                sellerAmount,
                orderId,
                $"订单商品销售获得销售积分，金额: {sellerAmount} 元 (订单: {order.Code})");

            // Collect transaction fee to system account
            if (feeAmount > 0)
            {
                await systemAccountService.CollectFeeAsync(
                    feeAmount,
                    orderId,
                    $"交易手续费，金额: {feeAmount} 元 (订单: {order.Code}, 卖家: {item.VendorId})");
            }
        }

        // Step 4: Calculate activity credits (if provider is registered)
        var provider = activityCreditProviders.FirstOrDefault();
        if (provider != null)
        {
            try
            {
                var activityCredits = await provider.CalculateActivityCreditsAsync(order);
                if (activityCredits > 0)
                {
                    await userAccountService.AddActivityCreditsAsync(
                        buyerId,
                        activityCredits,
                        orderId,
                        $"订单支付获得活动积分，金额: {activityCredits} 元");
                }
            }
            catch
            {
                // Activity credit calculation failure should not affect payment
                // Log but don't throw
            }
        }
    }
}
