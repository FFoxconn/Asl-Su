using System.Net;

namespace AslSu.TrendyolGo.Exceptions;

/// <summary>401 (bad Key/Secret/SupplierId) or 403 (bad required headers) from Trendyol Go.</summary>
public class TrendyolAuthException(string message, HttpStatusCode statusCode, string? responseBody)
    : TrendyolApiException(message, statusCode, responseBody);
