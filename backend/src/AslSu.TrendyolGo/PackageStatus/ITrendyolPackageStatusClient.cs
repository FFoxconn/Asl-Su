namespace AslSu.TrendyolGo.PackageStatus;

public interface ITrendyolPackageStatusClient
{
    /// <summary>Notifies Trendyol Go that a package has been accepted for preparation.
    /// Returns a failure outcome — never throws for a "not configured" state — if
    /// TrendyolGoOptions.AcceptOrderEndpointPath hasn't been set yet.</summary>
    Task<TrendyolPackageActionOutcome> AcceptPackageAsync(string packageId, CancellationToken cancellationToken = default);

    /// <summary>Notifies Trendyol Go that a package has been invoiced. Same "not configured"
    /// behavior as <see cref="AcceptPackageAsync"/>.</summary>
    Task<TrendyolPackageActionOutcome> InvoicePackageAsync(string packageId, CancellationToken cancellationToken = default);

    /// <summary>Notifies Trendyol Go that a package has shipped. Same "not configured"
    /// behavior as <see cref="AcceptPackageAsync"/>.</summary>
    Task<TrendyolPackageActionOutcome> ShipPackageAsync(string packageId, CancellationToken cancellationToken = default);
}
