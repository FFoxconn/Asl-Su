using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AslSu.Application.OrderSync;
using AslSu.Application.Webhooks;
using AslSu.Application.Webhooks.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AslSu.Infrastructure.Webhooks;

public class WebhookService(
    AslSuDbContext dbContext,
    IOptions<TrendyolGoOptions> options,
    IOrderSyncService orderSyncService,
    ILogger<WebhookService> logger) : IWebhookService
{
    public async Task<WebhookProcessResult> ProcessAsync(
        string rawBody, string? signatureHeaderValue, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        var signatureVerified = false;

        if (opts.IsWebhookSignatureConfigured)
        {
            if (!TrendyolWebhookSignatureValidator.IsValid(rawBody, signatureHeaderValue, opts.WebhookSecret))
            {
                return new WebhookProcessResult(false, false, false, "İmza doğrulanamadı.");
            }

            signatureVerified = true;
        }
        else
        {
            logger.LogWarning(
                "Trendyol Go webhook signature scheme (TrendyolGo:WebhookSecret / TrendyolGo:WebhookSignatureHeaderName) " +
                "is not configured — accepting this delivery unverified.");
        }

        var (eventType, externalEventId) = ParsePayload(rawBody);

        var alreadyProcessed = await dbContext.WebhookLogs
            .AnyAsync(w => w.ExternalEventId == externalEventId, cancellationToken);
        if (alreadyProcessed)
        {
            return new WebhookProcessResult(true, signatureVerified, true, "Bu olay zaten işlendi.");
        }

        var log = new WebhookLog
        {
            ReceivedAt = DateTime.UtcNow,
            EventType = eventType,
            ExternalEventId = externalEventId,
            RawPayloadJson = rawBody,
            ProcessingStatus = WebhookProcessingStatus.Pending,
        };
        dbContext.WebhookLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var summary = await orderSyncService.PullOrdersAsync(cancellationToken);
            log.ProcessingStatus = WebhookProcessingStatus.Processed;
            log.ProcessedAt = DateTime.UtcNow;
            logger.LogInformation("Webhook-triggered order pull for {ExternalEventId}: {Message}", externalEventId, summary.Message);
        }
        catch (Exception ex)
        {
            log.ProcessingStatus = WebhookProcessingStatus.Failed;
            log.ProcessedAt = DateTime.UtcNow;
            log.ErrorMessage = ex.Message;
            logger.LogError(ex, "Failed to process Trendyol Go webhook {ExternalEventId}", externalEventId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new WebhookProcessResult(true, signatureVerified, false, "Kaydedildi.");
    }

    /// <summary>Best-effort extraction of an event type and a dedupe key from commonly-used
    /// webhook field names — the real Trendyol Go webhook payload schema was never given in
    /// the integration brief. Falls back to a SHA-256 hash of the raw body as the dedupe key
    /// when no recognizable id field is present, so redeliveries of an identical payload still
    /// dedupe correctly even against an unknown schema.</summary>
    private static (string EventType, string ExternalEventId) ParsePayload(string rawBody)
    {
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            var eventType = GetString(root, "eventType", "type", "event") ?? "Unknown";
            var externalEventId = GetString(root, "eventId", "id", "webhookId", "packageId", "orderNumber");

            return (eventType, string.IsNullOrWhiteSpace(externalEventId) ? ComputeFallbackId(rawBody) : externalEventId);
        }
        catch (JsonException)
        {
            return ("Unknown", ComputeFallbackId(rawBody));
        }
    }

    private static string? GetString(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (el.TryGetProperty(key, out var value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    _ => null,
                };
            }
        }

        return null;
    }

    private static string ComputeFallbackId(string rawBody) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();
}
