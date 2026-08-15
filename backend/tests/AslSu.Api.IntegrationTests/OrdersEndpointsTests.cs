using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Catalog.Dtos;
using AslSu.Application.OrderSync.Dtos;
using AslSu.Application.Orders.Dtos;
using AslSu.Application.Products.Dtos;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AslSu.Api.IntegrationTests;

/// <summary>Always returns the same canned packages, so pulling twice is a direct test of
/// upsert-by-PackageId (no duplicates on re-pull).</summary>
public class FakeTrendyolOrderClientForPull(Func<IReadOnlyList<TrendyolPackageDto>> packages) : ITrendyolOrderClient
{
    public Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not used by these tests.");

    public Task<TrendyolPackagesOutcome> GetPackagesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new TrendyolPackagesOutcome(true, packages(), null));
}

public class OrdersEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public OrdersEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetOrders_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PullOrders_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-sync/orders/pull", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PullOrders_WhenTrendyolNotConfigured_ReturnsNoPackagesFetched()
    {
        // Trendyol Go isn't configured in this test host, so the real client's IsConfigured
        // guard should short-circuit before any HTTP call — same pattern as the other push
        // endpoints' "not configured in tests" behavior.
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/trendyol-sync/orders/pull", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<OrderSyncSummary>();
        Assert.NotNull(summary);
        Assert.Equal(0, summary!.TotalFetched);
    }

    [Fact]
    public async Task PullOrders_UpsertsByPackageId_RepullingDoesNotDuplicate()
    {
        var setupClient = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await setupClient.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-ORDER-1", Barcode: "BARCODE-ORDER-1", Name: "Order Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);

        var storeResponse = await setupClient.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: "Order Test Store", Code: "ORDER-STORE-1", Address: null));
        Assert.Equal(HttpStatusCode.OK, storeResponse.StatusCode);

        IReadOnlyList<TrendyolPackageDto> Packages() =>
        [
            new TrendyolPackageDto(
                PackageId: "PKG-REPULL-1",
                OrderNumber: "ORD-REPULL-1",
                Status: "Created",
                OrderDate: new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                CustomerName: "Test Customer",
                CustomerPhone: "5550001122",
                CustomerAddress: "Test Address",
                InvoiceAmount: 100m,
                InvoiceTaxAmount: 18m,
                BagCount: 1,
                ReceiptLink: null,
                StoreTgoId: null,
                Lines: [new TrendyolPackageLineDto("BARCODE-ORDER-1", 2, 50m, false, null)],
                RawJson: "{\"id\":\"PKG-REPULL-1\"}"),
        ];

        using var pullFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITrendyolOrderClient));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<ITrendyolOrderClient>(new FakeTrendyolOrderClientForPull(Packages));
        }));
        var pullClient = await TestAuthHelper.CreateAuthenticatedClientAsync(pullFactory);

        var firstPull = await pullClient.PostAsync("/api/trendyol-sync/orders/pull", null);
        Assert.Equal(HttpStatusCode.OK, firstPull.StatusCode);
        var firstSummary = await firstPull.Content.ReadFromJsonAsync<OrderSyncSummary>();
        Assert.Equal(1, firstSummary!.NewCount);

        var secondPull = await pullClient.PostAsync("/api/trendyol-sync/orders/pull", null);
        Assert.Equal(HttpStatusCode.OK, secondPull.StatusCode);
        var secondSummary = await secondPull.Content.ReadFromJsonAsync<OrderSyncSummary>();
        Assert.Equal(0, secondSummary!.NewCount);
        Assert.Equal(1, secondSummary.UpdatedCount);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var matchingOrders = await dbContext.Orders.Where(o => o.PackageId == "PKG-REPULL-1").ToListAsync();
            Assert.Single(matchingOrders);

            var items = await dbContext.OrderItems.Where(i => i.OrderId == matchingOrders[0].Id).ToListAsync();
            Assert.Single(items);
            Assert.Equal("BARCODE-ORDER-1", items[0].Barcode);
        }

        var listResponse = await pullClient.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<OrderListItemDto>>();
        Assert.Contains(list!, o => o.PackageId == "PKG-REPULL-1");

        var created = list!.Single(o => o.PackageId == "PKG-REPULL-1");
        var detailResponse = await pullClient.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<OrderDetailDto>();
        Assert.NotNull(detail);
        Assert.Equal("Test Customer", detail!.CustomerName);
        Assert.Single(detail.Items);
        Assert.Equal(2, detail.Items[0].Quantity);
    }

    [Fact]
    public async Task GetOrderById_WhenMissing_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/orders/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
