namespace AslSu.TrendyolGo;

/// <summary>User-facing Turkish messages for the Trendyol Go error categories, kept in one
/// place so every client (connection test, product/inventory batch submit, batch result
/// polling) reports the same wording for the same underlying HTTP status.</summary>
internal static class TrendyolErrorMessages
{
    public const string CheckCredentials = "API Key / API Secret / Supplier ID bilgilerini kontrol edin.";
    public const string CheckHeaders = "User-Agent veya yetkilendirme bilgilerini kontrol edin.";
    public const string RateLimited = "API rate limitine ulaşıldı.";
    public const string ServiceError = "Trendyol Go servisinde geçici hata.";
    public const string ConnectionFailed = "Trendyol Go'ya bağlanılamadı. BaseUrl bilgisini kontrol edin.";
}
