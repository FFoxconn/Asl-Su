using AslSu.Application.StockPriceSync;
using AslSu.Application.StockPriceSync.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.StockPriceSync;

public class StockPriceSyncService(AslSuDbContext dbContext, ITrendyolInventoryClient inventoryClient)
    : IStockPriceSyncService
{
    public async Task<StockPriceSyncSummary> PushChangedInventoryAsync(CancellationToken cancellationToken = default)
    {
        var changed = await dbContext.StoreProductInventories
            .Where(i => i.LastSyncedAt == null
                || i.Quantity != i.LastSyncedQuantity
                || i.SalePrice != i.LastSyncedSalePrice)
            .ToListAsync(cancellationToken);

        if (changed.Count == 0)
        {
            return new StockPriceSyncSummary(0, 0, 0, 0, [], "Senkronize edilecek stok/fiyat değişikliği yok.");
        }

        var productIds = changed.Select(i => i.ProductId).Distinct().ToList();
        var storeIds = changed.Select(i => i.StoreId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var stores = await dbContext.Stores
            .Where(s => storeIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var batches = BatchSplitter.Split<StoreProductInventory>(changed);
        var batchRequestIds = new List<string>();
        var submittedCount = 0;
        var failedCount = 0;

        foreach (var batch in batches)
        {
            var payloads = batch
                .Select(i => new TrendyolInventoryItemPayload(
                    products[i.ProductId].Barcode,
                    stores[i.StoreId].TgoStoreId,
                    i.Quantity,
                    i.SalePrice,
                    i.ListPrice))
                .ToList();

            var result = await inventoryClient.PushPriceAndInventoryBatchAsync(payloads, cancellationToken);
            var now = DateTime.UtcNow;

            if (result.Success)
            {
                foreach (var item in batch)
                {
                    item.LastSyncedQuantity = item.Quantity;
                    item.LastSyncedSalePrice = item.SalePrice;
                    item.LastSyncedAt = now;
                }
            }
            // On failure, LastSynced* values are left untouched so the row is picked up
            // again on the next push attempt.

            dbContext.BatchRequestLogs.Add(new BatchRequestLog
            {
                BatchRequestId = result.BatchRequestId ?? string.Empty,
                OperationType = BatchOperationType.PriceInventoryUpdate,
                RequestedAt = now,
                Status = result.Success ? BatchStatus.Pending : BatchStatus.Failed,
                SuccessCount = 0,
                FailureCount = 0,
                FailureReasonsJson = result.Success ? null : $"[\"{Escape(result.ErrorMessage ?? "Unknown error")}\"]",
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            if (result.Success)
            {
                batchRequestIds.Add(result.BatchRequestId!);
                submittedCount += batch.Count;
            }
            else
            {
                failedCount += batch.Count;
            }
        }

        var message = failedCount == 0
            ? $"{submittedCount} stok/fiyat kaydı {batches.Count} parti halinde gönderildi."
            : $"{submittedCount} kayıt gönderildi, {failedCount} kayıt gönderilemedi.";

        return new StockPriceSyncSummary(changed.Count, batches.Count, submittedCount, failedCount, batchRequestIds, message);
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}
