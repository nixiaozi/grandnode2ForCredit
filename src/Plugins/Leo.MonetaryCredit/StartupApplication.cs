using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Interfaces.System.ScheduleTasks;
using Grand.Infrastructure;
using Grand.Web.Common.Menu;
using Leo.MonetaryCredit.Infrastructure.Tasks;
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
        services.AddScoped<IRechargePaymentService, RechargePaymentService>();


        // Register MediatR notification handlers
        services.AddScoped<Infrastructure.Handler.MonetaryCreditOrderPaidHandler>();
        services.AddScoped<Infrastructure.Handler.MonetaryCreditOrderCancelledHandler>();
        services.AddScoped<Infrastructure.Handler.RechargeOrderPaidHandler>();

        // Pending credit service (holds credits during return window)
        services.AddScoped<IPendingCreditService, PendingCreditService>();

        // Scheduled task: release pending credits when return-window expires (runs daily)
        services.AddKeyedScoped<IScheduleTask, ReleasePendingCreditsTask>(ReleasePendingCreditsTask.TaskName);

        // Register admin menu provider
        services.AddScoped<IAdminMenuProvider, AdminMenuProvider>();

        // Register widget provider (adds "我的积分" link to account navigation)
        services.AddScoped<IWidgetProvider, MonetaryCreditWidgetProvider>();


        



    }

    public int Priority => 10;

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
        Console.WriteLine("Configuring Leo.MonetaryCredit plugin...");


        // application.Services.GetKeyedService(ReleasePendingCreditsTask.TaskName);
        IScheduleTaskService scheduleTaskService = (IScheduleTaskService)application.Services.GetService(typeof(IScheduleTaskService));

        var existingTask = scheduleTaskService.GetTaskByName(ReleasePendingCreditsTask.TaskName);
        existingTask.Wait();
        if (existingTask.Result == null)
        {
            var addtask = scheduleTaskService.InsertTask(new Grand.Domain.Tasks.ScheduleTask {
                ScheduleTaskName = ReleasePendingCreditsTask.TaskName,
                Enabled = true,
                StopOnError = false,
                TimeInterval = 1440  // run once per day (minutes)
            });

            addtask.Wait();
        }

    }

    public bool BeforeConfigure => false;
}
