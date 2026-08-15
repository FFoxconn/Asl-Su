using System.Net.Http.Json;
using System.Text.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Products;

public class TrendyolProductClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options) : ITrendyolProductClient
{
    public async Task<TrendyolBatchSubmitResult> CreateProductsBatchAsync(
        IReadOnlyList<TrendyolProductPayload> products, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        if (string.IsNullOrWhiteSpace(opts.ProductsEndpointPath))
        {
            return TrendyolBatchSubmitResult.Fail(
                "Ürün oluşturma endpoint'i henüz yapılandırılmamış. developers.tgoapps.com belgelerini " +
                "kontrol edip TrendyolGo:ProductsEndpointPath değerini ayarlayın.");
        }

        try
        {
            // Request body shape is our best-effort guess pending schema confirmation —
            // see TrendyolProductPayload's remarks.
            var response = await httpClient.PostAsJsonAsync(
                opts.ProductsEndpointPath, new { items = products }, cancellationToken);
            await TrendyolResponseHandler.EnsureSuccessAsync(response, cancellationToken);

            var batchRequestId = await TryReadBatchRequestIdAsync(response, cancellationToken);
            return batchRequestId is null
                ? TrendyolBatchSubmitResult.Fail(
                    "Trendyol Go isteği kabul etti ancak yanıtta batchRequestId bulunamadı — yanıt şemasını " +
                    "developers.tgoapps.com ile karşılaştırın.")
                : TrendyolBatchSubmitResult.Ok(batchRequestId);
        }
        catch (TrendyolAuthException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return TrendyolBatchSubmitResult.Fail("API Key / API Secret / Supplier ID bilgilerini kontrol edin.");
        }
        catch (TrendyolAuthException)
        {
            return TrendyolBatchSubmitResult.Fail("User-Agent veya yetkilendirme bilgilerini kontrol edin.");
        }
        catch (TrendyolRateLimitException)
        {
            return TrendyolBatchSubmitResult.Fail("API rate limitine ulaşıldı.");
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return TrendyolBatchSubmitResult.Fail("Trendyol Go servisinde geçici hata.");
        }
        catch (TrendyolApiException ex)
        {
            return TrendyolBatchSubmitResult.Fail($"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).");
        }
        catch (HttpRequestException)
        {
            return TrendyolBatchSubmitResult.Fail("Trendyol Go'ya bağlanılamadı. BaseUrl bilgisini kontrol edin.");
        }
    }

    private static async Task<string?> TryReadBatchRequestIdAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return document.RootElement.TryGetProperty("batchRequestId", out var value)
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
