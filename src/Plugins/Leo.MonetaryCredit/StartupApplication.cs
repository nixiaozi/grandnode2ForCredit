using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Infrastructure;
using Grand.Web.Common.Menu;
using Leo.MonetaryCredit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Leo.MonetaryCredit;

/// <summary>
///     Startup application - registers DI services
/// </summary>
public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register payment provider
        services.AddScoped<IPaymentProvider, MonetaryCreditPaymentProvider>();

        // Register domain services
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<ISystemAccountService, SystemAccountService>();
        services.AddScoped<IRechargeService, RechargeService>();
        services.AddScoped<IMonetaryCreditSettingsService, MonetaryCreditSettingsService>();
        services.AddScoped<ISystemAccountTransactionService, SystemAccountTransactionService>();


        // Register MediatR notification handler
        services.AddScoped<Infrastructure.Handler.MonetaryCreditOrderPaidHandler>();

        // Register admin menu provider
        services.AddScoped<IAdminMenuProvider, AdminMenuProvider>();
    }

    public int Priority => 10;

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public bool BeforeConfigure => false;
}
