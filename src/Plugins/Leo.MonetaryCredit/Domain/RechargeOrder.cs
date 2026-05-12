using Grand.Domain;

namespace Leo.MonetaryCredit.Domain;

/// <summary>
///     Recharge order with two-level approval workflow
///     Flow: Created → OperatorApproved → AdminApproved → Completed
///     Rejection at any level → Rejected
/// </summary>
public class RechargeOrder : BaseEntity
{
    /// <summary>
    ///     Target customer Id to recharge
    /// </summary>
    public string CustomerId { get; set; }

    /// <summary>
    ///     Recharge amount (in Yuan)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Recharge type: Frontend (user-initiated) or Backend (admin-initiated)
    /// </summary>
    public RechargeType RechargeType { get; set; }

    /// <summary>
    ///     Current approval status
    /// </summary>
    public RechargeStatus Status { get; set; } = RechargeStatus.Pending;

    /// <summary>
    ///     Operator who submitted/created this recharge order
    /// </summary>
    public string CreatedByOperatorId { get; set; }

    /// <summary>
    ///     Operator's name for display
    /// </summary>
    public string CreatedByOperatorName { get; set; }

    /// <summary>
    ///     Operator who approved at level 1
    /// </summary>
    public string? ApprovedByOperatorId { get; set; }

    /// <summary>
    ///     Admin who approved at level 2
    /// </summary>
    public string? ApprovedByAdminId { get; set; }

    /// <summary>
    ///     Rejection reason (if rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    ///     Admin remark / note
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    ///     Operator approval timestamp
    /// </summary>
    public DateTime? OperatorApprovedOnUtc { get; set; }

    /// <summary>
    ///     Admin approval timestamp
    /// </summary>
    public DateTime? AdminApprovedOnUtc { get; set; }

    /// <summary>
    ///     Completed timestamp (balance actually added)
    /// </summary>
    public DateTime? CompletedOnUtc { get; set; }
}

/// <summary>
///     Recharge type
/// </summary>
public enum RechargeType
{
    /// <summary>
    ///     User-initiated recharge from frontend
    /// </summary>
    Frontend = 0,

    /// <summary>
    ///     Admin-initiated recharge from backend (requires approval)
    /// </summary>
    Backend = 1
}

/// <summary>
///     Recharge approval status
/// </summary>
public enum RechargeStatus
{
    /// <summary>
    ///     Newly created, waiting for operator approval
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     Approved by operator, waiting for admin approval
    /// </summary>
    OperatorApproved = 1,

    /// <summary>
    ///     Approved by admin, recharge completed
    /// </summary>
    AdminApproved = 2,

    /// <summary>
    ///     Rejected by operator or admin
    /// </summary>
    Rejected = 3
}
