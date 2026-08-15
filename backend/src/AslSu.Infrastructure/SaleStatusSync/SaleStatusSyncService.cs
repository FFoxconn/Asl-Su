using AslSu.Application.SaleStatusSync;
using AslSu.Application.SaleStatusSync.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.SellUnsell;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.SaleStatusSync;

public class SaleStatusSyncService(AslSuDbContext dbContext, ITrendyolSellUnsellClient sellUnsellClient)
    : ISaleStatusSyncService
{
    public async Task<SaleStatusSyncSummary> PushChangedSaleStatusAsync(CancellationToken cancellationToken = default)
    {
        var changed = await dbContext.StoreProductInventories
            .Where(i => i.LastSyncedIsOnSale == null || i.IsOnSale != i.LastSyncedIsOnSale)
            .ToListAsync(cancellationToken);

        if (changed.Count == 0)
        {
            return new SaleStatusSyncSummary(0, 0, 0, 0, [], "Senkronize edilecek satış durumu değişikliği yok.");
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
                .Select(i => new TrendyolSaleStatusItemPayload(
                    products[i.ProductId].Barcode,
                    stores[i.StoreId].TgoStoreId,
                    i.IsOnSale,
                    i.IsOnSale ? null : i.UnsaleReasonCode))
                .ToList();

            var result = await sellUnsellClient.SetSaleStatusBatchAsync(payloads, cancellationToken);
            var now = DateTime.UtcNow;

            if (result.Success)
            {
                foreach (var item in batch)
                {
                    item.LastSyncedIsOnSale = item.IsOnSale;
                    item.SaleStatusLastSyncedAt = now;
                }
            }
            // On failure, LastSyncedIsOnSale is left untouched so the row is retried next time.

            dbContext.BatchRequestLogs.Add(new BatchRequestLog
            {
                BatchRequestId = result.BatchRequestId ?? string.Empty,
                OperationType = BatchOperationType.SellUnsell,
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
            ? $"{submittedCount} satış durumu değişikliği {batches.Count} parti halinde gönderildi."
            : $"{submittedCount} kayıt gönderildi, {failedCount} kayıt gönderilemedi.";

        return new SaleStatusSyncSummary(changed.Count, batches.Count, submittedCount, failedCount, batchRequestIds, message);
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}
