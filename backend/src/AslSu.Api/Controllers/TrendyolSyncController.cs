using AslSu.Application.BatchPolling;
using AslSu.Application.OrderSync;
using AslSu.Application.ProductSync;
using AslSu.Application.SaleStatusSync;
using AslSu.Application.StockPriceSync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trendyol-sync")]
public class TrendyolSyncController(
    IProductSyncService productSyncService,
    IStockPriceSyncService stockPriceSyncService,
    ISaleStatusSyncService saleStatusSyncService,
    IBatchPollingService batchPollingService,
    IOrderSyncService orderSyncService) : ControllerBase
{
    [HttpPost("products/push")]
    public async Task<IActionResult> PushProducts(CancellationToken cancellationToken) =>
        Ok(await productSyncService.PushUnsyncedProductsAsync(cancellationToken));

    [HttpPost("stock-price/push")]
    public async Task<IActionResult> PushStockPrice(CancellationToken cancellationToken) =>
        Ok(await stockPriceSyncService.PushChangedInventoryAsync(cancellationToken));

    [HttpPost("sale-status/push")]
    public async Task<IActionResult> PushSaleStatus(CancellationToken cancellationToken) =>
        Ok(await saleStatusSyncService.PushChangedSaleStatusAsync(cancellationToken));

    [HttpPost("batch-requests/poll")]
    public async Task<IActionResult> PollBatchRequests(CancellationToken cancellationToken) =>
        Ok(await batchPollingService.PollPendingBatchesAsync(cancellationToken));

    [HttpGet("batch-requests")]
    public async Task<IActionResult> GetBatchRequests([FromQuery] int take, CancellationToken cancellationToken) =>
        Ok(await productSyncService.GetRecentBatchRequestsAsync(take <= 0 ? 50 : take, cancellationToken));

    [HttpPost("orders/pull")]
    public async Task<IActionResult> PullOrders(CancellationToken cancellationToken) =>
        Ok(await orderSyncService.PullOrdersAsync(cancellationToken));
}
