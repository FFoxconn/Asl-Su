using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Catalog.Dtos;
using AslSu.Application.Products.Dtos;
using AslSu.Application.StockPrice.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class StockPriceEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public StockPriceEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<(int ProductId, int StoreId)> SeedProductAndStoreAsync(HttpClient client, string suffix)
    {
        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: $"SKU-SP-{suffix}", Barcode: $"BARCODE-SP-{suffix}", Name: $"Stock Test Product {suffix}",
            Description: null, CategoryId: null, BrandId: null, VatRate: 10m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var storeResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest(
            Name: $"Store {suffix}", Code: $"STORE-{suffix}", Address: null));
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();

        return (product!.Id, store!.Id);
    }

    [Fact]
    public async Task Upsert_NewRow_CreatesInventoryAndIsRetrievable()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (productId, storeId) = await SeedProductAndStoreAsync(client, "F1");

        var upsertResponse = await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(
            productId, storeId, Quantity: 15, SalePrice: 79.90m, ListPrice: 99.90m));

        Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);
        var upserted = await upsertResponse.Content.ReadFromJsonAsync<StockPriceDto>();
        Assert.Equal(15, upserted!.Quantity);
        Assert.Equal(79.90m, upserted.SalePrice);

        var getResponse = await client.GetAsync($"/api/stock-price?productId={productId}");
        var rows = await getResponse.Content.ReadFromJsonAsync<List<StockPriceDto>>();
        Assert.Single(rows!);
        Assert.Equal(storeId, rows![0].StoreId);
    }

    [Fact]
    public async Task Upsert_ExistingRow_UpdatesValuesInPlace()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (productId, storeId) = await SeedProductAndStoreAsync(client, "F2");

        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(productId, storeId, 10, 50m, 60m));
        var secondResponse = await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(productId, storeId, 3, 45m, 60m));

        var updated = await secondResponse.Content.ReadFromJsonAsync<StockPriceDto>();
        Assert.Equal(3, updated!.Quantity);
        Assert.Equal(45m, updated.SalePrice);

        var getResponse = await client.GetAsync($"/api/stock-price?productId={productId}");
        var rows = await getResponse.Content.ReadFromJsonAsync<List<StockPriceDto>>();
        Assert.Single(rows!);
    }

    [Fact]
    public async Task GetByProduct_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/stock-price?productId=1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SetSaleStatus_ForNonExistentRow_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PutAsJsonAsync(
            "/api/stock-price/sale-status", new SetSaleStatusRequest(999999, 999999, false, "OutOfStock"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetSaleStatus_TogglesIsOnSaleAndReason()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (productId, storeId) = await SeedProductAndStoreAsync(client, "F3");
        await client.PutAsJsonAsync("/api/stock-price", new UpsertStockPriceRequest(productId, storeId, 10, 50m, 60m));

        var disableResponse = await client.PutAsJsonAsync(
            "/api/stock-price/sale-status", new SetSaleStatusRequest(productId, storeId, false, "OutOfStock"));

        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);
        var disabled = await disableResponse.Content.ReadFromJsonAsync<StockPriceDto>();
        Assert.False(disabled!.IsOnSale);
        Assert.Equal("OutOfStock", disabled.UnsaleReasonCode);

        var enableResponse = await client.PutAsJsonAsync(
            "/api/stock-price/sale-status", new SetSaleStatusRequest(productId, storeId, true, null));
        var enabled = await enableResponse.Content.ReadFromJsonAsync<StockPriceDto>();
        Assert.True(enabled!.IsOnSale);
        Assert.Null(enabled.UnsaleReasonCode);
    }

    [Fact]
    public async Task SetSaleStatus_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            "/api/stock-price/sale-status", new SetSaleStatusRequest(1, 1, false, "OutOfStock"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
