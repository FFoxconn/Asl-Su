using System.Net;
using System.Net.Http.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Inventory;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolInventoryClientTests
{
    private static readonly TrendyolInventoryItemPayload[] SampleItems =
    [
        new("BARCODE-1", "store-1", 15, 79.90m, 99.90m),
    ];

    private static TrendyolInventoryClient CreateClient(FakeHttpMessageHandler fake, bool configured = true)
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var options = Options.Create(configured
            ? new TrendyolGoOptions { SupplierId = "123456", ApiKey = "key", ApiSecret = "secret", BaseUrl = "https://example.invalid" }
            : new TrendyolGoOptions());
        return new TrendyolInventoryClient(httpClient, options);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_WhenNotConfigured_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, configured: false);

        var result = await client.PushPriceAndInventoryBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_UsesTheGivenPriceAndInventoryEndpointPath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { batchRequestId = "batch-1" }),
        });
        var client = CreateClient(fake);

        await client.PushPriceAndInventoryBatchAsync(SampleItems);

        var sent = Assert.Single(fake.Requests);
        Assert.Equal(
            "/integrator/product/grocery/suppliers/123456/products/price-and-inventory",
            sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_With200AndBatchRequestId_ReturnsSuccess()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { batchRequestId = "batch-xyz" }),
        });
        var client = CreateClient(fake);

        var result = await client.PushPriceAndInventoryBatchAsync(SampleItems);

        Assert.True(result.Success);
        Assert.Equal("batch-xyz", result.BatchRequestId);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.PushPriceAndInventoryBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_With429_ReturnsRateLimitMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var client = CreateClient(fake);

        var result = await client.PushPriceAndInventoryBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("API rate limitine ulaşıldı.", result.ErrorMessage);
    }

    [Fact]
    public async Task PushPriceAndInventoryBatchAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.PushPriceAndInventoryBatchAsync(SampleItems);

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
