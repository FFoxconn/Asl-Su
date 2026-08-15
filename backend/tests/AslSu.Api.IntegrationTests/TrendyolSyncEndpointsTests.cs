using System.Net;
using System.Net.Http.Json;
using AslSu.Application.ProductSync.Dtos;
using AslSu.Application.Products.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class TrendyolSyncEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public TrendyolSyncEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PushProducts_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-sync/products/push", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBatchRequests_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/trendyol-sync/batch-requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PushProducts_ReturnsASelfConsistentSummary()
    {
        // Sibling tests in this class share one in-memory database (IClassFixture), so this
        // does not assume a pristine Products table — only that the returned counts add up,
        // regardless of what other tests have already pushed.
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/trendyol-sync/products/push", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<ProductSyncSummary>();
        Assert.NotNull(summary);
        Assert.Equal(summary!.TotalProducts, summary.SubmittedCount + summary.FailedCount);
        Assert.False(string.IsNullOrWhiteSpace(summary.Message));
    }

    [Fact]
    public async Task PushProducts_WithUnsyncedProduct_MarksItFailedSinceTrendyolEndpointIsNotConfiguredInTests()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SYNC-1", Barcode: "BARCODE-SYNC-1", Name: "Sync Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("NotSynced", created!.TgoSyncStatus);

        var pushResponse = await client.PostAsync("/api/trendyol-sync/products/push", null);
        Assert.Equal(HttpStatusCode.OK, pushResponse.StatusCode);
        var summary = await pushResponse.Content.ReadFromJsonAsync<ProductSyncSummary>();
        Assert.NotNull(summary);
        Assert.True(summary!.TotalProducts >= 1);
        Assert.True(summary.FailedCount >= 1);

        var getResponse = await client.GetAsync($"/api/products/{created.Id}");
        var reloaded = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Failed", reloaded!.TgoSyncStatus);
    }

    [Fact]
    public async Task PushProducts_RecordsABatchRequestLogRow()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SYNC-2", Barcode: "BARCODE-SYNC-2", Name: "Sync Test Product 2",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));

        await client.PostAsync("/api/trendyol-sync/products/push", null);

        var logsResponse = await client.GetAsync("/api/trendyol-sync/batch-requests");
        Assert.Equal(HttpStatusCode.OK, logsResponse.StatusCode);
        var logs = await logsResponse.Content.ReadFromJsonAsync<List<BatchRequestLogDto>>();
        Assert.NotNull(logs);
        Assert.Contains(logs!, l => l.OperationType == "ProductCreate");
    }
}
