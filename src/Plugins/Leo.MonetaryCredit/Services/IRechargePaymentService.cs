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
    ///     Create a PaymentTransaction for recharge and get the redirect URL
    /// </summary>
    /// <param name="rechargeOrderId">Recharge order id</param>
    /// <param name="paymentMethodSystemName">Selected payment method</param>
    /// <returns>Redirect URL to payment gateway</returns>
    Task<string> CreatePaymentAndRedirectAsync(string rechargeOrderId, string paymentMethodSystemName);
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
