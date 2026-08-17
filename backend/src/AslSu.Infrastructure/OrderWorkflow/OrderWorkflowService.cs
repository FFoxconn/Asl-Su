using AslSu.Application.OrderWorkflow;
using AslSu.Application.OrderWorkflow.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.PackageStatus;

namespace AslSu.Infrastructure.OrderWorkflow;

public class OrderWorkflowService(AslSuDbContext dbContext, ITrendyolPackageStatusClient packageStatusClient)
    : IOrderWorkflowService
{
    public Task<OrderWorkflowActionResult> AcceptAsync(int orderId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            orderId, WorkflowStatus.New, WorkflowStatus.Accepted,
            (order, ct) => packageStatusClient.AcceptPackageAsync(order.PackageId, ct), cancellationToken);

    public Task<OrderWorkflowActionResult> StartPreparingAsync(int orderId, CancellationToken cancellationToken = default) =>
        TransitionAsync(orderId, WorkflowStatus.Accepted, WorkflowStatus.Preparing, null, cancellationToken);

    public Task<OrderWorkflowActionResult> MarkPreparedAsync(int orderId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            orderId, WorkflowStatus.Preparing, WorkflowStatus.Prepared,
            (order, ct) => packageStatusClient.InvoicePackageAsync(order.PackageId, ct), cancellationToken);

    public Task<OrderWorkflowActionResult> DeliverAsync(
        int orderId, int? requiredCourierId = null, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            orderId, WorkflowStatus.Prepared, WorkflowStatus.Delivered,
            (order, ct) => packageStatusClient.ShipPackageAsync(order.PackageId, ct), cancellationToken, requiredCourierId);

    private async Task<OrderWorkflowActionResult> TransitionAsync(
        int orderId,
        WorkflowStatus expectedCurrent,
        WorkflowStatus next,
        Func<Order, CancellationToken, Task<TrendyolPackageActionOutcome>>? notifyTrendyol,
        CancellationToken cancellationToken,
        int? requiredCourierId = null)
    {
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null)
        {
            return OrderWorkflowActionResult.Fail(OrderWorkflowError.NotFound);
        }

        if (requiredCourierId.HasValue && order.CourierId != requiredCourierId.Value)
        {
            return OrderWorkflowActionResult.Fail(OrderWorkflowError.Forbidden);
        }

        if (order.WorkflowStatus != expectedCurrent)
        {
            return OrderWorkflowActionResult.Fail(OrderWorkflowError.InvalidTransition);
        }

        var trendyolNotified = false;
        string? trendyolMessage = null;
        if (notifyTrendyol is not null)
        {
            var outcome = await notifyTrendyol(order, cancellationToken);
            trendyolNotified = outcome.Success;
            trendyolMessage = outcome.ErrorMessage;
        }

        order.WorkflowStatus = next;
        order.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OrderWorkflowActionResult.Ok(next.ToString(), trendyolNotified, trendyolMessage);
    }
}
