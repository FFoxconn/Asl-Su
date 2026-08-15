using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Orders;

public class TrendyolOrderClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options) : ITrendyolOrderClient
{
    public Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default)
    {
        var supplierId = options.Value.SupplierId;
        // Endpoint path given in the integration brief; exact query parameters are
        // TODO: confirm against developers.tgoapps.com before relying on this for real order sync.
        return httpClient.GetAsync($"/integrator/order/grocery/suppliers/{supplierId}/packages", cancellationToken);
    }
}
