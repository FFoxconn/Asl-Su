using System.Security.Cryptography;
using System.Text;
using AslSu.TrendyolGo.Webhook;
using Xunit;

namespace AslSu.TrendyolGo.Tests;

public class TrendyolWebhookSignatureValidatorTests
{
    private const string Secret = "test-webhook-secret";

    private static string ComputeSignature(string body, string secret) =>
        Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

    [Fact]
    public void IsValid_WithCorrectSignature_ReturnsTrue()
    {
        var body = "{\"eventId\":\"evt-1\"}";
        var signature = ComputeSignature(body, Secret);

        Assert.True(TrendyolWebhookSignatureValidator.IsValid(body, signature, Secret));
    }

    [Fact]
    public void IsValid_IsCaseInsensitiveOnHexSignature()
    {
        var body = "{\"eventId\":\"evt-1\"}";
        var signature = ComputeSignature(body, Secret).ToUpperInvariant();

        Assert.True(TrendyolWebhookSignatureValidator.IsValid(body, signature, Secret));
    }

    [Fact]
    public void IsValid_WithWrongSecret_ReturnsFalse()
    {
        var body = "{\"eventId\":\"evt-1\"}";
        var signature = ComputeSignature(body, "a-different-secret");

        Assert.False(TrendyolWebhookSignatureValidator.IsValid(body, signature, Secret));
    }

    [Fact]
    public void IsValid_WithTamperedBody_ReturnsFalse()
    {
        var signature = ComputeSignature("{\"eventId\":\"evt-1\"}", Secret);

        Assert.False(TrendyolWebhookSignatureValidator.IsValid("{\"eventId\":\"evt-2\"}", signature, Secret));
    }

    [Fact]
    public void IsValid_WithMissingSignature_ReturnsFalse()
    {
        Assert.False(TrendyolWebhookSignatureValidator.IsValid("{}", null, Secret));
        Assert.False(TrendyolWebhookSignatureValidator.IsValid("{}", "", Secret));
    }
}
