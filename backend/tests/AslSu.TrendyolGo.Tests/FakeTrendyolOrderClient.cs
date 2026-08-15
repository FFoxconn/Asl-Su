using AslSu.TrendyolGo.Orders;

namespace AslSu.TrendyolGo.Tests;

public class FakeTrendyolOrderClient(Func<HttpResponseMessage> respond) : ITrendyolOrderClient
{
    public Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(respond());
}
