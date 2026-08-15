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
}
