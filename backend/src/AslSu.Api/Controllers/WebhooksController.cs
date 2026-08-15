using AslSu.Application.Webhooks;
using AslSu.TrendyolGo.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AslSu.Api.Controllers;

/// <summary>Receives inbound Trendyol Go webhook deliveries. Deliberately excluded from the
/// JWT auth pipeline (Trendyol Go doesn't have our tokens) — its own signature check, when
/// configured, is the access control here instead.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/webhooks")]
public class WebhooksController(IWebhookService webhookService, IOptions<TrendyolGoOptions> options) : ControllerBase
{
    [HttpPost("trendyol-go")]
    public async Task<IActionResult> ReceiveTrendyolGo(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var headerName = options.Value.WebhookSignatureHeaderName;
        string? signature = !string.IsNullOrWhiteSpace(headerName) && Request.Headers.TryGetValue(headerName, out var values)
            ? values.ToString()
            : null;

        var result = await webhookService.ProcessAsync(rawBody, signature, cancellationToken);
        return result.Accepted ? Ok(result) : Unauthorized(result.Message);
    }
}
