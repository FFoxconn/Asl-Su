using AslSu.TrendyolGo.BatchRequests;

namespace AslSu.TrendyolGo.Inventory;

public interface ITrendyolInventoryClient
{
    /// <summary>Submits one batch (already split to the configured max batch size) of
    /// stock/price updates to the price-and-inventory endpoint.</summary>
    Task<TrendyolBatchSubmitResult> PushPriceAndInventoryBatchAsync(
        IReadOnlyList<TrendyolInventoryItemPayload> items, CancellationToken cancellationToken = default);
}
