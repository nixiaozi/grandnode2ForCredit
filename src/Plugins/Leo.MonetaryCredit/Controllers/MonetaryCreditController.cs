using Grand.Infrastructure;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Models;
using Leo.MonetaryCredit.Services;
using Grand.Web.Common.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Leo.MonetaryCredit.Controllers;

/// <summary>
///     Public-facing controller for Monetary Credit (PaymentInfo + User Account)
/// </summary>
public class MonetaryCreditController(
    IContextAccessor contextAccessor,
    IUserAccountService userAccountService,
    IRechargeService rechargeService,
    IMonetaryCreditSettingsService settingsService)
    : BasePaymentController
{
    /// <summary>
    ///     PaymentInfo page - displayed during checkout
    /// </summary>
    public async Task<IActionResult> PaymentInfo()
    {
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var account = await userAccountService.GetOrCreateAccountAsync(customer.Id);

        var model = new PaymentInfoModel
        {
            Balance = account.Balance,
            CustomerEmail = customer.Email
        };

        return View(model);
    }

    /// <summary>
    ///     User account page - shows balance and credits
    /// </summary>
    public async Task<IActionResult> Account()
    {
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var account = await userAccountService.GetOrCreateAccountAsync(customer.Id);
        var settings = await settingsService.GetSettingsAsync();

        var model = new UserAccountViewModel
        {
            Balance = account.Balance,
            ShoppingCredits = account.ShoppingCredits,
            SalesCredits = account.SalesCredits,
            ActivityCredits = account.ActivityCredits,
            TotalRecharged = account.TotalRecharged,
            MaxRechargeAmount = settings.MaxRechargeAmount
        };

        return View(model);
    }

    /// <summary>
    ///     Transaction history page
    /// </summary>
    public async Task<IActionResult> Transactions(int page = 0)
    {
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var records = await userAccountService.GetTransactionRecordsAsync(customer.Id, page, 20);

        var model = records.Select(r => new CreditRecordViewModel
        {
            CreatedOnUtc = r.CreatedOnUtc,
            TransactionTypeName = r.TransactionType switch
            {
                CreditTransactionType.Recharge => "充值",
                CreditTransactionType.PaymentDeduction => "支付扣款",
                CreditTransactionType.ShoppingCreditEarned => "购物积分",
                CreditTransactionType.PaymentReceived => "销售收款",
                CreditTransactionType.SalesCreditEarned => "销售积分",
                CreditTransactionType.ActivityCreditEarned => "活动积分",
                CreditTransactionType.Refund => "退款",
                CreditTransactionType.TransactionFee => "交易手续费",
                _ => r.TransactionType.ToString()
            },
            Amount = r.Amount,
            Description = r.Description,
            BalanceAfter = r.BalanceAfter
        }).ToList();

        return View(model);
    }

    /// <summary>
    ///     Frontend recharge (POST)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Recharge(decimal amount)
    {
        if (amount <= 0)
            return Json(new { success = false, message = "充值金额必须大于零" });

        var customer = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            await rechargeService.CreateFrontendRechargeOrderAsync(customer.Id, amount);
            return Json(new { success = true, message = "充值成功" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}

/// <summary>
///     Payment info model
/// </summary>
public class PaymentInfoModel
{
    public decimal Balance { get; set; }
    public string CustomerEmail { get; set; }
}
