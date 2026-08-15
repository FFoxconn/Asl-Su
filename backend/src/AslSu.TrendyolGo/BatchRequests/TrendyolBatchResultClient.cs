using System.Net;
using System.Text.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.BatchRequests;

public class TrendyolBatchResultClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options)
    : ITrendyolBatchResultClient
{
    public async Task<TrendyolBatchResultOutcome> GetBatchRequestResultAsync(
        string batchRequestId, CancellationToken cancellationToken = default)
    {
        if (!options.Value.IsConfigured)
        {
            return TrendyolBatchResultOutcome.Fail("Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.");
        }

        var pathTemplate = options.Value.BatchResultEndpointPath;

        if (string.IsNullOrWhiteSpace(pathTemplate))
        {
            return TrendyolBatchResultOutcome.Fail(
                "Parti sonucu sorgulama endpoint'i henüz yapılandırılmamış. developers.tgoapps.com " +
                "belgelerini kontrol edip TrendyolGo:BatchResultEndpointPath değerini ayarlayın.");
        }

        var path = pathTemplate.Replace("{batchRequestId}", Uri.EscapeDataString(batchRequestId));

        try
        {
            var response = await httpClient.GetAsync(path, cancellationToken);
            await TrendyolResponseHandler.EnsureSuccessAsync(response, cancellationToken);
            return await ParseAsync(response, cancellationToken);
        }
        catch (TrendyolAuthException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return TrendyolBatchResultOutcome.Fail(TrendyolErrorMessages.CheckCredentials);
        }
        catch (TrendyolAuthException)
        {
            return TrendyolBatchResultOutcome.Fail(TrendyolErrorMessages.CheckHeaders);
        }
        catch (TrendyolRateLimitException)
        {
            return TrendyolBatchResultOutcome.Fail(TrendyolErrorMessages.RateLimited);
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return TrendyolBatchResultOutcome.Fail(TrendyolErrorMessages.ServiceError);
        }
        catch (TrendyolApiException ex)
        {
            return TrendyolBatchResultOutcome.Fail($"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).");
        }
        catch (HttpRequestException)
        {
            return TrendyolBatchResultOutcome.Fail(TrendyolErrorMessages.ConnectionFailed);
        }
    }

    /// <summary>Best-effort parse of an assumed
    /// {status, successCount, failureCount, failureReasons: [...]} shape — TODO: confirm the
    /// real response schema against developers.tgoapps.com. Falls back to an Unknown/zero-count
    /// outcome (still Success=true, since the HTTP call itself succeeded) if the shape doesn't
    /// match, rather than throwing.</summary>
    private static async Task<TrendyolBatchResultOutcome> ParseAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = document.RootElement;

            var status = ParseStatus(root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null);
            var successCount = root.TryGetProperty("successCount", out var scEl) && scEl.TryGetInt32(out var sc) ? sc : 0;
            var failureCount = root.TryGetProperty("failureCount", out var fcEl) && fcEl.TryGetInt32(out var fc) ? fc : 0;
            var failureReasons = root.TryGetProperty("failureReasons", out var frEl) && frEl.ValueKind == JsonValueKind.Array
                ? frEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => s.Length > 0).ToList()
                : [];

            return new TrendyolBatchResultOutcome(true, status, successCount, failureCount, failureReasons, null);
        }
        catch (JsonException)
        {
            return new TrendyolBatchResultOutcome(true, TrendyolBatchResultStatus.Unknown, 0, 0, [], null);
        }
    }

    private static TrendyolBatchResultStatus ParseStatus(string? statusText) => statusText?.ToLowerInvariant() switch
    {
        "completed" or "finished" or "done" => TrendyolBatchResultStatus.Completed,
        "processing" or "inprogress" or "in_progress" or "pending" => TrendyolBatchResultStatus.Processing,
        "failed" or "error" => TrendyolBatchResultStatus.Failed,
        _ => TrendyolBatchResultStatus.Unknown,
    };
}
