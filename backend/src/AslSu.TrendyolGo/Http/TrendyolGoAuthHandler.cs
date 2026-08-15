using System.Net.Http.Headers;
using System.Text;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Http;

/// <summary>Injects Basic Authentication and the required Trendyol Go headers on every
/// outgoing request. Never logs the Authorization header or the raw credentials.</summary>
public class TrendyolGoAuthHandler(IOptions<TrendyolGoOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opts.ApiKey}:{opts.ApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        request.Headers.UserAgent.Clear();
        request.Headers.UserAgent.ParseAdd($"{opts.SupplierId} - SelfIntegration");

        if (!string.IsNullOrWhiteSpace(opts.AgentName))
        {
            request.Headers.Remove("x-agentname");
            request.Headers.Add("x-agentname", opts.AgentName);
        }

        if (!string.IsNullOrWhiteSpace(opts.ExecutorUser))
        {
            request.Headers.Remove("x-executor-user");
            request.Headers.Add("x-executor-user", opts.ExecutorUser);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
