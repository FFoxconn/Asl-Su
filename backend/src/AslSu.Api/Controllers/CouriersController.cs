using System.Security.Claims;
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
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await courierService.GetAllAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create(CreateCourierRequest request, CancellationToken cancellationToken) =>
        Ok(await courierService.CreateAsync(request, cancellationToken));

    [HttpGet("{id:int}/stats")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetStats(int id, CancellationToken cancellationToken)
    {
        var stats = await courierService.GetStatsAsync(id, cancellationToken);
        return stats is null ? NotFound() : Ok(stats);
    }

    /// <summary>Courier-only: reports the caller's own current GPS position.</summary>
    [HttpPut("me/location")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> UpdateMyLocation(UpdateCourierLocationRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await courierService.UpdateLocationAsync(userId, request.Latitude, request.Longitude, cancellationToken);
        return updated ? NoContent() : Forbid();
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SetActive(int id, SetCourierActiveRequest request, CancellationToken cancellationToken)
    {
        var courier = await courierService.SetActiveAsync(id, request.IsActive, cancellationToken);
        return courier is null ? NotFound() : Ok(courier);
    }

    /// <summary>Grants login access to a courier that doesn't have it yet, or resets the
    /// password on the one it already has.</summary>
    [HttpPut("{id:int}/login")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SetLogin(int id, SetCourierLoginRequest request, CancellationToken cancellationToken)
    {
        var courier = await courierService.SetLoginAsync(id, request.Email, request.Password, cancellationToken);
        return courier is null ? NotFound() : Ok(courier);
    }
}
