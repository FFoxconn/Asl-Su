using System.Security.Cryptography;
using System.Text;

namespace AslSu.TrendyolGo.Webhook;

/// <summary>Best-effort webhook signature check: HMAC-SHA256 of the raw request body, hex-encoded,
/// compared against the provided signature header value in constant time. This exact algorithm
/// (HMAC-SHA256, hex encoding, raw-body-as-message) was never confirmed against
/// developers.tgoapps.com — it's the conventional shape for webhook signing, not a documented
/// fact about Trendyol Go specifically. Verify it once the real scheme is confirmed.</summary>
public static class TrendyolWebhookSignatureValidator
{
    public static bool IsValid(string rawBody, string? providedSignature, string secret)
    {
        if (string.IsNullOrWhiteSpace(providedSignature))
        {
            return false;
        }

        var computed = Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();

        var computedBytes = Encoding.UTF8.GetBytes(computed);
        var providedBytes = Encoding.UTF8.GetBytes(providedSignature.Trim().ToLowerInvariant());

        return computedBytes.Length == providedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(computedBytes, providedBytes);
    }
}
