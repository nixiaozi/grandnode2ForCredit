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
    }

    public int Priority => 0;
}
