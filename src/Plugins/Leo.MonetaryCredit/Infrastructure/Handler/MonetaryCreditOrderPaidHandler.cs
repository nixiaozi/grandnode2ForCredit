using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Domain.Orders;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Services;
using MediatR;

namespace Leo.MonetaryCredit.Infrastructure.Handler;

/// <summary>
///     Handles OrderPaidEvent - distributes balance and credits after payment.
///
///     Credit release strategy (configurable):
///     - ReturnWindowDays > 0 → shopping/sales credits are held as PendingCreditRecord
///       and released automatically after the return window expires (by a scheduled job
///       that calls IPendingCreditService.GetReleasableAsync / ReleaseAsync).
///       If the order is cancelled before the window closes, the handler
///       MonetaryCreditOrderCancelledHandler cancels the pending records instead.
///     - ReturnWindowDays == 0 → credits are issued immediately (legacy behaviour).
///
///     Balance flows (still immediate regardless of return window):
///     1. Buyer's balance is deducted in ProcessPayment (before this event).
///     2. Seller's balance is credited immediately (seller can use it; if refund occurs, handled separately).
///     3. System collects transaction fee immediately.
///
///     Credit flows (subject to return window):
///     4. Buyer's shopping credits — pending or immediate.
///     5. Seller's sales credits — pending or immediate.
///     6. Activity credits — always immediate (campaign-driven, not returnable).
/// </summary>
public class MonetaryCreditOrderPaidHandler(
    IUserAccountService userAccountService,
    ISystemAccountService systemAccountService,
    IPendingCreditService pendingCreditService,
    IMonetaryCreditSettingsService settingsService,
    IEnumerable<IActivityCreditProvider> activityCreditProviders)
    : INotificationHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;

        // Only process orders paid with Monetary Credit
        if (order.PaymentMethodSystemName != MonetaryCreditDefaults.ProviderSystemName)
            return;

        var settings = await settingsService.GetSettingsAsync();
        var feeRate = settings.TransactionFeeRate / 100m;         // e.g. 0.2% → 0.002
        var shoppingCreditRate = settings.ShoppingCreditRate;     // e.g. 0.05 → 5%
        var salesCreditRate = settings.SalesCreditRate;           // e.g. 0.03 → 3%
        var returnWindowDays = settings.ReturnWindowDays;

        var buyerId = order.CustomerId;
        var orderId = order.Id;
        var orderAmount = (decimal)order.OrderTotal;

        // ── Step 1: Buyer's shopping credits ──────────────────────────────────
        if (shoppingCreditRate > 0)
        {
            var shoppingCredits = orderAmount * shoppingCreditRate;
            var desc = $"订单支付获得购物积分 ({shoppingCreditRate:P0})，金额 {shoppingCredits:F2} 元 (订单: {order.Code})";

            if (returnWindowDays > 0)
            {
                await pendingCreditService.CreateAsync(
                    buyerId, orderId,
                    CreditTransactionType.ShoppingCreditEarned,
                    shoppingCredits, returnWindowDays,
                    desc);
            }
            else
            {
                await userAccountService.AddShoppingCreditsAsync(buyerId, shoppingCredits, orderId, desc);
            }
        }

        // ── Step 2: Per-item seller flows ─────────────────────────────────────
        foreach (var item in order.OrderItems)
        {
            if (string.IsNullOrEmpty(item.VendorId))
                continue; // Platform's own products — no seller to pay

            var itemAmount = (decimal)item.PriceInclTax;
            var feeAmount = itemAmount * feeRate;
            var sellerAmount = itemAmount - feeAmount;

            // Seller balance is credited immediately (economic reality)
            await userAccountService.AddBalanceAsync(
                item.VendorId,
                sellerAmount,
                orderId,
                null,
                $"订单商品销售收款 {sellerAmount:F2} 元 (订单: {order.Code})",
                CreditTransactionType.PaymentReceived);

            // Seller sales credits — subject to return window
            if (salesCreditRate > 0)
            {
                var salesCredits = sellerAmount * salesCreditRate;
                var desc = $"订单商品销售获得销售积分 ({salesCreditRate:P0})，金额 {salesCredits:F2} 元 (订单: {order.Code})";

                if (returnWindowDays > 0)
                {
                    await pendingCreditService.CreateAsync(
                        item.VendorId, orderId,
                        CreditTransactionType.SalesCreditEarned,
                        salesCredits, returnWindowDays,
                        desc);
                }
                else
                {
                    await userAccountService.AddSalesCreditsAsync(item.VendorId, salesCredits, orderId, desc);
                }
            }

            // System transaction fee
            if (feeAmount > 0)
            {
                await systemAccountService.CollectFeeAsync(
                    feeAmount,
                    orderId,
                    $"交易手续费 {feeAmount:F2} 元 (订单: {order.Code}, 卖家: {item.VendorId})");
            }
        }

        // ── Step 3: Activity credits (always immediate — campaign rules) ──────
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
                        $"订单支付获得活动积分 {activityCredits:F2} 元 (订单: {order.Code})");
                }
            }
            catch
            {
                // Activity credit failure must not block payment processing
            }
        }
    }
}
