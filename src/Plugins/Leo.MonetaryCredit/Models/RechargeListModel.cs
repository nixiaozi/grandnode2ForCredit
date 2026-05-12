using Leo.MonetaryCredit.Domain;

namespace Leo.MonetaryCredit.Models;

/// <summary>
///     Recharge list item model
/// </summary>
public class RechargeListModel
{
    public string Id { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public RechargeType RechargeType { get; set; }
    public RechargeStatus Status { get; set; }
    public string StatusName => Status switch
    {
        RechargeStatus.Pending => "待审批",
        RechargeStatus.OperatorApproved => "操作员已审批",
        RechargeStatus.AdminApproved => "已完成",
        RechargeStatus.Rejected => "已拒绝",
        _ => Status.ToString()
    };
    public string RechargeTypeName => RechargeType switch
    {
        RechargeType.Frontend => "用户充值",
        RechargeType.Backend => "后台充值",
        _ => RechargeType.ToString()
    };
    public string CreatedByOperatorName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
}
