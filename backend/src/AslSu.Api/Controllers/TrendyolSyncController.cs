using AslSu.Application.ProductSync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trendyol-sync")]
public class TrendyolSyncController(IProductSyncService productSyncService) : ControllerBase
{
    [HttpPost("products/push")]
    public async Task<IActionResult> PushProducts(CancellationToken cancellationToken) =>
        Ok(await productSyncService.PushUnsyncedProductsAsync(cancellationToken));

    [HttpGet("batch-requests")]
    public async Task<IActionResult> GetBatchRequests([FromQuery] int take, CancellationToken cancellationToken) =>
        Ok(await productSyncService.GetRecentBatchRequestsAsync(take <= 0 ? 50 : take, cancellationToken));
}
