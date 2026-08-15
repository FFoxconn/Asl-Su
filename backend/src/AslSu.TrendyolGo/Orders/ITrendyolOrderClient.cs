namespace AslSu.TrendyolGo.Orders;

public interface ITrendyolOrderClient
{
    /// <summary>Calls the packages (orders) endpoint and returns the raw response. Used both
    /// as a lightweight connection probe (Phase 4) and as the basis for <see cref="GetPackagesAsync"/>
    /// (Phase 8). Query parameters (date range, pagination, status filter) are not sent yet — TODO:
    /// confirm exact required parameters against the docs before relying on this for real
    /// order sync.</summary>
    Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default);

    /// <summary>Calls the packages endpoint and parses the response into
    /// <see cref="TrendyolPackageDto"/> instances, best-effort — see that type's remarks.</summary>
    Task<TrendyolPackagesOutcome> GetPackagesAsync(CancellationToken cancellationToken = default);
}
