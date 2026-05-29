using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.System.ScheduleTasks;
using Grand.Infrastructure.Plugins;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Infrastructure.Tasks;
using Leo.MonetaryCredit.Services;

namespace Leo.MonetaryCredit;

/// <summary>
///     Monetary Credit payment plugin
/// </summary>
public class MonetaryCreditPlugin(
    ISettingService settingService,
    IPluginTranslateResource pluginTranslateResource,
    IScheduleTaskService scheduleTaskService)
    : BasePlugin, IPlugin
{
    public override string ConfigurationUrl()
    {
        return MonetaryCreditDefaults.ConfigurationUrl;
    }

    public override async Task Install()
    {
        // Save default settings
        var settings = new MonetaryCreditSettings();
        await settingService.SaveSetting(settings);

        // Register language resources
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Leo.MonetaryCredit.FriendlyName", "余额支付 (Monetary Credit)");

        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.DescriptionText", "描述文本");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.DescriptionText.Hint", "在结算页面显示的描述信息");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.PaymentMethodDescription", "使用余额支付");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.DisplayOrder", "显示顺序");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.SkipPaymentInfo", "跳过支付信息页");

        // Settings
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.MaxRechargeAmount", "最大累计充值限额 (元)");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.MaxRechargeAmount.Hint", "每个用户最多可累计充值的金额(元)。0 表示不限制");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.TransactionFeeRate", "交易费率 (%)");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Payment.MonetaryCredit.TransactionFeeRate.Hint", "卖家收款时扣除的手续费比例(%)，默认 0.2%");

        // Admin recharge management
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Admin.MonetaryCredit.RechargeManagement", "充值管理");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Admin.MonetaryCredit.RechargeList", "充值记录");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Admin.MonetaryCredit.CreateRecharge", "创建充值");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Admin.MonetaryCredit.Approve", "审批");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.Admin.MonetaryCredit.Reject", "拒绝");

        // User account page
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.Account", "我的余额");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.Balance", "余额");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.ShoppingCredits", "购物积分");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.SalesCredits", "销售积分");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.ActivityCredits", "活动积分");
        await pluginTranslateResource.AddOrUpdatePluginTranslateResource(
            "Plugins.MonetaryCredit.TotalRecharged", "累计充值");

        // Register the daily "release pending credits" scheduled task in the database
        var existingTask = await scheduleTaskService.GetTaskByName(ReleasePendingCreditsTask.TaskName);
        if (existingTask == null)
        {
            await scheduleTaskService.InsertTask(new Grand.Domain.Tasks.ScheduleTask
            {
                ScheduleTaskName = ReleasePendingCreditsTask.TaskName,
                Enabled = true,
                StopOnError = false,
                TimeInterval = 1440  // run once per day (minutes)
            });
        }

        await base.Install();
    }

    public override async Task Uninstall()
    {
        // Delete settings
        await settingService.DeleteSetting<MonetaryCreditSettings>();

        // Delete all plugin translation resources
        var resourceKeys = new[]
        {
            "Leo.MonetaryCredit.FriendlyName",
            "Plugins.Payment.MonetaryCredit.DescriptionText",
            "Plugins.Payment.MonetaryCredit.DescriptionText.Hint",
            "Plugins.Payment.MonetaryCredit.PaymentMethodDescription",
            "Plugins.Payment.MonetaryCredit.DisplayOrder",
            "Plugins.Payment.MonetaryCredit.SkipPaymentInfo",
            "Plugins.Payment.MonetaryCredit.MaxRechargeAmount",
            "Plugins.Payment.MonetaryCredit.MaxRechargeAmount.Hint",
            "Plugins.Payment.MonetaryCredit.TransactionFeeRate",
            "Plugins.Payment.MonetaryCredit.TransactionFeeRate.Hint",
            "Plugins.Admin.MonetaryCredit.RechargeManagement",
            "Plugins.Admin.MonetaryCredit.RechargeList",
            "Plugins.Admin.MonetaryCredit.CreateRecharge",
            "Plugins.Admin.MonetaryCredit.Approve",
            "Plugins.Admin.MonetaryCredit.Reject",
            "Plugins.MonetaryCredit.Account",
            "Plugins.MonetaryCredit.Balance",
            "Plugins.MonetaryCredit.ShoppingCredits",
            "Plugins.MonetaryCredit.SalesCredits",
            "Plugins.MonetaryCredit.ActivityCredits",
            "Plugins.MonetaryCredit.TotalRecharged"
        };

        foreach (var key in resourceKeys)
        {
            await pluginTranslateResource.DeletePluginTranslationResource(key);
        }

        // Remove the scheduled task from database
        var task = await scheduleTaskService.GetTaskByName(ReleasePendingCreditsTask.TaskName);
        if (task != null)
            await scheduleTaskService.DeleteTask(task);

        await base.Uninstall();
    }
}
