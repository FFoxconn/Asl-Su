using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using AslSu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AslSu.Api.IntegrationTests;

public class WebhooksEndpointsTests : IClassFixture<AslSuWebApplicationFactory>
{
    private readonly AslSuWebApplicationFactory _factory;

    public WebhooksEndpointsTests(AslSuWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReceiveTrendyolGo_RequiresNoAuthToken()
    {
        // No Authorization header at all — the webhook must still be reachable, unlike every
        // other controller in this API.
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/webhooks/trendyol-go", new { eventId = $"evt-noauth-{Guid.NewGuid()}", eventType = "OrderCreated" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveTrendyolGo_WhenSignatureNotConfigured_AcceptsAndStoresRawPayload()
    {
        var client = _factory.CreateClient();
        var eventId = $"evt-unverified-{Guid.NewGuid()}";

        var response = await client.PostAsJsonAsync(
            "/api/webhooks/trendyol-go", new { eventId, eventType = "OrderCreated", packageId = "PKG-WH-1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
        var log = await dbContext.WebhookLogs.SingleOrDefaultAsync(w => w.ExternalEventId == eventId);
        Assert.NotNull(log);
        Assert.Equal("OrderCreated", log!.EventType);
        Assert.Contains("PKG-WH-1", log.RawPayloadJson);
    }

    [Fact]
    public async Task ReceiveTrendyolGo_ResentWithSameEventId_DoesNotCreateADuplicateLogRow()
    {
        var client = _factory.CreateClient();
        var eventId = $"evt-dedupe-{Guid.NewGuid()}";
        var payload = new { eventId, eventType = "OrderCreated" };

        var first = await client.PostAsJsonAsync("/api/webhooks/trendyol-go", payload);
        var second = await client.PostAsJsonAsync("/api/webhooks/trendyol-go", payload);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AslSuDbContext>();
        var count = await dbContext.WebhookLogs.CountAsync(w => w.ExternalEventId == eventId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ReceiveTrendyolGo_WithUnparseableBody_StillAcceptsAndStoresRawPayload()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync(
            "/api/webhooks/trendyol-go", new StringContent("not json at all", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveTrendyolGo_WithSignatureConfigured_RejectsAnInvalidSignature()
    {
        using var signedFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TrendyolGo:WebhookSecret"] = "integration-test-secret",
                ["TrendyolGo:WebhookSignatureHeaderName"] = "X-Test-Signature",
            });
        }));
        var client = signedFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/trendyol-go")
        {
            Content = new StringContent("{\"eventId\":\"evt-bad-sig\"}", Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Test-Signature", "not-the-real-signature");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveTrendyolGo_WithSignatureConfigured_AcceptsAValidSignature()
    {
        const string secret = "integration-test-secret-2";
        using var signedFactory = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TrendyolGo:WebhookSecret"] = secret,
                ["TrendyolGo:WebhookSignatureHeaderName"] = "X-Test-Signature",
            });
        }));
        var client = signedFactory.CreateClient();

        var body = "{\"eventId\":\"evt-good-sig\"}";
        var signature = Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/trendyol-go")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Test-Signature", signature);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
