using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using AslSu.TrendyolGo.Orders;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Connection;

public class TrendyolConnectionTester(ITrendyolOrderClient orderClient, IOptions<TrendyolGoOptions> options)
    : ITrendyolConnectionTester
{
    public async Task<TrendyolConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.IsConfigured)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.NotConfigured,
                "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.",
                null);
        }

        try
        {
            var response = await orderClient.GetPackagesRawAsync(cancellationToken);
            await TrendyolResponseHandler.EnsureSuccessAsync(response, cancellationToken);

            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.Success, "Trendyol Go API bağlantısı başarılı.", (int)response.StatusCode);
        }
        catch (TrendyolAuthException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.Unauthorized,
                "API Key / API Secret / Supplier ID bilgilerini kontrol edin.",
                (int)ex.StatusCode);
        }
        catch (TrendyolAuthException ex)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.Forbidden,
                "User-Agent veya yetkilendirme bilgilerini kontrol edin.",
                (int)ex.StatusCode);
        }
        catch (TrendyolRateLimitException ex)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.RateLimited, "API rate limitine ulaşıldı.", (int)ex.StatusCode);
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.ServiceError, "Trendyol Go servisinde geçici hata.", (int)ex.StatusCode);
        }
        catch (TrendyolApiException ex)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.Unknown, $"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).", (int)ex.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new TrendyolConnectionTestResult(
                TrendyolConnectionStatus.Unknown, "Trendyol Go'ya bağlanılamadı. BaseUrl bilgisini kontrol edin.", null);
        }
    }
}
