using AslSu.Application.OrderSync;
using AslSu.Application.OrderSync.Dtos;
using AslSu.Infrastructure.BackgroundServices;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class OrderPollingBackgroundServiceTests
{
    private class FakeOrderSyncService(OrderSyncSummary summary) : IOrderSyncService
    {
        public Task<OrderSyncSummary> PullOrdersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(summary);
    }

    private static OrderPollingBackgroundService CreateService(OrderSyncSummary summary)
    {
        var services = new ServiceCollection();
        services.AddScoped<IOrderSyncService>(_ => new FakeOrderSyncService(summary));
        var provider = services.BuildServiceProvider();

        return new OrderPollingBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new TrendyolGoOptions
            {
                SupplierId = "123456", ApiKey = "key", ApiSecret = "secret", BaseUrl = "https://example.invalid",
            }),
            NullLogger<OrderPollingBackgroundService>.Instance);
    }

    [Fact]
    public async Task PollOnceAsync_OnSuccess_ReturnsNormalInterval()
    {
        var service = CreateService(new OrderSyncSummary(1, 1, 0, 0, "1 sipariş çekildi: 1 yeni, 0 güncellendi."));

        var delay = await service.PollOnceAsync(CancellationToken.None);

        Assert.Equal(OrderPollingBackgroundService.NormalInterval, delay);
    }

    [Fact]
    public async Task PollOnceAsync_WhenMessageIndicatesRateLimit_ReturnsBackoffInterval()
    {
        var service = CreateService(new OrderSyncSummary(0, 0, 0, 0, "API rate limitine ulaşıldı."));

        var delay = await service.PollOnceAsync(CancellationToken.None);

        Assert.Equal(OrderPollingBackgroundService.RateLimitBackoff, delay);
    }

    [Fact]
    public async Task PollOnceAsync_WhenNotConfigured_StillCompletesWithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddScoped<IOrderSyncService>(_ => new FakeOrderSyncService(
            new OrderSyncSummary(0, 0, 0, 0, "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.")));
        var provider = services.BuildServiceProvider();

        var service = new OrderPollingBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new TrendyolGoOptions()),
            NullLogger<OrderPollingBackgroundService>.Instance);

        // PollOnceAsync itself doesn't check IsConfigured (ExecuteAsync's loop does, and skips
        // calling it entirely when not configured) — calling it directly still completes
        // gracefully since the underlying summary is a "not configured" message, not an error.
        var delay = await service.PollOnceAsync(CancellationToken.None);

        Assert.Equal(OrderPollingBackgroundService.NormalInterval, delay);
    }
}
