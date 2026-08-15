using System.Net;

namespace AslSu.TrendyolGo.Exceptions;

/// <summary>429 from Trendyol Go — rate limit reached.</summary>
public class TrendyolRateLimitException(HttpStatusCode statusCode, string? responseBody)
    : TrendyolApiException("Trendyol Go API rate limit reached.", statusCode, responseBody);
