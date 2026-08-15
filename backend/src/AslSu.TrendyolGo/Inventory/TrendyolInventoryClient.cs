using System.Net.Http.Json;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Inventory;

public class TrendyolInventoryClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options)
    : ITrendyolInventoryClient
{
    public Task<TrendyolBatchSubmitResult> PushPriceAndInventoryBatchAsync(
        IReadOnlyList<TrendyolInventoryItemPayload> items, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        if (!opts.IsConfigured)
        {
            return Task.FromResult(TrendyolBatchSubmitResult.Fail(
                "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış."));
        }

        // Endpoint path given verbatim in the integration brief. Request body shape is a
        // best-effort guess pending schema confirmation — see TrendyolInventoryItemPayload's
        // remarks.
        return TrendyolBatchSubmitHelper.ExecuteAsync(
            ct => httpClient.PostAsJsonAsync(
                $"/integrator/product/grocery/suppliers/{opts.SupplierId}/products/price-and-inventory",
                new { items },
                ct),
            cancellationToken);
    }
}
