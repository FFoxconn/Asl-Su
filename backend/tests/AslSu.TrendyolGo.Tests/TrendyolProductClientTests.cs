using System.Net;
using System.Net.Http.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Products;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolProductClientTests
{
    private static readonly TrendyolProductPayload[] SamplePayload =
    [
        new("BARCODE-1", "Test Product", null, null, 18m, "SKU-1", null, null),
    ];

    private static TrendyolProductClient CreateClient(FakeHttpMessageHandler fake, string productsEndpointPath = "/products")
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var options = Options.Create(new TrendyolGoOptions
        {
            SupplierId = "123", ApiKey = "key", ApiSecret = "secret",
            BaseUrl = "https://example.invalid", ProductsEndpointPath = productsEndpointPath,
        });
        return new TrendyolProductClient(httpClient, options);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_WithEndpointNotConfigured_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, productsEndpointPath: "");

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.False(result.Success);
        Assert.False(called);
        Assert.Contains("yapılandırılmamış", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_With200AndBatchRequestId_ReturnsSuccess()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { batchRequestId = "batch-abc-123" }),
        });
        var client = CreateClient(fake);

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.True(result.Success);
        Assert.Equal("batch-abc-123", result.BatchRequestId);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_With200ButNoBatchRequestId_ReturnsFailure()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { somethingElse = "value" }),
        });
        var client = CreateClient(fake);

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_With429_ReturnsRateLimitMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var client = CreateClient(fake);

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.False(result.Success);
        Assert.Equal("API rate limitine ulaşıldı.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateProductsBatchAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.CreateProductsBatchAsync(SamplePayload);

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
