namespace Leo.MonetaryCredit.Models.User;

/// <summary>
///     Model for MonetaryCreditNavigation ViewComponent
/// </summary>
public class MonetaryCreditNavigationModel
{
    /// <summary>
    ///     Customer ID
    /// </summary>
    public string CustomerId { get; set; }

    /// <summary>
    ///     Whether the customer has any credits
    /// </summary>
    public bool HasCredits { get; set; }

    /// <summary>
    ///     Total balance amount
    /// </summary>
    public decimal TotalBalance { get; set; }

    /// <summary>
    ///     URL to the account page
    /// </summary>
    public string AccountUrl { get; set; } = "/MonetaryCredit/Account";
}
