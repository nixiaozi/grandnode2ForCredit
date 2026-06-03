using Grand.Business.Core.Interfaces.Checkout.Payments;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Service for handling payment during recharge operations
/// </summary>
public interface IRechargePaymentService
{
    /// <summary>
    ///     Get available payment methods for recharge (excludes self)
    /// </summary>
    Task<IList<PaymentMethodModel>> GetAvailablePaymentMethodsAsync();

/// <summary>
///     Create a PaymentTransaction for recharge and get the redirect URL.
///     Returns a result containing the payment gateway redirect URL (may be empty for offline methods like COD).
/// </summary>
/// <param name="rechargeOrderId">Recharge order id</param>
/// <param name="paymentMethodSystemName">Selected payment method</param>
/// <returns>Result containing the redirect URL (empty if no redirect needed)</returns>
Task<RechargePaymentResult> CreatePaymentAndRedirectAsync(string rechargeOrderId, string paymentMethodSystemName);
}

/// <summary>
///     Payment method info for display
/// </summary>
public class PaymentMethodModel
{
    public string SystemName { get; set; }
    public string FriendlyName { get; set; }
    public string LogoUrl { get; set; }
    public string Description { get; set; }
}

/// <summary>
///     Result of CreatePaymentAndRedirectAsync
/// </summary>
public class RechargePaymentResult
{
    /// <summary>The recharge order id</summary>
    public string RechargeOrderId { get; set; }

    /// <summary>
    ///     The URL to redirect the user to for payment.
    ///     Empty or null if the payment method does not require an external redirect (e.g. CashOnDelivery).
    /// </summary>
    public string RedirectUrl { get; set; }

    /// <summary>True when <see cref="RedirectUrl"/> is a valid external payment gateway URL</summary>
    public bool HasRedirectUrl => !string.IsNullOrWhiteSpace(RedirectUrl);
}
