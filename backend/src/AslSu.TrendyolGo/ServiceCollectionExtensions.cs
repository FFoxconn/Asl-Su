using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Connection;
using AslSu.TrendyolGo.Http;
using AslSu.TrendyolGo.Orders;
using AslSu.TrendyolGo.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace AslSu.TrendyolGo;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers TrendyolGoOptions (validated: required in Production, optional
    /// elsewhere — Development, Testing, etc. — so the app and test hosts still run before
    /// credentials are configured), the auth-header delegating handler, a typed HttpClient
    /// enforcing TLS 1.2+, and 429/5xx retry-with-backoff — plus the Trendyol Go client
    /// interfaces.</summary>
    public static IServiceCollection AddTrendyolGoClient(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<TrendyolGoOptions>()
            .Bind(configuration.GetSection(TrendyolGoOptions.SectionName))
            .Validate(
                o => !environment.IsProduction() || o.IsConfigured,
                "TrendyolGo:SupplierId/ApiKey/ApiSecret/BaseUrl are not fully configured. Set them via " +
                "'dotnet user-secrets set \"TrendyolGo:<Key>\" \"<value>\"' in development, or " +
                "TrendyolGo__<Key> environment variables in other environments.")
            .ValidateOnStart();

        services.AddTransient<TrendyolGoAuthHandler>();

        ConfigureTrendyolHttpClient(services.AddHttpClient<ITrendyolOrderClient, TrendyolOrderClient>());
        ConfigureTrendyolHttpClient(services.AddHttpClient<ITrendyolProductClient, TrendyolProductClient>());

        services.AddScoped<ITrendyolConnectionTester, TrendyolConnectionTester>();

        return services;
    }

    /// <summary>Applies the settings every Trendyol Go typed HttpClient shares: BaseUrl,
    /// TLS 1.2+, the auth-header handler, and 429/5xx retry-with-backoff.</summary>
    private static void ConfigureTrendyolHttpClient(IHttpClientBuilder builder)
    {
        builder
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<TrendyolGoOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl);
                }
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                },
            })
            .AddHttpMessageHandler<TrendyolGoAuthHandler>()
            .AddResilienceHandler("trendyol-go-retry", resilienceBuilder =>
            {
                resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result is { } response &&
                        (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1),
                });
            });
    }
}
