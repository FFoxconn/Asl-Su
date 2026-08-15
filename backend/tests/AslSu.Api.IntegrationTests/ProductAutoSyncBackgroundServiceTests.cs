using AslSu.Application.BatchPolling;
using AslSu.Application.BatchPolling.Dtos;
using AslSu.Application.ProductSync;
using AslSu.Application.ProductSync.Dtos;
using AslSu.Application.SaleStatusSync;
using AslSu.Application.SaleStatusSync.Dtos;
using AslSu.Application.StockPriceSync;
using AslSu.Application.StockPriceSync.Dtos;
using AslSu.Infrastructure.BackgroundServices;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class ProductAutoSyncBackgroundServiceTests
{
    private class FakeProductSyncService(string? message) : IProductSyncService
    {
        public Task<ProductSyncSummary> PushUnsyncedProductsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProductSyncSummary(0, 0, 0, 0, [], message));

        public Task<IReadOnlyList<BatchRequestLogDto>> GetRecentBatchRequestsAsync(int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BatchRequestLogDto>>([]);
    }

    private class FakeStockPriceSyncService(string? message) : IStockPriceSyncService
    {
        public Task<StockPriceSyncSummary> PushChangedInventoryAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new StockPriceSyncSummary(0, 0, 0, 0, [], message));
    }

    private class FakeSaleStatusSyncService(string? message) : ISaleStatusSyncService
    {
        public Task<SaleStatusSyncSummary> PushChangedSaleStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new SaleStatusSyncSummary(0, 0, 0, 0, [], message));
    }

    private class FakeBatchPollingService(string? message) : IBatchPollingService
    {
        public Task<BatchPollSummary> PollPendingBatchesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new BatchPollSummary(0, 0, 0, 0, message));
    }

    private class ThrowingBatchPollingService : IBatchPollingService
    {
        public Task<BatchPollSummary> PollPendingBatchesAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("boom");
    }

    private static ProductAutoSyncBackgroundService CreateService(
        string? productMessage = "Senkronize edilecek ürün yok.",
        string? stockMessage = "Senkronize edilecek stok/fiyat değişikliği yok.",
        string? saleStatusMessage = "Senkronize edilecek satış durumu değişikliği yok.",
        IBatchPollingService? batchPollingService = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<IProductSyncService>(_ => new FakeProductSyncService(productMessage));
        services.AddScoped<IStockPriceSyncService>(_ => new FakeStockPriceSyncService(stockMessage));
        services.AddScoped<ISaleStatusSyncService>(_ => new FakeSaleStatusSyncService(saleStatusMessage));
        services.AddScoped(_ => batchPollingService ?? new FakeBatchPollingService("Kontrol edilecek parti yok."));
        var provider = services.BuildServiceProvider();

        return new ProductAutoSyncBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new TrendyolGoOptions
            {
                SupplierId = "123456", ApiKey = "key", ApiSecret = "secret", BaseUrl = "https://example.invalid",
            }),
            NullLogger<ProductAutoSyncBackgroundService>.Instance);
    }

    [Fact]
    public async Task RunCycleAsync_WhenNothingChanged_ReturnsNormalInterval()
    {
        var service = CreateService();

        var delay = await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(ProductAutoSyncBackgroundService.NormalInterval, delay);
    }

    [Fact]
    public async Task RunCycleAsync_WhenAnyStepReportsRateLimit_ReturnsBackoffInterval()
    {
        var service = CreateService(stockMessage: "API rate limitine ulaşıldı.");

        var delay = await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(ProductAutoSyncBackgroundService.RateLimitBackoff, delay);
    }

    [Fact]
    public async Task RunCycleAsync_WhenOneStepThrows_StillCompletesAndRunsTheRest()
    {
        var service = CreateService(batchPollingService: new ThrowingBatchPollingService());

        // Should not throw despite the batch-polling step failing internally.
        var delay = await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(ProductAutoSyncBackgroundService.NormalInterval, delay);
    }
}
