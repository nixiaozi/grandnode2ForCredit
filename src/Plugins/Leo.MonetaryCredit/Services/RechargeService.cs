using Grand.Data;
using Grand.Domain;
using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Services;

/// <summary>
///     Recharge service implementation with two-level approval workflow
/// </summary>
public class RechargeService(
    IRepository<RechargeOrder> rechargeOrderRepository,
    IRepository<UserAccount> userAccountRepository,
    IUserAccountService userAccountService,
    IMonetaryCreditSettingsService settingsService)
    : IRechargeService
{
    public async Task<RechargeOrder> CreateBackendRechargeOrderAsync(string customerId, decimal amount, string operatorId, string operatorName, string? remark)
    {
        if (amount <= 0)
            throw new ArgumentException("充值金额必须大于零");

        // Check max recharge limit
        var settings = await settingsService.GetSettingsAsync();
        if (settings.MaxRechargeAmount > 0)
        {
            var account = await userAccountService.GetOrCreateAccountAsync(customerId);
            if (account.TotalRecharged + amount > settings.MaxRechargeAmount)
                throw new InvalidOperationException(
                    $"充值后将超出最大累计充值限额 {settings.MaxRechargeAmount} 元。已充值: {account.TotalRecharged} 元");
        }

        var order = new RechargeOrder
        {
            CustomerId = customerId,
            Amount = amount,
            RechargeType = RechargeType.Backend,
            Status = RechargeStatus.Pending,
            CreatedByOperatorId = operatorId,
            CreatedByOperatorName = operatorName,
            Remark = remark
        };

        return await rechargeOrderRepository.InsertAsync(order);
    }

    public async Task<RechargeOrder> CreateFrontendRechargeOrderAsync(string customerId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("充值金额必须大于零");

        // Check max recharge limit
        var settings = await settingsService.GetSettingsAsync();
        if (settings.MaxRechargeAmount > 0)
        {
            var account = await userAccountService.GetOrCreateAccountAsync(customerId);
            if (account.TotalRecharged + amount > settings.MaxRechargeAmount)
                throw new InvalidOperationException(
                    $"充值后将超出最大累计充值限额 {settings.MaxRechargeAmount} 元。已充值: {account.TotalRecharged} 元");
        }

        var order = new RechargeOrder
        {
            CustomerId = customerId,
            Amount = amount,
            RechargeType = RechargeType.Frontend,
            Status = RechargeStatus.WaitingPayment
        };

        return await rechargeOrderRepository.InsertAsync(order);
    }

    public async Task<RechargeOrder> CompleteRechargeAfterPaymentAsync(string rechargeOrderId)
    {
        var order = await rechargeOrderRepository.GetByIdAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (order.Status != RechargeStatus.WaitingPayment)
            throw new InvalidOperationException($"充值订单当前状态 {order.Status} 不允许完成支付");

        // Actually add the balance
        var account = await userAccountService.AddBalanceAsync(
            order.CustomerId,
            order.Amount,
            null,
            order.Id,
            $"在线充值 - 支付成功，充值 {order.Amount} 元",
            CreditTransactionType.Recharge);

        // Update total recharged (atomic)
        await userAccountRepository.IncField(account.Id, x => x.TotalRecharged, order.Amount);

        order.Status = RechargeStatus.AdminApproved;
        order.ApprovedByOperatorId = "PAYMENT";
        order.ApprovedByAdminId = "PAYMENT";
        order.OperatorApprovedOnUtc = DateTime.UtcNow;
        order.AdminApprovedOnUtc = DateTime.UtcNow;
        order.CompletedOnUtc = DateTime.UtcNow;

        return await rechargeOrderRepository.UpdateAsync(order);
    }

    public async Task<RechargeOrder> FailRechargeAsync(string rechargeOrderId, string reason)
    {
        var order = await rechargeOrderRepository.GetByIdAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (order.Status != RechargeStatus.WaitingPayment)
            throw new InvalidOperationException($"充值订单当前状态 {order.Status} 不允许标记失败");

        order.Status = RechargeStatus.Rejected;
        order.RejectionReason = reason;
        order.UpdatedOnUtc = DateTime.UtcNow;
        order.UpdatedBy = "SYSTEM";

        return await rechargeOrderRepository.UpdateAsync(order);
    }

    public async Task<RechargeOrder> OperatorApproveAsync(string rechargeOrderId, string operatorId)
    {
        var order = await rechargeOrderRepository.GetByIdAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (order.Status != RechargeStatus.Pending)
            throw new InvalidOperationException($"充值订单当前状态 {order.Status} 不允许操作员审批");

        order.Status = RechargeStatus.OperatorApproved;
        order.ApprovedByOperatorId = operatorId;
        order.OperatorApprovedOnUtc = DateTime.UtcNow;

        return await rechargeOrderRepository.UpdateAsync(order);
    }

    public async Task<RechargeOrder> AdminApproveAsync(string rechargeOrderId, string adminId)
    {
        var order = await rechargeOrderRepository.GetByIdAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (order.Status != RechargeStatus.OperatorApproved)
            throw new InvalidOperationException($"充值订单当前状态 {order.Status} 不允许管理员审批");

        // Actually add the balance
        var account = await userAccountService.AddBalanceAsync(
            order.CustomerId,
            order.Amount,
            null,
            order.Id,
            $"后台充值 - 审批通过，充值 {order.Amount} 元",
            CreditTransactionType.Recharge);

        // Update total recharged (atomic)
        await userAccountRepository.IncField(account.Id, x => x.TotalRecharged, order.Amount);

        order.Status = RechargeStatus.AdminApproved;
        order.ApprovedByAdminId = adminId;
        order.AdminApprovedOnUtc = DateTime.UtcNow;
        order.CompletedOnUtc = DateTime.UtcNow;

        return await rechargeOrderRepository.UpdateAsync(order);
    }

    public async Task<RechargeOrder> RejectAsync(string rechargeOrderId, string rejectorId, string reason)
    {
        var order = await rechargeOrderRepository.GetByIdAsync(rechargeOrderId)
            ?? throw new FileNotFoundException($"充值订单 {rechargeOrderId} 不存在");

        if (order.Status is RechargeStatus.AdminApproved or RechargeStatus.Rejected)
            throw new InvalidOperationException($"充值订单当前状态 {order.Status} 不允许拒绝");

        order.Status = RechargeStatus.Rejected;
        order.RejectionReason = reason;
        order.UpdatedOnUtc = DateTime.UtcNow;
        order.UpdatedBy = rejectorId;

        return await rechargeOrderRepository.UpdateAsync(order);
    }

    public async Task<RechargeOrder?> GetRechargeOrderAsync(string rechargeOrderId)
    {
        return await rechargeOrderRepository.GetByIdAsync(rechargeOrderId);
    }

    public async Task<IPagedList<RechargeOrder>> GetRechargeOrdersAsync(
        int pageIndex = 0,
        int pageSize = 20,
        RechargeStatus? status = null,
        string? customerId = null)
    {
        var query = rechargeOrderRepository.Table;

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(customerId))
            query = query.Where(x => x.CustomerId == customerId);

        query = query.OrderByDescending(x => x.CreatedOnUtc);

        return await PagedList<RechargeOrder>.Create(query, pageIndex, pageSize);
    }

    public async Task<IList<RechargeOrder>> GetCustomerRechargeOrdersAsync(string customerId)
    {
        return rechargeOrderRepository.Table
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(50)
            .ToList();
    }

    public async Task<RechargeOrder> UpdateRechargeOrderAsync(RechargeOrder order)
    {
        return await rechargeOrderRepository.UpdateAsync(order);
    }
}

/// <summary>
///     Settings service interface (extracted for DI)
/// </summary>
public interface IMonetaryCreditSettingsService
{
    Task<Domain.MonetaryCreditSettings> GetSettingsAsync();
}

public class MonetaryCreditSettingsService(
    Grand.Business.Core.Interfaces.Common.Configuration.ISettingService settingService)
    : IMonetaryCreditSettingsService
{
    public async Task<Domain.MonetaryCreditSettings> GetSettingsAsync()
    {
        return await settingService.LoadSetting<Domain.MonetaryCreditSettings>();
    }
}
