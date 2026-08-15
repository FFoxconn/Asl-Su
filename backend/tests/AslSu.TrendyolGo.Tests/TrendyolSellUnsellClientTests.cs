using System.Net;
using System.Net.Http.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.SellUnsell;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolSellUnsellClientTests
{
    private static readonly TrendyolSaleStatusItemPayload[] SampleItems =
    [
        new("BARCODE-1", "store-1", false, "OutOfStock"),
    ];

    private static TrendyolSellUnsellClient CreateClient(
        FakeHttpMessageHandler fake, bool configured = true, string sellUnsellEndpointPath = "/sell-unsell")
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var options = Options.Create(configured
            ? new TrendyolGoOptions
            {
                SupplierId = "123456", ApiKey = "key", ApiSecret = "secret",
                BaseUrl = "https://example.invalid", SellUnsellEndpointPath = sellUnsellEndpointPath,
            }
            : new TrendyolGoOptions());
        return new TrendyolSellUnsellClient(httpClient, options);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_WhenNotConfiguredAtAll_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, configured: false);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_WhenEndpointPathNotSet_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, sellUnsellEndpointPath: "");

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.False(called);
        Assert.Contains("yapılandırılmamış", result.ErrorMessage);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_With200AndBatchRequestId_ReturnsSuccess()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { batchRequestId = "batch-sell-1" }),
        });
        var client = CreateClient(fake);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.True(result.Success);
        Assert.Equal("batch-sell-1", result.BatchRequestId);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_With403_ReturnsHeaderHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var client = CreateClient(fake);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("User-Agent veya yetkilendirme bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_With429_ReturnsRateLimitMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var client = CreateClient(fake);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("API rate limitine ulaşıldı.", result.ErrorMessage);
    }

    [Fact]
    public async Task SetSaleStatusBatchAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.SetSaleStatusBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
