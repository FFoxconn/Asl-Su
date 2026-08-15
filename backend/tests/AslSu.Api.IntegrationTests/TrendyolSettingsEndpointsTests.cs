using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AslSu.Application.TrendyolSettings.Dtos;
using Microsoft.Extensions.Configuration;
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

    [Fact]
    public async Task GetSettings_WithRealCredentialsConfigured_NeverReturnsTheRawApiSecret()
    {
        const string apiSecret = "super-secret-value-should-never-leak";
        using var configuredFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TrendyolGo:SupplierId"] = "123456",
                ["TrendyolGo:ApiKey"] = "some-api-key-value",
                ["TrendyolGo:ApiSecret"] = apiSecret,
                ["TrendyolGo:BaseUrl"] = "https://example.invalid",
            });
        }));
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(configuredFactory);

        var response = await client.GetAsync("/api/trendyol-settings");
        var rawBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(apiSecret, rawBody);
        var settings = await JsonSerializer.DeserializeAsync<TrendyolSettingsDto>(
            new MemoryStream(Encoding.UTF8.GetBytes(rawBody)),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(settings);
        Assert.True(settings!.IsConfigured);
        Assert.NotEqual("some-api-key-value", settings.MaskedApiKey);
    }
}
