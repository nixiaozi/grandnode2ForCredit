namespace Leo.MonetaryCredit;

/// <summary>
///     Monetary Credit payment plugin constants
/// </summary>
public static class MonetaryCreditDefaults
{
    public const string ProviderSystemName = "Leo.MonetaryCredit";
    public const string FriendlyName = "Leo.MonetaryCredit.FriendlyName";
    public const string ConfigurationUrl = "/Admin/MonetaryCredit/Configure";

    // User account page
    public const string UserAccountUrl = "/MonetaryCredit/Account";

    // Admin recharge management
    public const string RechargeListUrl = "/Admin/MonetaryCredit/RechargeList";

    // MongoDB collection names (auto from class names, listed here for reference)
    public const string UserAccountCollection = "UserAccount";
    public const string SystemAccountCollection = "SystemAccount";
    public const string RechargeOrderCollection = "RechargeOrder";
    public const string CreditRecordCollection = "CreditRecord";
}
