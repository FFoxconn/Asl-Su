using AslSu.Application.ProductSync;
using AslSu.Application.ProductSync.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Products;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.ProductSync;

public class ProductSyncService(AslSuDbContext dbContext, ITrendyolProductClient productClient)
    : IProductSyncService
{
    public async Task<ProductSyncSummary> PushUnsyncedProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .Where(p => p.TgoSyncStatus == TgoSyncStatus.NotSynced || p.TgoSyncStatus == TgoSyncStatus.Failed)
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return new ProductSyncSummary(0, 0, 0, 0, [], "Senkronize edilecek ürün yok.");
        }

        var batches = BatchSplitter.Split<Product>(products);
        var batchRequestIds = new List<string>();
        var submittedCount = 0;
        var failedCount = 0;

        foreach (var batch in batches)
        {
            var payloads = batch.Select(ToPayload).ToList();
            var result = await productClient.CreateProductsBatchAsync(payloads, cancellationToken);
            var now = DateTime.UtcNow;

            foreach (var product in batch)
            {
                product.TgoLastSyncDate = now;
                product.UpdatedAt = now;
                if (result.Success)
                {
                    product.TgoSyncStatus = TgoSyncStatus.Pending;
                    product.TgoLastError = null;
                }
                else
                {
                    product.TgoSyncStatus = TgoSyncStatus.Failed;
                    product.TgoLastError = result.ErrorMessage;
                }
            }

            dbContext.BatchRequestLogs.Add(new BatchRequestLog
            {
                BatchRequestId = result.BatchRequestId ?? string.Empty,
                OperationType = BatchOperationType.ProductCreate,
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
            ? $"{submittedCount} ürün {batches.Count} parti halinde gönderildi."
            : $"{submittedCount} ürün gönderildi, {failedCount} ürün gönderilemedi.";

        return new ProductSyncSummary(products.Count, batches.Count, submittedCount, failedCount, batchRequestIds, message);
    }

    public async Task<IReadOnlyList<BatchRequestLogDto>> GetRecentBatchRequestsAsync(
        int take = 50, CancellationToken cancellationToken = default) =>
        await dbContext.BatchRequestLogs
            .OrderByDescending(b => b.RequestedAt)
            .Take(take)
            .Select(b => new BatchRequestLogDto(
                b.Id, b.BatchRequestId, b.OperationType.ToString(), b.RequestedAt, b.Status.ToString(),
                b.SuccessCount, b.FailureCount, b.FailureReasonsJson))
            .ToListAsync(cancellationToken);

    private static TrendyolProductPayload ToPayload(Product product) => new(
        Barcode: product.Barcode,
        Title: product.Name,
        Brand: null,
        Category: null,
        VatRate: product.VatRate,
        StockCode: product.Sku,
        Description: product.Description,
        ImageUrl: product.ImageUrl);

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}
