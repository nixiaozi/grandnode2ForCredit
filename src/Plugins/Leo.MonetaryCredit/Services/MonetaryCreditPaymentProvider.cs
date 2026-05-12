using Grand.Business.Core.Enums.Checkout;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Utilities.Checkout;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Services;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Monetary Credit payment provider - pays with user balance
/// </summary>
public class MonetaryCreditPaymentProvider(
    IUserAccountService userAccountService,
    ITranslationService translationService,
    MonetaryCreditSettings settings)
    : IPaymentProvider
{
    public string ConfigurationUrl => MonetaryCreditDefaults.ConfigurationUrl;
    public string SystemName => MonetaryCreditDefaults.ProviderSystemName;
    public string FriendlyName => translationService.GetResource(MonetaryCreditDefaults.FriendlyName);
    public int Priority => settings.DisplayOrder;
    public IList<string> LimitedToStores => [];
    public IList<string> LimitedToGroups => [];
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
    public string LogoURL => "/Plugins/Leo.MonetaryCredit/logo.png";

    public Task<PaymentTransaction> InitPaymentTransaction()
    {
        return Task.FromResult<PaymentTransaction>(null);
    }

    /// <summary>
    ///     Process payment - deduct buyer's balance
    ///     If balance is insufficient, payment fails directly (no combined payment)
    /// </summary>
    public async Task<ProcessPaymentResult> ProcessPayment(PaymentTransaction paymentTransaction)
    {
        var result = new ProcessPaymentResult();

        try
        {
            var customerId = paymentTransaction.CustomerId;
            var amount = (decimal)paymentTransaction.TransactionAmount;

            // Check balance
            var hasBalance = await userAccountService.HasSufficientBalanceAsync(customerId, amount);
            if (!hasBalance)
            {
                var account = await userAccountService.GetAccountAsync(customerId);
                var currentBalance = account?.Balance ?? 0m;
                result.AddError($"余额不足。当前余额: {currentBalance:F2} 元, 需要支付: {amount:F2} 元");
                return result;
            }

            // Deduct balance
            await userAccountService.DeductBalanceAsync(
                customerId,
                amount,
                paymentTransaction.OrderCode,
                $"订单支付扣除余额，金额: {amount} 元");

            result.NewPaymentTransactionStatus = TransactionStatus.Paid;
        }
        catch (Exception ex)
        {
            result.AddError($"余额支付失败: {ex.Message}");
        }

        return result;
    }

    public Task PostProcessPayment(PaymentTransaction paymentTransaction)
    {
        // Credits distribution is handled by MonetaryCreditOrderPaidHandler (OrderPaidEvent)
        return Task.CompletedTask;
    }

    public Task<string> PostRedirectPayment(PaymentTransaction paymentTransaction)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
    {
        // Could hide if customer has no balance, but that's optional
        return Task.FromResult(false);
    }

    public Task<double> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
    {
        // No additional fee for balance payment
        return Task.FromResult(0.0);
    }

    public Task<CapturePaymentResult> Capture(PaymentTransaction paymentTransaction)
    {
        var result = new CapturePaymentResult();
        result.AddError("Capture method not supported");
        return Task.FromResult(result);
    }

    public Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
    {
        // TODO: Implement refund logic (return balance to buyer, reverse credits)
        var result = new RefundPaymentResult();
        result.AddError("Refund method not supported");
        return Task.FromResult(result);
    }

    public Task<VoidPaymentResult> Void(PaymentTransaction paymentTransaction)
    {
        var result = new VoidPaymentResult();
        result.AddError("Void method not supported");
        return Task.FromResult(result);
    }

    public Task CancelPayment(PaymentTransaction paymentTransaction)
    {
        paymentTransaction.TransactionStatus = TransactionStatus.Canceled;
        return Task.CompletedTask;
    }

    public Task<bool> CanRePostRedirectPayment(PaymentTransaction paymentTransaction)
    {
        return Task.FromResult(false);
    }

    public Task<IList<string>> ValidatePaymentForm(IDictionary<string, string> model)
    {
        return Task.FromResult<IList<string>>([]);
    }

    public Task<PaymentTransaction> SavePaymentInfo(IDictionary<string, string> model)
    {
        return Task.FromResult<PaymentTransaction>(null);
    }

    public Task<string> GetControllerRouteName()
    {
        return Task.FromResult("Plugin.MonetaryCredit");
    }

    public Task<bool> SupportCapture()
    {
        return Task.FromResult(false);
    }

    public Task<bool> SupportPartiallyRefund()
    {
        return Task.FromResult(false);
    }

    public Task<bool> SupportRefund()
    {
        return Task.FromResult(false);
    }

    public Task<bool> SupportVoid()
    {
        return Task.FromResult(false);
    }

    public Task<bool> SkipPaymentInfo()
    {
        return Task.FromResult(settings.SkipPaymentInfo);
    }

    public Task<string> Description()
    {
        return Task.FromResult(settings.DescriptionText);
    }
}
