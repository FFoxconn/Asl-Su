using AslSu.TrendyolGo.Orders;

namespace AslSu.TrendyolGo.Tests;

public class FakeTrendyolOrderClient(Func<HttpResponseMessage> respond) : ITrendyolOrderClient
{
    public Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(respond());

    public Task<TrendyolPackagesOutcome> GetPackagesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(TrendyolPackagesOutcome.Fail("Not implemented in this fake."));
}
