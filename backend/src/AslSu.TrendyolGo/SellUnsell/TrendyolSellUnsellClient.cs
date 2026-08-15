using System.Net.Http.Json;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.SellUnsell;

public class TrendyolSellUnsellClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options)
    : ITrendyolSellUnsellClient
{
    public Task<TrendyolBatchSubmitResult> SetSaleStatusBatchAsync(
        IReadOnlyList<TrendyolSaleStatusItemPayload> items, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        if (!opts.IsConfigured)
        {
            return Task.FromResult(TrendyolBatchSubmitResult.Fail(
                "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış."));
        }

        var endpointPath = opts.SellUnsellEndpointPath;

        if (string.IsNullOrWhiteSpace(endpointPath))
        {
            return Task.FromResult(TrendyolBatchSubmitResult.Fail(
                "Satışa açma/kapatma endpoint'i henüz yapılandırılmamış. developers.tgoapps.com " +
                "belgelerini kontrol edip TrendyolGo:SellUnsellEndpointPath değerini ayarlayın."));
        }

        // Request body shape is our best-effort guess pending schema confirmation —
        // see TrendyolSaleStatusItemPayload's remarks.
        return TrendyolBatchSubmitHelper.ExecuteAsync(
            ct => httpClient.PostAsJsonAsync(endpointPath, new { items }, ct),
            cancellationToken);
    }
}
