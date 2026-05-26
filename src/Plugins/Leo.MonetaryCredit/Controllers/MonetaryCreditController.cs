using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Domain.Payments;
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
    IRechargePaymentService rechargePaymentService,
    IOrderService orderService,
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
    ///     User account page - shows balance, credits and recharge form with quota
    /// </summary>
    public async Task<IActionResult> Account()
    {
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        var account = await userAccountService.GetOrCreateAccountAsync(customer.Id);
        var settings = await settingsService.GetSettingsAsync();

        // Load recent recharge orders for this customer
        var rechargeOrders = await rechargeService.GetCustomerRechargeOrdersAsync(customer.Id);

        var model = new UserAccountViewModel
        {
            Balance = account.Balance,
            ShoppingCredits = account.ShoppingCredits,
            SalesCredits = account.SalesCredits,
            ActivityCredits = account.ActivityCredits,
            TotalRecharged = account.TotalRecharged,
            MaxRechargeAmount = settings.MaxRechargeAmount,
            RechargeOrders = rechargeOrders.Select(o => new RechargeOrderSummaryModel
            {
                Id = o.Id,
                Amount = o.Amount,
                RechargeType = o.RechargeType,
                Status = o.Status,
                StatusName = o.Status switch
                {
                    RechargeStatus.WaitingPayment => "待支付",
                    RechargeStatus.Pending => "待审批",
                    RechargeStatus.OperatorApproved => "操作员已审批",
                    RechargeStatus.AdminApproved => "已完成",
                    RechargeStatus.Rejected => "已拒绝",
                    _ => o.Status.ToString()
                },
                CreatedOnUtc = o.CreatedOnUtc,
                CompletedOnUtc = o.CompletedOnUtc,
                RejectionReason = o.RejectionReason,
                PaymentMethodSystemName = o.PaymentMethodSystemName
            }).ToList()
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
    ///     Step 1: Create recharge order and redirect to payment method selection
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Recharge(decimal amount)
    {
        if (amount <= 0)
            return Json(new { success = false, message = "充值金额必须大于零" });

        var customer = contextAccessor.WorkContext.CurrentCustomer;

        try
        {
            var order = await rechargeService.CreateFrontendRechargeOrderAsync(customer.Id, amount);
            // Redirect to payment method selection page
            return Json(new { success = true, redirectUrl = $"/MonetaryCredit/RechargePaymentMethods/{order.Id}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    ///     Step 2: Show available payment methods for recharge
    /// </summary>
    public async Task<IActionResult> RechargePaymentMethods(string orderId)
    {
        var order = await rechargeService.GetRechargeOrderAsync(orderId);
        if (order == null || order.Status != RechargeStatus.WaitingPayment)
        {
            return RedirectToAction("Account");
        }

        // Verify ownership
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        if (order.CustomerId != customer.Id)
        {
            return RedirectToAction("Account");
        }

        var paymentMethods = await rechargePaymentService.GetAvailablePaymentMethodsAsync();
        if (paymentMethods.Count == 0)
        {
            // No payment methods available, show error
            ViewBag.ErrorMessage = "当前没有可用的支付方式，请联系管理员配置支付方式。";
            return View("RechargePaymentMethods", new RechargePaymentMethodsViewModel
            {
                OrderId = orderId,
                Amount = order.Amount,
                PaymentMethods = new List<PaymentMethodModel>()
            });
        }

        var model = new RechargePaymentMethodsViewModel
        {
            OrderId = orderId,
            Amount = order.Amount,
            PaymentMethods = paymentMethods
        };

        return View(model);
    }

    /// <summary>
    ///     Step 3: Redirect to selected payment gateway
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RechargeRedirect(string orderId, string paymentMethodSystemName)
    {
        var order = await rechargeService.GetRechargeOrderAsync(orderId);
        if (order == null || order.Status != RechargeStatus.WaitingPayment)
            return RedirectToAction("Account");

        var customer = contextAccessor.WorkContext.CurrentCustomer;
        if (order.CustomerId != customer.Id)
            return RedirectToAction("Account");

        try
        {
            return RedirectToAction("RechargeReturn", new { orderId});
        }
        catch (Exception ex)
        {
            // Mark recharge as failed
            await rechargeService.FailRechargeAsync(orderId, $"支付跳转失败: {ex.Message}");
            return RedirectToAction("RechargeReturn", new { orderId, success = false, message = ex.Message });
        }
    }

    /// <summary>
    ///     Step 4: Payment return page - user is redirected here after payment
    ///     Returns a "pending" page that polls via JS until status is confirmed
    /// </summary>
    public async Task<IActionResult> RechargeReturn(string orderId, bool? success, string? message)
    {
        var order = await rechargeService.GetRechargeOrderAsync(orderId);
        if (order == null)
            return RedirectToAction("Account");

        var model = new RechargeReturnViewModel
        {
            OrderId = orderId,
            Amount = order.Amount,
            Success = false,
            IsPending = false
        };

        // If already completed by webhook before user lands here
        if (order.Status == RechargeStatus.AdminApproved)
        {
            model.Success = true;
            model.Message = "充值成功！";
            return View(model);
        }

        if (order.Status == RechargeStatus.Rejected)
        {
            model.Success = false;
            model.Message = order.RejectionReason ?? "充值失败";
            return View(model);
        }

        // Still waiting for payment callback — let JS poll
        if (order.Status == RechargeStatus.WaitingPayment)
        {
            // Try one immediate check on the virtual order
            if (!string.IsNullOrEmpty(order.VirtualOrderId))
            {
                try
                {
                    var virtualOrder = await orderService.GetOrderById(order.VirtualOrderId);
                    if (virtualOrder?.PaymentStatusId == PaymentStatus.Paid)
                    {
                        await rechargeService.CompleteRechargeAfterPaymentAsync(orderId);
                        model.Success = true;
                        model.Message = "充值成功！";
                        return View(model);
                    }
                }
                catch { /* ignore */ }
            }

            // Not confirmed yet — show loading/polling page
            model.IsPending = true;
            model.Message = message ?? "正在等待支付确认...";
            return View(model);
        }

        model.Message = message ?? "支付未完成，您可以稍后在充值记录中重新发起支付";
        return View(model);
    }

    /// <summary>
    ///     Polling API - called by JS every few seconds to check recharge status
    /// </summary>
    public async Task<IActionResult> RechargeStatusCheck(string orderId)
    {
        var order = await rechargeService.GetRechargeOrderAsync(orderId);
        if (order == null)
            return Json(new { status = "notfound" });

        // Verify ownership
        var customer = contextAccessor.WorkContext.CurrentCustomer;
        if (order.CustomerId != customer.Id)
            return Json(new { status = "forbidden" });

        if (order.Status == RechargeStatus.AdminApproved)
            return Json(new { status = "success", amount = order.Amount });

        if (order.Status == RechargeStatus.Rejected)
            return Json(new { status = "failed", message = order.RejectionReason ?? "充值失败" });

        // Still WaitingPayment — try virtual order
        if (order.Status == RechargeStatus.WaitingPayment && !string.IsNullOrEmpty(order.VirtualOrderId))
        {
            try
            {
                var virtualOrder = await orderService.GetOrderById(order.VirtualOrderId);
                if (virtualOrder?.PaymentStatusId == PaymentStatus.Paid)
                {
                    await rechargeService.CompleteRechargeAfterPaymentAsync(orderId);
                    return Json(new { status = "success", amount = order.Amount });
                }
            }
            catch { /* ignore */ }
        }

        return Json(new { status = "pending" });
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

/// <summary>
///     Recharge payment method selection model
/// </summary>
public class RechargePaymentMethodsViewModel
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public IList<PaymentMethodModel> PaymentMethods { get; set; } = [];
}

/// <summary>
///     Recharge return result model
/// </summary>
public class RechargeReturnViewModel
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public bool Success { get; set; }
    /// <summary>True when payment has been initiated but confirmation is still pending</summary>
    public bool IsPending { get; set; }
    public string Message { get; set; }
}
