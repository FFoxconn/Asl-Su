using System.Net;
using System.Net.Http.Json;
using AslSu.Application.Couriers.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class CouriersEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public CouriersEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCouriers_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/couriers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourier_ThenListIncludesIt()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/couriers", new CreateCourierRequest("Mehmet Kaya", "5559998877"));
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CourierDto>();
        Assert.NotNull(created);
        Assert.True(created!.IsActive);

        var listResponse = await client.GetAsync("/api/couriers");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var couriers = await listResponse.Content.ReadFromJsonAsync<List<CourierDto>>();
        Assert.Contains(couriers!, c => c.Id == created.Id && c.Name == "Mehmet Kaya");
    }
}
