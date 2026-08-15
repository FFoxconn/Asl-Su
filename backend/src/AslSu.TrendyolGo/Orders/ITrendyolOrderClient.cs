namespace AslSu.TrendyolGo.Orders;

public interface ITrendyolOrderClient
{
    /// <summary>Calls the packages (orders) endpoint and returns the raw response. Used both
    /// as a lightweight connection probe (Phase 4) and, once the response schema is confirmed
    /// against developers.tgoapps.com, will grow into the real order-pull client (Phase 8).
    /// Query parameters (date range, pagination, status filter) are not sent yet — TODO:
    /// confirm exact required parameters against the docs before relying on this for real
    /// order sync.</summary>
    Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default);
}
