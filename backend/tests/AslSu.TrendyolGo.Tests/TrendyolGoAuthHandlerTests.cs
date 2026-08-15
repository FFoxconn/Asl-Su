using System.Net;
using System.Text;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolGoAuthHandlerTests
{
    private static TrendyolGoOptions ValidOptions() => new()
    {
        SupplierId = "123456",
        ApiKey = "test-key",
        ApiSecret = "test-secret",
        BaseUrl = "https://example.invalid",
        AgentName = "AslSu-Agent",
        ExecutorUser = "aslsu-system",
    };

    [Fact]
    public async Task SendAsync_AddsBasicAuthHeaderFromApiKeyAndSecret()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var authHandler = new TrendyolGoAuthHandler(Options.Create(ValidOptions())) { InnerHandler = fake };
        var invoker = new HttpMessageInvoker(authHandler);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test"), CancellationToken.None);

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("Basic", sent.Headers.Authorization!.Scheme);
        var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-key:test-secret"));
        Assert.Equal(expected, sent.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_SetsUserAgentToSupplierIdSelfIntegrationFormat()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var authHandler = new TrendyolGoAuthHandler(Options.Create(ValidOptions())) { InnerHandler = fake };
        var invoker = new HttpMessageInvoker(authHandler);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test"), CancellationToken.None);

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("123456 - SelfIntegration", sent.Headers.UserAgent.ToString());
    }

    [Fact]
    public async Task SendAsync_AddsAgentNameAndExecutorUserHeaders()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var authHandler = new TrendyolGoAuthHandler(Options.Create(ValidOptions())) { InnerHandler = fake };
        var invoker = new HttpMessageInvoker(authHandler);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test"), CancellationToken.None);

        var sent = Assert.Single(fake.Requests);
        Assert.Equal("AslSu-Agent", sent.Headers.GetValues("x-agentname").Single());
        Assert.Equal("aslsu-system", sent.Headers.GetValues("x-executor-user").Single());
    }

    [Fact]
    public async Task SendAsync_NeverPutsCredentialsInAnyHeaderOtherThanAuthorization()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var authHandler = new TrendyolGoAuthHandler(Options.Create(ValidOptions())) { InnerHandler = fake };
        var invoker = new HttpMessageInvoker(authHandler);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test"), CancellationToken.None);

        var sent = Assert.Single(fake.Requests);
        var nonAuthHeaderValues = sent.Headers
            .Where(h => h.Key != "Authorization")
            .SelectMany(h => h.Value);
        Assert.DoesNotContain(nonAuthHeaderValues, v => v.Contains("test-secret"));
    }
}
