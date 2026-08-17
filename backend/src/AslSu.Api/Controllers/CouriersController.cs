using AslSu.Application.Couriers;
using AslSu.Application.Couriers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/couriers")]
public class CouriersController(ICourierService courierService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await courierService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourierRequest request, CancellationToken cancellationToken) =>
        Ok(await courierService.CreateAsync(request, cancellationToken));

    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> GetStats(int id, CancellationToken cancellationToken)
    {
        var stats = await courierService.GetStatsAsync(id, cancellationToken);
        return stats is null ? NotFound() : Ok(stats);
    }
}