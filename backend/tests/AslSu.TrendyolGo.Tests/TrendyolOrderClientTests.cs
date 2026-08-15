using System.Net;
using System.Net.Http.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Orders;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolOrderClientTests
{
    private static TrendyolOrderClient CreateClient(FakeHttpMessageHandler fake)
    {
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var options = Options.Create(new TrendyolGoOptions
        {
            SupplierId = "123456", ApiKey = "key", ApiSecret = "secret", BaseUrl = "https://example.invalid",
        });
        return new TrendyolOrderClient(httpClient, options);
    }

    [Fact]
    public async Task GetPackagesAsync_WhenNotConfigured_FailsWithoutCallingNetwork()
    {
        var called = false;
        var fake = new FakeHttpMessageHandler(_ => { called = true; return new HttpResponseMessage(HttpStatusCode.OK); });
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://example.invalid") };
        var client = new TrendyolOrderClient(httpClient, Options.Create(new TrendyolGoOptions()));

        var result = await client.GetPackagesAsync();

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task GetPackagesAsync_CallsSupplierScopedPackagesPath()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<object>()),
        });
        var client = CreateClient(fake);

        await client.GetPackagesAsync();

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("/integrator/order/grocery/suppliers/123456/packages", sent.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetPackagesAsync_ParsesBareArrayOfPackages()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new object[]
            {
                new
                {
                    id = "PKG-1",
                    orderNumber = "ORD-1",
                    status = "Created",
                    orderDate = 1700000000000L,
                    customerFirstName = "Ada",
                    customerLastName = "Lovelace",
                    customerPhone = "5551234567",
                    totalPrice = 149.90,
                    totalTax = 12.5,
                    bagCount = 2,
                    lines = new[]
                    {
                        new { barcode = "BC-1", quantity = 2, price = 49.90 },
                        new { barcode = "BC-2", quantity = 1, price = 50.10 },
                    },
                },
            }),
        });
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.True(result.Success);
        var pkg = Assert.Single(result.Packages);
        Assert.Equal("PKG-1", pkg.PackageId);
        Assert.Equal("ORD-1", pkg.OrderNumber);
        Assert.Equal("Created", pkg.Status);
        Assert.Equal("Ada Lovelace", pkg.CustomerName);
        Assert.Equal("5551234567", pkg.CustomerPhone);
        Assert.Equal(149.90m, pkg.InvoiceAmount);
        Assert.Equal(2, pkg.BagCount);
        Assert.Equal(2, pkg.Lines.Count);
        Assert.Equal("BC-1", pkg.Lines[0].Barcode);
        Assert.Equal(2, pkg.Lines[0].Quantity);
        Assert.Equal(49.90m, pkg.Lines[0].UnitPrice);
        Assert.NotNull(pkg.RawJson);
    }

    [Fact]
    public async Task GetPackagesAsync_ParsesPackagesWrappedInContentProperty()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                content = new object[] { new { id = "PKG-9", orderNumber = "ORD-9" } },
                totalElements = 1,
            }),
        });
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.True(result.Success);
        var pkg = Assert.Single(result.Packages);
        Assert.Equal("PKG-9", pkg.PackageId);
    }

    [Fact]
    public async Task GetPackagesAsync_WithUnparseableBody_FallsBackToEmptyListRatherThanThrowing()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json at all"),
        });
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.True(result.Success);
        Assert.Empty(result.Packages);
    }

    [Fact]
    public async Task GetPackagesAsync_With401_ReturnsCredentialHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.False(result.Success);
        Assert.Equal("API Key / API Secret / Supplier ID bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetPackagesAsync_With403_ReturnsHeaderHint()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.False(result.Success);
        Assert.Equal("User-Agent veya yetkilendirme bilgilerini kontrol edin.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetPackagesAsync_With429_ReturnsRateLimitMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.False(result.Success);
        Assert.Equal("API rate limitine ulaşıldı.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetPackagesAsync_With500_ReturnsServiceErrorMessage()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(fake);

        var result = await client.GetPackagesAsync();

        Assert.False(result.Success);
        Assert.Equal("Trendyol Go servisinde geçici hata.", result.ErrorMessage);
    }
}
