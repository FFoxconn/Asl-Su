using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Catalog.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class CatalogEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public CatalogEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStores_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/stores");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStore_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/stores", new CreateStoreRequest("Şube", "SHB-UNAUTH", null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest("Kategori", null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBrands_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/brands");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBrand_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/brands", new CreateBrandRequest("Marka"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStore_ThenGetStores_ReturnsTheNewStore()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/stores", new CreateStoreRequest("Merkez Şube", $"SHB-{Guid.NewGuid():N}", "Test Adres"));
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<StoreDto>();
        Assert.NotNull(created);

        var listResponse = await client.GetAsync("/api/stores");
        var stores = await listResponse.Content.ReadFromJsonAsync<List<StoreDto>>();

        Assert.Contains(stores!, s => s.Id == created!.Id);
    }

    [Fact]
    public async Task CreateCategory_ThenGetCategories_ReturnsTheNewCategory()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/categories", new CreateCategoryRequest($"Kategori-{Guid.NewGuid():N}", null));
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(created);

        var listResponse = await client.GetAsync("/api/categories");
        var categories = await listResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();

        Assert.Contains(categories!, c => c.Id == created!.Id);
    }

    [Fact]
    public async Task CreateBrand_ThenGetBrands_ReturnsTheNewBrand()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/brands", new CreateBrandRequest($"Marka-{Guid.NewGuid():N}"));
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDto>();
        Assert.NotNull(created);

        var listResponse = await client.GetAsync("/api/brands");
        var brands = await listResponse.Content.ReadFromJsonAsync<List<BrandDto>>();

        Assert.Contains(brands!, b => b.Id == created!.Id);
    }
}
