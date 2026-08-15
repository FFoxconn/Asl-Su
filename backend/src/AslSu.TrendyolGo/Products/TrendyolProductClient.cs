using System.Net.Http.Json;
using AslSu.TrendyolGo.BatchRequests;
using AslSu.TrendyolGo.Configuration;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Products;

public class TrendyolProductClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options) : ITrendyolProductClient
{
    public Task<TrendyolBatchSubmitResult> CreateProductsBatchAsync(
        IReadOnlyList<TrendyolProductPayload> products, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        if (!opts.IsConfigured)
        {
            return Task.FromResult(TrendyolBatchSubmitResult.Fail(
                "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış."));
        }

        var endpointPath = opts.ProductsEndpointPath;

        if (string.IsNullOrWhiteSpace(endpointPath))
        {
            return Task.FromResult(TrendyolBatchSubmitResult.Fail(
                "Ürün oluşturma endpoint'i henüz yapılandırılmamış. developers.tgoapps.com belgelerini " +
                "kontrol edip TrendyolGo:ProductsEndpointPath değerini ayarlayın."));
        }

        // Request body shape is our best-effort guess pending schema confirmation —
        // see TrendyolProductPayload's remarks.
        return TrendyolBatchSubmitHelper.ExecuteAsync(
            ct => httpClient.PostAsJsonAsync(endpointPath, new { items = products }, ct),
            cancellationToken);
    }
}
