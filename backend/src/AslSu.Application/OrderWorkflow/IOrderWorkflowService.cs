using AslSu.Application.OrderWorkflow.Dtos;

namespace AslSu.Application.OrderWorkflow;

/// <summary>Drives the local fulfilment workflow — Yeni → Kabul Edildi → Hazırlanıyor →
/// Hazırlandı → Teslim Edildi — one step at a time. Accept/mark-prepared/deliver also
/// best-effort notify Trendyol Go (accept/invoice/ship); the local step always advances
/// regardless of whether that notification succeeds, since the local workflow must keep
/// working even before those endpoints are configured — the notification outcome is
/// surfaced on the result instead of blocking the transition.</summary>
public interface IOrderWorkflowService
{
    Task<OrderWorkflowActionResult> AcceptAsync(int orderId, CancellationToken cancellationToken = default);

    Task<OrderWorkflowActionResult> StartPreparingAsync(int orderId, CancellationToken cancellationToken = default);

    Task<OrderWorkflowActionResult> MarkPreparedAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Pass <paramref name="requiredCourierId"/> to only allow the transition when
    /// the order is assigned to that courier (used when a Courier-role caller invokes this
    /// themselves); null skips the ownership check, as Admin/Operator callers do.</summary>
    Task<OrderWorkflowActionResult> DeliverAsync(
        int orderId, int? requiredCourierId = null, CancellationToken cancellationToken = default);
}
