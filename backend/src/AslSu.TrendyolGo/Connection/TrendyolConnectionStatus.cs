namespace AslSu.TrendyolGo.Connection;

public enum TrendyolConnectionStatus
{
    Success,
    NotConfigured,
    Unauthorized,
    Forbidden,
    RateLimited,
    ServiceError,
    Unknown,
}
