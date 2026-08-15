namespace AslSu.Application.Webhooks.Dtos;

public record WebhookProcessResult(
    bool Accepted,
    bool SignatureVerified,
    bool Duplicate,
    string? Message);
