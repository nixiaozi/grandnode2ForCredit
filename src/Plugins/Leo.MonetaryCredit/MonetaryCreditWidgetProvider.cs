using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Interfaces.Common.Localization;

namespace Leo.MonetaryCredit;

/// <summary>
///     Widget provider - adds "My Credits" link to customer account navigation
/// </summary>
public class MonetaryCreditWidgetProvider : IWidgetProvider
{
    private readonly ITranslationService _translationService;

    public MonetaryCreditWidgetProvider(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    /// <summary>
    ///     Configuration URL (not used for widget, but required by interface)
    /// </summary>
    public string ConfigurationUrl => string.Empty;

    /// <summary>
    ///     System name of the provider
    /// </summary>
    public string SystemName => "Leo.MonetaryCredit.Widget";

    /// <summary>
    ///     Friendly name for display
    /// </summary>
    public string FriendlyName => _translationService.GetResource("Leo.MonetaryCredit.MyCredits");

    /// <summary>
    ///     Display order priority
    /// </summary>
    public int Priority => 10;

    /// <summary>
    ///     Limited to specific stores (empty = all stores)
    /// </summary>
    public IList<string> LimitedToStores => new List<string>();

    /// <summary>
    ///     Limited to specific customer groups (empty = all customers)
    /// </summary>
    public IList<string> LimitedToGroups => new List<string>();

    /// <summary>
    ///     Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>Widget zones list</returns>
    public async Task<IList<string>> GetWidgetZones()
    {
        return await Task.FromResult(new List<string>
        {
            "account_navigation_after"
        });
    }

    /// <summary>
    ///     Gets the ViewComponent name for rendering in public store
    /// </summary>
    /// <param name="widgetZone">The widget zone</param>
    /// <returns>ViewComponent name</returns>
    public Task<string> GetPublicViewComponentName(string widgetZone)
    {
        return Task.FromResult("MonetaryCreditNavigation");
    }
}
