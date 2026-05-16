using Grand.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Leo.MonetaryCredit.Models.User;

namespace Leo.MonetaryCredit.Components;

/// <summary>
///     ViewComponent - renders "My Credits" navigation link in customer account
/// </summary>
[ViewComponent(Name = "MonetaryCreditNavigation")]
public class MonetaryCreditNavigationViewComponent : ViewComponent
{
    private readonly IContextAccessor _contextAccessor;
    private readonly Services.IUserAccountService _userAccountService;

    public MonetaryCreditNavigationViewComponent(
        IContextAccessor contextAccessor,
        Services.IUserAccountService userAccountService)
    {
        _contextAccessor = contextAccessor;
        _userAccountService = userAccountService;
    }

    /// <summary>
    ///     Invoke the ViewComponent
    /// </summary>
    /// <param name="widgetZone">The widget zone</param>
    /// <param name="additionalData">Additional data (CustomerNavigationModel)</param>
    /// <returns>View component result</returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
    {
        var customer = _contextAccessor.WorkContext.CurrentCustomer;
        
        // Get user account to check if they have credits
        var account = await _userAccountService.GetOrCreateAccountAsync(customer.Id);
        
        // Create model for the view
        var model = new MonetaryCreditNavigationModel
        {
            CustomerId = customer.Id,
            HasCredits = account.Balance > 0 || account.ShoppingCredits > 0 || 
                        account.SalesCredits > 0 || account.ActivityCredits > 0,
            TotalBalance = account.Balance,
            AccountUrl = "/MonetaryCredit/Account"
        };

        return View(model);
    }
}
