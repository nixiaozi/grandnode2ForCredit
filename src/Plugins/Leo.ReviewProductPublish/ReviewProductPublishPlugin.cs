using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Plugins;

namespace Leo.ReviewProductPublish
{
    public class DiscountPlugin(
    IPluginTranslateResource pluginTranslateResource)
    : BasePlugin, IPlugin
    {

        public override async Task Install()
        {
            //CustomerGroup
            //await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.DiscountRules.CustomerGroups.Fields.CustomerGroup", "Required customer group");
            //await pluginTranslateResource.AddOrUpdatePluginTranslateResource("Plugins.DiscountRules.CustomerGroups.Fields.CustomerGroup.Hint",
            //    "Discount will be applied if customer is in the selected customer group.");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            //CustomerGroup
            //await pluginTranslateResource.DeletePluginTranslationResource("Plugins.DiscountRules.CustomerGroups.Fields.CustomerGroup");
            //await pluginTranslateResource.DeletePluginTranslationResource("Plugins.DiscountRules.CustomerGroups.Fields.CustomerGroup.Hint");

           
            await base.Uninstall();
        }
    }
}
