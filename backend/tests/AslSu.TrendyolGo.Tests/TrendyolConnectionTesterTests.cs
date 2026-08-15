using System.Net;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Connection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolConnectionTesterTests
{
    private static TrendyolGoOptions ConfiguredOptions() => new()
    {
        SupplierId = "123456",
        ApiKey = "key",
        ApiSecret = "secret",
        BaseUrl = "https://example.invalid",
        AgentName = "agent",
        ExecutorUser = "user",
    };

    [Fact]
    public async Task TestConnectionAsync_WhenNotConfigured_ReturnsNotConfiguredWithoutCallingClient()
    {
        var called = false;
        var client = new FakeTrendyolOrderClient(() => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var tester = new TrendyolConnectionTester(client, Options.Create(new TrendyolGoOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.NotConfigured, result.Status);
        Assert.False(called);
    }

    [Fact]
    public async Task TestConnectionAsync_With200_ReturnsSuccess()
    {
        var client = new FakeTrendyolOrderClient(() => new HttpResponseMessage(HttpStatusCode.OK));
        var tester = new TrendyolConnectionTester(client, Options.Create(ConfiguredOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.Success, result.Status);
        Assert.Equal("Trendyol Go API bağlantısı başarılı.", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_With401_ReturnsUnauthorizedWithCredentialHint()
    {
        var client = new FakeTrendyolOrderClient(() => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var tester = new TrendyolConnectionTester(client, Options.Create(ConfiguredOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.Unauthorized, result.Status);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_With403_ReturnsForbiddenWithHeaderHint()
    {
        var client = new FakeTrendyolOrderClient(() => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var tester = new TrendyolConnectionTester(client, Options.Create(ConfiguredOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.Forbidden, result.Status);
        Assert.Equal("User-Agent veya yetkilendirme bilgilerini kontrol edin.", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_With429_ReturnsRateLimited()
    {
        var client = new FakeTrendyolOrderClient(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var tester = new TrendyolConnectionTester(client, Options.Create(ConfiguredOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.RateLimited, result.Status);
        Assert.Equal("API rate limitine ulaşıldı.", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_With500_ReturnsServiceError()
    {
        var client = new FakeTrendyolOrderClient(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var tester = new TrendyolConnectionTester(client, Options.Create(ConfiguredOptions()));

        var result = await tester.TestConnectionAsync();

        Assert.Equal(TrendyolConnectionStatus.ServiceError, result.Status);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.Message);
    }
}
