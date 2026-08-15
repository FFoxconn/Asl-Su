using AslSu.Application.ProductSync.Dtos;

namespace AslSu.Application.ProductSync;

public interface IProductSyncService
{
    /// <summary>Pushes every product not yet successfully synced (NotSynced or Failed) to
    /// Trendyol Go in batches, updating each product's TgoSyncStatus/TgoLastSyncDate/
    /// TgoLastError and recording one BatchRequestLog row per batch submitted.</summary>
    Task<ProductSyncSummary> PushUnsyncedProductsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BatchRequestLogDto>> GetRecentBatchRequestsAsync(
        int take = 50, CancellationToken cancellationToken = default);
}
