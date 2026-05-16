using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Infrastructure;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Models;
using Leo.MonetaryCredit.Services;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leo.MonetaryCredit.Areas.Admin.Controllers;

/// <summary>
///     Admin controller for Monetary Credit - configuration and recharge management
/// </summary>
[AuthorizeAdmin]
[Area("Admin")]
public class MonetaryCreditController(
    IContextAccessor contextAccessor,
    ISettingService settingService,
    IUserAccountService userAccountService,
    ISystemAccountService systemAccountService,
    ISystemAccountTransactionService systemAccountTransactionService,
    IRechargeService rechargeService,
    ICustomerService customerService)
    : BaseController
{
    /// <summary>
    ///     Configure payment settings
    /// </summary>
    public async Task<IActionResult> Configure()
    {
        var settings = await settingService.LoadSetting<Domain.MonetaryCreditSettings>();

        var model = new ConfigurationModel
        {
            DisplayOrder = settings.DisplayOrder,
            DescriptionText = settings.DescriptionText,
            MaxRechargeAmount = settings.MaxRechargeAmount,
            TransactionFeeRate = settings.TransactionFeeRate,
            SkipPaymentInfo = settings.SkipPaymentInfo
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var settings = await settingService.LoadSetting<Domain.MonetaryCreditSettings>();

        settings.DisplayOrder = model.DisplayOrder;
        settings.DescriptionText = model.DescriptionText;
        settings.MaxRechargeAmount = model.MaxRechargeAmount;
        settings.TransactionFeeRate = model.TransactionFeeRate;
        settings.SkipPaymentInfo = model.SkipPaymentInfo;

        await settingService.SaveSetting(settings);

        return RedirectToAction("Configure");
    }

    /// <summary>
    ///     Recharge order list
    /// </summary>
    public async Task<IActionResult> RechargeList(int page = 0, RechargeStatus? status = null)
    {
        var orders = await rechargeService.GetRechargeOrdersAsync(page, 20, status);
        var list = orders.Select(o => new RechargeListModel
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.CustomerId, // Will be resolved by customer service
            Amount = o.Amount,
            RechargeType = o.RechargeType,
            Status = o.Status,
            CreatedByOperatorName = o.CreatedByOperatorName,
            RejectionReason = o.RejectionReason,
            CreatedOnUtc = o.CreatedOnUtc,
            CompletedOnUtc = o.CompletedOnUtc
        }).ToList();

        // Resolve customer names
        foreach (var item in list)
        {
            var customer = await customerService.GetCustomerById(item.CustomerId);
            item.CustomerName = customer?.Email ?? item.CustomerId;
        }

        return View(list);
    }

    /// <summary>
    ///     Create recharge order form
    /// </summary>
    public IActionResult CreateRecharge()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecharge(CreateRechargeModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var currentUser = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            await rechargeService.CreateBackendRechargeOrderAsync(
                model.CustomerId,
                model.Amount,
                currentUser.Id,
                currentUser.Email,
                model.Remark);

            return RedirectToAction("RechargeList");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    /// <summary>
    ///     Operator approves a recharge order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> OperatorApprove(string id)
    {
        var currentUser = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            await rechargeService.OperatorApproveAsync(id, currentUser.Id);
            return RedirectToAction("RechargeList");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction("RechargeList");
        }
    }

    /// <summary>
    ///     Admin approves a recharge order (final approval - balance added)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AdminApprove(string id)
    {
        var currentUser = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            await rechargeService.AdminApproveAsync(id, currentUser.Id);
            return RedirectToAction("RechargeList");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction("RechargeList");
        }
    }

    /// <summary>
    ///     Reject a recharge order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reject(string id, string reason)
    {
        var currentUser = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            await rechargeService.RejectAsync(id, currentUser.Id, reason);
            return RedirectToAction("RechargeList");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction("RechargeList");
        }
    }

    /// <summary>
    ///     View system account balance (transaction fees collected)
    ///     Also shows last 10 transactions as summary.
    /// </summary>
    public async Task<IActionResult> SystemAccount()
    {
        var account = await systemAccountService.GetOrCreateSystemAccountAsync();

        // Load last 10 transactions for summary
        var recentTransactions = await systemAccountTransactionService.GetTransactionsAsync(pageIndex: 0, pageSize: 10);
        ViewBag.RecentTransactions = recentTransactions;

        return View(account);
    }

    /// <summary>
    ///     Full paged list of system account transactions
    /// </summary>
    public async Task<IActionResult> SystemAccountTransactions(int page = 0)
    {
        var transactions = await systemAccountTransactionService.GetTransactionsAsync(page, 20);
        return View(transactions);
    }
}
