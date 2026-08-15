using AslSu.Application.OrderSync;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AslSu.Infrastructure.BackgroundServices;

/// <summary>Polling fallback for order updates, in case a Trendyol Go webhook delivery is
/// missed or the webhook endpoint (Phase 11) isn't reachable from Trendyol Go's side yet.
/// Runs continuously at a 30-60s cadence (default 45s) while the app is up; does nothing (no
/// network calls at all) until Trendyol Go is configured. Rate-limit aware: if a poll comes
/// back reporting Trendyol Go's rate limit was hit, the next poll is pushed out to
/// <see cref="RateLimitBackoff"/> instead of immediately retrying at the normal cadence.</summary>
public class OrderPollingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<TrendyolGoOptions> options,
    ILogger<OrderPollingBackgroundService> logger) : BackgroundService
{
    public static readonly TimeSpan NormalInterval = TimeSpan.FromSeconds(45);
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

            delay = options.Value.IsConfigured ? await PollOnceAsync(stoppingToken) : NormalInterval;
        }
    }

    /// <summary>Runs one poll cycle in its own DI scope — this service is a singleton but
    /// IOrderSyncService (and everything it depends on) is scoped — and returns how long to
    /// wait before the next cycle.</summary>
    public async Task<TimeSpan> PollOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var orderSyncService = scope.ServiceProvider.GetRequiredService<IOrderSyncService>();

        try
        {
            var summary = await orderSyncService.PullOrdersAsync(cancellationToken);
            logger.LogInformation("Polling fallback order pull: {Message}", summary.Message);

            return summary.Message?.Contains("rate limit", StringComparison.OrdinalIgnoreCase) == true
                ? RateLimitBackoff
                : NormalInterval;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Polling fallback order pull failed.");
            return NormalInterval;
        }
    }
}
