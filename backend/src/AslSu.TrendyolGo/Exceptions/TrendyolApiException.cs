using System.Net;

namespace AslSu.TrendyolGo.Exceptions;

/// <summary>Base exception for a non-success Trendyol Go API response. ResponseBody is
/// for server-side logging only — never surface it directly to web/mobile clients, since
/// it may contain data we shouldn't echo back verbatim.</summary>
public class TrendyolApiException(string message, HttpStatusCode statusCode, string? responseBody)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}
