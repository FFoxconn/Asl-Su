using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Products.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class ProductsEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public ProductsEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static CreateProductRequest NewProductRequest(string suffix) => new(
        Sku: $"SKU-{suffix}",
        Barcode: $"BARCODE-{suffix}",
        Name: $"Test Product {suffix}",
        Description: "A product used in integration tests.",
        CategoryId: null,
        BrandId: null,
        VatRate: 18m,
        ImageUrl: null);

    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreatedAndIsListed()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync("/api/products", NewProductRequest("A1"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("SKU-A1", created!.Sku);

        var listResponse = await client.GetAsync("/api/products");
        var products = await listResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.Contains(products!, p => p.Sku == "SKU-A1");
    }

    [Fact]
    public async Task Create_DuplicateBarcode_ReturnsConflict()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PostAsJsonAsync("/api/products", NewProductRequest("B1"));
        var duplicateRequest = NewProductRequest("B2") with { Barcode = "BARCODE-B1" };

        var response = await client.PostAsJsonAsync("/api/products", duplicateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateSku_ReturnsConflict()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        await client.PostAsJsonAsync("/api/products", NewProductRequest("C1"));
        var duplicateRequest = NewProductRequest("C2") with { Sku = "SKU-C1" };

        var response = await client.PostAsJsonAsync("/api/products", duplicateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingProduct_ChangesName()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var createResponse = await client.PostAsJsonAsync("/api/products", NewProductRequest("D1"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateRequest = new UpdateProductRequest(
            Name: "Renamed Product",
            Description: created!.Description,
            CategoryId: null,
            BrandId: null,
            VatRate: created.VatRate,
            ImageUrl: null,
            IsActive: true);

        var updateResponse = await client.PutAsJsonAsync($"/api/products/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Renamed Product", updated!.Name);
    }

    [Fact]
    public async Task Update_NonExistentProduct_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var updateRequest = new UpdateProductRequest("Nope", null, null, null, 0, null, true);

        var response = await client.PutAsJsonAsync("/api/products/999999", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingProduct_RemovesIt()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var createResponse = await client.PostAsJsonAsync("/api/products", NewProductRequest("E1"));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var deleteResponse = await client.DeleteAsync($"/api/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
