using System.Net;
using AslSu.TrendyolGo.Exceptions;

namespace AslSu.TrendyolGo.Http;

public static class TrendyolResponseHandler
{
    /// <summary>Throws a typed exception for any non-success response. The response body is
    /// captured for server-side logging only.</summary>
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new TrendyolAuthException(
                "Trendyol Go authentication failed (401).", response.StatusCode, body),
            HttpStatusCode.Forbidden => new TrendyolAuthException(
                "Trendyol Go authorization rejected (403).", response.StatusCode, body),
            HttpStatusCode.TooManyRequests => new TrendyolRateLimitException(response.StatusCode, body),
            _ => new TrendyolApiException(
                $"Trendyol Go request failed ({(int)response.StatusCode}).", response.StatusCode, body),
        };
    }
}
