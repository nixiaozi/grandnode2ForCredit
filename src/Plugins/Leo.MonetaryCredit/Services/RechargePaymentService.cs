using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Infrastructure;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Recharge payment service - bridges recharge orders with the platform's payment system
///     Creates a virtual Order for each recharge so that any payment plugin (Stripe, BrainTree, etc.)
///     can be used without modification.
/// </summary>
public class RechargePaymentService(
    IPaymentService paymentService,
    IPaymentTransactionService paymentTransactionService,
    IOrderService orderService,
    IContextAccessor contextAccessor,
    IRechargeService rechargeService)
    : IRechargePaymentService
{
    public async Task<IList<PaymentMethodModel>> GetAvailablePaymentMethodsAsync()
    {
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var storeId = contextAccessor.StoreContext.CurrentStore.Id;

        var allMethods = await paymentService.LoadActivePaymentMethods(customer, storeId);
        var models = new List<PaymentMethodModel>();

        foreach (var method in allMethods)
        {
            // Exclude self (MonetaryCredit) to prevent using balance to recharge balance
            if (method.SystemName.Equals(MonetaryCreditDefaults.ProviderSystemName, StringComparison.OrdinalIgnoreCase))
                continue;

            models.Add(new PaymentMethodModel
            {
                SystemName = method.SystemName,
                FriendlyName = method.FriendlyName,
                LogoUrl = method.LogoURL,
                Description = await method.Description()
            });
        }

        return models;
    }

    public async Task<RechargePaymentResult> CreatePaymentAndRedirectAsync(string rechargeOrderId, string paymentMethodSystemName)
    {
        var rechargeOrder = await rechargeService.GetRechargeOrderAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (rechargeOrder.Status != RechargeStatus.WaitingPayment)
            throw new InvalidOperationException($"充值订单当前状态 {rechargeOrder.Status} 不允许发起支付");

        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var store = contextAccessor.StoreContext.CurrentStore;
        var currency = contextAccessor.WorkContext.WorkingCurrency;

        // Create a virtual Order for payment gateway integration
        var virtualOrder = new Order
        {
            OrderGuid = Guid.NewGuid(),
            StoreId = store.Id,
            CustomerId = customer.Id,
            CustomerEmail = customer.Email,
            CustomerCurrencyCode = currency.CurrencyCode,
            CurrencyRate = currency.Rate,
            PaymentMethodSystemName = paymentMethodSystemName,
            OrderStatusId = (int)1,
            PaymentStatusId = PaymentStatus.Pending,
            ShippingStatusId = ShippingStatus.ShippingNotRequired,
            OrderSubtotalInclTax = (double)rechargeOrder.Amount,
            OrderSubtotalExclTax = (double)rechargeOrder.Amount,
            OrderTotal = (double)rechargeOrder.Amount,
            OrderDiscount = 0,
            OrderTax = 0,
            OrderShippingInclTax = 0,
            OrderShippingExclTax = 0,
            PaymentMethodAdditionalFeeInclTax = 0,
            PaymentMethodAdditionalFeeExclTax = 0,
            RefundedAmount = 0,
            Deleted = false,
            CreatedOnUtc = DateTime.UtcNow,
            CheckoutAttributeDescription = "RechargeOrder:" + rechargeOrderId,
        };

        await orderService.InsertOrder(virtualOrder);

        // Update recharge order with virtual order info
        rechargeOrder.PaymentMethodSystemName = paymentMethodSystemName;
        rechargeOrder.VirtualOrderId = virtualOrder.Id;
        await rechargeService.UpdateRechargeOrderAsync(rechargeOrder);

        // Create PaymentTransaction linked to the virtual order
        var paymentTransaction = new PaymentTransaction
        {
            PaymentMethodSystemName = paymentMethodSystemName,
            TransactionStatus = TransactionStatus.Pending,
            StoreId = store.Id,
            OrderGuid = virtualOrder.OrderGuid,
            CustomerId = customer.Id,
            CustomerEmail = customer.Email,
            CurrencyCode = currency.CurrencyCode,
            CurrencyRate = currency.Rate,
            TransactionAmount = (double)rechargeOrder.Amount,
            IPAddress = customer?.LastIpAddress,
            Description = $"账户充值 ¥{rechargeOrder.Amount}",
            AdditionalInfo = $"RechargeOrderId:{rechargeOrderId}"
        };

        await paymentTransactionService.InsertPaymentTransaction(paymentTransaction);
        rechargeOrder.PaymentTransactionId = paymentTransaction.Id;
        await rechargeService.UpdateRechargeOrderAsync(rechargeOrder);

        // Get redirect URL from the payment provider
        var redirectUrl = await paymentService.PostRedirectPayment(paymentTransaction);
        return new RechargePaymentResult
        {
            RechargeOrderId = rechargeOrderId,
            RedirectUrl = redirectUrl
        };
    }
}
