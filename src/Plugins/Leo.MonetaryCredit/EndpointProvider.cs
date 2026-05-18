using Grand.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Leo.MonetaryCredit;

/// <summary>
///     Endpoint provider - registers routes
/// </summary>
public class EndpointProvider : IEndpointProvider
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // PaymentInfo page (public)
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.PaymentInfo",
            "Plugins/MonetaryCredit/PaymentInfo",
            new { controller = "MonetaryCredit", action = "PaymentInfo", area = "" }
        );

        // User account page (public)
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.Account",
            "MonetaryCredit/Account",
            new { controller = "MonetaryCredit", action = "Account", area = "" }
        );

        // User account transaction history
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.Transactions",
            "MonetaryCredit/Transactions",
            new { controller = "MonetaryCredit", action = "Transactions", area = "" }
        );

        // Recharge payment method selection
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.RechargePaymentMethods",
            "MonetaryCredit/RechargePaymentMethods/{orderId}",
            new { controller = "MonetaryCredit", action = "RechargePaymentMethods", area = "" }
        );

        // Recharge redirect to payment gateway
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.RechargeRedirect",
            "MonetaryCredit/RechargeRedirect",
            new { controller = "MonetaryCredit", action = "RechargeRedirect", area = "" }
        );

        // Recharge payment return
        endpointRouteBuilder.MapControllerRoute("Plugin.MonetaryCredit.RechargeReturn",
            "MonetaryCredit/RechargeReturn",
            new { controller = "MonetaryCredit", action = "RechargeReturn", area = "" }
        );
    }

    public int Priority => 0;
}
