using System.Net;
using System.Net.Http.Json;
using AslSu.Application.TrendyolSettings.Dtos;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class TrendyolSettingsEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public TrendyolSettingsEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSettings_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/trendyol-settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_NoCredentialsConfiguredInTestHost_ReturnsNotConfigured()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/trendyol-settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var settings = await response.Content.ReadFromJsonAsync<TrendyolSettingsDto>();
        Assert.NotNull(settings);
        Assert.False(settings!.IsConfigured);
    }

    [Fact]
    public async Task TestConnection_NoCredentialsConfiguredInTestHost_ReturnsNotConfiguredMessageWithoutHittingNetwork()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsync("/api/trendyol-settings/test-connection", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TestConnectionResponse>();
        Assert.NotNull(result);
        Assert.False(result!.Success);
        Assert.Equal("Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.", result.Message);
    }

    [Fact]
    public async Task TestConnection_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/trendyol-settings/test-connection", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
