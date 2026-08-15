using AslSu.Application.BatchPolling;
using AslSu.Application.ProductSync;
using AslSu.Application.SaleStatusSync;
using AslSu.Application.StockPriceSync;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AslSu.Infrastructure.BackgroundServices;

/// <summary>Automates what Phases 5-7 otherwise left as manual buttons on the Products page:
/// pushing unsynced products, changed stock/price, and changed sale status to Trendyol Go, then
/// polling any still-pending batch results — all on a ~1 minute cadence instead of a click.
/// Each of the four steps only sends what's actually changed (the same delta-only logic the
/// manual buttons use), so a cycle where nothing changed is just a few cheap DB queries with no
/// network calls at all. Does nothing (no DB access, no network calls) until Trendyol Go is
/// configured. Rate-limit aware, same as <see cref="OrderPollingBackgroundService"/>.</summary>
public class ProductAutoSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<TrendyolGoOptions> options,
    ILogger<ProductAutoSyncBackgroundService> logger) : BackgroundService
{
    public static readonly TimeSpan NormalInterval = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan RateLimitBackoff = TimeSpan.FromMinutes(3);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.Zero;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            delay = options.Value.IsConfigured ? await RunCycleAsync(stoppingToken) : NormalInterval;
        }
    }

    /// <summary>Runs one push-everything-then-poll cycle in its own DI scope and returns how
    /// long to wait before the next cycle. Each step is independently guarded — one step
    /// throwing doesn't stop the rest of the cycle from running.</summary>
    public async Task<TimeSpan> RunCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var rateLimited = false;

        rateLimited |= await RunStepAsync(
            "product push",
            () => scope.ServiceProvider.GetRequiredService<IProductSyncService>().PushUnsyncedProductsAsync(cancellationToken),
            s => s.Message);

        rateLimited |= await RunStepAsync(
            "stock/price push",
            () => scope.ServiceProvider.GetRequiredService<IStockPriceSyncService>().PushChangedInventoryAsync(cancellationToken),
            s => s.Message);

        rateLimited |= await RunStepAsync(
            "sale status push",
            () => scope.ServiceProvider.GetRequiredService<ISaleStatusSyncService>().PushChangedSaleStatusAsync(cancellationToken),
            s => s.Message);

        rateLimited |= await RunStepAsync(
            "batch result poll",
            () => scope.ServiceProvider.GetRequiredService<IBatchPollingService>().PollPendingBatchesAsync(cancellationToken),
            s => s.Message);

        return rateLimited ? RateLimitBackoff : NormalInterval;
    }

    private async Task<bool> RunStepAsync<TSummary>(
        string stepName, Func<Task<TSummary>> runStep, Func<TSummary, string?> getMessage)
    {
        try
        {
            var summary = await runStep();
            var message = getMessage(summary);
            logger.LogInformation("Auto-sync {Step}: {Message}", stepName, message);
            return message?.Contains("rate limit", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auto-sync {Step} failed.", stepName);
            return false;
        }
    }
}
