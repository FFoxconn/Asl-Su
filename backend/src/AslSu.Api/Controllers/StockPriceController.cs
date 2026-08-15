using AslSu.Application.StockPrice;
using AslSu.Application.StockPrice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stock-price")]
public class StockPriceController(IStockPriceService stockPriceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByProduct([FromQuery] int productId, CancellationToken cancellationToken) =>
        Ok(await stockPriceService.GetByProductAsync(productId, cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Upsert(UpsertStockPriceRequest request, CancellationToken cancellationToken) =>
        Ok(await stockPriceService.UpsertAsync(request, cancellationToken));
}
