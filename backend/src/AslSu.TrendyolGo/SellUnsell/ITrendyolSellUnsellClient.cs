using AslSu.TrendyolGo.BatchRequests;

namespace AslSu.TrendyolGo.SellUnsell;

public interface ITrendyolSellUnsellClient
{
    /// <summary>Submits one batch (already split to the configured max batch size) of
    /// sale-status changes. Returns a failure result — never throws for a "not configured"
    /// state — if TrendyolGoOptions.SellUnsellEndpointPath hasn't been set yet.</summary>
    Task<TrendyolBatchSubmitResult> SetSaleStatusBatchAsync(
        IReadOnlyList<TrendyolSaleStatusItemPayload> items, CancellationToken cancellationToken = default);
}
