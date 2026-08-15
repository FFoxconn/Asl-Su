using System.Net;
using System.Net.Http.Json;
using AslSu.Application.BatchPolling.Dtos;
using AslSu.Application.Catalog.Dtos;
using AslSu.Application.ProductSync.Dtos;
using AslSu.Application.Products.Dtos;
using AslSu.Application.SaleStatusSync.Dtos;
using AslSu.Application.StockPrice.Dtos;
using AslSu.Application.StockPriceSync.Dtos;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task PushStockPrice_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-sync/stock-price/push", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PollBatchRequests_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-sync/batch-requests/poll", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PushStockPrice_WithChangedInventory_RecordsAPriceInventoryUpdateBatchLog()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-STOCKPUSH-1", Barcode: "BARCODE-STOCKPUSH-1", Name: "Stock Push Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var storeResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: "Stock Push Store", Code: "STOCKPUSH-STORE-1", Address: null));
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();

        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(product!.Id, store!.Id, 15, 79.90m, 99.90m));

        var pushResponse = await client.PostAsync("/api/trendyol-sync/stock-price/push", null);
        Assert.Equal(HttpStatusCode.OK, pushResponse.StatusCode);
        var summary = await pushResponse.Content.ReadFromJsonAsync<StockPriceSyncSummary>();
        Assert.NotNull(summary);
        Assert.True(summary!.TotalItems >= 1);

        var logsResponse = await client.GetAsync("/api/trendyol-sync/batch-requests");
        var logs = await logsResponse.Content.ReadFromJsonAsync<List<BatchRequestLogDto>>();
        Assert.Contains(logs!, l => l.OperationType == "PriceInventoryUpdate");
    }

    [Fact]
    public async Task PushStockPrice_SkipsRowsThatAreAlreadySyncedAndUnchanged()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-DELTA-1", Barcode: "BARCODE-DELTA-1", Name: "Delta Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var storeResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: "Delta Store", Code: "DELTA-STORE-1", Address: null));
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();

        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(product!.Id, store!.Id, 10, 50m, 60m));

        var syncedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var inventory = await dbContext.StoreProductInventories
                .SingleAsync(i => i.ProductId == product.Id && i.StoreId == store!.Id);
            inventory.LastSyncedQuantity = inventory.Quantity;
            inventory.LastSyncedSalePrice = inventory.SalePrice;
            inventory.LastSyncedAt = syncedAt;
            await dbContext.SaveChangesAsync();
        }

        await client.PostAsync("/api/trendyol-sync/stock-price/push", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var inventory = await dbContext.StoreProductInventories
                .SingleAsync(i => i.ProductId == product.Id && i.StoreId == store!.Id);
            // Untouched LastSyncedAt proves this row was excluded from the delta batch.
            Assert.Equal(syncedAt, inventory.LastSyncedAt);
        }
    }

    [Fact]
    public async Task PollBatchRequests_WithNoPendingBatches_ReturnsZeroPolled()
    {
        // Every batch submitted in this test host ends up Failed (Trendyol Go isn't
        // configured), never Pending, so this assertion is safe regardless of what sibling
        // tests in this shared-fixture class have already pushed.
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/trendyol-sync/batch-requests/poll", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<BatchPollSummary>();
        Assert.NotNull(summary);
        Assert.Equal(0, summary!.PolledCount);
    }

    [Fact]
    public async Task PushSaleStatus_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-sync/sale-status/push", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PushSaleStatus_WithChangedStatus_RecordsASellUnsellBatchLog()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SALEPUSH-1", Barcode: "BARCODE-SALEPUSH-1", Name: "Sale Push Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var storeResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: "Sale Push Store", Code: "SALEPUSH-STORE-1", Address: null));
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();

        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(product!.Id, store!.Id, 15, 79.90m, 99.90m));
        await client.PutAsJsonAsync(
            "/api/stock-price/sale-status", new SetSaleStatusRequest(product.Id, store!.Id, false, "OutOfStock"));

        var pushResponse = await client.PostAsync("/api/trendyol-sync/sale-status/push", null);
        Assert.Equal(HttpStatusCode.OK, pushResponse.StatusCode);
        var summary = await pushResponse.Content.ReadFromJsonAsync<SaleStatusSyncSummary>();
        Assert.NotNull(summary);
        Assert.True(summary!.TotalItems >= 1);

        var logsResponse = await client.GetAsync("/api/trendyol-sync/batch-requests");
        var logs = await logsResponse.Content.ReadFromJsonAsync<List<BatchRequestLogDto>>();
        Assert.Contains(logs!, l => l.OperationType == "SellUnsell");
    }

    [Fact]
    public async Task PushSaleStatus_SkipsRowsThatAreAlreadySyncedAndUnchanged()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SALEDELTA-1", Barcode: "BARCODE-SALEDELTA-1", Name: "Sale Delta Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var storeResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: "Sale Delta Store", Code: "SALEDELTA-STORE-1", Address: null));
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();

        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(product!.Id, store!.Id, 10, 50m, 60m));

        var syncedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var inventory = await dbContext.StoreProductInventories
                .SingleAsync(i => i.ProductId == product.Id && i.StoreId == store!.Id);
            inventory.LastSyncedIsOnSale = inventory.IsOnSale;
            inventory.SaleStatusLastSyncedAt = syncedAt;
            await dbContext.SaveChangesAsync();
        }

        await client.PostAsync("/api/trendyol-sync/sale-status/push", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
            var inventory = await dbContext.StoreProductInventories
                .SingleAsync(i => i.ProductId == product.Id && i.StoreId == store!.Id);
            // Untouched SaleStatusLastSyncedAt proves this row was excluded from the delta batch.
            Assert.Equal(syncedAt, inventory.SaleStatusLastSyncedAt);
        }
    }
}
