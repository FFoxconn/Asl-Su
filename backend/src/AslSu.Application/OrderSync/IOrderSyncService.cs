using AslSu.Application.OrderSync.Dtos;

namespace AslSu.Application.OrderSync;

public interface IOrderSyncService
{
    /// <summary>Pulls packages from Trendyol Go and upserts them by PackageId — inserts new
    /// orders, updates existing ones, never duplicates on re-pull.</summary>
    Task<OrderSyncSummary> PullOrdersAsync(CancellationToken cancellationToken = default);
}
