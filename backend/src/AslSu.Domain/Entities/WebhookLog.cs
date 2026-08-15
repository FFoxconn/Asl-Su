using AslSu.Domain.Enums;

namespace AslSu.Domain.Entities;

public class WebhookLog
{
    public int Id { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string EventType { get; set; } = string.Empty;

    /// <summary>Idempotency/dedupe key for a webhook delivery.</summary>
    public string ExternalEventId { get; set; } = string.Empty;
    public string RawPayloadJson { get; set; } = string.Empty;

    public DateTime? ProcessedAt { get; set; }
    public WebhookProcessingStatus ProcessingStatus { get; set; } = WebhookProcessingStatus.Pending;
    public string? ErrorMessage { get; set; }
}
