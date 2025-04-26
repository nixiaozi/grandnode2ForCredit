using Grand.Infrastructure;
using Grand.Web.Vendor.Interfaces;
using Leo.ReviewProductPublish.Services;

namespace Leo.ReviewProductPublish
{
    public class StartupApplication : IStartupApplication
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProductViewModelService, VendorProductViewModelService>();
        }

        public int Priority => 110;

        public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
        {
        }

        public bool BeforeConfigure => false;


    }
}
