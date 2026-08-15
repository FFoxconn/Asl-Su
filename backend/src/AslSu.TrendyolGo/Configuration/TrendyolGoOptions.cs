namespace AslSu.TrendyolGo.Configuration;

/// <summary>
/// Trendyol Go Market credentials and connection settings. Populated only from
/// dotnet user-secrets (dev) or environment variables (other environments) —
/// never from appsettings.json, never hardcoded.
///
/// BaseUrl, AgentName and ExecutorUser are left with no default on purpose: the
/// exact base domain and the required values for the x-agentname/x-executor-user
/// headers must be taken from the Trendyol Go partner panel / developers.tgoapps.com
/// documentation, not guessed here.
/// </summary>
public class TrendyolGoOptions
{
    public const string SectionName = "TrendyolGo";

    public string SupplierId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>Trendyol Go API base URL — verify against developers.tgoapps.com.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Value for the required x-agentname header — verify against developers.tgoapps.com.</summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>Value for the required x-executor-user header — verify against developers.tgoapps.com.</summary>
    public string ExecutorUser { get; set; } = string.Empty;

    /// <summary>Path of the createProducts (bulk product creation) endpoint. Left unset by
    /// design: the brief that shaped this integration gave the price-and-inventory and
    /// packages-GET endpoints verbatim, but never the exact createProducts path or payload
    /// shape. Rather than guess a plausible-looking URL, product push stays disabled — and
    /// reports itself as such — until this is set from developers.tgoapps.com.</summary>
    public string ProductsEndpointPath { get; set; } = string.Empty;

    /// <summary>Path template of the getBatchRequestResult endpoint, with a
    /// <c>{batchRequestId}</c> placeholder — e.g. "/integrator/.../batch-requests/{batchRequestId}".
    /// Never given in the integration brief, same reasoning as ProductsEndpointPath: left
    /// unset rather than guessed, until confirmed against developers.tgoapps.com.</summary>
    public string BatchResultEndpointPath { get; set; } = string.Empty;

    /// <summary>Path of the sell/unsell (sale status) endpoint. Never given in the
    /// integration brief, same reasoning as ProductsEndpointPath: left unset rather than
    /// guessed, until confirmed against developers.tgoapps.com. The set of valid unsell
    /// reason codes is likewise unconfirmed — not enforced as a closed set in this codebase.</summary>
    public string SellUnsellEndpointPath { get; set; } = string.Empty;

    /// <summary>Path template of the "accept package" endpoint, with a
    /// <c>{packageId}</c> placeholder. Never given in the integration brief, same reasoning
    /// as ProductsEndpointPath: left unset rather than guessed, until confirmed against
    /// developers.tgoapps.com.</summary>
    public string AcceptOrderEndpointPath { get; set; } = string.Empty;

    /// <summary>Path template of the "mark package invoiced" endpoint, with a
    /// <c>{packageId}</c> placeholder. Same reasoning as AcceptOrderEndpointPath.</summary>
    public string InvoiceOrderEndpointPath { get; set; } = string.Empty;

    /// <summary>Path template of the "mark package shipped" endpoint, with a
    /// <c>{packageId}</c> placeholder. Same reasoning as AcceptOrderEndpointPath.</summary>
    public string ShipOrderEndpointPath { get; set; } = string.Empty;

    /// <summary>Shared secret Trendyol Go signs webhook deliveries with. Never given in the
    /// integration brief — no webhook signature scheme (algorithm, header name, or secret
    /// source) was documented. Left unset by design: until confirmed against
    /// developers.tgoapps.com, the webhook endpoint accepts deliveries unverified rather than
    /// silently rejecting real ones or trusting a guessed scheme. Set together with
    /// <see cref="WebhookSignatureHeaderName"/>.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>HTTP header Trendyol Go sends the webhook signature in (e.g. an HMAC of the
    /// request body) — name unconfirmed, left unset for the same reason as
    /// <see cref="WebhookSecret"/>.</summary>
    public string WebhookSignatureHeaderName { get; set; } = string.Empty;

    public bool IsWebhookSignatureConfigured =>
        !string.IsNullOrWhiteSpace(WebhookSecret) && !string.IsNullOrWhiteSpace(WebhookSignatureHeaderName);

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SupplierId) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret) &&
        !string.IsNullOrWhiteSpace(BaseUrl);
}
