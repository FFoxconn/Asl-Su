using System.Net;
using System.Net.Http.Json;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class OrderWorkflowEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public OrderWorkflowEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<int> SeedOrderAsync(string packageId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();

        var store = new Store { Name = "Workflow Test Store", Code = $"WF-{packageId}", IsActive = true };
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
        return order.Id;
    }

    [Fact]
    public async Task Accept_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/orders/1/accept", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Accept_OnMissingOrder_ReturnsNotFound()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/orders/999999/accept", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Accept_OnNewOrder_AdvancesLocallyEvenThoughTrendyolIsNotConfiguredInTests()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var orderId = await SeedOrderAsync("PKG-WF-ACCEPT-1");

        var response = await client.PostAsync($"/api/orders/{orderId}/accept", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WorkflowActionResponse>();
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Accepted", result.WorkflowStatus);
        Assert.False(result.TrendyolNotified);
        Assert.False(string.IsNullOrWhiteSpace(result.TrendyolMessage));
    }

    [Fact]
    public async Task Accept_CalledTwice_SecondCallReturnsConflict()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var orderId = await SeedOrderAsync("PKG-WF-ACCEPT-2");

        var first = await client.PostAsync($"/api/orders/{orderId}/accept", null);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsync($"/api/orders/{orderId}/accept", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task StartPreparing_OnNewOrder_ReturnsConflictSinceAcceptMustHappenFirst()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var orderId = await SeedOrderAsync("PKG-WF-SKIP-1");

        var response = await client.PostAsync($"/api/orders/{orderId}/start-preparing", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task FullWorkflow_AdvancesThroughAllFourStepsInOrder()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);
        var orderId = await SeedOrderAsync("PKG-WF-FULL-1");

        var accept = await client.PostAsync($"/api/orders/{orderId}/accept", null);
        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);
        Assert.Equal("Accepted", (await accept.Content.ReadFromJsonAsync<WorkflowActionResponse>())!.WorkflowStatus);

        var preparing = await client.PostAsync($"/api/orders/{orderId}/start-preparing", null);
        Assert.Equal(HttpStatusCode.OK, preparing.StatusCode);
        Assert.Equal("Preparing", (await preparing.Content.ReadFromJsonAsync<WorkflowActionResponse>())!.WorkflowStatus);

        var prepared = await client.PostAsync($"/api/orders/{orderId}/mark-prepared", null);
        Assert.Equal(HttpStatusCode.OK, prepared.StatusCode);
        Assert.Equal("Prepared", (await prepared.Content.ReadFromJsonAsync<WorkflowActionResponse>())!.WorkflowStatus);

        var delivered = await client.PostAsync($"/api/orders/{orderId}/deliver", null);
        Assert.Equal(HttpStatusCode.OK, delivered.StatusCode);
        Assert.Equal("Delivered", (await delivered.Content.ReadFromJsonAsync<WorkflowActionResponse>())!.WorkflowStatus);

        var detailResponse = await client.GetAsync($"/api/orders/{orderId}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal("Delivered", detail.GetProperty("workflowStatus").GetString());
    }

    private record WorkflowActionResponse(bool Success, string? WorkflowStatus, bool TrendyolNotified, string? TrendyolMessage);
}
