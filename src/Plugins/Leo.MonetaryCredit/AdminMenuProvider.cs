using Grand.Web.Common.Menu;
using Microsoft.Extensions.Localization;

namespace Leo.MonetaryCredit;

/// <summary>
///     Registers admin menu entries for the Monetary Credit plugin.
///     Adds a top-level group under "Third party plugins" with sub-items
///     for Configure, RechargeList, CreateRecharge and SystemAccount.
/// </summary>
public class AdminMenuProvider : IAdminMenuProvider
{
    // ── IProvider ────────────────────────────────────────────────────────────
    public string ConfigurationUrl => "";
    public string SystemName => "Leo.MonetaryCredit.AdminMenu";
    public string FriendlyName => "Monetary Credit Menu";
    public int Priority => 0;
    public IList<string> LimitedToStores => Array.Empty<string>();
    public IList<string> LimitedToGroups => Array.Empty<string>();

    // ── IAdminMenuProvider ───────────────────────────────────────────────────
    public Task ManageSiteMap(SiteMapNode rootNode)
    {
        // Locate the "Third party plugins" anchor node (DisplayOrder = 12)
        var pluginNode = rootNode.ChildNodes
            .FirstOrDefault(x => x.SystemName == "Third party plugins");

        if (pluginNode == null)
            return Task.CompletedTask;

        // ── Parent group for Monetary Credit ─────────────────────────────────
        var creditGroup = new SiteMapNode
        {
            SystemName = "Leo.MonetaryCredit",
            ResourceName = "货币积分",
            IconClass = "fa fa-credit-card",
            Visible = true,
            PermissionNames = new List<string> { "ManagePlugins" },
            ChildNodes = new List<SiteMapNode>
            {
                new()
                {
                    SystemName     = "Leo.MonetaryCredit.Configure",
                    ResourceName   = "插件设置",
                    ControllerName = "MonetaryCredit",
                    ActionName     = "Configure",
                    IconClass      = "fa fa-cog",
                    Visible        = true,
                    PermissionNames = new List<string> { "ManagePlugins" }
                },
                new()
                {
                    SystemName     = "Leo.MonetaryCredit.RechargeList",
                    ResourceName   = "充值订单",
                    ControllerName = "MonetaryCredit",
                    ActionName     = "RechargeList",
                    IconClass      = "fa fa-list",
                    Visible        = true,
                    PermissionNames = new List<string> { "ManagePlugins" }
                },
                new()
                {
                    SystemName     = "Leo.MonetaryCredit.CreateRecharge",
                    ResourceName   = "手动充值",
                    ControllerName = "MonetaryCredit",
                    ActionName     = "CreateRecharge",
                    IconClass      = "fa fa-plus",
                    Visible        = true,
                    PermissionNames = new List<string> { "ManagePlugins" }
                },
                new()
                {
                    SystemName     = "Leo.MonetaryCredit.SystemAccount",
                    ResourceName   = "系统账户",
                    ControllerName = "MonetaryCredit",
                    ActionName     = "SystemAccount",
                    IconClass      = "fa fa-bank",
                    Visible        = true,
                    PermissionNames = new List<string> { "ManagePlugins" }
                },
                new()
                {
                    SystemName     = "Leo.MonetaryCredit.SystemAccountTransactions",
                    ResourceName   = "交易明细",
                    ControllerName = "MonetaryCredit",
                    ActionName     = "SystemAccountTransactions",
                    IconClass      = "fa fa-exchange",
                    Visible        = true,
                    PermissionNames = new List<string> { "ManagePlugins" }
                }
            }
        };

        pluginNode.ChildNodes.Add(creditGroup);
        return Task.CompletedTask;
    }
}
