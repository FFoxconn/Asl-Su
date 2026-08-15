using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.PackageStatus;

public class TrendyolPackageStatusClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options)
    : ITrendyolPackageStatusClient
{
    public Task<TrendyolPackageActionOutcome> AcceptPackageAsync(
        string packageId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            options.Value.AcceptOrderEndpointPath, "TrendyolGo:AcceptOrderEndpointPath",
            "Sipariş kabul endpoint'i", packageId, cancellationToken);

    public Task<TrendyolPackageActionOutcome> InvoicePackageAsync(
        string packageId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            options.Value.InvoiceOrderEndpointPath, "TrendyolGo:InvoiceOrderEndpointPath",
            "Faturalandırma endpoint'i", packageId, cancellationToken);

    public Task<TrendyolPackageActionOutcome> ShipPackageAsync(
        string packageId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            options.Value.ShipOrderEndpointPath, "TrendyolGo:ShipOrderEndpointPath",
            "Kargolama endpoint'i", packageId, cancellationToken);

    private async Task<TrendyolPackageActionOutcome> ExecuteAsync(
        string pathTemplate, string configKey, string endpointDisplayName,
        string packageId, CancellationToken cancellationToken)
    {
        if (!options.Value.IsConfigured)
        {
            return TrendyolPackageActionOutcome.Fail("Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.");
        }

        if (string.IsNullOrWhiteSpace(pathTemplate))
        {
            return TrendyolPackageActionOutcome.Fail(
                $"{endpointDisplayName} henüz yapılandırılmamış. developers.tgoapps.com belgelerini " +
                $"kontrol edip {configKey} değerini ayarlayın.");
        }

        var path = pathTemplate.Replace("{packageId}", Uri.EscapeDataString(packageId));

        try
        {
            var response = await httpClient.PostAsync(path, content: null, cancellationToken);
            await TrendyolResponseHandler.EnsureSuccessAsync(response, cancellationToken);
            return TrendyolPackageActionOutcome.Ok;
        }
        catch (TrendyolAuthException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return TrendyolPackageActionOutcome.Fail(TrendyolErrorMessages.CheckCredentials);
        }
        catch (TrendyolAuthException)
        {
            return TrendyolPackageActionOutcome.Fail(TrendyolErrorMessages.CheckHeaders);
        }
        catch (TrendyolRateLimitException)
        {
            return TrendyolPackageActionOutcome.Fail(TrendyolErrorMessages.RateLimited);
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return TrendyolPackageActionOutcome.Fail(TrendyolErrorMessages.ServiceError);
        }
        catch (TrendyolApiException ex)
        {
            return TrendyolPackageActionOutcome.Fail($"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).");
        }
        catch (HttpRequestException)
        {
            return TrendyolPackageActionOutcome.Fail(TrendyolErrorMessages.ConnectionFailed);
        }
    }
}
