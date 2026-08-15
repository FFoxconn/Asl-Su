using AslSu.Application.Webhooks.Dtos;

namespace AslSu.Application.Webhooks;

public interface IWebhookService
{
    /// <summary>Validates (if configured), dedupes, and records an inbound Trendyol Go
    /// webhook delivery, then best-effort triggers an order pull so affected orders refresh.
    /// Never throws for malformed/unrecognized payloads — everything is stored raw regardless
    /// of how well it parses.</summary>
    Task<WebhookProcessResult> ProcessAsync(
        string rawBody, string? signatureHeaderValue, CancellationToken cancellationToken = default);
}
