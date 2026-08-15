using AslSu.Application.StockPriceSync.Dtos;

namespace AslSu.Application.StockPriceSync;

public interface IStockPriceSyncService
{
    /// <summary>Pushes only StoreProductInventory rows whose Quantity/SalePrice differ from
    /// the last-synced values (or have never been synced) to Trendyol Go, in batches.</summary>
    Task<StockPriceSyncSummary> PushChangedInventoryAsync(CancellationToken cancellationToken = default);
}
