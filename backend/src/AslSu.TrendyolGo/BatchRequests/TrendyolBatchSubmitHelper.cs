using System.Text.Json;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;

namespace AslSu.TrendyolGo.BatchRequests;

/// <summary>Shared POST-a-batch-and-map-the-outcome logic for the product-creation and
/// price-and-inventory clients, so the 401/403/429/5xx-to-message mapping (and the
/// best-effort batchRequestId extraction) isn't duplicated between them.</summary>
internal static class TrendyolBatchSubmitHelper
{
    public static async Task<TrendyolBatchSubmitResult> ExecuteAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest, CancellationToken cancellationToken)
    {
        try
        {
            var response = await sendRequest(cancellationToken);
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
            return TrendyolBatchSubmitResult.Fail(TrendyolErrorMessages.CheckCredentials);
        }
        catch (TrendyolAuthException)
        {
            return TrendyolBatchSubmitResult.Fail(TrendyolErrorMessages.CheckHeaders);
        }
        catch (TrendyolRateLimitException)
        {
            return TrendyolBatchSubmitResult.Fail(TrendyolErrorMessages.RateLimited);
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return TrendyolBatchSubmitResult.Fail(TrendyolErrorMessages.ServiceError);
        }
        catch (TrendyolApiException ex)
        {
            return TrendyolBatchSubmitResult.Fail($"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).");
        }
        catch (HttpRequestException)
        {
            return TrendyolBatchSubmitResult.Fail(TrendyolErrorMessages.ConnectionFailed);
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
