using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Couriers.Dtos;
using AslSu.Application.Orders.Dtos;
using AslSu.Application.Products.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class OrderCourierAndSubstitutionEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public OrderCourierAndSubstitutionEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(int OrderId, int ItemId)> SeedOrderWithItemAsync(string packageId, string barcode)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();

        var store = new Store { Name = "Assign Test Store", Code = $"ASSIGN-{packageId}", IsActive = true };
        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var order = new Order
        {
            PackageId = packageId,
            OrderNumber = $"ORD-{packageId}",
            StoreId = store.Id,
            Status = OrderStatus.Created,
            WorkflowStatus = WorkflowStatus.New,
            OrderDate = now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var item = new OrderItem { OrderId = order.Id, Barcode = barcode, Quantity = 1, UnitPrice = 10m };
        dbContext.OrderItems.Add(item);
        await dbContext.SaveChangesAsync();

        return (order.Id, item.Id);
    }

    [Fact]
    public async Task AssignCourier_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/orders/1/courier", new AssignCourierRequest(1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AssignCourier_OnMissingOrder_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var courierResponse = await client.PostAsJsonAsync("/api/couriers", new CreateCourierRequest("Ali Veli", null));
        var courier = await courierResponse.Content.ReadFromJsonAsync<CourierDto>();

        var response = await client.PutAsJsonAsync("/api/orders/999999/courier", new AssignCourierRequest(courier!.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignCourier_WithMissingCourier_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (orderId, _) = await SeedOrderWithItemAsync("PKG-COURIER-MISSING", "BARCODE-CM-1");

        var response = await client.PutAsJsonAsync($"/api/orders/{orderId}/courier", new AssignCourierRequest(999999));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignCourier_Success_ReturnsUpdatedOrderWithCourierName()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (orderId, _) = await SeedOrderWithItemAsync("PKG-COURIER-1", "BARCODE-C-1");

        var courierResponse = await client.PostAsJsonAsync("/api/couriers", new CreateCourierRequest("Ahmet Demir", "5551112233"));
        var courier = await courierResponse.Content.ReadFromJsonAsync<CourierDto>();

        var response = await client.PutAsJsonAsync($"/api/orders/{orderId}/courier", new AssignCourierRequest(courier!.Id));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        Assert.Equal(courier.Id, updated!.CourierId);
        Assert.Equal("Ahmet Demir", updated.CourierName);

        var listResponse = await client.GetAsync("/api/orders");
        var list = await listResponse.Content.ReadFromJsonAsync<List<OrderListItemDto>>();
        Assert.Contains(list!, o => o.Id == orderId && o.CourierName == "Ahmet Demir");
    }

    [Fact]
    public async Task SubstituteItem_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/orders/1/items/1/substitute", new SubstituteOrderItemRequest(1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubstituteItem_OnMissingOrder_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SUB-MISSING-ORDER", Barcode: "BARCODE-SUB-MO", Name: "Sub Test Product",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.PutAsJsonAsync(
            "/api/orders/999999/items/1/substitute", new SubstituteOrderItemRequest(product!.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubstituteItem_OnMissingItem_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (orderId, _) = await SeedOrderWithItemAsync("PKG-SUB-MISSING-ITEM", "BARCODE-SMI-1");

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SUB-MISSING-ITEM", Barcode: "BARCODE-SUB-MI", Name: "Sub Test Product 2",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.PutAsJsonAsync(
            $"/api/orders/{orderId}/items/999999/substitute", new SubstituteOrderItemRequest(product!.Id));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubstituteItem_WithMissingProduct_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (orderId, itemId) = await SeedOrderWithItemAsync("PKG-SUB-MISSING-PRODUCT", "BARCODE-SMP-1");

        var response = await client.PutAsJsonAsync(
            $"/api/orders/{orderId}/items/{itemId}/substitute", new SubstituteOrderItemRequest(999999));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubstituteItem_Success_UpdatesItemAndFlagsSubstitution()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var (orderId, itemId) = await SeedOrderWithItemAsync("PKG-SUB-1", "BARCODE-ORIGINAL-1");

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Sku: "SKU-SUB-ALT-1", Barcode: "BARCODE-ALTERNATIVE-1", Name: "Alternatif Ürün",
            Description: null, CategoryId: null, BrandId: null, VatRate: 18m, ImageUrl: null));
        var altProduct = await productResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.PutAsJsonAsync(
            $"/api/orders/{orderId}/items/{itemId}/substitute", new SubstituteOrderItemRequest(altProduct!.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        var item = Assert.Single(updated!.Items, i => i.Id == itemId);
        Assert.Equal(altProduct.Id, item.ProductId);
        Assert.Equal("BARCODE-ALTERNATIVE-1", item.Barcode);
        Assert.True(item.IsSubstitution);
        Assert.Equal("BARCODE-ORIGINAL-1", item.SubstitutedForBarcode);
    }
}
