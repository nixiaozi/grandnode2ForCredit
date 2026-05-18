using Grand.Domain;
using Leo.MonetaryCredit.Domain;
using Leo.MonetaryCredit.Models;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Recharge service with approval workflow
/// </summary>
public interface IRechargeService
{
    /// <summary>
    ///     Create a new backend recharge order (Operator → Admin approval)
    /// </summary>
    Task<RechargeOrder> CreateBackendRechargeOrderAsync(string customerId, decimal amount, string operatorId, string operatorName, string? remark);

    /// <summary>
    ///     Create a new frontend recharge order (status: WaitingPayment)
    /// </summary>
    Task<RechargeOrder> CreateFrontendRechargeOrderAsync(string customerId, decimal amount);

    /// <summary>
    ///     Complete a frontend recharge order after successful payment
    ///     Auto-approves and adds balance
    /// </summary>
    Task<RechargeOrder> CompleteRechargeAfterPaymentAsync(string rechargeOrderId);

    /// <summary>
    ///     Mark a recharge order as failed (payment failed or cancelled)
    /// </summary>
    Task<RechargeOrder> FailRechargeAsync(string rechargeOrderId, string reason);

    /// <summary>
    ///     Operator approves a recharge order (Level 1)
    /// </summary>
    Task<RechargeOrder> OperatorApproveAsync(string rechargeOrderId, string operatorId);

    /// <summary>
    ///     Admin approves a recharge order (Level 2) - balance is actually added
    /// </summary>
    Task<RechargeOrder> AdminApproveAsync(string rechargeOrderId, string adminId);

    /// <summary>
    ///     Reject a recharge order (at any level)
    /// </summary>
    Task<RechargeOrder> RejectAsync(string rechargeOrderId, string rejectorId, string reason);

    /// <summary>
    ///     Get recharge order by id
    /// </summary>
    Task<RechargeOrder?> GetRechargeOrderAsync(string rechargeOrderId);

    /// <summary>
    ///     Get recharge orders with paging and filtering
    /// </summary>
    Task<IPagedList<RechargeOrder>> GetRechargeOrdersAsync(
        int pageIndex = 0,
        int pageSize = 20,
        RechargeStatus? status = null,
        string? customerId = null);

    /// <summary>
    ///     Get recharge orders for a specific customer
    /// </summary>
    Task<IList<RechargeOrder>> GetCustomerRechargeOrdersAsync(string customerId);

    /// <summary>
    ///     Update a recharge order (internal use)
    /// </summary>
    Task<RechargeOrder> UpdateRechargeOrderAsync(RechargeOrder order);
}
