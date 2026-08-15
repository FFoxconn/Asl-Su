using System.Net;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.PackageStatus;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolPackageStatusClientTests
{
    private static TrendyolPackageStatusClient CreateClient(FakeHttpMessageHandler fake, TrendyolGoOptions? options = null)
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        return new TrendyolPackageStatusClient(httpClient, Options.Create(options ?? new TrendyolGoOptions
        {
            SupplierId = "123456",
            ApiKey = "key",
            ApiSecret = "secret",
            BaseUrl = "https://example.invalid",
            AcceptOrderEndpointPath = "/orders/{packageId}/accept",
            InvoiceOrderEndpointPath = "/orders/{packageId}/invoice",
            ShipOrderEndpointPath = "/orders/{packageId}/ship",
        }));
    }

    [Fact]
    public async Task AcceptPackageAsync_WhenNotConfiguredAtAll_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, new TrendyolGoOptions());

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task AcceptPackageAsync_WhenEndpointPathNotSet_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var client = CreateClient(fake, new TrendyolGoOptions
        {
            SupplierId = "123456", ApiKey = "key", ApiSecret = "secret", BaseUrl = "https://example.invalid",
        });

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.False(called);
        Assert.Contains("yapılandırılmamış", result.ErrorMessage);
    }

    [Fact]
    public async Task AcceptPackageAsync_CallsThePackageIdSubstitutedPath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(fake);

        await client.AcceptPackageAsync("PKG-42");

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("/orders/PKG-42/accept", sent.RequestUri!.AbsolutePath);
        Assert.Equal(HttpMethod.Post, sent.Method);
    }

    [Fact]
    public async Task InvoicePackageAsync_CallsTheInvoicePath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(fake);

        await client.InvoicePackageAsync("PKG-42");

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("/orders/PKG-42/invoice", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ShipPackageAsync_CallsTheShipPath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(fake);

        await client.ShipPackageAsync("PKG-42");

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("/orders/PKG-42/ship", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task AcceptPackageAsync_With200_ReturnsSuccess()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(fake);

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task AcceptPackageAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task AcceptPackageAsync_With403_ReturnsHeaderHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var client = CreateClient(fake);

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.Equal("User-Agent veya yetkilendirme bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task AcceptPackageAsync_With429_ReturnsRateLimitMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var client = CreateClient(fake);

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.Equal("API rate limitine ulaşıldı.", result.ErrorMessage);
    }

    [Fact]
    public async Task AcceptPackageAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.AcceptPackageAsync("PKG-1");

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
