using AslSu.Application.SaleStatusSync.Dtos;

namespace AslSu.Application.SaleStatusSync;

public interface ISaleStatusSyncService
{
    /// <summary>Pushes only StoreProductInventory rows whose IsOnSale differs from the
    /// last-synced value (or have never been synced) to Trendyol Go, in batches.</summary>
    Task<SaleStatusSyncSummary> PushChangedSaleStatusAsync(CancellationToken cancellationToken = default);
}
